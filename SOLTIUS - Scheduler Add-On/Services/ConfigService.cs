using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SOLTIUS_Scheduler_Add_On.UI;

namespace SOLTIUS_Scheduler_Add_On.Services
{
    public class ConfigService
    {
        private readonly string _configFilePath;
        private readonly string _defaultFilePath;

        public ConfigService()
        {
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AllConfigurations.xml");
            _defaultFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DefaultConfig.xml");
        }

        public string GetActiveProfileName()
        {
            if (!File.Exists(_defaultFilePath)) return null;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DefaultSettings));
                using (FileStream fs = new FileStream(_defaultFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var def = (DefaultSettings)serializer.Deserialize(fs);
                    return def?.DefaultProfileName;
                }
            }
            catch
            {
                return null;
            }
        }

        public AppConfig GetActiveConfiguration()
        {
            if (!File.Exists(_defaultFilePath) || !File.Exists(_configFilePath)) return null;

            try
            {
                string activeName = GetActiveProfileName();
                if (string.IsNullOrEmpty(activeName)) return null;

                XmlSerializer allSer = new XmlSerializer(typeof(ConfigContainer));
                using (FileStream fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var container = (ConfigContainer)allSer.Deserialize(fs);
                    return container.Configurations.FirstOrDefault(x => x.ProfileName == activeName);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Mencari profil berdasarkan nama di AllConfigurations.xml.
        /// Digunakan mode CLI (scheduler) untuk memilih profil via -PROFILE:name.
        /// </summary>
        public AppConfig GetConfigurationByName(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName) || !File.Exists(_configFilePath)) return null;

            try
            {
                XmlSerializer allSer = new XmlSerializer(typeof(ConfigContainer));
                using (FileStream fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var container = (ConfigContainer)allSer.Deserialize(fs);
                    return container.Configurations.FirstOrDefault(x =>
                        x.ProfileName.Equals(profileName.Trim(), StringComparison.OrdinalIgnoreCase));
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Membuat connection string staging (SQL Server) dari profil.
        /// Return null bila profil tidak ada atau tipe DB bukan SQL Server.
        /// </summary>
        public static string BuildStagingConnectionString(AppConfig config)
        {
            if (config == null) return null;
            if (!string.Equals(config.ExternalDBType, "SQLServer", StringComparison.OrdinalIgnoreCase)) return null;

            string server = config.ExternalDBServer;
            if (!string.IsNullOrWhiteSpace(config.ExternalDBPort) && config.ExternalDBPort != "0")
                server += "," + config.ExternalDBPort.Trim();

            return $"Server={server};Database={config.ExternalDBName};User Id={config.ExternalDBUser};Password={config.ExternalDBPass};TrustServerCertificate=True;";
        }

        /// <summary>
        /// Mengirim konfigurasi profil aktif ke Web API (POST /api/ProfileSync).
        /// Bila gagal, payload masuk ke antrian retry. Throw bila tidak bisa dikirim.
        /// </summary>
        public Task SendActiveConfigurationToWebAsync(string fallbackWebApiUrl = null)
        {
            AppConfig config = GetActiveConfiguration();
            if (config == null)
                throw new Exception("Tidak ada profil aktif yang dapat dikirim ke Web API.");

            string webApiUrl = (config.WebApiUrl ?? "").Trim();

            // Fallback hanya dipakai bila profil kosong DAN URL fallback terlihat valid
            if (string.IsNullOrEmpty(webApiUrl) && !string.IsNullOrEmpty(fallbackWebApiUrl))
            {
                string fallback = fallbackWebApiUrl.Trim();
                if (Uri.TryCreate(fallback, UriKind.Absolute, out _) &&
                    !fallback.Contains("your-api-endpoint") &&
                    !fallback.Contains("example.com"))
                {
                    webApiUrl = fallback;
                }
            }

            if (string.IsNullOrEmpty(webApiUrl))
                throw new Exception("Profil aktif belum memiliki 'Web API URL'.\n\n" +
                    "Buka Configuration -> Manage Profile, isi field 'Web API URL' " +
                    "(contoh: http://localhost:5006), lalu klik Update.");

            config.WebApiUrl = webApiUrl;

            if (!ProfileSyncService.SendProfileSync(config, out string error))
                throw new Exception($"Gagal mengirim konfigurasi ke Web API: {error}");

            return Task.CompletedTask;
        }
    }
}