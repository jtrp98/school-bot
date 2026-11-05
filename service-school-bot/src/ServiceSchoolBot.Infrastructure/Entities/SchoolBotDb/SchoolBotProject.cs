using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotProject
{
    public int SchoolBotProjectId { get; set; }

    public string OwnerKind { get; set; } = null!;

    public int OwnerId { get; set; }

    public string Mode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int CurrentRevision { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }
}
