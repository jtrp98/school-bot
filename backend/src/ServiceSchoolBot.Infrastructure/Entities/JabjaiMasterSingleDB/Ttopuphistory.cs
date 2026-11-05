using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Ttopuphistory
{
    public int HistoryId { get; set; }

    public string? OrderNo { get; set; }

    public int? UserId { get; set; }

    public int? TradeMony { get; set; }

    public string? HashData { get; set; }

    public string? PayType { get; set; }

    public string? SiteCd { get; set; }

    public string? RetUrl { get; set; }

    public string? GoodName { get; set; }

    public string? OrderFirstName { get; set; }

    public string? OrderLastName { get; set; }

    public string? OrderEmail { get; set; }

    public string? Currency { get; set; }

    public int? Fee { get; set; }

    public string? ResCd { get; set; }

    public string? ResMsg { get; set; }

    public string? Tno { get; set; }

    public decimal? Money { get; set; }
}
