using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class ViewTopupMemoryDatum
{
    public string? UserName { get; set; }

    public string? UserId { get; set; }

    public string? UserType { get; set; }

    public int? SchoolId { get; set; }

    public string? Keyword { get; set; }

    public int? KeywordOrder { get; set; }

    public bool? Active { get; set; }
}
