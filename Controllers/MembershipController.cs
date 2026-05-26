using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSRd.DTOs;
using SSRd.Services;

namespace SSRd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membership;
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(IMembershipService membership, ILogger<MembershipController> logger)
        {
            _membership = membership;
            _logger = logger;
        }

        [HttpGet("{membershipNo}")]
        public async Task<IActionResult> GetByMembershipNo(string membershipNo)
        {
            try
            {
                if (!IsOwnerOrAdmin(membershipNo))
                    return Forbid();

                var result = await _membership.Getmemberdetails(membershipNo);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Status = false,
                        Message = "Membership not found"
                    });
                }

                return Ok(new
                {
                    Status = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching member details for {membershipNo}", membershipNo);
                return StatusCode(500, new
                {
                    Status = false,
                    Message = "Something went wrong. Please try again."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByRDNo(string? membershipNo, string? rdaccno)
        {
            try
            {
                if (string.IsNullOrEmpty(membershipNo) && string.IsNullOrEmpty(rdaccno))
                {
                    return BadRequest(new
                    {
                        Status = false,
                        Message = "Please provide Membership No or RD Account No"
                    });
                }

                if (!string.IsNullOrEmpty(membershipNo) && !IsOwnerOrAdmin(membershipNo))
                    return Forbid();

                var result = await _membership.Getrdaccdetails(membershipNo, rdaccno);

                if (!IsAdmin() && !string.IsNullOrEmpty(rdaccno))
                {
                    var tokenMembership = User.FindFirst("membershipNo")?.Value;
                    result = result
                        .Where(r => r.MEMBERSHIP_NO == tokenMembership)
                        .ToList();
                }

                if (result == null || !result.Any())
                {
                    return NotFound(new
                    {
                        Status = false,
                        Message = "RD Account not found"
                    });
                }

                return Ok(new
                {
                    Status = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching RD Account details for {membershipNo}", membershipNo);
                return StatusCode(500, new
                {
                    Status = false,
                    Message = "Something went wrong. Please try again."
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("mempending")]
        public async Task<IActionResult> Pending()
        {
            var list = await _membership.GetMemPendingAsync();
            return Ok(ApiResponse<object>.Ok(list));
        }



        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/memauthorize")]
        public async Task<IActionResult> Authorize(int id)
        {
            try
            {
                var adminId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var adminUserName = User.FindFirst("userName")?.Value ?? "unknown";
                var adminBranch = User.FindFirst("branch")?.Value;
                var result = await _membership.MemAuthorizeAsync(id, adminId, adminUserName, adminBranch);
                return Ok(ApiResponse<MembersResponse>.Ok(result, result.Message));

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authorize Member {Id} failed", id);
                return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
            }
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        private bool IsOwnerOrAdmin(string membershipNo)
        {
            if (IsAdmin()) return true;
            var tokenMembership = User.FindFirst("membershipNo")?.Value;
            return !string.IsNullOrEmpty(tokenMembership)
                && string.Equals(tokenMembership, membershipNo, StringComparison.OrdinalIgnoreCase);
        }
    }
}
