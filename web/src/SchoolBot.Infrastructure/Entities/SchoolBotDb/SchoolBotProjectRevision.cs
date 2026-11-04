using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotProjectRevision
{
    public int SchoolBotProjectRevisionId { get; set; }

    public int ProjectId { get; set; }

    public int Revision { get; set; }

    public string Content { get; set; } = null!;

    public string ContentHash { get; set; } = null!;

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
