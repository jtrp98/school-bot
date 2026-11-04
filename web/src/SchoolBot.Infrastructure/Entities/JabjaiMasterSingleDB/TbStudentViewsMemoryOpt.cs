using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbStudentViewsMemoryOpt
{
    public int? SId { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public int? SchoolId { get; set; }

    public string? SStudentId { get; set; }

    public string? NTerm { get; set; }
}
