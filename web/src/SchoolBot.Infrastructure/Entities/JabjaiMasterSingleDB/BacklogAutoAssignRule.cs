using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class BacklogAutoAssignRule
{
    public int Id { get; set; }

    public int BacklogProjectId { get; set; }

    public string BacklogProjectKey { get; set; } = null!;

    public string BacklogProjectName { get; set; } = null!;

    public int AssigneeUserId { get; set; }

    public string AssigneeName { get; set; } = null!;

    public string? AssigneeMailAddress { get; set; }

    public bool OverrideExisting { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
