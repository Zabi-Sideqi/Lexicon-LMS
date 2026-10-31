using System.IO.Compression;

namespace LMS.Services;

// Shared server-side validation for documents and assignment submissions.
public static class FileValidator
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];

    public static async Task ValidateAsync(
        Stream fileStream,
        string fileName,
        long fileSize)
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

        // Buffer non-seekable streams so signature and package checks do not consume
        // the stream that will subsequently be saved.
        Stream inspectionStream = fileStream;
        MemoryStream? buffer = null;
        var originalPosition = fileStream.CanSeek ? fileStream.Position : 0;

        try
        {
            if (!fileStream.CanSeek)
            {
                buffer = new MemoryStream();
                await fileStream.CopyToAsync(buffer);
                if (buffer.Length != fileSize)
                    throw new ArgumentException("Filens innehåll motsvarar inte angiven filstorlek.");
                buffer.Position = 0;
                inspectionStream = buffer;
            }

            inspectionStream.Position = 0;
            var signature = new byte[5];
            var bytesRead = await inspectionStream.ReadAsync(signature.AsMemory(0, 5));
            inspectionStream.Position = 0;

            if (extension == ".pdf")
            {
                if (bytesRead < PdfSignature.Length ||
                    !signature.AsSpan(0, PdfSignature.Length).SequenceEqual(PdfSignature))
                {
                    throw new ArgumentException("Filen har inte ett giltigt PDF-signaturinnehåll.");
                }
            }
            else
            {
                var zipSignature = signature.AsSpan(0, Math.Min(bytesRead, 4));
                if (bytesRead < 4 || !zipSignature.SequenceEqual(ZipSignature))
                    throw new ArgumentException("Filen är inte ett giltigt DOCX-paket.");

                try
                {
                    using var archive = new ZipArchive(
                        inspectionStream, ZipArchiveMode.Read, leaveOpen: true);

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
        }
        finally
        {
            if (buffer is not null)
                await buffer.DisposeAsync();
            else if (fileStream.CanSeek)
                fileStream.Position = originalPosition;
        }
    }
}