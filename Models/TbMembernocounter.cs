using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSRd.Models;
[Table("tb_membernocounter")]
public class TbMembernocounter
    {
    public int Id { get; set; }

    public string? BranchCode { get; set; }

    public string? BankRef_ID { get; set; }

    public int? MemberCtr { get; set; }

    public int? NonMemberCtr { get; set; }


    public double? Mem_Reg_Ctr { get; set; }
    
    public int? RD_Ctr { get; set; }

   
}

