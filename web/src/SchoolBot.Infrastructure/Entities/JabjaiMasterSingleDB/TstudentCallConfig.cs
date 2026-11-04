using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TstudentCallConfig
{
    public int SchoolId { get; set; }

    public string? Gate { get; set; }

    public string? BgCard { get; set; }

    public bool? IsActive { get; set; }

    public double? Radius { get; set; }

    public DateTime? Created { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? Modified { get; set; }

    public bool? IsShowLevel { get; set; }

    public bool? IsShowLastName { get; set; }

    public bool? IsShowParent { get; set; }

    public short? CardType { get; set; }

    public short? NameType { get; set; }

    public bool? IsEng { get; set; }
}
