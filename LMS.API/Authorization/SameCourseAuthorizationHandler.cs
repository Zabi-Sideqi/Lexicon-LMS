using System.Security.Claims;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LMS.API.Authorization;

public sealed class SameCourseAuthorizationHandler(
    UserManager<ApplicationUser> userManager)
    : AuthorizationHandler<SameCourseRequirement, int>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameCourseRequirement requirement,
        int courseId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var user = await userManager.FindByIdAsync(userId);

        if (user?.CourseId == courseId)
            context.Succeed(requirement);
    }
}