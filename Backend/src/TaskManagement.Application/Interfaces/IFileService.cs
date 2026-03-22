using System;
using System.IO;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task<(byte[] Bytes, string ContentType, string FileName)?> DownloadFileAsync(string fileUrl);
        bool DeleteFile(string fileUrl);
    }
}
