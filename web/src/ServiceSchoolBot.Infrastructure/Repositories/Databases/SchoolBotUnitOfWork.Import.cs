using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class SchoolBotUnitOfWork
{
    public Task<IdempotentCommitResult<ProjectCommitResult>> CommitGuestImportAsync(
        IdempotencyCommand idempotency,
        CommitProjectRequest request,
        SchoolBotAuditEventRecord audit,
        CancellationToken ct = default) =>
        Project.CommitGuestImportAsync(idempotency, request, audit, ct);
}
