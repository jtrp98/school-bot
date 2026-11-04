using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TparentCard
{
    public int Id { get; set; }

    public int SId { get; set; }

    public int SchoolId { get; set; }

    public short No { get; set; }

    public short? Type { get; set; }

    public string? ParentName { get; set; }

    public string? Nfc { get; set; }

    public string? Nfcencrypt { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDel { get; set; }

    public bool? IsActive { get; set; }

    public string? Barcode { get; set; }
}
