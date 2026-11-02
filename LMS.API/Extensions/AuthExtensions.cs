using Domain.Models.Configurations;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LMS.API.Authorization;
using Microsoft.AspNetCore.Authorization;
namespace LMS.API.Extensions;

public static class AuthExtensions
{
    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.Section))
                .ValidateDataAnnotations()
                .ValidateOnStart();


        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
                {
                    JwtSettings settings = jwtSettings.Value;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = settings.Issuer,
                        ValidAudience = settings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey))
                    };
                });
    }
    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(opt =>
        {
            opt.Password.RequireDigit = false;
            opt.Password.RequireLowercase = false;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequiredLength = 3;

            opt.User.RequireUniqueEmail = true;
        })
           .AddRoles<IdentityRole>()
           .AddEntityFrameworkStores<ApplicationDbContext>()
           .AddDefaultTokenProviders();
    }
    public static void ConfigurePolicys(this IServiceCollection services)
    {

        services.AddScoped<IAuthorizationHandler, SameCourseAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, OwnSubmissionAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy("Default", policy =>
            {
                policy.RequireAuthenticatedUser();
            })
            .AddPolicy("Student", policy =>
            {
                policy.RequireRole("Student");
            })
            .AddPolicy("Teacher", policy =>
            {
                policy.RequireRole("Teacher");
            })
            .AddPolicy("StudentOrTeacher", policy =>
            {
                policy.RequireRole("Student", "Teacher");
            })
            .AddPolicy("SameCourse", policy =>
            {
                policy.RequireRole("Student");
                policy.AddRequirements(new SameCourseRequirement());
            })
            .AddPolicy("OwnSubmission", policy =>
            {
                policy.RequireRole("Student");
                policy.AddRequirements(new OwnSubmissionRequirement());
            });
    }


    public static void ConfigureCors(this IServiceCollection services)
    {
        //ToDo: Restrict to Blazor BFF
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

        });
    }
}
