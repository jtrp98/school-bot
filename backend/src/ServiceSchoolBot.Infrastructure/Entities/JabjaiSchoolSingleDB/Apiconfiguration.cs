using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class Apiconfiguration
{
    public int ConfigurationId { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}
