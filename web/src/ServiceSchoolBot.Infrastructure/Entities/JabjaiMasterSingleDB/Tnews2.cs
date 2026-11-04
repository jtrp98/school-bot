using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tnews2
{
    public int NewsId { get; set; }

    public short? SystemType { get; set; }

    public string? SendType { get; set; }

    public string? Title { get; set; }

    public int? SchoolId { get; set; }

    public short? ToType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsSystemLogin { get; set; }

    public string? SystemLogin { get; set; }

    public bool? IsStudentLogin { get; set; }

    public string? StudentLogin { get; set; }

    public bool? IsNoteHead { get; set; }

    public string? NoteHead { get; set; }

    public bool? IsNoteAppWeb { get; set; }

    public string? NoteAppWeb { get; set; }

    public bool? IsPopup { get; set; }

    public string? Popup { get; set; }

    public bool? IsBanner { get; set; }

    public string? Banner { get; set; }

    public bool? IsEmail { get; set; }

    public string? Email { get; set; }

    public string? ToSid { get; set; }

    public bool? IsSms { get; set; }

    public string? Sms { get; set; }

    public bool? IsLine { get; set; }

    public string? Line { get; set; }

    public bool? IsLineGroup { get; set; }

    public string? LineGroup { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsDone { get; set; }

    public string? LineGroupAttach { get; set; }

    public string? LineGroupLink { get; set; }
}
