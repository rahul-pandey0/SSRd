namespace SSRd.Services;

// Replace this with your actual SMS provider (Twilio, MSG91, etc.)
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger) => _logger = logger;

    public Task SendOtpAsync(string phoneNo, string otp)
    {
        // TODO: Integrate real SMS gateway here
        // Example for MSG91: POST https://api.msg91.com/api/v5/otp
        _logger.LogInformation("[SMS] OTP {Otp} sent to {PhoneNo}", otp, phoneNo);
        return Task.CompletedTask;
    }
}
