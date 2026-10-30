
using System.Reflection;
using System.Security.Claims;
using LMS.Presentation.Controllers;
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Contracts;

namespace LMS.tests;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetTeacherDashboard_ReturnsOkWithDashboardFromService()
    {
        // Arrange
        const string teacherId = "teacher-1";

        var expected = new TeacherDashboardDto(
            Array.Empty<TeacherDashboardCourseDto>(),
            Array.Empty<TeacherDashboardActivityDto>(),
            Array.Empty<TeacherDashboardSubmissionDto>(),
            Array.Empty<TeacherDashboardAttentionDto>(),
            0,
            0,
            0,
            0,
            0);

        var dashboardService = new Mock<IDashboardService>();

        dashboardService
            .Setup(x => x.GetTeacherDashboardAsync(teacherId))
            .ReturnsAsync(expected);

        var manager = new Mock<IServiceManager>();

        manager
            .SetupGet(x => x.DashboardService)
            .Returns(dashboardService.Object);

        var controller = new DashboardController(manager.Object);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, teacherId)
            },
            "TestAuthentication");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        // Act
        var action = await controller.GetTeacherDashboard();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(action.Result);

        Assert.Same(expected, ok.Value);

        dashboardService.Verify(
            x => x.GetTeacherDashboardAsync(teacherId),
            Times.Once);
    }

    [Fact]
    public void GetTeacherDashboard_HasTeacherRoleAuthorization()
    {
        // Arrange
        var method = typeof(DashboardController)
            .GetMethod(nameof(DashboardController.GetTeacherDashboard));

        Assert.NotNull(method);

        // Act
        var authorize = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        Assert.Equal("Teacher", authorize.Roles);
    }

    [Fact]
    public void Controller_UsesExpectedTeacherRoute()
    {
        // Arrange
        var controllerType = typeof(DashboardController);

        var route = controllerType
            .GetCustomAttributes(typeof(RouteAttribute), true)
            .Cast<RouteAttribute>()
            .Single();

        var method = controllerType
            .GetMethod(nameof(DashboardController.GetTeacherDashboard));

        Assert.NotNull(method);

        var httpGet = method!
            .GetCustomAttributes(typeof(HttpGetAttribute), true)
            .Cast<HttpGetAttribute>()
            .Single();

        // Assert
        Assert.Equal("api/dashboard", route.Template);
        Assert.Equal("teacher", httpGet.Template);
    }
}
