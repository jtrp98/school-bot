using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TempCard
{
    public int? SchoolId { get; set; }

    public int? UserId { get; set; }

    public int? UserType { get; set; }

    public Guid? CardId { get; set; }

    public string? CardName { get; set; }

    public string? BarCode { get; set; }

    public string? Nfc { get; set; }

    public decimal? Money { get; set; }

    public string? Nfcencrypt { get; set; }

    public bool? CDel { get; set; }

    public Guid? CardHistoryId { get; set; }

    public string? NfcencryptReverse { get; set; }

    public string? Nfcreverse { get; set; }

    public DateTime? ReturnDate { get; set; }
}
