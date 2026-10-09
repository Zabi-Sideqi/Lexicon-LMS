namespace LMS.Shared.DTOs.CourseDtos;

public sealed record StudentCourseDto(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate);