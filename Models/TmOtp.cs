using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models;

[Table("tm_otp")]
public class TmOtp
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("PhoneNo")]
    [MaxLength(20)]
    public string PhoneNo { get; set; } = string.Empty;

    [Column("OtpCode")]
    [MaxLength(6)]
    public string OtpCode { get; set; } = string.Empty;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    [Column("IsUsed")]
    public bool IsUsed { get; set; } = false;
}
