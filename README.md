# SOLTIUS Add-On — Scheduler + Web API (Staging 2026 Auth)

Template integrasi **SAP Business One** dengan aplikasi third-party: aplikasi eksternal mengirim data via **Web API (.NET 8)** ke **staging DB**, lalu **Scheduler Add-On (.NET Framework 4.8)** menyinkronkannya ke SAP B1 lewat **DI API (COM)**.

```
External App ──POST──▶ Web API (.NET 8) ──INSERT──▶ Staging DB
                           │
                    BackgroundService (audit log)
                           │
                    Scheduler (WinForms / Windows Service)
                           │
                    SAP DI API (COM) ──▶ SAP Business One
```

## 🗂️ Struktur Solusi

| Project | Framework | Peran |
|---------|-----------|-------|
| `SOLTIUS - Web API Add-On` | .NET 8 (ASP.NET Core) | Terima data dari aplikasi eksternal, auth OAuth2+JWT, tulis ke staging DB, audit log |
| `SOLTIUS - Scheduler Add-On` | .NET Framework 4.8 | Polling data pending dari staging, buat dokumen di SAP via DI API, retry & dead-letter |

## ✨ Fitur Utama

- **Autentikasi** OAuth2 `client_credentials` + JWT Bearer + refresh token (7 hari)
- **Endpoint**: `POST /api/SalesOrder`, `POST /api/ProfileSync` (XML), `GET /api/Status`, `GET /health`
- **Staging DB**: MySQL / SQL Server (dipilih via `Configuration/Config.xml`)
- **Audit log**: `Channel<T>` + `BackgroundService` → tabel `api_logs` & `sync_logs`
- **Scheduler**: mode Interval/Realtime, dual-mode (UI WinForms / Windows Service), retry max 5x → dead-letter
- **Security**: rate limiting (100 req/menit/IP), limit body 1MB, startup validation JWT, Swagger hanya di Development

## 🚀 Menjalankan

### Web API
```bash
set JWT_SIGNING_KEY=random-key-min-32-karakter
set CLIENT_SECRET=secret-client-scheduler
cd "SOLTIUS - Web API Add-On"
dotnet run --urls http://localhost:5006
```
Swagger: `http://localhost:5006/swagger` (hanya Development)

### Scheduler
Buka `SOLTIUS - Scheduler Add-On.sln` di **Visual Studio 2022** (butuh COM reference `SAPbobsCOM`), build, jalankan → isi profil → Start Scheduler.

## 📚 Dokumentasi

Dokumentasi lengkap ada di folder [`Documentation/`](./Documentation/):
- `01_KondisiSaatIni.md` — cara kerja program & saran pengembangan
- `CHANGELOG.md` — log perubahan/update

## 📝 Catatan

- **Kredensial di repo ini adalah placeholder/contoh** — ganti dengan env var (`JWT_SIGNING_KEY`, `CLIENT_SECRET`) dan jangan commit secret asli.
- Scheduler harus di-build dari Visual Studio (COM reference `SAPbobsCOM`).
- Untuk akses dari PC luar: bind ke `0.0.0.0` + buka port di Windows Firewall + wajib HTTPS di produksi.
