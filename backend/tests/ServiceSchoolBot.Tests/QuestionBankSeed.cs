using System.Text.Json;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

internal static class QuestionBankSeed
{
    public static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-11-03T02:00:00Z");
    public static Guid Key(int value) => Guid.Parse("00000000-0000-4000-8000-" + value.ToString("D12"));
    public static JsonElement Text(string text) => JsonSerializer.SerializeToElement(new { th = text });
    public static AssessmentPolicy Policy(bool quiz) => quiz
        ? new(3, "latest", 90, "after_close", true, true)
        : new(2, "best", 120, "after_submit", false, false);
    public static QuestionBankQuestion[] Questions(bool revised) =>
    [
        new(Key(201), "single_choice", Text(revised ? "คำถามใหม่" : "คำถามเดิม"),
            [new(Key(211), Text(revised ? "A2" : "A")), new(Key(212), Text(revised ? "B2" : "B"))],
            [Key(revised ? 212 : 211)], revised ? "7" : "2"),
        new(Key(202), "boolean", Text("คำถามจริงเท็จ"),
            [new(Key(221), Text("จริง")), new(Key(222), Text("เท็จ"))],
            [Key(revised ? 222 : 221)], revised ? "11" : "3")
    ];

    public static CurriculumStoreSnapshot Build()
    {
        var now = Start;
        var bank = new SchoolBotQuestionBankRecord(1, Key(200), 1, Text("คลังเดิม").GetRawText(),
            Questions(false), "fixture:bank-096-v1", "published", now.AddDays(-1),
            false, 101, now.AddDays(-2), 103, now.AddDays(-1));
        var curriculum = new SchoolBotCurriculumRecord(1, Key(100), 1, 1, "curriculum", [], [],
            "fixture:curriculum", "published", now.AddDays(-1), false, 101, now, 103, now);
        var lessons = Enumerable.Range(1, 2).Select(id => new SchoolBotLessonRecord(id, 1, Key(110 + id), id,
            "lesson", "objective", "instructions", false, 101, now, 101, now)).ToArray();
        var activities = Enumerable.Range(1, 2).Select(id => new SchoolBotActivityRecord(id, id,
            Key(120 + id), 1, id == 1 ? "quiz" : "test", id == 1 ? "quiz" : "test", null, "[]",
            JsonSerializer.Serialize(new
            {
                kind = "question-v1", bank_id = 1, question_keys = new[] { Key(201), Key(202) },
                default_policy = Policy(id == 1)
            }), false, 101, now, 101, now)).ToArray();
        var run = new SchoolBotCourseRunRecord(1, 1, 401, 2569, "1", [501], null, null, now,
            "active", false, 201, now, 201, now, 11);
        var settings = Enumerable.Range(1, 2).Select(id => new SchoolBotCourseRunActivityRecord(id, 1, id,
            now.AddHours(-1), now.AddHours(1), null, false, 201, now, 201, now, 11)).ToArray();
        var snapshot = new AttemptSnapshot(1, 1, 1, 1, "fixture-evaluator-v1", Questions(false),
            [Key(202), Key(201)], [new(Key(201), [Key(212), Key(211)]), new(Key(202), [Key(221), Key(222)])],
            Policy(true), now.AddHours(-1), now.AddHours(1));
        var attempts = Enumerable.Range(1, 2).Select(id => new SchoolBotAttemptRecord(id, 1, 1, 301, id,
            id == 1 ? "submitted" : "started", now, now.AddSeconds(90), id == 1 ? now.AddSeconds(1) : null,
            JsonSerializer.Serialize(snapshot), id == 1 ? JsonSerializer.Serialize(new AssessmentAnswer[]
            {
                new(Key(201), [Key(211)]), new(Key(202), [Key(222)])
            }) : "[]", id == 1 ? 2m : null, 5m, false, 301, now, 301, now, 11)).ToArray();
        var audits = new SchoolBotAuditEventRecord[]
        {
            new(1, "1001", "QuestionBank.Created", "SchoolBotQuestionBank", 1, Key(901),
                "Question bank created", false, 101, now.AddDays(-2), 101, now.AddDays(-2)),
            new(2, "1003", "QuestionBank.Published", "SchoolBotQuestionBank", 1, Key(902),
                "fixture:approved-v1", false, 103, now.AddDays(-1), 103, now.AddDays(-1))
        };
        return new(1, [curriculum], lessons, [], [], activities, audits, [], [bank], [run], settings, attempts);
    }
}
