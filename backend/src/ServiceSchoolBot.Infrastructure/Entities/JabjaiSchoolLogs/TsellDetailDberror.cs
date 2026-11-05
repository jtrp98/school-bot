using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TsellDetailDberror
{
    public int SellDetailsId { get; set; }

    public int NSell { get; set; }

    public int NOrder { get; set; }

    public int? NProduct { get; set; }

    public int? NNumber { get; set; }

    public char? CDel { get; set; }

    public decimal? NPrice { get; set; }

    public decimal? NCost { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int DiscountPer { get; set; }

    public decimal DiscountPrice { get; set; }

    public string? Id { get; set; }

    public bool Processed { get; set; }
}
