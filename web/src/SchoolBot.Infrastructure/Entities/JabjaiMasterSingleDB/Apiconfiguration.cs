using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Apiconfiguration
{
    public int ConfigurationId { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
