using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ApplicationUrldatum
{
    public int Id { get; set; }

    public string ApplicationType { get; set; } = null!;

    public string ProdUrl { get; set; } = null!;

    public string DevUrl { get; set; } = null!;

    public DateTime Tstamp { get; set; }

    public string? UatUrl { get; set; }

    public string? AlphaUrl { get; set; }
}
