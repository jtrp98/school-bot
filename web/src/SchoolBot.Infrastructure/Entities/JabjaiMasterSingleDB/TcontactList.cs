using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcontactList
{
    public int ContactListId { get; set; }

    public int? SchoolId { get; set; }

    public string? Episode { get; set; }

    public string? ContactNo { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public short? Period { get; set; }

    public short? RemainDay { get; set; }

    public string? Status { get; set; }

    public string? StatusOther { get; set; }

    public string? ServiceCharge { get; set; }

    public string? Vat { get; set; }

    public short? StudentAmount { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDelete { get; set; }
}
