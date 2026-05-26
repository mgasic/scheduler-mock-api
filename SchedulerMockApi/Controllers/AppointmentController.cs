using Microsoft.AspNetCore.Mvc;
using SchedulerMockApi.Models;
using SchedulerMockApi.Services;

namespace SchedulerMockApi.Controllers;

[ApiController]
[Route("appointments")]
public class AppointmentController : ControllerBase
{
    private readonly MockDataStore _store;
    public AppointmentController(MockDataStore store) => _store = store;

    [HttpPost]
    public IActionResult Book([FromBody] Appointment req)
    {
        if (string.IsNullOrEmpty(req.slot_date) || string.IsNullOrEmpty(req.appt_type_id))
            return BadRequest(new { message = "Mandatory parameter missing: slot_date, appt_type_id" });

        req.appointment_id = "APT" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        req.status = "scheduled";

        _store.Appointments.Add(req);
        _store.Save("appointments.json", _store.Appointments);

        return Ok(new
        {
            message = "Appointment booked successfully",
            appointment_id = req.appointment_id,
            patient_id = req.patient_id,
            status = "true"
        });
    }

    [HttpGet("{appointment_id}")]
    public IActionResult GetById(string appointment_id)
    {
        var appt = _store.Appointments.FirstOrDefault(a => a.appointment_id == appointment_id);
        if (appt == null) return NotFound(new { message = "Appointment not found" });
        return Ok(appt);
    }

    [HttpGet("patient/{patient_id}")]
    public IActionResult GetByPatient(string patient_id)
    {
        var appts = _store.Appointments.Where(a => a.patient_id == patient_id).ToList();
        return Ok(new { waitlisted = "NO", onRecall = "NO", appointments = appts });
    }

    [HttpGet("cancellation-reasons")]
    public IActionResult GetCancellationReasons()
    {
        return Ok(_store.CancellationReasons);
    }

    [HttpPut("{appointment_id}/cancel")]
    public IActionResult Cancel(string appointment_id, [FromBody] CancelRequest req)
    {
        var appt = _store.Appointments.FirstOrDefault(a => a.appointment_id == appointment_id);
        if (appt == null) return NotFound(new { message = "Appointment not found" });
        if (string.IsNullOrEmpty(req.appointment_comment))
            return BadRequest(new { message = "Mandatory parameter missing: appointment_comment" });

        appt.status = "cancelled";
        appt.cancellation_reason_id = req.cancellation_reason_id;
        appt.appointment_comment = req.appointment_comment;

        _store.Save("appointments.json", _store.Appointments);

        return Ok(new
        {
            message = "Appointment cancelled successfully",
            appointment_id,
            status = "cancelled"
        });
    }

    [HttpPost("{appointment_id}/reschedule")]
    public IActionResult Reschedule(string appointment_id, [FromBody] RescheduleRequest req)
    {
        var old = _store.Appointments.FirstOrDefault(a => a.appointment_id == appointment_id);
        if (old == null) return NotFound(new { message = "Appointment not found" });

        old.status = "rescheduled";
        var newAppt = new Appointment
        {
            appointment_id = "APT" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            patient_id = old.patient_id,
            provider_id = req.provider_id ?? old.provider_id,
            facility_id = req.facility_id ?? old.facility_id,
            appt_type_id = req.appt_type_id ?? old.appt_type_id,
            slot_date = req.slot_date,
            slot_time = req.slot_time,
            duration = req.duration > 0 ? req.duration : old.duration,
            reason_id = req.reason_id ?? old.reason_id,
            reason = req.reason ?? old.reason,
            service_id = req.service_id ?? old.service_id,
            status = "scheduled"
        };

        _store.Appointments.Add(newAppt);
        _store.Save("appointments.json", _store.Appointments);

        return Ok(new
        {
            message = "Appointment rescheduled successfully",
            appointment_id = newAppt.appointment_id,
            patient_id = newAppt.patient_id,
            status = "true"
        });
    }
}

public class CancelRequest
{
    public string? cancellation_reason_id { get; set; }
    public string? appointment_comment { get; set; }
}

public class RescheduleRequest
{
    public string slot_date { get; set; } = "";
    public string slot_time { get; set; } = "";
    public string? appt_type_id { get; set; }
    public int duration { get; set; }
    public string? facility_id { get; set; }
    public string? provider_id { get; set; }
    public string? reason_id { get; set; }
    public string? reason { get; set; }
    public string? service_id { get; set; }
}
