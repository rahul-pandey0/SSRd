using Microsoft.EntityFrameworkCore;
using SSRd.Data;
using SSRd.Models;

namespace SSRd.Services;

public class OtpService : IOtpService
{
    private readonly AppDbContext _db;
    private const int ExpiryMinutes = 5;

    public OtpService(AppDbContext db) => _db = db;

    public async Task<string> GenerateAndStoreAsync(string phoneNo)
    {
        // Invalidate any existing unused OTPs for this number
        var existing = await _db.TmOtps
            .Where(o => o.PhoneNo == phoneNo && !o.IsUsed)
            .ToListAsync();
        _db.TmOtps.RemoveRange(existing);

        var code = new Random().Next(100000, 999999).ToString();
        _db.TmOtps.Add(new TmOtp
        {
            PhoneNo = phoneNo,
            OtpCode = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
            IsUsed = false
        });

        await _db.SaveChangesAsync();
        return code;
    }

    public async Task<bool> VerifyAsync(string phoneNo, string otp)
    {
        var entry = await _db.TmOtps
            .Where(o => o.PhoneNo == phoneNo && o.OtpCode == otp && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (entry is null) return false;
        if (DateTime.UtcNow > entry.ExpiresAt) return false;

        return true;
    }

    public async Task InvalidateAsync(string phoneNo, string otp)
    {
        var entry = await _db.TmOtps
            .Where(o => o.PhoneNo == phoneNo && o.OtpCode == otp && !o.IsUsed)
            .FirstOrDefaultAsync();

        if (entry is not null)
        {
            entry.IsUsed = true;
            await _db.SaveChangesAsync();
        }
    }
}
