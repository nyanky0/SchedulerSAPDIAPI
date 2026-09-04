using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Models.Configuration;
using SOLTIUS_Web_API_Add_On.Models.Status;
using SOLTIUS_Web_API_Add_On.Services.Configuration;

namespace SOLTIUS_Web_API_Add_On.Services.Status
{
    public class StatusService : IStatusService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        // Cache hasil status agar /api/Status tidak membuka koneksi DB setiap kali dipanggil.
        private static readonly object _cacheLock = new object();
        private static ApiStatus _cachedStatus;
        private static DateTime _cacheTimeUtc = DateTime.MinValue;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(10);

        public StatusService(
            IConfigurationService configurationService,
            IDatabaseConnectionFactory connectionFactory)
        {
            _configurationService = configurationService;
            _connectionFactory = connectionFactory;
        }

        public Task<ApiStatus> GetStatusAsync()
        {
            lock (_cacheLock)
            {
                // Return cache kalau masih segar
                if (_cachedStatus != null && DateTime.UtcNow - _cacheTimeUtc < CacheTtl)
                {
                    return Task.FromResult(_cachedStatus);
                }

                _cachedStatus = BuildStatus();
                _cacheTimeUtc = DateTime.UtcNow;
                return Task.FromResult(_cachedStatus);
            }
        }

        private ApiStatus BuildStatus()
        {
            if (!_configurationService.ConfigExists())
            {
                return new ApiStatus
                {
                    Configured = false,
                    DatabaseConnection = false,
                    Status = "Configuration required"
                };
            }

            DBConfig config = _configurationService.GetDatabaseConfig();

            try
            {
                using var connection =
                    _connectionFactory.CreateConnection(config);

                connection.Open();

                return new ApiStatus
                {
                    Configured = true,
                    DatabaseConnection = true,
                    Status = "Ready"
                };
            }
            catch
            {
                return new ApiStatus
                {
                    Configured = true,
                    DatabaseConnection = false,
                    Status = "Database unavailable"
                };
            }
        }
    }
}
