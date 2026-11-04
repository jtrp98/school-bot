using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TFsdqAnswer
{
    public int TFsdqAnswerId { get; set; }

    public int? TFsdqQuestionId { get; set; }

    public int? TFsdqPointPoint { get; set; }

    public int? SId { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }
}
