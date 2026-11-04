using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpreRegisterRequiredCategory
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryNameEn { get; set; }

    public int? Order { get; set; }

    public bool? IsDel { get; set; }

    public DateTime? CreateDate { get; set; }
}
