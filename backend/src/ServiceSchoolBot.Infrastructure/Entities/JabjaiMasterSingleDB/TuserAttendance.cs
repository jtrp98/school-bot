using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TuserAttendance
{
    public int AttendanceId { get; set; }

    public int SchoolId { get; set; }

    public int SId { get; set; }

    public int CType { get; set; }

    public string MacAddress { get; set; } = null!;

    public int ShopId { get; set; }

    public string Nfcencrypt { get; set; } = null!;

    public string? SName { get; set; }

    public string? SLastName { get; set; }

    public string? SStudentId { get; set; }

    public string? SStudentPicture { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime Tstamp { get; set; }
}
