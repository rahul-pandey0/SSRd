namespace SSRd.Services;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNo, string otp);
}
