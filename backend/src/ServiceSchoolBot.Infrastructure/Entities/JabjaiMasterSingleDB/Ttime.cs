using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Ttime
{
    public int TimeId { get; set; }

    public DateTime? DTimeStartIn { get; set; }

    public DateTime? DTimeEndIn { get; set; }

    public DateTime? DTimeStartOut { get; set; }

    public DateTime? DTimeEndOut { get; set; }

    public int? NDay { get; set; }

    public char? CDel { get; set; }

    public int? NTimeType { get; set; }

    public int? NTimeLate { get; set; }

    public DateTime? DTimeHalf { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
