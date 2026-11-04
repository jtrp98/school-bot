using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class Pgtreportsetting
{
    public int Reportsettingid { get; set; }

    public int Nyear { get; set; }

    public string Numberyear { get; set; } = null!;

    public int Classlevelid { get; set; }

    public string Classlevelname { get; set; } = null!;

    public string Reporttype { get; set; } = null!;

    public int? Reporttypeid { get; set; }

    public string Settingname { get; set; } = null!;

    public string Settings { get; set; } = null!;

    public int Createdby { get; set; }

    public int Updatedby { get; set; }

    public bool? Cdel { get; set; }

    public int Schoolid { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }
}
