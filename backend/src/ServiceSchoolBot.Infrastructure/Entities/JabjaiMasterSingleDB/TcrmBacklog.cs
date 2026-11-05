using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmBacklog
{
    public int CrmBacklogId { get; set; }

    public string? Json { get; set; }

    public DateTime? CreateDate { get; set; }

    public bool? HasUpdate { get; set; }
}
