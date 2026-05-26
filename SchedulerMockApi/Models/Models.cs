namespace SchedulerMockApi.Models;

public class Patient
{
    public string patient_id { get; set; } = "";
    public string first_name { get; set; } = "";
    public string last_name { get; set; } = "";
    public string dob { get; set; } = "";
    public string phone { get; set; } = "";
    public string email { get; set; } = "";
    public string? gender { get; set; }
    public string? zip_code { get; set; }
    public string? address_line_one { get; set; }
    public string? address_line_two { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? euid { get; set; }
}

public class Appointment
{
    public string appointment_id { get; set; } = "";
    public string patient_id { get; set; } = "";
    public string provider_id { get; set; } = "";
    public string facility_id { get; set; } = "";
    public string appt_type_id { get; set; } = "";
    public string slot_date { get; set; } = "";
    public string slot_time { get; set; } = "";
    public int duration { get; set; }
    public string reason_id { get; set; } = "";
    public string? reason { get; set; }
    public string? service_id { get; set; }
    public string status { get; set; } = "scheduled";
    public string? cancellation_reason_id { get; set; }
    public string? appointment_comment { get; set; }
    public bool? add_patient_to_waitlist { get; set; }
    public string? waitlisted { get; set; } = "NO";
    public string? onRecall { get; set; } = "NO";
}

public class Slot
{
    public string slot_id { get; set; } = "";
    public string provider_id { get; set; } = "";
    public string facility_id { get; set; } = "";
    public string slot_date { get; set; } = "";
    public string slot_time { get; set; } = "";
    public int duration { get; set; } = 30;
    public string appt_type_id { get; set; } = "";
    public bool is_locked { get; set; } = false;
}

public class SlotLock
{
    public string lock_id { get; set; } = "";
    public string slot_id { get; set; } = "";
    public string? patient_id { get; set; }
    public DateTime locked_at { get; set; }
    public string status { get; set; } = "locked";
}

public class Provider
{
    public string provider_id { get; set; } = "";
    public string first_name { get; set; } = "";
    public string last_name { get; set; } = "";
    public string specialty { get; set; } = "";
    public string npi { get; set; } = "";
    public List<string>? facility_ids { get; set; }
}

public class Facility
{
    public string facility_id { get; set; } = "";
    public string name { get; set; } = "";
    public string address { get; set; } = "";
    public string city { get; set; } = "";
    public string state { get; set; } = "";
    public string zip_code { get; set; } = "";
    public string phone { get; set; } = "";
}

public class Schedule
{
    public string schedule_id { get; set; } = "";
    public string provider_id { get; set; } = "";
    public string facility_id { get; set; } = "";
    public string ehr_name { get; set; } = "";
    public string tablespace_id { get; set; } = "";
    public string provider_group_id { get; set; } = "";
    public string client_id { get; set; } = "";
}

public class Service
{
    public string service_id { get; set; } = "";
    public string name { get; set; } = "";
    public string? description { get; set; }
}

public class VisitReason
{
    public string reason_id { get; set; } = "";
    public string name { get; set; } = "";
    public string? description { get; set; }
    public bool is_treated_by_provider { get; set; } = true;
    public List<string>? provider_ids { get; set; }
    public List<TriageNode>? triage { get; set; }
}

public class TriageNode
{
    public string node_id { get; set; } = "";
    public string question { get; set; } = "";
    public List<TriageAnswer>? answers { get; set; }
}

public class TriageAnswer
{
    public string answer_id { get; set; } = "";
    public string text { get; set; } = "";
    public string? next_node_id { get; set; }
    public string? action { get; set; }
}

public class CancellationReason
{
    public string reason_id { get; set; } = "";
    public string description { get; set; } = "";
}

public class AppointmentType
{
    public string appt_type_id { get; set; } = "";
    public string name { get; set; } = "";
    public int duration { get; set; } = 30;
    public string? description { get; set; }
}

public class Specialty
{
    public string specialty_id { get; set; } = "";
    public string name { get; set; } = "";
}

public class PublishRequest
{
    public string request_id { get; set; } = "";
    public string status { get; set; } = "Success";
    public string message { get; set; } = "";
    public string processed_ts { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
    public string type { get; set; } = "";
}
