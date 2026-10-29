
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;

internal class DataSeedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger<DataSeedService> logger;

    private UserManager<ApplicationUser> userManager = null!;
    private RoleManager<IdentityRole> roleManager = null!;
    private string _password = null!;

    private const string DemoRole = "Demo";
    private const string TeacherRole = "Teacher";
    private const string StudentRole = "Student";

    private const string DemoEmail = "DemoUser@Lms.com";
    private const string TeacherEmail = "teacher@lms.test";
    private const string StudentEmail = "student@lms.test";
    private const string StudentModuleName = "Overlap test";

    public DataSeedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DataSeedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (!env.IsDevelopment())
            return;

        userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _password = configuration["password"]!;
        ArgumentNullException.ThrowIfNull(_password, nameof(_password));

        try
        {
            await CreateRolesAsync([DemoRole, TeacherRole, StudentRole]);

            await CreateUserIfMissingAsync(DemoEmail, DemoRole);
            await CreateUserIfMissingAsync(TeacherEmail, TeacherRole);
            await CreateUserIfMissingAsync(StudentEmail, StudentRole);

            await AssignStudentToModuleCourseAsync(dbContext);

            await AssignTeacherToCoursesAsync(dbContext);

            logger.LogInformation("Seed complete");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Data seed failed.");
            throw;
        }
    }

    private async Task CreateRolesAsync(string[] roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var role = new IdentityRole { Name = roleName };
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task CreateUserIfMissingAsync(string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email
            };

            var result = await userManager.CreateAsync(user, _password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }


    private async Task AssignStudentToModuleCourseAsync(
        ApplicationDbContext dbContext)
    {
        var module = await dbContext.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Name == StudentModuleName);

        if (module is null)
        {
            logger.LogWarning(
                "Module '{ModuleName}' was not found. " +
                "The demo student was not assigned to a course.",
                StudentModuleName);

            return;
        }

        var student = await userManager.FindByEmailAsync(StudentEmail);

        if (student is null)
        {
            logger.LogWarning(
                "Demo student {Email} was not found.",
                StudentEmail);

            return;
        }

        if (student.CourseId == module.CourseId)
        {
            logger.LogInformation(
                "Demo student is already assigned to course {CourseId}.",
                module.CourseId);

            return;
        }

        student.CourseId = module.CourseId;

        var result = await userManager.UpdateAsync(student);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation(
            "Assigned demo student {Email} to course {CourseId} " +
            "because it contains module '{ModuleName}'.",
            StudentEmail,
            module.CourseId,
            StudentModuleName);
    }

    private async Task AssignTeacherToCoursesAsync(
    ApplicationDbContext dbContext)
    {
        var teacher = await userManager.FindByEmailAsync(TeacherEmail);

        if (teacher is null)
        {
            logger.LogWarning(
                "Demo teacher {Email} was not found.",
                TeacherEmail);

            return;
        }

        var courses = await dbContext.Courses
            .Include(course => course.Teachers)
            .ToListAsync();

        if (courses.Count == 0)
        {
            logger.LogWarning("No courses were found.");
            return;
        }

        foreach (var course in courses)
        {
            if (course.Teachers.Any(
                existingTeacher => existingTeacher.Id == teacher.Id))
            {
                continue;
            }

            course.Teachers.Add(teacher);

            logger.LogInformation(
                "Assigned demo teacher {Email} to course {CourseId} ({CourseName}).",
                TeacherEmail,
                course.Id,
                course.Name);
        }

        await dbContext.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
