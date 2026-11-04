using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbVerifierDevice
{
    public Guid FdVerifierId { get; set; }

    public string? FdData { get; set; }

    public int? FdChanelMessage { get; set; }

    public int? FdUserId { get; set; }

    public int? FdSchoolId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? FdStatus { get; set; }
}
