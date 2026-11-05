using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class ThomeWorkLearningFile
{
    public int AttachId { get; set; }

    public int? LearnId { get; set; }

    public string? SFileName { get; set; }

    public string? ContentType { get; set; }

    public string? Title { get; set; }

    public int? SchoolId { get; set; }

    public bool? CDel { get; set; }
}
