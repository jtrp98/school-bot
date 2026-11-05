using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TlineBotUserSession
{
    public string UserId { get; set; } = null!;

    public string? CurrentStep { get; set; }

    public long? ImportDocumentId { get; set; }

    public string? TempData { get; set; }

    public DateTime? InsertedDateTime { get; set; }
}
