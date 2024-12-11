using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Domain.Interfaces;
public interface IFileHandler
{
    Task<string> UploadFileAsync(IFormFile file, FileContainer container, CancellationToken cancellationToken = default);
    Task<GetFileResponse?> GetFileAsync(string fileName, FileContainer container, CancellationToken cancellationToken = default);
    Task<string> UpdateFileAsync(string fileName, IFormFile file, FileContainer container, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileName, FileContainer container, CancellationToken cancellationToken = default);
}

public record GetFileResponse(Stream stream, string ContentType, long Size);
