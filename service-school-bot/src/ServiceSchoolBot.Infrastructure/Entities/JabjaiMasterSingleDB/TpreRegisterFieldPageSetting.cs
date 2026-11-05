using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TpreRegisterFieldPageSetting
{
    public int SchoolId { get; set; }

    public int Vfiid { get; set; }

    public short PageScopeId { get; set; }

    public int? CategoryId { get; set; }

    public bool IsVisible { get; set; }

    public bool IsRequired { get; set; }

    public bool AllowSkip { get; set; }

    public bool IsDelete { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }
}
