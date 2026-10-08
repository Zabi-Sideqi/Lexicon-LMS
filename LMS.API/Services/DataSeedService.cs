using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

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

    public DataSeedService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DataSeedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        if (!env.IsDevelopment()) return;

        userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
                            ?? throw new ArgumentNullException();

        roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()
                            ?? throw new ArgumentNullException();

        _password = configuration["password"]!;
        ArgumentNullException.ThrowIfNull(_password, nameof(_password));

        try
        {
            await CreateRolesAsync([DemoRole, TeacherRole, StudentRole]);

            // Skapar bara användare som saknas, så det fungerar även om databasen redan har användare
            await CreateUserIfMissingAsync(DemoEmail, DemoRole);
            await CreateUserIfMissingAsync(TeacherEmail, TeacherRole);
            await CreateUserIfMissingAsync(StudentEmail, StudentRole);

            logger.LogInformation("Seed complete");
        }
        catch (Exception ex)
        {
            logger.LogError($"Data seed fail with message: {ex.Message}. Exceeption: {ex.InnerException}");
            throw;
        }
    }

    private async Task CreateRolesAsync(string[] rolenames)
    {
        foreach (string rolename in rolenames)
        {
            if (await roleManager.RoleExistsAsync(rolename)) continue;
            var role = new IdentityRole { Name = rolename };
            var res = await roleManager.CreateAsync(role);

            if (!res.Succeeded) throw new Exception
                    (string.Join("\n", res.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
    }

    private async Task CreateUserIfMissingAsync(string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser { Email = email, UserName = email };

            var result = await userManager.CreateAsync(user, _password);
            if (!result.Succeeded)
                throw new Exception(string.Join("\n",
                    result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                throw new Exception(string.Join("\n",
                    roleResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}