using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tsm
{
    public int NSms { get; set; }

    public string? Smstitle { get; set; }

    public string? Smsdesp { get; set; }

    public int? Smstype { get; set; }

    public int? SmssubType { get; set; }

    public string? Smsdate { get; set; }

    public int? Smsduration { get; set; }

    public string? Smsstatus { get; set; }

    public string? Smsall { get; set; }

    public DateTime? DSend { get; set; }

    public string? ScheduledId { get; set; }

    public char? Smsemp { get; set; }

    public int? Useradd { get; set; }

    public bool? IsDel { get; set; }

    public int? NActionType { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public int? SmsgroupType { get; set; }

    public int? SmsgroupId { get; set; }

    public bool? SendToLine { get; set; }

    public bool? SendFileWithOwner { get; set; }

    public bool? SendFileWithOwnerOcr { get; set; }
}
