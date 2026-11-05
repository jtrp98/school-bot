using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionPromiseToPay
{
    public Guid PromiseId { get; set; }

    public Guid CaseId { get; set; }

    public Guid? AttemptId { get; set; }

    public DateTime PromiseDate { get; set; }

    public decimal? PromisedAmount { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? Result { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual CollectionCase Case { get; set; } = null!;
}
