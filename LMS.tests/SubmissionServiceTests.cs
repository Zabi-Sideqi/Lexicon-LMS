using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Services;
using LMS.Shared.DTOs.CommonDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Service.Contracts;

namespace LMS.tests;

public class SubmissionServiceTests
{
    [Fact]
    public async Task GetSubmissionsForTeacherPagedAsync_IncludesStudentsWithoutSubmissions()
    {
        var teacher = new ApplicationUser { Id = "teacher-1" };
        var student = new ApplicationUser { Id = "student-1", UserName = "Student One" };
        var course = new Course
        {
            Id = 1,
            Name = "Course",
            Description = "Test",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Students = new List<ApplicationUser> { student }
        };
        var assignment = new Activity
        {
            Id = 20,
            ModuleId = 5,
            Type = ActivityType.Lecture,
            Name = "Assignment One",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 1),
            EndTime = new DateTime(2026, 11, 1)
        };

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        userManager.Setup(m => m.FindByIdAsync("teacher-1")).ReturnsAsync(teacher);
        userManager.Setup(m => m.IsInRoleAsync(teacher, "Teacher")).ReturnsAsync(true);

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository.Setup(r => r.GetAllAsync(false))
            .ReturnsAsync(new[] { course });
        courseRepository.Setup(r => r.GetCourseWithStudentsAsync(1, false))
            .ReturnsAsync(course);

        var activityRepository = new Mock<IActivityRepository>();
        activityRepository.Setup(r => r.GetAssignmentsByCourseIdAsync(1, false))
            .ReturnsAsync(new[] { assignment });

        var submissionRepository = new Mock<ISubmissionRepository>();
        submissionRepository.Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(Array.Empty<Submission>());

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(u => u.CourseRepository).Returns(courseRepository.Object);
        unitOfWork.SetupGet(u => u.ActivityRepository).Returns(activityRepository.Object);
        unitOfWork.SetupGet(u => u.SubmissionRepository).Returns(submissionRepository.Object);

        var service = new SubmissionService(
            unitOfWork.Object,
            Mock.Of<IFileStorage>(),
            userManager.Object);

        var result = await service.GetSubmissionsForTeacherPagedAsync(
            "teacher-1",
            new PageParameters { PageNumber = 1, PageSize = 10 });

        var row = Assert.Single(result.Items);
        Assert.Null(row.SubmissionId);
        Assert.Equal("Student One", row.StudentName);
        Assert.Equal("Assignment One", row.ActivityName);
        Assert.Equal(1, result.TotalCount);
    }
}