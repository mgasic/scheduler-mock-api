using Microsoft.AspNetCore.Mvc;
using SchedulerMockApi.Services;

namespace SchedulerMockApi.Controllers;

[ApiController]
public class MiscController : ControllerBase
{
    private readonly MockDataStore _store;
    public MiscController(MockDataStore store) => _store = store;

    [HttpGet("services")]
    public IActionResult GetServices() => Ok(new { services = _store.Services });

    [HttpGet("appointment-types")]
    public IActionResult GetAppointmentTypes() => Ok(new { appointment_types = _store.AppointmentTypes });

    [HttpGet("specialties")]
    public IActionResult GetSpecialties() => Ok(new { specialties = _store.Specialties });

    [HttpGet("visit-reasons")]
    public IActionResult GetVisitReasons() => Ok(new { visit_reasons = _store.VisitReasons });

    [HttpGet("visit-reasons/search")]
    public IActionResult SearchVisitReasons([FromQuery] string q)
    {
        var results = _store.VisitReasons
            .Where(r => r.name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Ok(new { visit_reasons = results });
    }

    [HttpGet("triage")]
    public IActionResult GetTriage([FromQuery] string reason_id)
    {
        var reason = _store.VisitReasons.FirstOrDefault(r => r.reason_id == reason_id);
        if (reason?.triage == null) return Ok(new { message = "No triage configured" });
        return Ok(new { triage = reason.triage });
    }

    [HttpGet("triage/fhir")]
    public IActionResult GetTriageFhir([FromQuery] string reason_id)
    {
        var reason = _store.VisitReasons.FirstOrDefault(r => r.reason_id == reason_id);
        if (reason?.triage == null) return Ok(new { message = "No triage configured" });
        return Ok(new { format = "fhir", triage = reason.triage });
    }

    [HttpPost("admin/reset")]
    public IActionResult Reset()
    {
        _store.Reset();
        return Ok(new { message = "Mock data reset to seed state successfully" });
    }
}
