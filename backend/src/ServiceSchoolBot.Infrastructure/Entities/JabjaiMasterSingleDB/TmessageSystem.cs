using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TmessageSystem
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public DateTime? AddDate { get; set; }

    public string? AddBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateBy { get; set; }

    public string? IsDelete { get; set; }
}
