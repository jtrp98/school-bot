using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbackupCardHistory
{
    public Guid CardHistoryId { get; set; }

    public int SchoolId { get; set; }

    public Guid? CardId { get; set; }

    public int? UserType { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public DateTime? BorrowingDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? CDel { get; set; }

    public decimal? Insurance { get; set; }
}
