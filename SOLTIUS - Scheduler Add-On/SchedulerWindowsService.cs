using System;
using System.ServiceProcess;
using SOLTIUS_Scheduler_Add_On.Model;
using SOLTIUS_Scheduler_Add_On.Services;

namespace SOLTIUS_Scheduler_Add_On
{
    /// <summary>
    /// Windows Service wrapper. Saat service di-start oleh Service Control Manager,
    /// este instance membaca SchedulerSettings.xml dan menstart SyncSchedulerEngine.
    /// Dipilih via Program.Main ketika Environment.UserInteractive = false.
    /// Nama service disinkronkan dari SchedulerConfig.ServiceName.
    /// </summary>
    public class SchedulerWindowsService : ServiceBase
    {
        private SyncSchedulerEngine _engine;

        public SchedulerWindowsService()
        {
            ServiceName = SchedulerConfig.Load().ServiceName;
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _engine = new SyncSchedulerEngine();
                _engine.LogMessage += m => System.Diagnostics.EventLog.WriteEntry(ServiceName, m, System.Diagnostics.EventLogEntryType.Information);
                SchedulerConfig config = SchedulerConfig.Load();
                _engine.Start(config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry(ServiceName, "OnStart gagal: " + ex.Message,
                    System.Diagnostics.EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _engine?.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry(ServiceName, "OnStop: " + ex.Message,
                    System.Diagnostics.EventLogEntryType.Warning);
            }
        }
    }
}