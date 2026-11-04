using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TgradeRegisterPeriod
{
    public int NGradeRegisterPeriod { get; set; }

    public string NTerm { get; set; } = null!;

    public DateTime? BeforeMidtermStart { get; set; }

    public DateTime? BeforeMidtermEnd { get; set; }

    public DateTime? DuringMidtermStart { get; set; }

    public DateTime? DuringMidtermEnd { get; set; }

    public DateTime? AfterMidtermStart { get; set; }

    public DateTime? AfterMidtermEnd { get; set; }

    public DateTime? FinaltermStart { get; set; }

    public DateTime? FinaltermEnd { get; set; }

    public DateTime? ExtraStart { get; set; }

    public DateTime? ExtraEnd { get; set; }

    public DateTime LastUpdate { get; set; }

    public int UpdateByEmp { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
