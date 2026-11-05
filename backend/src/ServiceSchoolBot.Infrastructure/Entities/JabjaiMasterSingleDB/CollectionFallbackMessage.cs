using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class CollectionFallbackMessage
{
    public Guid MessageId { get; set; }

    public Guid CaseId { get; set; }

    public string Channel { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public string? DeliveryStatus { get; set; }

    public string? Content { get; set; }

    public virtual CollectionCase Case { get; set; } = null!;
}
