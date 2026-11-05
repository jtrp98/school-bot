using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TfcmsendLog
{
    public long Id { get; set; }

    public Guid SendCorrelationId { get; set; }

    public int? SchoolId { get; set; }

    public int PushMid { get; set; }

    public int PushMtype { get; set; }

    public int? PushUserId { get; set; }

    public int BatchNumber { get; set; }

    public int BatchTotal { get; set; }

    public string? Token { get; set; }

    public int? UserId { get; set; }

    public bool IsSuccess { get; set; }

    public string? MessageId { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string SendMode { get; set; } = null!;

    public string? NotificationTitle { get; set; }

    public string? NotificationBody { get; set; }

    public long? BatchElapsedMs { get; set; }

    public DateTime CreatedDate { get; set; }
}
