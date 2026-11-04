using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpreRegisterSurveyTopic
{
    public int TopicId { get; set; }

    public int SchoolId { get; set; }

    public string TopicCode { get; set; } = null!;

    public string TopicName { get; set; } = null!;

    public string? TopicNameEn { get; set; }

    public bool IsRequired { get; set; }

    public int? Order { get; set; }

    public bool IsActive { get; set; }

    public bool IsDel { get; set; }

    public DateTime CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public virtual ICollection<TpreRegisterSurveyOption> TpreRegisterSurveyOptions { get; set; } = new List<TpreRegisterSurveyOption>();
}
