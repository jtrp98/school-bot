using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tmessage
{
    public int NMessageId { get; set; }

    public int UserId { get; set; }

    public string? SMessage { get; set; }

    public string? STitle { get; set; }

    public int? NType { get; set; }

    public DateTime? DSend { get; set; }

    public int? NStatus { get; set; }

    public string? ScheduledId { get; set; }

    public int? PushId { get; set; }

    public int? HomeworkId { get; set; }

    public char? CDel { get; set; }

    public int? SellId { get; set; }

    public int? TopupId { get; set; }
}
