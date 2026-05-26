using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models
{
    [Table("tt_rddepositonline")]
    public class TtRdDepositOnline
    {
        [Key]
        public int RdDepositsId { get; set; }

        [StringLength(30)]
        public string? MEMBERSHIP_NO { get; set; }

        [StringLength(30)]

        public string? name { get; set; }
        [StringLength(15)]
        public string? Deposit_Type { get; set; }

        public double? Deposit_Amount { get; set; }

        public int? Tenor { get; set; }

        [StringLength(30)]
        public string? Nominee { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Column("CreatedBy")]
        [MaxLength(30)]
        public string? CreatedBy { get; set; }

        //[Column("CreatedTime")]
        //public TimeOnly? CreatedTime { get; set; }

        [Column("AuthStatus")]
        [MaxLength(1)]
        public string AuthStatus { get; set; } = "U";

        [Column("AuthBy")]
        [MaxLength(30)]
        public string? AuthBy { get; set; }

        [Column("AuthTime")]
        public DateTime? AuthTime { get; set; }
    }
}