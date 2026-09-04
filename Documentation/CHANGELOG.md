# 📝 CHANGELOG — SOLTIUS Add-On (Staging 2026 Auth)

> Log perubahan/update project selama development.
> Format: **tanggal — deskripsi perubahan (project / file yang berubah)**.
> Baru = paling atas. Update file ini SETIAP ada perubahan di project.

---

## [Unreleased / Baseline]

### 2026-09-04 — Security: scrub kredensial dari repo public
- **Web API**: `appsettings.json` — kredensial DB asli diganti placeholder env var (`${STAGING_DB_SERVER}`, `${STAGING_DB_USER}`, `${LOG_DB_*}`) supaya aman di repo public.
- **Web API**: `Configuration/Config.xml` & root `Configuration/Config.xml` — isi kredensial dikosongkan (diisi otomatis via `POST /api/ProfileSync` dari Scheduler).
- **Docs**: `01_KondisiSaatIni.md` — hapus contoh string password plaintext dari teks.
- ⚠️ **PENTING**: kredensial lama (sudah pernah ke-push ke repo public) harus dianggap bocor → ganti password DB & rotate credential.

### 2026-09-04 — Fix: retry logic, quantity desimal, cleanup repo
- **Scheduler**: Perbaiki retry logic di `SalesOrderSyncRunner` — hapus `GetCurrentRetryCount()` yang selalu return 0 & `GetMaxRetryCount()` hardcoded; sekarang update status dulu lalu cek `IsRetryLimitExceeded` → dead-letter langsung di cycle yang sama.
- **Scheduler**: `SyncLogModel.Quantity` diubah `int` → `double` (di `Model/Model.cs`, `DatabaseService`, `FormMain`) — quantity desimal tidak ter-truncate lagi.
- **Scheduler**: Kolom `Quantity` di `TBL_SYNC_HISTORY` & `TBL_SYNC_ERROR` diubah `INT` → `DECIMAL(18,2)` (berlaku untuk tabel baru; tabel lama perlu ALTER manual).
- **Repo**: `build_verify/` (42MB DLL hasil build) di-ignore & dikeluarkan dari tracking.
- **Repo**: Hapus baris ignore `/SOLTIUS - Scheduler Add-On/UI/FormMain.cs` di `.gitignore` — FormMain.cs ikut ke-commit.
- **Docs**: Ditambahkan `README.md` di root.

### 2026-09-04 — Dokumentasi awal dibuat
- **Docs**: Dibuat folder `Documentation/` berisi:
  - `README.md` (daftar isi)
  - `01_KondisiSaatIni.md` (cara kerja program saat ini + saran kekurangan)
  - `CHANGELOG.md` (file ini)
- Tidak ada perubahan kode — murni dokumentasi kondisi baseline.

---

## Template entri baru (copy-paste di atas baris ini)

```markdown
### YYYY-MM-DD — <judul singkat perubahan>
- **<Web API | Scheduler | Docs>**: <deskripsi perubahan>
- **File**: <path file yang berubah>
- <catatan tambahan / alasan kalau perlu>
```
