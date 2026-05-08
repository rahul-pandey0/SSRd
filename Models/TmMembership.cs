using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models;

[Table("tm_membership")]
public class TmMembership
{
    [Key]
    [Column("MEMBER_ID")]
    public int MemberId { get; set; }

    [Column("MEMBERSHIP_NO")]
    [MaxLength(30)]
    public string? MembershipNo { get; set; }

    [Column("FORM_NUMBER")]
    public int? FormNumber { get; set; }

    [Column("FORM_DATE")]
    public DateOnly? FormDate { get; set; }

    [Column("NAME")]
    [MaxLength(150)]
    public string? Name { get; set; }

    [Column("FATHER_NAME")]
    [MaxLength(150)]
    public string? FatherName { get; set; }

    [Column("ADDRESS")]
    [MaxLength(150)]
    public string? Address { get; set; }

    [Column("PhoneNo")]
    [MaxLength(20)]
    public string? PhoneNo { get; set; }

    [Column("CreatedDate")]
    public DateOnly? CreatedDate { get; set; }

    [Column("Age")]
    public double Age { get; set; } = 0;

    [Column("Birthdate")]
    public DateOnly? Birthdate { get; set; }

    [Column("Pancard")]
    [MaxLength(45)]
    public string? Pancard { get; set; }

    [Column("AdharCard")]
    [MaxLength(45)]
    public string? AdharCard { get; set; }

    [Column("AuthStatus")]
    [MaxLength(1)]
    public string AuthStatus { get; set; } = "U";
}
