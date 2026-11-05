using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TaccountJournal
{
    public int AccountJournalId { get; set; }

    public string? RefId { get; set; }

    public string? Code { get; set; }

    public string? Url { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public decimal? DebitCredit { get; set; }

    public string? Description { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }
}
