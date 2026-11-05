using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    public static void ValidateStoredMetadata(
        SchoolBotCurriculumRecord existing, string expectedETag)
    {
        if (!string.Equals(existing.Lifecycle, "draft", StringComparison.OrdinalIgnoreCase))
            throw new DomainRuleException(ErrorCodes.StateConflict, "Cannot modify metadata of a published curriculum.");

        if (!OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
            throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

    }

    public static void ValidateStoredContent(
        SchoolBotCurriculumRecord existing, string expectedETag)
    {
        if (!string.Equals(existing.Lifecycle, "draft", StringComparison.OrdinalIgnoreCase))
            throw new DomainRuleException(ErrorCodes.StateConflict, "Cannot modify content of a published curriculum.");

        if (!OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
            throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

    }

    public static bool IsStoredPublicationReplay(
        SchoolBotCurriculumRecord existing, IReadOnlyList<SchoolBotAuditEventRecord> auditEvents,
        string approvalRef, string expectedETag, int curriculumId)
    {
        if (string.Equals(existing.Lifecycle, "published", StringComparison.OrdinalIgnoreCase))
        {
            var existingPublishEvent = auditEvents.FirstOrDefault(a =>
                a.EntityId == curriculumId &&
                string.Equals(a.EntityType, "SchoolBotCurriculum", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(a.Event, "Curriculum.Published", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(a.Reason, approvalRef, StringComparison.Ordinal));

            if (existingPublishEvent is not null)
            {
                return true;
            }

            throw new DomainRuleException(ErrorCodes.StateConflict, "Curriculum is already published.");
        }

        if (!string.Equals(existing.Lifecycle, "draft", StringComparison.OrdinalIgnoreCase))
            throw new DomainRuleException(ErrorCodes.StateConflict, "Curriculum must be in draft state to publish.");

        if (!OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
            throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");

        return false;
    }
}
