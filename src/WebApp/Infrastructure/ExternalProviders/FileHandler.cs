using Application.InfrastructureInterfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.ExternalProviders;

internal class FileHandler : IFileHandler
{
    private readonly BlobServiceClient _blobServiceClient;
    public FileHandler(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }
    public async Task<string> UploadFileAsync(IFormFile file, FileContainer container, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = GetUniqueFileName(file.FileName);
        var blobContainer = await GetBlobContainerClientAsync(container, cancellationToken);
        var blob = blobContainer.GetBlobClient(uniqueFileName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = file.ContentType
        };

        var blobUploadOptions = new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        };

        await using (Stream data = file.OpenReadStream())
        {
            await blob.UploadAsync(data, blobUploadOptions, cancellationToken);
        }

        var fileUrl = blob.Uri.AbsoluteUri;

        return fileUrl;
    }

    public async Task<GetFileResponse?> GetFileAsync(string fileName, FileContainer container, CancellationToken cancellationToken = default)
    {
        var blobContainer = await GetBlobContainerClientAsync(container, cancellationToken);
        var blob = blobContainer.GetBlobClient(fileName);

        if (await blob.ExistsAsync(cancellationToken))
        {
            Stream data = await blob.OpenReadAsync(cancellationToken: cancellationToken);

            var blobProperties = await blob.GetPropertiesAsync();
            string contentType = blobProperties.Value.ContentType;
            long contentLenght = blobProperties.Value.ContentLength;

            var response = new GetFileResponse(data, contentType, contentLenght);
            return response;
        }

        return null;
    }

    public async Task<string> UpdateFileAsync(string fileName, IFormFile file, FileContainer container, CancellationToken cancellationToken = default)
    {
        await DeleteFileAsync(fileName, container, cancellationToken);
        return await UploadFileAsync(file, container, cancellationToken);
    }

    public async Task DeleteFileAsync(string fileName, FileContainer container, CancellationToken cancellationToken = default)
    {
        var blobContainer = await GetBlobContainerClientAsync(container, cancellationToken);
        var blob = blobContainer.GetBlobClient(fileName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private async Task<BlobContainerClient> GetBlobContainerClientAsync(FileContainer container, CancellationToken cancellationToken = default)
    {
        string? containerName = null;
        if (container == FileContainer.Photos)
        {
            containerName = "photos";
        }
        else if (container == FileContainer.Songs)
        {
            containerName = "songs";
        }

        BlobContainerClient blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);

        if (!await blobContainer.ExistsAsync(cancellationToken))
        {
            await blobContainer.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            await blobContainer.SetAccessPolicyAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        }

        return blobContainer;
    }

    private string GetUniqueFileName(string fileName)
    {
        return $"{Guid.NewGuid().ToString().AsSpan(0, 4)}_{fileName}";
    }
}
