using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TdocumentStampDate
{
    public int Id { get; set; }

    public int? SchoolId { get; set; }

    public string? DocType { get; set; }

    public DateTime? DocDate { get; set; }
}
