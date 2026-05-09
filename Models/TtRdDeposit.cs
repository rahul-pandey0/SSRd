using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models
{
    [Table("tt_rddeposits")]
    public class TtRdDeposit
    {
        [Key]
        public int RdDepositsId { get; set; }

        [StringLength(16)]
        public string? RdRefNumber { get; set; }

        [StringLength(30)]
        public string? MEMBERSHIP_NO { get; set; }

        [StringLength(15)]
        public string? Deposit_Type { get; set; }

        [StringLength(45)]
        public string? Product_Code { get; set; }

        public double? Deposit_Amount { get; set; }

        public int? Tenor { get; set; }

        public DateTime? value_Date { get; set; }

        public DateTime? Book_Date { get; set; }

        public DateTime? Liquidation_Date { get; set; }

        [StringLength(20)]
        public string? Deposit_Account { get; set; }

        [StringLength(1)]
        public string? AuthStatus { get; set; }
    }
}