using SSRd.DTOs;

namespace SSRd.Services;

public interface IAuthService
{
    Task<InitiateResponse> InitiateAsync(string phoneNo);
    Task<AuthResponse> VerifyAndLoginAsync(VerifyOtpRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}
