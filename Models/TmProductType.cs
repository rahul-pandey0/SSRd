using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models;

    [Table("tm_product_type")]
    public class TmProductType
    {
    [Key]
        public int Product_Type_ID { get; set; }

        public string? Product_Code { get; set; }

        public string? DESC { get; set; }

        public string? TYPE { get; set; }

        public string? InterestRateCode { get; set; }

        public double? InterestRate { get; set; }

        public double? Charges { get; set; }

        public string? PaymentBy { get; set; }

        public string? CashGl { get; set; }

        public string? CustAcc { get; set; }

        public string? LiabilityGl { get; set; }

        public string? InterestExpenseAccount { get; set; }

        public string? ChargeIncomeAcc { get; set; }

        public int? CreatedBy { get; set; }

        public DateOnly? CreatedDate { get; set; }

        public DateOnly? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public int? AuthorisedBy { get; set; }

        public string? AuthStatus { get; set; }

        public DateOnly? AuthorisedDate { get; set; }

        public string? RecordStatus { get; set; }

        public string? ScheduleReqd { get; set; }

        public double? Interest { get; set; }

        public string? Liquidation_Reqd { get; set; }

        public string? PeriodType { get; set; }

        public string? ProvisionAcc { get; set; }

        public double? Frequency { get; set; }

        public double? AgreedAmount { get; set; }

        public double? PrematureCharge { get; set; }

        public string? CarrerSystem { get; set; }

        public string? Calculation_Method { get; set; }

        public double? ProductBasedAccountNo { get; set; }

        public string? ProductBasedCode { get; set; }
    }

