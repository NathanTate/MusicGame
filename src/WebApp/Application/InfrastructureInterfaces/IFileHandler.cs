using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.InfrastructureInterfaces;
public interface IFileHandler
{
    public Task<string> UploadFileAsync(IFormFile file, FileContainer container, CancellationToken cancellationToken = default);
    public Task<GetFileResponse?> GetFileAsync(string fileName, FileContainer container, CancellationToken cancellationToken = default);
    //public Task<string> UpdateFileAsync(string fileName, IFormFile file, CancellationToken cancellationToken = default);
    public Task DeleteFileAsync(string fileName, FileContainer container, CancellationToken cancellationToken = default);
}

public record GetFileResponse(Stream stream, string ContentType, long Size);
