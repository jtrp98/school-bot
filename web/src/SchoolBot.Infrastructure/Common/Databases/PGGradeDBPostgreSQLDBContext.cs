using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SchoolBot.Infrastructure.Entities.PGGradeDB;

namespace SchoolBot.Infrastructure.Common.Databases;

public partial class PGGradeDBPostgreSQLDBContext : DbContext
{
    public PGGradeDBPostgreSQLDBContext(DbContextOptions<PGGradeDBPostgreSQLDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Col> Cols { get; set; }

    public virtual DbSet<CompassLevelSubjectStat> CompassLevelSubjectStats { get; set; }

    public virtual DbSet<CompassMbtiResult> CompassMbtiResults { get; set; }

    public virtual DbSet<CompassStudentLevelScore> CompassStudentLevelScores { get; set; }

    public virtual DbSet<PgtacademicSetting> PgtacademicSettings { get; set; }

    public virtual DbSet<PgtacademicYearlySetting> PgtacademicYearlySettings { get; set; }

    public virtual DbSet<Pgtassessment> Pgtassessments { get; set; }

    public virtual DbSet<PgtassessmentBackup> PgtassessmentBackups { get; set; }

    public virtual DbSet<PgtassessmentHistory> PgtassessmentHistories { get; set; }

    public virtual DbSet<PgtassessmentTopic> PgtassessmentTopics { get; set; }

    public virtual DbSet<PgtassessmentTopicsSetting> PgtassessmentTopicsSettings { get; set; }

    public virtual DbSet<PgtassessmentType> PgtassessmentTypes { get; set; }

    public virtual DbSet<PgtdowntimeAnnouncement> PgtdowntimeAnnouncements { get; set; }

    public virtual DbSet<Pgtgrade> Pgtgrades { get; set; }

    public virtual DbSet<PgtgradeAuditLog> PgtgradeAuditLogs { get; set; }

    public virtual DbSet<PgtgradeBackup> PgtgradeBackups { get; set; }

    public virtual DbSet<PgtgradeBackup1> PgtgradeBackups1 { get; set; }

    public virtual DbSet<PgtgradeDetail> PgtgradeDetails { get; set; }

    public virtual DbSet<PgtgradeDetailBackup> PgtgradeDetailBackups { get; set; }

    public virtual DbSet<PgtgradeDetailDeletedRecordsBackup> PgtgradeDetailDeletedRecordsBackups { get; set; }

    public virtual DbSet<PgtgradeDetailHistory> PgtgradeDetailHistories { get; set; }

    public virtual DbSet<PgtgradeDetailRemoved> PgtgradeDetailRemoveds { get; set; }

    public virtual DbSet<PgtgradeEdit> PgtgradeEdits { get; set; }

    public virtual DbSet<PgtgradeEditsBackup> PgtgradeEditsBackups { get; set; }

    public virtual DbSet<PgtgradeEditsHistory> PgtgradeEditsHistories { get; set; }

    public virtual DbSet<PgtgradeHistory> PgtgradeHistories { get; set; }

    public virtual DbSet<PgtgradeRoomMoveLog> PgtgradeRoomMoveLogs { get; set; }

    public virtual DbSet<PgtgradeSetting> PgtgradeSettings { get; set; }

    public virtual DbSet<PgtgradeShareInfo> PgtgradeShareInfos { get; set; }

    public virtual DbSet<PgtgradeTemplate> PgtgradeTemplates { get; set; }

    public virtual DbSet<Pgtreportsetting> Pgtreportsettings { get; set; }

    public virtual DbSet<PgtstudentAcademicAssessment> PgtstudentAcademicAssessments { get; set; }

    public virtual DbSet<PgtstudentAcademicAssessmentScore> PgtstudentAcademicAssessmentScores { get; set; }

    public virtual DbSet<PgtstudentAcademicAssessmentTopic> PgtstudentAcademicAssessmentTopics { get; set; }

    public virtual DbSet<PgtstudentActivityAssessment> PgtstudentActivityAssessments { get; set; }

    public virtual DbSet<PgtstudentActivityAssessmentScore> PgtstudentActivityAssessmentScores { get; set; }

    public virtual DbSet<PgtstudentActivityAssessmentTopic> PgtstudentActivityAssessmentTopics { get; set; }

    public virtual DbSet<PgtstudentComment> PgtstudentComments { get; set; }

    public virtual DbSet<PgtstudentCompetencyAssessment> PgtstudentCompetencyAssessments { get; set; }

    public virtual DbSet<PgtstudentCompetencyAssessmentScore> PgtstudentCompetencyAssessmentScores { get; set; }

    public virtual DbSet<PgtstudentCompetencyAssessmentTopic> PgtstudentCompetencyAssessmentTopics { get; set; }

    public virtual DbSet<PgtstudentSubjectComment> PgtstudentSubjectComments { get; set; }

    public virtual DbSet<SearchByNgradeIdTemp> SearchByNgradeIdTemps { get; set; }

    public virtual DbSet<SearchBySidtemp> SearchBySidtemps { get; set; }

    public virtual DbSet<SqlqueryLog> SqlqueryLogs { get; set; }

    public virtual DbSet<StudentSrc> StudentSrcs { get; set; }

    public virtual DbSet<TempUser> TempUsers { get; set; }

    public virtual DbSet<Termcount> Termcounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_stat_statements")
            .HasPostgresExtension("pgaudit")
            .HasPostgresExtension("postgres_fdw");

        modelBuilder.Entity<Col>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("cols");

            entity.Property(e => e.StringAgg)
                .UseCollation("C")
                .HasColumnName("string_agg");
        });

        modelBuilder.Entity<CompassLevelSubjectStat>(entity =>
        {
            entity.HasKey(e => new { e.LevelCode, e.Subject }).HasName("compass_level_subject_stat_pkey");

            entity.ToTable("compass_level_subject_stat");

            entity.Property(e => e.LevelCode).HasColumnName("level_code");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.LevelName).HasColumnName("level_name");
            entity.Property(e => e.MeanScore).HasColumnName("mean_score");
            entity.Property(e => e.SchoolCount).HasColumnName("school_count");
            entity.Property(e => e.StudentCount).HasColumnName("student_count");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<CompassMbtiResult>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SId }).HasName("compass_mbti_result_pkey");

            entity.ToTable("compass_mbti_result");

            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SId).HasColumnName("s_id");
            entity.Property(e => e.Answers).HasColumnName("answers");
            entity.Property(e => e.CareerName).HasColumnName("career_name");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.MatchPercent).HasColumnName("match_percent");
            entity.Property(e => e.PathCode).HasColumnName("path_code");
            entity.Property(e => e.Reason).HasColumnName("reason");
        });

        modelBuilder.Entity<CompassStudentLevelScore>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SId }).HasName("compass_student_level_score_pkey");

            entity.ToTable("compass_student_level_score");

            entity.HasIndex(e => new { e.LevelCode, e.OverallAvg }, "ix_cslscore_level_avg");

            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.SId).HasColumnName("s_id");
            entity.Property(e => e.LevelCode).HasColumnName("level_code");
            entity.Property(e => e.OverallAvg).HasColumnName("overall_avg");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<PgtacademicSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PGTAcademicSetting_pkey");

            entity.ToTable("PGTAcademicSetting");

            entity.HasIndex(e => e.SchoolId, "idx_pgtacademicsetting_school_active").HasFilter("(\"IsDeleted\" = false)");

            entity.HasIndex(e => new { e.SchoolId, e.SettingKey }, "idx_pgtacademicsetting_school_setting_active").HasFilter("(\"IsDeleted\" = false)");

            entity.Property(e => e.SettingId).HasColumnName("SettingID");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtacademicYearlySetting>(entity =>
        {
            entity.HasKey(e => e.YearlySettingId).HasName("PGTAcademicYearlySetting_pkey");

            entity.ToTable("PGTAcademicYearlySetting");

            entity.Property(e => e.YearlySettingId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("YearlySettingID");
            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Pgtassessment>(entity =>
        {
            entity.HasKey(e => e.AssessmentId).HasName("PGTAssessment_pkey");

            entity
                .ToTable("PGTAssessment")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_analyze_scale_factor", "0.01")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_cost_limit", "1000")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_scale_factor", "0.02")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "80");

            entity.HasIndex(e => e.NGradeId, "IDX_nGradeId");

            entity.HasIndex(e => e.NGradeId, "IDX_nGradeId_ccnew");

            entity.HasIndex(e => new { e.SchoolId, e.NYear, e.NTerm, e.NTsubLevel, e.NTermSubLevel2, e.SPlaneId }, "IX_PGTAssessment_1");

            entity.HasIndex(e => new { e.SchoolId, e.NYear, e.NTerm, e.NTsubLevel, e.NTermSubLevel2, e.SPlaneId }, "IX_PGTAssessment_1_ccnew");

            entity.HasIndex(e => e.AssessmentId, "IX_PGTAssessment_AssessmentId");

            entity.HasIndex(e => e.AssessmentId, "IX_PGTAssessment_AssessmentId_ccnew");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId }, "IX_PGTAssessment_SchoolID_nGradeID");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId, e.AssessmentId }, "IX_PGTAssessment_SchoolID_nGradeID_AssessmentId");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId, e.AssessmentId }, "IX_PGTAssessment_SchoolID_nGradeID_AssessmentId_ccnew");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId }, "IX_PGTAssessment_SchoolID_nGradeID_ccnew");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId }, "PGTAssessment_SchoolID_nGradeID");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId }, "PGTAssessment_SchoolID_nGradeID_ccnew");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.NTermSubLevel2, e.SPlaneId }, "PGTAssessment_SchoolID_nTerm_nTermSublevel2_splaneID");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.NTermSubLevel2, e.SPlaneId }, "PGTAssessment_SchoolID_nTerm_nTermSublevel2_splaneID_ccnew");

            entity.HasIndex(e => e.AssessmentId, "PGTAssessment_pkey_ccnew").IsUnique();

            entity.HasIndex(e => e.AssessmentId, "PGTAssessment_pkey_ccnew1").IsUnique();

            entity.HasIndex(e => e.AssessmentId, "PGTAssessment_pkey_ccnew_ccnew").IsUnique();

            entity.HasIndex(e => new { e.SchoolId, e.MaxScoreIdentifier, e.NameIdentifier }, "idx_pgtassessment_partial").HasFilter("((\"cDel\" = false) AND (\"IsActive\" = true))");

            entity.HasIndex(e => new { e.SchoolId, e.MaxScoreIdentifier, e.NameIdentifier }, "idx_pgtassessment_partial_ccnew").HasFilter("((\"cDel\" = false) AND (\"IsActive\" = true))");

            entity.HasIndex(e => new { e.SchoolId, e.MaxScoreIdentifier, e.NameIdentifier }, "idx_pgtassessment_schoolid_maxscore_name");

            entity.HasIndex(e => new { e.SchoolId, e.MaxScoreIdentifier, e.NameIdentifier }, "idx_pgtassessment_schoolid_maxscore_name_ccnew");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId, e.AssessmentMaxScore }, "idx_schoolid_ngradeid_assmaxscore").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId, e.AssessmentMaxScore }, "idx_schoolid_ngradeid_assmaxscore_ccnew").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtassessmentBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTAssessment_Backup");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtassessmentHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTAssessmentHistory");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NTsubLevel).HasColumnName("nTSubLevel");
            entity.Property(e => e.NYear).HasColumnName("nYear");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtassessmentTopic>(entity =>
        {
            entity.HasKey(e => e.AssessmentTopicId);

            entity.ToTable("PGTAssessmentTopic");

            entity.Property(e => e.AssessmentTopicId).UseIdentityAlwaysColumn();
            entity.Property(e => e.AssessmentType).HasMaxLength(10);
        });

        modelBuilder.Entity<PgtassessmentTopicsSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PGTAssessmentTopicsSettings_pkey");

            entity.ToTable("PGTAssessmentTopicsSettings");

            entity.HasIndex(e => new { e.SchoolId, e.Nyear, e.AssessmentType, e.IsDeleted, e.SettingId }, "ix_pgtast_filter_sort");

            entity.Property(e => e.SettingId).HasColumnName("SettingID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Nyear).HasColumnName("NYear");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtassessmentType>(entity =>
        {
            entity.HasKey(e => e.AssessmentTypeId).HasName("PGTAssessmentType_pkey");

            entity.ToTable("PGTAssessmentType");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtdowntimeAnnouncement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PGTDowntimeAnnouncement_pkey");

            entity.ToTable("PGTDowntimeAnnouncement");

            entity.HasIndex(e => new { e.IsActive, e.EndTime, e.StartTime }, "ix_pgtdowntime_active_endtime_starttime");

            entity.Property(e => e.EndTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Pgtgrade>(entity =>
        {
            entity.HasKey(e => e.NGradeId).HasName("PGTGrade_pkey");

            entity.ToTable("PGTGrade");

            entity.HasIndex(e => new { e.SchoolId, e.CDel }, "INDEX_VV_406675_406674");

            entity.HasIndex(e => e.SchoolId, "INDEX_VV_SchoolID");

            entity.HasIndex(e => new { e.NGradeId, e.SchoolId }, "IX_TGrade");

            entity.HasIndex(e => new { e.SPlaneId, e.NTermSubLevel2, e.SchoolId }, "IX_TGrade_sPlaneID_nTermSubLevel2_SchoolID_8CF8A");

            entity.HasIndex(e => new { e.NTerm, e.SchoolId }, "TGrade_nterm_schoolid");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.NGradeId }, "idx_pg_grade_schoolid_nterm_ngradeid");

            entity.HasIndex(e => e.NGradeId, "idx_pg_grades_ngradeid");

            entity.HasIndex(e => e.SchoolId, "idx_pg_grades_schoolid");

            entity.HasIndex(e => e.SPlaneId, "idx_pg_grades_splaneid");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.SPlaneId, e.NGradeId, e.NTermSubLevel2 }, "idx_pgtgrade_common_joins");

            entity.HasIndex(e => new { e.SchoolId, e.PlanId, e.NTerm }, "idx_pgtgrade_schoolid_planid_term");

            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DAdd)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dAdd");
            entity.Property(e => e.DUpdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dUpdate");
            entity.Property(e => e.FRatioAfterMidTerm).HasColumnName("fRatioAfterMidTerm");
            entity.Property(e => e.FRatioBeforeMidTerm).HasColumnName("fRatioBeforeMidTerm");
            entity.Property(e => e.FRatioLateTerm).HasColumnName("fRatioLateTerm");
            entity.Property(e => e.FRatioMidTerm).HasColumnName("fRatioMidTerm");
            entity.Property(e => e.FRatioQuiz).HasColumnName("fRatioQuiz");
            entity.Property(e => e.FRatioQuizPass).HasColumnName("fRatioQuizPass");
            entity.Property(e => e.MaxAfterMidTerm).HasColumnName("maxAfterMidTerm");
            entity.Property(e => e.MaxBeforeMidTerm).HasColumnName("maxBeforeMidTerm");
            entity.Property(e => e.MaxBehaviorTotal).HasColumnName("maxBehaviorTotal");
            entity.Property(e => e.MaxFinalTerm).HasColumnName("maxFinalTerm");
            entity.Property(e => e.MaxGradeTotal).HasColumnName("maxGradeTotal");
            entity.Property(e => e.MaxMidTerm).HasColumnName("maxMidTerm");
            entity.Property(e => e.MaxReadWriteTotal).HasColumnName("maxReadWriteTotal");
            entity.Property(e => e.MaxSamattanaTotal).HasColumnName("maxSamattanaTotal");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NUserAdd).HasColumnName("nUserAdd");
            entity.Property(e => e.NUserUpdate).HasColumnName("nUserUpdate");
            entity.Property(e => e.OptionFinal).HasColumnName("optionFinal");
            entity.Property(e => e.OptionMid).HasColumnName("optionMid");
            entity.Property(e => e.ReportPrint).HasColumnName("reportPrint");
            entity.Property(e => e.SNote).HasColumnName("sNote");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SortNumber).HasColumnName("sortNumber");
            entity.Property(e => e.StudyFriday).HasColumnName("studyFriday");
            entity.Property(e => e.StudyMonday).HasColumnName("studyMonday");
            entity.Property(e => e.StudySaturday).HasColumnName("studySaturday");
            entity.Property(e => e.StudySunday).HasColumnName("studySunday");
            entity.Property(e => e.StudyThursday).HasColumnName("studyThursday");
            entity.Property(e => e.StudyTuesday).HasColumnName("studyTuesday");
            entity.Property(e => e.StudyWednesday).HasColumnName("studyWednesday");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeAuditLog>(entity =>
        {
            entity.HasKey(e => e.GradeAuditLogId).HasName("PGTGradeAuditLog_pkey");

            entity.ToTable("PGTGradeAuditLog");

            entity.Property(e => e.Logdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
        });

        modelBuilder.Entity<PgtgradeBackup>(entity =>
        {
            entity.HasKey(e => e.BackupId).HasName("PGTGradeBackup_pkey");

            entity.ToTable("PGTGradeBackup");

            entity.HasIndex(e => new { e.SchoolId, e.SourceNtermSubLevel2 }, "ix_grade_backup_room");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm }, "ix_grade_backup_school_term");

            entity.Property(e => e.BackupId).ValueGeneratedNever();
            entity.Property(e => e.BackupType).HasMaxLength(30);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DestinationNtermSubLevel2).HasColumnName("DestinationNTermSubLevel2");
            entity.Property(e => e.NTerm)
                .HasMaxLength(50)
                .HasColumnName("nTerm");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SourceNtermSubLevel2).HasColumnName("SourceNTermSubLevel2");
        });

        modelBuilder.Entity<PgtgradeBackup1>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTGrade_Backup");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DAdd)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dAdd");
            entity.Property(e => e.DUpdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dUpdate");
            entity.Property(e => e.FRatioAfterMidTerm).HasColumnName("fRatioAfterMidTerm");
            entity.Property(e => e.FRatioBeforeMidTerm).HasColumnName("fRatioBeforeMidTerm");
            entity.Property(e => e.FRatioLateTerm).HasColumnName("fRatioLateTerm");
            entity.Property(e => e.FRatioMidTerm).HasColumnName("fRatioMidTerm");
            entity.Property(e => e.FRatioQuiz).HasColumnName("fRatioQuiz");
            entity.Property(e => e.FRatioQuizPass).HasColumnName("fRatioQuizPass");
            entity.Property(e => e.MaxAfterMidTerm).HasColumnName("maxAfterMidTerm");
            entity.Property(e => e.MaxBeforeMidTerm).HasColumnName("maxBeforeMidTerm");
            entity.Property(e => e.MaxBehaviorTotal).HasColumnName("maxBehaviorTotal");
            entity.Property(e => e.MaxFinalTerm).HasColumnName("maxFinalTerm");
            entity.Property(e => e.MaxGradeTotal).HasColumnName("maxGradeTotal");
            entity.Property(e => e.MaxMidTerm).HasColumnName("maxMidTerm");
            entity.Property(e => e.MaxReadWriteTotal).HasColumnName("maxReadWriteTotal");
            entity.Property(e => e.MaxSamattanaTotal).HasColumnName("maxSamattanaTotal");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NUserAdd).HasColumnName("nUserAdd");
            entity.Property(e => e.NUserUpdate).HasColumnName("nUserUpdate");
            entity.Property(e => e.OptionFinal).HasColumnName("optionFinal");
            entity.Property(e => e.OptionMid).HasColumnName("optionMid");
            entity.Property(e => e.ReportPrint).HasColumnName("reportPrint");
            entity.Property(e => e.SNote).HasColumnName("sNote");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SortNumber).HasColumnName("sortNumber");
            entity.Property(e => e.StudyFriday).HasColumnName("studyFriday");
            entity.Property(e => e.StudyMonday).HasColumnName("studyMonday");
            entity.Property(e => e.StudySaturday).HasColumnName("studySaturday");
            entity.Property(e => e.StudySunday).HasColumnName("studySunday");
            entity.Property(e => e.StudyThursday).HasColumnName("studyThursday");
            entity.Property(e => e.StudyTuesday).HasColumnName("studyTuesday");
            entity.Property(e => e.StudyWednesday).HasColumnName("studyWednesday");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeDetail>(entity =>
        {
            entity.HasKey(e => e.NGradeDetailId).HasName("PGTGradeDetail_pkey");

            entity
                .ToTable("PGTGradeDetail")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_analyze_scale_factor", "0.01")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_cost_limit", "1000")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_scale_factor", "0.03")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "80");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.GetGradeLabel }, "INDEX_VV_406860_406859");

            entity.HasIndex(e => e.SId, "IX_PGTGradeDetail_2");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.SId }, "IX_VV_PGTGradeDetail_SchoolID_cDel_sID");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId, e.SId, e.CDel }, "IX_VV_PGTGradeDetail_SchoolID_nGradeID_cDel_sID");

            entity.HasIndex(e => e.NGradeDetailId, "PGTGradeDetail_pkey_ccnew").IsUnique();

            entity.HasIndex(e => e.CDel, "idx_pg_gradedetails_cdel");

            entity.HasIndex(e => e.NGradeId, "idx_pg_gradedetails_ngradeid");

            entity.HasIndex(e => new { e.SchoolId, e.SId, e.CDel }, "idx_pgtgradedetail_covering");

            entity.HasIndex(e => new { e.SchoolId, e.SId, e.NGradeDetailId }, "idx_pgtgradedetail_filter");

            entity.HasIndex(e => new { e.SchoolId, e.SId, e.NGradeDetailId }, "idx_pgtgradedetail_schoolid_sid_ngradedetailid");

            entity.Property(e => e.NGradeDetailId).HasColumnName("nGradeDetailId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetAfterQuiz100).HasColumnName("getAfterQuiz100");
            entity.Property(e => e.GetBeforeQuiz100).HasColumnName("getBeforeQuiz100");
            entity.Property(e => e.GetBehaviorLabel).HasColumnName("getBehaviorLabel");
            entity.Property(e => e.GetBehaviorTotal).HasColumnName("getBehaviorTotal");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetQuiz100).HasColumnName("getQuiz100");
            entity.Property(e => e.GetReadWrite).HasColumnName("getReadWrite");
            entity.Property(e => e.GetReadWriteTotal).HasColumnName("getReadWriteTotal");
            entity.Property(e => e.GetSamattana).HasColumnName("getSamattana");
            entity.Property(e => e.GetSamattanaTotal).HasColumnName("getSamattanaTotal");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GetSpecial).HasColumnName("getSpecial");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreBeforeAfterMidTerm).HasColumnName("scoreBeforeAfterMidTerm");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeDetailBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTGradeDetail_Backup");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetAfterQuiz100).HasColumnName("getAfterQuiz100");
            entity.Property(e => e.GetBeforeQuiz100).HasColumnName("getBeforeQuiz100");
            entity.Property(e => e.GetBehaviorLabel).HasColumnName("getBehaviorLabel");
            entity.Property(e => e.GetBehaviorTotal).HasColumnName("getBehaviorTotal");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetQuiz100).HasColumnName("getQuiz100");
            entity.Property(e => e.GetReadWrite).HasColumnName("getReadWrite");
            entity.Property(e => e.GetReadWriteTotal).HasColumnName("getReadWriteTotal");
            entity.Property(e => e.GetSamattana).HasColumnName("getSamattana");
            entity.Property(e => e.GetSamattanaTotal).HasColumnName("getSamattanaTotal");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GetSpecial).HasColumnName("getSpecial");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.NGradeDetailId).HasColumnName("nGradeDetailId");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreBeforeAfterMidTerm).HasColumnName("scoreBeforeAfterMidTerm");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeDetailDeletedRecordsBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTGradeDetail_DeletedRecords_Backup");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetAfterQuiz100).HasColumnName("getAfterQuiz100");
            entity.Property(e => e.GetBeforeQuiz100).HasColumnName("getBeforeQuiz100");
            entity.Property(e => e.GetBehaviorLabel).HasColumnName("getBehaviorLabel");
            entity.Property(e => e.GetBehaviorTotal).HasColumnName("getBehaviorTotal");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetQuiz100).HasColumnName("getQuiz100");
            entity.Property(e => e.GetReadWrite).HasColumnName("getReadWrite");
            entity.Property(e => e.GetReadWriteTotal).HasColumnName("getReadWriteTotal");
            entity.Property(e => e.GetSamattana).HasColumnName("getSamattana");
            entity.Property(e => e.GetSamattanaTotal).HasColumnName("getSamattanaTotal");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GetSpecial).HasColumnName("getSpecial");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.NGradeDetailId).HasColumnName("nGradeDetailId");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreBeforeAfterMidTerm).HasColumnName("scoreBeforeAfterMidTerm");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeDetailHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTGradeDetailHistory");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetAfterQuiz100).HasColumnName("getAfterQuiz100");
            entity.Property(e => e.GetBeforeQuiz100).HasColumnName("getBeforeQuiz100");
            entity.Property(e => e.GetBehaviorLabel).HasColumnName("getBehaviorLabel");
            entity.Property(e => e.GetBehaviorTotal).HasColumnName("getBehaviorTotal");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetQuiz100).HasColumnName("getQuiz100");
            entity.Property(e => e.GetReadWrite).HasColumnName("getReadWrite");
            entity.Property(e => e.GetReadWriteTotal).HasColumnName("getReadWriteTotal");
            entity.Property(e => e.GetSamattana).HasColumnName("getSamattana");
            entity.Property(e => e.GetSamattanaTotal).HasColumnName("getSamattanaTotal");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GetSpecial).HasColumnName("getSpecial");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.NGradeDetailId).HasColumnName("nGradeDetailId");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreBeforeAfterMidTerm).HasColumnName("scoreBeforeAfterMidTerm");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeDetailRemoved>(entity =>
        {
            entity.HasKey(e => e.NGradeDetailId).HasName("PGTGradeDetailRemoved_pkey");

            entity.ToTable("PGTGradeDetailRemoved");

            entity.HasIndex(e => e.NGradeDetailId, "INDEX_VV_Removed_379553_379552");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.SId }, "INDEX_VV_Removed_406724_406723");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.GetGradeLabel }, "INDEX_VV_Removed_406860_406859");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId, e.SId, e.CDel }, "IX_PGTGradeDetailRemovedParam");

            entity.HasIndex(e => e.SId, "IX_PGTGradeDetailRemoved_2");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.SId }, "IX_VV_PGTGradeDetailRemoved_SchoolID_cDel_sID");

            entity.HasIndex(e => new { e.SchoolId, e.NGradeId, e.SId, e.CDel }, "IX_VV_PGTGradeDetailRemoved_SchoolID_nGradeID_cDel_sID");

            entity.HasIndex(e => e.NGradeDetailId, "PGTGradeDetailRemoved_pkey_ccnew").IsUnique();

            entity.Property(e => e.NGradeDetailId)
                .ValueGeneratedNever()
                .HasColumnName("nGradeDetailId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetAfterQuiz100).HasColumnName("getAfterQuiz100");
            entity.Property(e => e.GetBeforeQuiz100).HasColumnName("getBeforeQuiz100");
            entity.Property(e => e.GetBehaviorLabel).HasColumnName("getBehaviorLabel");
            entity.Property(e => e.GetBehaviorTotal).HasColumnName("getBehaviorTotal");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetQuiz100).HasColumnName("getQuiz100");
            entity.Property(e => e.GetReadWrite).HasColumnName("getReadWrite");
            entity.Property(e => e.GetReadWriteTotal).HasColumnName("getReadWriteTotal");
            entity.Property(e => e.GetSamattana).HasColumnName("getSamattana");
            entity.Property(e => e.GetSamattanaTotal).HasColumnName("getSamattanaTotal");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GetSpecial).HasColumnName("getSpecial");
            entity.Property(e => e.NCredit).HasColumnName("nCredit");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreBeforeAfterMidTerm).HasColumnName("scoreBeforeAfterMidTerm");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeEdit>(entity =>
        {
            entity.HasKey(e => e.GradeEditsId).HasName("PGTGradeEdits_pkey");

            entity.ToTable("PGTGradeEdits");

            entity.HasIndex(e => new { e.GradeEditsId, e.GradeDetailId }, "INDEX_VV_370674_370673");

            entity.HasIndex(e => new { e.GradeDetailId, e.UseGradeSet }, "idx_pgtgradeedits_common_join");

            entity.HasIndex(e => e.GradeDetailId, "idx_pgtgradeedits_gradedetailid");

            entity.HasIndex(e => e.UseGradeSet, "idx_pgtgradeedits_usegradeset");

            entity.Property(e => e.GradeEditsId).HasColumnName("GradeEditsID");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DateUpdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GetSpecial).HasColumnName("getSpecial");
            entity.Property(e => e.GradeDetailId).HasColumnName("GradeDetailID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeEditsBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTGradeEdits_Backup");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DateUpdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GetSpecial).HasColumnName("getSpecial");
            entity.Property(e => e.GradeDetailId).HasColumnName("GradeDetailID");
            entity.Property(e => e.GradeEditsId).HasColumnName("GradeEditsID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeEditsHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTGradeEditsHistory");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DateUpdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GetFinal100).HasColumnName("getFinal100");
            entity.Property(e => e.GetGradeLabel).HasColumnName("getGradeLabel");
            entity.Property(e => e.GetMid100).HasColumnName("getMid100");
            entity.Property(e => e.GetScore100).HasColumnName("getScore100");
            entity.Property(e => e.GradeDetailId).HasColumnName("GradeDetailID");
            entity.Property(e => e.GradeEditsId).HasColumnName("GradeEditsID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ScoreFinalTerm).HasColumnName("scoreFinalTerm");
            entity.Property(e => e.ScoreMidTerm).HasColumnName("scoreMidTerm");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PGTGradeHistory");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DAdd)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dAdd");
            entity.Property(e => e.DUpdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dUpdate");
            entity.Property(e => e.FRatioAfterMidTerm).HasColumnName("fRatioAfterMidTerm");
            entity.Property(e => e.FRatioBeforeMidTerm).HasColumnName("fRatioBeforeMidTerm");
            entity.Property(e => e.FRatioLateTerm).HasColumnName("fRatioLateTerm");
            entity.Property(e => e.FRatioMidTerm).HasColumnName("fRatioMidTerm");
            entity.Property(e => e.FRatioQuiz).HasColumnName("fRatioQuiz");
            entity.Property(e => e.FRatioQuizPass).HasColumnName("fRatioQuizPass");
            entity.Property(e => e.MaxAfterMidTerm).HasColumnName("maxAfterMidTerm");
            entity.Property(e => e.MaxBeforeMidTerm).HasColumnName("maxBeforeMidTerm");
            entity.Property(e => e.MaxBehaviorTotal).HasColumnName("maxBehaviorTotal");
            entity.Property(e => e.MaxFinalTerm).HasColumnName("maxFinalTerm");
            entity.Property(e => e.MaxGradeTotal).HasColumnName("maxGradeTotal");
            entity.Property(e => e.MaxMidTerm).HasColumnName("maxMidTerm");
            entity.Property(e => e.MaxReadWriteTotal).HasColumnName("maxReadWriteTotal");
            entity.Property(e => e.MaxSamattanaTotal).HasColumnName("maxSamattanaTotal");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.NTermSubLevel2).HasColumnName("nTermSubLevel2");
            entity.Property(e => e.NUserAdd).HasColumnName("nUserAdd");
            entity.Property(e => e.NUserUpdate).HasColumnName("nUserUpdate");
            entity.Property(e => e.OptionFinal).HasColumnName("optionFinal");
            entity.Property(e => e.OptionMid).HasColumnName("optionMid");
            entity.Property(e => e.ReportPrint).HasColumnName("reportPrint");
            entity.Property(e => e.SNote).HasColumnName("sNote");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SortNumber).HasColumnName("sortNumber");
            entity.Property(e => e.StudyFriday).HasColumnName("studyFriday");
            entity.Property(e => e.StudyMonday).HasColumnName("studyMonday");
            entity.Property(e => e.StudySaturday).HasColumnName("studySaturday");
            entity.Property(e => e.StudySunday).HasColumnName("studySunday");
            entity.Property(e => e.StudyThursday).HasColumnName("studyThursday");
            entity.Property(e => e.StudyTuesday).HasColumnName("studyTuesday");
            entity.Property(e => e.StudyWednesday).HasColumnName("studyWednesday");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeRoomMoveLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PGTGradeRoomMoveLog_pkey");

            entity.ToTable("PGTGradeRoomMoveLog");

            entity.HasIndex(e => e.RequestId, "ix_grade_move_log_req");

            entity.HasIndex(e => new { e.SchoolId, e.SId, e.NTerm }, "ix_grade_move_log_student");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DestinationNgradeId).HasColumnName("DestinationNGradeId");
            entity.Property(e => e.DestinationNtermSubLevel2).HasColumnName("DestinationNTermSubLevel2");
            entity.Property(e => e.NTerm)
                .HasMaxLength(50)
                .HasColumnName("nTerm");
            entity.Property(e => e.NgradeDetailId).HasColumnName("NGradeDetailId");
            entity.Property(e => e.Reason).HasMaxLength(50);
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SourceNgradeId).HasColumnName("SourceNGradeId");
            entity.Property(e => e.SourceNtermSubLevel2).HasColumnName("SourceNTermSubLevel2");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<PgtgradeSetting>(entity =>
        {
            entity.HasKey(e => e.GradeSettingId).HasName("PGGradeSetting_pkey");

            entity.ToTable("PGTGradeSetting");

            entity.Property(e => e.GradeSettingId).HasDefaultValueSql("nextval('\"PGGradeSetting_GradeSettingId_seq\"'::regclass)");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.ClassLevelName).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NumberYear).HasMaxLength(10);
            entity.Property(e => e.Nyear).HasColumnName("NYear");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SettingMode)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Separate'::character varying");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeShareInfo>(entity =>
        {
            entity.HasKey(e => e.NGradeShareInfoId).HasName("PGTGradeShareInfo_pkey");

            entity.ToTable("PGTGradeShareInfo");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.FromNTsubLevel2, e.SPlaneId }, "idx_pgtgradeshareinfo_from_lookup").HasFilter("(\"cDel\" IS NULL)");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.FromNTsubLevel2, e.ToNTsubLevel2, e.SPlaneId }, "idx_pgtgradeshareinfo_full_lookup");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.FromNTsubLevel2, e.ToNTsubLevel2, e.SPlaneId }, "idx_pgtgradeshareinfo_full_lookup_active").HasFilter("(\"cDel\" IS NULL)");

            entity.HasIndex(e => new { e.SchoolId, e.NTerm, e.ToNTsubLevel2, e.SPlaneId }, "idx_pgtgradeshareinfo_lookup").HasFilter("(\"cDel\" IS NULL)");

            entity.HasIndex(e => new { e.NTerm, e.SPlaneId, e.FromNTsubLevel2, e.SchoolId }, "idx_pgtgradeshareinfo_term_plane_from_sublevel_school").HasFilter("(\"cDel\" IS NULL)");

            entity.HasIndex(e => new { e.NTerm, e.SPlaneId, e.ToNTsubLevel2, e.SchoolId }, "idx_pgtgradeshareinfo_term_plane_to_sublevel_school").HasFilter("(\"cDel\" IS NULL)");

            entity.Property(e => e.NGradeShareInfoId).HasColumnName("nGradeShareInfoId");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FromNTsubLevel2).HasColumnName("from_nTSubLevel2");
            entity.Property(e => e.NTerm).HasColumnName("nTerm");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ToNTsubLevel2).HasColumnName("to_nTSubLevel2");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtgradeTemplate>(entity =>
        {
            entity.HasKey(e => e.GradeTemplateId).HasName("PGGradeTemplate_pkey");

            entity.ToTable("PGTGradeTemplate");

            entity.Property(e => e.GradeTemplateId).HasDefaultValueSql("nextval('\"PGGradeTemplate_GradeTemplateId_seq\"'::regclass)");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Pgtreportsetting>(entity =>
        {
            entity.HasKey(e => e.Reportsettingid).HasName("pgtreportsettings_pkey");

            entity.ToTable("pgtreportsettings");

            entity.Property(e => e.Reportsettingid).HasColumnName("reportsettingid");
            entity.Property(e => e.Cdel)
                .HasDefaultValue(false)
                .HasColumnName("cdel");
            entity.Property(e => e.Classlevelid).HasColumnName("classlevelid");
            entity.Property(e => e.Classlevelname)
                .HasMaxLength(255)
                .HasColumnName("classlevelname");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Numberyear)
                .HasMaxLength(10)
                .HasColumnName("numberyear");
            entity.Property(e => e.Nyear).HasColumnName("nyear");
            entity.Property(e => e.Reporttype)
                .HasMaxLength(25)
                .HasColumnName("reporttype");
            entity.Property(e => e.Reporttypeid).HasColumnName("reporttypeid");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Settingname)
                .HasMaxLength(255)
                .HasColumnName("settingname");
            entity.Property(e => e.Settings).HasColumnName("settings");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Updatedby).HasColumnName("updatedby");
        });

        modelBuilder.Entity<PgtstudentAcademicAssessment>(entity =>
        {
            entity.HasKey(e => e.StudentAcademicAssessmentId);

            entity.ToTable("PGTStudentAcademicAssessment");

            entity.Property(e => e.StudentAcademicAssessmentId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ContinuousAssessmentMaxScore).HasPrecision(12, 2);
            entity.Property(e => e.FinalTermMaxScore).HasPrecision(12, 2);
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Nterm)
                .HasMaxLength(20)
                .HasColumnName("NTerm");
            entity.Property(e => e.NtermSubLevel2).HasColumnName("NTermSubLevel2");
            entity.Property(e => e.NtsubLevel).HasColumnName("NTSubLevel");
            entity.Property(e => e.Nyear).HasColumnName("NYear");
            entity.Property(e => e.SplaneId).HasColumnName("SPlaneId");
            entity.Property(e => e.TotalMaxScore).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentAcademicAssessmentScore>(entity =>
        {
            entity.HasKey(e => e.StudentAcademicAssessmentScoreId);

            entity.ToTable("PGTStudentAcademicAssessmentScore");

            entity.Property(e => e.StudentAcademicAssessmentScoreId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ContinuousAssessmentScore).HasPrecision(12, 2);
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FinalTermScore).HasPrecision(12, 2);
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ScoreData).HasColumnType("jsonb");
            entity.Property(e => e.Sid).HasColumnName("SId");
            entity.Property(e => e.TotalScore).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentAcademicAssessmentTopic>(entity =>
        {
            entity.HasKey(e => e.StudentAcademicAssessmentTopicId);

            entity.ToTable("PGTStudentAcademicAssessmentTopic");

            entity.Property(e => e.StudentAcademicAssessmentTopicId).UseIdentityAlwaysColumn();
            entity.Property(e => e.AssessmentCategoryCode).HasMaxLength(2);
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentActivityAssessment>(entity =>
        {
            entity.HasKey(e => e.StudentActivityAssessmentId);

            entity.ToTable("PGTStudentActivityAssessment");

            entity.Property(e => e.StudentActivityAssessmentId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ActivityMaxScore).HasPrecision(12, 2);
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Nterm)
                .HasMaxLength(20)
                .HasColumnName("NTerm");
            entity.Property(e => e.NtermSubLevel2).HasColumnName("NTermSubLevel2");
            entity.Property(e => e.NtsubLevel).HasColumnName("NTSubLevel");
            entity.Property(e => e.Nyear).HasColumnName("NYear");
            entity.Property(e => e.SplaneId).HasColumnName("SPlaneId");
            entity.Property(e => e.TotalMaxScore).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentActivityAssessmentScore>(entity =>
        {
            entity.HasKey(e => e.StudentActivityAssessmentScoreId).HasName("PK_PGStudentActivityAssessmentScore");

            entity.ToTable("PGTStudentActivityAssessmentScore");

            entity.Property(e => e.StudentActivityAssessmentScoreId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ActivityScore).HasPrecision(12, 2);
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.EvaluationResult).HasMaxLength(1);
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ScoreData).HasColumnType("jsonb");
            entity.Property(e => e.Sid).HasColumnName("SId");
            entity.Property(e => e.TotalScore).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentActivityAssessmentTopic>(entity =>
        {
            entity.HasKey(e => e.StudentActivityAssessmentTopicId);

            entity.ToTable("PGTStudentActivityAssessmentTopic");

            entity.Property(e => e.StudentActivityAssessmentTopicId).UseIdentityAlwaysColumn();
            entity.Property(e => e.AssessmentCategoryCode).HasMaxLength(2);
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PGTStudentComment_pkey");

            entity.ToTable("PGTStudentComment");

            entity.HasIndex(e => new { e.SchoolId, e.Nterm, e.SId }, "idx_pgtstudentcomment_lookup").HasFilter("(\"cDel\" IS DISTINCT FROM true)");

            entity.HasIndex(e => new { e.SchoolId, e.Nterm, e.CDel }, "idx_pgtstudentcomment_school_term_cdel");

            entity.HasIndex(e => new { e.SchoolId, e.Nterm }, "idx_pgtstudentcomment_school_term_cdel_null").HasFilter("(\"cDel\" IS NULL)");

            entity.HasIndex(e => new { e.SchoolId, e.Nterm }, "idx_pgtstudentcomment_school_term_roletype_null").HasFilter("(\"RoleType\" IS NULL)");

            entity.HasIndex(e => new { e.SchoolId, e.Nterm }, "idx_pgtstudentcomment_supervisor").HasFilter("(\"cDel\" IS DISTINCT FROM true)");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CommentType).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentCompetencyAssessment>(entity =>
        {
            entity.HasKey(e => e.StudentCompetencyAssessmentId);

            entity.ToTable("PGTStudentCompetencyAssessment");

            entity.Property(e => e.StudentCompetencyAssessmentId).UseIdentityAlwaysColumn();
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Nterm)
                .HasMaxLength(20)
                .HasColumnName("NTerm");
            entity.Property(e => e.NtermSubLevel2).HasColumnName("NTermSubLevel2");
            entity.Property(e => e.NtsubLevel).HasColumnName("NTSubLevel");
            entity.Property(e => e.Nyear).HasColumnName("NYear");
        });

        modelBuilder.Entity<PgtstudentCompetencyAssessmentScore>(entity =>
        {
            entity.HasKey(e => e.StudentCompetencyAssessmentScoreId);

            entity.ToTable("PGTStudentCompetencyAssessmentScore");

            entity.Property(e => e.StudentCompetencyAssessmentScoreId).UseIdentityAlwaysColumn();
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ScoreData).HasColumnType("jsonb");
            entity.Property(e => e.Sid).HasColumnName("SId");
            entity.Property(e => e.UpdatedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentCompetencyAssessmentTopic>(entity =>
        {
            entity.HasKey(e => e.StudentCompetencyAssessmentTopicId);

            entity.ToTable("PGTStudentCompetencyAssessmentTopic");

            entity.Property(e => e.StudentCompetencyAssessmentTopicId).UseIdentityAlwaysColumn();
            entity.Property(e => e.DeletedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.InsertedDateTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PgtstudentSubjectComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PGTStudentSubjectComment_pkey");

            entity.ToTable("PGTStudentSubjectComment");

            entity.HasIndex(e => new { e.SchoolId, e.CDel, e.Nterm, e.SPlaneId, e.SId }, "ix_pgt_studsubjcomment_filters_sid");

            entity.Property(e => e.CDel)
                .HasDefaultValue(false)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SPlaneId).HasColumnName("sPlaneID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<SearchByNgradeIdTemp>(entity =>
        {
            entity.HasKey(e => e.NGradeId);

            entity.ToTable("SearchByNGradeIdTemp");

            entity.Property(e => e.NGradeId)
                .ValueGeneratedNever()
                .HasColumnName("nGradeId");
        });

        modelBuilder.Entity<SearchBySidtemp>(entity =>
        {
            entity.HasKey(e => e.SId);

            entity.ToTable("SearchBySIDTemp");

            entity.Property(e => e.SId)
                .ValueGeneratedNever()
                .HasColumnName("sID");
        });

        modelBuilder.Entity<SqlqueryLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("SQLQueryLogs");

            entity.Property(e => e.ApplicationName).HasColumnName("application_name");
            entity.Property(e => e.BackendStart).HasColumnName("backend_start");
            entity.Property(e => e.ClientAddr).HasColumnName("client_addr");
            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.QueryDuration).HasColumnName("query_duration");
            entity.Property(e => e.QueryStart).HasColumnName("query_start");
            entity.Property(e => e.QueryText).HasColumnName("query_text");
            entity.Property(e => e.State).HasColumnName("state");
        });

        modelBuilder.Entity<StudentSrc>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("student_src");

            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.NstudentNumber).HasColumnName("NStudentNumber");
            entity.Property(e => e.NstudentStatus).HasColumnName("NStudentStatus");
            entity.Property(e => e.Nterm).HasColumnName("NTerm");
            entity.Property(e => e.Sid).HasColumnName("SId");
            entity.Property(e => e.Slastname).HasColumnName("SLastname");
            entity.Property(e => e.Sname).HasColumnName("SName");
            entity.Property(e => e.SstudentId).HasColumnName("SStudentID");
            entity.Property(e => e.SstudentLastEn).HasColumnName("SStudentLastEn");
            entity.Property(e => e.SstudentNameEn).HasColumnName("SStudentNameEn");
            entity.Property(e => e.SstudentTitle).HasColumnName("SStudentTitle");
            entity.Property(e => e.Sterm).HasColumnName("STerm");
        });

        modelBuilder.Entity<TempUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tempUsers");
        });

        modelBuilder.Entity<Termcount>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("termcount");

            entity.Property(e => e.Count).HasColumnName("count");
        });
        modelBuilder.HasSequence("PGTAssessment_AssessmentId_seq")
            .HasMin(108500001L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("PGTAssessmentType_AssessmentTypeId_seq")
            .HasMin(12200L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("PGTGrade_nGradeId_seq")
            .HasMin(2015000L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("PGTGradeAuditLog_GradeAuditLogId_seq").HasMin(6500000L);
        modelBuilder.HasSequence("PGTGradeDetail_nGradeDetailId_seq")
            .HasMin(90150000L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("PGTGradeEdits_GradeEditsID_seq")
            .HasMin(120200L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("PGTGradeShareInfo_nGradeShareInfoId_seq").HasMin(90000L);
        modelBuilder.HasSequence("PGTStudentComment_CommentId_seq").HasMin(15000L);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
