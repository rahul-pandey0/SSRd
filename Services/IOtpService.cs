namespace SSRd.Services;

public interface IOtpService
{
    Task<string> GenerateAndStoreAsync(string phoneNo);
    Task<bool> VerifyAsync(string phoneNo, string otp);
    Task InvalidateAsync(string phoneNo, string otp);
}
