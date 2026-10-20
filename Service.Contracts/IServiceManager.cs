namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    ICourseService CourseService { get; }
    IModuleService ModuleService { get; }
    IUserService UserService { get; }
    IDocumentService DocumentService { get; }
    IActivityService ActivityService { get; }
    ISubmissionService SubmissionService { get; }
    IFeedbackService FeedbackService { get; }
    INotificationService NotificationService { get; }
}