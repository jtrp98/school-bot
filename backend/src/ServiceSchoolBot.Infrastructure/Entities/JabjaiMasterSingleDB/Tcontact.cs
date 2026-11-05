using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tcontact
{
    public int ContactId { get; set; }

    public int? SchoolId { get; set; }

    public string? SaleId { get; set; }

    public string? SupportId { get; set; }

    public int? StudentFree { get; set; }

    public int? StudentPay { get; set; }

    public int? StudentCount { get; set; }

    public short? PackageId { get; set; }

    public string? Remark { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDelete { get; set; }

    public string? ServiceType { get; set; }
}
