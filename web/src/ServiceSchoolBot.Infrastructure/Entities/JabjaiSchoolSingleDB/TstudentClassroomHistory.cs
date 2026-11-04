using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TstudentClassroomHistory
{
    public int NHistoryId { get; set; }

    public int? SId { get; set; }

    public string? NTerm { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public int? NStudentNumber { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public int? NStudentStatus { get; set; }

    public DateTime? MoveInDate { get; set; }

    public DateTime? MoveOutDate { get; set; }

    public int? DropOutType { get; set; }

    public bool? IsActive { get; set; }

    public string? Note { get; set; }
}
