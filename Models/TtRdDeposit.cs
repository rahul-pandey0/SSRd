using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models
{
    [Table("tt_rddeposits")]
    public class TtRdDeposit
    {
        [Key]
        public int RdDepositsId { get; set; }

        [StringLength(45)]
        public string? BranchCode { get; set; }

        [StringLength(16)]
        public string? RdRefNumber { get; set; }

        [StringLength(30)]
        public string? MEMBERSHIP_NO { get; set; }

        [StringLength(15)]
        public string? Deposit_Type { get; set; }

        [StringLength(45)]
        public string? Product_Code { get; set; }

        [StringLength(15)]
        public string? ProductType { get; set; }

        [StringLength(30)]
        public string? InterestRateCode { get; set; }

        public double? InterestRate { get; set; }

        [StringLength(2)]
        public string? PaymentBy { get; set; }

        [StringLength(30)]
        public string? LiabilityGl { get; set; }

        [StringLength(30)]
        public string? InterestExpenseAccount { get; set; }

        [StringLength(30)]
        public string? CustAcc { get; set; }

        [StringLength(30)]
        public string? CashGl { get; set; }

        public double? Deposit_Amount { get; set; }

        public int? Tenor { get; set; }

        [StringLength(30)]
        public string? Nominee { get; set; }

        [StringLength(30)]
        public string? Periodicity { get; set; }

        public DateTime? value_Date { get; set; }
        public DateTime? Book_Date { get; set; }
        public DateTime? Liquidation_Date { get; set; }

        [StringLength(15)]
        public string? Liquidation_Mode { get; set; }

        [StringLength(20)]
        public string? Beneficiary_Account { get; set; }

        [StringLength(20)]
        public string? Deposit_Account { get; set; }

        [StringLength(1)]
        public string? Blocked { get; set; }

        [StringLength(45)]
        public string? BlockedReferenceNo { get; set; }

        public double? BlockedAmount { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? AuthorisedBy { get; set; }

        [StringLength(1)]
        public string? AuthStatus { get; set; }

        public DateTime? AuthorisedDate { get; set; }

        [StringLength(1)]
        public string? RecordStatus { get; set; }

        [StringLength(1)]
        public string? LiquidationStatus { get; set; }

        [StringLength(105)]
        public string? AgentName { get; set; }

        public double? Penalty { get; set; }

        [StringLength(250)]
        public string? Remarks_RD { get; set; }

        public double? ProvisionAmount { get; set; }

        [StringLength(45)]
        public string? EventDraft_RCre { get; set; }

        public double? ExcessAmount { get; set; }

    }
}


