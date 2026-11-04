using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tdocument
{
    public long DocumentId { get; set; }

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

    public DateTime? ApprovedUserId { get; set; }

    public DateTime? ApprovedDateTime { get; set; }
}
