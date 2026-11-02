using LMS.Blazor.Client.Services.ApiProxy;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace LMS.Blazor.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("StudentOrTeacher", policy =>
            {
                policy.RequireRole("Student", "Teacher");
            });
        });
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthenticationStateDeserialization();
        builder.Services.AddScoped(_ => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        });
        builder.Services.AddScoped<IApiProxyClient, ApiProxyClient>();

        await builder.Build().RunAsync();
    }
}
