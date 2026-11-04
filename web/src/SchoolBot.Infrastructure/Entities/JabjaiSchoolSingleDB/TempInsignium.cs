using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TempInsignium
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public int? Year { get; set; }

    public string? Grade { get; set; }

    public string? Position { get; set; }

    public string? BookNumber { get; set; }

    public string? Part { get; set; }

    public string? Duty { get; set; }

    public string? Number { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }
}
