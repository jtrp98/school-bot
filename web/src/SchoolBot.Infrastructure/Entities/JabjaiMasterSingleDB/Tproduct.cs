using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Tproduct
{
    public int ProductId { get; set; }

    public string? ProductCode { get; set; }

    public string? Name { get; set; }

    public bool? IsActive { get; set; }
}
