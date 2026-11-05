using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TlogError
{
    public DateTime DLogError { get; set; }

    public string? SLogError { get; set; }

    public string? SFunction { get; set; }
}
