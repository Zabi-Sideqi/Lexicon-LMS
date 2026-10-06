using LMS.API.Extensions;
using LMS.API.Services;
using LMS.Infrastructure.Data;
using LMS.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContext")
            ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddControllers(opt =>
        {
            opt.ReturnHttpNotAcceptable = true;
            opt.Filters.Add(new ProducesAttribute("application/json"));
        })
        .AddApplicationPart(typeof(AssemblyReference).Assembly);

        builder.Services.AddHostedService<DataSeedService>();

        // Registrera OpenAPI med Bearer Security Scheme för .NET 10
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Klistra in din JWT access token här."
                };

                return Task.CompletedTask;
            });
        });

        builder.Services.AddRepositories();
        builder.Services.AddServiceLayer();
        builder.Services.ConfigureAuthentication(builder.Configuration);
        builder.Services.ConfigureIdentity();
        builder.Services.ConfigurePolicys();

        var app = builder.Build();

        app.ConfigureExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            // Aktivera Scalar UI med den nya metoden
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("LMS API Documentation")
                    .WithTheme(ScalarTheme.Purple)
                    .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme);
            });
        }

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers()
           .RequireAuthorization("Default");

        app.Run();
    }
}