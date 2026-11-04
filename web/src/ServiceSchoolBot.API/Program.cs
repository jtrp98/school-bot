using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServiceSchoolBot.API;
using ServiceSchoolBot.API.Extensions;
using ServiceSchoolBot.Application.Extensions;
using ServiceSchoolBot.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
var hasConfiguredBinding = !string.IsNullOrEmpty(builder.Configuration["urls"]) ||
    !string.IsNullOrEmpty(builder.Configuration["HTTP_PORTS"]) ||
    !string.IsNullOrEmpty(builder.Configuration["HTTPS_PORTS"]) ||
    builder.Configuration.GetSection("Kestrel:Endpoints").Exists();
// Clients reach SchoolBot only through the Gateway: gRPC (h2c) on 8080, health on 8081, loopback by default.
if (!hasConfiguredBinding)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(IPAddress.Loopback, 8080, listen => listen.Protocols = HttpProtocols.Http2);
        options.Listen(IPAddress.Loopback, 8081, listen => listen.Protocols = HttpProtocols.Http1);
    });
}

var memoryStorage = string.Equals(builder.Configuration["SchoolBot:Storage"], "Memory", StringComparison.OrdinalIgnoreCase);
if (memoryStorage && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Memory storage is restricted to Development.");
if (memoryStorage) builder.Services.AddInMemoryInfrastructureServices(builder.Configuration);
else builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddLicenseServices();
builder.Services.AddProjectServices();
builder.Services.AddCurriculumServices();
builder.Services.AddAssessmentServices();
builder.Services.AddManagementStore();
builder.Services.AddManagementServices();
builder.Services.AddPrincipalResolvers();
builder.Services.AddSchoolBotGrpc();

var app = builder.Build();
app.MapSchoolBotGrpc();
app.MapGet("/healthz", () => Results.Ok(new { status = "ok", service = "school-bot-api", poc = true, storage = memoryStorage ? "memory" : "postgresql" }));

if (memoryStorage) app.Logger.LogWarning("School Bot uses development-only in-memory storage.");
app.Run();

public partial class Program;
