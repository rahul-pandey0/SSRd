using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models;

[Table("tb_user_password")]
public class TbUserPassword
{
    [Key]
    [Column("i_UserId")]
    public int UserId { get; set; }

    [Column("s_UserName")]
    [MaxLength(150)]
    public string? UserName { get; set; }

    [Column("s_Password")]
    [MaxLength(100)]
    public string? Password { get; set; }

    [Column("s_UserType")]
    [MaxLength(30)]
    public string? UserType { get; set; }

    [Column("s_Status")]
    public int? Status { get; set; }

    [Column("s_UserBranch")]
    [MaxLength(30)]
    public string? UserBranch { get; set; }

    [Column("AuthStatus")]
    [MaxLength(1)]
    public string? AuthStatus { get; set; }

    [Column("RecordStatus")]
    [MaxLength(1)]
    public string? RecordStatus { get; set; }

    [Column("U_Name")]
    [MaxLength(45)]
    public string? UName { get; set; }

    [Column("U_PhoneNo")]
    [MaxLength(20)]
    public string? UPhoneNo { get; set; }

    [Column("U_Email")]
    [MaxLength(45)]
    public string? UEmail { get; set; }

    [Column("LastLogin")]
    public DateOnly? LastLogin { get; set; }
}
