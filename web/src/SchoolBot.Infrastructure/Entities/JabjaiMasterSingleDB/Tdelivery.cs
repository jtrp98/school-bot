using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tdelivery
{
    public int DeliveryId { get; set; }

    public int? SchoolId { get; set; }

    public string? RecipientType { get; set; }

    public string? TransmissionType { get; set; }

    public DateTime? TransmissionDate { get; set; }

    public string? TransportationName { get; set; }

    public string? TrackingNo { get; set; }

    public string? RecipientName { get; set; }

    public string? RecipientTel { get; set; }

    public string? RefNo { get; set; }

    public string? Note { get; set; }

    public string? AttachUrl { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }

    public string? NotifyMessage { get; set; }

    public DateTime? NotifyDate { get; set; }
}
