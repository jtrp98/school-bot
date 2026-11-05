using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Globalization;
using System.Text.Json;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed partial class QuestionBankService
{
    public async Task<QuestionBankDraftView> GetQuestionBankAsync(
        AssessmentPrincipalContext principal, int id, CancellationToken ct = default)
    {
        AuthorizeRead(principal);
        ValidateId(id);
        var bank = await dbAdapter.GetQuestionBankByIdAsync(id, ct)
            ?? throw new DomainRuleException(ErrorCodes.NotFound, "Question bank not found.");
        var audit = (await dbAdapter.ListAuditEventsByEntityAsync("SchoolBotQuestionBank", id, ct))
            .LastOrDefault(row => !row.IsDelete && row.Event == "QuestionBank.Published");
        return new(bank.SchoolBotQuestionBankId, bank.BankKey, bank.Revision,
            JsonSerializer.Deserialize<JsonElement>(bank.Title), bank.Questions, bank.SourceRef, bank.Lifecycle,
            bank.PublishedAt, bank.UpdateDate!.Value, audit is null ? null :
                new CurriculumApprovalView(audit.Actor, audit.CreateDate!.Value, audit.Reason));
    }

    public async Task<QuestionBankListResponse> ListQuestionBanksAsync(
        AssessmentPrincipalContext principal, Guid? bankKey = null, string? lifecycle = null,
        int limit = 50, string? cursor = null, CancellationToken ct = default)
    {
        AuthorizeRead(principal);
        var scope = JsonSerializer.Serialize(new { principal.Session.PrincipalId, principal.Session.SchoolId, bankKey, lifecycle });
        var offset = AssessmentPagination.Offset(limit, cursor, scope);
        var banks = await dbAdapter.ListQuestionBanksAsync(bankKey, lifecycle, ct);
        var items = banks.Skip(offset).Take(limit).Select(bank => new QuestionBankSummary(
            bank.SchoolBotQuestionBankId, bank.BankKey, bank.Revision, JsonSerializer.Deserialize<JsonElement>(bank.Title),
            bank.SourceRef, bank.Lifecycle, bank.PublishedAt, bank.UpdateDate!.Value)).ToArray();
        return new(items, AssessmentPagination.Next(offset, items.Length, banks.Count, scope));
    }

    private void AuthorizeRead(AssessmentPrincipalContext principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var access = CurriculumAuthorizationService.EvaluateReadCurriculum(principal.Session, false, clock.GetUtcNow());
        if (!access.IsAllowed) throw new DomainRuleException(access.ErrorCode!, access.Message!);
    }
}

internal static class AssessmentPagination
{
    public static int Offset(int limit, string? cursor, string scope)
    {
        if (limit is < 1 or > 100)
            throw new DomainRuleException(ErrorCodes.InvalidQuery, "Invalid page limit.");
        if (cursor is null) return 0;
        try
        {
            var text = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = text.Split(':');
            if (parts.Length == 2 && parts[0] == ScopeHash(scope) &&
                int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var offset) && offset >= 0)
                return offset;
        }
        catch (FormatException) { }
        throw new DomainRuleException(ErrorCodes.InvalidQuery, "Invalid cursor.");
    }

    public static string? Next(int offset, int count, int total, string scope) => offset + count < total
        ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            ScopeHash(scope) + ":" + (offset + count).ToString(CultureInfo.InvariantCulture)))
        : null;

    private static string ScopeHash(string scope) => Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(scope)));
}
