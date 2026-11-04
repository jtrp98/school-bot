using System;
using System.Collections.Generic;

namespace SchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class TbPaymentMethod
{
    public int Id { get; set; }

    public string? PublicKey { get; set; }

    /// <summary>
    /// 1 = Kbank
    /// 2 = KTB
    /// 3 = Beam
    /// </summary>
    public short? PaymentMethod { get; set; }

    public string? MerchantId { get; set; }

    public string? SecretKey { get; set; }

    public bool? IsActive { get; set; }

    public bool? ActiveInvoice { get; set; }

    public string? PartnerId { get; set; }

    public string? PartnerSecret { get; set; }

    public int? PromptPayActive { get; set; }

    public string? ConsumerId { get; set; }

    public string? ConsumerSecret { get; set; }

    public string? CampaignId { get; set; }

    public string? CgaMerchantCreditCardFullId { get; set; }

    public string? CgaTerminalCreditCardFullId { get; set; }

    public string? CgaMerchantCreditCardInstallmentId { get; set; }

    public string? CgaTerminalCreditCardInstallmentId { get; set; }

    public string? CgaSmartPayCreditCardInstallmentId { get; set; }

    public bool? DisableNormalCard { get; set; }

    public string? PublicKeyInvoice { get; set; }

    public string? SecretKeyInvoice { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreateAt { get; set; }

    public int? CreateBy { get; set; }

    public DateTime? DeleteAt { get; set; }

    public int? SchoolId { get; set; }

    public decimal? Fee { get; set; }

    public short? TransactionType { get; set; }

    public short? PaymentChannel { get; set; }

    public string? Suffix { get; set; }

    public string? TaxId { get; set; }

    public string? ProfileName { get; set; }
}
