using SOLTIUS_Scheduler_Add_On.Security;
using System;
using System.Text;

namespace SOLTIUS_Scheduler_Add_On.UI
{
    /// <summary>
    /// Penyimpanan password akses aplikasi (Security\AccessConfig.xml).
    /// Format baru: AES (kelas Encryption lokal).
    /// Format lama: Base64 biasa — masih didukung untuk migrasi, dan otomatis
    /// ditulis ulang ke AES saat form password disimpan.
    /// </summary>
    public static class AccessPasswordStore
    {
        private static readonly Encryption _encryption = new Encryption();

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            return _encryption.encrypt(plainText);
        }

        public static string Decrypt(string stored)
        {
            if (string.IsNullOrEmpty(stored)) return "";

            // Coba AES dulu (format baru)
            try
            {
                return _encryption.decrypt(stored);
            }
            catch
            {
                // Gagal = kemungkinan format lama (Base64). Fallback decode.
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(stored));
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// True bila nilai tersimpan masih format lama (Base64), perlu dimigrasi ke AES.
        /// </summary>
        public static bool IsLegacyFormat(string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            try
            {
                _encryption.decrypt(stored);
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
