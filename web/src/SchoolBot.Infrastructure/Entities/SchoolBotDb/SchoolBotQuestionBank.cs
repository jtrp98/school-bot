using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotQuestionBank
{
    public int SchoolBotQuestionBankId { get; set; }

    public Guid BankKey { get; set; }

    public int Revision { get; set; }

    public string Title { get; set; } = null!;

    public string Questions { get; set; } = null!;

    public string SourceRef { get; set; } = null!;

    public string Lifecycle { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
