using Microsoft.AspNetCore.Mvc;
using SchedulerMockApi.Services;

namespace SchedulerMockApi.Controllers;

[ApiController]
[Route("providers")]
public class ProviderController : ControllerBase
{
    private readonly MockDataStore _store;
    public ProviderController(MockDataStore store) => _store = store;

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var all = _store.Providers;
        var paged = all.Skip((page - 1) * limit).Take(limit).ToList();
        return Ok(new { limit, current_page = page, total_pages = (int)Math.Ceiling((double)all.Count / limit), data = paged });
    }

    [HttpGet("{provider_id}")]
    public IActionResult GetById(string provider_id)
    {
        var p = _store.Providers.FirstOrDefault(x => x.provider_id == provider_id);
        if (p == null) return NotFound(new { message = "Provider not found" });
        return Ok(p);
    }

    [HttpGet("{provider_id}/visit-reasons")]
    public IActionResult GetVisitReasons(string provider_id, [FromQuery] bool? is_treated_by_provider)
    {
        var reasons = _store.VisitReasons.Where(r =>
            r.provider_ids == null || r.provider_ids.Contains(provider_id)).ToList();
        if (is_treated_by_provider.HasValue)
            reasons = reasons.Where(r => r.is_treated_by_provider == is_treated_by_provider.Value).ToList();
        return Ok(new { visit_reasons = reasons });
    }

    [HttpGet("{provider_id}/visit-reasons/search")]
    public IActionResult SearchVisitReasons(string provider_id, [FromQuery] string q)
    {
        var results = _store.VisitReasons
            .Where(r => r.name.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                (r.provider_ids == null || r.provider_ids.Contains(provider_id)))
            .ToList();
        return Ok(new { visit_reasons = results });
    }

    [HttpPost("manual-publish")]
    public IActionResult ManualPublish([FromBody] object req)
    {
        var requestId = Guid.NewGuid().ToString("N");
        _store.PublishRequests.Add(new Models.PublishRequest { request_id = requestId, type = "provider", status = "Success", message = "Providers published", processed_ts = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") });
        return Ok(new { request_id = requestId, message = "Request triggered, providers will be sent asynchronously to Pub Sub." });
    }

    [HttpGet("manual-publish/status")]
    public IActionResult ManualPublishStatus([FromQuery] string request_id)
    {
        var req = _store.PublishRequests.FirstOrDefault(r => r.request_id == request_id);
        if (req == null) return BadRequest(new { request_id, message = "Request not found" });
        return Ok(new { request_id = req.request_id, status = req.status, message = req.message, processed_ts = req.processed_ts });
    }
}
