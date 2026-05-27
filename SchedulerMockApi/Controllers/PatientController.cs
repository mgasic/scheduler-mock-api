using Microsoft.AspNetCore.Mvc;
using SchedulerMockApi.Models;
using SchedulerMockApi.Services;

namespace SchedulerMockApi.Controllers;

[ApiController]
public class PatientController : ControllerBase
{
    private readonly MockDataStore _store;
    public PatientController(MockDataStore store) => _store = store;

    [HttpGet("patient/search")]
    public IActionResult Search(
        [FromQuery] string? first_name,
        [FromQuery] string? last_name,
        [FromQuery] string? dob,
        [FromQuery] string? zip_code,
        [FromQuery] string? phone,
        [FromQuery] string? email,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var query = _store.Patients.Where(p =>
            (string.IsNullOrEmpty(first_name) || p.first_name.Contains(first_name, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(last_name)  || p.last_name.Contains(last_name,   StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(dob)        || p.dob == dob) &&
            (string.IsNullOrEmpty(zip_code)   || p.zip_code == zip_code) &&
            (string.IsNullOrEmpty(phone)      || p.phone == phone) &&
            (string.IsNullOrEmpty(email)      || p.email.Equals(email, StringComparison.OrdinalIgnoreCase))
        );

        var total   = query.Count();
        var results = query.Skip((page - 1) * limit).Take(limit).ToList();

        return Ok(new
        {
            success     = true,
            total,
            page,
            limit,
            total_pages = (int)Math.Ceiling((double)total / limit),
            patients    = results
        });
    }

    [HttpGet("patient/{patient_id}")]
    public IActionResult GetById(string patient_id)
    {
        var p = _store.Patients.FirstOrDefault(x => x.patient_id == patient_id);
        if (p == null) return NotFound(new { success = false, message = "Patient not found" });
        return Ok(new { success = true, patient = p });
    }

    [HttpGet("nextgate/patient-id")]
    public IActionResult GetPatientId(
        [FromQuery] string? euid,
        [FromQuery] string? first_name,
        [FromQuery] string? last_name,
        [FromQuery] string? dob,
        [FromQuery] string? phone)
    {
        var patient = _store.Patients.FirstOrDefault(p =>
            (!string.IsNullOrEmpty(euid) && p.euid == euid) ||
            (p.first_name == first_name && p.last_name == last_name && p.dob == dob));

        if (patient != null)
            return Ok(new { success = true, patient_id = patient.patient_id, message = "Patient Found" });

        return Ok(new { success = false, patient_id = (string?)null, message = "Patient not found in NextGate" });
    }

    [HttpPost("nextgate/patient-id")]
    public IActionResult CreateOrGetPatientId([FromBody] Patient req)
    {
        if (string.IsNullOrWhiteSpace(req.first_name) || string.IsNullOrWhiteSpace(req.last_name) || string.IsNullOrWhiteSpace(req.dob))
            return BadRequest(new { success = false, message = "Mandatory fields missing: first_name, last_name, dob" });

        var existing = _store.Patients.FirstOrDefault(p =>
            p.first_name == req.first_name && p.last_name == req.last_name && p.dob == req.dob);

        if (existing != null)
            return Ok(new { success = true, patient_id = existing.patient_id, message = "Patient found in system" });

        req.patient_id = "SELFPT" + new Random().Next(1000, 9999);
        if (string.IsNullOrEmpty(req.euid)) req.euid = "NG" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        _store.Patients.Add(req);
        _store.Save("patients.json", _store.Patients);

        return Ok(new { success = true, patient_id = req.patient_id, message = "Temporary patient created in Dash" });
    }

    [HttpPost("patient")]
    public IActionResult AddPatient([FromBody] Patient req)
    {
        if (string.IsNullOrWhiteSpace(req.first_name) || string.IsNullOrWhiteSpace(req.last_name) || string.IsNullOrWhiteSpace(req.dob))
            return BadRequest(new { success = false, message = "Mandatory fields missing: first_name, last_name, dob" });

        var dup = _store.Patients.FirstOrDefault(p =>
            p.first_name == req.first_name && p.last_name == req.last_name && p.dob == req.dob && p.phone == req.phone);
        if (dup != null)
            return Conflict(new { success = false, message = "Patient already exists", patient_id = dup.patient_id });

        req.patient_id = "PT" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        if (string.IsNullOrEmpty(req.euid)) req.euid = "NG" + Guid.NewGuid().ToString("N")[..8].ToUpper();

        _store.Patients.Add(req);
        _store.Save("patients.json", _store.Patients);

        return Ok(new { success = true, patient_id = req.patient_id, message = "Patient added successfully" });
    }

    [HttpPut("patient/{patient_id}")]
    public IActionResult UpdatePatient(string patient_id, [FromBody] Patient req)
    {
        var existing = _store.Patients.FirstOrDefault(p => p.patient_id == patient_id);
        if (existing == null) return NotFound(new { success = false, message = "Patient not found" });

        if (!string.IsNullOrEmpty(req.first_name))    existing.first_name       = req.first_name;
        if (!string.IsNullOrEmpty(req.last_name))     existing.last_name        = req.last_name;
        if (!string.IsNullOrEmpty(req.dob))           existing.dob              = req.dob;
        if (!string.IsNullOrEmpty(req.phone))         existing.phone            = req.phone;
        if (!string.IsNullOrEmpty(req.email))         existing.email            = req.email;
        if (!string.IsNullOrEmpty(req.gender))        existing.gender           = req.gender;
        if (!string.IsNullOrEmpty(req.zip_code))      existing.zip_code         = req.zip_code;
        if (!string.IsNullOrEmpty(req.address_line_one)) existing.address_line_one = req.address_line_one;
        if (!string.IsNullOrEmpty(req.address_line_two)) existing.address_line_two = req.address_line_two;
        if (!string.IsNullOrEmpty(req.city))          existing.city             = req.city;
        if (!string.IsNullOrEmpty(req.state))         existing.state            = req.state;

        _store.Save("patients.json", _store.Patients);
        return Ok(new { success = true, patient_id = existing.patient_id, message = "Patient updated successfully" });
    }

    [HttpDelete("patient/{patient_id}")]
    public IActionResult DeletePatient(string patient_id)
    {
        var existing = _store.Patients.FirstOrDefault(p => p.patient_id == patient_id);
        if (existing == null) return NotFound(new { success = false, message = "Patient not found" });

        var active = _store.Appointments.Any(a => a.patient_id == patient_id && a.status == "scheduled");
        if (active)
            return Conflict(new { success = false, message = "Cannot delete patient with active appointments. Cancel appointments first." });

        _store.Patients.Remove(existing);
        _store.Save("patients.json", _store.Patients);
        return Ok(new { success = true, message = "Patient deleted successfully" });
    }
}
