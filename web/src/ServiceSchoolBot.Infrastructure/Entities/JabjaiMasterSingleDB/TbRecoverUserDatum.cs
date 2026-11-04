using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbRecoverUserDatum
{
    public Guid RecoverId { get; set; }

    public string? Token { get; set; }

    public int? UserId { get; set; }

    public int? SchoolId { get; set; }

    public string? Imei { get; set; }

    public string? System { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? AppvoreStatus { get; set; }

    public int? AppvoreUserId { get; set; }

    public DateTime? AppvoreDate { get; set; }

    public string? Email { get; set; }

    public string? StudentPicture0 { get; set; }

    public string? StudentPicture1 { get; set; }

    public string? ParentPicture0 { get; set; }

    public string? ParentPicture1 { get; set; }

    public string? TeacherPicture0 { get; set; }

    public string? TeacherPicture1 { get; set; }

    public string? EmailPin { get; set; }

    public string? UserType { get; set; }

    public Guid? FdChangePasswordId { get; set; }
}
