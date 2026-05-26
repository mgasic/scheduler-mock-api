using Microsoft.AspNetCore.Mvc;
using SchedulerMockApi.Services;

namespace SchedulerMockApi.Controllers;

[ApiController]
[Route("facilities")]
public class FacilityController : ControllerBase
{
    private readonly MockDataStore _store;
    public FacilityController(MockDataStore store) => _store = store;

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var all = _store.Facilities;
        var paged = all.Skip((page - 1) * limit).Take(limit).ToList();
        return Ok(new { limit, current_page = page, total_pages = (int)Math.Ceiling((double)all.Count / limit), data = paged });
    }

    [HttpGet("{facility_id}")]
    public IActionResult GetById(string facility_id)
    {
        var f = _store.Facilities.FirstOrDefault(x => x.facility_id == facility_id);
        if (f == null) return NotFound(new { message = "Facility not found" });
        return Ok(f);
    }

    [HttpPost("manual-publish")]
    public IActionResult ManualPublish([FromBody] object req)
    {
        var requestId = Guid.NewGuid().ToString("N");
        _store.PublishRequests.Add(new Models.PublishRequest { request_id = requestId, type = "facility", status = "Success", message = "Facilities published", processed_ts = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") });
        return Ok(new { request_id = requestId, message = "Request triggered, facilities will be sent asynchronously to Pub Sub." });
    }

    [HttpGet("manual-publish/status")]
    public IActionResult ManualPublishStatus([FromQuery] string request_id)
    {
        var req = _store.PublishRequests.FirstOrDefault(r => r.request_id == request_id);
        if (req == null) return BadRequest(new { request_id, message = "Request not found" });
        return Ok(req);
    }
}
