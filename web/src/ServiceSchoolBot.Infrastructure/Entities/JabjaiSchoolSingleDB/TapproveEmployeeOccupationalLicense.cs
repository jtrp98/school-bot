using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TapproveEmployeeOccupationalLicense
{
    public int Id { get; set; }

    public int? Version { get; set; }

    public int EmployeeId { get; set; }

    public int SchoolId { get; set; }

    public int? No { get; set; }

    public int? ProfessionalLicenseType { get; set; }

    public string? ProfessionalLicenseNumber { get; set; }

    public string? ProfessionalLicenseName { get; set; }

    public DateTime? CertificateIssueDate { get; set; }

    public DateTime? CertificateExpireDate { get; set; }

    public string? OrganizationIssuingCertificate { get; set; }

    public DateTime? RequestApproveDate { get; set; }

    public DateTime? ApproveDate { get; set; }

    public int? ApproveBy { get; set; }

    public string? ApproveStatus { get; set; }

    public string? ApproveComment { get; set; }

    public bool? UserAcknowledge { get; set; }

    public DateTime? EndDateShowResult { get; set; }
}
