using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tparent
{
    public int ParentId { get; set; }

    public string? ParentTitle { get; set; }

    public string ParentName { get; set; } = null!;

    public string ParentLastname { get; set; } = null!;

    public string? Tel { get; set; }

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string CreatedAt { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public bool CDel { get; set; }

    public bool IsVerified { get; set; }

    public virtual ICollection<TparentStudent> TparentStudents { get; set; } = new List<TparentStudent>();
}
