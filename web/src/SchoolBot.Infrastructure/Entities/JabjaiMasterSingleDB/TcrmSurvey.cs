using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmSurvey
{
    public int CrmSurveyId { get; set; }

    public int? SchoolId { get; set; }

    public string? SenderName { get; set; }

    public string? SenderType { get; set; }

    public string? SenderCode { get; set; }

    public string? Tel { get; set; }

    public string? Email { get; set; }

    public string? Note { get; set; }

    public DateTime? TopupDate { get; set; }

    public Guid? Session { get; set; }

    public DateTime? CreateDate { get; set; }
}
