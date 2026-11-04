using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class OfflineSalesBackup
{
    public Guid SalesId { get; set; }

    public int SchoolId { get; set; }

    public int EmployeeId { get; set; }

    public int ShopId { get; set; }

    public int UserId { get; set; }

    public string CType { get; set; } = null!;

    public decimal SalesAmount { get; set; }

    public DateTime SalesDate { get; set; }

    public Guid? TempCardHistoryId { get; set; }

    public Guid? TempCardId { get; set; }

    public string? DeviceId { get; set; }

    public bool Processed { get; set; }

    public DateTime Tstamp { get; set; }

    public string? SalesProductInfo { get; set; }

    public string? Barcode { get; set; }

    public Guid? CardId { get; set; }

    public string? SourceIp { get; set; }
}
