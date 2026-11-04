using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TroomChange
{
    public string RoomChangeId { get; set; } = null!;

    public int? SId { get; set; }

    public int? Level2Old { get; set; }

    public int? Level2New { get; set; }

    public DateOnly? DayChange { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
