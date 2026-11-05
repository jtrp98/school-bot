using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ModifiedShop
{
    public int Id { get; set; }

    public DateTime LastModified { get; set; }

    public bool ShopModified { get; set; }

    public int ShopId { get; set; }

    public int SchoolId { get; set; }
}
