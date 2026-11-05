using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Ttimetype
{
    public int NTimeType { get; set; }

    public string? STimeType { get; set; }

    public char? CDel { get; set; }

    public char? CType { get; set; }

    public char? CUserType { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
