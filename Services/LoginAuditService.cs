using Microsoft.EntityFrameworkCore;
using SSRd.Data;
using SSRd.Models;

namespace SSRd.Services;

public class LoginAuditService : ILoginAuditService
{
    private readonly AppDbContext _db;

    public LoginAuditService(AppDbContext db) => _db = db;

    public async Task<int> RecordLoginAsync(int? userId, string? userName, string? ipAddress, string? branchCode)
    {
        var now = DateTime.Now;
        var entry = new TbUserLoginDetails
        {
            UserId = userId,
            UserName = userName,
            Date = DateOnly.FromDateTime(now),
            LoginTime = TimeOnly.FromDateTime(now),
            Status = 1,
            IpAddress = ipAddress,
            BranchCode = branchCode
        };
        _db.TbUserLoginDetails.Add(entry);
        await _db.SaveChangesAsync();
        return entry.Id;
    }

    public async Task RecordLogoutAsync(int loginId)
    {
        var entry = await _db.TbUserLoginDetails.FirstOrDefaultAsync(x => x.Id == loginId);
        if (entry is null) return;
        entry.LogoutTime = TimeOnly.FromDateTime(DateTime.Now);
        entry.Status = 0;
        await _db.SaveChangesAsync();
    }
}
