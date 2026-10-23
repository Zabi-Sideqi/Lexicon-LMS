
using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<ICourseService> _courseService;
    private readonly Lazy<IModuleService> _moduleService;
    private readonly Lazy<IUserService> _userService;
    private readonly Lazy<IDocumentService> _documentService;
    private readonly Lazy<IActivityService> _activityService;
    private readonly Lazy<ISubmissionService> _submissionService;
    private readonly Lazy<IFeedbackService> _feedbackService;
    private readonly Lazy<INotificationService> _notificationService;

    public IAuthService AuthService => _authService.Value;
    public ICourseService CourseService => _courseService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IUserService UserService => _userService.Value;
    public IDocumentService DocumentService => _documentService.Value;
    public IActivityService ActivityService => _activityService.Value;
    public ISubmissionService SubmissionService => _submissionService.Value;
    public IFeedbackService FeedbackService => _feedbackService.Value;
    public INotificationService NotificationService => _notificationService.Value;

    public ServiceManager(
        Lazy<IAuthService> authService,
        Lazy<ICourseService> courseService,
        Lazy<IModuleService> moduleService,
        Lazy<IUserService> userService,
        Lazy<IDocumentService> documentService,
        Lazy<IActivityService> activityService,
        Lazy<ISubmissionService> submissionService,
        Lazy<IFeedbackService> feedbackService,
        Lazy<INotificationService> notificationService)

    {
        _authService = authService;
        _courseService = courseService;
        _moduleService = moduleService;
        _userService = userService;
        _documentService = documentService;
        _activityService = activityService;
        _submissionService = submissionService;
        _feedbackService = feedbackService;
        _notificationService = notificationService;
    }
}
