
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;
using System.Text;

namespace LMS.API.Services;

internal class DataSeedService : IHostedService
{
    private const string TeacherRole = "Teacher";
    private const string StudentRole = "Student";

    private const string DemoEmail = "DemoUser@Lms.com";
    private const string TeacherEmail = "teacher@lms.test";
    private const string StudentEmail = "erik@lms.test";
    private const string SecondStudentEmail = "maja@lms.test";
    private const string ThirdStudentEmail = "linnea@lms.test";

    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger<DataSeedService> logger;

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

        var userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var password = configuration["password"];

        ArgumentNullException.ThrowIfNull(password, nameof(password));

        try
        {
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (configuration.GetValue<bool>("Seed:ResetOnStartup"))
            {
                logger.LogWarning(
                    "Seed reset enabled. Removing all application data from the development database.");
                await ResetDatabaseAsync(dbContext, cancellationToken);
            }

            await RemoveDemoUserAsync(userManager);
            await CreateRolesAsync(roleManager, [TeacherRole, StudentRole]);

            var teacher = await CreateUserIfMissingAsync(
                userManager, TeacherEmail, TeacherRole, password);
            var student = await CreateUserIfMissingAsync(
                userManager, StudentEmail, StudentRole, password, "Erik", "student@lms.test");
            var secondStudent = await CreateUserIfMissingAsync(
                userManager, SecondStudentEmail, StudentRole, password, "Maja", "student2@lms.test");
            var thirdStudent = await CreateUserIfMissingAsync(
                userManager, ThirdStudentEmail, StudentRole, password, "Linnea", "student3@lms.test");

            await SeedDemoDataAsync(
                dbContext,
                userManager,
                fileStorage,
                teacher,
                student,
                secondStudent,
                thirdStudent,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Demo seed complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Data seed failed.");
            throw;
        }
    }

    private static async Task ResetDatabaseAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Notifications.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Feedbacks.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Submissions.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Documents.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Activities.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Modules.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Users.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Courses.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Roles.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task RemoveDemoUserAsync(
        UserManager<ApplicationUser> userManager)
    {
        var demoUser = await userManager.FindByEmailAsync(DemoEmail);
        if (demoUser is null)
            return;

        var result = await userManager.DeleteAsync(demoUser);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(error => error.Description)));
        }

        logger.LogInformation("Removed obsolete demo account {DemoEmail}.", DemoEmail);
    }

    private static async Task CreateRolesAsync(
        RoleManager<IdentityRole> roleManager,
        string[] roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole { Name = roleName });

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(error => error.Description)));
            }
        }
    }

    private static async Task<ApplicationUser> CreateUserIfMissingAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string role,
        string password,
        string? displayName = null,
        string? legacyEmail = null)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null && legacyEmail is not null)
            user = await userManager.FindByEmailAsync(legacyEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = displayName ?? email
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(error => error.Description)));
            }
        }
        else if (user.Email != email ||
                 (displayName is not null && user.UserName != displayName))
        {
            user.Email = email;
            if (displayName is not null)
                user.UserName = displayName;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        updateResult.Errors.Select(error => error.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        roleResult.Errors.Select(error => error.Description)));
            }
        }

        return user;
    }

    private static async Task SeedDemoDataAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IFileStorage fileStorage,
        ApplicationUser teacher,
        ApplicationUser student,
        ApplicationUser secondStudent,
        ApplicationUser thirdStudent,
        CancellationToken cancellationToken)
    {
        await dbContext.Notifications
            .Where(notification =>
                notification.Message == "Demo: din inlamning har fatt aterkoppling." ||
                notification.Message == "Demo: en student har lamnat in C#-uppgiften.")
            .ExecuteDeleteAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var firstCourse = await GetOrCreateCourseAsync(
            dbContext,
            "C# and Programming",
            "Learn C# fundamentals, problem solving, and object-oriented programming.",
            today.AddDays(-30),
            today.AddDays(60),
            cancellationToken,
            "C# och programmering");
        var secondCourse = await GetOrCreateCourseAsync(
            dbContext,
            "Web Development with .NET",
            "Build accessible websites and APIs with modern tools.",
            today.AddDays(-7),
            today.AddDays(75),
            cancellationToken,
            "Webbutveckling med .NET");

        await AssignTeacherToCourseAsync(dbContext, teacher, firstCourse, cancellationToken);
        await AssignTeacherToCourseAsync(dbContext, teacher, secondCourse, cancellationToken);
        await AssignStudentToCourseAsync(userManager, student, firstCourse.Id);
        await AssignStudentToCourseAsync(userManager, thirdStudent, firstCourse.Id);
        await AssignStudentToCourseAsync(userManager, secondStudent, secondCourse.Id);

        var programmingModule = await GetOrCreateModuleAsync(
            dbContext, firstCourse, "Programming Fundamentals",
            "Variables, conditionals, loops, and methods.", today.AddDays(-30),
            today.AddDays(-22), cancellationToken, "Programmeringsgrunder");
        var projectModule = await GetOrCreateModuleAsync(
            dbContext, firstCourse, "Programming Projects",
            "Plan and build small C# projects.", today.AddDays(-21),
            today.AddDays(35), cancellationToken, "Inlamningsprojekt");
        var frontendModule = await GetOrCreateModuleAsync(
            dbContext, secondCourse, "Frontend and Accessibility",
            "Create accessible and responsive web interfaces.", today.AddDays(-5),
            today.AddDays(16), cancellationToken, "Frontend och tillganglighet");
        var apiModule = await GetOrCreateModuleAsync(
            dbContext, secondCourse, "APIs and Data Storage",
            "Build an API and store data securely.", today.AddDays(17),
            today.AddDays(50), cancellationToken, "API och datalagring");

        var lecture = await GetOrCreateActivityAsync(
            dbContext, programmingModule, ActivityType.Lecture,
            "Introduction to C#", "An overview of C# fundamentals.",
            today.AddDays(-28), today.AddDays(-28), cancellationToken,
            ["Introduktion till C#"]);
        await GetOrCreateActivityAsync(
            dbContext, programmingModule, ActivityType.ELearning,
            "E-learning: conditionals and loops", "Review the material and try the examples.",
            today.AddDays(-25), today.AddDays(-23), cancellationToken,
            ["E-learning: villkor och loopar"]);
        var firstAssignment = await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Assignment,
            "Assignment: C# fundamentals", "Submit your solution as a PDF.",
            today.AddDays(-18), today.AddDays(-12), cancellationToken,
            ["Inlamning: C# fundamentals"]);
        await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Exercise,
            "Coding exercise: method library", "Practice organizing code into methods.",
            today.AddDays(-11), today.AddDays(-10), cancellationToken,
            ["Kodovning: metodbibliotek"]);
        var lateSubmissionAssignment = await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Assignment,
            "Assignment: debugging challenge", "Find and fix the bugs in the sample application.",
            today.AddDays(-9), today.AddDays(-8), cancellationToken);
        var exceptionHandlingAssignment = await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Assignment,
            "Assignment: exception handling", "Add robust error handling to the application.",
            today.AddDays(-6), today.AddDays(-3), cancellationToken);
        await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Assignment,
            "Assignment: algorithm analysis", "Compare the runtime of two search algorithms.",
            today.AddDays(-5), today.AddDays(-4), cancellationToken);
        var programmingAssignment = await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Assignment,
            "Assignment: build a C# application", "Build and submit a small C# application.",
            today.AddDays(-2), today.AddDays(10), cancellationToken);
        var unitTestingAssignment = await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Assignment,
            "Assignment: unit testing", "Write unit tests for your C# application.",
            today.AddDays(11), today.AddDays(18), cancellationToken);
        await GetOrCreateActivityAsync(
            dbContext, projectModule, ActivityType.Assignment,
            "Assignment: code review", "Review a peer's code and provide constructive feedback.",
            today.AddDays(4), today.AddDays(8), cancellationToken);
        var frontendAssignment = await GetOrCreateActivityAsync(
            dbContext, frontendModule, ActivityType.Assignment,
            "Assignment: accessible website", "Explain the accessibility choices in your solution.",
            today.AddDays(2), today.AddDays(16), cancellationToken,
            ["Inlamning: tillganglig webbplats"]);
        await GetOrCreateActivityAsync(
            dbContext, apiModule, ActivityType.ELearning,
            "E-learning: REST APIs", "An introduction to resources and HTTP.",
            today.AddDays(18), today.AddDays(24), cancellationToken,
            ["E-learning: REST API"]);

        await EnsureDocumentAsync(
            dbContext, fileStorage, teacher, firstCourse, programmingModule,
            lecture, "CSharp-course-guide.pdf", "demo-course-guide.pdf",
            "Course guide: C# programming fundamentals.", cancellationToken);
        await EnsureDocumentAsync(
            dbContext, fileStorage, teacher, secondCourse, frontendModule,
            null, "Web-accessibility-checklist.pdf", "demo-accessibility-checklist.pdf",
            "Checklist: accessible web design.", cancellationToken);

        var firstSubmission = await EnsureSubmissionAsync(
            dbContext, fileStorage, student, firstAssignment,
            "CSharp-fundamentals-submission.pdf",
            "demo-student-fundamentals.pdf",
            "Student submission: C# programming fundamentals.",
            today.AddDays(-14), cancellationToken);
        var secondSubmission = await EnsureSubmissionAsync(
            dbContext, fileStorage, thirdStudent, firstAssignment,
            "CSharp-fundamentals-second-submission.pdf",
            "demo-student3-fundamentals.pdf",
            "Second student submission: C# programming fundamentals.",
            today.AddDays(-13), cancellationToken);
        var projectSubmission = await EnsureSubmissionAsync(
            dbContext, fileStorage, student, programmingAssignment,
            "CSharp-application-submission.pdf",
            "demo-student-application.pdf",
            "Student submission: a small C# application.",
            today.AddDays(-1), cancellationToken);
        var frontendSubmission = await EnsureSubmissionAsync(
            dbContext, fileStorage, secondStudent, frontendAssignment,
            "accessible-website-submission.pdf",
            "demo-student2-accessible-website.pdf",
            "Student submission: accessible website design.",
            today.AddDays(-1), cancellationToken);
        var lateSubmission = await EnsureSubmissionAsync(
            dbContext, fileStorage, student, lateSubmissionAssignment,
            "debugging-challenge-submission.pdf",
            "demo-student-debugging-challenge.pdf",
            "Student submission: debugging challenge completed after the deadline.",
            today.AddDays(-7), cancellationToken);
        await EnsureSubmissionAsync(
            dbContext, fileStorage, thirdStudent, exceptionHandlingAssignment,
            "exception-handling-submission.pdf",
            "demo-student3-exception-handling.pdf",
            "Student submission: exception handling exercise.",
            today.AddDays(-4), cancellationToken);
        await EnsureSubmissionAsync(
            dbContext, fileStorage, student, unitTestingAssignment,
            "unit-testing-submission.pdf",
            "demo-student-unit-testing.pdf",
            "Student submission: unit tests for the C# application.",
            today.AddDays(-1), cancellationToken);

        var feedbackItems = new[]
        {
            await EnsureFeedbackAsync(
                dbContext, firstSubmission, teacher,
                "Great work! Your methods are clear and the solution is correct.",
                cancellationToken),
            await EnsureFeedbackAsync(
                dbContext, secondSubmission, teacher,
                "Good solution. Consider adding a few more edge-case tests.",
                cancellationToken),
            await EnsureFeedbackAsync(
                dbContext, projectSubmission, teacher,
                "Well-structured application. The naming and error handling are strong.",
                cancellationToken),
            await EnsureFeedbackAsync(
                dbContext, frontendSubmission, teacher,
                "Excellent accessibility choices. The keyboard navigation is clear.",
                cancellationToken),
            await EnsureFeedbackAsync(
                dbContext, lateSubmission, teacher,
                "Good debugging work. Please plan ahead to meet future deadlines.",
                cancellationToken)
        };

        await EnsureNotificationAsync(
            dbContext, student.Id, feedbackItems[0].Id,
            "Your C# fundamentals submission has received feedback.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, thirdStudent.Id, feedbackItems[1].Id,
            "Your C# fundamentals submission has received feedback.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, student.Id, feedbackItems[2].Id,
            "Your C# application submission has received feedback.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, secondStudent.Id, feedbackItems[3].Id,
            "Your accessible website submission has received feedback.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, teacher.Id, null,
            "A student submitted the C# fundamentals assignment.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, teacher.Id, null,
            "A student submitted the C# application assignment.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, teacher.Id, null,
            "A student submitted the accessible website assignment.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, student.Id, feedbackItems[4].Id,
            "Your debugging challenge submission has received feedback.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, teacher.Id, null,
            "A student submitted the debugging challenge after the deadline.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, teacher.Id, null,
            "A student submitted the exception handling assignment.", cancellationToken);
        await EnsureNotificationAsync(
            dbContext, teacher.Id, null,
            "A student submitted the unit testing assignment.", cancellationToken);
    }

    private static async Task AssignStudentToCourseAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser student,
        int courseId)
    {
        if (student.CourseId == courseId)
            return;

        student.CourseId = courseId;
        var result = await userManager.UpdateAsync(student);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(error => error.Description)));
        }
    }

    private static async Task<Course> GetOrCreateCourseAsync(
        ApplicationDbContext dbContext,
        string name,
        string description,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken,
        string? legacyName = null)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(
            existing => existing.Name == name ||
                        (legacyName != null && existing.Name == legacyName),
            cancellationToken);

        if (course is not null)
        {
            course.Name = name;
            course.Description = description;
            course.StartDate = startDate;
            course.EndDate = endDate;
            await dbContext.SaveChangesAsync(cancellationToken);
            return course;
        }

        course = new Course
        {
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate
        };
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(cancellationToken);
        return course;
    }

    private static async Task<Module> GetOrCreateModuleAsync(
        ApplicationDbContext dbContext,
        Course course,
        string name,
        string description,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken,
        string? legacyName = null)
    {
        var module = await dbContext.Modules.FirstOrDefaultAsync(
            existing => existing.CourseId == course.Id &&
                        (existing.Name == name ||
                         (legacyName != null && existing.Name == legacyName)),
            cancellationToken);
        if (module is not null)
        {
            module.Name = name;
            module.Description = description;
            module.StartDate = startDate;
            module.EndDate = endDate;
            await dbContext.SaveChangesAsync(cancellationToken);
            return module;
        }

        module = new Module
        {
            CourseId = course.Id,
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate
        };
        dbContext.Modules.Add(module);
        await dbContext.SaveChangesAsync(cancellationToken);
        return module;
    }

    private static async Task<Activity> GetOrCreateActivityAsync(
        ApplicationDbContext dbContext,
        Module module,
        ActivityType type,
        string name,
        string description,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken,
        string[]? legacyNames = null)
    {
        var activity = await dbContext.Activities.FirstOrDefaultAsync(
            existing => existing.ModuleId == module.Id &&
                        (existing.Name == name ||
                         (legacyNames != null && legacyNames.Contains(existing.Name))),
            cancellationToken);
        if (activity is not null)
        {
            activity.Type = type;
            activity.Name = name;
            activity.Description = description;
            activity.StartTime = startTime;
            activity.EndTime = endTime;
            await dbContext.SaveChangesAsync(cancellationToken);
            return activity;
        }

        activity = new Activity
        {
            ModuleId = module.Id,
            Type = type,
            Name = name,
            Description = description,
            StartTime = startTime,
            EndTime = endTime
        };
        dbContext.Activities.Add(activity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return activity;
    }

    private static async Task EnsureDocumentAsync(
        ApplicationDbContext dbContext,
        IFileStorage fileStorage,
        ApplicationUser teacher,
        Course course,
        Module module,
        Activity? activity,
        string fileName,
        string storedFileName,
        string contents,
        CancellationToken cancellationToken)
    {
        var fileBytes = CreateDemoPdf(contents);
        using (var stream = new MemoryStream(fileBytes))
            await fileStorage.SaveAsync(stream, storedFileName);

        var document = await dbContext.Documents.FirstOrDefaultAsync(
            existing => existing.StoredFileName == storedFileName,
            cancellationToken);
        if (document is not null)
            return;

        dbContext.Documents.Add(new Document
        {
            FileName = fileName,
            StoredFileName = storedFileName,
            ContentType = "application/pdf",
            FileSize = fileBytes.LongLength,
            UploadedAt = DateTime.UtcNow,
            UploadedById = teacher.Id,
            CourseId = course.Id,
            ModuleId = module.Id,
            ActivityId = activity?.Id
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Submission> EnsureSubmissionAsync(
        ApplicationDbContext dbContext,
        IFileStorage fileStorage,
        ApplicationUser student,
        Activity activity,
        string fileName,
        string storedFileName,
        string contents,
        DateTime submittedAt,
        CancellationToken cancellationToken)
    {
        var fileBytes = CreateDemoPdf(contents);
        using (var stream = new MemoryStream(fileBytes))
            await fileStorage.SaveAsync(stream, storedFileName);

        var submission = await dbContext.Submissions.FirstOrDefaultAsync(
            existing => existing.ActivityId == activity.Id &&
                        existing.StudentId == student.Id,
            cancellationToken);
        if (submission is not null)
            return submission;

        submission = new Submission
        {
            ActivityId = activity.Id,
            StudentId = student.Id,
            FileName = fileName,
            StoredFileName = storedFileName,
            ContentType = "application/pdf",
            FileSize = fileBytes.LongLength,
            SubmittedAt = submittedAt
        };
        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync(cancellationToken);
        return submission;
    }

    private static async Task<Feedback> EnsureFeedbackAsync(
        ApplicationDbContext dbContext,
        Submission submission,
        ApplicationUser teacher,
        string text,
        CancellationToken cancellationToken)
    {
        var feedback = await dbContext.Feedbacks.FirstOrDefaultAsync(
            existing => existing.SubmissionId == submission.Id &&
                        existing.TeacherId == teacher.Id,
            cancellationToken);
        if (feedback is not null)
        {
            feedback.Text = text;
            await dbContext.SaveChangesAsync(cancellationToken);
            return feedback;
        }

        feedback = new Feedback
        {
            SubmissionId = submission.Id,
            TeacherId = teacher.Id,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Feedbacks.Add(feedback);
        await dbContext.SaveChangesAsync(cancellationToken);
        return feedback;
    }

    private static async Task EnsureNotificationAsync(
        ApplicationDbContext dbContext,
        string recipientId,
        int? feedbackId,
        string message,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Notifications.AnyAsync(
            notification => notification.RecipientId == recipientId &&
                            notification.Message == message,
            cancellationToken);
        if (exists)
            return;

        dbContext.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            FeedbackId = feedbackId,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static byte[] CreateDemoPdf(string text)
    {
        var escapedText = text
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
        var content = $"BT /F1 14 Tf 50 700 Td ({escapedText}) Tj ET";
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] " +
                "/Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\n" +
                $"stream\n{content}\nendstream"
        };

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII, 1024, leaveOpen: true);
        writer.Write("%PDF-1.4\n");
        var offsets = new List<long>();
        for (var index = 0; index < objects.Length; index++)
        {
            writer.Flush();
            offsets.Add(stream.Position);
            writer.Write($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }

        writer.Flush();
        var crossReferenceOffset = stream.Position;
        writer.Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets)
            writer.Write($"{offset:D10} 00000 n \n");
        writer.Write(
            $"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\n" +
            $"startxref\n{crossReferenceOffset}\n%%EOF\n");
        writer.Flush();
        return stream.ToArray();
    }

    private static async Task AssignTeacherToCourseAsync(
        ApplicationDbContext dbContext,
        ApplicationUser teacher,
        Course course,
        CancellationToken cancellationToken)
    {
        var courseWithTeachers = await dbContext.Courses
            .Include(existing => existing.Teachers)
            .SingleAsync(existing => existing.Id == course.Id, cancellationToken);

        if (courseWithTeachers.Teachers.Any(
            existingTeacher => existingTeacher.Id == teacher.Id))
        {
            return;
        }

        courseWithTeachers.Teachers.Add(teacher);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
