using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSRd.DTOs;
using SSRd.Services;

namespace SSRd.Controllers
{
    [ApiController]
    [Route("api/rdcreation")]
    public class RDCreationController : ControllerBase
    {
        private readonly IMembershipService _membership;
        private readonly ILogger<RDCreationController> _logger;

        public RDCreationController(IMembershipService membership, ILogger<RDCreationController> logger)
        {
            _membership = membership;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] RDCreateRequest request)
        {
            try
            {
                var tokenMembershipNo = User.FindFirst("membershipNo")?.Value;
                if (!string.IsNullOrEmpty(tokenMembershipNo)
                    && !string.IsNullOrEmpty(request.MEMBERSHIP_NO)
                    && tokenMembershipNo != request.MEMBERSHIP_NO)
                {
                    return Forbid();
                }
                request.MEMBERSHIP_NO ??= tokenMembershipNo;

                var createdBy = tokenMembershipNo ?? request.MEMBERSHIP_NO ?? "unknown";
                var result = await _membership.RegisterAsync(request, createdBy);
                return Ok(ApiResponse<RDResponse>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RD creation failed for {MembershipNo}", request.MEMBERSHIP_NO);
                return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> Pending()
        {
            var list = await _membership.GetPendingAsync();
            return Ok(ApiResponse<object>.Ok(list));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/authorize")]
        public async Task<IActionResult> Authorize(int id)
        {
            try
            {
                var adminId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var adminUserName = User.FindFirst("userName")?.Value ?? "unknown";
                var adminBranch = User.FindFirst("branch")?.Value;
                var result = await _membership.AuthorizeAsync(id, adminId, adminUserName, adminBranch);
                return Ok(ApiResponse<RDResponse>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authorize RD {Id} failed", id);
                return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
            }
        }
    }
}
