using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TsmsgroupUser
{
    public int SmsgroupId { get; set; }

    public int UserId { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }
}
