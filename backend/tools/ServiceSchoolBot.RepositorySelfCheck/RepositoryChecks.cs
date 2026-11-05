using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Projects;

internal static class RepositoryChecks
{
    public static async Task RunAsync(string connection)
    {
        var schoolId = Random.Shared.Next(10000, 100000000);
        await using var context = Check.Context(connection);
        var repository = Check.Repository(context);
        var project = await repository.CommitProjectAsync(new(null, schoolId, "student", 71, "block", "Project", "{}"));
        Check.That(project.Project.SchoolBotProjectId > 0 && project.Revision.ProjectId == project.Project.SchoolBotProjectId,
            "Project identity mapping failed.");
        var etag = OpaqueConcurrencyToken.ToETag(project.Project.UpdateDate);
        await using (var reader = Check.Context(connection))
        {
            var persisted = await Check.Repository(reader).GetProjectByIdAsync(project.Project.SchoolBotProjectId);
            Check.That(persisted == project.Project, "Project did not survive a new repository/context.");
            Check.That((await Check.Repository(reader).ListProjectsByOwnerAsync(schoolId + 1, "student", 71)).Count == 0,
                "Project crossed school boundaries.");
        }
        await Check.RejectAsync(() => repository.CommitProjectAsync(new(project.Project.SchoolBotProjectId,
            schoolId + 1, "student", 71, "block", "Wrong school", "{}", etag)), ErrorCodes.NotFound);
        var changed = await repository.CommitProjectAsync(new(project.Project.SchoolBotProjectId,
            schoolId, "student", 71, "block", "Updated", "{}", etag));
        await Check.RejectAsync(() => repository.CommitProjectAsync(new(project.Project.SchoolBotProjectId,
            schoolId, "student", 71, "block", "Stale", "{}", etag)), ErrorCodes.VersionConflict);
        Check.That((await repository.ListProjectRevisionsAsync(project.Project.SchoolBotProjectId)).Count == 2,
            "Rejected project mutation changed its revisions.");
        Check.That(changed.Project.UpdateDate > project.Project.UpdateDate, "Project ETag did not advance.");

        var now = DateTimeOffset.UtcNow;
        var license = await repository.CreateLicenseAsync(new(0, "Test school", Guid.NewGuid().ToString("N").PadRight(64, '0'),
            "test", now.AddDays(30), null, null, 1, null, false, null, null, null, null, schoolId));
        var activation = await repository.CommitLicenseActivationAsync(new(license.SchoolBotLicenseId, null, 1,
            "device-1", "{}", now.AddDays(1)));
        Check.That(activation.License.CurrentBindingId == activation.Binding.SchoolBotLicenseBindingId,
            "License pointer did not use the generated binding identity.");
        await Check.RejectAsync(() => repository.CommitLicenseActivationAsync(new(license.SchoolBotLicenseId, null, 1,
            "device-2", "{}", now.AddDays(1))), ErrorCodes.DeviceLimit);
        var other = await repository.CreateLicenseAsync(license with
        {
            SchoolBotLicenseId = 0, KeyHash = Guid.NewGuid().ToString("N").PadRight(64, '0'), CurrentBindingId = null
        });
        try
        {
            await repository.CommitLicenseActivationAsync(new(other.SchoolBotLicenseId, null, 1, "device-x", "{}", now.AddDays(1)),
                prepare: _ => throw new OperationCanceledException("Synthetic signing failure."));
            throw new InvalidOperationException("Signing failure was accepted.");
        }
        catch (OperationCanceledException) { }
        Check.That((await repository.ListBindingsByLicenseIdAsync(other.SchoolBotLicenseId)).Count == 0,
            "A failed signing callback left a binding behind.");
        var otherActivation = await repository.CommitLicenseActivationAsync(new(other.SchoolBotLicenseId, null, 1,
            "device-y", "{}", now.AddDays(1)));
        Check.That(otherActivation.Binding.SchoolBotLicenseBindingId != activation.Binding.SchoolBotLicenseBindingId,
            "Binding identities collided between licenses.");
        var detached = await repository.CommitLicenseUnbindAsync(new(license.SchoolBotLicenseId,
            OpaqueConcurrencyToken.ToETag(activation.License.UpdateDate), "next-device", 7));
        Check.That(detached.CurrentBindingId is null && detached.BindingEpoch == 2, "Unbind did not advance the epoch.");

        var session = await repository.CreateSessionAsync(new(0, "desktop", 71, Guid.NewGuid().ToString("N").PadRight(64, '0'),
            "student", 71, null, "device", null, now.AddHours(1), null, false, null, null, null, null, schoolId));
        await repository.RevokeSessionAsync(session.TokenHash, 71);
        Check.That((await repository.GetSessionByTokenHashAsync(session.TokenHash))?.RevokedAt is not null,
            "Session revocation did not persist.");

        var command = new IdempotencyCommand(null, "staff_admin:7", "self_check_curriculum", Guid.NewGuid(), new string('a', 64));
        var curriculumKey = Guid.NewGuid();
        var calls = 0;
        var first = await repository.CommitWithIdempotencyAsync(command, (_, _, _, _) => Task.CompletedTask,
            async (scope, timestamp, ct) =>
            {
                calls++;
                var record = await scope.CreateCurriculumAsync(Curriculum(curriculumKey, timestamp), Audit(timestamp), ct);
                return new MutationResult<SchoolBotCurriculumRecord>(record, 201, record.SchoolBotCurriculumId,
                    OpaqueConcurrencyToken.ToETag(record.UpdateDate));
            });
        await using (var restart = Check.Context(connection))
        {
            var replay = await Check.Repository(restart).CommitWithIdempotencyAsync<SchoolBotCurriculumRecord>(command, (_, _, _, _) => Task.CompletedTask,
                (_, _, _) => throw new InvalidOperationException("Replay executed the mutation."));
            Check.That(replay.Replayed && replay.Result.Body.SchoolBotCurriculumId == first.Result.Body.SchoolBotCurriculumId,
                "Idempotency result did not survive a new context.");
        }
        Check.That(calls == 1, "Mutation was repeated.");
        await CurriculumChecks.RunAsync(repository, first.Result.Body);
        await Check.RejectAsync(() => repository.CommitWithIdempotencyAsync<SchoolBotCurriculumRecord>(
            command with { RequestHash = new string('b', 64) }, (_, _, _, _) => Task.CompletedTask,
            (_, _, _) => throw new InvalidOperationException()), ErrorCodes.IdempotencyConflict);

        var rollbackKey = Guid.NewGuid();
        try
        {
            await repository.CommitWithIdempotencyAsync<SchoolBotCurriculumRecord>(command with { RequestKey = Guid.NewGuid() },
                (_, _, _, _) => Task.CompletedTask, async (scope, timestamp, ct) =>
                {
                    await scope.CreateCurriculumAsync(Curriculum(rollbackKey, timestamp), Audit(timestamp), ct);
                    throw new OperationCanceledException("Synthetic failure after insert.");
                });
        }
        catch (OperationCanceledException) { }
        Check.That(await repository.GetCurriculumByKeyAndRevisionAsync(rollbackKey, 1) is null, "Transaction did not roll back.");
        Check.That(await context.SchoolBotIdempotencies.CountAsync(row => row.RequestKey == command.RequestKey) == 1,
            "Idempotency ledger duplicated the request.");
        Console.WriteLine("PASS: SchoolBot projects, license activation/unbind/signing rollback, sessions, curriculum audit/replay/rollback.");
    }

    private static SchoolBotCurriculumRecord Curriculum(Guid key, DateTimeOffset now) =>
        new(0, key, 1, 1, "{\"th\":\"Test\"}", [], [], "self-check", "draft", null, false, 7, now, 7, now);

    private static SchoolBotAuditEventRecord Audit(DateTimeOffset now) =>
        new(0, "staff_admin:7", "Curriculum.Created", "SchoolBotCurriculum", 0, Guid.NewGuid(), "self-check", false, 7, now, 7, now);
}
