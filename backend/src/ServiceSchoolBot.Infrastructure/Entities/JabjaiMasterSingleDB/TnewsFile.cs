using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TnewsFile
{
    public int? NSms { get; set; }

    public string? SFileName { get; set; }

    public string? ContentType { get; set; }

    public int NNewsFileId { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? FullFileName { get; set; }

    public bool? StudentCodeOwner { get; set; }

    public string? StudentCodeFileName { get; set; }
}
