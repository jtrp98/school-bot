using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TchangeRequest
{
    public int ChangeRequestId { get; set; }

    public string DocumentNo { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string ModuleName { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }
}
