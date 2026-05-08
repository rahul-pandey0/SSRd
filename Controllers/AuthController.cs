using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSRd.DTOs;
using SSRd.Services;

namespace SSRd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    /// <summary>
    /// Step 1: Enter mobile number. Sends OTP and tells client if member exists or needs registration.
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateRequest request)
    {
        try
        {
            var result = await _auth.InitiateAsync(request.PhoneNo);
            return Ok(ApiResponse<InitiateResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initiate failed for {PhoneNo}", request.PhoneNo);
            return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
        }
    }

    /// <summary>
    /// Step 2a (Existing member): Verify OTP and receive JWT session token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var result = await _auth.VerifyAndLoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {PhoneNo}", request.PhoneNo);
            return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
        }
    }

    /// <summary>
    /// Step 2b (New member): Submit registration details + OTP to create account.
    /// Account is created but no token is issued — proceed to /initiate then /login to authenticate.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _auth.RegisterAsync(request);
            return Ok(ApiResponse<RegisterResponse>.Ok(result, result.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register failed for {PhoneNo}", request.PhoneNo);
            return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
        }
    }

    /// <summary>
    /// Verify JWT token is valid (protected endpoint).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var memberId = User.FindFirst("memberId")?.Value;
        var name = User.FindFirst("name")?.Value;
        var membershipNo = User.FindFirst("membershipNo")?.Value;
        var phoneNo = User.FindFirst("phoneNo")?.Value;

        return Ok(ApiResponse<object>.Ok(new { memberId, name, membershipNo, phoneNo }));
    }
}
