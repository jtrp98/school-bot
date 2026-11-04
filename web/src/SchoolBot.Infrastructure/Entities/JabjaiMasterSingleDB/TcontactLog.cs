using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcontactLog
{
    public int LogId { get; set; }

    public int? SchoolId { get; set; }

    public int? ContactId { get; set; }

    public int? ContactListId { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public string? JsonLog { get; set; }

    public string? DataModel { get; set; }
}
