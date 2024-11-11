using Domain.Primitives;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Helpers;
public static class PhotoFileValidator
{
    public static string? Validate(IFormFile photo)
    {
        if (photo is null || photo.Length == 0)
        {
            return "File size should be more than 0 bytes";
        }

        if (photo.Length > SD.MaxPhotoSize)
        {
            return $"File size is: {photo.Length / 1024 / 1024}mb, but the max allowed is {SD.MaxPhotoSize / 1024 / 1024}mb";
        }

        var fileExtension = Path.GetExtension(photo.FileName);

        if (!SD.AllowedPhotoExtensions.Contains(fileExtension))
        {
            return $"Allowed extenions {string.Join(", ", SD.AllowedPhotoExtensions)}, your was {fileExtension}";
        }

        return null;
    }
}
