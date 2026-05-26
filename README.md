# Scheduler Mock API

C# .NET 8 Web API koji mockuje Relatient Scheduling Workflow API za potrebe testiranja.

## Pokretanje

```bash
cd SchedulerMockApi
dotnet run
```

API će biti dostupan na `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

## Struktura podataka

```
Data/
  seed/         ← originalni mock podaci (uvek nepromenjeni, git-tracked)
  runtime/      ← runtime mutacije (gitignored, briše se pri resetu)
```

- Svi **GET** endpointi čitaju iz in-memory state-a (čitanog iz seed ili runtime JSON-a).
- **POST/PUT** operacije menjaju in-memory state i snimaju u `Data/runtime/`.
- `POST /admin/reset` briše runtime fajlove i vraća stanje na seed podatke.

## Endpointi

### Auth
| Method | Path | Opis |
|--------|------|------|
| POST | `/oauth2/token` | Generiše mock OAuth token |

### Health
| Method | Path | Opis |
|--------|------|------|
| GET | `/health/check` | Health check |

### Pacijenti
| Method | Path | Opis |
|--------|------|------|
| GET | `/patient/search` | Pretraga pacijenata |
| GET | `/nextgate/patient-id` | Dohvati patient ID iz NextGate |
| POST | `/nextgate/patient-id` | Kreiranje privremenog pacijenta |
| POST | `/patient` | Dodaj pacijenta |
| PUT | `/patient/{patient_id}` | Ažuriranje pacijenta |

### Termini
| Method | Path | Opis |
|--------|------|------|
| POST | `/appointments` | Zakaži termin |
| GET | `/appointments/{appointment_id}` | Detalji termina |
| GET | `/appointments/patient/{patient_id}` | Svi termini pacijenta |
| GET | `/appointments/cancellation-reasons` | Razlozi otkazivanja |
| PUT | `/appointments/{appointment_id}/cancel` | Otkaži termin |
| POST | `/appointments/{appointment_id}/reschedule` | Pomeri termin |

### Slotovi
| Method | Path | Opis |
|--------|------|------|
| POST | `/slots` | Pretraga slobodnih termina |
| POST | `/slots/earliest-availability` | Najraniji slobodan termin |
| POST | `/slots/lock` | Zaključaj slot |
| POST | `/slots/release` | Oslobodi slot |
| POST | `/slots/manual-publish` | Publish slots na Pub Sub |
| GET | `/slots/manual-publish/status` | Status async publish zahteva |

### Provideri
| Method | Path | Opis |
|--------|------|------|
| GET | `/providers` | Lista svih providera |
| GET | `/providers/{provider_id}` | Provider po ID |
| GET | `/providers/{provider_id}/visit-reasons` | Razlozi posete za providera |
| GET | `/providers/{provider_id}/visit-reasons/search` | Pretraga razloga |
| POST | `/providers/manual-publish` | Publish providers |
| GET | `/providers/manual-publish/status` | Status |

### Objekti/Ustanove
| Method | Path | Opis |
|--------|------|------|
| GET | `/facilities` | Lista svih ustanova |
| GET | `/facilities/{facility_id}` | Ustanova po ID |
| POST | `/facilities/manual-publish` | Publish facilities |
| GET | `/facilities/manual-publish/status` | Status |

### Rasporedi
| Method | Path | Opis |
|--------|------|------|
| GET | `/schedules` | Lista rasporeda |
| GET | `/schedules/{schedule_id}` | Raspored po ID |
| POST | `/schedules/manual-publish` | Publish schedules |
| GET | `/schedules/manual-publish/status` | Status |

### Ostalo
| Method | Path | Opis |
|--------|------|------|
| GET | `/services` | Sve usluge |
| GET | `/appointment-types` | Tipovi termina |
| GET | `/specialties` | Specijalnosti |
| GET | `/visit-reasons` | Svi razlozi posete |
| GET | `/visit-reasons/search?q=` | Pretraga razloga |
| GET | `/triage?reason_id=` | Triage stablo |
| GET | `/triage/fhir?reason_id=` | Triage u FHIR formatu |

### Admin
| Method | Path | Opis |
|--------|------|------|
| POST | `/admin/reset` | **Reset svih podataka na seed stanje** |

## Mock podaci (seed)

| Entitet | Podaci |
|---------|--------|
| Pacijenti | 3 (Robert Downey, Jane Smith, John Doe) |
| Provideri | 3 (Emily Chen, Michael Brown, Sarah Johnson) |
| Ustanove | 3 |
| Slotovi | ~150 (Jun 2026, radni dani) |
| Termini | 2 zakazana termina |
| Razlozi posete | 4 |
| Razlozi otkazivanja | 5 |
| Tipovi termina | 4 |

## Test primer - kompletan workflow

```bash
# 1. Dohvati token
curl -X POST http://localhost:5000/oauth2/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials"

# 2. Pretraga pacijenta
curl "http://localhost:5000/patient/search?first_name=Robert&last_name=Downey&dob=07/15/1970&zip_code=30062&phone=4702390995"

# 3. Pretraga slobodnih slotova
curl -X POST http://localhost:5000/slots \
  -H "Content-Type: application/json" \
  -d '{"provider_id":"provider123","facility_id":"facility123","start_date":"2026-06-01","end_date":"2026-06-07"}'

# 4. Zaključaj slot
curl -X POST http://localhost:5000/slots/lock \
  -H "Content-Type: application/json" \
  -d '{"slot_id":"SLOT_ID_OVDE","patient_id":"123456"}'

# 5. Zakaži termin
curl -X POST http://localhost:5000/appointments \
  -H "Content-Type: application/json" \
  -d '{"slot_date":"2026-06-05","slot_time":"10:00:00","appt_type_id":"appt456","duration":30,"facility_id":"facility123","patient_id":"123456","provider_id":"provider123","reason_id":"reason001","reason":"Back pain","service_id":"service123"}'

# 6. Reset podataka
curl -X POST http://localhost:5000/admin/reset
```
