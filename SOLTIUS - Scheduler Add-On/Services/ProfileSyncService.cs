using SOLTIUS_Scheduler_Add_On.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SOLTIUS_Scheduler_Add_On.Services
{
    /// <summary>
    /// Mengirim konfigurasi profil aktif ke Web API (POST /api/ProfileSync).
    /// Jika pengiriman gagal, payload disimpan ke antrian retry (PendingProfileSync.xml)
    /// dan dicoba kirim ulang secara otomatis pada kesempatan berikutnya.
    /// </summary>
    public class ProfileSyncService
    {
        private static readonly string QueuePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PendingProfileSync.xml");

        public static string BuildExternalDatabaseXml(AppConfig config)
        {
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Configuration",
                    new XElement("Environment", string.IsNullOrWhiteSpace(config.Environment) ? "Unknown" : config.Environment),
                    new XElement("ExternalDatabase",
                        new XElement("ExternalDBType", config.ExternalDBType ?? ""),
                        new XElement("ExternalDBServer", config.ExternalDBServer ?? ""),
                        new XElement("ExternalDBPort", string.IsNullOrWhiteSpace(config.ExternalDBPort) ? "0" : config.ExternalDBPort),
                        new XElement("ExternalDBName", config.ExternalDBName ?? ""),
                        new XElement("ExternalDBUser", config.ExternalDBUser ?? ""),
                        new XElement("ExternalDBPass", config.ExternalDBPass ?? "")
                    )
                )
            );

            using (StringWriterUtf8 sw = new StringWriterUtf8())
            {
                doc.Save(sw);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Mengirim profil aktif ke Web API. Sebelum mengirim, antrian retry lama dicoba flush lebih dulu.
        /// </summary>
        public static void SyncActiveProfile(AppConfig config)
        {
            if (config == null) return;

            FlushPendingSyncs();

            string webApiUrl = (config.WebApiUrl ?? "").Trim();
            if (string.IsNullOrEmpty(webApiUrl))
            {
                MessageBox.Show(
                    $"Profil '{config.ProfileName}' tidak memiliki 'Web API URL'.\n\n" +
                    "Isi field tersebut lewat menu Configuration -> Manage Profile terlebih dahulu.",
                    "Profile Sync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (SendProfileSync(config, out string error))
            {
                MessageBox.Show(
                    $"Konfigurasi profil '{config.ProfileName}' berhasil dikirim ke Web API.",
                    "Profile Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"Gagal mengirim konfigurasi profil ke Web API.\n\nDetail: {error}\n\n" +
                    "Payload disimpan ke antrian retry dan akan dicoba kirim ulang secara otomatis.",
                    "Profile Sync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Mengirim konfigurasi profil ke Web API tanpa dialog UI.
        /// Bila gagal, payload otomatis masuk antrian retry.
        /// </summary>
        /// <returns>true bila berhasil; false bila gagal (error berisi detail).</returns>
        public static bool SendProfileSync(AppConfig config, out string error)
        {
            error = null;
            if (config == null)
            {
                error = "Profil tidak ditemukan.";
                return false;
            }

            string webApiUrl = (config.WebApiUrl ?? "").Trim();
            if (string.IsNullOrEmpty(webApiUrl))
            {
                error = "Web API URL belum diisi pada profil.";
                return false;
            }

            string xml = BuildExternalDatabaseXml(config);

            if (TryPostProfileSync(webApiUrl, xml, out error))
                return true;

            Enqueue(config, xml);
            return false;
        }

        /// <summary>
        /// Mencoba mengirim ulang semua payload yang gagal di antrian.
        /// </summary>
        /// <returns>Jumlah item yang MASIH gagal setelah percobaan ini.</returns>
        public static int FlushPendingSyncs()
        {
            try
            {
                PendingSyncContainer container = LoadQueue();
                if (container.Items.Count == 0) return 0;

                List<PendingProfileSync> remaining = new List<PendingProfileSync>();
                foreach (PendingProfileSync item in container.Items)
                {
                    // EXPIRY: buang item yang sudah mengantri lebih dari 7 hari.
                    // Payload lama kemungkinan besar sudah basi (konfigurasi profil sudah berubah).
                    if (item.CreatedAt < DateTime.Now.AddDays(-7))
                        continue;

                    if (!TryPostProfileSync(item.WebApiUrl, item.XmlPayload, out _))
                        remaining.Add(item);
                }

                container.Items = remaining;
                SaveQueue(container);
                return remaining.Count;
            }
            catch
            {
                return -1;
            }
        }

        private static bool TryPostProfileSync(string webApiUrl, string xml, out string error)
        {
            error = null;
            try
            {
                string url = webApiUrl.TrimEnd('/') + "/api/ProfileSync";

                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(90) })
                using (var content = new StringContent(xml, Encoding.UTF8, "application/xml"))
                {
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        // Baca body response agar detail error dari server ikut tampil,
                        // bukan hanya "HTTP 500 Internal Server Error".
                        string responseBody = "";
                        try
                        {
                            responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }
                        catch { }

                        if (!string.IsNullOrWhiteSpace(responseBody))
                            error = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Detail: {responseBody}";
                        else
                            error = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";

                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || ex is OperationCanceledException)
                    error = "Waktu tunggu ke Web API habis (timeout 90 detik). Server membutuhkan waktu lebih lama dari perkiraan.";
                else if (ex.InnerException != null)
                    error = $"{ex.Message} -> {ex.InnerException.Message}";
                else
                    error = ex.Message;
                return false;
            }
        }

        #region Retry Queue (PendingProfileSync.xml)

        private static PendingSyncContainer LoadQueue()
        {
            if (!File.Exists(QueuePath)) return new PendingSyncContainer();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PendingSyncContainer));
                using (FileStream fs = new FileStream(QueuePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return (PendingSyncContainer)serializer.Deserialize(fs) ?? new PendingSyncContainer();
                }
            }
            catch
            {
                return new PendingSyncContainer();
            }
        }

        private static void Enqueue(AppConfig config, string xml)
        {
            try
            {
                PendingSyncContainer container = LoadQueue();

                // DEDUP: jangan tambah payload yang identik (WebApiUrl + XML) sudah ada di antrian
                bool alreadyQueued = container.Items.Any(item =>
                    string.Equals(item.WebApiUrl, (config.WebApiUrl ?? "").Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.XmlPayload, xml, StringComparison.Ordinal));

                if (alreadyQueued) return;

                container.Items.Add(new PendingProfileSync
                {
                    ProfileName = config.ProfileName,
                    WebApiUrl = (config.WebApiUrl ?? "").Trim(),
                    XmlPayload = xml,
                    CreatedAt = DateTime.Now
                });

                SaveQueue(container);
            }
            catch
            {
                // Abaikan bila antrian gagal disimpan; tidak memblokir proses utama.
            }
        }

        private static void SaveQueue(PendingSyncContainer container)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PendingSyncContainer));
            using (StreamWriter writer = new StreamWriter(QueuePath))
            {
                serializer.Serialize(writer, container);
            }
        }

        #endregion
    }

    /// <summary>
    /// StringWriter dengan encoding UTF-8 agar deklarasi XML tidak hilang.
    /// </summary>
    internal class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    [Serializable]
    public class PendingProfileSync
    {
        public string ProfileName { get; set; }
        public string WebApiUrl { get; set; }
        public string XmlPayload { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Serializable]
    [XmlRoot("PendingProfileSyncs")]
    public class PendingSyncContainer
    {
        [XmlElement("Pending")]
        public List<PendingProfileSync> Items { get; set; } = new List<PendingProfileSync>();
    }
}