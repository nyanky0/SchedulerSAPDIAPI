using System.Threading.Channels;

namespace SOLTIUS_Web_API_Add_On.Services.AuditLog
{
    public interface IAuditLogService
    {
        void LogApiRequest(string method, string path, int statusCode,
            string clientIp, string uid, string detail, int durationMs = 0);

        void LogSyncResult(string uid, string cardCode, string status,
            string sapDocEntry, string errorMessage);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly Channel<ApiLogEntry> _apiChannel;
        private readonly Channel<SyncLogEntry> _syncChannel;

        public AuditLogService(
            Channel<ApiLogEntry> apiChannel,
            Channel<SyncLogEntry> syncChannel)
        {
            _apiChannel = apiChannel;
            _syncChannel = syncChannel;
        }

        public void LogApiRequest(string method, string path, int statusCode,
            string clientIp, string uid, string detail, int durationMs = 0)
        {
            var entry = new ApiLogEntry
            {
                Ts = System.DateTime.UtcNow,
                Method = method ?? "",
                Path = path ?? "",
                Status = statusCode,
                ClientIp = clientIp ?? "",
                Uid = uid ?? "",
                Detail = detail ?? "",
                DurationMs = durationMs
            };

            _apiChannel.Writer.TryWrite(entry);
        }

        public void LogSyncResult(string uid, string cardCode, string status,
            string sapDocEntry, string errorMessage)
        {
            var entry = new SyncLogEntry
            {
                Ts = System.DateTime.UtcNow,
                Uid = uid ?? "",
                CardCode = cardCode ?? "",
                Status = status ?? "",
                DocEntry = sapDocEntry ?? "",
                ErrorMessage = errorMessage ?? ""
            };

            _syncChannel.Writer.TryWrite(entry);
        }
    }
}
