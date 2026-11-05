using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Implementations.Management;
using ServiceSchoolBot.Application.Services.Implementations.Projects;
using ServiceSchoolBot.Application.Services.Implementations.TenantRecheck;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Projects;

namespace ServiceSchoolBot.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITenantRecheckService, TenantRecheckService>();
        return services;
    }

    public static IServiceCollection AddLicenseServices(this IServiceCollection services)
    {
        services.AddSingleton<LicenseAdminService.ReplayState>();
        services.AddSingleton<LicenseDeviceService.ReplayState>();
        services.AddScoped<ILicenseAdminService, LicenseAdminService>();
        services.AddSingleton<ILicenseTokenSigner, TestLicenseTokenSigner>();
        services.AddScoped<ILicenseDeviceService, LicenseDeviceService>();
        return services;
    }

    public static IServiceCollection AddProjectServices(this IServiceCollection services)
    {
        services.AddSingleton<ProjectService.ReplayState>();
        services.AddScoped<IProjectService, ProjectService>();
        return services;
    }

    public static IServiceCollection AddCurriculumServices(this IServiceCollection services)
    {
        services.AddScoped<ICurriculumService, CurriculumService>();
        return services;
    }

    public static IServiceCollection AddAssessmentServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IAssessmentAuthority, UnconfiguredAssessmentAuthority>();
        services.AddScoped<QuestionBankService>();
        services.AddScoped<AssessmentReadService>();
        services.AddScoped<LearningSubmissionService>();
        services.AddScoped<TeacherRunService>();
        services.AddScoped<AttemptFinalizer>();
        services.AddScoped<LevelPromotionService>();
        services.AddScoped<ScoreExportService>();
        services.AddScoped<CriteriaApprovalService>();
        services.TryAddSingleton<IGradeDestination, UnconfiguredGradeDestination>();
        services.TryAddSingleton<IExportPrincipalResolver, UnconfiguredExportPrincipalResolver>();
        return services;
    }

    public static IServiceCollection AddManagementServices(this IServiceCollection services)
    {
        services.AddSingleton<ManagementService>();
        services.AddScoped<ReleaseService>();
        return services;
    }
}
