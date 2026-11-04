using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TlineMessage
{
    public int LineMessageId { get; set; }

    public int? SchoolId { get; set; }

    public string? Type { get; set; }

    public string? UserId { get; set; }

    public string? GroupId { get; set; }

    public string? Text { get; set; }

    public DateTime? CreateDate { get; set; }

    public Guid? Session { get; set; }

    public string? ReferenceKey { get; set; }

    public string? ReferenceValue { get; set; }

    public string? Model { get; set; }
}
