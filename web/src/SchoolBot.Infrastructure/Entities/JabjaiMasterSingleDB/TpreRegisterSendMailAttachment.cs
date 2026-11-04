using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TpreRegisterSendMailAttachment
{
    public int PreRegisterSendMailAttachmentId { get; set; }

    public int SendMailId { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public string? FilePath { get; set; }

    public long? FileSize { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDel { get; set; }

    public virtual TpreRegisterSendMail SendMail { get; set; } = null!;
}
