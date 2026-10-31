
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Services;
using LMS.Services.Helpers;
using Moq;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.tests;

public class DashboardServiceTests
{
    private const string TeacherId = "teacher-1";

    private sealed class TestContext
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<ICourseRepository> Courses { get; } = new();
        public Mock<IActivityRepository> Activities { get; } = new();
        public Mock<ISubmissionRepository> Submissions { get; } = new();

        public Mock<UserManager<ApplicationUser>> UserManager { get; }

        public Mock<ISubmissionService> SubmissionService { get; } = new();

        public DashboardService Service { get; }

        public ApplicationUser Teacher { get; } = new()
        {
            Id = TeacherId,
            UserName = "Teacher One",
            Email = "teacher@test.com"
        };

        public TestContext(bool isTeacher = true)
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();

            UserManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            UserManager
                .Setup(x => x.FindByIdAsync(TeacherId))
                .ReturnsAsync(Teacher);

            UserManager
                .Setup(x => x.IsInRoleAsync(Teacher, "Teacher"))
                .ReturnsAsync(isTeacher);

            UnitOfWork.SetupGet(x => x.CourseRepository)
                .Returns(Courses.Object);

            UnitOfWork.SetupGet(x => x.ActivityRepository)
                .Returns(Activities.Object);

            UnitOfWork.SetupGet(x => x.SubmissionRepository)
                .Returns(Submissions.Object);

            Courses.Setup(x => x.GetAllAsync(false))
                .ReturnsAsync(Enumerable.Empty<Course>());

            Service = new DashboardService(
                UnitOfWork.Object,
                UserManager.Object,
                SubmissionService.Object);
        }
    }

    [Fact]
    public async Task GetTeacherDashboardAsync_EmptyTeacherId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var context = new TestContext();

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => context.Service.GetTeacherDashboardAsync(" "));

        // Assert
        Assert.Equal(
            "A signed-in teacher is required.",
            exception.Message);

        context.UserManager.Verify(
            x => x.FindByIdAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTeacherDashboardAsync_NonTeacher_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var context = new TestContext(isTeacher: false);

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => context.Service.GetTeacherDashboardAsync(TeacherId));

        // Assert
        Assert.Equal(
            "Only teachers can view their dashboard.",
            exception.Message);

        context.Courses.Verify(
            x => x.GetAllAsync(false),
            Times.Never);
    }

    [Fact]
    public async Task GetTeacherDashboardAsync_TeacherNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var context = new TestContext();

        context.UserManager
            .Setup(x => x.FindByIdAsync(TeacherId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => context.Service.GetTeacherDashboardAsync(TeacherId));

        // Assert
        Assert.Equal(
            "Only teachers can view their dashboard.",
            exception.Message);

        context.Courses.Verify(
            x => x.GetAllAsync(false),
            Times.Never);
    }

    [Fact]
    public async Task GetTeacherDashboardAsync_ReturnsAggregatedTeacherDashboard()
    {
        // Arrange
        var context = new TestContext();

        var now = SchoolTime.FromUtc(DateTime.UtcNow);

        var student1 = new ApplicationUser
        {
            Id = "student-1",
            UserName = "Student One",
            Email = "student1@test.com"
        };

        var student2 = new ApplicationUser
        {
            Id = "student-2",
            UserName = "Student Two",
            Email = "student2@test.com"
        };

        var course = new Course
        {
            Id = 1,
            Name = "C# Development",
            Description = "Programming course",
            StartDate = now.Date.AddDays(-10),
            EndDate = now.Date.AddDays(30),
            Students = new List<ApplicationUser>
            {
                student1,
                student2
            }
        };

        var upcomingActivity = new Activity
        {
            Id = 10,
            ModuleId = 1,
            Type = ActivityType.Lecture,
            Name = "Upcoming lecture",
            Description = "Lecture description",
            StartTime = now.AddDays(1),
            EndTime = now.AddDays(1).AddHours(1)
        };

        var assignment = new Activity
        {
            Id = 20,
            ModuleId = 2,
            Type = ActivityType.Assignment,
            Name = "Final assignment",
            Description = "Assignment description",
            StartTime = now.AddDays(-3),
            EndTime = now.AddDays(-1)
        };

        var submission = new Submission
        {
            Id = 100,
            ActivityId = assignment.Id,
            Activity = assignment,
            StudentId = student1.Id,
            Student = student1,
            FileName = "assignment.pdf",
            StoredFileName = "stored-assignment.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            SubmittedAt = DateTime.UtcNow.AddHours(-12),
            Feedbacks = new List<Feedback>()
        };

        context.Courses
            .Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(new[] { course });

        context.Courses
            .Setup(x => x.GetCourseWithStudentsAsync(course.Id, false))
            .ReturnsAsync(course);

        context.Activities
            .Setup(x => x.GetByCourseIdAsync(course.Id, false))
            .ReturnsAsync(new[] { upcomingActivity, assignment });

        context.Submissions
            .Setup(x => x.GetByCourseIdAsync(course.Id, false))
            .ReturnsAsync(new[] { submission });

        // Act
        var result = await context.Service.GetTeacherDashboardAsync(TeacherId);

        // Assert
        Assert.Single(result.Courses);
        Assert.Equal("C# Development", result.Courses.First().Name);
        Assert.Equal(2, result.Courses.First().StudentCount);
        Assert.True(result.Courses.First().IsActive);
        Assert.Equal(1, result.Courses.First().UpcomingActivityCount);

        Assert.Single(result.UpcomingActivities);
        Assert.Equal("Upcoming lecture", result.UpcomingActivities.First().Name);

        Assert.Single(result.RecentSubmissions);
        Assert.Equal(100, result.RecentSubmissions.First().SubmissionId);
        Assert.Equal("Student One", result.RecentSubmissions.First().StudentName);

        Assert.Equal(1, result.TotalSubmissions);
        Assert.Equal(1, result.PendingFeedbackCount);
        Assert.Equal(1, result.LateSubmissionCount);
        Assert.Equal(1, result.MissingSubmissionCount);
        Assert.Equal(2, result.StudentCount);

        Assert.Equal(2, result.AttentionItems.Count);
    }
}
