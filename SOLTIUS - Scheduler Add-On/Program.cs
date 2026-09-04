using SOLTIUS_Scheduler_Add_On.UI;
using System;
using System.IO; // Diperlukan untuk mengecek file
using System.ServiceProcess;
using System.Windows.Forms;

namespace SOLTIUS_Scheduler_Add_On
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Dual-mode: bila dijalankan oleh Service Control Manager (non-interactive),
        /// jalankan sebagai Windows Service. Bila interactive, jalankan UI WinForms.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                // Mode Windows Service (SCM) — tanpa UI
                ServiceBase.Run(new SchedulerWindowsService());
                return;
            }

            // =========================================================
            // Mode UI (interactive)
            // =========================================================
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Tentukan path file config (sesuaikan dengan folder yang kita buat di FormPassword)
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Security", "AccessConfig.xml");

            // 2. Cek apakah file sudah ada
            if (!File.Exists(configPath))
            {
                // Jika TIDAK ADA, buka FormPassword untuk setup awal
                MessageBox.Show("Access Tidak ditemukan, Silakan buat password baru.",
                                "Setup Awal", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Application.Run(new FormPassword());

                // Setelah FormPassword ditutup, kita cek lagi. 
                // Jika user menutup form tanpa menyimpan, kita matikan aplikasi.
                if (!File.Exists(configPath))
                {
                    return; // Keluar dari program
                }
            }

            // 3. Jika file ADA (atau baru saja dibuat), lanjut ke FormLogin
            FormLogin frm = new FormLogin();
            Application.Run(frm);
        }
    }
}