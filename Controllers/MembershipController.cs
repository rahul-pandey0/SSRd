using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSRd.DTOs;
using SSRd.Services;

namespace SSRd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _auth;
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(IMembershipService auth, ILogger<MembershipController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [HttpGet("{membershipNo}")]
        public async Task<IActionResult> GetByMembershipNo(string membershipNo)
        {
            try
            {
                var result = await _auth.Getmemberdetails(membershipNo);

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

                var result = await _auth.Getrdaccdetails(membershipNo, rdaccno);

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
    }

}