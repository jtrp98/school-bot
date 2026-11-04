using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcontactListAttach
{
    public int AttachId { get; set; }

    public int? ContactListId { get; set; }

    public int? SchoolId { get; set; }

    public string? AttachUrl { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? CreateBy { get; set; }

    public int? ModifyBy { get; set; }
}
