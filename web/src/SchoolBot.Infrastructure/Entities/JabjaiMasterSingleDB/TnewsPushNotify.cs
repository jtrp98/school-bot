using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TnewsPushNotify
{
    public int PushId { get; set; }

    public int NewsId { get; set; }

    public int UserId { get; set; }

    public int UserType { get; set; }

    public DateTime? SendDate { get; set; }

    public DateTime? ReadDate { get; set; }

    public int SchoolId { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifyBy { get; set; }

    public bool IsDelete { get; set; }
}
