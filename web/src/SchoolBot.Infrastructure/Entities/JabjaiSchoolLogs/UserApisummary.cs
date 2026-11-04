using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class UserApisummary
{
    public int Id { get; set; }

    public string? Path { get; set; }

    public int? Requestcount { get; set; }

    public string? Applicationname { get; set; }

    public DateOnly? Businessdate { get; set; }

    public int? Schoolid { get; set; }
}
