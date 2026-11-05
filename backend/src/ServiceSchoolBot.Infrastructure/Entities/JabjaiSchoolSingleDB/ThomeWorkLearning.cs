using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class ThomeWorkLearning
{
    public int LearnId { get; set; }

    public int? OnlineId { get; set; }

    public int? SPlaneId { get; set; }

    public int? SEmp { get; set; }

    public string? TitleName { get; set; }

    public string? Description { get; set; }

    public string? LinkYt { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? SchoolId { get; set; }

    public string? SelectedRoom { get; set; }

    public string? SelectedStudent { get; set; }

    public short? AssignType { get; set; }

    public bool? CDel { get; set; }

    public DateTime? DisplayDate { get; set; }

    public short? DisplayType { get; set; }
}
