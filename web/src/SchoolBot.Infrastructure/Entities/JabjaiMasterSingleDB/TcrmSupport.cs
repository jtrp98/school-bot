using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmSupport
{
    public int CrmSupportId { get; set; }

    public DateTime? IssueDate { get; set; }

    public int? SchoolId { get; set; }

    public string? Channel { get; set; }

    public int? ContactId { get; set; }

    public string? Type { get; set; }

    public string? SubType { get; set; }

    public string? RefCode { get; set; }

    public string? SupportDetail { get; set; }

    public string? Subject { get; set; }

    public string? Question { get; set; }

    public string? Answer { get; set; }

    public bool? IsFollowUp { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public int? BacklogProjectId { get; set; }

    public int? BacklogIssueId { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }

    public string? LineInfo { get; set; }

    public Guid? Session { get; set; }

    public bool? HasRemindFollowUp { get; set; }

    public DateTime? FollowUpEndDate { get; set; }

    public string? FollowUpFrequency { get; set; }

    public DateTime? CustomerFollowUpDate { get; set; }

    public DateTime? CustomerFollowUpEndDate { get; set; }

    public string? CustomerFollowUpFrequency { get; set; }

    public string? CustomerNotifyMessage { get; set; }

    public int? AssignStaffId { get; set; }

    public string? Note { get; set; }

    public string? Onboarding { get; set; }

    public string? ReferenceKey { get; set; }

    public string? ReferenceValue { get; set; }

    public string? BacklogModel { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }
}
