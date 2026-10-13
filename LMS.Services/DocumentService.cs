using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.Services;

public class DocumentService : IDocumentService
{
    private static readonly string[] AllowedExtensions = [".pdf", ".docx"];
    private const long MaxFileSize = 10 * 1024 * 1024;

    private readonly IUnitOfWork _uow;
    private readonly IFileStorage _fileStorage;
    private readonly UserManager<ApplicationUser> _userManager;

    public DocumentService(
        IUnitOfWork uow,
        IFileStorage fileStorage,
        UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _fileStorage = fileStorage;
        _userManager = userManager;
    }

    public async Task<DocumentDto> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        int? courseId,
        int? moduleId,
        string uploadedByUserId)
    {
        ValidateFile(fileName, fileSize);

        if ((courseId.HasValue ? 1 : 0) + (moduleId.HasValue ? 1 : 0) != 1)
            throw new ArgumentException(
                "Exakt en av CourseId eller ModuleId måste anges.");

        if (courseId.HasValue)
        {
            var exists = await _uow.CourseExistsAsync(courseId.Value);
            if (!exists)
                throw new ArgumentException($"Course with id {courseId} not found.");
        }
        else if (moduleId.HasValue)
        {
            var module = await _uow.ModuleRepository.GetByIdAsync(moduleId.Value);
            if (module is null)
                throw new ArgumentException($"Module with id {moduleId} not found.");
        }

        var storedFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(fileName);
        await _fileStorage.SaveAsync(fileStream, storedFileName);

        var document = new Document
        {
            FileName = fileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileSize = fileSize,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId,
            CourseId = courseId,
            ModuleId = moduleId
        };

        _uow.DocumentRepository.Create(document);
        await _uow.SaveChangesAsync();

        return MapToDto(document);
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(
        int id, string userId)
    {
        var document = await _uow.DocumentRepository.GetByIdAsync(id);

        if (document is null)
            throw new ArgumentException($"Document with id {id} not found.");

        await EnsureCanAccessAsync(document, userId);

        var stream = await _fileStorage.OpenReadAsync(document.StoredFileName);

        return (stream, document.FileName, document.ContentType);
    }

    public async Task<IEnumerable<DocumentDto>> GetByCourseAsync(int courseId, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new ArgumentException("User not found.");

        var isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");

        if (!isTeacher)
        {
            if (user.CourseId != courseId)
                throw new UnauthorizedAccessException(
                    "Du har inte behörighet att se dokument i denna kurs.");
        }

        var documents = await _uow.DocumentRepository.GetByCourseIdAsync(courseId);

        return documents.Select(MapToDto);
    }

    private static void ValidateFile(string fileName, long fileSize)
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

    private async Task EnsureCanAccessAsync(Document document, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new ArgumentException("User not found.");

        var isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");
        if (isTeacher)
            return;   // Lärare kan se allt

        // Student: måste vara i samma kurs
        if (document.CourseId.HasValue && user.CourseId == document.CourseId)
            return;

        if (document.ModuleId.HasValue)
        {
            var module = await _uow.ModuleRepository.GetByIdAsync(document.ModuleId.Value);
            if (module is not null && user.CourseId == module.CourseId)
                return;
        }

        throw new UnauthorizedAccessException(
            "Du har inte behörighet att komma åt detta dokument.");
    }

    private static DocumentDto MapToDto(Document document) =>
        new(
            document.Id,
            document.FileName,
            document.ContentType,
            document.FileSize,
            document.UploadedAt,
            document.UploadedByUserId,
            document.CourseId,
            document.ModuleId);
}