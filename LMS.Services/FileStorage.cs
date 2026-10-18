using Service.Contracts;

namespace LMS.Services;

public class FileStorage : IFileStorage
{
    private readonly string _basePath;

    public FileStorage(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string storedFileName)
    {
        var path = Path.Combine(_basePath, storedFileName);

        using var output = new FileStream(path, FileMode.Create);
        await fileStream.CopyToAsync(output);

        return path;
    }

    public Task<Stream> OpenReadAsync(string storedFileName)
    {
        var path = Path.Combine(_basePath, storedFileName);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Filen {storedFileName} hittades inte.");

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storedFileName)
    {
        var path = Path.Combine(_basePath, storedFileName);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}