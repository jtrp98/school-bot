using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class SchoolTemployee
{
    public int SchoolId { get; set; }

    public int SEmp { get; set; }

    public int? NStudentStatus { get; set; }

    public string? SStudentId { get; set; }
}
