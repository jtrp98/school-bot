using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TclassOnline
{
    public int OnlineId { get; set; }

    public string? TermId { get; set; }

    public int? LevelId { get; set; }

    public int? PlanId { get; set; }

    public string? TitleName { get; set; }

    public int? TeacherId { get; set; }

    public int? SchoolId { get; set; }

    public string? SelectedRoom { get; set; }

    public bool? CDel { get; set; }

    public string? ShareId { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? Created { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? Modfied { get; set; }
}
