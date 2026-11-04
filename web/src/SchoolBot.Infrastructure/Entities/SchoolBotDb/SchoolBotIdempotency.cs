using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.SchoolBotDb;

public partial class SchoolBotIdempotency
{
    public int SchoolBotIdempotencyId { get; set; }

    public string Principal { get; set; } = null!;

    public string Operation { get; set; } = null!;

    public Guid RequestKey { get; set; }

    public string RequestHash { get; set; } = null!;

    public string? NonceHash { get; set; }

    public int HttpStatus { get; set; }

    public byte[]? ResultCipher { get; set; }

    public string? ResultKeyId { get; set; }

    public int? ResourceId { get; set; }

    public DateTime CompletedAt { get; set; }

    public DateTime ReplayUntil { get; set; }

    public bool IsDelete { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? SchoolId { get; set; }
}
