using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ThomeWorkReply
{
    public int ReplyId { get; set; }

    public int? HomeWorkId { get; set; }

    public int? UserId { get; set; }

    public int? ReplyRefId { get; set; }

    public string? Comment { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public short? Type { get; set; }

    public int? SchoolId { get; set; }

    public int? SId { get; set; }

    public int? TId { get; set; }
}
