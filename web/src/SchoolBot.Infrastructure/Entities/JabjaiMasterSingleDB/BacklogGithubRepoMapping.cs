using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class BacklogGithubRepoMapping
{
    public int Id { get; set; }

    public int BacklogProjectId { get; set; }

    public string BacklogProjectKey { get; set; } = null!;

    public string BacklogProjectName { get; set; } = null!;

    public string GithubOwner { get; set; } = null!;

    public string GithubRepo { get; set; } = null!;

    public string DefaultBranch { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? RepoRole { get; set; }

    public string? RepoNote { get; set; }
}
