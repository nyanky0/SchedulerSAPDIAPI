# 🛠️ Cara Kerja Program — SOLTIUS Add-On (Staging 2026 Auth)

> Status: **MASIH DALAM PENGEMBANGAN (belum jadi)**
> Dokumen ini menjelaskan cara kerja program **pada kondisi saat ini**, ditulis ulang dari source code (bukan hanya dari GUIDE.md).
> Terakhir diperbarui: September 2026

---

## 1. Gambaran Umum (Apa Program Ini?)

Program ini adalah **template add-on integrasi SAP Business One** dengan aplikasi third-party, buatan template SOLTIUS.

**Tujuannya:** Aplikasi eksternal (punya PC luar) mengirim data penjualan → masuk ke **database staging** via **Web API** → lalu **Scheduler (Add-On)** menarik data pending dari staging → dan **membuat dokumen di SAP B1** lewat DI API (COM).

```
Aplikasi Eksternal (PC luar)
        │  HTTPS / POST
        ▼
┌───────────────────────────────┐
│  SOLTIUS - Web API Add-On    │  (.NET 8, ASP.NET Core, port 5006)
│  - Auth OAuth2 + JWT          │
│  - Menerima Sales Order       │
│  - Menulis ke STAGING database│
│  - Audit log (Channel buffer) │
└──────────────┬────────────────┘
               │ menulis
               ▼
        ┌──────────────┐         ┌───────────────────┐
        │  STAGING DB  │  <-fix-> │  LOG DB           │
        │  (test)      │    read  │  (log)            │
        └──────┬───────┘         └───────────────────┘
               │  polling (timer)
               ▼
┌───────────────────────────────┐
│  SOLTIUS - Scheduler Add-On   │  (.NET Framework 4.8, WinForms + bisa jadi Windows Service)
│  - Baca SO pending status=0   │
│  - Sync ke SAP via DI API COM │
│  - Update status + retry logic│
└──────────────┬────────────────┘
               │  SAP DI API (COM, SAPbobsCOM)
               ▼
        ┌──────────────┐
        │  SAP B1 (DB) │
        └──────────────┘
```

> ⚠️ Catatan penting: Di GUIDE.prinsip alurnya `External App → Web API → Staging DB → Scheduler → SAP DI API`. Namun **pada kondisi kode saat ini**, Scheduler **TIDAK memakai Web API** untuk situs data — Scheduler baca langsung dari Staging DB (SQL Server) sendiri. Web API dipakai Scheduler hanya untuk **Push konfigurasi profil** (`POST /api/ProfileSync`). Ini penting biar tidak salah paham.

---

## 2. Dua Project dalam Solusi

Solution `SOLTIUS - Scheduler Add-On.sln` berisi **2 project**:

| Project | Framework | Jenis | Peran |
|---------|-----------|-------|-------|
| `SOLTIUS - Web API Add-On` | .NET 8 (ASP.NET Core) | Web API (WinExe) | Menerima data dari aplikasi eksternal, tulis ke staging DB, autentikasi, logging |
| `SOLTIUS - Scheduler Add-On` | .NET Framework 4.8 | WinForms + Windows Service | Menarik data pending dari staging, buat dokumen di SAP via DI API |

---

## 3. Komponen A: SOLTIUS - Web API Add-On

### 3.1 Teknologi & Dependency
- `net8.0`, ASP.NET Core, Dapper, Microsoft.Data.SqlClient, MySql.Data, JwtBearer, Swashbuckle.
- Mendukung **2 tipe staging DB**: MySQL dan SQL Server (dipilih dari `Config.xml` → `ExternalDBType`).

### 3.2 Alur Startup (`Program.cs`)
1. **Kestrel limit** — max request body 1 MB.
2. **Validasi JWT** — kalau `SigningKey` kosong / < 32 karakter / berisi placeholder `BABIBABI...` → **langsung throw** (server tidak start). Env var `JWT_SIGNING_KEY`.
3. **Validasi ClientSecret** tiap client — kalau placeholder → tidak start. Env var `CLIENT_SECRET`.
4. **JWT Bearer auth** + Authorization.
5. **RefreshTokenStore** (singleton, **in-memory**) — TTL dari `RefreshTokenDays` (default 7 hari → 10080 menit).
6. **Rate limiting** — 100 request/menit per IP, fixed window,  429 kalau lewat.
7. **Health check** — `GET /health`.
8. **Audit log channels** — 2 buah `Channel<T>` bounded (kapasitas 10000, `DropOldest`): satu untuk api_logs, satu untuk sync_logs.
9. **LogFlushWorker** (BackgroundService) — dibaca kalau `LogDatabase` terisi.
10. **DI registrasi** — services, repositories, connection factory, initializers.
11. **Startup DB init** — kalau `Config.xml` ada → buat tabel staging otomatis. Log DB → buat tabel log otomatis.
12. **Middleware pipeline**: GlobalException → RequestLogging → Authentication → Authorization → RateLimiter → endpoints.
13. **Swagger** hanya aktif di environment `Development`.

### 3.3 Autentikasi (OAuth2 + JWT)
- `POST /oauth2/token` — grant `client_credentials`, cek `client_id` + `client_secret` dari `Jwt:Clients`. Dapat `access_token` + `refresh_token`.
- `POST /oauth2/refresh` — tukar refresh token (single-use, dihapus dari store saat dipakai) → token baru.
- Semua endpoint API (kecuali `/oauth2/*` dan `/health`) harus sertakan `Authorization: Bearer <jwt>`.
- Refresh token store **in-memory** → hilang saat Web API restart (lihat §6 kekurangan).

### 3.4 Endpoint yang Ada

| Method | Path | Auth | Fungsi |
|--------|------|------|--------|
| POST | `/oauth2/token` | No (anon) | Minta access + refresh token |
| POST | `/oauth2/refresh` | No (anon) | Refresh access token |
| POST | `/api/SalesOrder` | Yes | Terima Sales Order → tulis ke staging DB |
| POST | `/api/ProfileSync` | Yes | Terima XML konfigurasi profil dari Scheduler |
| GET | `/api/Status` | Yes | Cek status config + koneksi DB |
| GET | `/health` | No | Health check |

### 3.5 Endpoint Detail

**`POST /api/SalesOrder`**
- Body = `SalesOrderHeader` (Request DTO):
  - `cardCode` (required, max 30) • `cardName` (max 200) • `docDate/docDueDate/taxDate` • `remarks` (max 254) • `documentLines[]`
  - Detail per line: `itemCode` (required max 30) • `itemDescription` (max 200) • `warehouseCode` (max 20) • `quantity` (range 0.001–9.999.999) • `price` (0–999.999.999)
  - Validasi count line: min 1, max 100.
- Alur: validasi `ModelState` → `SalesOrderService.SaveSalesOrderAsync` → `SalesOrderRepository.InsertSalesOrderAsync` (transaksi Dapper, insert header + detail, `process_status=0`) → log audit.
- Response: `200 { success=true, message="Sales Order Received" }` / `400` kalau validasi gagal.

**`POST /api/ProfileSync`** (Content-Type: `application/xml`)
- Digunakan **Scheduler** untuk mengirim konfigurasi database staging aktif → Web API → **disimpan ke `Configuration/Config.xml`**.
- XML berisi `<Configuration><ExternalDatabase>...` (`ExternalDBType`, server, port, db, user, pass).
- Server memakai string ini untuk konek ke **staging DB** (yang dipakai sendiri oleh Web API untuk menulis data).
- Kalau gagal konek → 500. Ada **retry queue** di sisi Scheduler.

**`GET /api/Status`**
- `configured` = apakah `Config.xml` ada.
- `databaseConnection` = apakah bisa konek ke DB config.
- `status` = "Ready" / "Configuration required" / "Database unavailable".
- Return `503` kalau belum ready.
- Ada **cache 10 detik** supaya tidak buka koneksi DB tiap request.

### 3.6 Database yang Dipakai Web API
- **Staging DB** (`test`) — dari `Config.xml`:
  - `sales_order_header` — `id`, `cardcode`, `cardname`, `docdate`, `docduedate`, `taxdate`, `remarks`, `process_status`, `retrycount`, `created_at`, `updated_at`, `processed_at`, `errormessage`.
  - `sales_order_detail` — `id`, `header_id`, `linenum`, `itemcode`, `itemname`, `warehouse`, `quantity`, `price`, `process_status`, `retrycount`, `created_at`, `updated_at`, `processed_at`, `errormessage`.
- **Log DB** (`log`) — dari `appsettings.json` (`LogDatabase`):
  - `api_logs` — `id`, `ts`, `method`, `path`, `status`, `client_ip`, `uid`, `detail`, `duration_ms`.
  - `sync_logs` — `id`, `ts`, `uid`, `card_code`, `doc_type`, `doc_entry`, `item_code`, `quantity`, `price`, `warehouse`, `status`, `error_source`, `error_message`.

> Tabel staging & log dibuat **otomatis saat startup** (IF NOT EXISTS).

### 3.7 Audit Log (Channel + Background Worker)
- `AuditLogService` menulis entry ke `Channel<T>` (non-blocking).
- `LogFlushWorker` (BackgroundService) tiap **5 detik / 100 entry** membuang batch ke DB log (batch insert Dapper).
- Kalau DB log down → batch yang sudah dibaca dari channel **hilang** (catatan §6), log error saja.

---

## 4. Komponen B: SOLTIUS - Scheduler Add-On

### 4.1 Teknologi & Dependency
- `.NET Framework 4.8`, WinForms, `COMReference SAPbobsCOM` (DI API 10.0), System.ServiceProcess.
- NuGet: ClosedXML (Excel), MySql.Data, Newtonsoft.Json, RestSharp, System.Text.Json, BouncyCastle, dll.

### 4.2 Dual Mode (UI / Windows Service)
`Program.Main`:
- `Environment.UserInteractive == false` → **jalan sebagai Windows Service** (`SchedulerWindowsService`).
- interactive → **WinForms**:
  1. Cek file `Security/AccessConfig.xml` — kalau belum ada → buka `FormPassword` (setup password awal).
  2. Lalu `FormLogin` (login aplikasi).

### 4.3 Security / Password
- `Encryption.cs` — class AES (Rfc2898DeriveBytes) untuk enkripsi password.
- `AccessPasswordStore.cs` — simpan password akses di `Security\AccessConfig.xml`, format **AES** (baru), migrasi otomatis dari format lama Base64.
- ⚠️ **Kunci AES hardcoded di code** (`"SOMETHING DEAD INSIDE"`) — hanya obfuscation, bukan keamanan nyata.

### 4.4 Windows Service (`SchedulerWindowsService.cs`)
- Membaca `SchedulerConfig.Load()` untuk `ServiceName`.
- `OnStart` → buat `SyncSchedulerEngine`, log ke Event Log.
- `OnStop` → stop engine.

### 4.5 Konfigurasi Profil
- `SchedulerConfig.cs` → file `SchedulerSettings.xml`: mode (`Interval`/`Realtime`), interval menit/detik, `SyncSalesOrder`, `SyncServiceLayer` (belum diimplementasi di engine), nama service.
- `ConfigService.cs`:
  - `AllConfigurations.xml` = daftar semua profil (`AppConfig`).
  - `DefaultConfig.xml` = profil yang aktif.
  - `GetActiveConfiguration()` mengembalikan `AppConfig` profil aktif.
  - `BuildStagingConnectionString(config)` — **hanya SQL Server** (kalau tipe lain → null, scheduler-skip).

### 4.6 Sync Engine (`SyncSchedulerEngine.cs`)
- `System.Threading.Timer`, interval dari `ActiveIntervalSeconds`.
- Cegah overlap dengan flag `_isExecuting` (skip kalau cycle sebelumnya belum selesai).
- Tiap cycle: cek `SyncSalesOrder`/`SyncServiceLayer` → kalau semua mati, lewati. Ambil profil aktif → `SalesOrderSyncRunner.RunPendingSync`.
- Tulis log ke file `SchedulerLog.txt` di folder aplikasi.

### 4.7 Sinkronisasi Sales Order (`SalesOrderSyncRunner.cs`)
1. `ConfigService.BuildStagingConnectionString` → `DatabaseService`.
2. `LoadPendingSalesOrders()` → SO dengan `process_status = 0` (join header+detail).
3. Untuk tiap order:
   - Kalau `IsRetryLimitExceeded(HeaderId)` → `MarkAsExceededRetryLimit` (status=3) → log "max retry limit exceeded" → skip.
   - Else: `SapSyncService.ConnectToDIAPI(config)` → `ExecuteSalesOrderSync(order)` → buat dokumen SAP → `UpdateSalesOrderStatus(headerId, 1)` (sukses, reset retrycount).
   - Gagal → `UpdateSalesOrderStatus(headerId, 2)` (gagal, bertambah retrycount) → log error. Kalau retry penuh → status 3.
4. Log hasil ke DB (status Success/Failed) via `SyncLogModel`.

### 4.8 Koneksi & Buat Dokumen SAP (`SapSyncService.cs`)
- `ConnectToDIAPI` — buat `Company` COM, pilih `DbServerType` dari `SAPServerType` (string mengandung "2019"/"2017"/"2016"/"2022"/"HANA", default MSSQL2017), set server/license/companyDB/user/dbuser.
- `ExecuteSalesOrderSync(PendingSalesOrder)` — buat `oOrders` (`BoObjectTypes.oOrders`), set header (CardCode, DocDate, DocDueDate, TaxDate, Comments), loop lines (ItemCode, Quantity, Price, WarehouseCode, `Add()`), lalu `oOrder.Add()`, return `GetNewObjectKey()` (DocEntry).
- Release COM object (child lalu parent) di `finally` — mencegah memory leak COM.

### 4.9 Profile Sync ke Web API (`ProfileSyncService.cs`)
- `BuildExternalDatabaseXml(config)` — ubah `AppConfig` → XML `<Configuration><ExternalDatabase>`.
- `SendProfileSync` → `POST {WebApiUrl}/api/ProfileSync` (Content-Type application/xml, timeout 90s).
- Kalau gagal → simpan payload ke **`PendingProfileSync.xml`** (retry queue, dedup, expiry 7 hari) → `FlushPendingSyncs` coba kirim ulang.
- ⚠️ Dikirim **tanpa token & tanpa HTTPS** kalau URL pakai `http:` (lihat §6 kekurangan).

### 4.10 Database (Local Scheduler)
`DatabaseService.cs` (koneksi ke staging DB, bukan SAP DB):
- Buat tabel **`TBL_SYNC_HISTORY`** & **`TBL_SYNC_ERROR`** (IF NOT EXISTS).
- `SaveLogToDatabase` — insert ke history; kalau Failed → juga insert ke error table.
- `LoadPendingSalesOrders` — query SO pending + lines.
- `UpdateSalesOrderStatus` — update status header + detail, kelola `retrycount` (sukses→0, gagal→+1), `errormessage`, `processed_at`.
- `IsRetryLimitExceeded` — cek `retrycount >= _maxRetryCount (default 5)`.
- `MarkAsExceededRetryLimit` — status=3 + message.
- `LoadLogHistory`, `GetLatestUnresolvedErrors`, `MarkErrorsAsResolved` — untuk UI.

---

## 5. Perbedaan Kode vs GUIDE.md (hal yang perlu diperhatikan)

| Aspek | GUIDE.md bilang | Kondisi kode sekarang |
|-------|-----------------|------------------------|
| Sync Service Layer | `SyncServiceLayer` ada di config | **Belum diimplementasi** di engine — flag ada tapi tidak menjalankan apa-apa. |
| Staging DB | MySQL / SQL Server | Web API dukung keduanya; **Scheduler hanya SQL Server** (`BuildStagingConnectionString` return null selain SQLServer). |
| Alur data | Web API → staging → scheduler → SAP | Benar. Tapi Scheduler **baca langsung dari staging DB**, bukan via Web API. |
| Retry default | GUIDE: pakai config `Scheduler:MaxRetryCount` | Scheduler pakai **hardcoded 5** di `SalesOrderSyncRunner.GetMaxRetryCount()`; Web API punya config `Scheduler:MaxRetryCount` tapi sepertinya tidak dipakai Scheduler (bedanya lokal). |

---

## 6. Saran Kekurangan / Perbaikan (untuk dipertimbangkan)

Berikut kekurangan yang acuu temukan dari baca code (bukan cuma asumsi) — berguna untuk roadmap:

### 🔴 Keamanan (PRIORITAS TINGGI)
1. **Refresh token store in-memory** — hilang saat Web API restart; semua refresh token jadi invalid. Pertimbangkan persistent store (SQL/Redis).
2. **ProfileSync dikirim tanpa HTTPS & tanpa auth di sisi Scheduler** — kredensial DB (user/password staging) dikirim plaintext kalau URL `http://`. Wajib HTTPS di produksi.
3. **Password DB & kredensial di `appsettings.json` / `Config.xml` ter-commit** — contoh `REDACTED`/`REDACTED` di file (plaintext). Pindahkan ke env var / secret store.
4. **Kunci AES hardcoded** (`"SOMETHING DEAD INSIDE"`) di `Encryption.cs` — bukan keamanan sungguhan; hanya obfuscation.

### 🟡 Ketahanan / Keandalan
5. **`GetCurrentRetryCount` selalu return 0** di `SalesOrderSyncRunner` — logika retry pakai proxy `IsRetryLimitExceeded` + update counter; baris `if (newRetryCount >= GetMaxRetryCount())` **tidak pernah true** → jalur dead-letter di dalam catch tidak efektif. Perlu dibersihkan.
6. **Log DB down → batch log hilang** — `LogFlushWorker` punya kapasitas channel, tapi begitu batch keluar dari channel lalu gagal insert, entry hilang. Pertimbangkan retry/backpresure.
7. **Status cache 10 detik** — `static` shared antar request; OK untuk monitoring, tapi perlu hati-hati kalau banyak instance.
8. **Error di `ProfileSync` controller** return 500 dengan pesan Kafkaesque; kalau config belum ada, `GetDatabaseConfig()` throw → 500 terus sampai config dibuat manual.

### 🟢 Arsitektur / Code Health
9. **Dua overload `ExecuteSalesOrderSync`** (`SyncLogModel` yang lama & `PendingSalesOrder`) — yang `SyncLogModel` tampaknya tidak dipakai runner; jadi dead code. 
10. **Model vs DB mismatch** — `SyncLogModel.Quantity` adalah `int` tapi quantity staging `decimal` → bisa ke-truncate. `SyncLogModel` log quantity pakai `(int)`.
11. **Web API membaca data staging untuk logging** — Scheduler & Web API **DUA jalur logging terpisah** (`TBL_SYNC_HISTORY` di scheduler vs `api_logs`+`sync_logs` di web api). Perlu disepakati mana source of truth.
12. **`SyncServiceLayer` flag ada tapi tidak diimplementasi** — kalau diaktifkan user diam-diam tidak sinkron apa pun. Matikan atau sematkan warning.

---

## 7. Rencana / Status Pengembangan

Status saat dokumentasi ini dibuat: **belum jadi / masih dikembangkan**.
- Web API inti (auth, sales order, status, profile sync, logging) sudah ada & berjalan secara konsep.
- Scheduler (engine, sync sales order, profile push, retry, UI login/service) sudah ada.
- Yang **paling mungkin dikerjakan selanjutnya**:
  - Sinkronisasi Service Layer (`SyncServiceLayer`) yang masih stub.
  - Implementasi retry logic yang benar (membersihkan `GetCurrentRetryCount`).
  - Memutuskan satu jalur logging.
  - Menambang HTTPS / pemindahan kredensial ke env var / secret.
  - Ekspor Excel (ClosedXML sudah ada dependency-nya — cek apakah fitur Excel di UI sudah dipakai).

---

## 8. Cara Menjalankan (ringkas)

### Web API
```bash
set JWT_SIGNING_KEY=your-random-key-min-32-chars
set CLIENT_SECRET=your-client-secret
cd "D:\...\Template_Addon_Staging_2026_Auth 2\Template_Addon_Staging_2026_Auth\SOLTIUS - Web API Add-On"
dotnet run --urls http://localhost:5006
```
- Swagger: `http://localhost:5006/swagger` (Development).

### Scheduler
- Buka `.sln` di Visual Studio 2022 (karena COM reference SAPbobsCOM), build, jalankan.
- UI → isi profil → Start Scheduler.

---

## 9. File Structure (ringkas)

```
Template_Addon_Staging_2026_Auth/
├── GUIDE.md
├── Documentation/                       <-- folder ini
│   ├── README.md
│   ├── 01_KondisiSaatIni.md
│   └── CHANGELOG.md
├── SOLTIUS - Web API Add-On/            (.NET 8)
│   ├── Program.cs
│   ├── Authentication/  (JwtOptions, RefreshTokenStore)
│   ├── Controllers/     (CustomApiControllerBase, Auth, SalesOrder, ProfileSync, Status)
│   ├── Models/          (Auth, Configuration, Status, Transaction)
│   ├── Repositories/    (SalesOrderRepository)
│   ├── Services/        (SalesOrder, AuditLog, Configuration, Status)
│   ├── Database/        (Factories, Interfaces, Initializers)
│   ├── Middleware/      (GlobalException, RequestLogging)
│   └── Configuration/Config.xml
└── SOLTIUS - Scheduler Add-On/          (.NET Framework 4.8)
    ├── Program.cs
    ├── SchedulerWindowsService.cs
    ├── Security/        (Encryption.cs)
    ├── Model/           (Model, PendingSalesOrder, SchedulerConfig)
    ├── Services/        (ConfigService, DatabaseService, ProfileSyncService,
    │                     SalesOrderSyncRunner, SapSyncService, SyncSchedulerEngine)
    └── UI/              (FormMain, FormLogin, FormPassword, FormSettingScheduler,
                          ManageProfile, FormChooseCF, FormCreate, UITheme, AccessPasswordStore)
```