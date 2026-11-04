using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmAssignContact
{
    public int CrmAssignContactId { get; set; }

    public int? CrmContactId { get; set; }

    public int? SaleId { get; set; }

    public DateTime? CreateDate { get; set; }
}
