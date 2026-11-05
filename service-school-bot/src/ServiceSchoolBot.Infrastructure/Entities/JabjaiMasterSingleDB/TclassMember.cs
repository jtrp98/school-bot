using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TclassMember
{
    public int NClassMemberid { get; set; }

    public int? NTeacherHeadid { get; set; }

    public int? NTeacherAssistOne { get; set; }

    public int? NTeacherAssistTwo { get; set; }

    public int? NTermSubLevel2 { get; set; }

    public string? NTerm { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
