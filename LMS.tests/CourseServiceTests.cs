using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Services;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LMS.tests;

public class CourseServiceTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>()).Object;
    }

    [Fact]
    public async Task UpdateAsync_RejectsCoursePeriodThatExcludesExistingModule()
    {
        var course = new Course
        {
            Id = 1,
            Name = "Course",
            Description = "Test",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 6, 30)
        };

        var module = new Module
        {
            Id = 10,
            CourseId = 1,
            Name = "June module",
            Description = "Test",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 30)
        };

        var courses = new Mock<ICourseRepository>();
        courses.Setup(r => r.GetByIdAsync(1, true)).ReturnsAsync(course);

        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(new[] { module });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.CourseRepository).Returns(courses.Object);
        uow.SetupGet(x => x.ModuleRepository).Returns(modules.Object);

        var service = new CourseService(uow.Object, CreateUserManager());
        var dto = new CourseForUpdateDto
        {
            Name = "Course",
            Description = "Updated",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 6, 15)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(1, dto));

        Assert.Equal(new DateTime(2026, 6, 30), course.EndDate);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AllowsCoursePeriodThatContainsEveryModule()
    {
        var course = new Course
        {
            Id = 1,
            Name = "Course",
            Description = "Test",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 6, 30)
        };

        var module = new Module
        {
            Id = 10,
            CourseId = 1,
            Name = "June module",
            Description = "Test",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 15)
        };

        var courses = new Mock<ICourseRepository>();
        courses.Setup(r => r.GetByIdAsync(1, true)).ReturnsAsync(course);

        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(new[] { module });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.CourseRepository).Returns(courses.Object);
        uow.SetupGet(x => x.ModuleRepository).Returns(modules.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new CourseService(uow.Object, CreateUserManager());
        var dto = new CourseForUpdateDto
        {
            Name = "Course",
            Description = "Updated",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 6, 15)
        };

        await service.UpdateAsync(1, dto);

        Assert.Equal(dto.EndDate, course.EndDate);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}