using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TteacherStudentType
{
    public int Id { get; set; }

    public int StudentTypeId { get; set; }

    public int SchoolId { get; set; }

    public int SEmp { get; set; }

    public bool Status { get; set; }

    public bool IsDel { get; set; }

    public DateTime CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
