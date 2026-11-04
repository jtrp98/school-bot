using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TstudentCallGate
{
    public Guid Token { get; set; }

    public int? SchoolId { get; set; }

    public short? Gate { get; set; }

    public string? GateName { get; set; }

    public string? SelectedRoom { get; set; }

    public string? FullUrl { get; set; }

    public string? ShortUrl { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDel { get; set; }
}
