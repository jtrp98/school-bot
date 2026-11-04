using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpermissionCard
{
    public int Id { get; set; }

    public int? RefNo { get; set; }

    public int? SchoolId { get; set; }

    public int? StudentId { get; set; }

    public int? TypeId { get; set; }

    public short? Time { get; set; }

    public bool? IsAttach { get; set; }

    public string? AttachUrl { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Cause { get; set; }

    public string? Note { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? Creator { get; set; }

    public int? Modifier { get; set; }

    public bool? IsDel { get; set; }
}
