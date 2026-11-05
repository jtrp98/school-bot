using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tadmin
{
    public string Id { get; set; } = null!;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Picture { get; set; }

    public bool? Del { get; set; }

    public string? Name { get; set; }

    public bool? Active { get; set; }

    public string? Email { get; set; }

    public string? Tel { get; set; }

    public int Id2 { get; set; }

    public short? RoleId { get; set; }

    public int? OfficerId { get; set; }

    public string? Code { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? NickName { get; set; }

    public string? Position { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public string? BacklogEmailAccount { get; set; }

    public string? LineUserId { get; set; }
}
