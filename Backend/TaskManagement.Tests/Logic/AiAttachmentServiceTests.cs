using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Moq;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.Tests.Logic
{
    public sealed class AiAttachmentServiceTests : IDisposable
    {
        private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), $"sprinta-ai-attachments-{Guid.NewGuid():N}");
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _workspaceId = Guid.NewGuid();
        private readonly Guid _conversationId = Guid.NewGuid();
        private readonly ApplicationDbContext _context;
        private readonly Mock<IAiService> _aiService = new();
        private readonly AiAttachmentService _service;

        public AiAttachmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.AiConversations.Add(new AiConversation
            {
                Id = _conversationId,
                UserId = _userId,
                WorkspaceId = _workspaceId,
                Title = "Attachment test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();

            var environment = new Mock<IHostEnvironment>();
            environment.SetupGet(item => item.ContentRootPath).Returns(_storageRoot);
            _service = new AiAttachmentService(_context, _aiService.Object, environment.Object);
        }

        [Fact]
        public async Task UploadAndChat_LongText_UsesBoundedChunksAndReturnsCitations()
        {
            var text = string.Join('\n', Enumerable.Range(1, 700).Select(index =>
                $"Dòng {index}: SprintA lưu tài liệu riêng tư và truy xuất đúng đoạn liên quan đến citation."));
            var bytes = Encoding.UTF8.GetBytes(text);
            IReadOnlyList<AiAttachmentRagSourceDto>? capturedSources = null;
            _aiService
                .Setup(item => item.ChatWithAttachmentsAsync(
                    _userId,
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<AiAttachmentRagSourceDto>>(),
                    It.IsAny<IReadOnlyList<AiAttachmentImageInputDto>>()))
                .Callback<Guid, string, IReadOnlyList<AiAttachmentRagSourceDto>, IReadOnlyList<AiAttachmentImageInputDto>>(
                    (_, _, sources, _) => capturedSources = sources)
                .ReturnsAsync("Câu trả lời có nguồn [S1].");

            await using var stream = new MemoryStream(bytes);
            var attachment = await _service.UploadAsync(
                _userId, _workspaceId, _conversationId, "roadmap.txt", "text/plain", stream, bytes.LongLength);
            var response = await _service.ChatAsync(
                _userId, _workspaceId, _conversationId, [attachment.Id], "citation riêng tư");

            attachment.Status.Should().Be("Ready");
            attachment.ChunkCount.Should().BeGreaterThan(1);
            capturedSources.Should().NotBeNull();
            capturedSources!.Count.Should().BeLessThanOrEqualTo(8);
            capturedSources.Sum(item => item.Content.Length).Should().BeLessThanOrEqualTo(12_000);
            capturedSources.Sum(item => item.Content.Length).Should().BeLessThan(text.Length);
            response.Citations.Should().NotBeEmpty();
            response.Citations.Should().OnlyContain(item => item.SourceId.StartsWith('S'));
        }

        [Fact]
        public async Task StoredAttachment_DifferentUser_CannotReadOrUseIt()
        {
            var bytes = Encoding.UTF8.GetBytes("Thông tin chỉ thuộc user hiện tại.");
            await using var stream = new MemoryStream(bytes);
            var attachment = await _service.UploadAsync(
                _userId, _workspaceId, _conversationId, "private.txt", "text/plain", stream, bytes.LongLength);
            var otherUserId = Guid.NewGuid();

            (await _service.GetAsync(otherUserId, attachment.Id)).Should().BeNull();
            (await _service.GetContentAsync(otherUserId, attachment.Id)).Should().BeNull();
            await FluentActions.Invoking(() => _service.ChatAsync(
                    otherUserId, _workspaceId, _conversationId, [attachment.Id], "đọc file"))
                .Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Upload_FakeImageSignatureAndOversize_AreRejected()
        {
            var fakePng = Encoding.UTF8.GetBytes("not a png");
            await using var fakeStream = new MemoryStream(fakePng);
            await FluentActions.Invoking(() => _service.UploadAsync(
                    _userId, _workspaceId, _conversationId, "fake.png", "image/png", fakeStream, fakePng.LongLength))
                .Should().ThrowAsync<InvalidDataException>();

            await using var oversizeStream = new MemoryStream([1]);
            await FluentActions.Invoking(() => _service.UploadAsync(
                    _userId, _workspaceId, _conversationId, "large.webp", "image/webp", oversizeStream, 5 * 1024 * 1024 + 1))
                .Should().ThrowAsync<InvalidDataException>();
        }

        public void Dispose()
        {
            _context.Dispose();
            if (Directory.Exists(_storageRoot)) Directory.Delete(_storageRoot, recursive: true);
        }
    }
}
