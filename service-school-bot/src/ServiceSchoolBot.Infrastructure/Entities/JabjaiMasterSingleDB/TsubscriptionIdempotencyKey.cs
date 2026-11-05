using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TsubscriptionIdempotencyKey
{
    public string IdempotencyKey { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string? CallerSid { get; set; }

    public int ResponseStatus { get; set; }

    public string? ResponseBody { get; set; }

    public DateTime CreatedAt { get; set; }
}
