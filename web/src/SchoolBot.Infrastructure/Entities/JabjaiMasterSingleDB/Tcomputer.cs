using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tcomputer
{
    public int NComputerId { get; set; }

    public string? SComputerName { get; set; }

    public string? SMac { get; set; }

    public int? NCompany { get; set; }

    public char? CType { get; set; }

    public string? SToken { get; set; }

    public char? CStatus { get; set; }
}
