using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

namespace ServiceSchoolBot.Infrastructure.Common.Databases;

public partial class JabjaiSchoolLogsPostgreSQLDBContext : DbContext
{
    public JabjaiSchoolLogsPostgreSQLDBContext(DbContextOptions<JabjaiSchoolLogsPostgreSQLDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AlertNotification> AlertNotifications { get; set; }

    public virtual DbSet<ApiCallLog> ApiCallLogs { get; set; }

    public virtual DbSet<ApiLog> ApiLogs { get; set; }

    public virtual DbSet<ApiLogsAcademic> ApiLogsAcademics { get; set; }

    public virtual DbSet<ApiLogsHistory> ApiLogsHistories { get; set; }

    public virtual DbSet<ApiLogsNotification> ApiLogsNotifications { get; set; }

    public virtual DbSet<Apilogstemp> Apilogstemps { get; set; }

    public virtual DbSet<Apiresponse> Apiresponses { get; set; }

    public virtual DbSet<ApplicationLog> ApplicationLogs { get; set; }

    public virtual DbSet<BoatUserId> BoatUserIds { get; set; }

    public virtual DbSet<DailySalesCount> DailySalesCounts { get; set; }

    public virtual DbSet<DeviceActivityLog> DeviceActivityLogs { get; set; }

    public virtual DbSet<DeviceDetail> DeviceDetails { get; set; }

    public virtual DbSet<DeviceUpdateStatus> DeviceUpdateStatuses { get; set; }

    public virtual DbSet<ErrorId> ErrorIds { get; set; }

    public virtual DbSet<KtbapprovalTransaction> KtbapprovalTransactions { get; set; }

    public virtual DbSet<KtbpaymentTransaction> KtbpaymentTransactions { get; set; }

    public virtual DbSet<LogTrace> LogTraces { get; set; }

    public virtual DbSet<ModifiedShop> ModifiedShops { get; set; }

    public virtual DbSet<ModifiedUser> ModifiedUsers { get; set; }

    public virtual DbSet<OffTsellHistoryNew> OffTsellHistoryNews { get; set; }

    public virtual DbSet<OffTsellNew> OffTsellNews { get; set; }

    public virtual DbSet<OfflineSale> OfflineSales { get; set; }

    public virtual DbSet<OfflineSalesBackup> OfflineSalesBackups { get; set; }

    public virtual DbSet<OnlineScanWorkerLog> OnlineScanWorkerLogs { get; set; }

    public virtual DbSet<PaymentServersDetail> PaymentServersDetails { get; set; }

    public virtual DbSet<PromptPayTsellDetail> PromptPayTsellDetails { get; set; }

    public virtual DbSet<SalesDatum> SalesData { get; set; }

    public virtual DbSet<ScanReconciliationResult> ScanReconciliationResults { get; set; }

    public virtual DbSet<SchedulesLog> SchedulesLogs { get; set; }

    public virtual DbSet<ServiceCallLog> ServiceCallLogs { get; set; }

    public virtual DbSet<TacademicLog> TacademicLogs { get; set; }

    public virtual DbSet<TacademicLogHistory> TacademicLogHistories { get; set; }

    public virtual DbSet<TacademicPerfLog> TacademicPerfLogs { get; set; }

    public virtual DbSet<TapirequestResponse> TapirequestResponses { get; set; }

    public virtual DbSet<TattendanceConfigChangeLog> TattendanceConfigChangeLogs { get; set; }

    public virtual DbSet<TauditDailyRollup> TauditDailyRollups { get; set; }

    public virtual DbSet<TauditFinding> TauditFindings { get; set; }

    public virtual DbSet<TbBehaviorJob> TbBehaviorJobs { get; set; }

    public virtual DbSet<TbResourceUsageLog> TbResourceUsageLogs { get; set; }

    public virtual DbSet<TbSendApiLog> TbSendApiLogs { get; set; }

    public virtual DbSet<TbSendApiLogHistory> TbSendApiLogHistories { get; set; }

    public virtual DbSet<TdbjobLog> TdbjobLogs { get; set; }

    public virtual DbSet<TdeviceRequestLog> TdeviceRequestLogs { get; set; }

    public virtual DbSet<TempWatchdoglog> TempWatchdoglogs { get; set; }

    public virtual DbSet<TfcmdeliveryAck> TfcmdeliveryAcks { get; set; }

    public virtual DbSet<TfcmsendLog> TfcmsendLogs { get; set; }

    public virtual DbSet<TfcmsendRequestLog> TfcmsendRequestLogs { get; set; }

    public virtual DbSet<TgradeAuditLog> TgradeAuditLogs { get; set; }

    public virtual DbSet<TgradeAuditLogHistory> TgradeAuditLogHistories { get; set; }

    public virtual DbSet<TgradeAuditLogHistoryBackup> TgradeAuditLogHistoryBackups { get; set; }

    public virtual DbSet<TgradeDetailAuditLog> TgradeDetailAuditLogs { get; set; }

    public virtual DbSet<TgradeDetailIssueLog> TgradeDetailIssueLogs { get; set; }

    public virtual DbSet<ThardwareScanLog> ThardwareScanLogs { get; set; }

    public virtual DbSet<TjobConfig> TjobConfigs { get; set; }

    public virtual DbSet<TjobConfigHistory> TjobConfigHistories { get; set; }

    public virtual DbSet<TjobRunLog> TjobRunLogs { get; set; }

    public virtual DbSet<Tlog> Tlogs { get; set; }

    public virtual DbSet<TmanualPayment> TmanualPayments { get; set; }

    public virtual DbSet<TmessageBoxDberror> TmessageBoxDberrors { get; set; }

    public virtual DbSet<TmoneyDberror> TmoneyDberrors { get; set; }

    public virtual DbSet<TmoneyDberrorBkup> TmoneyDberrorBkups { get; set; }

    public virtual DbSet<TmpApiresponse> TmpApiresponses { get; set; }

    public virtual DbSet<TschoolFeatureFlag> TschoolFeatureFlags { get; set; }

    public virtual DbSet<TsellApi> TsellApis { get; set; }

    public virtual DbSet<TsellDberror> TsellDberrors { get; set; }

    public virtual DbSet<TsellDetailDberror> TsellDetailDberrors { get; set; }

    public virtual DbSet<TsweepLog> TsweepLogs { get; set; }

    public virtual DbSet<TsystemLogHistoryBackup> TsystemLogHistoryBackups { get; set; }

    public virtual DbSet<Tsystemlog> Tsystemlogs { get; set; }

    public virtual DbSet<UpdatedShop> UpdatedShops { get; set; }

    public virtual DbSet<UpdatedUser> UpdatedUsers { get; set; }

    public virtual DbSet<UserApiresponse> UserApiresponses { get; set; }

    public virtual DbSet<UserApisummary> UserApisummaries { get; set; }

    public virtual DbSet<WatchdogLog> WatchdogLogs { get; set; }

    public virtual DbSet<WatchdogWatchexceptionlog> WatchdogWatchexceptionlogs { get; set; }

    public virtual DbSet<WatchdogWatchlog> WatchdogWatchlogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_repack")
            .HasPostgresExtension("pg_stat_statements")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgaudit");

        modelBuilder.Entity<AlertNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alert_notifications_pkey");

            entity.ToTable("alert_notifications");

            entity.HasIndex(e => new { e.Severity, e.Status }, "alert_notifications_severity_status_idx");

            entity.HasIndex(e => e.CreatedAt, "idx_alert_created_at");

            entity.HasIndex(e => e.TraceId, "idx_alert_trace_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AlertType)
                .HasMaxLength(100)
                .HasColumnName("alert_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasComment("Winter is coming!!!")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsSent)
                .HasDefaultValue(false)
                .HasColumnName("is_sent");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ResolvedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy)
                .HasMaxLength(100)
                .HasColumnName("resolved_by");
            entity.Property(e => e.RetryCount)
                .HasDefaultValue(0)
                .HasColumnName("retry_count");
            entity.Property(e => e.SentAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .HasComment("CRITICAL, FATAL, EMERGENCY")
                .HasColumnName("severity");
            entity.Property(e => e.SourceService)
                .HasMaxLength(100)
                .HasColumnName("source_service");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasComment("PENDING, ACKNOWLEDGED, RESOLVED")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TraceId)
                .HasComment("Optional: Link to trace if alert triggered by specific request")
                .HasColumnName("trace_id");

            entity.HasOne(d => d.Trace).WithMany(p => p.AlertNotifications)
                .HasForeignKey(d => d.TraceId)
                .HasConstraintName("alert_notifications_trace_id_fkey");
        });

        modelBuilder.Entity<ApiCallLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("api_call_logs_pkey");

            entity.ToTable("api_call_logs");

            entity.HasIndex(e => new { e.TargetService, e.StatusCode }, "api_call_logs_target_service_status_code_idx");

            entity.HasIndex(e => e.CalledAt, "idx_api_called_at");

            entity.HasIndex(e => e.TraceId, "idx_api_trace_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CalledAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("called_at");
            entity.Property(e => e.CallerService)
                .HasMaxLength(100)
                .HasColumnName("caller_service");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.Endpoint)
                .HasMaxLength(255)
                .HasColumnName("endpoint");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.IsSuccess)
                .HasDefaultValue(true)
                .HasColumnName("is_success");
            entity.Property(e => e.Method)
                .HasMaxLength(10)
                .HasComment("GET, POST, etc.")
                .HasColumnName("method");
            entity.Property(e => e.RequestBody)
                .HasColumnType("jsonb")
                .HasColumnName("request_body");
            entity.Property(e => e.ResponseBody)
                .HasColumnType("jsonb")
                .HasColumnName("response_body");
            entity.Property(e => e.StatusCode).HasColumnName("status_code");
            entity.Property(e => e.TargetService)
                .HasMaxLength(100)
                .HasColumnName("target_service");
            entity.Property(e => e.TraceId).HasColumnName("trace_id");

            entity.HasOne(d => d.Trace).WithMany(p => p.ApiCallLogs)
                .HasForeignKey(d => d.TraceId)
                .HasConstraintName("api_call_logs_trace_id_fkey");
        });

        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("api_logs_new_new_orig_2_pkey");

            entity
                .ToTable("api_logs")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_analyze_scale_factor", "0.02")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_enabled", "false")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_cost_limit", "2000")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_scale_factor", "0.02")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_threshold", "5000");

            entity.HasIndex(e => new { e.Schoolid, e.Starttime }, "idx_api_logs_notification").HasFilter("((path)::text = 'Notification'::text)");

            entity.HasIndex(e => e.Path, "idx_api_logs_path_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.Path, e.Tstamp }, "idx_api_logs_path_tstamp").IsDescending(false, true);

            entity.HasIndex(e => e.Requestbody, "idx_api_logs_requestbody_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('api_logs_new_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Applicationname)
                .HasMaxLength(100)
                .HasColumnName("applicationname");
            entity.Property(e => e.Endtime)
                .HasMaxLength(255)
                .HasColumnName("endtime");
            entity.Property(e => e.Exceptions).HasColumnName("exceptions");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasColumnName("path");
            entity.Property(e => e.Requestbody).HasColumnName("requestbody");
            entity.Property(e => e.Requestheader).HasColumnName("requestheader");
            entity.Property(e => e.Responsebody).HasColumnName("responsebody");
            entity.Property(e => e.Responsetime)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("responsetime");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Sourceip)
                .HasMaxLength(50)
                .HasColumnName("sourceip");
            entity.Property(e => e.Starttime)
                .HasMaxLength(255)
                .HasColumnName("starttime");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("((CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text) + '07:00:00'::interval)")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tstamp");
        });

        modelBuilder.Entity<ApiLogsAcademic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("api_logs_academic_pkey");

            entity.ToTable("api_logs_academic");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Applicationname)
                .HasMaxLength(100)
                .HasColumnName("applicationname");
            entity.Property(e => e.Endtime)
                .HasMaxLength(255)
                .HasColumnName("endtime");
            entity.Property(e => e.Exceptions).HasColumnName("exceptions");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasColumnName("path");
            entity.Property(e => e.Requestbody).HasColumnName("requestbody");
            entity.Property(e => e.Requestheader).HasColumnName("requestheader");
            entity.Property(e => e.Responsebody).HasColumnName("responsebody");
            entity.Property(e => e.Responsetime)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("responsetime");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Sourceip)
                .HasMaxLength(50)
                .HasColumnName("sourceip");
            entity.Property(e => e.Starttime)
                .HasMaxLength(255)
                .HasColumnName("starttime");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("((CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text) + '07:00:00'::interval)")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tstamp");
        });

        modelBuilder.Entity<ApiLogsHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("api_logs_new_new_orig_3_pkey");

            entity
                .ToTable("api_logs_history")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_analyze_scale_factor", "0.02")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_enabled", "false")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_cost_limit", "2000")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_scale_factor", "0.02")
                .HasAnnotation("Npgsql:StorageParameter:autovacuum_vacuum_threshold", "5000");

            entity.HasIndex(e => e.Applicationname, "IDX_VV_apilog_new2_applicationname")
                .HasOperators(new[] { "varchar_ops" })
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.HasIndex(e => e.Schoolid, "IDX_VV_apilog_new2_schoolid").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.HasIndex(e => e.Tstamp, "IDX_VV_apilog_new2_time_stamp").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.HasIndex(e => e.Path, "idx_api_3_logs_path_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('api_logs_new_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Applicationname)
                .HasMaxLength(100)
                .HasColumnName("applicationname");
            entity.Property(e => e.Endtime)
                .HasMaxLength(255)
                .HasColumnName("endtime");
            entity.Property(e => e.Exceptions).HasColumnName("exceptions");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasColumnName("path");
            entity.Property(e => e.Requestbody).HasColumnName("requestbody");
            entity.Property(e => e.Requestheader).HasColumnName("requestheader");
            entity.Property(e => e.Responsebody).HasColumnName("responsebody");
            entity.Property(e => e.Responsetime)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("responsetime");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Sourceip)
                .HasMaxLength(50)
                .HasColumnName("sourceip");
            entity.Property(e => e.Starttime)
                .HasMaxLength(255)
                .HasColumnName("starttime");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("((CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text) + '07:00:00'::interval)")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tstamp");
        });

        modelBuilder.Entity<ApiLogsNotification>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("api_logs_notification");

            entity.Property(e => e.Applicationname)
                .HasMaxLength(100)
                .HasColumnName("applicationname");
            entity.Property(e => e.Endtime)
                .HasMaxLength(255)
                .HasColumnName("endtime");
            entity.Property(e => e.Exceptions).HasColumnName("exceptions");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasColumnName("path");
            entity.Property(e => e.Requestbody).HasColumnName("requestbody");
            entity.Property(e => e.Requestheader).HasColumnName("requestheader");
            entity.Property(e => e.Responsebody).HasColumnName("responsebody");
            entity.Property(e => e.Responsetime)
                .HasMaxLength(255)
                .HasColumnName("responsetime");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Sourceip)
                .HasMaxLength(50)
                .HasColumnName("sourceip");
            entity.Property(e => e.Starttime)
                .HasMaxLength(255)
                .HasColumnName("starttime");
            entity.Property(e => e.Tstamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tstamp");
        });

        modelBuilder.Entity<Apilogstemp>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("apilogstemp");

            entity.Property(e => e.Applicationname)
                .HasMaxLength(100)
                .HasColumnName("applicationname");
            entity.Property(e => e.Endtime)
                .HasMaxLength(255)
                .HasColumnName("endtime");
            entity.Property(e => e.Exceptions).HasColumnName("exceptions");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasColumnName("path");
            entity.Property(e => e.Requestbody).HasColumnName("requestbody");
            entity.Property(e => e.Requestheader).HasColumnName("requestheader");
            entity.Property(e => e.Responsebody).HasColumnName("responsebody");
            entity.Property(e => e.Responsetime)
                .HasMaxLength(255)
                .HasColumnName("responsetime");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Sourceip)
                .HasMaxLength(50)
                .HasColumnName("sourceip");
            entity.Property(e => e.Starttime)
                .HasMaxLength(255)
                .HasColumnName("starttime");
            entity.Property(e => e.Tstamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tstamp");
        });

        modelBuilder.Entity<Apiresponse>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("APIResponse_pkey");

            entity.ToTable("APIResponse");

            entity.HasIndex(e => e.SchoolId, "INDEX_VV_5254518_5254517");

            entity.HasIndex(e => e.Tstamp, "IX_APIResponse");

            entity.HasIndex(e => e.MethodName, "IX_APIResponse_MethodName");

            entity.Property(e => e.LogId)
                .HasDefaultValueSql("nextval('\"APIResponse_SeqNo\"'::regclass)")
                .HasColumnName("LogID");
            entity.Property(e => e.Apirequest).HasColumnName("APIRequest");
            entity.Property(e => e.Apiresponse1).HasColumnName("APIResponse");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.DeviceTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SourceIp).HasColumnName("SourceIP");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ApplicationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("application_logs_pkey");

            entity.ToTable("application_logs");

            entity.HasIndex(e => e.CreatedAt, "application_logs_created_at_idx");

            entity.HasIndex(e => new { e.ServiceName, e.Level }, "application_logs_service_name_level_idx");

            entity.HasIndex(e => e.Tag, "idx_app_tag");

            entity.HasIndex(e => e.TraceId, "idx_app_trace_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Level)
                .HasMaxLength(10)
                .HasComment("INFO, WARN, ERROR, FATAL")
                .HasColumnName("level");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Payload)
                .HasComment("เก็บ Contextual data เช่น ข้อมูลตัวแปร ณ ขณะนั้น")
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
            entity.Property(e => e.StackTrace)
                .HasComment("สำหรับ Error เท่านั้น")
                .HasColumnName("stack_trace");
            entity.Property(e => e.Tag)
                .HasMaxLength(50)
                .HasComment("เช่น Auth, Database, PaymentFlow เอาไว้ Filter กลุ่มงาน")
                .HasColumnName("tag");
            entity.Property(e => e.TraceId).HasColumnName("trace_id");

            entity.HasOne(d => d.Trace).WithMany(p => p.ApplicationLogs)
                .HasForeignKey(d => d.TraceId)
                .HasConstraintName("application_logs_trace_id_fkey");
        });

        modelBuilder.Entity<BoatUserId>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BoatUserID_pkey");

            entity.ToTable("BoatUserID");

            entity.HasIndex(e => e.Type, "IDX_VV_BU_Type");

            entity.HasIndex(e => e.SId, "IDX_VV_BU_sID");

            entity.HasIndex(e => e.Id, "IDX_VV_PK_ID").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "false");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"BoatUserID_ID_Seq\"'::regclass)")
                .HasColumnName("ID");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.MoneyId).HasColumnName("MoneyID");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SSellId).HasColumnName("sSellID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<DailySalesCount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DailySalesCount_pkey");

            entity.ToTable("DailySalesCount");

            entity.HasIndex(e => e.DeviceId, "public_DailySalesCount_DeviceID");

            entity.HasIndex(e => e.SalesData, "public_DailySalesCount_SalesData");

            entity.HasIndex(e => e.ShopId, "public_DailySalesCount_ShopID");

            entity.HasIndex(e => e.Tstamp, "public_DailySalesCount_Tstamp");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"DailySalesCount_ID\"'::regclass)")
                .HasColumnName("ID");
            entity.Property(e => e.DeviceId).HasColumnName("DeviceID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<DeviceActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("public_DeviceActivityLog_PK_DeviceActivityLog1B");

            entity.ToTable("DeviceActivityLog");

            entity.HasIndex(e => new { e.SchoolId, e.Tstamp }, "public_DeviceActivityLog_INDEX_VVB_schoolid_tstap");

            entity.HasIndex(e => e.Tstamp, "public_DeviceActivityLog_INDEX_VVB_tstamp");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("LogID");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.DeviceType).HasMaxLength(150);
            entity.Property(e => e.LogDescription).HasMaxLength(2900);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<DeviceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DeviceDetails_pkey");

            entity.HasIndex(e => e.DeviceId, "IDX_DD_DeviceID");

            entity.HasIndex(e => e.SchoolId, "IDX_DD_SchoolID").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.HasIndex(e => new { e.SchoolId, e.DeviceId }, "IDX_DD_SchoolID_DeviceID");

            entity.HasIndex(e => e.Id, "IX_PK_DD_ID").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "false");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"DeviceDetails_ID\"'::regclass)")
                .HasColumnName("ID");
            entity.Property(e => e.DeviceId).HasColumnName("DeviceID");
            entity.Property(e => e.Ipaddress).HasColumnName("IPAddress");
            entity.Property(e => e.LastUpdated).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<DeviceUpdateStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DeviceUpdateStatus_pkey");

            entity.ToTable("DeviceUpdateStatus");

            entity.HasIndex(e => new { e.SchoolId, e.DeviceId, e.UserId }, "idx_deviceupdatestatus_schoolid_deviceid_userid");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(500)
                .HasColumnName("DeviceID");
            entity.Property(e => e.NeedToUpdate).HasDefaultValue(true);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ErrorId>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ErrorIDs");

            entity.Property(e => e.Id)
                .HasMaxLength(300)
                .HasColumnName("id");
        });

        modelBuilder.Entity<KtbapprovalTransaction>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("KTBApprovalTransaction", "dbo");

            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.ApprovalTransId)
                .ValueGeneratedOnAdd()
                .HasColumnName("ApprovalTransID");
            entity.Property(e => e.BankRef).HasMaxLength(20);
            entity.Property(e => e.Channel).HasMaxLength(20);
            entity.Property(e => e.ComCode).HasMaxLength(10);
            entity.Property(e => e.Command).HasMaxLength(20);
            entity.Property(e => e.CreateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CusName).HasMaxLength(60);
            entity.Property(e => e.DateTime).HasMaxLength(14);
            entity.Property(e => e.EffDate).HasMaxLength(8);
            entity.Property(e => e.Ktbpassword)
                .HasMaxLength(20)
                .HasColumnName("KTBPassword");
            entity.Property(e => e.Ktbuser)
                .HasMaxLength(20)
                .HasColumnName("KTBUser");
            entity.Property(e => e.ProdCode).HasMaxLength(10);
            entity.Property(e => e.Ref1).HasMaxLength(20);
            entity.Property(e => e.Ref2).HasMaxLength(20);
            entity.Property(e => e.Ref3).HasMaxLength(20);
            entity.Property(e => e.Ref4).HasMaxLength(20);
            entity.Property(e => e.RespMsg).HasMaxLength(100);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TransId).HasColumnName("TransID");
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<KtbpaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PaymentTransId).HasName("public_KTBPaymentTransaction_PK_KTBPaymentTransaction");

            entity.ToTable("KTBPaymentTransaction");

            entity.HasIndex(e => new { e.RespCode, e.CreateDate }, "public_KTBPaymentTransaction_INDEX_VV_119275_119274");

            entity.HasIndex(e => e.Ktbuser, "public_KTBPaymentTransaction_INDEX_VV_57250_57249");

            entity.HasIndex(e => new { e.SchoolId, e.RespCode }, "public_KTBPaymentTransaction_INDEX_VV_89957_89956");

            entity.HasIndex(e => e.Ref2, "public_KTBPaymentTransaction_INDEX_VV_9155_9154");

            entity.Property(e => e.PaymentTransId).HasColumnName("PaymentTransID");
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.BankRef).HasMaxLength(20);
            entity.Property(e => e.Channel).HasMaxLength(20);
            entity.Property(e => e.ComCode).HasMaxLength(10);
            entity.Property(e => e.Command).HasMaxLength(20);
            entity.Property(e => e.CreateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CusName).HasMaxLength(60);
            entity.Property(e => e.DateTime).HasMaxLength(14);
            entity.Property(e => e.EffDate).HasMaxLength(8);
            entity.Property(e => e.Ktbpassword)
                .HasMaxLength(20)
                .HasColumnName("KTBPassword");
            entity.Property(e => e.Ktbuser)
                .HasMaxLength(20)
                .HasColumnName("KTBUser");
            entity.Property(e => e.ProdCode).HasMaxLength(10);
            entity.Property(e => e.Ref1).HasMaxLength(20);
            entity.Property(e => e.Ref2).HasMaxLength(20);
            entity.Property(e => e.Ref3).HasMaxLength(20);
            entity.Property(e => e.Ref4).HasMaxLength(20);
            entity.Property(e => e.RespMsg).HasMaxLength(100);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TransId).HasColumnName("TransID");
            entity.Property(e => e.TranxId).HasMaxLength(20);
            entity.Property(e => e.UpdateDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<LogTrace>(entity =>
        {
            entity.HasKey(e => e.TraceId).HasName("log_traces_pkey");

            entity.ToTable("log_traces");

            entity.HasIndex(e => e.CreatedAt, "idx_traces_created_at");

            entity.HasIndex(e => e.ProjectName, "idx_traces_project");

            entity.HasIndex(e => new { e.ProjectName, e.Status }, "log_traces_project_name_status_idx");

            entity.Property(e => e.TraceId)
                .ValueGeneratedNever()
                .HasComment("Unique ID for end-to-end request tracking")
                .HasColumnName("trace_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.EndAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_at");
            entity.Property(e => e.InitialEndpoint)
                .HasMaxLength(255)
                .HasColumnName("initial_endpoint");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .HasColumnName("project_name");
            entity.Property(e => e.RootService)
                .HasMaxLength(100)
                .HasColumnName("root_service");
            entity.Property(e => e.StartAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("SUCCESS, FAILED, PARTIAL_SUCCESS")
                .HasColumnName("status");
        });

        modelBuilder.Entity<ModifiedShop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ModifiedShops_pkey");

            entity.HasIndex(e => new { e.ShopId, e.ShopModified }, "ModifiedShops_ShopID_ShopModified_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LastModified).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
        });

        modelBuilder.Entity<ModifiedUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ModifiedUsers_pkey");

            entity.HasIndex(e => new { e.UserId, e.UserModified }, "ModifiedUsers_UserID_UserModified_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LastModified).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<OffTsellHistoryNew>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("OffTSellHistoryNew");

            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DSell)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dSell");
            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .HasColumnName("id");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SId2).HasColumnName("sID2");
            entity.Property(e => e.SSellId).HasColumnName("sSellID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<OffTsellNew>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("OffTSellNew");

            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DSell)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dSell");
            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .HasColumnName("id");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SId2).HasColumnName("sID2");
            entity.Property(e => e.SSellId).HasColumnName("sSellID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedTime).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<OfflineSale>(entity =>
        {
            entity.HasKey(e => e.SalesId).HasName("OfflineSales_pkey");

            entity.HasIndex(e => e.SourceIp, "OfflineSales_INDEX_VV_20400_20399");

            entity.Property(e => e.SalesId)
                .ValueGeneratedNever()
                .HasColumnName("SalesID");
            entity.Property(e => e.Barcode).HasMaxLength(350);
            entity.Property(e => e.CType)
                .HasMaxLength(5)
                .HasColumnName("cType");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SalesDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SalesProductInfo).HasColumnType("character varying");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.SourceIp)
                .HasMaxLength(250)
                .HasColumnName("SourceIP");
            entity.Property(e => e.TempCardHistoryId).HasColumnName("TempCardHistoryID");
            entity.Property(e => e.TempCardId).HasColumnName("TempCardID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<OfflineSalesBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("OfflineSales_Backup");

            entity.Property(e => e.Barcode).HasMaxLength(350);
            entity.Property(e => e.CType)
                .HasMaxLength(5)
                .HasColumnName("cType");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SalesDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SalesId).HasColumnName("SalesID");
            entity.Property(e => e.SalesProductInfo).HasColumnType("character varying");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.SourceIp)
                .HasMaxLength(250)
                .HasColumnName("SourceIP");
            entity.Property(e => e.TempCardHistoryId).HasColumnName("TempCardHistoryID");
            entity.Property(e => e.TempCardId).HasColumnName("TempCardID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<OnlineScanWorkerLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("online_scan_worker_logs_pkey");

            entity.ToTable("online_scan_worker_logs");

            entity.HasIndex(e => e.CorrelationId, "idx_online_scan_worker_logs_correlationid");

            entity.HasIndex(e => e.QueueId, "idx_online_scan_worker_logs_queueid");

            entity.HasIndex(e => e.DequeuedAt, "idx_online_scan_worker_logs_time");

            entity.HasIndex(e => e.EnvelopeType, "idx_online_scan_worker_logs_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvailableWorkerThreads).HasColumnName("available_worker_threads");
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DequeuedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dequeued_at");
            entity.Property(e => e.EnvelopeType).HasColumnName("envelope_type");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.InputJson).HasColumnName("input_json");
            entity.Property(e => e.MachineName).HasColumnName("machine_name");
            entity.Property(e => e.OutputSummary).HasColumnName("output_summary");
            entity.Property(e => e.ProcessMs).HasColumnName("process_ms");
            entity.Property(e => e.QueueDepthAtDequeue).HasColumnName("queue_depth_at_dequeue");
            entity.Property(e => e.QueueId).HasColumnName("queue_id");
            entity.Property(e => e.QueueWaitMs).HasColumnName("queue_wait_ms");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.StepsJson).HasColumnName("steps_json");
            entity.Property(e => e.Success).HasColumnName("success");
            entity.Property(e => e.WorkerId).HasColumnName("worker_id");
        });

        modelBuilder.Entity<PaymentServersDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PaymentServersDetails_pkey");

            entity.HasIndex(e => e.Ipaddress, "PaymentServersDetails_IPAddress_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Ipaddress).HasColumnName("IPAddress");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<PromptPayTsellDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PromptPayTSellDetails_pkey");

            entity.ToTable("PromptPayTSellDetails");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.SSellId).HasColumnName("sSellID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TsellDetailData).HasColumnName("TSellDetailData");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<SalesDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sales_data_pkey");

            entity.ToTable("sales_data");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Productinfo).HasColumnName("productinfo");
            entity.Property(e => e.Salesinfo).HasColumnName("salesinfo");
            entity.Property(e => e.Tstamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tstamp");
        });

        modelBuilder.Entity<ScanReconciliationResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ScanReconciliationResult_pkey");

            entity.ToTable("ScanReconciliationResult");

            entity.HasIndex(e => e.DeviceActivityLogId, "IX_ScanReconciliationResult_DeviceActivityLogId");

            entity.HasIndex(e => e.DeviceId, "IX_ScanReconciliationResult_DeviceID");

            entity.HasIndex(e => e.HardwareScanLogId, "IX_ScanReconciliationResult_HardwareScanLogId");

            entity.HasIndex(e => e.ScanTime, "IX_ScanReconciliationResult_ScanTime").IsDescending();

            entity.HasIndex(e => e.SchoolId, "IX_ScanReconciliationResult_SchoolID");

            entity.HasIndex(e => new { e.Status, e.DetectedAt }, "IX_ScanReconciliationResult_Status_DetectedAt");

            entity.Property(e => e.DetectedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.DeviceSideNote).HasMaxLength(2900);
            entity.Property(e => e.ResolutionAction).HasMaxLength(30);
            entity.Property(e => e.ResolvedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ResolvedBy).HasMaxLength(200);
            entity.Property(e => e.ScanTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<SchedulesLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schedules_logs_pkey");

            entity.ToTable("schedules_logs");

            entity.HasIndex(e => e.TraceId, "idx_sched_trace_id");

            entity.HasIndex(e => e.CreatedAt, "schedules_logs_created_at_idx");

            entity.HasIndex(e => new { e.ScheduleCode, e.Status }, "schedules_logs_schedule_code_status_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .HasColumnName("project_name");
            entity.Property(e => e.RunAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("run_at");
            entity.Property(e => e.ScheduleCode)
                .HasMaxLength(100)
                .HasColumnName("schedule_code");
            entity.Property(e => e.ScheduleName)
                .HasMaxLength(150)
                .HasColumnName("schedule_name");
            entity.Property(e => e.Status)
                .HasComment("0: Pending, 1: Success, 2: Failed")
                .HasColumnName("status");
            entity.Property(e => e.TraceId)
                .HasComment("Link to master trace")
                .HasColumnName("trace_id");

            entity.HasOne(d => d.Trace).WithMany(p => p.SchedulesLogs)
                .HasForeignKey(d => d.TraceId)
                .HasConstraintName("schedules_logs_trace_id_fkey");
        });

        modelBuilder.Entity<ServiceCallLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("service_call_logs_pkey");

            entity.ToTable("service_call_logs");

            entity.HasIndex(e => e.TraceId, "idx_svc_trace_id");

            entity.HasIndex(e => e.CalledAt, "service_call_logs_called_at_idx");

            entity.HasIndex(e => new { e.GrpcService, e.GrpcMethod }, "service_call_logs_grpc_service_grpc_method_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CalledAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("called_at");
            entity.Property(e => e.CallerService)
                .HasMaxLength(100)
                .HasColumnName("caller_service");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.GrpcMethod)
                .HasMaxLength(100)
                .HasColumnName("grpc_method");
            entity.Property(e => e.GrpcService)
                .HasMaxLength(150)
                .HasColumnName("grpc_service");
            entity.Property(e => e.GrpcStatus)
                .HasMaxLength(50)
                .HasColumnName("grpc_status");
            entity.Property(e => e.IsSuccess)
                .HasDefaultValue(true)
                .HasColumnName("is_success");
            entity.Property(e => e.RequestProto)
                .HasColumnType("jsonb")
                .HasColumnName("request_proto");
            entity.Property(e => e.ResponseProto)
                .HasColumnType("jsonb")
                .HasColumnName("response_proto");
            entity.Property(e => e.TargetService)
                .HasMaxLength(100)
                .HasColumnName("target_service");
            entity.Property(e => e.TraceId).HasColumnName("trace_id");

            entity.HasOne(d => d.Trace).WithMany(p => p.ServiceCallLogs)
                .HasForeignKey(d => d.TraceId)
                .HasConstraintName("service_call_logs_trace_id_fkey");
        });

        modelBuilder.Entity<TacademicLog>(entity =>
        {
            entity.HasKey(e => new { e.AcademicLogId, e.SEmp, e.DLog, e.SchoolId }).HasName("TAcademicLog_pkey");

            entity.ToTable("TAcademicLog");

            entity.HasIndex(e => new { e.NMenu, e.Action }, "idx_tacademiclog_menu_action");

            entity.HasIndex(e => new { e.SchoolId, e.NMenu, e.Action, e.DLog }, "idx_tacademiclog_school_menu_action_dlog");

            entity.HasIndex(e => new { e.SchoolId, e.DLog }, "idx_tacademiclog_schoolid_dlog");

            entity.HasIndex(e => e.SystemLogId, "idx_tacademiclog_systemlogid");

            entity.Property(e => e.AcademicLogId)
                .HasDefaultValueSql("nextval('\"TAcademicLog_Seq\"'::regclass)")
                .HasColumnName("AcademicLogID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.DLog)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dLog");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.NMenu).HasColumnName("nMenu");
            entity.Property(e => e.NSession).HasColumnName("nSession");
            entity.Property(e => e.SLog)
                .HasColumnType("character varying")
                .HasColumnName("sLog");
            entity.Property(e => e.SystemLogId).HasColumnName("SystemLogID");
            entity.Property(e => e.Systemtype).HasColumnName("systemtype");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TacademicLogHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TAcademicLog_History");

            entity.HasIndex(e => new { e.SchoolId, e.NMenu, e.Action, e.DLog }, "idx_tacademicloghistory_school_menu_action_dlog");

            entity.HasIndex(e => new { e.SchoolId, e.DLog }, "idx_tacademicloghistory_schoolid_dlog");

            entity.Property(e => e.AcademicLogId)
                .HasDefaultValueSql("nextval('\"TAcademicLog_Seq\"'::regclass)")
                .HasColumnName("AcademicLogID");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DLog)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dLog");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.NMenu).HasColumnName("nMenu");
            entity.Property(e => e.NSession).HasColumnName("nSession");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SLog)
                .HasColumnType("character varying")
                .HasColumnName("sLog");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SystemLogId).HasColumnName("SystemLogID");
            entity.Property(e => e.Systemtype).HasColumnName("systemtype");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TacademicPerfLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TAcademicPerf_Log_pkey");

            entity.ToTable("TAcademicPerf_Log");

            entity.HasIndex(e => new { e.MethodName, e.CreatedAt }, "ix_perf_log_method_time").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Context).HasColumnName("context");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.MethodName).HasColumnName("method_name");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.ServerName).HasColumnName("server_name");
            entity.Property(e => e.Steps)
                .HasColumnType("jsonb")
                .HasColumnName("steps");
            entity.Property(e => e.TotalMs).HasColumnName("total_ms");
        });

        modelBuilder.Entity<TapirequestResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TAPIRequestResponse_pkey");

            entity.ToTable("TAPIRequestResponse");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"TAPIRequestResponse_Seq\"'::regclass)")
                .HasColumnName("ID");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ServerIp).HasColumnName("ServerIP");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TattendanceConfigChangeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TAttendanceConfigChangeLog_pkey");

            entity.ToTable("TAttendanceConfigChangeLog");

            entity.HasIndex(e => new { e.SchoolId, e.AffectedDateFrom, e.AffectedDateTo }, "idx_attcfg_affected");

            entity.HasIndex(e => new { e.SchoolId, e.ChangedAt }, "idx_attcfg_school_date").IsDescending(false, true);

            entity.HasIndex(e => new { e.ConfigType, e.EntityId }, "idx_attcfg_type");

            entity.Property(e => e.Action).HasMaxLength(10);
            entity.Property(e => e.AffectedDays).HasMaxLength(50);
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ConfigType).HasMaxLength(30);
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.EntityName).HasMaxLength(200);
            entity.Property(e => e.ImpactFlags).HasMaxLength(200);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.NewValue).HasColumnType("jsonb");
            entity.Property(e => e.OldValue).HasColumnType("jsonb");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SourceMethod).HasMaxLength(100);
            entity.Property(e => e.SourcePage).HasMaxLength(100);
        });

        modelBuilder.Entity<TauditDailyRollup>(entity =>
        {
            entity.HasKey(e => new { e.AuditDate, e.TopicKey, e.MetricKey }).HasName("TAuditDailyRollup_pkey");

            entity.ToTable("TAuditDailyRollup");

            entity.HasIndex(e => new { e.TopicKey, e.AuditDate }, "IX_TAuditDailyRollup_TopicKey_AuditDate").IsDescending(false, true);

            entity.Property(e => e.ComputeStatus).HasDefaultValueSql("'Success'::text");
            entity.Property(e => e.ComputedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TauditFinding>(entity =>
        {
            entity.HasKey(e => e.FindingId).HasName("TAuditFinding_pkey");

            entity.ToTable("TAuditFinding");

            entity.HasIndex(e => new { e.Topic, e.SchoolId }, "IX_TAuditFinding_Topic_SchoolID");

            entity.HasIndex(e => new { e.WeekStart, e.Status }, "IX_TAuditFinding_WeekStart_Status");

            entity.Property(e => e.FindingId).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ResolvedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Severity).HasComment("Red or Yellow only — matches the dashboard dot color. Unknown/absent data is not a finding.");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Open'::text")
                .HasComment("Open (default) -> Acknowledged -> Resolved | AcceptedRisk. Resolved and AcceptedRisk both count as \"closed\" for the weekly open/closed count.");
        });

        modelBuilder.Entity<TbBehaviorJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TB_BehaviorJob_pkey");

            entity.ToTable("TB_BehaviorJob");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.LogDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ScholId).HasColumnName("ScholID");
        });

        modelBuilder.Entity<TbResourceUsageLog>(entity =>
        {
            entity.HasKey(e => e.ResourceUsageLogId).HasName("TB_ResourceUsageLog_pkey");

            entity.ToTable("TB_ResourceUsageLog");

            entity.HasIndex(e => e.CapturedAt, "IX_TB_ResourceUsageLog_CapturedAt");

            entity.Property(e => e.CapturedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.MachineName).HasMaxLength(100);
            entity.Property(e => e.ProcessCpuPercent).HasPrecision(5, 2);
            entity.Property(e => e.ProcessPrivateMemoryMb).HasPrecision(10, 2);
            entity.Property(e => e.ProcessWorkingSetMb).HasPrecision(10, 2);
            entity.Property(e => e.SystemAvailableMemoryMb).HasPrecision(10, 2);
            entity.Property(e => e.SystemCpuPercent).HasPrecision(5, 2);
            entity.Property(e => e.SystemTotalMemoryMb).HasPrecision(10, 2);
        });

        modelBuilder.Entity<TbSendApiLog>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("TB_Send_API_Log_pkey");

            entity.ToTable("TB_Send_API_Log");

            entity.HasIndex(e => new { e.SchoolId, e.ApiName, e.Tstamp }, "idx_tb_send_api_log_school_api_date");

            entity.HasIndex(e => new { e.SchoolId, e.Tstamp }, "idx_tb_send_api_log_schoolid_tstamp");

            entity.Property(e => e.Guid)
                .ValueGeneratedNever()
                .HasColumnName("GUID");
            entity.Property(e => e.ApiName)
                .HasColumnType("character varying")
                .HasColumnName("Api_Name");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("((now() AT TIME ZONE 'utc'::text) + '07:00:00'::interval)")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"TB_Send_API_Log_Seq\"'::regclass)")
                .HasColumnName("ID");
            entity.Property(e => e.Info).HasColumnType("character varying");
            entity.Property(e => e.Result).HasColumnType("character varying");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("((now() AT TIME ZONE 'utc'::text) + '07:00:00'::interval)")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TbSendApiLogHistory>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("TB_Send_API_Log_pkey_202603100200402");

            entity.ToTable("TB_Send_API_Log_History");

            entity.Property(e => e.Guid)
                .ValueGeneratedNever()
                .HasColumnName("GUID");
            entity.Property(e => e.ApiName)
                .HasColumnType("character varying")
                .HasColumnName("Api_Name");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("((now() AT TIME ZONE 'utc'::text) + '07:00:00'::interval)")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"TB_Send_API_Log_Seq\"'::regclass)")
                .HasColumnName("ID");
            entity.Property(e => e.Info).HasColumnType("character varying");
            entity.Property(e => e.Result).HasColumnType("character varying");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("((now() AT TIME ZONE 'utc'::text) + '07:00:00'::interval)")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TdbjobLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TDBJobLogs_pkey");

            entity.ToTable("TDBJobLogs");

            entity.HasIndex(e => e.BusinessDate, "IX_TDBJobLogs_BusinessDate");

            entity.HasIndex(e => e.JobName, "IX_TDBJobLogs_JobName");

            entity.HasIndex(e => e.Status, "IX_TDBJobLogs_Status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"TDBJobLogs_seq\"'::regclass)")
                .HasColumnName("ID");
            entity.Property(e => e.EndTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StartTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TdeviceRequestLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("TDeviceRequestLog_pkey");

            entity.ToTable("TDeviceRequestLog");

            entity.HasIndex(e => new { e.DeviceId, e.Tstamp }, "IX_TDeviceRequestLog_DeviceID_Tstamp");

            entity.HasIndex(e => new { e.SchoolId, e.Activity, e.Tstamp }, "IX_TDeviceRequestLog_SchoolID_Activity_Tstamp");

            entity.HasIndex(e => new { e.SchoolId, e.Tstamp }, "IX_TDeviceRequestLog_SchoolID_Tstamp");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Activity).HasMaxLength(100);
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Tstamp)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserCode).HasMaxLength(100);
        });

        modelBuilder.Entity<TempWatchdoglog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("temp_watchdoglogs");

            entity.Property(e => e.AuthKey).HasColumnName("auth_key");
            entity.Property(e => e.SchoolId).HasColumnName("school_id");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<TfcmdeliveryAck>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TFCMDeliveryAck_pkey");

            entity.ToTable("TFCMDeliveryAck");

            entity.HasIndex(e => e.PushMid, "IX_TFCMDeliveryAck_PushMid");

            entity.HasIndex(e => e.RequestId, "IX_TFCMDeliveryAck_RequestId");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AckTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.AckType).HasMaxLength(20);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.RequestId).HasMaxLength(64);
            entity.Property(e => e.Token).HasMaxLength(2000);
        });

        modelBuilder.Entity<TfcmsendLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TFCMSendLog_pkey");

            entity.ToTable("TFCMSendLog");

            entity.HasIndex(e => e.CreatedDate, "IX_TFCMSendLog_CreatedDate");

            entity.HasIndex(e => e.IsRecipientMismatch, "IX_TFCMSendLog_IsRecipientMismatch").HasFilter("(\"IsRecipientMismatch\" = true)");

            entity.HasIndex(e => new { e.PushMid, e.SchoolId }, "IX_TFCMSendLog_PushMid_SchoolID");

            entity.HasIndex(e => e.RequestId, "IX_TFCMSendLog_RequestId");

            entity.HasIndex(e => e.SendCorrelationId, "IX_TFCMSendLog_SendCorrelationID");

            entity.HasIndex(e => new { e.UserId, e.CreatedDate }, "IX_TFCMSendLog_UserID_CreatedDate");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ErrorCode).HasMaxLength(100);
            entity.Property(e => e.MessageId)
                .HasMaxLength(200)
                .HasColumnName("MessageID");
            entity.Property(e => e.NotificationBody).HasMaxLength(1000);
            entity.Property(e => e.NotificationTitle).HasMaxLength(500);
            entity.Property(e => e.PushUserId).HasColumnName("PushUserID");
            entity.Property(e => e.RequestId).HasMaxLength(64);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SendCorrelationId).HasColumnName("SendCorrelationID");
            entity.Property(e => e.SendMode).HasMaxLength(20);
            entity.Property(e => e.Token).HasMaxLength(2000);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TfcmsendRequestLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TFCMSendRequestLog_pkey");

            entity.ToTable("TFCMSendRequestLog");

            entity.HasIndex(e => e.CreatedDate, "IX_TFCMSendRequestLog_CreatedDate");

            entity.HasIndex(e => e.RequestId, "IX_TFCMSendRequestLog_RequestId");

            entity.HasIndex(e => e.Stage, "IX_TFCMSendRequestLog_Stage");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CompletedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.RequestId).HasMaxLength(64);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SendCorrelationId).HasColumnName("SendCorrelationID");
            entity.Property(e => e.Stage).HasMaxLength(30);
        });

        modelBuilder.Entity<TgradeAuditLog>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.GradeAuditLogId }).HasName("TGradeAuditLogNew_pkey");

            entity.ToTable("TGradeAuditLog");

            entity.Property(e => e.GradeAuditLogId).HasDefaultValueSql("nextval('\"TGradeAuditLogNew_Seq\"'::regclass)");
            entity.Property(e => e.Logdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
        });

        modelBuilder.Entity<TgradeAuditLogHistory>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.GradeAuditLogId }).HasName("TGradeAuditLog_pkey");

            entity.ToTable("TGradeAuditLog_History");

            entity.Property(e => e.GradeAuditLogId).HasDefaultValueSql("nextval('\"TGradeAuditLog_Seq\"'::regclass)");
            entity.Property(e => e.Logdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
        });

        modelBuilder.Entity<TgradeAuditLogHistoryBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TGradeAuditLog_History_Backup");

            entity.Property(e => e.GradeAuditLogId).HasDefaultValueSql("nextval('\"TGradeAuditLog_Seq\"'::regclass)");
            entity.Property(e => e.Logdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
        });

        modelBuilder.Entity<TgradeDetailAuditLog>(entity =>
        {
            entity.HasKey(e => e.GradeDetailAuditLogId).HasName("TGradeDetailAuditLog_pkey");

            entity.ToTable("TGradeDetailAuditLog");

            entity.Property(e => e.Logdate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
        });

        modelBuilder.Entity<TgradeDetailIssueLog>(entity =>
        {
            entity.HasKey(e => e.GradeDetailIssueLogId).HasName("TGradeDetailIssueLog_pkey");

            entity.ToTable("TGradeDetailIssueLog");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NGradeId).HasColumnName("nGradeId");
            entity.Property(e => e.STerm).HasColumnName("sTerm");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
        });

        modelBuilder.Entity<ThardwareScanLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("THardwareScanLog_pkey");

            entity.ToTable("THardwareScanLog");

            entity.HasIndex(e => e.CardNumber, "IX_THardwareScanLog_CardNumber");

            entity.HasIndex(e => e.CorrelationId, "IX_THardwareScanLog_CorrelationId")
                .IsUnique()
                .HasFilter("(\"CorrelationId\" IS NOT NULL)");

            entity.HasIndex(e => e.CorrelationId, "IX_THardwareScanLog_CorrelationId_Trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.EntityId, e.ReceivedAt }, "IX_THardwareScanLog_EntityId_ReceivedAt");

            entity.HasIndex(e => e.ReceivedAt, "IX_THardwareScanLog_ReceivedAt");

            entity.HasIndex(e => e.ScanChannel, "IX_THardwareScanLog_ScanChannel");

            entity.HasIndex(e => new { e.SchoolId, e.ReceivedAt }, "IX_THardwareScanLog_SchoolID_ReceivedAt");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.CardNumber).HasMaxLength(50);
            entity.Property(e => e.ClientScanTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ClientScanTimeRaw).HasMaxLength(64);
            entity.Property(e => e.CorrelationId).HasMaxLength(64);
            entity.Property(e => e.DeviceIdentifier).HasMaxLength(100);
            entity.Property(e => e.EnrichedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.EntityName).HasMaxLength(200);
            entity.Property(e => e.EntityType).HasMaxLength(20);
            entity.Property(e => e.HttpMethod).HasMaxLength(8);
            entity.Property(e => e.ParseNote).HasMaxLength(500);
            entity.Property(e => e.Path).HasMaxLength(300);
            entity.Property(e => e.ReceivedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.RequestHost).HasMaxLength(200);
            entity.Property(e => e.ScanChannel).HasMaxLength(30);
            entity.Property(e => e.ScanDirection).HasMaxLength(10);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SchoolName).HasMaxLength(200);
            entity.Property(e => e.SourceIp)
                .HasMaxLength(45)
                .HasColumnName("SourceIP");
        });

        modelBuilder.Entity<TjobConfig>(entity =>
        {
            entity.HasKey(e => new { e.JobKey, e.InstanceKey }).HasName("TJobConfig_pkey");

            entity.ToTable("TJobConfig");

            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TjobConfigHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("TJobConfigHistory_pkey");

            entity.ToTable("TJobConfigHistory");

            entity.HasIndex(e => new { e.JobKey, e.ChangedAt }, "IX_TJobConfigHistory_JobKey_ChangedAt").IsDescending(false, true);

            entity.Property(e => e.HistoryId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ChangedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TjobRunLog>(entity =>
        {
            entity.HasKey(e => e.JobRunId).HasName("TJobRunLog_pkey");

            entity.ToTable("TJobRunLog");

            entity.HasIndex(e => new { e.JobKey, e.StartedAt }, "IX_TJobRunLog_JobKey_StartedAt").IsDescending(false, true);

            entity.HasIndex(e => e.Status, "IX_TJobRunLog_Status_Running").HasFilter("(\"Status\" = 'Running'::text)");

            entity.Property(e => e.JobRunId).ValueGeneratedNever();
            entity.Property(e => e.FinishedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StartedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Tlog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tlog");

            entity.Property(e => e.Applicationname)
                .HasMaxLength(100)
                .HasColumnName("applicationname");
            entity.Property(e => e.Endtime)
                .HasMaxLength(255)
                .HasColumnName("endtime");
            entity.Property(e => e.Exceptions).HasColumnName("exceptions");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasColumnName("path");
            entity.Property(e => e.Requestbody).HasColumnName("requestbody");
            entity.Property(e => e.Requestheader).HasColumnName("requestheader");
            entity.Property(e => e.Responsebody).HasColumnName("responsebody");
            entity.Property(e => e.Responsetime)
                .HasMaxLength(255)
                .HasColumnName("responsetime");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
            entity.Property(e => e.Sourceip)
                .HasMaxLength(50)
                .HasColumnName("sourceip");
            entity.Property(e => e.Starttime)
                .HasMaxLength(255)
                .HasColumnName("starttime");
            entity.Property(e => e.Tstamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("tstamp");
        });

        modelBuilder.Entity<TmanualPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TManualPayment_pkey");

            entity.ToTable("TManualPayment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"TManualPayment_ID_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ApproveId).HasColumnName("approve_id");
            entity.Property(e => e.CancelerId).HasColumnName("canceler_id");
            entity.Property(e => e.CreateDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
            entity.Property(e => e.EvidenceUrl)
                .HasMaxLength(250)
                .HasColumnName("evidence_url");
            entity.Property(e => e.Remark)
                .HasMaxLength(100)
                .HasColumnName("remark");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .HasColumnName("transaction_type");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("update_date");
        });

        modelBuilder.Entity<TmessageBoxDberror>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TMessageBoxDBError_pkey");

            entity.ToTable("TMessageBoxDBError");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SellId).HasColumnName("SellID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TmoneyDberror>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.MoneyId, e.TopupId }).HasName("public_TMoneyDBError_PK_TMoneyDBError");

            entity.ToTable("TMoneyDBError");

            entity.HasIndex(e => new { e.SchoolId, e.DMoney, e.SourceIp, e.Processed }, "public_TMoneyDBError_IX_TMoneyDBError");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.MoneyId)
                .ValueGeneratedOnAdd()
                .HasColumnName("MoneyID");
            entity.Property(e => e.TopupId).HasMaxLength(46);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.ChargeId)
                .HasMaxLength(100)
                .HasColumnName("ChargeID");
            entity.Property(e => e.CheckId)
                .HasMaxLength(400)
                .HasColumnName("CheckID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DMoney)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dMoney");
            entity.Property(e => e.DayCancal)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dayCancal");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.Insurance).HasPrecision(18, 2);
            entity.Property(e => e.NBalance)
                .HasPrecision(18, 2)
                .HasColumnName("nBalance");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 2)
                .HasColumnName("nMoney");
            entity.Property(e => e.PaymentInfo).HasMaxLength(500);
            entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SourceIp)
                .HasMaxLength(200)
                .HasColumnName("SourceIP");
            entity.Property(e => e.TopupType)
                .HasMaxLength(5)
                .HasColumnName("topup_type");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedTime).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TmoneyDberrorBkup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TMoneyDBError_Bkup");

            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CType)
                .HasMaxLength(1)
                .HasColumnName("cType");
            entity.Property(e => e.CardHistoryId).HasColumnName("CardHistoryID");
            entity.Property(e => e.ChargeId)
                .HasMaxLength(100)
                .HasColumnName("ChargeID");
            entity.Property(e => e.CheckId)
                .HasMaxLength(400)
                .HasColumnName("CheckID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DMoney)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dMoney");
            entity.Property(e => e.DayCancal)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dayCancal");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.Insurance).HasPrecision(18, 2);
            entity.Property(e => e.MoneyId).HasColumnName("MoneyID");
            entity.Property(e => e.NBalance)
                .HasPrecision(18, 2)
                .HasColumnName("nBalance");
            entity.Property(e => e.NMoney)
                .HasPrecision(18, 2)
                .HasColumnName("nMoney");
            entity.Property(e => e.PaymentInfo).HasMaxLength(500);
            entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SourceIp)
                .HasMaxLength(200)
                .HasColumnName("SourceIP");
            entity.Property(e => e.TopupId).HasMaxLength(46);
            entity.Property(e => e.TopupType)
                .HasMaxLength(5)
                .HasColumnName("topup_type");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedTime).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TmpApiresponse>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tmpAPIResponse");

            entity.Property(e => e.Apirequest).HasColumnName("APIRequest");
            entity.Property(e => e.Apiresponse).HasColumnName("APIResponse");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.DeviceTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SourceIp).HasColumnName("SourceIP");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TschoolFeatureFlag>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.FeatureKey }).HasName("TSchoolFeatureFlag_pkey");

            entity.ToTable("TSchoolFeatureFlag");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.EffectiveFrom).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TsellApi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TSell_API_pkey");

            entity.ToTable("TSell_API");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Tstamp).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TsellDberror>(entity =>
        {
            entity.HasKey(e => new { e.SchoolId, e.SSellId }).HasName("public_TSellDBError_PK_TSellDBError");

            entity.ToTable("TSellDBError");

            entity.HasIndex(e => new { e.SchoolId, e.DSell, e.SourceIp, e.Processed }, "public_TSellDBError_IX_TSellDBError");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SSellId)
                .ValueGeneratedOnAdd()
                .HasColumnName("sSellID");
            entity.Property(e => e.ApplicationName).HasMaxLength(200);
            entity.Property(e => e.Barcode).HasMaxLength(40);
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DSell)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dSell");
            entity.Property(e => e.DayCancal)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("dayCancal");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("DeviceID");
            entity.Property(e => e.Id)
                .HasMaxLength(300)
                .HasColumnName("id");
            entity.Property(e => e.NSession).HasColumnName("nSession");
            entity.Property(e => e.NTotal)
                .HasPrecision(18, 2)
                .HasColumnName("nTotal");
            entity.Property(e => e.NfcorEncrypt)
                .HasMaxLength(40)
                .HasColumnName("NFCOrEncrypt");
            entity.Property(e => e.RefPromptPay).HasMaxLength(300);
            entity.Property(e => e.ReferenceOrder).HasMaxLength(30);
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SId2).HasColumnName("sID2");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.SourceIp)
                .HasMaxLength(200)
                .HasColumnName("SourceIP");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UpdatedTime).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsellDetailDberror>(entity =>
        {
            entity.HasKey(e => new { e.SellDetailsId, e.NSell, e.NOrder, e.SchoolId }).HasName("public_TSell_DetailDBError_PK_TSell_DetailDBError");

            entity.ToTable("TSell_DetailDBError");

            entity.HasIndex(e => e.NSell, "public_TSell_DetailDBError_IDX_TSell_nSellDBError");

            entity.Property(e => e.SellDetailsId)
                .ValueGeneratedOnAdd()
                .HasColumnName("SellDetailsID");
            entity.Property(e => e.NSell).HasColumnName("nSell");
            entity.Property(e => e.NOrder).HasColumnName("nOrder");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.CDel)
                .HasMaxLength(1)
                .HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DiscountPrice).HasPrecision(18);
            entity.Property(e => e.Id)
                .HasMaxLength(300)
                .HasColumnName("id");
            entity.Property(e => e.NCost)
                .HasPrecision(18, 2)
                .HasColumnName("nCost");
            entity.Property(e => e.NNumber).HasColumnName("nNumber");
            entity.Property(e => e.NPrice)
                .HasPrecision(18, 2)
                .HasColumnName("nPrice");
            entity.Property(e => e.NProduct).HasColumnName("nProduct");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<TsweepLog>(entity =>
        {
            entity.HasKey(e => e.SweepLogId).HasName("TSweepLog_pkey");

            entity.ToTable("TSweepLog");

            entity.Property(e => e.SweepLogId)
                .HasDefaultValueSql("nextval('\"TSweepLog_Seq\"'::regclass)")
                .HasColumnName("SweepLogID");
            entity.Property(e => e.Amount).HasPrecision(9, 2);
            entity.Property(e => e.Bank).HasMaxLength(20);
            entity.Property(e => e.ChargeId)
                .HasMaxLength(100)
                .HasColumnName("ChargeID");
            entity.Property(e => e.CreateDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ReferenceNo).HasMaxLength(100);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransId).HasColumnName("TransID");
            entity.Property(e => e.UpdateName).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<TsystemLogHistoryBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TSystemLog_History_Backup");

            entity.HasIndex(e => new { e.SchoolId, e.DLog }, "IDX_TSystemlog_Restored_SchoolID_dLog");

            entity.HasIndex(e => e.DLog, "IDX_TSystemlog_Restored_dLog");

            entity.HasIndex(e => new { e.SchoolId, e.DLog, e.SEmp }, "idx_syslog_school_dlog_emp").HasFilter("(\"sEmp\" > 0)");

            entity.HasIndex(e => e.SLog, "idx_syslog_slog_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.NMenu, e.Action, e.SystemLogId }, "idx_tsystemlog_backup_menu_action_syslogid");

            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DLog)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dLog");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.NMenu).HasColumnName("nMenu");
            entity.Property(e => e.NSession).HasColumnName("nSession");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SLog)
                .HasColumnType("character varying")
                .HasColumnName("sLog");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SystemLogId)
                .HasDefaultValueSql("nextval('\"TSystemlog_Seq\"'::regclass)")
                .HasColumnName("SystemLogID");
            entity.Property(e => e.Systemtype).HasColumnName("systemtype");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Tsystemlog>(entity =>
        {
            entity.HasKey(e => new { e.SystemLogId, e.SEmp, e.DLog, e.SchoolId }).HasName("TSystemlog_pkey");

            entity.ToTable("TSystemlog");

            entity.HasIndex(e => new { e.SchoolId, e.DLog }, "IDX_TSystemlog_SchoolID_dLog");

            entity.HasIndex(e => e.DLog, "IDX_TSystemlog_dLog");

            entity.Property(e => e.SystemLogId)
                .HasDefaultValueSql("nextval('\"TSystemlog_Seq\"'::regclass)")
                .HasColumnName("SystemLogID");
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.DLog)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dLog");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.CDel).HasColumnName("cDel");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.NMenu).HasColumnName("nMenu");
            entity.Property(e => e.NSession).HasColumnName("nSession");
            entity.Property(e => e.SLog)
                .HasColumnType("character varying")
                .HasColumnName("sLog");
            entity.Property(e => e.Systemtype).HasColumnName("systemtype");
            entity.Property(e => e.UpdatedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<UpdatedShop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UpdatedShops_pkey");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(300)
                .HasColumnName("DeviceID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<UpdatedUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UpdatedUsers_pkey");

            entity.HasIndex(e => new { e.SchoolId, e.UserId, e.DeviceId, e.SId }, "UpdatedUsers_SchoolID_UserID_DeviceID_sID_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(500)
                .HasColumnName("DeviceID");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<UserApiresponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("public_UserAPIResponse_PK_UserAPIResponse");

            entity.ToTable("UserAPIResponse");

            entity.HasIndex(e => e.ApiprocessedTime, "public_UserAPIResponse_INDEX_VV_129_128");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ApiprocessedTime)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("APIProcessedTime");
            entity.Property(e => e.ApplicationName).HasMaxLength(1000);
            entity.Property(e => e.DeviceTime).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.MethodName).HasMaxLength(200);
            entity.Property(e => e.SEmp).HasColumnName("sEmp");
            entity.Property(e => e.SId).HasColumnName("sID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.Status).HasMaxLength(1000);
            entity.Property(e => e.Tstamp).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<UserApisummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserAPISummary_pkey");

            entity.ToTable("UserAPISummary");

            entity.HasIndex(e => e.Path, "idx_userapisummary_path_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => new { e.Schoolid, e.Businessdate, e.Path }, "idx_userapisummary_schoolid_date_path");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Applicationname).HasColumnName("applicationname");
            entity.Property(e => e.Businessdate).HasColumnName("businessdate");
            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.Requestcount).HasColumnName("requestcount");
            entity.Property(e => e.Schoolid).HasColumnName("schoolid");
        });

        modelBuilder.Entity<WatchdogLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("watchdog_logs_pkey");

            entity.ToTable("watchdog_logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Callingfrom)
                .HasColumnType("character varying")
                .HasColumnName("callingfrom");
            entity.Property(e => e.Callingmethod)
                .HasMaxLength(100)
                .HasColumnName("callingmethod");
            entity.Property(e => e.Eventid)
                .HasMaxLength(100)
                .HasColumnName("eventid");
            entity.Property(e => e.Linenumber).HasColumnName("linenumber");
            entity.Property(e => e.Loglevel)
                .HasMaxLength(30)
                .HasColumnName("loglevel");
            entity.Property(e => e.Message)
                .HasColumnType("character varying")
                .HasColumnName("message");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
        });

        modelBuilder.Entity<WatchdogWatchexceptionlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("watchdog_watchexceptionlog_pkey");

            entity.ToTable("watchdog_watchexceptionlog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Encounteredat).HasColumnName("encounteredat");
            entity.Property(e => e.Message)
                .HasColumnType("character varying")
                .HasColumnName("message");
            entity.Property(e => e.Method)
                .HasMaxLength(30)
                .HasColumnName("method");
            entity.Property(e => e.Path)
                .HasColumnType("character varying")
                .HasColumnName("path");
            entity.Property(e => e.Querystring)
                .HasColumnType("character varying")
                .HasColumnName("querystring");
            entity.Property(e => e.Requestbody)
                .HasColumnType("character varying")
                .HasColumnName("requestbody");
            entity.Property(e => e.Source)
                .HasColumnType("character varying")
                .HasColumnName("source");
            entity.Property(e => e.Stacktrace)
                .HasColumnType("character varying")
                .HasColumnName("stacktrace");
            entity.Property(e => e.Typeof)
                .HasColumnType("character varying")
                .HasColumnName("typeof");
        });

        modelBuilder.Entity<WatchdogWatchlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("watchdog_watchlog_pkey");

            entity.ToTable("watchdog_watchlog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Endtime).HasColumnName("endtime");
            entity.Property(e => e.Host)
                .HasColumnType("character varying")
                .HasColumnName("host");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(30)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Method)
                .HasMaxLength(30)
                .HasColumnName("method");
            entity.Property(e => e.Path)
                .HasColumnType("character varying")
                .HasColumnName("path");
            entity.Property(e => e.Querystring)
                .HasColumnType("character varying")
                .HasColumnName("querystring");
            entity.Property(e => e.Requestbody)
                .HasColumnType("character varying")
                .HasColumnName("requestbody");
            entity.Property(e => e.Requestheaders)
                .HasColumnType("character varying")
                .HasColumnName("requestheaders");
            entity.Property(e => e.Responsebody)
                .HasColumnType("character varying")
                .HasColumnName("responsebody");
            entity.Property(e => e.Responseheaders)
                .HasColumnType("character varying")
                .HasColumnName("responseheaders");
            entity.Property(e => e.Responsestatus).HasColumnName("responsestatus");
            entity.Property(e => e.Starttime).HasColumnName("starttime");
            entity.Property(e => e.Timespent)
                .HasColumnType("character varying")
                .HasColumnName("timespent");
        });
        modelBuilder.HasSequence("api_logs_academic_id_seq");
        modelBuilder.HasSequence("api_logs_new_id_seq").IsCyclic();
        modelBuilder.HasSequence("APIResponse_SeqNo");
        modelBuilder.HasSequence("BoatUserID_ID_Seq");
        modelBuilder.HasSequence("DailySalesCount_ID");
        modelBuilder.HasSequence("DeviceDetails_ID");
        modelBuilder.HasSequence("TAcademicLog_Seq");
        modelBuilder.HasSequence("TB_Send_API_Log_Seq");
        modelBuilder.HasSequence("TManualPayment_ID_seq");
        modelBuilder.HasSequence("TSweepLog_Seq");
        modelBuilder.HasSequence("TSystemlog_Seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
