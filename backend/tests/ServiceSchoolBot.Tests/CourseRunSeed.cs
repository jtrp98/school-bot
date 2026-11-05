using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

internal static class CourseRunSeed
{
    internal const string StudentToken = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    internal static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-11-03T00:00:00Z");
    internal static Guid Key(int id) => QuestionBankSeed.Key(id);
    private static string TokenHash => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(StudentToken)));

    internal static CurriculumStoreSnapshot Build(string deviceJkt)
    {
        var curriculum = new SchoolBotCurriculumRecord(1, Key(100), 1, 1, "Beginner", ["ป.1"],
            [new(Key(101), "robotics", "Robotics")], "fixture:course", "published",
            Now.AddDays(-1), false, 1, Now, 2, Now);
        var lesson = new SchoolBotLessonRecord(1, 1, Key(110), 1, "Lesson 1", "objective",
            "instructions", false, 1, Now, 2, Now);
        var missionModes = new[] { "block", "arm", "code" };
        var missions = missionModes.Select((mode, index) => new SchoolBotMissionRecord(index + 1, 1,
            Key(201 + index), index + 1, mode, "mission instructions",
            mode == "arm" ? "teacher" : "automatic", false, 1, Now, 2, Now)).ToArray();
        var criteria = missions.Select(mission => new SchoolBotCriteriaRecord(mission.SchoolBotMissionId,
            mission.SchoolBotMissionId, 1, CriteriaRules(mission.Method),
            "fixture:criteria", "approver", Now.AddDays(-1), false, 1, Now, 2, Now)).ToArray();
        var names = new[] { "slide", "lesson plan", "exercise", "block", "arm", "code", "quiz", "test" };
        var activities = names.Select((name, index) =>
        {
            var project = index is >= 3 and <= 5;
            var type = "instruction";
            if (project) type = "project";
            else if (index >= 6) type = name;
            return new SchoolBotActivityRecord(index + 1, 1, Key(301 + index), index + 1, name,
                type, project ? name : null, "[]", ActivityConfig(name, index), false, 1, Now, 2, Now);
        }).ToArray();
        var run = new SchoolBotCourseRunRecord(1, 1, 401, 101, "term-1", [501], null, null,
            Now.AddDays(-1), "active", false, 2, Now, 2, Now, 11);
        var settings = activities.Select(activity => new SchoolBotCourseRunActivityRecord(
            activity.SchoolBotActivityId, 1, activity.SchoolBotActivityId, Now.AddHours(-1),
            Now.AddHours(1), null, false, 2, Now, 2, Now, 11)).ToArray();
        var session = new SchoolBotSessionRecord(1, "desktop", 9001, TokenHash,
            "student", 301, null, deviceJkt, null, Now.AddHours(8), null, false, 301, Now, 301, Now, 11);
        var bank = QuestionBankSeed.Build().QuestionBanks!;
        return new(1, [curriculum], [lesson], missions, criteria, activities, [], [],
            bank, [run], settings, [], [session]);
    }

    private static string CriteriaRules(string method)
    {
        if (method == "teacher")
            return JsonSerializer.Serialize(new
            {
                kind = "teacher-v1",
                items = new[] { new { id = Key(701), instruction = "check" } }
            });
        return JsonSerializer.Serialize(new
        {
            kind = "automatic-v1",
            combine = "all",
            rules = new[] { new { id = Key(702), type = "uses_op", op = "jump", minimum = 1 } }
        });
    }

    private static string ActivityConfig(string name, int index)
    {
        if (index is >= 3 and <= 5)
            return JsonSerializer.Serialize(new
            {
                kind = "project-v1",
                mission_id = index - 2
            });
        if (index < 3)
            return JsonSerializer.Serialize(new
            {
                kind = "instruction-v1",
                instructions = name,
                media_refs = new[] { "fixture:" + name }
            });
        return JsonSerializer.Serialize(new
        {
            kind = "question-v1",
            bank_id = 1,
            question_keys = new[] { Key(201), Key(202) },
            default_policy = QuestionBankSeed.Policy(index == 6)
        });
    }
}
