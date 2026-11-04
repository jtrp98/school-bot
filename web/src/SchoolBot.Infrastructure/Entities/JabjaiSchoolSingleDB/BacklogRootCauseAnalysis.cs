using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class BacklogRootCauseAnalysis
{
    public int Id { get; set; }

    public int BacklogProjectId { get; set; }

    public string IssueKey { get; set; } = null!;

    public string IssueSummary { get; set; } = null!;

    public string RootCauseMarkdown { get; set; } = null!;

    public string ModelUsed { get; set; } = null!;

    public int RequestedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
