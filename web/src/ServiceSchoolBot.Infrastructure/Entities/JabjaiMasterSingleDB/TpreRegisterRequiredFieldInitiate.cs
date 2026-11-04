using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TpreRegisterRequiredFieldInitiate
{
    public int Vfiid { get; set; }

    public int? CategoryId { get; set; }

    public string? No { get; set; }

    public bool? IsHead { get; set; }

    public string? FieldName { get; set; }

    public string? FieldNameEn { get; set; }

    public string? InputFieldName { get; set; }

    public bool? DefaultStatus { get; set; }

    public int? Order { get; set; }

    public bool? IsDel { get; set; }

    public DateTime? CreateDate { get; set; }
}
