using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Application.Services.Implementations.Projects;

public sealed partial class ProjectService
{
    public static void ValidateStoredCommitRequest(CommitProjectRequest request)
    {
        if (request.SchoolId <= 0)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "SchoolId must be greater than zero.");
        if (request.OwnerId <= 0)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "OwnerId must be greater than zero.");
        if (!string.Equals(request.OwnerKind, "student", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.OwnerKind, "employee", StringComparison.OrdinalIgnoreCase))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "OwnerKind must be 'student' or 'employee'.");
        if (!string.Equals(request.Mode, "block", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Mode, "arm", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Mode, "code", StringComparison.OrdinalIgnoreCase))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Mode must be 'block', 'arm', or 'code'.");
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 200)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Name must be between 1 and 200 characters.");
        if (request.Content is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Content cannot be null.");

    }

    public static void ValidateStoredProjectIdentity(
        SchoolBotProjectRecord existing, CommitProjectRequest request)
    {
        if (existing.SchoolId != request.SchoolId ||
            !string.Equals(existing.OwnerKind, request.OwnerKind, StringComparison.OrdinalIgnoreCase) ||
            existing.OwnerId != request.OwnerId ||
            !string.Equals(existing.Mode, request.Mode, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Project tenant, owner, or mode cannot be changed.");
        }

    }
}
