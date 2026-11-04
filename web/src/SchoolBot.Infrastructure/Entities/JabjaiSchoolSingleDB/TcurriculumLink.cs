using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TcurriculumLink
{
    public int CurriculumLinkId { get; set; }

    public int? Nyear { get; set; }

    public string? Nterm { get; set; }

    public int? NtsubLevel { get; set; }

    public int? SchoolId { get; set; }

    public int? InsertedUserId { get; set; }

    public DateTime? InsertedDateTime { get; set; }
}
