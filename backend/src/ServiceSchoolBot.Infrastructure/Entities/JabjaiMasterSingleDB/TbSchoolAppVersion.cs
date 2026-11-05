using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbSchoolAppVersion
{
    public Guid Id { get; set; }

    public int? SchoolId { get; set; }

    public Guid? AppVersion { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int AppId { get; set; }
}
