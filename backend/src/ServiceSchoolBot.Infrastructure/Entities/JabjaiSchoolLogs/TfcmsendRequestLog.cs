using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class TfcmsendRequestLog
{
    public long Id { get; set; }

    public string RequestId { get; set; } = null!;

    public Guid? SendCorrelationId { get; set; }

    public int? SchoolId { get; set; }

    public int PushMid { get; set; }

    public int RequestedTokenCount { get; set; }

    public int ValidTokenCountAfterFilter { get; set; }

    public string Stage { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }
}
