using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmessageTemplate
{
    public int MessageTemplateId { get; set; }

    public string? Message { get; set; }
}
