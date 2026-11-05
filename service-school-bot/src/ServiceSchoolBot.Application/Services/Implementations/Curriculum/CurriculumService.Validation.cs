using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    private static void ValidateCreateRequest(CreateCurriculumRequest request)
    {
        if (request.CurriculumKey == Guid.Empty)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "curriculum_key must not be empty.");
        if (request.Revision < 1)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "revision must be at least 1.");
        if (request.LevelId < 1)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "level_id must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "title must not be empty.");
        if (string.IsNullOrWhiteSpace(request.SourceRef) || request.SourceRef.Length > MaxSourceRefLength)
            throw new DomainRuleException(ErrorCodes.InvalidInput, $"source_ref must be between 1 and {MaxSourceRefLength} characters.");
        if (request.RecommendedGrades is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "recommended_grades cannot be null.");
        if (request.Subjects is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "subjects cannot be null.");
    }

    private static void ValidateMetadataInput(CurriculumMetadataInput request)
    {
        if (request.LevelId < 1)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "level_id must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "title must not be empty.");
        if (string.IsNullOrWhiteSpace(request.SourceRef) || request.SourceRef.Length > MaxSourceRefLength)
            throw new DomainRuleException(ErrorCodes.InvalidInput, $"source_ref must be between 1 and {MaxSourceRefLength} characters.");
        if (request.RecommendedGrades is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "recommended_grades cannot be null.");
        if (request.Subjects is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "subjects cannot be null.");
    }

    private static void ValidateContentInput(UpdateCurriculumContentRequest request)
    {
        if (request.Lessons is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "lessons cannot be null.");

        foreach (var l in request.Lessons)
        {
            if (l.LessonKey == Guid.Empty)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "lesson_key must not be empty.");
            if (l.Ordinal < 1)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "ordinal must be at least 1.");
            if (string.IsNullOrWhiteSpace(l.Title))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "lesson title must not be empty.");
            if (string.IsNullOrWhiteSpace(l.Objective))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "lesson objective must not be empty.");
            if (string.IsNullOrWhiteSpace(l.Instructions))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "lesson instructions must not be empty.");

            if (l.Missions is not null)
            {
                foreach (var m in l.Missions)
                {
                    if (m.MissionKey == Guid.Empty)
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "mission_key must not be empty.");
                    if (m.Ordinal < 1)
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "mission ordinal must be at least 1.");
                    if (string.IsNullOrWhiteSpace(m.Title))
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "mission title must not be empty.");
                    if (string.IsNullOrWhiteSpace(m.Instructions))
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "mission instructions must not be empty.");
                    if (string.IsNullOrWhiteSpace(m.Method))
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "mission method must not be empty.");
                    if (m.Criteria is not null)
                    {
                        if (m.Criteria.Revision < 1)
                            throw new DomainRuleException(ErrorCodes.InvalidInput, "criteria revision must be at least 1.");
                        if (string.IsNullOrWhiteSpace(m.Criteria.Rules))
                            throw new DomainRuleException(ErrorCodes.InvalidInput, "criteria rules must not be empty.");
                    }
                }
            }

            if (l.Activities is not null)
            {
                foreach (var a in l.Activities)
                {
                    if (a.ActivityKey == Guid.Empty)
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "activity_key must not be empty.");
                    if (a.Ordinal < 1)
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "activity ordinal must be at least 1.");
                    if (string.IsNullOrWhiteSpace(a.Title))
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "activity title must not be empty.");
                    if (string.IsNullOrWhiteSpace(a.Type))
                        throw new DomainRuleException(ErrorCodes.InvalidInput, "activity type must not be empty.");
                }
            }
        }
    }
}
