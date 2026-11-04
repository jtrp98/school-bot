using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ApplicationLog
{
    public long Id { get; set; }

    public Guid? TraceId { get; set; }

    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// INFO, WARN, ERROR, FATAL
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// เช่น Auth, Database, PaymentFlow เอาไว้ Filter กลุ่มงาน
    /// </summary>
    public string? Tag { get; set; }

    public string Message { get; set; } = null!;

    /// <summary>
    /// เก็บ Contextual data เช่น ข้อมูลตัวแปร ณ ขณะนั้น
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// สำหรับ Error เท่านั้น
    /// </summary>
    public string? StackTrace { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual LogTrace? Trace { get; set; }
}
