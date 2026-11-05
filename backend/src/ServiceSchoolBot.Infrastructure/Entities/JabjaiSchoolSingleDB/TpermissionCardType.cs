using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpermissionCardType
{
    public int Id { get; set; }

    public int? SchoolId { get; set; }

    public string? Permission { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public int? Creator { get; set; }

    public int? Modifier { get; set; }

    public bool? IsDel { get; set; }
}
