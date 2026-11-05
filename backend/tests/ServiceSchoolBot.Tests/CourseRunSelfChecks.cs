using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using System.Text.Json;

namespace ServiceSchoolBot.Tests;

public sealed class CourseRunSelfChecks
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SelectedBeginnerCourseReadsAllActivitiesWithoutChangingState(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await fixture.Initialize();
        var curricula = GrpcTest.Curricula(fixture);
        var assessments = GrpcTest.Assessments(fixture);
        var student = await fixture.Student();
        var baseline = JsonSerializer.Serialize(await fixture.Snapshot());
        var robot = new FakeActivityRobot();
        for (var read = 0; read < 2; read++)
        {
            var course = CurriculumRpc.ToModel(
                await curricula.GetPublishedCurriculumAsync(new() { Id = 1 }, GrpcTest.Headers(student)));
            Assert.NotNull(course);
            Assert.Equal(1, course.LevelId);
            Assert.Equal("Beginner", course.Title);
            Assert.Equal(["ป.1"], course.RecommendedGrades);
            Assert.Equal("robotics", Assert.Single(course.Subjects).Code);
            var lesson = Assert.Single(course.Lessons);
            Assert.Equal("Lesson 1", lesson.Title);
            Assert.Equal(3, lesson.Missions.Count);
            Assert.Equal(8, lesson.Activities.Count);
            Assert.Equal(Enumerable.Range(1, 8), lesson.Activities.Select(row => row.Ordinal));
            foreach (var activity in lesson.Activities)
            {
                robot.Open(activity);
                if (activity.Type == "project")
                {
                    Assert.Equal(activity.Title, activity.Mode);
                    var mission = lesson.Missions.Single(row => row.Title == activity.Title);
                    Assert.Equal(activity.Mode == "arm" ? "teacher" : "automatic", mission.Method);
                }
                else
                {
                    Assert.Null(activity.Mode);
                }
            }

            var progress = AssessmentRpc.ToModel(
                await assessments.GetProgressAsync(new() { CourseRunId = 1 }, GrpcTest.Headers(student)));
            Assert.NotNull(progress);
            Assert.Equal(301, progress.LearnerId);
            Assert.Equal(1, progress.CourseRunId);
            Assert.Equal(course.Id, progress.CurriculumId);
            Assert.Equal(course.LevelId, progress.LevelId);
            Assert.Equal(4, fixture.ClassroomGrade);
            Assert.Equal(8, progress.Activities.Count);
            Assert.Equal(3, progress.Missions.Count);
            Assert.Equal(CourseRunSeed.Now, progress.ComputedAt);
            Assert.Equal(lesson.Activities.Select(row => (row.Type, row.Mode)),
                progress.Activities.Select(row => (row.Type, row.Mode)));
            Assert.All(progress.Activities, activity =>
            {
                Assert.Null(activity.LatestSubmission);
                Assert.Null(activity.SelectedAttempt);
            });
            Assert.All(progress.Missions, mission => Assert.Null(mission.LatestSubmission));

            var reader = fixture.Reader();
            var reopenedService = new AssessmentReadService(reader, fixture, fixture.Clock, reader);
            var reopened = await reopenedService.GetProgressAsync(new AssessmentPrincipalContext(
                CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 }, null, "student", 301), 1);
            Assert.Equal(JsonSerializer.Serialize(progress), JsonSerializer.Serialize(reopened));
            var run = await reader.GetCourseRunByIdAsync(1);
            Assert.NotNull(run);
            Assert.Equal(11, run.SchoolId);
            Assert.Equal(course.Id, run.CurriculumId);
            Assert.Equal(401, run.ClassroomId);
            Assert.Equal(101, run.NYear);
            Assert.Equal("term-1", run.NTerm);
            Assert.Null((await reader.GetCurriculumByIdAsync(run.CurriculumId))!.SchoolId);
            Assert.Equal(baseline, JsonSerializer.Serialize(await fixture.Snapshot()));
        }
        Assert.Equal(Enumerable.Repeat(new[] { "block", "arm", "code" }, 2).SelectMany(modes => modes), robot.Modes);
        foreach (var mode in new[] { "block", "arm", "code" })
        {
            var progress = AssessmentRpc.ToModel(await assessments.GetProgressAsync(
                new() { CourseRunId = 1, Mode = mode }, GrpcTest.Headers(student)));
            Assert.Equal(mode, Assert.Single(progress.Activities).Mode);
            Assert.Single(progress.Missions);
        }
        Assert.Equal(baseline, JsonSerializer.Serialize(await fixture.Snapshot()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StudentSessionAndRosterAreCheckedOnEachRead(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await fixture.Initialize();
        var assessments = GrpcTest.Assessments(fixture);
        var student = await fixture.Student();
        var baseline = JsonSerializer.Serialize(await fixture.Snapshot());
        fixture.School.SeedStudent(new(301, 11, "student", false));
        var denied = await GrpcTest.Fails(
            assessments.GetProgressAsync(new() { CourseRunId = 1 }, GrpcTest.Headers(student)));
        Assert.Equal(401, denied.HttpStatus);
        Assert.Equal(baseline, JsonSerializer.Serialize(await fixture.Snapshot()));
    }

    private sealed class FakeActivityRobot
    {
        internal List<string> Modes { get; } = [];

        internal void Open(ActivityInput activity)
        {
            using var config = JsonDocument.Parse(activity.Config!);
            if (activity.Type == "project")
            {
                Assert.Equal("project-v1", config.RootElement.GetProperty("kind").GetString());
                Modes.Add(activity.Mode!);
            }
            else
            {
                Assert.Null(activity.Mode);
                Assert.Contains(config.RootElement.GetProperty("kind").GetString(),
                    new[] { "instruction-v1", "question-v1" });
            }
        }
    }
}
