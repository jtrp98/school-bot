using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Thomework
{
    public int NHomeWork { get; set; }

    public int SchoolId { get; set; }

    public int? OnlineId { get; set; }

    public int? SPlaneId { get; set; }

    public DateTime? DStart { get; set; }

    public DateTime? DEnd { get; set; }

    public DateTime? DNotification { get; set; }

    public DateTime? DOrder { get; set; }

    public int? SEmp { get; set; }

    public string? SHomeworkDetail { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool CDel { get; set; }

    public string? TitleName { get; set; }

    public double? MaxScore { get; set; }

    public short? AssignType { get; set; }

    public string? LinkYt { get; set; }

    public string? SelectedRoom { get; set; }

    public string? SelectedStudent { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public DateTime? DisplayDate { get; set; }

    public short? DisplayType { get; set; }
}
