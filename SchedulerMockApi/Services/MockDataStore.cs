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

    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public MockDataStore()
    {
        _dataDir = Path.Combine(AppContext.BaseDirectory, "Data", "runtime");
        _seedDir = Path.Combine(AppContext.BaseDirectory, "Data", "seed");

        Directory.CreateDirectory(_dataDir);

        Patients = LoadOrSeed<List<Patient>>("patients.json") ?? new();
        Appointments = LoadOrSeed<List<Appointment>>("appointments.json") ?? new();
        Slots = LoadOrSeed<List<Slot>>("slots.json") ?? new();
        SlotLocks = new List<SlotLock>();
        Providers = LoadOrSeed<List<Provider>>("providers.json") ?? new();
        Facilities = LoadOrSeed<List<Facility>>("facilities.json") ?? new();
        Schedules = LoadOrSeed<List<Schedule>>("schedules.json") ?? new();
        Services = LoadOrSeed<List<Service>>("services.json") ?? new();
        VisitReasons = LoadOrSeed<List<VisitReason>>("visit_reasons.json") ?? new();
        CancellationReasons = LoadOrSeed<List<CancellationReason>>("cancellation_reasons.json") ?? new();
        AppointmentTypes = LoadOrSeed<List<AppointmentType>>("appointment_types.json") ?? new();
        Specialties = LoadOrSeed<List<Specialty>>("specialties.json") ?? new();
        PublishRequests = new List<PublishRequest>();
    }

    private T? LoadOrSeed<T>(string filename)
    {
        var runtimePath = Path.Combine(_dataDir, filename);
        var seedPath = Path.Combine(_seedDir, filename);

        string? json = null;
        if (File.Exists(runtimePath))
            json = File.ReadAllText(runtimePath);
        else if (File.Exists(seedPath))
            json = File.ReadAllText(seedPath);

        return json != null ? JsonSerializer.Deserialize<T>(json, _jsonOpts) : default;
    }

    public void Save<T>(string filename, T data)
    {
        var path = Path.Combine(_dataDir, filename);
        File.WriteAllText(path, JsonSerializer.Serialize(data, _jsonOpts));
    }

    public void Reset()
    {
        foreach (var f in Directory.GetFiles(_dataDir, "*.json"))
            File.Delete(f);

        Patients = LoadOrSeed<List<Patient>>("patients.json") ?? new();
        Appointments = LoadOrSeed<List<Appointment>>("appointments.json") ?? new();
        Slots = LoadOrSeed<List<Slot>>("slots.json") ?? new();
        SlotLocks = new();
        PublishRequests = new();
    }
}
