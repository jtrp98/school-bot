using System.Globalization;
using System.Text.Json;
using ServiceSchoolBot.Domain.Assessment;
using Grpc.Core;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using static ServiceSchoolBot.API.GrpcServices.AssessmentProtoMapper;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

// Follows AssessmentEndpoints: authenticate, then reject the body, then the idempotency key, then call
// the service; a version conflict reports the bank's current ETag.
public sealed class AssessmentGrpcService(
    SchoolBotGrpcRunner runner,
    QuestionBankService banks,
    AssessmentReadService reads,
    LearningSubmissionService submissions,
    TeacherRunService teachers,
    LevelPromotionService promotions,
    ScoreExportService exports,
    CriteriaApprovalService criteriaApproval,
    IAssessmentAuthResolver authResolver,
    IAssessmentRepository repository) : Rpc.AssessmentService.AssessmentServiceBase
{
    public override Task<Rpc.TeacherMutationResponse> ListLearningLevels(Rpc.ListLearningLevelsRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => TeacherResponse(new { items = await criteriaApproval.ListLevelsAsync(principal.Session, call.CancellationToken) }));
    public override Task<Rpc.TeacherMutationResponse> ApproveLearningCriteria(Rpc.ApproveLearningCriteriaRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await criteriaApproval.ApproveAsync(principal.Session, request.Id, request.ApprovalRef,
                request.IfMatch, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });
    public override Task<Rpc.TeacherMutationResponse> GetGradeMapping(Rpc.GradeRunRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => TeacherResponse(await exports.GetMappingAsync(principal, request.CourseRunId, call.CancellationToken)));
    public override Task<Rpc.TeacherMutationResponse> SetGradeMapping(Rpc.SetGradeMappingRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await exports.SetMappingAsync(principal, request.CourseRunId, request.CandidateKey,
                request.ActivityId, request.Reason, RequiredKey(request.IdempotencyKey), request.IfMatch, call.CancellationToken));
        });
    public override Task<Rpc.TeacherMutationResponse> RetireGradeMapping(Rpc.RetireGradeMappingRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await exports.RetireMappingAsync(principal, request.CourseRunId, request.Id,
                request.IfMatch, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });
    public override Task<Rpc.TeacherMutationResponse> CreateScorePreview(Rpc.CreateScorePreviewRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await exports.PreviewAsync(principal, request.CourseRunId, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });
    public override Task<Rpc.TeacherMutationResponse> GetScorePreview(Rpc.GradeIdRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => TeacherResponse(await exports.GetPreviewAsync(principal, request.Id, call.CancellationToken)));
    public override Task<Rpc.TeacherMutationResponse> ConfirmScoreExport(Rpc.ConfirmScoreExportRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await exports.ConfirmAsync(principal, request.PreviewId, request.SnapshotHash,
                request.IfMatch, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });
    public override Task<Rpc.TeacherMutationResponse> GetScoreExport(Rpc.GradeIdRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => TeacherResponse(await exports.GetExportAsync(principal, request.Id, call.CancellationToken)));
    public override Task<Rpc.TeacherMutationResponse> PreviewScoreResolution(Rpc.PreviewScoreResolutionRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await exports.PreviewResolutionAsync(principal, request.ExportId,
                request.Decisions.Select(d => new ScoreResolutionDecision(d.LearnerId, d.Action)).ToArray(),
                RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });
    public override Task<Rpc.TeacherMutationResponse> ProcessScoreExport(Rpc.ProcessScoreExportRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            _ = RequiredKey(request.IdempotencyKey);
            return TeacherResponse(new { completed = await exports.ProcessAsync(principal, request.Id, call.CancellationToken) });
        });
    public override Task<Rpc.TeacherMutationResponse> GetCourseRun(Rpc.GetCourseRunRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => TeacherResponse(await teachers.GetRunAsync(principal, request.Id, call.CancellationToken)));
    public override Task<Rpc.TeacherMutationResponse> CreateLevelProposal(Rpc.CreateLevelProposalRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await promotions.CreateAsync(principal, request.CourseRunId, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.TeacherMutationResponse> GetLevelProposal(Rpc.GetLevelProposalRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => TeacherResponse(await promotions.GetAsync(principal, request.Id, call.CancellationToken)));

    public override Task<Rpc.TeacherMutationResponse> ConfirmLevelProposal(Rpc.DecideLevelProposalRequest request, ServerCallContext context) =>
        DecideLevel(request, context, true);

    public override Task<Rpc.TeacherMutationResponse> DeclineLevelProposal(Rpc.DecideLevelProposalRequest request, ServerCallContext context) =>
        DecideLevel(request, context, false);

    private Task<Rpc.TeacherMutationResponse> DecideLevel(Rpc.DecideLevelProposalRequest request, ServerCallContext context, bool promote) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await promotions.DecideAsync(principal, request.Id, promote, request.SnapshotHash,
                request.IfMatch, request.HasReason ? request.Reason : null, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });
    public override Task<Rpc.ProgressResponse> GetProgress(Rpc.GetProgressRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
            ToProto(await reads.GetProgressAsync(principal, request.HasCourseRunId ? request.CourseRunId : null,
                request.HasCurriculumId ? request.CurriculumId : null, request.HasMode ? request.Mode : null,
                call.CancellationToken)));

    public override Task<Rpc.QuestionBankMutationResponse> CreateQuestionBank(
        Rpc.CreateQuestionBankRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            var body = new CreateQuestionBankRequest(Guid.Parse(request.BankKey), request.Revision,
                Json(request.HasTitleJson, request.TitleJson), ToDomain(request.Questions)!,
                request.HasSourceRef ? request.SourceRef : null!);
            return ToMutation(await banks.CreateQuestionBankAsync(principal, body, call.RequestId,
                RequestKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.QuestionBankMutationResponse> UpdateQuestionBank(
        Rpc.UpdateQuestionBankRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            RejectBody(request.Body);
            var body = new UpdateQuestionBankDraftRequest(Json(request.HasTitleJson, request.TitleJson),
                ToDomain(request.Questions)!, request.HasSourceRef ? request.SourceRef : null!);
            return ToMutation(await banks.UpdateQuestionBankDraftAsync(principal, request.Id, body, request.IfMatch,
                call.RequestId, RequestKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.QuestionBankMutationResponse> PublishQuestionBank(
        Rpc.PublishQuestionBankRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            RejectBody(request.Body);
            var body = new PublishQuestionBankRequest(request.HasApprovalRef ? request.ApprovalRef : null);
            return ToMutation(await banks.PublishQuestionBankAsync(principal, request.Id, body, request.IfMatch,
                call.RequestId, RequestKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.QuestionBankResponse> GetQuestionBank(
        Rpc.GetQuestionBankRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            var view = await banks.GetQuestionBankAsync(principal, request.Id, call.CancellationToken);
            return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
        });

    public override Task<Rpc.QuestionBankListResponse> ListQuestionBanks(
        Rpc.ListQuestionBanksRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
            ToProto(await banks.ListQuestionBanksAsync(principal, request.HasBankKey ? Guid.Parse(request.BankKey) : null,
                request.HasLifecycle ? request.Lifecycle : null, request.HasLimit ? request.Limit : 50,
                request.HasCursor ? request.Cursor : null, call.CancellationToken)));

    public override Task<Rpc.ActivityPolicyResponse> GetActivityPolicy(
        Rpc.GetActivityPolicyRequest request, ServerCallContext context) =>
        Run(context, request.CourseRunId,
            async (call, principal) =>
            {
                var view = await reads.GetActivityPolicyAsync(principal, request.CourseRunId, request.ActivityId,
                    call.CancellationToken);
                return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
            });

    public override Task<Rpc.AttemptResponse> GetAttempt(Rpc.GetAttemptRequest request, ServerCallContext context) =>
        Run(context, request.Id, async (call, principal) =>
        {
            var view = await reads.GetAttemptAsync(principal, request.Id, call.CancellationToken);
            return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
        });

    public override Task<Rpc.AttemptListResponse> ListAttempts(
        Rpc.ListAttemptsRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
            ToProto(await reads.ListAttemptsAsync(principal, request.CourseRunId,
                request.HasActivityId ? request.ActivityId : null, request.HasState ? request.State : null,
                request.HasLimit ? request.Limit : 50, request.HasCursor ? request.Cursor : null,
                call.CancellationToken)));

    // routeId is the {id} route value the HTTP filter read to report a version conflict.
    private Task<T> Run<T>(ServerCallContext context, int? routeId,
        Func<SchoolBotRpcCall, AssessmentPrincipalContext, Task<T>> body) =>
        runner.RunAsync(context, async call =>
        {
            try
            {
                var principal = await authResolver.ResolveAsync(call.Identity, call.CancellationToken);
                return await body(call, principal);
            }
            catch (DomainRuleException error) when (error.Code == ErrorCodes.VersionConflict && routeId is { } id)
            {
                var bank = await repository.GetQuestionBankByIdAsync(id, call.CancellationToken);
                throw new SchoolBotRpcFailure(StatusCodes.Status412PreconditionFailed, ErrorCodes.VersionConflict,
                    "Concurrency token mismatch.", OpaqueConcurrencyToken.ToETag(bank?.UpdateDate));
            }
            catch (System.Text.Json.JsonException)
            {
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid request body.");
            }
        });

    private static void RejectBody(Rpc.AssessmentBody body)
    {
        switch (body)
        {
            case Rpc.AssessmentBody.NotJson:
                throw new SchoolBotRpcFailure(415, "UNSUPPORTED_MEDIA_TYPE", "Content-Type must be application/json.");
            case Rpc.AssessmentBody.Malformed:
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid request body.");
            case Rpc.AssessmentBody.DuplicateProperty:
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Duplicate JSON property.");
            case Rpc.AssessmentBody.Null:
                throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "A request body is required.");
        }
    }

    // Header absent: no key; anything but one canonical non-empty UUID is rejected.
    private static Guid? RequestKey(IReadOnlyList<string> values)
    {
        if (values.Count == 0) return null;
        if (values.Count != 1 || values[0] is not { Length: 36 } text ||
            !Guid.TryParseExact(text, "D", out var key) || key == Guid.Empty)
            throw new SchoolBotRpcFailure(400, ErrorCodes.InvalidInput, "Invalid idempotency key.");
        return key;
    }

    private static Rpc.QuestionBankMutationResponse ToMutation(QuestionBankMutationResponse mutation)
    {
        var result = new Rpc.QuestionBankMutationResponse
        {
            Id = mutation.Id,
            UpdateDate = Time(mutation.UpdateDate),
            Etag = OpaqueConcurrencyToken.ToETag(mutation.UpdateDate)
        };
        if (Time(mutation.PublishedAt) is { } published) result.PublishedAt = published;
        return result;
    }

    public override Task<Rpc.AttemptResponse> StartAttempt(Rpc.StartAttemptRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            var view = await reads.StartAttemptAsync(principal, request.CourseRunId, request.ActivityId,
                RequiredKey(request.IdempotencyKey), call.CancellationToken);
            return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
        });

    public override Task<Rpc.AttemptResponse> SaveAttemptAnswers(
        Rpc.SaveAttemptAnswersRequest request, ServerCallContext context) => WriteAnswers(request, context, false);

    public override Task<Rpc.AttemptResponse> SubmitAttempt(
        Rpc.SaveAttemptAnswersRequest request, ServerCallContext context) => WriteAnswers(request, context, true);

    private Task<Rpc.AttemptResponse> WriteAnswers(Rpc.SaveAttemptAnswersRequest request,
        ServerCallContext context, bool submit) => Run(context, null, async (call, principal) =>
    {
        RejectBody(request.Body);
        var key = RequiredKey(request.IdempotencyKey);
        var answers = request.Answers.Select(answer =>
        {
            if (!Guid.TryParseExact(answer.QuestionKey, "D", out var questionKey))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid question key.");
            var options = answer.SelectedOptionKeys.Select(value =>
                Guid.TryParseExact(value, "D", out var option) ? option :
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid option key.")).ToArray();
            return new AssessmentAnswer(questionKey, options);
        }).ToArray();
        try
        {
            var view = await reads.SaveAttemptAsync(principal, request.Id, answers, submit,
                request.IfMatch, key, call.CancellationToken);
            return ToProto(view, OpaqueConcurrencyToken.ToETag(view.UpdateDate));
        }
        catch (DomainRuleException ex) when (ex.Code == ErrorCodes.VersionConflict)
        {
            var view = await reads.GetAttemptAsync(principal, request.Id, call.CancellationToken);
            throw new SchoolBotRpcFailure(412, ErrorCodes.VersionConflict, "Concurrency token mismatch.",
                OpaqueConcurrencyToken.ToETag(view.UpdateDate));
        }
    });

    private static Guid RequiredKey(IReadOnlyList<string> values) => RequestKey(values)
        ?? throw new DomainRuleException(ErrorCodes.InvalidInput, "Idempotency key is required.");


    public override Task<Rpc.LearningRunsResponse> ListLearningRuns(Rpc.GetLearningCurriculumRequest request,
        ServerCallContext context) => Run(context, null, async (call, principal) => new Rpc.LearningRunsResponse
        {
            RunsJson = (await submissions.GetRunsAsync(principal, request.Id, call.CancellationToken)).GetRawText()
        });
    public override Task<Rpc.LearningCurriculumResponse> GetLearningCurriculum(Rpc.GetLearningCurriculumRequest request,
        ServerCallContext context) => Run(context, null, async (call, principal) => new Rpc.LearningCurriculumResponse
        {
            CurriculumJson = (await submissions.GetCurriculumAsync(principal, request.Id, call.CancellationToken)).GetRawText()
        });
    public override Task<Rpc.SubmissionResponse> CreateSubmission(Rpc.CreateSubmissionRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return ToSubmission(await submissions.CreateAsync(principal,
                new(request.MissionId, request.CriteriaId, request.ProjectRevisionId,
                    request.HasEvidenceJson ? request.EvidenceJson : null), RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.SubmissionResponse> GetSubmission(Rpc.GetSubmissionRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => ToSubmission(await submissions.GetAsync(principal, request.Id, call.CancellationToken)));

    public override Task<Rpc.SubmissionListResponse> ListSubmissions(Rpc.ListSubmissionsRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            var response = new Rpc.SubmissionListResponse();
            response.Items.AddRange((await submissions.ListAsync(principal,
                request.HasLearnerId ? request.LearnerId : null, request.HasMissionId ? request.MissionId : null,
                request.HasLimit ? request.Limit : 50, call.CancellationToken)).Select(ToSubmission));
            return response;
        });

    public override Task<Rpc.SubmissionResponse> ReviewSubmission(Rpc.ReviewSubmissionRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            var results = request.RuleResults.Select(r => Guid.TryParseExact(r.RuleId, "D", out var id)
                ? new LearningRuleResult(id, r.Matched)
                : throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid rubric rule ID.")).ToArray();
            try
            {
                return ToSubmission(await submissions.ReviewAsync(principal, request.Id,
                    new(request.Outcome, request.Feedback, results, request.HasSupersedesId ? request.SupersedesId : null,
                        request.HasReason ? request.Reason : null), request.IfMatch, RequiredKey(request.IdempotencyKey), call.CancellationToken));
            }
            catch (DomainRuleException ex) when (ex.Code == ErrorCodes.VersionConflict)
            {
                var current = await submissions.GetAsync(principal, request.Id, call.CancellationToken);
                throw new SchoolBotRpcFailure(412, ex.Code, ex.Message, OpaqueConcurrencyToken.ToETag(current.Submission.UpdateDate));
            }
        });

    private static Rpc.SubmissionResponse ToSubmission(SubmissionView view)
    {
        var s = view.Submission;
        var result = new Rpc.SubmissionResponse
        {
            Id = s.Id, LearnerId = s.LearnerId, MissionId = s.MissionId, CriteriaId = s.CriteriaId,
            ProjectRevisionId = s.ProjectRevisionId, SubmittedAt = Time(s.SubmittedAt), State = s.State,
            UpdateDate = Time(s.UpdateDate), Etag = OpaqueConcurrencyToken.ToETag(s.UpdateDate)
        };
        if (view.Assessment is not null) result.AssessmentJson = JsonSerializer.Serialize(view.Assessment,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        if (view.ProjectContent is not null) result.ProjectContentJson = view.ProjectContent;
        if (view.ProjectMode is not null) result.ProjectMode = view.ProjectMode;
        if (s.Evidence is not null) result.EvidenceJson = s.Evidence;
        return result;
    }


    public override Task<Rpc.TeacherMutationResponse> GrantRetake(Rpc.GrantRetakeRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await teachers.GrantRetakeAsync(principal, request.CourseRunId, request.ActivityId,
                request.LearnerId, request.Reason, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.TeacherMutationResponse> UpdateActivityPolicy(Rpc.UpdateActivityPolicyRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await teachers.UpdateActivityPolicyAsync(principal, request.CourseRunId,
                request.ActivityId, request.HasTimeLimitSeconds ? request.TimeLimitSeconds : null,
                request.HasOpensAt ? TeacherTime(request.OpensAt) : null, request.HasClosesAt ? TeacherTime(request.ClosesAt) : null,
                request.IfMatch, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.TeacherMutationResponse> SetScoringPolicy(Rpc.SetScoringPolicyRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            var policy = JsonSerializer.Deserialize<ScoringPolicy>(request.PolicyJson)
                ?? throw new DomainRuleException(ErrorCodes.InvalidInput, "Scoring policy is required.");
            return TeacherResponse(await teachers.SetScoringPolicyAsync(principal, request.CourseRunId, policy,
                request.IfMatch, RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.TeacherMutationResponse> ActivateCourseRun(Rpc.ActivateCourseRunRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) =>
        {
            RejectBody(request.Body);
            return TeacherResponse(await teachers.ActivateAsync(principal, request.CourseRunId, request.IfMatch,
                RequiredKey(request.IdempotencyKey), call.CancellationToken));
        });

    public override Task<Rpc.TeacherMutationResponse> GetCourseRunTotal(Rpc.GetCourseRunTotalRequest request, ServerCallContext context) =>
        Run(context, null, async (call, principal) => TeacherResponse(await teachers.GetTotalAsync(principal,
            request.CourseRunId, request.LearnerId, call.CancellationToken)));

    private static Rpc.TeacherMutationResponse TeacherResponse<T>(T result)
    {
        var response = new Rpc.TeacherMutationResponse
        {
            ResultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower })
        };
        DateTimeOffset? updated = result switch
        {
            ServiceSchoolBot.Domain.Assessment.SchoolBotCourseRunRecord run => run.UpdateDate,
            ServiceSchoolBot.Domain.Assessment.SchoolBotCourseRunActivityRecord activity => activity.UpdateDate,
            ServiceSchoolBot.Domain.Assessment.SchoolBotLevelProposalRecord proposal => proposal.UpdateDate,
            ScorePreviewRecord preview => preview.UpdateDate,
            ScoreMappingRecord mapping => mapping.UpdateDate,
            ScoreExportRecord export => export.UpdateDate,
            ServiceSchoolBot.Domain.Curriculum.SchoolBotCurriculumRecord curriculum => curriculum.UpdateDate,
            _ => null
        };
        if (updated is not null) response.Etag = OpaqueConcurrencyToken.ToETag(updated);
        return response;
    }

    private static DateTimeOffset TeacherTime(string value) => DateTimeOffset.TryParse(value,
        CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) ? date :
        throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid assessment time.");

}

