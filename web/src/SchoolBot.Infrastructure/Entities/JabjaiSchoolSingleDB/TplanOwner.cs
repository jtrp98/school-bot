using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TplanOwner
{
    public int NPlanOwnerId { get; set; }

    public int SEmp { get; set; }

    public int ChangeScoreAnytime { get; set; }

    public int? CDel { get; set; }

    public int SPlaneId { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
