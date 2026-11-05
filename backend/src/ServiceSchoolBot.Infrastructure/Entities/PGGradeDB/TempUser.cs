using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.PGGradeDB;

public partial class TempUser
{
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;
}
