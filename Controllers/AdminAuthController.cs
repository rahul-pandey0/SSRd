using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSRd.DTOs;
using SSRd.Services;

namespace SSRd.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _admin;
    private readonly ILoginAuditService _audit;
    private readonly ILogger<AdminAuthController> _logger;

    public AdminAuthController(
        IAdminAuthService admin,
        ILoginAuditService audit,
        ILogger<AdminAuthController> logger)
    {
        _admin = admin;
        _audit = audit;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _admin.LoginAsync(request, ip);
            return Ok(ApiResponse<AdminLoginResponse>.Ok(result, "Admin login successful."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin login failed for {UserName}", request.UserName);
            return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("logout/{auditId:int}")]
    public async Task<IActionResult> Logout(int auditId)
    {
        await _audit.RecordLogoutAsync(auditId);
        return Ok(ApiResponse<object>.Ok(new { auditId }, "Logged out."));
    }
}
