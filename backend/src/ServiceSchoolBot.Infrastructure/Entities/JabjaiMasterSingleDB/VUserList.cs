using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class VUserList
{
    public int? SchoolId { get; set; }

    public int? SId { get; set; }

    public string? SStudentId { get; set; }

    public string? SName { get; set; }

    public string? SLastName { get; set; }

    public string? CDel { get; set; }

    public int? CType { get; set; }

    public decimal? NMoney { get; set; }

    public int? NStudentStatus { get; set; }

    public DateTime? CreatedDate { get; set; }
}
