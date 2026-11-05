using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TimportDocumentNotification
{
    public long ImportDocumentNotificationId { get; set; }

    public long? ImportDocumentId { get; set; }

    public string? Channel { get; set; }

    public string? EventType { get; set; }

    public string? Status { get; set; }

    public string? Message { get; set; }

    public string? Payload { get; set; }

    public int? SentCount { get; set; }

    public DateTime? SentDateTime { get; set; }

    public DateTime? InsertedDateTime { get; set; }
}
