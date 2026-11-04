using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmReminder
{
    public int CrmReminderId { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }
}
