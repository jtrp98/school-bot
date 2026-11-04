using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Taverment
{
    public int NAvermentId { get; set; }

    public string? SAverment { get; set; }

    public string? NCompany { get; set; }

    public string? SMac { get; set; }

    public DateTime? DAdd { get; set; }

    public int? CType { get; set; }

    public int? NStatus { get; set; }
}
