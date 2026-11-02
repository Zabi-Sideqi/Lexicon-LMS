using System.IO.Compression;

namespace LMS.Services;

public static class FileValidator
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];

    public static async Task ValidateAsync(Stream fileStream, string fileName, long fileSize)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not ".pdf" and not ".docx")
            throw new ArgumentException(
                $"Filtypen {extension} är inte tillåten. Tillåtna: .pdf, .docx.");

        if (fileSize > MaxFileSize)
            throw new ArgumentException("Filen är för stor. Maxstorlek är 10 MB.");

        if (fileSize <= 0)
            throw new ArgumentException("Filen är tom.");

        if (!fileStream.CanSeek)
            throw new ArgumentException("Uppladdningsströmmen måste kunna läsas om.");

        var originalPosition = fileStream.Position;
        try
        {
            if (fileStream.Length - originalPosition != fileSize)
                throw new ArgumentException("Filens innehåll motsvarar inte angiven filstorlek.");

            fileStream.Position = 0;
            var signature = new byte[5];
            var bytesRead = await fileStream.ReadAsync(signature.AsMemory(0, signature.Length));
            fileStream.Position = 0;

            if (extension == ".pdf")
            {
                if (bytesRead < PdfSignature.Length ||
                    !signature.Take(PdfSignature.Length).SequenceEqual(PdfSignature))
                {
                    throw new ArgumentException("Filen har inte ett giltigt PDF-signaturinnehåll.");
                }

                return;
            }

            if (bytesRead < 4 ||
                !signature.Take(4).SequenceEqual(ZipSignature))
                throw new ArgumentException("Filen är inte ett giltigt DOCX-paket.");

            try
            {
                using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: true);
                var hasContentTypes = archive.GetEntry("[Content_Types].xml") is not null;
                var hasWordDocument = archive.GetEntry("word/document.xml") is not null;

                if (!hasContentTypes || !hasWordDocument)
                    throw new ArgumentException("Filen är inte ett giltigt DOCX-dokument.");
            }
            catch (InvalidDataException)
            {
                throw new ArgumentException("Filen är inte ett giltigt DOCX-paket.");
            }
        }
        finally
        {
            fileStream.Position = originalPosition;
        }
    }

    public static string GetVerifiedContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => throw new ArgumentException("Filtypen är inte tillåten.")
        };
}