
namespace Domain.Models.Exceptions;

public class ResourceNotFoundException : NotFoundException
{
    public ResourceNotFoundException(
        string message,
        string title = "Not Found")
        : base(message, title)
    {
    }
}
