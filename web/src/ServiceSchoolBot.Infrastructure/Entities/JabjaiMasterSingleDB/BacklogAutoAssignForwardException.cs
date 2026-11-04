using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class BacklogAutoAssignForwardException
{
    public int Id { get; set; }

    public int BacklogUserId { get; set; }

    public string BacklogUserName { get; set; } = null!;

    public string? BacklogMailAddress { get; set; }

    public string? Note { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
