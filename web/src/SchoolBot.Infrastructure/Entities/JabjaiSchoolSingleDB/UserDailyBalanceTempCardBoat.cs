using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class UserDailyBalanceTempCardBoat
{
    public int? SchoolId { get; set; }

    public int? SId { get; set; }

    public string? SStudentId { get; set; }

    public string? SName { get; set; }

    public string? SLastname { get; set; }

    public char? CDel { get; set; }

    public char? CType { get; set; }

    public DateOnly? BusinessDate { get; set; }

    public decimal? OpeningBalance { get; set; }

    public decimal? TotalTopUp { get; set; }

    public decimal? TotalCancelTopUp { get; set; }

    public decimal? TotalWithDraw { get; set; }

    public decimal? TotalCancelWithDraw { get; set; }

    public decimal? TotalSales { get; set; }

    public decimal? TotalCancelSales { get; set; }

    public DateOnly? LastUpdatedDate { get; set; }

    public decimal? Balance { get; set; }

    public Guid? CardHistoryId { get; set; }

    public decimal? OfflineSales { get; set; }

    public double? Insurance { get; set; }

    public double? SystemBalance { get; set; }

    public int Id { get; set; }
}
