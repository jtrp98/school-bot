using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class PgtdowntimeAnnouncement
{
    public int AnnouncementId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool? IsActive { get; set; }
}
