using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class SchoolContact
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Nickname { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
