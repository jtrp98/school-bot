using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TbackupCard
{
    public Guid CardId { get; set; }

    public int SchoolId { get; set; }

    public string? CardName { get; set; }

    public string? BarCode { get; set; }

    public string? Nfc { get; set; }

    public string? Nfcreverse { get; set; }

    public string? Nfcencrypt { get; set; }

    public string? NfcencryptReverse { get; set; }

    public decimal? Money { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }

    public decimal? Insurance { get; set; }
}
