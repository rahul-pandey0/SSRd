using System.Net;
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

          await SendOtpSms(phoneNo, code);

        return code;
    }

    private async Task SendOtpSms(string phoneNo, string otp)
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string authKey = "3605erh7Z0jtiCI4oEW9TT";
            string senderId = "SBSCCL";

            string message = $"Your OTP is {otp}. Do not share it with anyone.SOUTH BANGALORE SOUHARDA CREDIT CO-OPERATIVE LIMITED";

            string url =
                $"https://sms.shreetripada.com/api/sendapi.php" +
                $"?auth_key={authKey}" +
                $"&mobiles={phoneNo}" +
                $"&message={Uri.EscapeDataString(message)}" +
                $"&sender={senderId}" +
                $"&route=4";

            using HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);

            // Save SMS Log
            //await SaveSmsLog(message, response);
        }
        catch (Exception ex)
        {
            // Optional logging
            throw new Exception("SMS sending failed", ex);
        }
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
