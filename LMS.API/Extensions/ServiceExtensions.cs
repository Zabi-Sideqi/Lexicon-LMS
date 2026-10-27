
using LMS.Infrastructure.Repositories;
using LMS.Services;
using Service.Contracts;

namespace LMS.API.Extensions;

public static class ServiceExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static void AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddLazy<IAuthService>();
        services.AddLazy<IUserService>();
        services.AddLazy<ICourseService>();
        services.AddLazy<IModuleService>();
        services.AddLazy<IDocumentService>();
        services.AddLazy<IActivityService>();
        services.AddLazy<ISubmissionService>();
        services.AddLazy<IFeedbackService>();
        services.AddLazy<INotificationService>();
        services.AddLazy<IDashboardService>();
    }
}
