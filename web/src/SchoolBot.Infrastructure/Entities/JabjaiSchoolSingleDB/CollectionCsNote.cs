using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class CollectionCsNote
{
    public Guid NoteId { get; set; }

    public Guid CaseId { get; set; }

    public int AdminId { get; set; }

    public string Note { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public virtual CollectionCase Case { get; set; } = null!;
}
