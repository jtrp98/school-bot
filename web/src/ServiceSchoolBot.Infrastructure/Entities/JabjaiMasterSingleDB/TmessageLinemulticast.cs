using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmessageLinemulticast
{
    public int MessageId { get; set; }

    public int GroupId { get; set; }

    public string? StreamId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }
}
