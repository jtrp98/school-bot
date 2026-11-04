using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TteacherCardInfo
{
    public int TeacherCardInfoId { get; set; }

    public string? ElementValue { get; set; }

    public string? ElementName { get; set; }

    public DateTime? Date { get; set; }

    public int? CDel { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
