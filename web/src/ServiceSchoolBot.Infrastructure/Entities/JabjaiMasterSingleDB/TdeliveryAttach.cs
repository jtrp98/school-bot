using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TdeliveryAttach
{
    public int DeliveryAttachId { get; set; }

    public int? DeliveryId { get; set; }

    public string? AttachUrl { get; set; }
}
