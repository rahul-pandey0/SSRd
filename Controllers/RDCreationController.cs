using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSRd.DTOs;
using SSRd.Services;

namespace SSRd.Controllers
{
    public class RDCreationController : Controller
    {
        private readonly IMembershipService _auth;
        private readonly ILogger<RDCreationController> _logger;

        public RDCreationController(IMembershipService auth, ILogger<RDCreationController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [HttpPost("RDcareation")]
        public async Task<IActionResult> Register([FromBody] RDCreateRequest request)
        {
            try
            {
                var result = await _auth.RegisterAsync(request);
                return Ok(ApiResponse<RDResponse>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, ApiResponse<object>.Fail("Something went wrong. Please try again."));
            }
        }

    }
}
