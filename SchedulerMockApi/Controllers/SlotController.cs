using Microsoft.AspNetCore.Mvc;
using SchedulerMockApi.Models;
using SchedulerMockApi.Services;

namespace SchedulerMockApi.Controllers;

[ApiController]
[Route("slots")]
public class SlotController : ControllerBase
{
    private readonly MockDataStore _store;
    public SlotController(MockDataStore store) => _store = store;

    [HttpPost]
    public IActionResult Search([FromBody] SlotSearchRequest req)
    {
        var results = _store.Slots.Where(s =>
            !s.is_locked &&
            (string.IsNullOrEmpty(req.provider_id) || s.provider_id == req.provider_id) &&
            (string.IsNullOrEmpty(req.facility_id) || s.facility_id == req.facility_id) &&
            (string.IsNullOrEmpty(req.appt_type_id) || s.appt_type_id == req.appt_type_id)
        ).ToList();

        if (!string.IsNullOrEmpty(req.start_date) && DateTime.TryParse(req.start_date, out var startDt))
            results = results.Where(s => DateTime.TryParse(s.slot_date, out var d) && d >= startDt).ToList();
        if (!string.IsNullOrEmpty(req.end_date) && DateTime.TryParse(req.end_date, out var endDt))
            results = results.Where(s => DateTime.TryParse(s.slot_date, out var d) && d <= endDt).ToList();

        return Ok(new { slots = results });
    }

    [HttpPost("earliest-availability")]
    public IActionResult EarliestAvailability([FromBody] SlotSearchRequest req)
    {
        var slot = _store.Slots.Where(s => !s.is_locked &&
            (string.IsNullOrEmpty(req.provider_id) || s.provider_id == req.provider_id) &&
            (string.IsNullOrEmpty(req.facility_id) || s.facility_id == req.facility_id))
            .OrderBy(s => s.slot_date).ThenBy(s => s.slot_time)
            .FirstOrDefault();

        return Ok(new { slot });
    }

    [HttpPost("lock")]
    public IActionResult Lock([FromBody] SlotLockReq req)
    {
        var slot = _store.Slots.FirstOrDefault(s => s.slot_id == req.slot_id);
        if (slot == null) return BadRequest(new { message = "Slot not found" });
        if (slot.is_locked) return BadRequest(new { message = "Slot is already locked" });

        slot.is_locked = true;
        var lockId = "LOCK" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        _store.SlotLocks.Add(new SlotLock
        {
            lock_id = lockId,
            slot_id = req.slot_id,
            patient_id = req.patient_id,
            locked_at = DateTime.UtcNow,
            status = "locked"
        });

        return Ok(new { lock_id = lockId, status = "locked" });
    }

    [HttpPost("release")]
    public IActionResult Release([FromBody] SlotReleaseReq req)
    {
        if (string.IsNullOrEmpty(req.lock_id))
            return BadRequest(new { message = "Mandatory parameter missing: lock_id" });

        var lockEntry = _store.SlotLocks.FirstOrDefault(l => l.lock_id == req.lock_id);
        if (lockEntry == null) return NotFound(new { message = "Lock not found" });

        var slot = _store.Slots.FirstOrDefault(s => s.slot_id == lockEntry.slot_id);
        if (slot != null) slot.is_locked = false;

        _store.SlotLocks.Remove(lockEntry);

        return Ok(new { message = "Slot released successfully", lock_id = req.lock_id, status = "released" });
    }

    [HttpPost("manual-publish")]
    public IActionResult ManualPublish([FromBody] ManualPublishReq req)
    {
        var requestId = Guid.NewGuid().ToString("N");
        _store.PublishRequests.Add(new PublishRequest { request_id = requestId, status = "Success", message = "Slots published", processed_ts = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"), type = "slot" });
        return Ok(new { request_id = requestId, message = "Request triggered, slots will be sent asynchronously to Pub Sub." });
    }

    [HttpGet("manual-publish/status")]
    public IActionResult ManualPublishStatus([FromQuery] string request_id)
    {
        var req = _store.PublishRequests.FirstOrDefault(r => r.request_id == request_id);
        if (req == null) return BadRequest(new { request_id, message = "Request not found" });
        return Ok(new { request_id = req.request_id, status = req.status, message = req.message, processed_ts = req.processed_ts });
    }
}

public class SlotSearchRequest
{
    public string? provider_id { get; set; }
    public string? facility_id { get; set; }
    public string? appt_type_id { get; set; }
    public string? start_date { get; set; }
    public string? end_date { get; set; }
    public string? patient_id { get; set; }
}

public class SlotLockReq
{
    public string slot_id { get; set; } = "";
    public string? patient_id { get; set; }
}

public class SlotReleaseReq
{
    public string? lock_id { get; set; }
}

public class ManualPublishReq
{
    public List<string>? provider_ids { get; set; }
    public string? start_date { get; set; }
    public string? end_date { get; set; }
}
