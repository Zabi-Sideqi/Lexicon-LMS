using System.IO.Compression;
using LMS.Services;

namespace LMS.tests;

public class FileValidatorTests
{
    [Fact]
    public async Task ValidateAsync_AcceptsPdfWithPdfSignature()
    {
        var bytes = "%PDF-1.7\nvalid test"u8.ToArray();
        await using var stream = new MemoryStream(bytes);

        await FileValidator.ValidateAsync(stream, "sample.pdf", bytes.Length);

        Assert.Equal(0, stream.Position);
        Assert.Equal("application/pdf", FileValidator.GetVerifiedContentType("sample.pdf"));
    }

    [Fact]
    public async Task ValidateAsync_RejectsTextFileRenamedAsPdf()
    {
        var bytes = "This is not a PDF"u8.ToArray();
        await using var stream = new MemoryStream(bytes);

        await Assert.ThrowsAsync<ArgumentException>(
            () => FileValidator.ValidateAsync(stream, "fake.pdf", bytes.Length));
    }

    [Fact]
    public async Task ValidateAsync_AcceptsDocxPackage()
    {
        await using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var contentTypes = archive.CreateEntry("[Content_Types].xml");
            await using (var writer = new StreamWriter(contentTypes.Open()))
                await writer.WriteAsync("<Types />");

            var document = archive.CreateEntry("word/document.xml");
            await using (var writer = new StreamWriter(document.Open()))
                await writer.WriteAsync("<document />");
        }

        stream.Position = 0;
        await FileValidator.ValidateAsync(stream, "sample.docx", stream.Length);

        Assert.Equal(0, stream.Position);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileValidator.GetVerifiedContentType("sample.docx"));
    }

    [Fact]
    public async Task ValidateAsync_RejectsZipWithoutWordDocument()
    {
        await using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("[Content_Types].xml");
            await using var writer = new StreamWriter(entry.Open());
            await writer.WriteAsync("<Types />");
        }

        stream.Position = 0;
        await Assert.ThrowsAsync<ArgumentException>(
            () => FileValidator.ValidateAsync(stream, "not-a-docx.docx", stream.Length));
    }
}