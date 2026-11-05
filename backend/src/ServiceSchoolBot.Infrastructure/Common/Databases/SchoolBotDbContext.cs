using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Common.Databases;

public partial class SchoolBotDbContext : DbContext
{
    public SchoolBotDbContext(DbContextOptions<SchoolBotDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SchoolBotActivity> SchoolBotActivities { get; set; }

    public virtual DbSet<SchoolBotAssessment> SchoolBotAssessments { get; set; }

    public virtual DbSet<SchoolBotAttempt> SchoolBotAttempts { get; set; }

    public virtual DbSet<SchoolBotAuditEvent> SchoolBotAuditEvents { get; set; }

    public virtual DbSet<SchoolBotCourseRun> SchoolBotCourseRuns { get; set; }

    public virtual DbSet<SchoolBotCourseRunActivity> SchoolBotCourseRunActivities { get; set; }

    public virtual DbSet<SchoolBotCriterion> SchoolBotCriteria { get; set; }

    public virtual DbSet<SchoolBotCurriculum> SchoolBotCurricula { get; set; }

    public virtual DbSet<SchoolBotGradeSlot> SchoolBotGradeSlots { get; set; }

    public virtual DbSet<SchoolBotGradingJob> SchoolBotGradingJobs { get; set; }

    public virtual DbSet<SchoolBotIdempotency> SchoolBotIdempotencies { get; set; }

    public virtual DbSet<SchoolBotLesson> SchoolBotLessons { get; set; }

    public virtual DbSet<SchoolBotLevel> SchoolBotLevels { get; set; }

    public virtual DbSet<SchoolBotLevelPromotionPolicy> SchoolBotLevelPromotionPolicies { get; set; }

    public virtual DbSet<SchoolBotLevelProposal> SchoolBotLevelProposals { get; set; }

    public virtual DbSet<SchoolBotLicense> SchoolBotLicenses { get; set; }

    public virtual DbSet<SchoolBotLicenseBinding> SchoolBotLicenseBindings { get; set; }

    public virtual DbSet<SchoolBotMission> SchoolBotMissions { get; set; }

    public virtual DbSet<SchoolBotProject> SchoolBotProjects { get; set; }

    public virtual DbSet<SchoolBotProjectRevision> SchoolBotProjectRevisions { get; set; }

    public virtual DbSet<SchoolBotQuestionBank> SchoolBotQuestionBanks { get; set; }

    public virtual DbSet<SchoolBotRelease> SchoolBotReleases { get; set; }

    public virtual DbSet<SchoolBotRetakeGrant> SchoolBotRetakeGrants { get; set; }

    public virtual DbSet<SchoolBotScoreExport> SchoolBotScoreExports { get; set; }

    public virtual DbSet<SchoolBotScoreMapping> SchoolBotScoreMappings { get; set; }

    public virtual DbSet<SchoolBotScoreOutbox> SchoolBotScoreOutboxes { get; set; }

    public virtual DbSet<SchoolBotScorePreview> SchoolBotScorePreviews { get; set; }

    public virtual DbSet<SchoolBotScorePreviewItem> SchoolBotScorePreviewItems { get; set; }

    public virtual DbSet<SchoolBotScoringPolicy> SchoolBotScoringPolicies { get; set; }

    public virtual DbSet<SchoolBotSession> SchoolBotSessions { get; set; }

    public virtual DbSet<SchoolBotStaffRole> SchoolBotStaffRoles { get; set; }

    public virtual DbSet<SchoolBotSubmission> SchoolBotSubmissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchoolBotActivity>(entity =>
        {
            entity.ToTable("SchoolBotActivity");

            entity.Property(e => e.SchoolBotActivityId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Config).HasColumnType("jsonb");
            entity.Property(e => e.Indicators).HasColumnType("jsonb");
            entity.Property(e => e.Mode).HasMaxLength(16);
            entity.Property(e => e.Title).HasColumnType("jsonb");
            entity.Property(e => e.Type).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotAssessment>(entity =>
        {
            entity.ToTable("SchoolBotAssessment");

            entity.Property(e => e.SchoolBotAssessmentId).UseIdentityAlwaysColumn();
            entity.Property(e => e.EarnedScore).HasPrecision(18, 6);
            entity.Property(e => e.Feedback).HasMaxLength(4000);
            entity.Property(e => e.GraderVersion).HasMaxLength(128);
            entity.Property(e => e.MaxScore).HasPrecision(18, 6);
            entity.Property(e => e.Method).HasMaxLength(16);
            entity.Property(e => e.Outcome).HasMaxLength(16);
            entity.Property(e => e.RuleResults).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotAttempt>(entity =>
        {
            entity.ToTable("SchoolBotAttempt");

            entity.Property(e => e.SchoolBotAttemptId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Answers).HasColumnType("jsonb");
            entity.Property(e => e.EarnedScore).HasPrecision(18, 6);
            entity.Property(e => e.MaxScore).HasPrecision(18, 6);
            entity.Property(e => e.Snapshot).HasColumnType("jsonb");
            entity.Property(e => e.State).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotAuditEvent>(entity =>
        {
            entity.ToTable("SchoolBotAuditEvent");

            entity.Property(e => e.SchoolBotAuditEventId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Actor).HasMaxLength(512);
            entity.Property(e => e.EntityType).HasMaxLength(64);
            entity.Property(e => e.Event).HasMaxLength(64);
            entity.Property(e => e.Reason).HasMaxLength(1000);
        });

        modelBuilder.Entity<SchoolBotCourseRun>(entity =>
        {
            entity.ToTable("SchoolBotCourseRun");

            entity.Property(e => e.SchoolBotCourseRunId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Lifecycle).HasMaxLength(16);
            entity.Property(e => e.Nterm)
                .HasMaxLength(64)
                .HasColumnName("NTerm");
            entity.Property(e => e.Nyear).HasColumnName("NYear");
        });

        modelBuilder.Entity<SchoolBotCourseRunActivity>(entity =>
        {
            entity.ToTable("SchoolBotCourseRunActivity");

            entity.Property(e => e.SchoolBotCourseRunActivityId).UseIdentityAlwaysColumn();
            entity.Property(e => e.PolicyOverride).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotCriterion>(entity =>
        {
            entity.HasKey(e => e.SchoolBotCriteriaId);

            entity.Property(e => e.SchoolBotCriteriaId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ApprovedBy).HasMaxLength(512);
            entity.Property(e => e.Rules).HasColumnType("jsonb");
            entity.Property(e => e.SourceRef).HasMaxLength(2048);
        });

        modelBuilder.Entity<SchoolBotCurriculum>(entity =>
        {
            entity.ToTable("SchoolBotCurriculum");

            entity.Property(e => e.SchoolBotCurriculumId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Lifecycle).HasMaxLength(16);
            entity.Property(e => e.RecommendedGrades).HasColumnType("jsonb");
            entity.Property(e => e.SourceRef).HasMaxLength(2048);
            entity.Property(e => e.Subjects).HasColumnType("jsonb");
            entity.Property(e => e.Title).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotGradeSlot>(entity =>
        {
            entity.ToTable("SchoolBotGradeSlot");

            entity.Property(e => e.SchoolBotGradeSlotId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Category).HasMaxLength(32);
            entity.Property(e => e.NgradeId).HasColumnName("NGradeId");
            entity.Property(e => e.OwnershipReason).HasMaxLength(1000);
            entity.Property(e => e.ScoreIdentifier).HasMaxLength(128);
            entity.Property(e => e.TargetSnapshot).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotGradingJob>(entity =>
        {
            entity.ToTable("SchoolBotGradingJob");

            entity.Property(e => e.SchoolBotGradingJobId).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastErrorCode).HasMaxLength(64);
            entity.Property(e => e.LeaseOwner).HasMaxLength(128);
            entity.Property(e => e.State).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotIdempotency>(entity =>
        {
            entity.ToTable("SchoolBotIdempotency");

            entity.Property(e => e.SchoolBotIdempotencyId).UseIdentityAlwaysColumn();
            entity.Property(e => e.NonceHash).HasMaxLength(64);
            entity.Property(e => e.Operation).HasMaxLength(200);
            entity.Property(e => e.Principal).HasMaxLength(512);
            entity.Property(e => e.RequestHash).HasMaxLength(64);
            entity.Property(e => e.ResultKeyId).HasMaxLength(128);
        });

        modelBuilder.Entity<SchoolBotLesson>(entity =>
        {
            entity.ToTable("SchoolBotLesson");

            entity.Property(e => e.SchoolBotLessonId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Instructions).HasColumnType("jsonb");
            entity.Property(e => e.Objective).HasColumnType("jsonb");
            entity.Property(e => e.Title).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotLevel>(entity =>
        {
            entity.ToTable("SchoolBotLevel");

            entity.Property(e => e.SchoolBotLevelId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Code).HasMaxLength(64);
            entity.Property(e => e.Title).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotLevelPromotionPolicy>(entity =>
        {
            entity.ToTable("SchoolBotLevelPromotionPolicy");

            entity.Property(e => e.SchoolBotLevelPromotionPolicyId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Threshold).HasPrecision(9, 6);
        });

        modelBuilder.Entity<SchoolBotLevelProposal>(entity =>
        {
            entity.ToTable("SchoolBotLevelProposal");

            entity.Property(e => e.SchoolBotLevelProposalId).UseIdentityAlwaysColumn();
            entity.Property(e => e.DecisionBy).HasMaxLength(512);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Snapshot).HasColumnType("jsonb");
            entity.Property(e => e.SnapshotHash).HasMaxLength(64);
            entity.Property(e => e.State).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotLicense>(entity =>
        {
            entity.ToTable("SchoolBotLicense");

            entity.Property(e => e.SchoolBotLicenseId).UseIdentityAlwaysColumn();
            entity.Property(e => e.AllowedNextJkt).HasMaxLength(64);
            entity.Property(e => e.KeyHash).HasMaxLength(64);
            entity.Property(e => e.KeyHint).HasMaxLength(32);
            entity.Property(e => e.SchoolName).HasMaxLength(200);
        });

        modelBuilder.Entity<SchoolBotLicenseBinding>(entity =>
        {
            entity.ToTable("SchoolBotLicenseBinding");

            entity.Property(e => e.SchoolBotLicenseBindingId).UseIdentityAlwaysColumn();
            entity.Property(e => e.DeviceJkt).HasMaxLength(64);
            entity.Property(e => e.PublicJwk).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotMission>(entity =>
        {
            entity.ToTable("SchoolBotMission");

            entity.Property(e => e.SchoolBotMissionId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Instructions).HasColumnType("jsonb");
            entity.Property(e => e.Method).HasMaxLength(16);
            entity.Property(e => e.Title).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotProject>(entity =>
        {
            entity.ToTable("SchoolBotProject");

            entity.Property(e => e.SchoolBotProjectId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Mode).HasMaxLength(16);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OwnerKind).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotProjectRevision>(entity =>
        {
            entity.ToTable("SchoolBotProjectRevision");

            entity.Property(e => e.SchoolBotProjectRevisionId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Content).HasColumnType("jsonb");
            entity.Property(e => e.ContentHash).HasMaxLength(64);
        });

        modelBuilder.Entity<SchoolBotQuestionBank>(entity =>
        {
            entity.ToTable("SchoolBotQuestionBank");

            entity.Property(e => e.SchoolBotQuestionBankId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Lifecycle).HasMaxLength(16);
            entity.Property(e => e.Questions).HasColumnType("jsonb");
            entity.Property(e => e.SourceRef).HasMaxLength(2048);
            entity.Property(e => e.Title).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotRelease>(entity =>
        {
            entity.ToTable("SchoolBotRelease");

            entity.Property(e => e.SchoolBotReleaseId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ArtifactHash).HasMaxLength(64);
            entity.Property(e => e.Channel).HasMaxLength(16);
            entity.Property(e => e.Lifecycle).HasMaxLength(16);
            entity.Property(e => e.Manifest).HasMaxLength(16384);
            entity.Property(e => e.Platform).HasMaxLength(32);
            entity.Property(e => e.VersionText).HasMaxLength(32);
        });

        modelBuilder.Entity<SchoolBotRetakeGrant>(entity =>
        {
            entity.ToTable("SchoolBotRetakeGrant");

            entity.Property(e => e.SchoolBotRetakeGrantId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Reason).HasMaxLength(1000);
        });

        modelBuilder.Entity<SchoolBotScoreExport>(entity =>
        {
            entity.ToTable("SchoolBotScoreExport");

            entity.Property(e => e.SchoolBotScoreExportId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ConfirmedBy).HasMaxLength(512);
            entity.Property(e => e.State).HasMaxLength(32);
        });

        modelBuilder.Entity<SchoolBotScoreMapping>(entity =>
        {
            entity.ToTable("SchoolBotScoreMapping");

            entity.Property(e => e.SchoolBotScoreMappingId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Sources).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotScoreOutbox>(entity =>
        {
            entity.ToTable("SchoolBotScoreOutbox");

            entity.Property(e => e.SchoolBotScoreOutboxId).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastErrorCode).HasMaxLength(64);
            entity.Property(e => e.LeaseOwner).HasMaxLength(128);
            entity.Property(e => e.PayloadHash).HasMaxLength(64);
            entity.Property(e => e.ReceiptHash).HasMaxLength(64);
            entity.Property(e => e.State).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotScorePreview>(entity =>
        {
            entity.ToTable("SchoolBotScorePreview");

            entity.Property(e => e.SchoolBotScorePreviewId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Context).HasColumnType("jsonb");
            entity.Property(e => e.Principal).HasMaxLength(512);
            entity.Property(e => e.SnapshotHash).HasMaxLength(64);
            entity.Property(e => e.State).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotScorePreviewItem>(entity =>
        {
            entity.ToTable("SchoolBotScorePreviewItem");

            entity.Property(e => e.SchoolBotScorePreviewItemId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ReasonCodes).HasColumnType("jsonb");
            entity.Property(e => e.Snapshot).HasColumnType("jsonb");
            entity.Property(e => e.State).HasMaxLength(16);
        });

        modelBuilder.Entity<SchoolBotScoringPolicy>(entity =>
        {
            entity.ToTable("SchoolBotScoringPolicy");

            entity.Property(e => e.SchoolBotScoringPolicyId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Policy).HasColumnType("jsonb");
        });

        modelBuilder.Entity<SchoolBotSession>(entity =>
        {
            entity.ToTable("SchoolBotSession");

            entity.Property(e => e.SchoolBotSessionId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Channel)
                .HasMaxLength(16)
                .HasDefaultValueSql("'desktop'::character varying");
            entity.Property(e => e.DeviceJkt).HasMaxLength(64);
            entity.Property(e => e.LegacyAdminId).HasMaxLength(64);
            entity.Property(e => e.ProfileKind).HasMaxLength(16);
            entity.Property(e => e.TokenHash).HasMaxLength(64);
        });

        modelBuilder.Entity<SchoolBotStaffRole>(entity =>
        {
            entity.ToTable("SchoolBotStaffRole");

            entity.Property(e => e.SchoolBotStaffRoleId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Role).HasMaxLength(32);
        });

        modelBuilder.Entity<SchoolBotSubmission>(entity =>
        {
            entity.ToTable("SchoolBotSubmission");

            entity.Property(e => e.SchoolBotSubmissionId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Evidence).HasColumnType("jsonb");
            entity.Property(e => e.State).HasMaxLength(24);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
