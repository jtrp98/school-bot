using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tofficer
{
    public int OfficerId { get; set; }

    public string? Code { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? NickName { get; set; }

    public string? Position { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }
}
