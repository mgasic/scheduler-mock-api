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
        [FromQuery] string? email)
    {
        var results = _store.Patients.Where(p =>
            (string.IsNullOrEmpty(first_name) || p.first_name.Contains(first_name, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(last_name) || p.last_name.Contains(last_name, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(dob) || p.dob == dob) &&
            (string.IsNullOrEmpty(zip_code) || p.zip_code == zip_code) &&
            (string.IsNullOrEmpty(phone) || p.phone == phone) &&
            (string.IsNullOrEmpty(email) || p.email == email)
        ).ToList();

        return Ok(new { success = true, patients = results });
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
        var existing = _store.Patients.FirstOrDefault(p =>
            p.first_name == req.first_name && p.last_name == req.last_name && p.dob == req.dob);

        if (existing != null)
            return Ok(new { success = true, patient_id = existing.patient_id, message = "Patient found" });

        var newId = "SELFPT" + new Random().Next(1000, 9999);
        req.patient_id = newId;
        _store.Patients.Add(req);
        _store.Save("patients.json", _store.Patients);

        return Ok(new { success = true, patient_id = newId, message = "Temporary patient created in Dash" });
    }

    [HttpPost("patient")]
    public IActionResult AddPatient([FromBody] Patient req)
    {
        req.patient_id = "PT" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        _store.Patients.Add(req);
        _store.Save("patients.json", _store.Patients);
        return Ok(new { success = true, patient_id = req.patient_id, message = "Patient added successfully" });
    }

    [HttpPut("patient/{patient_id}")]
    public IActionResult UpdatePatient(string patient_id, [FromBody] Patient req)
    {
        var existing = _store.Patients.FirstOrDefault(p => p.patient_id == patient_id);
        if (existing == null) return NotFound(new { message = "Patient not found" });

        if (!string.IsNullOrEmpty(req.first_name)) existing.first_name = req.first_name;
        if (!string.IsNullOrEmpty(req.last_name)) existing.last_name = req.last_name;
        if (!string.IsNullOrEmpty(req.dob)) existing.dob = req.dob;
        if (!string.IsNullOrEmpty(req.phone)) existing.phone = req.phone;
        if (!string.IsNullOrEmpty(req.email)) existing.email = req.email;
        if (!string.IsNullOrEmpty(req.gender)) existing.gender = req.gender;
        if (!string.IsNullOrEmpty(req.zip_code)) existing.zip_code = req.zip_code;

        _store.Save("patients.json", _store.Patients);
        return Ok(new { success = true, patient_id = existing.patient_id, message = "Patient updated successfully" });
    }
}
