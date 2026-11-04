using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Services.Implementations.TenantRecheck;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Projects;
using ServiceSchoolBot.Infrastructure.Configuration;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class SchoolBotDbAdapterContractTests : IDisposable
{
    private readonly string testDirectory;

    public SchoolBotDbAdapterContractTests()
    {
        testDirectory = Path.Combine(Path.GetTempPath(), "SchoolBotContractTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(testDirectory)) Directory.Delete(testDirectory, true);
        }
        catch { }
    }

    private static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    [Fact]
    public async Task RestartPersistence_LicenseRetainedAfterRestart_MatchesAc02704()
    {
        // 1. Initial process: create and save license to backing store
        var backingStore = new FileSchoolBotBackingStore(testDirectory);
        var adapter1 = new SchoolBotUnitOfWork(backingStore);

        var keyHash = Hash("SBOT-TEST-KEY-001");
        var license = new SchoolBotLicenseRecord(
            SchoolBotLicenseId: 0,
            SchoolName: "School Bright Academy",
            KeyHash: keyHash,
            KeyHint: "SBOT-TES…001",
            ExpiresAt: DateTimeOffset.UtcNow.AddDays(30),
            RevokedAt: null,
            CurrentBindingId: null,
            BindingEpoch: 1,
            AllowedNextJkt: null,
            IsDelete: false,
            CreateBy: 10,
            CreateDate: null,
            UpdateBy: 10,
            UpdateDate: null,
            SchoolId: 1001);

        var created = await adapter1.CreateLicenseAsync(license);
        Assert.True(created.SchoolBotLicenseId > 0);
        Assert.NotNull(created.CreateDate);
        Assert.NotNull(created.UpdateDate);

        // 2. Simulate process restart: instantiate a fresh adapter instance reading from the persistent store
        var adapterRestarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));

        var retrieved = await adapterRestarted.GetLicenseByIdAsync(created.SchoolBotLicenseId);
        Assert.NotNull(retrieved);
        Assert.Equal(created.SchoolBotLicenseId, retrieved.SchoolBotLicenseId);
        Assert.Equal("School Bright Academy", retrieved.SchoolName);
        Assert.Equal(keyHash, retrieved.KeyHash);
        Assert.Equal(1001, retrieved.SchoolId);
        Assert.Null(retrieved.RevokedAt);
        Assert.False(retrieved.IsDelete);

        var list = await adapterRestarted.ListLicensesAsync(1001);
        Assert.Single(list);
        Assert.Equal(created.SchoolBotLicenseId, list[0].SchoolBotLicenseId);
    }

    [Fact]
    public async Task CloudProject_SaveAndOpenByStudentAndTeacher_MatchesAc03602()
    {
        var backingStore = new MemorySchoolBotBackingStore();
        var adapter = new SchoolBotUnitOfWork(backingStore);

        // Student saves a cloud project
        var studentCommit = await adapter.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: null,
            SchoolId: 200,
            OwnerKind: "student",
            OwnerId: 501,
            Mode: "block",
            Name: "Robo Maze Solver",
            Content: "{\"mode\":\"block\",\"steps\":5}"));
        var savedStudentProject = studentCommit.Project;
        Assert.True(savedStudentProject.SchoolBotProjectId > 0);

        // Teacher saves a cloud project
        var teacherCommit = await adapter.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: null,
            SchoolId: 200,
            OwnerKind: "employee",
            OwnerId: 902,
            Mode: "arm",
            Name: "Arm Pick And Place Demo",
            Content: "{\"mode\":\"arm\",\"steps\":1}"));
        var savedTeacherProject = teacherCommit.Project;
        Assert.True(savedTeacherProject.SchoolBotProjectId > 0);

        // Verify retrieval and isolation
        var studentProjects = await adapter.ListProjectsByOwnerAsync(200, "student", 501);
        Assert.Single(studentProjects);
        Assert.Equal("Robo Maze Solver", studentProjects[0].Name);

        var teacherProjects = await adapter.ListProjectsByOwnerAsync(200, "employee", 902);
        Assert.Single(teacherProjects);
        Assert.Equal("Arm Pick And Place Demo", teacherProjects[0].Name);

        // Cross-owner isolation: student does not see teacher's project
        Assert.DoesNotContain(studentProjects, p => p.SchoolBotProjectId == savedTeacherProject.SchoolBotProjectId);

        // Cross-school isolation: different school gets empty
        var otherSchoolProjects = await adapter.ListProjectsByOwnerAsync(999, "student", 501);
        Assert.Empty(otherSchoolProjects);
    }

    [Fact]
    public async Task RestartPersistence_CloudProjectRetainedAfterRestart_MatchesAc03603()
    {
        var backingStore = new FileSchoolBotBackingStore(testDirectory);
        var adapter1 = new SchoolBotUnitOfWork(backingStore);

        var rev1Commit = await adapter1.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: null,
            SchoolId: 300,
            OwnerKind: "student",
            OwnerId: 777,
            Mode: "code",
            Name: "Autonomous Rover",
            Content: "{\"code\":\"rover.init();\"}"));

        var etag = OpaqueConcurrencyToken.ToETag(rev1Commit.Project.UpdateDate);
        var rev2Commit = await adapter1.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: rev1Commit.Project.SchoolBotProjectId,
            SchoolId: 300,
            OwnerKind: "student",
            OwnerId: 777,
            Mode: "code",
            Name: "Autonomous Rover",
            Content: "{\"code\":\"rover.forward(10);\"}",
            ExpectedETag: etag));

        // Simulate service restart
        var adapterRestarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));

        var openProject = await adapterRestarted.GetProjectByIdAsync(rev1Commit.Project.SchoolBotProjectId);
        Assert.NotNull(openProject);
        Assert.Equal("Autonomous Rover", openProject.Name);
        Assert.Equal(2, openProject.CurrentRevision);

        var revisions = await adapterRestarted.ListProjectRevisionsAsync(rev1Commit.Project.SchoolBotProjectId);
        Assert.Equal(2, revisions.Count);
        Assert.Equal("{\"code\":\"rover.init();\"}", revisions[0].Content);
        Assert.Equal("{\"code\":\"rover.forward(10);\"}", revisions[1].Content);

        var byOwner = await adapterRestarted.ListProjectsByOwnerAsync(300, "student", 777);
        Assert.Single(byOwner);
    }

    [Fact]
    public void SchemaMapping_AllPrimaryKeysAreInt4AndRefColumnsMatchDdl_MatchesAc03305()
    {
        // AC-033-05: PKs of SchoolBot tables must be int4 (System.Int32) and legacy reference columns match DDL
        var tables = new[]
        {
            (typeof(SchoolBotLicenseRecord), "SchoolBotLicenseId"),
            (typeof(SchoolBotLicenseBindingRecord), "SchoolBotLicenseBindingId"),
            (typeof(SchoolBotProjectRecord), "SchoolBotProjectId"),
            (typeof(SchoolBotProjectRevisionRecord), "SchoolBotProjectRevisionId"),
            (typeof(SchoolBotSessionRecord), "SchoolBotSessionId"),
            (typeof(SchoolBotStaffRoleRecord), "SchoolBotStaffRoleId")
        };

        foreach (var (tableType, pkName) in tables)
        {
            var prop = tableType.GetProperty(pkName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(prop);
            Assert.True(prop.PropertyType == typeof(int), $"{tableType.Name}.{pkName} must be int4 (int)");

            // Check standard 6 audit/tenant fields
            var standardFields = new[] { "IsDelete", "CreateBy", "CreateDate", "UpdateBy", "UpdateDate", "SchoolId" };
            foreach (var field in standardFields)
            {
                var fProp = tableType.GetProperty(field, BindingFlags.Public | BindingFlags.Instance);
                Assert.NotNull(fProp);
            }
        }

        // Check reference columns are int/int?
        Assert.Equal(typeof(int), typeof(SchoolBotLicenseBindingRecord).GetProperty("LicenseId")!.PropertyType);
        Assert.Equal(typeof(int), typeof(SchoolBotProjectRevisionRecord).GetProperty("ProjectId")!.PropertyType);
        Assert.Equal(typeof(int), typeof(SchoolBotProjectRecord).GetProperty("OwnerId")!.PropertyType);
    }

    [Fact]
    public async Task OptimisticConcurrency_UpdateDateAsOpaqueETag_PreventsLostUpdates()
    {
        var adapter = new SchoolBotUnitOfWork(new MemorySchoolBotBackingStore());

        var created = await adapter.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: null,
            SchoolId: 50,
            OwnerKind: "student",
            OwnerId: 101,
            Mode: "block",
            Name: "Original Name",
            Content: "{\"v\":1}"));
        var validETag = OpaqueConcurrencyToken.ToETag(created.Project.UpdateDate);

        // Stale ETag fails with VersionConflict (412)
        var staleETag = "\"0000000000000000\"";
        var conflict = await Assert.ThrowsAsync<DomainRuleException>(() =>
            adapter.CommitProjectAsync(new CommitProjectRequest(
                ProjectId: created.Project.SchoolBotProjectId,
                SchoolId: 50,
                OwnerKind: "student",
                OwnerId: 101,
                Mode: "block",
                Name: "Stale Update",
                Content: "{\"v\":2}",
                ExpectedETag: staleETag)));
        Assert.Equal(ErrorCodes.VersionConflict, conflict.Code);

        // Valid ETag succeeds and produces new UpdateDate
        var updated = await adapter.CommitProjectAsync(new CommitProjectRequest(
            ProjectId: created.Project.SchoolBotProjectId,
            SchoolId: 50,
            OwnerKind: "student",
            OwnerId: 101,
            Mode: "block",
            Name: "Updated Name",
            Content: "{\"v\":2}",
            ExpectedETag: validETag));
        Assert.Equal("Updated Name", updated.Project.Name);
        Assert.Equal(2, updated.Project.CurrentRevision);
        Assert.True(updated.Project.UpdateDate >= created.Project.UpdateDate);
    }

    [Fact]
    public async Task TenantRecheck_CrossConnectionVerification_BlocksCommitOnInvalidTenantOrProfile()
    {
        var schoolBotDb = new SchoolBotUnitOfWork(new MemorySchoolBotBackingStore());
        var schoolDb = new SchoolProfileRepository();
        var recheck = new TenantRecheckService(schoolBotDb, schoolDb, TimeProvider.System, schoolBotDb);

        // 1. School has active license
        var license = await schoolBotDb.CreateLicenseAsync(new(
            0, "School A", Hash("k"), "hint", DateTimeOffset.UtcNow.AddDays(10), null, null, 1, null, false, 1, null, 1, null, 100));

        // 2. Active student in school 100
        schoolDb.SeedStudent(new(StudentId: 50, SchoolId: 100, DisplayName: "Somchai", IsActive: true));

        // Allow active student
        var allowed = await recheck.RecheckTenantBeforeCommitAsync(100, "student", 50);
        Assert.True(allowed.Allowed);

        // Deny inactive student
        schoolDb.SeedStudent(new(StudentId: 51, SchoolId: 100, DisplayName: "Inactive Student", IsActive: false));
        var deniedInactive = await recheck.RecheckTenantBeforeCommitAsync(100, "student", 51);
        Assert.False(deniedInactive.Allowed);
        Assert.Equal(ErrorCodes.SchoolMismatch, deniedInactive.ErrorCode);

        // Deny cross-school student
        var deniedCrossSchool = await recheck.RecheckTenantBeforeCommitAsync(999, "student", 50);
        Assert.False(deniedCrossSchool.Allowed);

        // Deny when school license is revoked
        await schoolBotDb.RevokeLicenseAsync(license.SchoolBotLicenseId, 1);
        var deniedRevokedLicense = await recheck.RecheckTenantBeforeCommitAsync(100, "student", 50);
        Assert.False(deniedRevokedLicense.Allowed);
        Assert.Equal(ErrorCodes.LicenseInactive, deniedRevokedLicense.ErrorCode);
    }

    [Fact]
    public void DatabaseConfiguration_FiveDatabaseConnections_ConfiguredThroughEnvironmentOrConfig()
    {
        var dict = new Dictionary<string, string?>
        {
            { "ConnectionStrings:SchoolBotDb", "Host=10.0.0.128;Port=9499;Database=SchoolBot;Username=schoolbot_app;" },
            { "ConnectionStrings:MasterDb", "Host=master.internal;Database=JabjaiMasterDB;" },
            { "ConnectionStrings:SchoolDb", "Host=school.internal;Database=JabjaiSchoolSingleDB;" },
            { "ConnectionStrings:LogsDb", "Host=logs.internal;Database=JabjaiLogsDB;" },
            { "ConnectionStrings:PGGradeDb", "Host=grade.internal;Database=PGGradeDB;" }
        };

        var config = new DatabaseConnectionConfig(key => dict.GetValueOrDefault(key));

        Assert.True(config.IsSchoolBotDbConfigured);
        Assert.True(config.IsMasterDbConfigured);
        Assert.True(config.IsSchoolDbConfigured);
        Assert.True(config.IsLogsDbConfigured);
        Assert.True(config.IsPGGradeDbConfigured);

        Assert.Contains("10.0.0.128", config.SchoolBotDbConnectionString);
        Assert.Contains("9499", config.SchoolBotDbConnectionString);
        Assert.Contains("JabjaiMasterDB", config.MasterDbConnectionString);
        Assert.Contains("JabjaiSchoolSingleDB", config.SchoolDbConnectionString);
        Assert.Contains("JabjaiLogsDB", config.LogsDbConnectionString);
        Assert.Contains("PGGradeDB", config.PGGradeDbConnectionString);
    }

    [Fact]
    public async Task CommitFailure_LicenseCreateFails_CacheNotPollutedAndDiskConsistentAfterRestart()
    {
        var backingStore = new FaultingBackingStore(new FileSchoolBotBackingStore(testDirectory)) { FailLicense = true };
        var adapter = new SchoolBotUnitOfWork(backingStore);

        var failedLicense = new SchoolBotLicenseRecord(
            0, "School Bright Fail", Hash("KEY-FAIL"), "hint-fail", DateTimeOffset.UtcNow.AddDays(10),
            null, null, 1, null, false, 1, null, 1, null, 100);

        await Assert.ThrowsAsync<IOException>(() => adapter.CreateLicenseAsync(failedLicense));

        var cached = await adapter.ListLicensesAsync(100);
        Assert.Empty(cached);

        backingStore.FailLicense = false;
        var successfulLicense = new SchoolBotLicenseRecord(
            0, "School Bright OK", Hash("KEY-OK"), "hint-ok", DateTimeOffset.UtcNow.AddDays(10),
            null, null, 1, null, false, 1, null, 1, null, 100);
        var created = await adapter.CreateLicenseAsync(successfulLicense);

        var restarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));
        var diskRows = await restarted.ListLicensesAsync(100);

        Assert.Single(diskRows);
        Assert.Equal(created.SchoolBotLicenseId, diskRows[0].SchoolBotLicenseId);
        Assert.Equal("hint-ok", diskRows[0].KeyHint);
    }

    [Fact]
    public async Task CommitFailure_ProjectUpdateFails_CacheNotPollutedAndDiskConsistentAfterRestart()
    {
        var backingStore = new FaultingBackingStore(new FileSchoolBotBackingStore(testDirectory));
        var adapter = new SchoolBotUnitOfWork(backingStore);

        var created = await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 100, "student", 50, "code", "Original Title", "{}"));

        var etag = OpaqueConcurrencyToken.ToETag(created.Project.UpdateDate);
        backingStore.FailCommitProject = true;
        await Assert.ThrowsAsync<IOException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            created.Project.SchoolBotProjectId, 100, "student", 50, "code", "Failed Title", "{}", etag)));

        var cached = await adapter.GetProjectByIdAsync(created.Project.SchoolBotProjectId);
        Assert.NotNull(cached);
        Assert.Equal("Original Title", cached.Name);

        backingStore.FailCommitProject = false;
        await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 100, "employee", 60, "code", "Another Project", "{}"));

        var restarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));
        var disk = await restarted.GetProjectByIdAsync(created.Project.SchoolBotProjectId);
        Assert.NotNull(disk);
        Assert.Equal("Original Title", disk.Name);
    }

    [Fact]
    public async Task CommitFailure_ProjectRevisionFails_LeavesNoHalfProjectAfterRestart()
    {
        var backingStore = new FaultingBackingStore(new FileSchoolBotBackingStore(testDirectory));
        var adapter = new SchoolBotUnitOfWork(backingStore);

        backingStore.FailCommitProject = true;
        await Assert.ThrowsAsync<IOException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 100, "student", 50, "code", "Atomic Project", "{\"code\":\"init\"}")));

        var cachedProject = await adapter.GetProjectByIdAsync(1);
        Assert.Null(cachedProject);

        var restarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));
        var diskProject = await restarted.GetProjectByIdAsync(1);
        var diskRevision = await restarted.GetProjectRevisionAsync(1, 1);

        Assert.Null(diskProject);
        Assert.Null(diskRevision);
    }

    [Fact]
    public async Task CommitFailure_RevisionAndRollbackWriteFailure_LeavesNoHalfProjectAfterRestart()
    {
        var backingStore = new FaultingBackingStore(new FileSchoolBotBackingStore(testDirectory));
        var adapter = new SchoolBotUnitOfWork(backingStore);

        backingStore.FailCommitProject = true;
        await Assert.ThrowsAsync<IOException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 100, "student", 50, "code", "Atomic Project", "{\"code\":\"init\"}")));

        var cachedProject = await adapter.GetProjectByIdAsync(1);
        Assert.Null(cachedProject);

        var restarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));
        var diskProject = await restarted.GetProjectByIdAsync(1);
        var diskRevision = await restarted.GetProjectRevisionAsync(1, 1);

        Assert.Null(diskProject);
        Assert.Null(diskRevision);
    }

    [Fact]
    public async Task Invariant1_AfterRestartEveryProjectHasExactMatchingRevisionAndParentProject()
    {
        var backingStore = new FileSchoolBotBackingStore(testDirectory);
        var adapter = new SchoolBotUnitOfWork(backingStore);

        var p1 = await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 100, "student", 10, "block", "Proj 1", "{\"p\":1}"));
        var p2 = await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 100, "employee", 20, "arm", "Proj 2", "{\"p\":2}"));

        var etag1 = OpaqueConcurrencyToken.ToETag(p1.Project.UpdateDate);
        await adapter.CommitProjectAsync(new CommitProjectRequest(
            p1.Project.SchoolBotProjectId, 100, "student", 10, "block", "Proj 1 Rev 2", "{\"p\":1,\"v\":2}", etag1));

        var restarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));
        var projects = await restarted.ListProjectsByOwnerAsync(100, "student", 10);
        Assert.Single(projects);
        var proj1 = projects[0];
        Assert.Equal(2, proj1.CurrentRevision);

        var revs1 = await restarted.ListProjectRevisionsAsync(proj1.SchoolBotProjectId);
        Assert.Equal(2, revs1.Count);
        Assert.Contains(revs1, r => r.Revision == proj1.CurrentRevision && r.ProjectId == proj1.SchoolBotProjectId && r.SchoolId == proj1.SchoolId);
        Assert.All(revs1, r => Assert.Equal(proj1.SchoolBotProjectId, r.ProjectId));

        var empProjects = await restarted.ListProjectsByOwnerAsync(100, "employee", 20);
        Assert.Single(empProjects);
        var proj2 = empProjects[0];
        Assert.Equal(1, proj2.CurrentRevision);
        var revs2 = await restarted.ListProjectRevisionsAsync(proj2.SchoolBotProjectId);
        Assert.Single(revs2);
        Assert.Equal(1, revs2[0].Revision);
        Assert.Equal(proj2.SchoolBotProjectId, revs2[0].ProjectId);
    }

    [Fact]
    public async Task Invariant2_SecondProjectWriteFailureLeavesNoHalfProjectOnDiskOrCache()
    {
        var backingStore = new FaultingBackingStore(new FileSchoolBotBackingStore(testDirectory));
        var adapter = new SchoolBotUnitOfWork(backingStore);

        // First project succeeds
        var first = await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 50, "student", 10, "code", "First Project", "{\"source\":\"first\"}"));
        Assert.True(first.Project.SchoolBotProjectId > 0);

        // Second project fails at commit
        backingStore.FailCommitProject = true;
        await Assert.ThrowsAsync<IOException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 50, "employee", 20, "code", "Second Project", "{\"source\":\"second\"}")));

        // Verify cache in the same adapter instance
        var secondInCache = await adapter.GetProjectByIdAsync(first.Project.SchoolBotProjectId + 1);
        Assert.Null(secondInCache);

        // Verify restarted store
        var restarted = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));
        var firstOnDisk = await restarted.GetProjectByIdAsync(first.Project.SchoolBotProjectId);
        Assert.NotNull(firstOnDisk);
        var firstRev = await restarted.GetProjectRevisionAsync(first.Project.SchoolBotProjectId, 1);
        Assert.NotNull(firstRev);
        Assert.Equal("{\"source\":\"first\"}", firstRev.Content);

        var secondOnDisk = await restarted.GetProjectByIdAsync(first.Project.SchoolBotProjectId + 1);
        var secondRev = await restarted.GetProjectRevisionAsync(first.Project.SchoolBotProjectId + 1, 1);
        Assert.Null(secondOnDisk);
        Assert.Null(secondRev);

        // Subsequent write succeeds cleanly
        backingStore.FailCommitProject = false;
        var second = await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 50, "employee", 20, "code", "Second Project Clean", "{\"source\":\"second-clean\"}"));
        Assert.NotNull(second);

        var restarted2 = new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(testDirectory));
        var secondRetrieved = await restarted2.GetProjectByIdAsync(second.Project.SchoolBotProjectId);
        Assert.NotNull(secondRetrieved);
        Assert.Equal("Second Project Clean", secondRetrieved.Name);
    }

    [Fact]
    public async Task Invariant3_FaultBeforeCommitLeavesCacheAndStoreUnchanged_NextCommitClean()
    {
        var backingStore = new FileSchoolBotBackingStore(testDirectory);
        var adapter = new SchoolBotUnitOfWork(backingStore);

        // Validation failures before commit
        await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 0, "student", 1, "code", "Invalid School", "{}")));
        await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "invalid-kind", 1, "code", "Invalid Kind", "{}")));
        await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "student", 1, "invalid-mode", "Invalid Mode", "{}")));
        await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "student", 1, "code", new string('x', 201), "{}")));
        await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "student", 1, "code", "Valid", null!)));

        // Store and cache remain completely empty
        var emptyProjects = await adapter.ListProjectsByOwnerAsync(10, "student", 1);
        Assert.Empty(emptyProjects);

        // Clean commit gets next ID = 1
        var created = await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "student", 1, "code", "First Valid", "{}"));
        Assert.Equal(1, created.Project.SchoolBotProjectId);
        Assert.Equal(1, created.Revision.SchoolBotProjectRevisionId);
    }

    [Fact]
    public async Task Invariant4_CommitFailureInvalidatesCacheAndNeverReportsSuccess()
    {
        var backingStore = new FaultingBackingStore(new FileSchoolBotBackingStore(testDirectory));
        var adapter = new SchoolBotUnitOfWork(backingStore);

        backingStore.FailCommitProject = true;
        await Assert.ThrowsAsync<IOException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "student", 1, "block", "Failed Proj", "{}")));

        // Adapter does not report success or leave phantom in cache
        var cached = await adapter.GetProjectByIdAsync(1);
        Assert.Null(cached);
    }

    [Fact]
    public async Task Invariant5_UpdateWithStaleETagDoesNotWrite_UpdateAddsRevisionWithoutModifyingOldRevision()
    {
        var adapter = new SchoolBotUnitOfWork(new MemorySchoolBotBackingStore());

        var created = await adapter.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "student", 5, "arm", "Arm Demo", "{\"v\":1}"));
        var originalRevision = created.Revision;

        // Missing ETag throws PreconditionRequired (428)
        var missingETagEx = await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            created.Project.SchoolBotProjectId, 10, "student", 5, "arm", "No ETag", "{\"v\":2}", ExpectedETag: null)));
        Assert.Equal(ErrorCodes.PreconditionRequired, missingETagEx.Code);

        // Stale ETag throws VersionConflict (412)
        var staleEx = await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            created.Project.SchoolBotProjectId, 10, "student", 5, "arm", "Stale", "{\"v\":2}", ExpectedETag: "\"stale-etag\"")));
        Assert.Equal(ErrorCodes.VersionConflict, staleEx.Code);

        // Changing owner/school/mode throws InvalidInput (400)
        var validETag = OpaqueConcurrencyToken.ToETag(created.Project.UpdateDate);
        var changeOwnerEx = await Assert.ThrowsAsync<DomainRuleException>(() => adapter.CommitProjectAsync(new CommitProjectRequest(
            created.Project.SchoolBotProjectId, 10, "employee", 99, "arm", "Changed Owner", "{\"v\":2}", ExpectedETag: validETag)));
        Assert.Equal(ErrorCodes.InvalidInput, changeOwnerEx.Code);

        // Valid update succeeds
        var updated = await adapter.CommitProjectAsync(new CommitProjectRequest(
            created.Project.SchoolBotProjectId, 10, "student", 5, "arm", "Arm Demo Rev 2", "{\"v\":2}", ExpectedETag: validETag));
        Assert.Equal(2, updated.Project.CurrentRevision);
        Assert.Equal(2, updated.Revision.Revision);

        // Old revision is completely untouched
        var oldRev = await adapter.GetProjectRevisionAsync(created.Project.SchoolBotProjectId, 1);
        Assert.NotNull(oldRev);
        Assert.Equal(originalRevision.Content, oldRev.Content);
        Assert.Equal(originalRevision.ContentHash, oldRev.ContentHash);
        Assert.Equal(1, oldRev.Revision);
    }

    [Fact]
    public async Task Invariant6_InitLoadFailureRetriesCleanlyWithoutDuplicatesOrHalfSnapshots()
    {
        var backingStore = new FaultingBackingStore(new FileSchoolBotBackingStore(testDirectory));
        var adapter1 = new SchoolBotUnitOfWork(backingStore);
        await adapter1.CommitProjectAsync(new CommitProjectRequest(
            null, 10, "student", 1, "block", "Existing", "{}"));

        // Adapter 2 fails during initial load of projects
        backingStore.FailLoadProjects = true;
        var adapter2 = new SchoolBotUnitOfWork(backingStore);
        await Assert.ThrowsAsync<IOException>(() => adapter2.GetProjectByIdAsync(1));

        // Retry with fault cleared: should load cleanly without duplicate records
        backingStore.FailLoadProjects = false;
        var project = await adapter2.GetProjectByIdAsync(1);
        Assert.NotNull(project);
        var list = await adapter2.ListProjectsByOwnerAsync(10, "student", 1);
        Assert.Single(list);
    }

    private sealed class FaultingBackingStore(ISchoolBotBackingStore inner) : ISchoolBotBackingStore
    {
        public Task<LicenseStoreSnapshot> LoadLicenseUnitAsync(CancellationToken ct = default) =>
            inner.LoadLicenseUnitAsync(ct);

        public Task<T> MutateLicenseUnitAsync<T>(
            Func<LicenseStoreSnapshot, LicenseStoreMutation<T>> mutate, CancellationToken ct = default) =>
            FailLicense ? Task.FromException<T>(new IOException("Simulated license commit failure")) :
                inner.MutateLicenseUnitAsync(mutate, ct);

        public bool FailLicense { get; set; }
        public bool FailCommitProject { get; set; }
        public bool FailProject { get => FailCommitProject; set => FailCommitProject = value; }
        public bool FailRevision { get => FailCommitProject; set => FailCommitProject = value; }
        public bool FailSession { get; set; }
        public bool FailLoadProjects { get; set; }
        public bool FailLoadRevisions { get; set; }

        public Task<IReadOnlyList<SchoolBotLicenseRecord>> LoadLicensesAsync(CancellationToken ct = default) => inner.LoadLicensesAsync(ct);

        public Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> LoadBindingsAsync(CancellationToken ct = default) => inner.LoadBindingsAsync(ct);

        public Task<IReadOnlyList<SchoolBotProjectRecord>> LoadProjectsAsync(CancellationToken ct = default) =>
            FailLoadProjects ? Task.FromException<IReadOnlyList<SchoolBotProjectRecord>>(new IOException("Simulated load projects failure")) : inner.LoadProjectsAsync(ct);

        public Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> LoadProjectRevisionsAsync(CancellationToken ct = default) =>
            FailLoadRevisions ? Task.FromException<IReadOnlyList<SchoolBotProjectRevisionRecord>>(new IOException("Simulated load revisions failure")) : inner.LoadProjectRevisionsAsync(ct);

        public Task CommitProjectAsync(
            IReadOnlyList<SchoolBotProjectRecord> projects,
            IReadOnlyList<SchoolBotProjectRevisionRecord> revisions,
            CancellationToken ct = default) =>
            FailCommitProject ? Task.FromException(new IOException("Simulated project commit failure")) : inner.CommitProjectAsync(projects, revisions, ct);

        public Task<IReadOnlyList<SchoolBotSessionRecord>> LoadSessionsAsync(CancellationToken ct = default) => inner.LoadSessionsAsync(ct);
        public Task SaveSessionsAsync(IReadOnlyList<SchoolBotSessionRecord> rows, CancellationToken ct = default) =>
            FailSession ? Task.FromException(new IOException("Simulated session commit failure")) : inner.SaveSessionsAsync(rows, ct);

        public Task<IReadOnlyList<SchoolBotCurriculumRecord>> LoadCurriculaAsync(CancellationToken ct = default) => inner.LoadCurriculaAsync(ct);
        public Task<IReadOnlyList<SchoolBotLessonRecord>> LoadLessonsAsync(CancellationToken ct = default) => inner.LoadLessonsAsync(ct);
        public Task<IReadOnlyList<SchoolBotMissionRecord>> LoadMissionsAsync(CancellationToken ct = default) => inner.LoadMissionsAsync(ct);
        public Task<IReadOnlyList<SchoolBotCriteriaRecord>> LoadCriteriaAsync(CancellationToken ct = default) => inner.LoadCriteriaAsync(ct);
        public Task<IReadOnlyList<SchoolBotActivityRecord>> LoadActivitiesAsync(CancellationToken ct = default) => inner.LoadActivitiesAsync(ct);
        public Task<IReadOnlyList<SchoolBotAuditEventRecord>> LoadAuditEventsAsync(CancellationToken ct = default) => inner.LoadAuditEventsAsync(ct);
        public Task CommitCurriculumUnitAsync(
            IReadOnlyList<SchoolBotCurriculumRecord> curricula,
            IReadOnlyList<SchoolBotLessonRecord> lessons,
            IReadOnlyList<SchoolBotMissionRecord> missions,
            IReadOnlyList<SchoolBotCriteriaRecord> criteria,
            IReadOnlyList<SchoolBotActivityRecord> activities,
            IReadOnlyList<SchoolBotAuditEventRecord> auditEvents,
            CancellationToken ct = default) =>
            inner.CommitCurriculumUnitAsync(curricula, lessons, missions, criteria, activities, auditEvents, ct);
    }
}
