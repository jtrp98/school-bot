using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TimportDocument
{
    public long ImportDocumentId { get; set; }

    public string? SourceChannel { get; set; }

    public string? SourceMessageId { get; set; }

    public string? SourceGroupId { get; set; }

    public string? SenderId { get; set; }

    public string? SenderDisplayName { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }

    public string? GdriveFileId { get; set; }

    public string? GdriveWebViewLink { get; set; }

    public string? GdriveDownloadLink { get; set; }

    public string? FileCategory { get; set; }

    public string? ProcessStatus { get; set; }

    public string? Metadata { get; set; }

    public int? SchoolId { get; set; }

    public DateTime? InsertedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }

    public bool? IsApproved { get; set; }

    public string? ApprovalValidationMessage { get; set; }

    public int? ApprovedUserId { get; set; }

    public DateTime? ApprovedDateTime { get; set; }

    public bool? IsRejected { get; set; }

    public int? RejectedUserId { get; set; }

    public DateTime? RejectedDateTime { get; set; }

    public string? ExtractedData { get; set; }

    public long? LatestImportDocumentNotificationId { get; set; }

    public long? WaitingApproveImportDocumentNotificationId { get; set; }

    public string? ImportResult { get; set; }
}
