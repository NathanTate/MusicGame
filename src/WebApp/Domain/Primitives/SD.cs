using System.Collections.Immutable;

namespace Domain.Primitives;
public static class SD
{
    public static ImmutableList<string> AllowedSongTypes = ["audio/mpeg", "audio/wav"];
    public const int MaxSongSize = 10 * 1024 * 1024;
    public static ImmutableList<string> AllowedPhotoExtensions = [".png", ".webp", ".jpg"];
    public const int MaxPhotoSize = 1024 * 512;
}
