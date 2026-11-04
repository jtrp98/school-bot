using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TonboardingWatch
{
    public int Id { get; set; }

    public int SchoolId { get; set; }

    public string GroupName { get; set; } = null!;

    public DateOnly TrainingDate { get; set; }

    public int WatchDays { get; set; }

    public int TargetActiveDays { get; set; }

    public string Status { get; set; } = null!;

    public int? ResultActiveDays { get; set; }

    public string? Note { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
