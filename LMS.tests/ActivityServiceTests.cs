
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Services;
using LMS.Shared.DTOs.ActivityDtos;
using Moq;
using Xunit;

namespace LMS.tests;

public class ActivityServiceTests
{
    // Startdatum och slutdatum som används i våra tester.
    private static readonly DateTime ModuleStart =
        new(2026, 10, 1);

    private static readonly DateTime ModuleEnd =
        new(2026, 10, 31);

    // Skapar en testmodul som aktiviteterna kan tillhöra.
    // Modulen används som testdata och sparas inte i databasen.
    private static Module CreateModule()
    {
        return new Module
        {
            Id = 1,
            CourseId = 1,
            Name = "Test module",
            Description = "Test",
            StartDate = ModuleStart,
            EndDate = ModuleEnd
        };
    }

    // Skapar testdata för en ny aktivitet.
    // Standardtiden är 09.00–10.00, men vi kan ange andra tider.
    private static ActivityCreateDto CreateDto(
        DateTime? start = null,
        DateTime? end = null)
    {
        return new ActivityCreateDto
        {
            ModuleId = 1,
            Type = "Lecture",
            Name = "Test activity",
            Description = "Test",
            StartTime = start ?? new DateTime(2026, 10, 10, 9, 0, 0),
            EndTime = end ?? new DateTime(2026, 10, 10, 10, 0, 0)
        };
    }

    // Skapar ActivityService med mockar i stället för en riktig databas.
    // Vi kan skicka in befintliga aktiviteter för att testa överlappning.
    // Metoden returnerar tjänsten och mockarna så att vi kan kontrollera metodanrop.
    private static (
        ActivityService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IActivityRepository> ActivityRepository)
        CreateService(IEnumerable<Activity>? existingActivities = null)
    {
        // Skapar en mock av modulrepositoryt som returnerar vår testmodul.
        var moduleRepository = new Mock<IModuleRepository>();

        moduleRepository
            .Setup(r => r.GetByIdAsync(1, false))
            .ReturnsAsync(CreateModule());

        // Skapar en mock av aktivitetsrepositoryt.
        // Den returnerar befintliga aktiviteter eller en tom lista.
        var activityRepository = new Mock<IActivityRepository>();

        activityRepository
            .Setup(r => r.GetByModuleIdAsync(1, false))
            .ReturnsAsync(
                existingActivities?.ToList() ?? new List<Activity>());

        // Skapar en mock av UnitOfWork och kopplar den till våra mockade repositories.
        var unitOfWork = new Mock<IUnitOfWork>();

        unitOfWork
            .SetupGet(u => u.ModuleRepository)
            .Returns(moduleRepository.Object);

        unitOfWork
            .SetupGet(u => u.ActivityRepository)
            .Returns(activityRepository.Object);

        // Simulerar en lyckad sparning utan att använda en riktig databas.
        unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Returnerar tjänsten och mockarna som används i testerna.
        return (
            new ActivityService(unitOfWork.Object),
            unitOfWork,
            activityRepository);
    }

    [Fact]
    public async Task CreateAsync_StartAfterEnd_ThrowsArgumentException()
    {
        // Arrange: förbered tjänsten och skapa en aktivitet med ogiltiga tider.
        var (service, unitOfWork, repository) = CreateService();

        // Aktiviteten börjar kl. 11.00 men slutar kl. 10.00.
        var dto = CreateDto(
            new DateTime(2026, 10, 10, 11, 0, 0),
            new DateTime(2026, 10, 10, 10, 0, 0));

        // Act & Assert: kontrollera att tjänsten kastar ett undantag.
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto));

        // Kontrollera att aktiviteten inte skickades till repositoryt för att skapas.
        repository.Verify(
            r => r.Create(It.IsAny<Activity>()), Times.Never);

        // Kontrollera att ingen sparning försöktes.
        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_OutsideModulePeriod_ThrowsArgumentException()
    {
        // Arrange: förbered tjänsten och skapa en aktivitet utanför modulens datum.
        var (service, unitOfWork, repository) = CreateService();

        // Aktiviteten är den 1 november, men modulen slutar den 31 oktober.
        var dto = CreateDto(
            new DateTime(2026, 11, 1, 9, 0, 0),
            new DateTime(2026, 11, 1, 10, 0, 0));

        // Act & Assert: kontrollera att tjänsten avvisar aktiviteten.
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto));

        // Kontrollera att aktiviteten inte skapades.
        repository.Verify(
            r => r.Create(It.IsAny<Activity>()), Times.Never);

        // Kontrollera att ingen sparning försöktes.
        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_OverlappingActivity_ThrowsArgumentException()
    {
        // Arrange: skapa en aktivitet som redan finns i modulen.
        var existing = new Activity
        {
            Id = 10,
            ModuleId = 1,
            Type = ActivityType.Lecture,
            Name = "Existing activity",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 9, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 10, 0, 0)
        };

        // Låt den mockade repositoryn returnera den befintliga aktiviteten.
        var (service, unitOfWork, repository) =
            CreateService(new[] { existing });

        // Den nya aktiviteten överlappar den befintliga mellan kl. 09.30 och 10.00.
        var dto = CreateDto(
            new DateTime(2026, 10, 10, 9, 30, 0),
            new DateTime(2026, 10, 10, 10, 30, 0));

        // Act & Assert: kontrollera att överlappande aktiviteter avvisas.
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto));

        // Kontrollera att den nya aktiviteten inte skapades.
        repository.Verify(
            r => r.Create(It.IsAny<Activity>()), Times.Never);

        // Kontrollera att ingen sparning försöktes.
        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidActivity_SavesActivity()
    {
        // Arrange: förbered tjänsten och skapa en giltig aktivitet.
        var (service, unitOfWork, repository) = CreateService();
        var dto = CreateDto();

        // Act: försök att skapa aktiviteten.
        var result = await service.CreateAsync(dto);

        // Assert: kontrollera att resultatet innehåller rätt uppgifter.
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.ModuleId, result.ModuleId);
        Assert.Equal("Lecture", result.Type);

        // Kontrollera att Create anropades en gång med rätt modul-ID och namn.
        repository.Verify(
            r => r.Create(It.Is<Activity>(a =>
                a.ModuleId == dto.ModuleId &&
                a.Name == dto.Name)),
            Times.Once);

        // Kontrollera att sparning begärdes exakt en gång.
        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ActivitiesTouchButDoNotOverlap_SavesActivity()
    {
        // Arrange: skapa en befintlig aktivitet som pågår kl. 09.00–10.00.
        var existing = new Activity
        {
            Id = 10,
            ModuleId = 1,
            Type = ActivityType.Lecture,
            Name = "Existing activity",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 9, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 10, 0, 0)
        };

        // Låt repositoryt returnera den befintliga aktiviteten.
        var (service, unitOfWork, repository) =
            CreateService(new[] { existing });

        // Den nya aktiviteten börjar exakt när den befintliga slutar.
        var dto = CreateDto(
            new DateTime(2026, 10, 10, 10, 0, 0),
            new DateTime(2026, 10, 10, 11, 0, 0));

        // Act: skapa aktiviteten som följer direkt efter den befintliga.
        await service.CreateAsync(dto);

        // Assert: kontrollera att Create anropades exakt en gång.
        repository.Verify(
            r => r.Create(It.IsAny<Activity>()), Times.Once);

        // Kontrollera att sparning begärdes exakt en gång.
        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
[Theory]
[InlineData("ELearning")]
[InlineData("Other")]
public async Task CreateAsync_NewActivityTypes_SavesActivity(string type)
    {
        // Arrange
        var (service, unitOfWork, repository) = CreateService();
        var dto = CreateDto();
        dto.Type = type;

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal(type, result.Type);

        repository.Verify(
            r => r.Create(It.Is<Activity>(a =>
                a.Type.ToString() == type)),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
[Fact]
public async Task CreateAsync_InvalidActivityType_ThrowsArgumentException()
    {
        // Arrange
        var (service, unitOfWork, repository) = CreateService();
        var dto = CreateDto();
        dto.Type = "InvalidType";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(dto));

        repository.Verify(
            r => r.Create(It.IsAny<Activity>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
[Fact]
public async Task UpdateAsync_ValidActivity_UpdatesAndSavesActivity()
    {
        // Arrange
        var existing = new Activity
        {
            Id = 10,
            ModuleId = 1,
            Type = ActivityType.Lecture,
            Name = "Old activity",
            Description = "Old description",
            StartTime = new DateTime(2026, 10, 10, 9, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 10, 0, 0)
        };

        var (service, unitOfWork, repository) =
            CreateService(new[] { existing });

        repository
            .Setup(r => r.GetByIdAsync(10, true))
            .ReturnsAsync(existing);

        var dto = new ActivityUpdateDto
        {
            Type = "ELearning",
            Name = "Updated activity",
            Description = "Updated description",
            StartTime = new DateTime(2026, 10, 10, 11, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 12, 0, 0)
        };

        // Act
        await service.UpdateAsync(10, dto);

        // Assert
        Assert.Equal(ActivityType.ELearning, existing.Type);
        Assert.Equal("Updated activity", existing.Name);
        Assert.Equal("Updated description", existing.Description);

        repository.Verify(
            r => r.Update(existing),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
   
[Fact]
public async Task UpdateAsync_StartAfterEnd_ThrowsArgumentException()
    {
        // Arrange
        var existing = new Activity
        {
            Id = 10,
            ModuleId = 1,
            Type = ActivityType.Lecture,
            Name = "Test activity",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 9, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 10, 0, 0)
        };

        var (service, unitOfWork, repository) =
            CreateService(new[] { existing });

        repository
            .Setup(r => r.GetByIdAsync(10, true))
            .ReturnsAsync(existing);

        var dto = new ActivityUpdateDto
        {
            Type = "Lecture",
            Name = "Updated activity",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 11, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 10, 0, 0)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(10, dto));

        repository.Verify(
            r => r.Update(It.IsAny<Activity>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
  
[Fact]
public async Task UpdateAsync_OverlappingActivity_ThrowsArgumentException()
    {
        // Arrange
        var existing = new Activity
        {
            Id = 10,
            ModuleId = 1,
            Type = ActivityType.Lecture,
            Name = "Activity to update",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 9, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 10, 0, 0)
        };

        var otherActivity = new Activity
        {
            Id = 20,
            ModuleId = 1,
            Type = ActivityType.Exercise,
            Name = "Other activity",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 11, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 12, 0, 0)
        };

        var (service, unitOfWork, repository) =
            CreateService(new[] { existing, otherActivity });

        repository
            .Setup(r => r.GetByIdAsync(10, true))
            .ReturnsAsync(existing);

        var dto = new ActivityUpdateDto
        {
            Type = "Lecture",
            Name = "Updated activity",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 11, 30, 0),
            EndTime = new DateTime(2026, 10, 10, 12, 30, 0)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(10, dto));

        repository.Verify(
            r => r.Update(It.IsAny<Activity>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

   
[Fact]
public async Task UpdateAsync_ActivityNotFound_ThrowsArgumentException()
    {
        // Arrange
        var (service, unitOfWork, repository) = CreateService();

        repository
            .Setup(r => r.GetByIdAsync(99, true))
            .ReturnsAsync((Activity?)null);

        var dto = new ActivityUpdateDto
        {
            Type = "Lecture",
            Name = "Updated activity",
            Description = "Test",
            StartTime = new DateTime(2026, 10, 10, 11, 0, 0),
            EndTime = new DateTime(2026, 10, 10, 12, 0, 0)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => service.UpdateAsync(99, dto));
        repository.Verify(
            r => r.Update(It.IsAny<Activity>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

}
