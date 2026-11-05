using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class OnlineScanWorkerLog
{
    public long Id { get; set; }

    public string EnvelopeType { get; set; } = null!;

    public DateTime DequeuedAt { get; set; }

    public int QueueWaitMs { get; set; }

    public int ProcessMs { get; set; }

    public int RetryCount { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public int? SchoolId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? StepsJson { get; set; }

    public string? QueueId { get; set; }

    public string? InputJson { get; set; }

    public string? OutputSummary { get; set; }

    public string? WorkerId { get; set; }

    public long? QueueDepthAtDequeue { get; set; }

    public int? AvailableWorkerThreads { get; set; }

    public string? CorrelationId { get; set; }

    public string? MachineName { get; set; }
}
