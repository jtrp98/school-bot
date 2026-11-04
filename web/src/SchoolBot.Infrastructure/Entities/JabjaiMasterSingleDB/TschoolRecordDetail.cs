using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TschoolRecordDetail
{
    public int NSchoolRecordId { get; set; }

    public int SPlaneId { get; set; }

    public string? Grade { get; set; }

    public string? ReGrade { get; set; }

    public string? Note { get; set; }

    public string? Score { get; set; }

    public string? MaxScore { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
