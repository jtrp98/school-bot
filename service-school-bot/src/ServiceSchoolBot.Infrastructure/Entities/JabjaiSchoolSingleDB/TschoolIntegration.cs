using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TschoolIntegration
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string? ApiKeyEncrypt { get; set; }

    public string Provider { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? CDel { get; set; }
}
