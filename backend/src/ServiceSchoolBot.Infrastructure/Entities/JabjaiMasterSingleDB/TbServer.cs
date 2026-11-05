using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbServer
{
    public Guid Id { get; set; }

    public string? Message { get; set; }

    public bool? Status { get; set; }

    public string? ServerName { get; set; }

    public string? Url { get; set; }
}
