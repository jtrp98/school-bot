using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ExcelExportDatum
{
    public int ExportId { get; set; }

    public string? Name { get; set; }

    public string? StudentId { get; set; }

    public int SchoolId { get; set; }

    public decimal NMoney { get; set; }

    public char? Status { get; set; }

    public DateTime Tstamp { get; set; }

    public bool Success { get; set; }

    public bool RecordExists { get; set; }

    public string? Remarks { get; set; }
}
