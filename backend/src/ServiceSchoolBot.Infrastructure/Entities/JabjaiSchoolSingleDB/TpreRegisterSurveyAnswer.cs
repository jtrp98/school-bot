using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpreRegisterSurveyAnswer
{
    public long AnswerId { get; set; }

    public int SchoolId { get; set; }

    public int PreRegisterId { get; set; }

    public int TopicId { get; set; }

    public int OptionId { get; set; }

    public string? OtherText { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual TpreRegisterSurveyOption Option { get; set; } = null!;
}
