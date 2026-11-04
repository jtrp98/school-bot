using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class SchoolTuser
{
    public int? SchoolId { get; set; }

    public int? SId { get; set; }

    public int? NStudentStatus { get; set; }

    public string? SStudentId { get; set; }
}
