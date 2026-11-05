using System.Text.Json;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;

internal static class CurriculumChecks
{
    public static async Task RunAsync(SchoolBotUnitOfWork repository, SchoolBotCurriculumRecord curriculum)
    {
        var now = DateTimeOffset.UtcNow;
        var lessonKey = Guid.NewGuid();
        var changed = await repository.UpdateCurriculumContentAsync(curriculum.SchoolBotCurriculumId,
            [new(1, curriculum.SchoolBotCurriculumId, lessonKey, 1, "{}", "{}", "{}", false, 7, now, 7, now)],
            [new(1, 1, Guid.NewGuid(), 1, "{}", "{}", "{}", false, 7, now, 7, now)],
            [new(0, 1, 1, "{}", "self-check", null, null, false, 7, now, 7, now)],
            [new(0, 1, Guid.NewGuid(), 1, "{}", "quiz", null, "[]", "{}", false, 7, now, 7, now)],
            OpaqueConcurrencyToken.ToETag(curriculum.UpdateDate), Audit("Curriculum.Updated", "SchoolBotCurriculum", 7));
        var lessons = await repository.ListLessonsByCurriculumIdAsync(curriculum.SchoolBotCurriculumId);
        Check.That(lessons.Count == 1 && lessons[0].LessonKey == lessonKey, "Curriculum content did not persist.");
        var missions = await repository.ListMissionsByLessonIdAsync(lessons[0].SchoolBotLessonId);
        Check.That(missions.Count == 1 && (await repository.ListCriteriaByMissionIdAsync(missions[0].SchoolBotMissionId)).Count == 1,
            "Generated mission/criteria identities were not linked correctly.");
        Check.That((await repository.ListActivitiesByLessonIdAsync(lessons[0].SchoolBotLessonId)).Count == 1,
            "Activity did not reference the generated lesson identity.");
        Check.That(changed.UpdateDate > curriculum.UpdateDate, "Content ETag did not advance.");

        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var text = JsonSerializer.SerializeToElement(new { th = "Test" });
        QuestionBankQuestion[] questions = [new(Guid.NewGuid(), "single_choice", text,
            [new(first, text), new(second, text)], [first], "1")];
        var bank = new SchoolBotQuestionBankRecord(0, Guid.NewGuid(), 1, "{\"th\":\"Bank\"}", questions,
            "self-check", "draft", null, false, 7, now, 7, now);
        var command = new IdempotencyCommand(null, "staff_admin:7", "bank-self-check", Guid.NewGuid(), new string('c', 64));
        var created = await repository.CommitWithIdempotencyAsync(command, (_, _, _, _) => Task.CompletedTask,
            async (scope, _, ct) =>
            {
                var result = await scope.CreateQuestionBankAsync(bank, Audit("QuestionBank.Created", "SchoolBotQuestionBank", 7), ct);
                return new MutationResult<SchoolBotQuestionBankRecord>(result, 201, result.SchoolBotQuestionBankId,
                    OpaqueConcurrencyToken.ToETag(result.UpdateDate));
            });
        var published = await repository.CommitWithIdempotencyAsync(command with { RequestKey = Guid.NewGuid(), Principal = "staff_admin:8" },
            (_, _, _, _) => Task.CompletedTask, async (scope, _, ct) =>
            {
                var result = await scope.PublishQuestionBankAsync(created.Result.Body.SchoolBotQuestionBankId,
                    created.Result.ETag!, Audit("QuestionBank.Published", "SchoolBotQuestionBank", 8), ct);
                return new MutationResult<SchoolBotQuestionBankRecord>(result, 200, result.SchoolBotQuestionBankId,
                    OpaqueConcurrencyToken.ToETag(result.UpdateDate));
            });
        Check.That(published.Result.Body.Lifecycle == "published" && published.Result.Body.Questions.Count == 1,
            "Question bank JSON or publication did not persist.");
        Check.That((await repository.ListAuditEventsByEntityAsync("SchoolBotQuestionBank", published.Result.Body.SchoolBotQuestionBankId)).Count == 2,
            "Question bank audit was not committed with its mutations.");
        Console.WriteLine("PASS: curriculum hierarchy identity mapping and question bank publication/audit/idempotency.");
    }

    private static SchoolBotAuditEventRecord Audit(string action, string entity, int actor) =>
        new(0, "staff_admin:" + actor, action, entity, 0, Guid.NewGuid(), "self-check", false, actor, null, actor, null);
}
