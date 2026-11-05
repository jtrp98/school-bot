using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TuserCard
{
    public int Id { get; set; }

    public int SId { get; set; }

    public int SchoolId { get; set; }

    public short? No { get; set; }

    public string? Nfc { get; set; }

    public string? Nfcreverse { get; set; }

    public string? Nfcencrypt { get; set; }

    public string? NfcencryptReverse { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDel { get; set; }

    public bool? IsActive { get; set; }

    public string? FreeText { get; set; }
}
