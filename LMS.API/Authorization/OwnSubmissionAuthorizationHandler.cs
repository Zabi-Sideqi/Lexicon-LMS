using Domain.Contracts;
using LMS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LMS.API.Authorization;

public sealed class OwnSubmissionAuthorizationHandler(
    IUnitOfWork unitOfWork)
    : AuthorizationHandler<OwnSubmissionRequirement, int>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnSubmissionRequirement requirement,
        int submissionId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var submission = await unitOfWork.SubmissionRepository
            .GetByIdAsync(submissionId);

        if (submission?.StudentId == userId)
            context.Succeed(requirement);
    }
}