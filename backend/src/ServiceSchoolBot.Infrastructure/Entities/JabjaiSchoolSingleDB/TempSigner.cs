using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TempSigner
{
    public int SignerId { get; set; }

    public int SchoolId { get; set; }

    public int? YearId { get; set; }

    public string? TermId { get; set; }

    public string? Position { get; set; }

    public string? PositionEn { get; set; }

    public int? DefaultId { get; set; }

    public int? EmpId { get; set; }

    public string? Signer { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsDel { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public string? OldSigner { get; set; }
}
