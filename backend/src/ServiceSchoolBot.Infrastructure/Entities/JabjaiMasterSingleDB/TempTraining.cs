using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TempTraining
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public string? ProjectName { get; set; }

    public string? TrainingName { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Place { get; set; }

    public int? ProvinceId { get; set; }

    public string? Country { get; set; }

    public decimal? Expenses { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public string? Province { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }

    public string? TrainingType { get; set; }

    public decimal? TrainingHours { get; set; }
}
