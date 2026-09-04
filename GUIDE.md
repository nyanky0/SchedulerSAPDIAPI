# SOLTIUS Add-On Template — Guide Penggunaan

## Overview

Template untuk integrasi SAP Business One dengan application third party.
Data masuk ke staging DB via Web API, lalu Scheduler sync ke SAP via DI API.

```
External App ──POST──▶ Web API (.NET 8) ──INSERT──▶ Staging DB
                                    │
                              BackgroundService
                                    │
                              Log DB (api_logs + sync_logs)
                                    │
                              Scheduler (WinForms .NET 4.8, timer)
                                    │
                              SAP DI API (COM)
                                    │
                              SAP Business One
```

---

## 1. Setup Lingkungan

### Prasyarat
- .NET 8 SDK (untuk Web API)
- Visual Studio 2022 (untuk build Scheduler — COM reference SAPbobsCOM)
- SQL Server (staging + log database)
- SAP Business One dengan DI API terinstall

### Database
Buat 2 database di SQL Server:
- **test** — staging database (sales_order_header, sales_order_detail)
- **log** — logging database (api_logs, sync_logs)

Schema akan dibuat otomatis saat startup.

### Environment Variables

Set sebelum menjalankan Web API:

```cmd
:: Windows
set JWT_SIGNING_KEY=masukkan-string-random-minimal-32-karakter
set CLIENT_SECRET=masukkan-secret-untuk-client-scheduler-addon
```

```bash
# Linux/WSL
export JWT_SIGNING_KEY="masukkan-string-random-minimal-32-karakter"
export CLIENT_SECRET="masukkan-secret-untuk-client-scheduler-addon"
```

**PENTING:** Server TIDAK AKAN start kalau SigningKey atau ClientSecret masih default.

### Konfigurasi Database

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "server=IP_SERVER;database=test;uid=USER;pwd=PASS;"
  },
  "LogDatabase": {
    "Server": "IP_SERVER",
    "DatabaseName": "log",
    "UserName": "USER",
    "Password": "PASS"
  }
}
```

---

## 2. Menjalankan Web API

```cmd
:: Dari Visual Studio: tekan F5

:: Dari command line:
set JWT_SIGNING_KEY=your-secret-key-min-32-chars
set CLIENT_SECRET=your-client-secret
cd "D:\Work Stuff\Addons\Template_Addon_Staging_2026_Auth\SOLTIUS - Web API Add-On"
dotnet run --urls http://localhost:5016
```

Swagger UI hanya tersedia di mode Development (localhost).

---

## 3. Autentikasi (OAuth2 + JWT)

### 3.1 Minta Token

```
POST http://localhost:5016/oauth2/token
Content-Type: application/json

{
  "grant_type": "client_credentials",
  "client_id": "scheduler-addon",
  "client_secret": "YOUR_CLIENT_SECRET"
}
```

Response:
```json
{
  "access_token": "eyJhbG...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "abc123..."
}
```

### 3.2 Pakai Token

Semua request ke API harus sertakan header:
```
Authorization: Bearer eyJhbG...
```

Tanpa token → 401 Unauthorized.

### 3.3 Refresh Token

Token expired? Gunakan refresh_token untuk dapat access_token baru tanpa login ulang:

```
POST http://localhost:5016/oauth2/refresh
Content-Type: application/json

{
  "refresh_token": "abc123...",
  "client_id": "scheduler-addon",
  "client_secret": "YOUR_CLIENT_SECRET"
}
```

Refresh token berlaku 7 hari.

---

## 4. API Endpoints

### POST /api/SalesOrder
Kirim data Sales Order ke staging.

**Request:**
```json
{
  "cardCode": "C-001",
  "cardName": "Customer Name",
  "docDate": "2026-08-29",
  "docDueDate": "2026-09-05",
  "taxDate": "2026-08-29",
  "remarks": "Order dari system X",
  "documentLines": [
    {
      "itemCode": "ITEM-001",
      "itemDescription": "Product A",
      "warehouseCode": "WH01",
      "quantity": 10,
      "price": 50000
    }
  ]
}
```

**Validation:**
- `cardCode`: required, max 30 karakter
- `documentLines`: minimal 1, maksimal 100
- `itemCode`: required, max 30 karakter
- `quantity`: antara 0.001 - 9,999,999
- `price`: antara 0 - 999,999,999

**Response sukses:**
```json
{
  "success": true,
  "message": "Sales Order Received"
}
```

### GET /api/Status
Cek status koneksi database.

**Response:**
```json
{
  "configured": true,
  "databaseConnection": true,
  "status": "Ready"
}
```

### GET /health
Health check endpoint (untuk load balancer / monitoring).

**Response:** 200 OK

---

## 5. Menjalankan Scheduler

Scheduler harus di-build dari Visual Studio (karena COM reference SAPbobsCOM).

1. Buka `SOLTIUS - Scheduler Add-On.sln` di Visual Studio
2. Build solution (Ctrl+Shift+B)
3. Jalankan — UI Scheduler akan muncul
4. Isi konfigurasi:
   - Database staging (SQL Server)
   - SAP credentials
   - Timer interval
5. Tekan Start Scheduler

Scheduler akan:
- Membaca SO pending (process_status = 0) dari staging
- Sync ke SAP via DI API
- Update status: 1 = sukses, 2 = gagal (retry), 3 = dead-letter (max retry)

---

## 6. Menambah Custom API Baru

### Step 1: Buat Model

Buat file baru di `Models/Transaction/YourModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SOLTIUS_Web_API_Add_On.Models.Transaction
{
    public class YourModel
    {
        [Required(ErrorMessage = "FieldX is required.")]
        [StringLength(50, MinimumLength = 1)]
        [JsonPropertyName("fieldX")]
        public string FieldX { get; set; } = "";

        [Range(0, 999999)]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
```

### Step 2: Buat Repository + Service

`Repositories/IYourRepository.cs`:
```csharp
namespace SOLTIUS_Web_API_Add_On.Repositories
{
    public interface IYourRepository
    {
        Task InsertAsync(YourModel model);
    }
}
```

`Repositories/YourRepository.cs`:
```csharp
using Dapper;
using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Models.Configuration;
using SOLTIUS_Web_API_Add_On.Services.Configuration;

namespace SOLTIUS_Web_API_Add_On.Repositories
{
    public class YourRepository : IYourRepository
    {
        private readonly IDatabaseConnectionFactory _connFactory;
        private readonly IConfigurationService _cfgService;

        public YourRepository(IDatabaseConnectionFactory connFactory, IConfigurationService cfgService)
        {
            _connFactory = connFactory;
            _cfgService = cfgService;
        }

        public async Task InsertAsync(YourModel model)
        {
            DBConfig config = _cfgService.GetDatabaseConfig();
            using var conn = _connFactory.CreateConnection(config);
            await conn.OpenAsync();
            await conn.ExecuteAsync("INSERT INTO your_table (...) VALUES (...)", model);
        }
    }
}
```

`Services/IYourService.cs` + `Services/YourService.cs` — wrapper logic bisnis.

### Step 3: Buat Controller

**PENTING:** inherit `CustomApiControllerBase` untuk otomatis auth.

```csharp
using Microsoft.AspNetCore.Mvc;
using SOLTIUS_Web_API_Add_On.Models.Transaction;
using SOLTIUS_Web_API_Add_On.Repositories;

namespace SOLTIUS_Web_API_Add_On.Controllers
{
    [Route("api/[controller]")]
    public class YourController : CustomApiControllerBase
    {
        private readonly IYourRepository _repo;

        public YourController(IYourRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] YourModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _repo.InsertAsync(model);
            return Ok(new { success = true });
        }
    }
}
```

### Step 4: Register DI di Program.cs

```csharp
builder.Services.AddScoped<IYourRepository, YourRepository>();
builder.Services.AddScoped<IYourService, YourService>();
```

### Step 5: Build & Test

```bash
dotnet build
dotnet run --urls http://localhost:5016
```

Test tanpa token → 401. Test dengan token → 200.

---

## 7. Struktur Log Database

### Table: api_logs
| Kolom | Tipe | Deskripsi |
|-------|------|-----------|
| id | BIGINT PK | Auto increment |
| ts | DATETIME2 | Timestamp UTC |
| method | VARCHAR(10) | HTTP method |
| path | VARCHAR(500) | Request path |
| status | INT | HTTP status code |
| client_ip | VARCHAR(45) | IP client |
| detail | NVARCHAR(500) | Detail tambahan |
| duration_ms | INT | Durasi request |

### Table: sync_logs
| Kolom | Tipe | Deskripsi |
|-------|------|-----------|
| id | BIGINT PK | Auto increment |
| ts | DATETIME2 | Timestamp UTC |
| card_code | VARCHAR(30) | Kode customer |
| doc_type | VARCHAR(50) | Tipe dokumen |
| doc_entry | VARCHAR(50) | DocEntry SAP |
| status | VARCHAR(20) | Success/Failed |
| error_message | NVARCHAR(MAX) | Error detail |

Log ditulis secara asynchronous via `Channel<T>` + `BackgroundService`. Buffer 100 entries atau 5 detik, whichever comes first. Jika DB log down, data di-buffer dan log error.

---

## 8. Security Features

| Feature | Status | Keterangan |
|---------|--------|------------|
| JWT Bearer Auth | Aktif | Semua API butuh token |
| Startup Validation | Aktif | Reject kalau signing key default |
| Swagger Disable | Aktif | Hanya di Development |
| Rate Limiting | Aktif | 100 requests/menit per IP |
| Request Size Limit | Aktif | Max 1MB per request |
| Input Validation | Aktif | `[Required]`, `[StringLength]`, `[Range]` |
| Global Exception | Aktif | Production: generic error, Development: detail |
| Health Check | Aktif | GET /health |
| Refresh Token | Aktif | 7 hari, single-use |

---

## 9. Rate Limiting

Default: 100 requests per menit per IP. Di atas limit → 429 Too Many Requests.

Ubah di `Program.cs`:
```csharp
options.AddFixedWindowLimiter("api-limiter", limiterOptions =>
{
    limiterOptions.PermitLimit = 200; // ubah sesuai kebutuhan
    limiterOptions.Window = TimeSpan.FromMinutes(1);
});
```

---

## 10. Troubleshooting

| Masalah | Solusi |
|---------|--------|
| Server tidak start: "JWT SigningKey is not configured" | Set env var `JWT_SIGNING_KEY` (min 32 karakter) |
| Server tidak start: "ClientSecret default" | Set env var `CLIENT_SECRET` |
| 401 Unauthorized | Token expired → minta baru atau pakai refresh token |
| 429 Too Many Requests | Rate limit tercapai, tunggu 1 menit |
| 500 error di Production | Cek log DB `log` → table `api_logs` |
| Swagger tidak muncul | Pastikan environment = Development |
| Scheduler tidak sync | Cek config SAP di UI Scheduler, pastikan DI API terinstall |
| Build Scheduler gagal | Buka di Visual Studio, pastikan SAPbobsCOM reference ada |

---

## 11. File Structure

```
Template_Addon_Staging_2026_Auth/
├── SOLTIUS - Scheduler Add-On/        (.NET Framework 4.8, WinForms)
│   ├── Model/
│   │   ├── Model.cs                   (SyncLogModel — tanpa UID)
│   │   ├── PendingSalesOrder.cs       (DTO — tanpa UID)
│   │   └── SchedulerConfig.cs
│   ├── Services/
│   │   ├── DatabaseService.cs         (staging DB read/write — tanpa UID)
│   │   ├── SapSyncService.cs          (DI API COM interop)
│   │   ├── SalesOrderSyncRunner.cs    (orchestrator — tanpa UID)
│   │   └── ConfigService.cs
│   └── UI/                            (FormMain, config forms)
│
└── SOLTIUS - Web API Add-On/          (.NET 8, ASP.NET Core)
    ├── Program.cs                     (DI + middleware + startup)
    ├── appsettings.json               (config + DB + JWT)
    ├── Authentication/
    │   ├── JwtOptions.cs
    │   └── RefreshTokenStore.cs       (in-memory refresh token)
    ├── Controllers/
    │   ├── CustomApiControllerBase.cs ([Authorize] base class)
    │   ├── AuthController.cs          (POST /oauth2/token, /refresh)
    │   ├── SalesOrderController.cs    (POST /api/SalesOrder)
    │   └── Status/StatusController.cs (GET /api/Status)
    ├── Models/
    │   ├── Auth/TokenResponse.cs
    │   ├── Configuration/DBConfig.cs
    │   └── Transaction/
    │       ├── SalesOrderHeader.cs    (dengan validation attributes)
    │       └── SalesOrderDetail.cs    (dengan validation attributes)
    ├── Repositories/
    │   ├── ISalesOrderRepository.cs
    │   └── SalesOrderRepository.cs    (Dapper, tanpa UID)
    ├── Services/
    │   ├── SalesOrderService.cs
    │   ├── AuditLog/
    │   │   ├── AuditLogService.cs     (Channel<T> writer)
    │   │   ├── LogModels.cs           (ApiLogEntry, SyncLogEntry)
    │   │   ├── LogFlushWorker.cs      (BackgroundService, flush ke DB)
    │   │   └── LogDatabaseOptions.cs
    │   ├── Configuration/
    │   └── Status/
    ├── Database/
    │   ├── Factories/DatabaseConnectionFactory.cs
    │   └── Initializers/
    │       ├── MySqlDatabaseInitializer.cs
    │       ├── SqlServerDatabaseInitializer.cs
    │       └── LogDatabaseInitializer.cs  (api_logs + sync_logs tables)
    └── Middleware/
        ├── GlobalExceptionMiddleware.cs
        └── RequestLoggingMiddleware.cs   (skip swagger paths)
```

---

## 12. Checklist Deploy Production

- [ ] Set `JWT_SIGNING_KEY` environment variable (min 32 karakter random)
- [ ] Set `CLIENT_SECRET` environment variable
- [ ] Pastikan `appsettings.json` punya signing key yang benar
- [ ] Pastikan database `test` (staging) dan `log` sudah dibuat
- [ ] Pastikan Scheduler punya SAP DI API terinstall
- [ ] Build Scheduler dari Visual Studio (bukan dotnet build)
- [ ] HTTPS: jalankan di belakang IIS/nginx reverse proxy dengan TLS
- [ ] Backup database staging dan log secara berkala
