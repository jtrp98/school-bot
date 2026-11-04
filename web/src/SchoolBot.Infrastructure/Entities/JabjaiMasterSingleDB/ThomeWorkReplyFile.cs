using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ThomeWorkReplyFile
{
    public int FileId { get; set; }

    public int HomeWorkId { get; set; }

    public int ReplyId { get; set; }

    public string? FileUrl { get; set; }

    public string? FileTitle { get; set; }

    public string? ContentType { get; set; }

    public int? SchoolId { get; set; }

    public bool? CDel { get; set; }

    public DateTime? CreateDate { get; set; }
}
