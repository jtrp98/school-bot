using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpreRegisterSurveyOption
{
    public int OptionId { get; set; }

    public int TopicId { get; set; }

    public string OptionCode { get; set; } = null!;

    public string OptionName { get; set; } = null!;

    public string? OptionNameEn { get; set; }

    public bool IsOther { get; set; }

    public int? Order { get; set; }

    public bool IsActive { get; set; }

    public bool IsDel { get; set; }

    public DateTime CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public virtual TpreRegisterSurveyTopic Topic { get; set; } = null!;

    public virtual ICollection<TpreRegisterSurveyAnswer> TpreRegisterSurveyAnswers { get; set; } = new List<TpreRegisterSurveyAnswer>();
}
