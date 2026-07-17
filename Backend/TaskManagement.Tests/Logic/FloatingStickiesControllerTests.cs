using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Controllers;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Tests.Logic
{
    public class FloatingStickiesControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _otherUserId = Guid.NewGuid();

        public FloatingStickiesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetFloatingStickies_ReturnsOnlyCurrentUsersNotes()
        {
            var ownNote = CreateNote(_userId, isFloating: true);
            var otherUsersNote = CreateNote(_otherUserId, isFloating: true);
            _context.StickyNotes.AddRange(ownNote, otherUsersNote);
            await _context.SaveChangesAsync();

            var result = await CreateController(_userId).GetFloatingStickies();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var json = JsonSerializer.Serialize(ok.Value);
            json.Should().Contain(ownNote.Id.ToString());
            json.Should().NotContain(otherUsersNote.Id.ToString());
        }

        [Fact]
        public async Task SetFloatingState_OtherUsersNote_Returns404AndDoesNotMutate()
        {
            var otherUsersNote = CreateNote(_otherUserId, isFloating: false);
            _context.StickyNotes.Add(otherUsersNote);
            await _context.SaveChangesAsync();

            var result = await CreateController(_userId).SetFloatingState(
                otherUsersNote.Id,
                new StickyFloatingStateDto { IsFloating = true, PositionX = 320, PositionY = 180 });

            result.Should().BeOfType<NotFoundObjectResult>();
            (await _context.StickyNotes.SingleAsync(note => note.Id == otherUsersNote.Id))
                .IsFloating.Should().BeFalse();
        }

        [Fact]
        public async Task SetFloatingState_SixthNote_Returns409AndDoesNotMutate()
        {
            for (var index = 0; index < 5; index++)
            {
                _context.StickyNotes.Add(CreateNote(_userId, isFloating: true));
            }

            var sixthNote = CreateNote(_userId, isFloating: false);
            _context.StickyNotes.Add(sixthNote);
            await _context.SaveChangesAsync();

            var result = await CreateController(_userId).SetFloatingState(
                sixthNote.Id,
                new StickyFloatingStateDto { IsFloating = true, PositionX = 640, PositionY = 240 });

            result.Should().BeOfType<ConflictObjectResult>();
            (await _context.StickyNotes.CountAsync(note => note.UserId == _userId && note.IsFloating))
                .Should().Be(5);
            (await _context.StickyNotes.SingleAsync(note => note.Id == sixthNote.Id))
                .IsFloating.Should().BeFalse();
        }

        private StickiesController CreateController(Guid userId)
        {
            return new StickiesController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                        }, "TestAuth"))
                    }
                }
            };
        }

        private static StickyNote CreateNote(Guid userId, bool isFloating)
        {
            return new StickyNote
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Floating test note",
                Content = "Test content",
                Color = "#EAB308",
                IsFloating = isFloating,
                PositionX = isFloating ? 300 : null,
                PositionY = isFloating ? 160 : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
