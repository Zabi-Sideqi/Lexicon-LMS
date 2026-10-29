using Microsoft.AspNetCore.Authorization;

namespace LMS.API.Authorization;

public sealed class SameCourseRequirement : IAuthorizationRequirement
{
}