# Scheduler Mock API – Tehnička Dokumentacija

> Verzija: 1.1  
> Okruženje: Windows / IIS + .NET 8 ASP.NET Core  
> Repozitorijum: https://github.com/mgasic/scheduler-mock-api

---

## 1. Arhitektura i princip rada

Scheduler Mock API je C# .NET 8 Web API koji simulira ponašanje Relatient Scheduling Workflow API-ja za lokalno testiranje bez potrebe za konekcijom na produkcione sisteme.

### Tok podataka

```
IIS → aspNetCore modul → SchedulerMockApi.dll
                              │
                    MockDataStore (Singleton)
                         /         \
              Data/seed/         Data/runtime/
            (originalni         (izmene tokom
            mock podaci,         testiranja,
            uvek netaknuti)      gitignored)
```

**Princip perzistencije:**
- Svi podaci se čuvaju **in-memory** tokom rada servisa.
- `GET` endpointi čitaju iz memorije.
- `POST` / `PUT` / `DELETE` operacije menjaju in-memory stanje i snimaju promenjen JSON u `Data/runtime/`.
- `POST /admin/reset` briše sve runtime fajlove i vraća sistem na seed stanje.
- Seed fajlovi u `Data/seed/` su **uvek nepromenjeni** i praćeni gitom – garantuju repeatability testova.

---

## 2. Pokretanje na IIS (Windows)

### Preduslovi

1. Windows Server 2016+ ili Windows 10/11
2. IIS sa instaliranim **ASP.NET Core Hosting Bundle** (.NET 8):  
   https://dotnet.microsoft.com/en-us/download/dotnet/8.0
3. IIS modul `AspNetCoreModuleV2` (instalira se automatski sa Hosting Bundle)

### Koraci za deploy

```powershell
# 1. Kloniraj repo
git clone https://github.com/mgasic/scheduler-mock-api
cd scheduler-mock-api

# 2. Publish projekta
cd SchedulerMockApi
dotnet publish -c Release -o C:\inetpub\wwwroot\scheduler-mock-api

# 3. Kopiraj seed podatke
xcopy /E /I Data\seed C:\inetpub\wwwroot\scheduler-mock-api\Data\seed

# 4. Kreiraj runtime folder (mora biti writable za IIS korisnika)
mkdir C:\inetpub\wwwroot\scheduler-mock-api\Data\runtime

# 5. Prava pristupa za IIS_IUSRS
icacls "C:\inetpub\wwwroot\scheduler-mock-api\Data\runtime" /grant "IIS_IUSRS:(OI)(CI)F"
```

### IIS Site konfiguracija

1. Otvori **IIS Manager**
2. Kreiraj novi Site ili Application:
   - **Physical path:** `C:\inetpub\wwwroot\scheduler-mock-api`
   - **Port:** `5000` (ili po dogovoru)
   - **Application Pool:** bez managed code (No Managed Code)
3. `web.config` je već u publish folderu sa ispravnom konfiguracijom.

### Provera

```
http://localhost:5000/swagger   ← Swagger UI
http://localhost:5000/health/check   ← Health endpoint
```

---

## 3. Šifarnici (Reference Data)

Svi šifarnici su potrebni za rad administracije pacijenata, zakazivanje i otkazivanje termina. Čuvaju se u seed JSON fajlovima i učitavaju pri startu.

### 3.1 Tipovi termina (`/appointment-types`)

| ID | Naziv | Trajanje |
|----|-------|----------|
| `appt123` | New Patient Visit | 60 min |
| `appt456` | Follow-up | 30 min |
| `appt789` | Annual Physical | 45 min |
| `appt012` | Sick Visit | 20 min |

### 3.2 Razlozi posete (`/visit-reasons`)

| ID | Naziv | Provideri |
|----|-------|-----------|
| `reason001` | Back Pain | provider123, provider456 |
| `reason002` | Annual Physical | provider123 |
| `reason003` | Cold/Flu Symptoms | provider123, provider456 |
| `reason004` | Pediatric Checkup | provider789 |

### 3.3 Razlozi otkazivanja (`/appointments/cancellation-reasons`)

| ID | Opis |
|----|------|
| `cr_01` | Patient requested cancellation |
| `cr_02` | Provider unavailable |
| `cr_03` | Patient no-show |
| `cr_04` | Insurance issue |
| `cr_05` | Personal emergency |

### 3.4 Usluge (`/services`)

| ID | Naziv |
|----|-------|
| `service123` | General Consultation |
| `service456` | Follow-up Visit |
| `service789` | Preventive Care |

### 3.5 Specijalnosti (`/specialties`)

| ID | Naziv |
|----|-------|
| `sp001` | Family Medicine |
| `sp002` | Internal Medicine |
| `sp003` | Pediatrics |
| `sp004` | Orthopedics |

### 3.6 Provideri (`/providers`)

| ID | Ime | Specijalnost | Ustanove |
|----|-----|-------------|----------|
| `provider123` | Emily Chen | Family Medicine | facility123, facility456 |
| `provider456` | Michael Brown | Internal Medicine | facility123 |
| `provider789` | Sarah Johnson | Pediatrics | facility789 |

### 3.7 Ustanove (`/facilities`)

| ID | Naziv | Grad |
|----|-------|------|
| `facility123` | Downtown Medical Center | Springfield, IL |
| `facility456` | North Side Clinic | Springfield, IL |
| `facility789` | Pediatric Health Center | Springfield, IL |

---

## 4. Administracija pacijenata – API Reference

### 4.1 Pretraga pacijenata

```
GET /patient/search
```

### 4.2 Dohvatanje pacijenta po ID

```
GET /patient/{patient_id}
```

### 4.3 Kreiranje pacijenta

```
POST /patient
```

### 4.4 Ažuriranje pacijenta

```
PUT /patient/{patient_id}
```

### 4.5 Brisanje pacijenta

```
DELETE /patient/{patient_id}
```

### 4.6 NextGate – Provera / Kreiranje privremenog pacijenta

```
GET /nextgate/patient-id
POST /nextgate/patient-id
```

---

## 5. Zakazivanje termina – API Reference

### 5.1 Pretraga slotova

```
POST /slots
POST /slots/earliest-availability
```

### 5.2 Zaključavanje i oslobađanje slotova

```
POST /slots/lock
POST /slots/release
```

### 5.3 Zakazivanje termina

```
POST /appointments
```

### 5.4 Pregled termina pacijenta

```
GET /appointments/patient/{patient_id}
GET /appointments/{appointment_id}
```

### 5.5 Otkazivanje i pomeranje termina

```
GET  /appointments/cancellation-reasons
PUT  /appointments/{appointment_id}/cancel
POST /appointments/{appointment_id}/reschedule
```

---

## 6. Admin

### 6.1 Reset mock podataka

```
POST /admin/reset
```
