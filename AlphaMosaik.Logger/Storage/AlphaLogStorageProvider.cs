using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Configuration;
using System.IO;
using Microsoft.SharePoint.Administration;

namespace AlphaMosaik.Logger.Storage
{
    public class AlphaLogStorageProvider : BaseStorageProvider, IDisposable
    {
        public string LogFilePath { get; set; }

        private FileInfo traceLogFileInfo;
        private StreamWriter fileStream;

        public AlphaLogStorageProvider()
        {
        }

        public override void AddEntry(LogEntry entry)
        {
            fileStream.WriteLine(entry.ToString());
            fileStream.Flush();
        }

        public override LogEntry GetEntry(int idx)
        {
            throw new NotImplementedException();
        }

        public override List<LogEntry> GetEntries(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public override List<LogEntry> GetAllEntries()
        {
            throw new NotImplementedException();
        }

        public override void LoadSettings()
        {
            string enabledAsString = ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProviderIsEnabled].Value;
            Enabled = bool.Parse(enabledAsString);

            if (Enabled)
            {
                LogFilePath = ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProvideFilePath].Value;

                Initialization();
            }
        }

        public override void SaveSettings()
        {
            ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProviderIsEnabled].Value = Enabled.ToString();
            ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProvideFilePath].Value = LogFilePath;
            ConfigurationManager.SaveSettings();
        }

        public void Dispose()
        {
            CloseResources();
        }

        protected void Initialization()
        {
            traceLogFileInfo = new FileInfo(LogFilePath);
            if (!traceLogFileInfo.Exists)
            {
                traceLogFileInfo.Create();
            }

            fileStream = new StreamWriter(traceLogFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
        }

        private void CloseResources()
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }
    }
}
