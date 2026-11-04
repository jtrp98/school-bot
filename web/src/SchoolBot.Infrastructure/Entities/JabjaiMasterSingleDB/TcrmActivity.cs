using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmActivity
{
    public int CrmActivityId { get; set; }

    public int? SchoolId { get; set; }

    public string? SchoolName { get; set; }

    public string? Type { get; set; }

    public string? ActivityType { get; set; }

    public string? MeetingType { get; set; }

    public string? ActivityTitle { get; set; }

    public string? ActivityDescription { get; set; }

    public string? ActivityStatus { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsRecurring { get; set; }

    public string? Map { get; set; }

    public string? LatLng { get; set; }

    public string? AttachUrl { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }
}
