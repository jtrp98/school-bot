using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiSchoolLogs;

public partial class ModifiedUser
{
    public int Id { get; set; }

    public DateTime LastModified { get; set; }

    public bool UserModified { get; set; }

    public int UserId { get; set; }

    public int SchoolId { get; set; }
}
