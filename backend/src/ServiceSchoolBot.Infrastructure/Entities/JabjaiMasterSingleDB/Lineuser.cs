using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Lineuser
{
    public int StudentId { get; set; }

    public string LineuserId { get; set; } = null!;

    public string? RichMenuId { get; set; }

    public int? SchoolId { get; set; }

    public DateTime? RegisterDate { get; set; }

    public string? Status { get; set; }

    public string? Os { get; set; }

    public DateTime? UpdateDate { get; set; }
}
