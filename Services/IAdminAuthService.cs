using SSRd.DTOs;

namespace SSRd.Services;

public interface IAdminAuthService
{
    Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request, string? ipAddress);
}
