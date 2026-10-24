namespace LMS.Services;

// The same file rules as in DocumentService (US06), kept in one place.
public static class FileValidator
{
    private static readonly string[] AllowedExtensions = [".pdf", ".docx"];
    private const long MaxFileSize = 10 * 1024 * 1024;

    public static void Validate(string fileName, long fileSize)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException(
                $"Filtypen {extension} är inte tillåten. Tillåtna: {string.Join(", ", AllowedExtensions)}.");

        if (fileSize > MaxFileSize)
            throw new ArgumentException(
                $"Filen är för stor. Maxstorlek är {MaxFileSize / (1024 * 1024)} MB.");

        if (fileSize == 0)
            throw new ArgumentException("Filen är tom.");
    }
}
