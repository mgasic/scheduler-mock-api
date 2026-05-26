using Microsoft.AspNetCore.Mvc;

namespace SchedulerMockApi.Controllers;

[ApiController]
[Route("oauth2")]
public class AuthController : ControllerBase
{
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public IActionResult GenerateToken([FromForm] string grant_type)
    {
        if (grant_type != "client_credentials")
            return BadRequest(new { error = "unsupported_grant_type" });

        return Ok(new
        {
            access_token = "mock-access-token-" + Guid.NewGuid().ToString("N"),
            token_type = "Bearer",
            expires_in = "3600"
        });
    }
}
