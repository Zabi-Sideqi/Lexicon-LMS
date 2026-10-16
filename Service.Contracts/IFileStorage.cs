namespace Service.Contracts;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream fileStream, string storedFileName);
    Task<Stream> OpenReadAsync(string storedFileName);
    Task DeleteAsync(string storedFileName);
}