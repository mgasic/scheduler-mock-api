using System.Text.Json;
using SchedulerMockApi.Models;

namespace SchedulerMockApi.Services;

public class MockDataStore
{
    private readonly string _dataDir;
    private readonly string _seedDir;

    public List<Patient> Patients { get; private set; }
    public List<Appointment> Appointments { get; private set; }
    public List<Slot> Slots { get; private set; }
    public List<SlotLock> SlotLocks { get; private set; }
    public List<Provider> Providers { get; private set; }
    public List<Facility> Facilities { get; private set; }
    public List<Schedule> Schedules { get; private set; }
    public List<Service> Services { get; private set; }
    public List<VisitReason> VisitReasons { get; private set; }
    public List<CancellationReason> CancellationReasons { get; private set; }
    public List<AppointmentType> AppointmentTypes { get; private set; }
    public List<Specialty> Specialties { get; private set; }
    public List<PublishRequest> PublishRequests { get; private set; }

    private static readonly JsonSerializerOptions _jsonOpts = new()
        { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public MockDataStore()
    {
        _dataDir = Path.Combine(AppContext.BaseDirectory, "Data", "runtime");
        _seedDir = Path.Combine(AppContext.BaseDirectory, "Data", "seed");
        Directory.CreateDirectory(_dataDir);
        LoadAll();
    }

    private void LoadAll()
    {
        Patients            = LoadOrSeed<List<Patient>>("patients.json") ?? new();
        Appointments        = LoadOrSeed<List<Appointment>>("appointments.json") ?? new();
        Providers           = LoadOrSeed<List<Provider>>("providers.json") ?? new();
        Facilities          = LoadOrSeed<List<Facility>>("facilities.json") ?? new();
        Schedules           = LoadOrSeed<List<Schedule>>("schedules.json") ?? new();
        Services            = LoadOrSeed<List<Service>>("services.json") ?? new();
        VisitReasons        = LoadOrSeed<List<VisitReason>>("visit_reasons.json") ?? new();
        CancellationReasons = LoadOrSeed<List<CancellationReason>>("cancellation_reasons.json") ?? new();
        AppointmentTypes    = LoadOrSeed<List<AppointmentType>>("appointment_types.json") ?? new();
        Specialties         = LoadOrSeed<List<Specialty>>("specialties.json") ?? new();
        SlotLocks           = new();
        PublishRequests     = new();

        var loadedSlots = LoadOrSeed<List<Slot>>("slots.json");
        Slots = (loadedSlots != null && loadedSlots.Count > 0) ? loadedSlots : GenerateDefaultSlots();
    }

    private static List<Slot> GenerateDefaultSlots()
    {
        var slots    = new List<Slot>();
        var times    = new[] { "08:00:00","08:30:00","09:00:00","09:30:00","10:00:00","10:30:00","11:00:00","14:00:00","14:30:00","15:00:00","15:30:00","16:00:00" };
        var provFac  = new[] { ("provider123","facility123"), ("provider456","facility123"), ("provider789","facility789") };
        var apptTypes= new[] { "appt123","appt456","appt789","appt012" };
        var rng      = new Random(42);

        var start        = DateTime.Today.AddDays(1);
        int weekdaysDone = 0;

        for (int offset = 0; weekdaysDone < 22; offset++)
        {
            var d = start.AddDays(offset);
            if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) continue;
            weekdaysDone++;

            foreach (var (prov, fac) in provFac)
            {
                var shuffled = times.OrderBy(_ => rng.Next()).Take(6);
                foreach (var t in shuffled)
                {
                    slots.Add(new Slot
                    {
                        slot_id      = $"slot_{prov}_{d:yyyy-MM-dd}_{t.Replace(":", "")}",
                        provider_id  = prov,
                        facility_id  = fac,
                        slot_date    = d.ToString("yyyy-MM-dd"),
                        slot_time    = t,
                        duration     = 30,
                        appt_type_id = apptTypes[rng.Next(apptTypes.Length)],
                        is_locked    = false
                    });
                }
            }
        }
        return slots;
    }

    private T? LoadOrSeed<T>(string filename)
    {
        var runtimePath = Path.Combine(_dataDir, filename);
        var seedPath    = Path.Combine(_seedDir, filename);
        string? json = File.Exists(runtimePath) ? File.ReadAllText(runtimePath)
                     : File.Exists(seedPath)    ? File.ReadAllText(seedPath)
                     : null;
        return json != null ? JsonSerializer.Deserialize<T>(json, _jsonOpts) : default;
    }

    public void Save<T>(string filename, T data)
        => File.WriteAllText(Path.Combine(_dataDir, filename), JsonSerializer.Serialize(data, _jsonOpts));

    public void Reset()
    {
        foreach (var f in Directory.GetFiles(_dataDir, "*.json")) File.Delete(f);
        LoadAll();
    }
}
