
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

    public IAuthService AuthService => _authService.Value;
    public ICourseService CourseService => _courseService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IUserService UserService => _userService.Value;
    public IDocumentService DocumentService => _documentService.Value;
    public IActivityService ActivityService => _activityService.Value;

    public ServiceManager(
        Lazy<IAuthService> authService,
        Lazy<ICourseService> courseService,
        Lazy<IModuleService> moduleService,
        Lazy<IUserService> userService,
        Lazy<IDocumentService> documentService,
        Lazy<IActivityService> activityService)
    {
        _authService = authService;
        _courseService = courseService;
        _moduleService = moduleService;
        _userService = userService;
        _documentService = documentService;
        _activityService = activityService;
    }
}
