
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Services;
using LMS.Shared.DTOs.ModuleDtos;
using Moq;
using Xunit;

namespace LMS.tests;

public class ModuleServiceTests
{
    [Fact]
    public async Task CreateAsync_StartDateAfterEndDate_ThrowsArgumentException()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Name = "Test course",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        };

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(course);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForCreationDto
        {
            CourseId = 1,
            Name = "Test module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 20),
            EndDate = new DateTime(2026, 10, 10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto));
    }


    [Fact]
    public async Task CreateAsync_DatesOutsideCourse_ThrowsArgumentException()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Name = "Test course",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        };

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(course);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForCreationDto
        {
            CourseId = 1,
            Name = "Outside module",
            Description = "Test",
            StartDate = new DateTime(2026, 9, 25),
            EndDate = new DateTime(2026, 10, 5)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto));
    }


    [Fact]
    public async Task CreateAsync_OverlappingDates_ThrowsArgumentException()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Name = "Test course",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        };

        var existingModule = new Module
        {
            Id = 1,
            CourseId = 1,
            Name = "Existing module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 10),
            EndDate = new DateTime(2026, 10, 15)
        };

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(course);

        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository
            .Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(new List<Module> { existingModule });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);
        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForCreationDto
        {
            CourseId = 1,
            Name = "Overlapping module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 12),
            EndDate = new DateTime(2026, 10, 18)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto));
    }


    [Fact]
    public async Task CreateAsync_ValidModule_CreatesAndReturnsModule()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Name = "Test course",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        };

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(course);

        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository
            .Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(new List<Module>());

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);
        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);
        unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForCreationDto
        {
            CourseId = 1,
            Name = "Valid module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 10),
            EndDate = new DateTime(2026, 10, 15)
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal("Valid module", result.Name);
        Assert.Equal(1, result.CourseId);

        moduleRepository.Verify(
            r => r.Create(It.IsAny<Module>()), Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task UpdateAsync_ValidModule_UpdatesSuccessfully()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Name = "Test course",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        };

        var existingModule = new Module
        {
            Id = 1,
            CourseId = 1,
            Name = "Old name",
            Description = "Old description",
            StartDate = new DateTime(2026, 10, 10),
            EndDate = new DateTime(2026, 10, 15)
        };

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(course);

        var moduleRepository = new Mock<IModuleRepository>();

        moduleRepository
            .Setup(r => r.GetByIdAsync(1, true))
            .ReturnsAsync(existingModule);

        moduleRepository
            .Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(new List<Module> { existingModule });

        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);

        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);

        unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForUpdateDto
        {
            CourseId = 1,
            Name = "Updated name",
            Description = "Updated description",
            StartDate = new DateTime(2026, 10, 11),
            EndDate = new DateTime(2026, 10, 16)
        };

        // Act
        await service.UpdateAsync(1, dto);

        // Assert
        Assert.Equal("Updated name", existingModule.Name);
        Assert.Equal("Updated description", existingModule.Description);

        moduleRepository.Verify(
            r => r.Update(existingModule), Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_OverlappingDates_ThrowsArgumentException()
    {
        var course = new Course
        {
            Id = 1,
            Name = "Test course",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        };

        var moduleToUpdate = new Module
        {
            Id = 1,
            CourseId = 1,
            Name = "Module to update",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 5),
            EndDate = new DateTime(2026, 10, 9)
        };

        var otherModule = new Module
        {
            Id = 2,
            CourseId = 1,
            Name = "Other module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 15),
            EndDate = new DateTime(2026, 10, 20)
        };

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(course);

        var moduleRepository = new Mock<IModuleRepository>();

        moduleRepository
            .Setup(r => r.GetByIdAsync(1, true))
            .ReturnsAsync(moduleToUpdate);

        moduleRepository
            .Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(new List<Module> { moduleToUpdate, otherModule });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);
        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForUpdateDto
        {
            CourseId = 1,
            Name = "Updated module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 17),
            EndDate = new DateTime(2026, 10, 22)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task UpdateAsync_DatesOutsideCourse_ThrowsArgumentException()
    {
        var course = new Course
        {
            Id = 1,
            Name = "Test course",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31)
        };

        var existingModule = new Module
        {
            Id = 1,
            CourseId = 1,
            Name = "Test module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 10),
            EndDate = new DateTime(2026, 10, 15)
        };

        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(course);

        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository
            .Setup(r => r.GetByIdAsync(1, true))
            .ReturnsAsync(existingModule);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);
        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForUpdateDto
        {
            CourseId = 1,
            Name = "Updated module",
            Description = "Test",
            StartDate = new DateTime(2026, 9, 25),
            EndDate = new DateTime(2026, 10, 5)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task UpdateAsync_ModuleNotFound_ThrowsResourceNotFoundException()
    {
        var moduleRepository = new Mock<IModuleRepository>();

        moduleRepository
            .Setup(r => r.GetByIdAsync(99, true))
            .ReturnsAsync((Module?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);

        var service = new ModuleService(unitOfWork.Object);

        var dto = new ModuleForUpdateDto
        {
            CourseId = 1,
            Name = "Updated module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 10),
            EndDate = new DateTime(2026, 10, 15)
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => service.UpdateAsync(99, dto));
    }

    [Fact]
    public async Task GetByCourseIdAsync_ReturnsModulesOrderedByStartDate()
    {
        // Arrange
        var modules = new List<Module>
    {
        new Module
        {
            Id = 2,
            CourseId = 1,
            Name = "Later module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 20),
            EndDate = new DateTime(2026, 10, 25)
        },
        new Module
        {
            Id = 1,
            CourseId = 1,
            Name = "Earlier module",
            Description = "Test",
            StartDate = new DateTime(2026, 10, 5),
            EndDate = new DateTime(2026, 10, 10)
        }
    };

        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository
            .Setup(r => r.GetByCourseIdAsync(1, false))
            .ReturnsAsync(modules);
        var courseRepository = new Mock<ICourseRepository>();
        courseRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(new Course
            {
                Id = 1,
                Name = "Test course",
                Description = "Test",
                StartDate = new DateTime(2026, 10, 1),
                EndDate = new DateTime(2026, 10, 31)
            });
        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .SetupGet(u => u.CourseRepository)
            .Returns(courseRepository.Object);

        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);
        var service = new ModuleService(unitOfWork.Object);

        // Act
        var result = await service.GetByCourseIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Earlier module", result.First().Name);
        Assert.Equal("Later module", result.Last().Name);
    }

}
