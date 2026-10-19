using LMS.Shared.DTOs.DocumentDtos;

namespace Service.Contracts;

public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        int? courseId,
        int? moduleId,
        int? activityId,
        string uploadedByUserId);

    Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(int id, string userId);

    Task<IEnumerable<DocumentDto>> GetByCourseAsync(int courseId, string userId);
}