using Microsoft.EntityFrameworkCore;
using SSRd.Data;
using SSRd.DTOs;
using SSRd.Models;

namespace SSRd.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IOtpService _otp;
    private readonly ISmsService _sms;
    private readonly IJwtService _jwt;
    private readonly ILoginAuditService _audit;
    private readonly IHttpContextAccessor _http;

    public AuthService(
        AppDbContext db,
        IOtpService otp,
        ISmsService sms,
        IJwtService jwt,
        ILoginAuditService audit,
        IHttpContextAccessor http)
    {
        _db = db;
        _otp = otp;
        _sms = sms;
        _jwt = jwt;
        _audit = audit;
        _http = http;
    }

    public async Task<InitiateResponse> InitiateAsync(string phoneNo)
    {
        var member = await _db.TmMemberships
            .FirstOrDefaultAsync(m => m.PhoneNo == phoneNo);

        var code = await _otp.GenerateAndStoreAsync(phoneNo);
        await _sms.SendOtpAsync(phoneNo, code);

        return member is not null
            ? new InitiateResponse { IsExistingMember = true, Message = "OTP sent. Please verify to login." }
            : new InitiateResponse { IsExistingMember = false, Message = "OTP sent. Please complete registration." };
    }

    public async Task<AuthResponse> VerifyAndLoginAsync(VerifyOtpRequest request)
    {
        if (!await _otp.VerifyAsync(request.PhoneNo, request.Otp))
            throw new InvalidOperationException("Invalid or expired OTP.");

        var member = await _db.TmMemberships
            .FirstOrDefaultAsync(m => m.PhoneNo == request.PhoneNo)
            ?? throw new InvalidOperationException("Member not found. Please register first.");

        await _otp.InvalidateAsync(request.PhoneNo, request.Otp);

        var (token, expiresAt) = _jwt.GenerateToken(member);

        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        await _audit.RecordLoginAsync(member.MemberId, member.Name, ip, branchCode: null);

        return new AuthResponse
        {
            Token = token,
            MemberId = member.MemberId,
            Name = member.Name ?? string.Empty,
            MembershipNo = member.MembershipNo ?? string.Empty,
            ExpiresAt = expiresAt
        };
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (!await _otp.VerifyAsync(request.PhoneNo, request.Otp))
            throw new InvalidOperationException("Invalid or expired OTP.");

        var existing = await _db.Tmmebershipregistrations
            .AnyAsync(m => m.PhoneNo == request.PhoneNo);
        if (existing)
            throw new InvalidOperationException("Mobile number already registered. Admin has to approve your request for membership .");

        var member = new Tmmebershipregistration
        {
            PhoneNo = request.PhoneNo,
            Branchcode = "BR001",
            Name = request.Name,
            FatherName = request.FatherName,
            Address = request.Address,
            Birthdate = request.Birthdate,
            Age = CalculateAge(request.Birthdate),
            Pancard = request.Pancard,
            AdharCard = request.AdharCard,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            FormDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AuthStatus = "U"
        };


        _db.Tmmebershipregistrations.Add(member);
        await _db.SaveChangesAsync();

        // Assign membership number now that auto-increment ID is available
        member.MembershipNo = $"SSR{member.MemberId:D6}";
        await _db.SaveChangesAsync();

        await _otp.InvalidateAsync(request.PhoneNo, request.Otp);

        return new RegisterResponse
        {
            MemberId = member.MemberId,
            MembershipNo = member.MembershipNo,
            Message = "Registration successful. Please login with your mobile number."
        };


    }
    private double CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }
}
