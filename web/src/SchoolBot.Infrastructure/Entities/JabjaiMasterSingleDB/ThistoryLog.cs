using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ThistoryLog
{
    public int Id { get; set; }

    public int? SchoolId { get; set; }

    public DateTime? Date { get; set; }

    public int? MenuId { get; set; }

    public string? FunctionName { get; set; }

    public int? SId { get; set; }

    public int? SEmp { get; set; }

    public int? UpdateBy { get; set; }

    public string? JsonDetail { get; set; }
}
