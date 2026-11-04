using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TexpenseVehicle
{
    public int ExpenseVehicleId { get; set; }

    public int? StaffId { get; set; }

    public string? VehicleCode { get; set; }
}
