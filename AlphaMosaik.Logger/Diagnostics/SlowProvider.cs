using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Storage;
using System.Threading;

namespace AlphaMosaik.Logger.Diagnostics
{
    public class SlowProvider : BaseStorageProvider
    {
        public int SleepTime { get; set; }

        public override void AddEntry(LogEntry entry)
        {
            Thread.Sleep(SleepTime);
        }

        public override LogEntry GetEntry(int idx)
        {
            return null;
        }

        public override List<LogEntry> GetEntries(DateTime from, DateTime to)
        {
            return null;
        }

        public override List<LogEntry> GetAllEntries()
        {
            return null;
        }

        public override void LoadSettings()
        {
            string enabledAsString = ConfigurationManager.Logger.ProviderSettings["AlphaSlowLogProvider.IsEnabled"].Value;
            string sleepTimeAsString = ConfigurationManager.Logger.ProviderSettings["AlphaSlowLogProvider.SleepTime"].Value;
            Enabled = bool.Parse(enabledAsString);
            SleepTime = int.Parse(sleepTimeAsString);
        }

        public override void SaveSettings()
        {
            ConfigurationManager.Logger.ProviderSettings["AlphaSlowLogProvider.IsEnabled"].Value = Enabled.ToString();
            ConfigurationManager.Logger.ProviderSettings["AlphaSlowLogProvider.SleepTime"].Value = SleepTime.ToString();
        }
    }
}
