using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Storage;
using System.Diagnostics;

namespace AlphaMosaik.Logger.Providers
{
    public class AlphaEventLogProvider : BaseStorageProvider
    {
        private EventLog alphaEventLog;

        public override void AddEntry(LogEntry entry)
        {
            alphaEventLog.WriteEntry(entry.Message);
        }

        public override LogEntry GetEntry(int idx)
        {
            EventLogEntry eventLogEntry = alphaEventLog.Entries[idx];
            LogEntry logEntry = new LogEntry(eventLogEntry.Message);

            return logEntry;
        }

        public override List<LogEntry> GetEntries(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public override List<LogEntry> GetAllEntries()
        {
            List<LogEntry> allEntries;
            lock(alphaEventLog)
            {
                allEntries = new List<LogEntry>(alphaEventLog.Entries.Count);

                foreach (EventLogEntry eventLogEntry in alphaEventLog.Entries)
                {
                    LogEntry logEntry = new LogEntry(eventLogEntry.Message);
                    allEntries.Add(logEntry);
                }
            }
            return allEntries;
        }

        public override void LoadSettings()
        {
            try
            {
                string enabledAsString = ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaEventLogProviderIsEnabled].Value;
                Enabled = bool.Parse(enabledAsString);
            }
            catch (Exception) // L'accès incorrect à une clé de configuration lance ArgumentException. Le bool.Parse peut lancer ArgumentNullException ou FormatException
            {
                LoggerEventLog.WriteEntry("Configuration error for the provider " + Name + ". The provider will be disabled.", EventLogEntryType.Error);
                Enabled = false;
                return;
            }

            if(Enabled)
            {
                Initialization();
            }
        }

        public override void SaveSettings()
        {
            ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaEventLogProviderIsEnabled].Value = Enabled.ToString();
            ConfigurationManager.SaveSettings();
        }

        private void Initialization()
        {
            alphaEventLog = new EventLog();
            alphaEventLog.Source = "AlphamosaikLoggerProvider"; // Les propriétés Source et Log doivent correspondre à celles
            alphaEventLog.Log = "AlphaMosaik";                  // définies lors de l'installation du service. Voir AlphaMosaik.Logger.WindowsService.ProjectInstaller.cs
        }
    }
}
