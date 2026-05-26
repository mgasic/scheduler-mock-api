using Microsoft.AspNetCore.Mvc;

namespace SchedulerMockApi.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet("check")]
    public IActionResult Check([FromQuery] bool all_services = false)
    {
        var response = new Dictionary<string, object>
        {
            ["status"] = "healthy",
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        if (all_services)
            response["details"] = new { dash = "healthy", ehr = "healthy", nextgate = "healthy" };

        return Ok(response);
    }
}
