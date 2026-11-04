using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TcrmContact
{
    public int CrmContactId { get; set; }

    public string? Type { get; set; }

    public string? SchoolName { get; set; }

    public string? Address { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? SubDistrict { get; set; }

    public int? StudentQty { get; set; }

    public string? SchoolGroup { get; set; }

    public string? SchoolEducation { get; set; }

    public string? KnowFrom { get; set; }

    public int? Score { get; set; }

    public string? EventName { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DeleteBy { get; set; }

    public string? TaxNo { get; set; }

    public string? SignName { get; set; }

    public string? SignPosition { get; set; }

    public string? WitnessName { get; set; }

    public string? WitnessPosition { get; set; }

    public string? ContactPhone { get; set; }

    public string? Zipcode { get; set; }

    public int? SaleAreaId { get; set; }

    public string? SchoolCode { get; set; }

    public string? SchoolCriterion { get; set; }

    public string? SchoolClusther { get; set; }

    public string? SchoolNetwork { get; set; }
}
