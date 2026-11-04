using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tnews
{
    public int NewsId { get; set; }

    public short? Type { get; set; }

    public short? ToType { get; set; }

    public string? Title { get; set; }

    public string? Detail { get; set; }

    public string? Remark { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsEmail { get; set; }

    public bool? IsBroadcast { get; set; }

    public bool? IsPopup { get; set; }

    public string? PopupUrl { get; set; }

    public bool? IsBanner { get; set; }

    public string? BannerUrl { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsDone { get; set; }

    public int? SchoolId { get; set; }
}
