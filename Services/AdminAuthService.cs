using Microsoft.EntityFrameworkCore;
using SSRd.Data;
using SSRd.DTOs;

namespace SSRd.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly ILoginAuditService _audit;
    private readonly IPasswordCipher _cipher;
    private readonly IConfiguration _config;

    public AdminAuthService(
        AppDbContext db,
        IJwtService jwt,
        ILoginAuditService audit,
        IPasswordCipher cipher,
        IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _audit = audit;
        _cipher = cipher;
        _config = config;
    }

    public async Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request, string? ipAddress)
    {
        var user = await _db.TbUserPasswords
            .FirstOrDefaultAsync(u => u.UserName == request.UserName)
            ?? throw new InvalidOperationException("Invalid username or password.");

        if (user.AuthStatus != "A")
            throw new InvalidOperationException("User is not authorized.");
        if (user.RecordStatus == "C")
            throw new InvalidOperationException("User account is closed.");

        if (!VerifyPassword(request.Password, user.Password))
            throw new InvalidOperationException("Invalid username or password.");

        var (token, expiresAt) = _jwt.GenerateAdminToken(user);

        var auditId = await _audit.RecordLoginAsync(
            user.UserId, user.UserName, ipAddress, user.UserBranch);

        user.LastLogin = DateOnly.FromDateTime(DateTime.Now);
        await _db.SaveChangesAsync();

        return new AdminLoginResponse
        {
            Token = token,
            UserId = user.UserId,
            UserName = user.UserName ?? string.Empty,
            Branch = user.UserBranch,
            ExpiresAt = expiresAt,
            LoginAuditId = auditId
        };
    }

    private bool VerifyPassword(string supplied, string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;
        var key = _config["Security:LegacyPasswordKey"]
            ?? throw new InvalidOperationException("Security:LegacyPasswordKey is not configured.");
        try
        {
            var decrypted = _cipher.Decrypt(stored, key);
            return CryptographicEquals(decrypted, supplied);
        }
        catch
        {
            return false;
        }
    }

    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}
