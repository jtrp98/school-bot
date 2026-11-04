using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbPaymentGateway
{
    public int FdPaymentGatewayId { get; set; }

    public int? FdSchoolId { get; set; }

    public string? FdPublicKey { get; set; }

    public string? FdSecretKey { get; set; }

    public bool? FdActive { get; set; }

    public bool? FdActiveInvoice { get; set; }

    public string? FdPartnerId { get; set; }

    public string? FdPartnerSecret { get; set; }

    public string? FdMerchantMid { get; set; }

    public int? FdPromptPayActive { get; set; }

    public string? FdConsumerId { get; set; }

    public string? FdConsumerSecret { get; set; }

    public string? FdCampaignId { get; set; }

    public string? FdCgaMerchantCreditCardFullId { get; set; }

    public string? FdCgaTerminalCreditCardFullId { get; set; }

    public string? FdCgaMerchantCreditCardInstallmentId { get; set; }

    public string? FdCgaTerminalCreditCardInstallmentId { get; set; }

    public string? FdCgaSmartPayCreditCardInstallmentId { get; set; }

    public bool? DisableNormalCard { get; set; }

    public string? FdPublicKeyInvoice { get; set; }

    public string? FdSecretKeyInvoice { get; set; }

    public decimal? FdFeePayment { get; set; }

    public decimal? FdFeeInvoice { get; set; }

    public bool? FdBblpayment { get; set; }

    public string? FdSuffix { get; set; }

    public bool? FdKtbpayment { get; set; }

    public short? FdPaymentType { get; set; }

    public string? FdBeamMerchant { get; set; }

    public string? FdBeamSecretKey { get; set; }
}
