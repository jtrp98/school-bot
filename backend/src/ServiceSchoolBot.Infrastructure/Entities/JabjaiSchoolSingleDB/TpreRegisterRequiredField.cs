using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpreRegisterRequiredField
{
    public int Vfiid { get; set; }

    public int? CategoryId { get; set; }

    public int SchoolId { get; set; }

    public bool? Status { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
