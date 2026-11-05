using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TempTeaching
{
    public int SEmp { get; set; }

    public int Id { get; set; }

    public int? NYear { get; set; }

    public string? NTerm { get; set; }

    public int? CourseTypeId { get; set; }

    public int? SubjectId { get; set; }

    public string? SClassId { get; set; }

    public string? SRoomId { get; set; }

    public int? HoursPerWeek { get; set; }

    public int? DirectTeaching { get; set; }

    public int? CompetentTeaching { get; set; }

    public int? WantTrain { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public int SchoolId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool CDel { get; set; }
}
