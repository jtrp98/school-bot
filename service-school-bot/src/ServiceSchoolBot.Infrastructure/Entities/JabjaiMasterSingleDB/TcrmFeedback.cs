using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmFeedback
{
    public int CrmFeedbackId { get; set; }

    public int? CrmSupportId { get; set; }

    public int? Score { get; set; }

    public string? Reasons { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreateDate { get; set; }
}
