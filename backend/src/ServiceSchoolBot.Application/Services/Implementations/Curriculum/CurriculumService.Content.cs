using ServiceSchoolBot.Application.Common.Model.Curriculum;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    public async Task<CurriculumMutationResponse> UpdateContentAsync(
        CurriculumPrincipalContext principal,
        int id,
        UpdateCurriculumContentRequest request,
        string? expectedETag,
        Guid? requestId = null,
        Guid? idempotencyKey = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        var now = clock.GetUtcNow();
        var authCheck = CurriculumAuthorizationService.EvaluateUpdateCurriculumContent(principal, now);
        if (!authCheck.IsAllowed)
            throw new DomainRuleException(authCheck.ErrorCode!, authCheck.Message!);

        if (string.IsNullOrWhiteSpace(expectedETag))
            throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match header is required.");

        ValidateContentInput(request);

        var operation = "update_curriculum_content";
        var requestHash = ComputeSha256($"{id}|{JsonSerializer.Serialize(request)}");

        return await CommitMutationAsync(principal, operation, requestHash, idempotencyKey, 200,
            async (transaction, commitNow, token) =>
            {
                var lessons = new List<SchoolBotLessonRecord>();
                var missions = new List<SchoolBotMissionRecord>();
                var criteria = new List<SchoolBotCriteriaRecord>();
                var activities = new List<SchoolBotActivityRecord>();

                int tempLessonId = 1;
                int tempMissionId = 1;

                foreach (var l in request.Lessons)
                {
                    var currentLessonId = tempLessonId++;
                    lessons.Add(new SchoolBotLessonRecord(
                        SchoolBotLessonId: currentLessonId,
                        CurriculumId: id,
                        LessonKey: l.LessonKey,
                        Ordinal: l.Ordinal,
                        Title: l.Title,
                        Objective: l.Objective,
                        Instructions: l.Instructions,
                        IsDelete: false,
                        CreateBy: null,
                        CreateDate: commitNow,
                        UpdateBy: null,
                        UpdateDate: commitNow,
                        SchoolId: null));

                    if (l.Missions is not null)
                    {
                        foreach (var m in l.Missions)
                        {
                            var currentMissionId = tempMissionId++;
                            missions.Add(new SchoolBotMissionRecord(
                                SchoolBotMissionId: currentMissionId,
                                LessonId: currentLessonId,
                                MissionKey: m.MissionKey,
                                Ordinal: m.Ordinal,
                                Title: m.Title,
                                Instructions: m.Instructions,
                                Method: m.Method,
                                IsDelete: false,
                                CreateBy: null,
                                CreateDate: commitNow,
                                UpdateBy: null,
                                UpdateDate: commitNow,
                                SchoolId: null));

                            if (m.Criteria is not null)
                            {
                                criteria.Add(new SchoolBotCriteriaRecord(
                                    SchoolBotCriteriaId: 0,
                                    MissionId: currentMissionId,
                                    Revision: m.Criteria.Revision,
                                    Rules: m.Criteria.Rules,
                                    SourceRef: m.Criteria.SourceRef,
                                    ApprovedBy: m.Criteria.ApprovedBy,
                                    ApprovedAt: m.Criteria.ApprovedAt,
                                    IsDelete: false,
                                    CreateBy: null,
                                    CreateDate: commitNow,
                                    UpdateBy: null,
                                    UpdateDate: commitNow,
                                    SchoolId: null));
                            }
                        }
                    }

                    if (l.Activities is not null)
                    {
                        foreach (var a in l.Activities)
                        {
                            activities.Add(new SchoolBotActivityRecord(
                                SchoolBotActivityId: 0,
                                LessonId: currentLessonId,
                                ActivityKey: a.ActivityKey,
                                Ordinal: a.Ordinal,
                                Title: a.Title,
                                Type: a.Type,
                                Mode: a.Mode,
                                Indicators: a.Indicators ?? "[]",
                                Config: a.Config ?? "{}",
                                IsDelete: false,
                                CreateBy: null,
                                CreateDate: commitNow,
                                UpdateBy: null,
                                UpdateDate: commitNow,
                                SchoolId: null));
                        }
                    }
                }

                var audit = new SchoolBotAuditEventRecord(
                    SchoolBotAuditEventId: 0,
                    Actor: principal.PrincipalId!,
                    Event: "Curriculum.ContentUpdated",
                    EntityType: "SchoolBotCurriculum",
                    EntityId: id,
                    RequestId: requestId ?? Guid.NewGuid(),
                    Reason: "Curriculum content updated",
                    IsDelete: false,
                    CreateBy: null,
                    CreateDate: commitNow,
                    UpdateBy: null,
                    UpdateDate: commitNow,
                    SchoolId: null
                );

                var updated = await transaction.UpdateCurriculumContentAsync(
                    id,
                    lessons,
                    missions,
                    criteria,
                    activities,
                    expectedETag,
                    audit,
                    token);

                var response = new CurriculumMutationResponse(updated.SchoolBotCurriculumId, updated.UpdateDate!.Value);
                return response;
            }, ct);
    }

}
