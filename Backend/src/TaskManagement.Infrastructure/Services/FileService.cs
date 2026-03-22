using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadFolder;

        public FileService(IWebHostEnvironment env)
        {
            // Store files in a non-public folder as per requirement
            _uploadFolder = Path.Combine(env.ContentRootPath, "SecureUploads");
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            if (fileStream == null || fileStream.Length == 0) return string.Empty;

            var fileExtension = Path.GetExtension(fileName);
            var safeFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(_uploadFolder, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            return safeFileName; // Return the Guid-based name
        }

        public async Task<(byte[] Bytes, string ContentType, string FileName)?> DownloadFileAsync(string fileName)
        {
            var filePath = Path.Combine(_uploadFolder, fileName);
            if (!File.Exists(filePath)) return null;

            var bytes = await File.ReadAllBytesAsync(filePath);
            var contentType = "application/octet-stream"; // Simple implementation
            return (bytes, contentType, fileName);
        }

        public bool DeleteFile(string fileName)
        {
            var filePath = Path.Combine(_uploadFolder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
    }
}
