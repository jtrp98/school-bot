using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TregisterSetup
{
    public int RegisterSetupId { get; set; }

    public int? NYear { get; set; }

    public int? Year { get; set; }

    public string? StudentType { get; set; }

    public int? NTsubLevel { get; set; }

    public int? RegisterPlanSetupId { get; set; }

    public int? StudentMax { get; set; }

    public DateTime? SubmitDocumentDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Fee { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int? ExamAnnounce { get; set; }

    public DateTime? MeetingDate { get; set; }

    public string? MeetingTime { get; set; }

    public string? MeetingPlace { get; set; }

    public string? AttachmentsPassExam { get; set; }

    public string? AttachmentsFailExam { get; set; }

    public int SchoolId { get; set; }

    public int? PlanId { get; set; }

    public int? PaymentGroupId { get; set; }

    public bool? IsActiveBackupPlan { get; set; }

    public int? OrderPlans { get; set; }

    public string? BackupPlans { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }

    public string? AttachmentsSubstitute { get; set; }

    public string? NTerm { get; set; }

    public int? RegisterProgramSetupId { get; set; }
}
