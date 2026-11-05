using ServiceSchoolBot.Application.Common.Model.Curriculum;
namespace ServiceSchoolBot.Application.Services.Interfaces.Curriculum;

public interface ICurriculumService
{
    Task<CurriculumMutationResponse> CreateCurriculumAsync(
        CurriculumPrincipalContext principal,
        CreateCurriculumRequest request,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default);

    Task<CurriculumMutationResponse> UpdateMetadataAsync(
        CurriculumPrincipalContext principal,
        int id,
        CurriculumMetadataInput request,
        string? expectedETag,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default);

    Task<CurriculumMutationResponse> UpdateContentAsync(
        CurriculumPrincipalContext principal,
        int id,
        UpdateCurriculumContentRequest request,
        string? expectedETag,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default);

    Task<CurriculumImportResponse> ImportCurriculumLessonsAsync(
        CurriculumPrincipalContext principal,
        int id,
        CurriculumImportRequest request,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default);

    Task<CurriculumMutationResponse> PublishCurriculumAsync(
        CurriculumPrincipalContext principal,
        int id,
        PublishCurriculumRequest request,
        string? expectedETag,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default);

    Task<CurriculumAdminView> GetDraftViewAsync(
        CurriculumPrincipalContext principal,
        int id,
        CancellationToken ct = default);

    Task<CurriculumDraftView> GetPublishedViewAsync(
        CurriculumPrincipalContext principal,
        int id,
        CancellationToken ct = default);

    Task<CurriculumListResponse> ListPublishedCurriculaAsync(
        CurriculumPrincipalContext principal,
        int? levelId = null,
        string? recommendedGrade = null,
        Guid? subjectKey = null,
        string? mode = null,
        int limit = 50,
        string? cursor = null,
        CancellationToken ct = default);

    Task<CurriculumListResponse> ListAdminCurriculaAsync(
        CurriculumPrincipalContext principal,
        string? lifecycle = null,
        int? levelId = null,
        int limit = 50,
        string? cursor = null,
        CancellationToken ct = default);
}
