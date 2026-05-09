using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models;

[Table("tb_userlogindetails")]
public class TbUserLoginDetails
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int? UserId { get; set; }

    [Column("UserName")]
    [MaxLength(45)]
    public string? UserName { get; set; }

    [Column("dt_Date")]
    public DateOnly? Date { get; set; }

    [Column("LoginTime")]
    public TimeOnly? LoginTime { get; set; }

    [Column("LogoutTime")]
    public TimeOnly? LogoutTime { get; set; }

    [Column("Status")]
    public int? Status { get; set; }

    [Column("IpAddress")]
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Column("BranchCode")]
    [MaxLength(45)]
    public string? BranchCode { get; set; }
}
