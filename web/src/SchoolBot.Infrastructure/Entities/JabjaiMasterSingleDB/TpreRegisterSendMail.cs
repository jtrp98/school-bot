using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TpreRegisterSendMail
{
    public int SendMailId { get; set; }

    public int PreRegisterId { get; set; }

    public string? SendTo { get; set; }

    public string? SendFrom { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public DateTime? SendDate { get; set; }

    public int? SendBy { get; set; }

    public virtual ICollection<TpreRegisterSendMailAttachment> TpreRegisterSendMailAttachments { get; set; } = new List<TpreRegisterSendMailAttachment>();
}
