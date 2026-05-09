using Microsoft.EntityFrameworkCore;
using SSRd.Data;
using SSRd.DTOs;
using SSRd.Models;
namespace SSRd.Services;
public class MembershipService:IMembershipService
{
    private readonly AppDbContext _db;
private readonly IJwtService _jwt;

public MembershipService(AppDbContext db, IJwtService jwt)
{
    _db = db;
   
    _jwt = jwt;
}

public async Task<TmMembership?> Getmemberdetails(string membershipNo)
{
        var member = await _db.TmMemberships
                       .FirstOrDefaultAsync(x => x.MembershipNo == membershipNo);


        return member;
    }

    public async Task<List<TtRdDeposit>> Getrdaccdetails(string? membershipNo, string? rdaccno)
    {
        var result = await _db.TtRdDeposit
            .Where(x =>
                (!string.IsNullOrEmpty(membershipNo) && x.MEMBERSHIP_NO == membershipNo)
                ||
                (!string.IsNullOrEmpty(rdaccno) && x.Deposit_Account == rdaccno)
            )
            .ToListAsync();

        return result;
    }


    public async Task<RDResponse> RegisterAsync(RDCreateRequest request)
    {
        bool memberExists = await _db.TmMemberships
        .AnyAsync(x => x.MembershipNo == request.MEMBERSHIP_NO);

        if (!memberExists)
        {
            return new RDResponse
            {
                MEMBERSHIP_NO = request.MEMBERSHIP_NO,
                Message = "Invalid Membership Number"
            };
        }
        var member = new TtRdDepositOnline
        {
            MEMBERSHIP_NO = request.MEMBERSHIP_NO,
            name = request.name,
            Deposit_Type = "RD",
            Tenor = request.Tenor,
            Deposit_Amount = request.Deposit_Amount,
            CreatedDate = DateTime.UtcNow
        };

        _db.TtRdDepositOnlines.Add(member);

        int result = await _db.SaveChangesAsync();

        if (result > 0)
        {
            return new RDResponse

            {
                MEMBERSHIP_NO = member.MEMBERSHIP_NO,
                Message = "Data inserted successfully"
            };
        }

        return new RDResponse
        {
            MEMBERSHIP_NO = member.MEMBERSHIP_NO,
            Message = "Data insertion failed"
        };
    }

}
