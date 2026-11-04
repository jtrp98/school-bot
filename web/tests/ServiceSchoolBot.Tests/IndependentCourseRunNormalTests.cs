using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentCourseRunNormalTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-11-03T00:00:00Z");

    private const string Token = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCA";
    private static readonly string[] Titles =
        ["สไลด์", "แผนการสอน", "ใบงาน", "สร้างด้วย Block", "ฝึกแขนกล", "เขียนโปรแกรม", "แบบฝึก Quiz", "ข้อสอบ"];
    private static readonly string[] Types =
        ["instruction", "instruction", "instruction", "project", "project", "project", "quiz", "test"];
    private static readonly string?[] Modes = [null, null, null, "block", "arm", "code", null, null];

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GradeFourStudentReadsSelectedBeginnerAndOpensEightActivitiesWithoutWriting(bool file)
    {
        using var fixture = new IndependentRunHost(file);
        await fixture.Seed();
        var curricula = GrpcTest.Curricula(fixture);
        var assessments = GrpcTest.Assessments(fixture);
        var student = await GatewayCaller.Desktop(fixture, new SchoolBotUnitOfWork(fixture.Store(), fixture.Clock),
            11, "student", 301);
        var before = JsonSerializer.Serialize(await fixture.Snapshot());
        var licenses = JsonSerializer.Serialize(await fixture.Store().LoadLicensesAsync());
        var bindings = JsonSerializer.Serialize(await fixture.Store().LoadBindingsAsync());
        var diskBefore = file ? await File.ReadAllBytesAsync(fixture.SnapshotPath) : null;
        var robot = new FakeRobot();
        for (var service = 0; service < 2; service++)
        {
            var course = CurriculumRpc.ToModel(
                await curricula.GetPublishedCurriculumAsync(new() { Id = 71 }, GrpcTest.Headers(student)));
            Assert.Equal(71, course.Id);
            Assert.Equal(1, course.LevelId);
            Assert.Equal("Beginner", course.Title);
            Assert.Equal(2, course.Revision);
            Assert.Equal(["ป.1"], course.RecommendedGrades);
            Assert.Equal(new CurriculumSubject(Key(80), "computing", "วิทยาการคำนวณ"), Assert.Single(course.Subjects));
            var lesson = Assert.Single(course.Lessons);
            Assert.Equal(Key(81), lesson.LessonKey);
            Assert.Equal("เริ่มต้นกับหุ่นยนต์", lesson.Title);
            Assert.Equal(Titles, lesson.Activities.Select(activity => activity.Title));
            Assert.Equal(Types, lesson.Activities.Select(activity => activity.Type));
            Assert.Equal(Modes, lesson.Activities.Select(activity => activity.Mode));
            Assert.Equal(Enumerable.Range(1, 8), lesson.Activities.Select(activity => activity.Ordinal));
            Assert.Equal(new[] { "automatic", "teacher", "automatic" }, lesson.Missions.Select(mission => mission.Method));
            for (var index = 0; index < 8; index++)
            {
                var activity = lesson.Activities[index];
                using var config = JsonDocument.Parse(activity.Config!);
                var kind = config.RootElement.GetProperty("kind").GetString();
                if (index < 3)
                {
                    Assert.Equal("instruction-v1", kind);
                    Assert.Equal(Titles[index], config.RootElement.GetProperty("instructions").GetString());
                    Assert.Single(config.RootElement.GetProperty("media_refs").EnumerateArray());
                }
                else if (index < 6)
                {
                    Assert.Equal("project-v1", kind);
                    Assert.Equal(91 + index - 3, config.RootElement.GetProperty("mission_id").GetInt32());
                    Assert.Equal(Key(91 + index - 3), lesson.Missions[index - 3].MissionKey);
                    robot.Open(activity.Mode!);
                }
                else
                {
                    Assert.Equal("question-v1", kind);
                    await assessments.GetActivityPolicyAsync(new() { CourseRunId = 72, ActivityId = 101 + index },
                        GrpcTest.Headers(student));
                }
            }
            var progress = AssessmentRpc.ToModel(
                await assessments.GetProgressAsync(new() { CourseRunId = 72 }, GrpcTest.Headers(student)));
            Assert.Equal(301, progress.LearnerId);
            Assert.Equal(72, progress.CourseRunId);
            Assert.Equal(71, progress.CurriculumId);
            Assert.Equal(1, progress.LevelId);
            Assert.Equal(Now, progress.ComputedAt);
            Assert.Equal(Enumerable.Range(101, 8), progress.Activities.Select(activity => activity.ActivityId));
            Assert.Equal(Types, progress.Activities.Select(activity => activity.Type));
            Assert.Equal(Modes, progress.Activities.Select(activity => activity.Mode));
            Assert.Equal(new[] { 91, 92, 93 }, progress.Missions.Select(mission => mission.MissionId));
            Assert.All(progress.Activities, activity =>
            {
                Assert.Null(activity.LatestSubmission);
                Assert.Null(activity.SelectedAttempt);
            });
            Assert.All(progress.Missions, mission => Assert.Null(mission.LatestSubmission));
        }
        Assert.Equal(new[] { "block", "arm", "code", "block", "arm", "code" }, robot.Modes);
        Assert.Equal(4, fixture.ClassroomGrade);
        Assert.Equal(2, fixture.CurriculumReads);
        Assert.Equal(6, fixture.ProgressReads);
        Assert.True(fixture.AuthorityReads >= 6);
        var saved = await fixture.Snapshot();
        Assert.Equal(before, JsonSerializer.Serialize(saved));
        Assert.Null(Assert.Single(saved.Curricula).SchoolId);
        var run = Assert.Single(saved.CourseRuns!);
        Assert.Equal((11, 71, 401, 101, "term-1"),
            (run.SchoolId, run.CurriculumId, run.ClassroomId, run.NYear, run.NTerm));
        Assert.Equal([501], run.PlanCourseIds);
        Assert.Equal(licenses, JsonSerializer.Serialize(await fixture.Store().LoadLicensesAsync()));
        Assert.Equal(bindings, JsonSerializer.Serialize(await fixture.Store().LoadBindingsAsync()));
        Assert.Equal(8, saved.CourseRunActivities!.Count);
        Assert.Empty(saved.Attempts!);
        Assert.Empty(saved.AuditEvents);
        Assert.Empty(saved.Idempotency!);
        if (file) Assert.Equal(diskBefore, await File.ReadAllBytesAsync(fixture.SnapshotPath));
        var reader = new SchoolBotUnitOfWork(fixture.Store(), fixture.Clock);
        Assert.Equal(1, (await reader.GetCurriculumByIdAsync(71))!.LevelId);
        Assert.Equal(71, (await reader.GetCourseRunByIdAsync(72))!.CurriculumId);
    }

    private static string TokenHash => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Token)));

    private static Guid Key(int number) => Guid.Parse($"12345678-1234-4123-8123-{number:D12}");

    private sealed class FakeRobot
    {
        internal List<string> Modes { get; } = [];
        internal void Open(string mode) => Modes.Add(mode);
    }

    private sealed class IndependentRunHost(bool file) : WebApplicationFactory<Program>, IAssessmentAuthority
    {
        private readonly string directory = Path.Combine(AppContext.BaseDirectory,
            "IndependentCourseRunNormal-" + Guid.NewGuid().ToString("N"));
        private readonly MemorySchoolBotBackingStore memory = new();
        private readonly SchoolProfileRepository school = new();
        private SchoolBotUnitOfWork[] adapters = [];
        private CurriculumService[] curricula = [];
        private AssessmentReadService[] progress = [];
        internal int CurriculumReads;
        internal int ProgressReads;
        internal int AuthorityReads;
        internal int ClassroomGrade => 4;
        internal FakeClock Clock { get; } = new(Now);
        internal string SnapshotPath => Path.Combine(directory, "curricula_snapshot.json");
        internal ISchoolBotBackingStore Store() => file ? new FileSchoolBotBackingStore(directory) : memory;
        internal Task<CurriculumStoreSnapshot> Snapshot() => ((ICurriculumBackingStore)Store()).LoadCurriculumUnitAsync();

        internal async Task Seed()
        {
            using var signer = new TestLicenseTokenSigner();
            using var deviceA = CreateDeviceKey().Key;
            using var deviceB = CreateDeviceKey().Key;
            Assert.False(deviceA.ExportSubjectPublicKeyInfo().SequenceEqual(deviceB.ExportSubjectPublicKeyInfo()));
            foreach (var key in new[] { deviceA, deviceB })
            {
                var parameters = key.ExportParameters(false);
                var jwk = new DeviceJwk("EC", "P-256", TestLicenseTokenSigner.Base64UrlEncode(parameters.Q.X!),
                    TestLicenseTokenSigner.Base64UrlEncode(parameters.Q.Y!));
                var proof = CreateProof(key, "normal-challenge", "GET", "/v2/progress", null, "{}");
                DeviceProofVerifier.VerifyDeviceProof(proof, jwk, "normal-challenge", "GET", "/v2/progress", null,
                    DeviceProofVerifier.ComputeBodyHashWithoutProof("{}"));
            }
            var signed = signer.Sign("test", new { school_id = 11, exp = Now.AddDays(10).ToUnixTimeSeconds() });
            Assert.True(signer.Verify(signed, out var header, out var payload));
            header!.Dispose();
            payload!.Dispose();
            var principal = LicenseAdminPrincipalStub.CreateLicenseAdmin("P1", validFrom: Now.AddDays(-1));
            Assert.Equal("staff_admin", principal.Channel);
            Assert.Contains(principal.Roles!, grant => grant.IsActive(Now, "license_admin"));
            var publicKey = deviceA.ExportParameters(false);
            var device = new DeviceJwk("EC", "P-256", TestLicenseTokenSigner.Base64UrlEncode(publicKey.Q.X!),
                TestLicenseTokenSigner.Base64UrlEncode(publicKey.Q.Y!));
            var deviceJkt = DeviceProofVerifier.ComputeDeviceJkt(device);
            var setup = new SchoolBotUnitOfWork(Store(), Clock);
            var license = await setup.CreateLicenseAsync(new SchoolBotLicenseRecord(0, "S1", new string('c', 64),
                "test", Now.AddDays(10), null, null, 0, null, false, null, null, null, null, 11));
            await setup.CommitLicenseActivationAsync(new(license.SchoolBotLicenseId, null, 0, deviceJkt,
                JsonSerializer.Serialize(device), Now.AddMinutes(15)));
            await ((ICurriculumBackingStore)Store()).MutateCurriculumUnitAsync((_, _) =>
                Task.FromResult(new CurriculumStoreMutation<bool>(BuildSeed(deviceJkt), true)));
            school.SeedStudent(new(301, 11, "student", true));
            adapters = [new(Store(), Clock), new(Store(), Clock)];
            curricula = adapters.Select(adapter => new CurriculumService(adapter, Clock)).ToArray();
            progress = adapters.Select(adapter => new AssessmentReadService(adapter, this, Clock, adapter)).ToArray();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseSchoolBotMemoryStorage().ConfigureServices(services =>
        {
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<ISchoolBotBackingStore>(Store());
            services.AddSchoolBotRepositories(adapters[0]);
            services.AddSingleton<ISchoolProfileRepository>(school);
            services.AddSingleton<IAssessmentAuthority>(this);
            services.AddTrustedGateway();
            services.AddScoped<ICurriculumService>(_ => curricula[Interlocked.Increment(ref CurriculumReads) % 2]);
            services.AddScoped(_ => progress[Interlocked.Increment(ref ProgressReads) % 2]);
        });

        public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
            CancellationToken ct = default)
        {
            AuthorityReads++;
            Assert.Equal("student", principal.ProfileKind);
            Assert.Equal(301, principal.LearnerId);
            Assert.Equal(11, principal.Session.SchoolId);
            Assert.Equal((11, 401, 101, "term-1"), (run.SchoolId, run.ClassroomId, run.NYear, run.NTerm));
            Assert.Equal([501], run.PlanCourseIds);
            return Task.FromResult(true);
        }

        private static CurriculumStoreSnapshot BuildSeed(string deviceJkt)
        {
            var course = new SchoolBotCurriculumRecord(71, Key(71), 2, 1, "Beginner", ["ป.1"],
                [new(Key(80), "computing", "วิทยาการคำนวณ")], "test:course", "published",
                Now.AddDays(-1), false, 1, Now, 2, Now);
            var lesson = new SchoolBotLessonRecord(81, 71, Key(81), 1, "เริ่มต้นกับหุ่นยนต์", "เรียนรู้", "ฝึกฝน",
                false, 1, Now, 2, Now);
            var missions = new[] { "automatic", "teacher", "automatic" }.Select((method, index) =>
                new SchoolBotMissionRecord(91 + index, 81, Key(91 + index), index + 1, Titles[index + 3],
                    "ทำภารกิจ", method, false, 1, Now, 2, Now)).ToArray();
            var criteria = missions.Select(mission => new SchoolBotCriteriaRecord(mission.SchoolBotMissionId,
                mission.SchoolBotMissionId, 1, mission.Method == "teacher"
                    ? JsonSerializer.Serialize(new { kind = "teacher-v1",
                        items = new[] { new { id = Key(600), instruction = "ตรวจ" } } })
                    : JsonSerializer.Serialize(new { kind = "automatic-v1", combine = "all",
                        rules = new[] { new { id = Key(601), type = "uses_op", op = "jump", minimum = 1 } } }),
                "test:criteria", "approver", Now.AddDays(-1), false, 1, Now, 2, Now)).ToArray();
            var activities = Enumerable.Range(0, 8).Select(index =>
            {
                object config = index < 3
                    ? new { kind = "instruction-v1", instructions = Titles[index], media_refs = new[] { "test:media" } }
                    : index < 6 ? new { kind = "project-v1", mission_id = 91 + index - 3 }
                    : new { kind = "question-v1", bank_id = 1, question_keys = new[] { Key(610) },
                        default_policy = new AssessmentPolicy(2, "best", 120, "after_submit", false, false) };
                return new SchoolBotActivityRecord(101 + index, 81, Key(101 + index), index + 1,
                    Titles[index], Types[index], Modes[index], "[]", JsonSerializer.Serialize(config), false, 1, Now, 2, Now);
            }).ToArray();
            var bank = new SchoolBotQuestionBankRecord(1, Key(620), 1, "{\"th\":\"คำถาม\"}",
                [new(Key(610), "boolean", JsonSerializer.SerializeToElement(new { th = "จริงหรือไม่" }),
                    [new(Key(611), JsonSerializer.SerializeToElement(new { th = "จริง" })),
                     new(Key(612), JsonSerializer.SerializeToElement(new { th = "เท็จ" }))], [Key(611)], "1")],
                "test:bank", "published", Now.AddDays(-1), false, 1, Now, 2, Now);
            var run = new SchoolBotCourseRunRecord(72, 71, 401, 101, "term-1", [501], null, null,
                Now.AddDays(-1), "active", false, 2, Now, 2, Now, 11);
            var settings = activities.Select(activity => new SchoolBotCourseRunActivityRecord(activity.SchoolBotActivityId,
                72, activity.SchoolBotActivityId, Now.AddHours(-1), Now.AddHours(1), null, false, 2, Now, 2, Now, 11)).ToArray();
            var session = new SchoolBotSessionRecord(1, "desktop", 9001, TokenHash, "student", 301,
                null, deviceJkt, null, Now.AddHours(8), null, false, 301, Now, 301, Now, 11);
            return new(1, [course], [lesson], missions, criteria, activities, [], [], [bank], [run], settings, [], [session]);
        }
    }
}


