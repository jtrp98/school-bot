using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class StudentSrc
{
    public int? Sid { get; set; }

    public int? NGradeId { get; set; }

    public string? Sname { get; set; }

    public string? Slastname { get; set; }

    public string? SstudentTitle { get; set; }

    public string? Sterm { get; set; }

    public string? SstudentId { get; set; }

    public int? NstudentNumber { get; set; }

    public string? SstudentNameEn { get; set; }

    public string? SstudentLastEn { get; set; }

    public int? NstudentStatus { get; set; }

    public string? Nterm { get; set; }
}
