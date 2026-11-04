using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TempToeic
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public int? Toeicscore { get; set; }

    public string? InstitutionAnnouncement { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool CDel { get; set; }
}
