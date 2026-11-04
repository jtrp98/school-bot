using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ShortenUrl
{
    public int UrlId { get; set; }

    public string UrlString { get; set; } = null!;

    public string ShortUrl { get; set; } = null!;

    public DateTime Tstamp { get; set; }
}
