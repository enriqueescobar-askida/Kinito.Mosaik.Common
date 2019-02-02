using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AlphaMosaik.Logger.Storage;
using System.Diagnostics;

namespace AlphaMosaik.Logger.Providers
{
    public class AlphaTextLogProvider : BaseStorageProvider, IDisposable
    {
        public string LogFilePath { get; set; }
        public int MaxFileSize { get; set; }

        private FileInfo traceLogFileInfo;
        private StreamWriter fileStream;
        private int flushCounter;
        private bool disposed;

        public AlphaTextLogProvider()
        {
        }

        public override void AddEntry(LogEntry entry)
        {
            fileStream.WriteLine(entry.ToString());
            fileStream.Flush();

            if(flushCounter > 100)
            {
                RefreshFilePath();
                flushCounter = 0;
            }

            flushCounter++;
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
            try
            {
                string enabledAsString = ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProviderIsEnabled].Value;
                Enabled = bool.Parse(enabledAsString);
            }
            catch (Exception)
            {
                LoggerEventLog.WriteEntry("Configuration error for the provider " + Name + ". The provider will be disabled.", EventLogEntryType.Error);
                Enabled = false;
                return;
            }

            if (Enabled)
            {
                LogFilePath = ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProviderFilePath].Value;
                MaxFileSize = int.Parse(ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProviderMaxFileSize].Value);

                Initialization();
            }
        }

        public override void SaveSettings()
        {
            ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProviderIsEnabled].Value = Enabled.ToString();
            ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaTextLogProviderFilePath].Value = LogFilePath;
            ConfigurationManager.SaveSettings();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this); // Evite d'appeler le destructeur par la suite
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    CloseResources();
                }
                disposed = true;
            }
        }

        protected void Initialization()
        {
            traceLogFileInfo = new FileInfo(GenerateFilePath(LogFilePath));
            fileStream = new StreamWriter(traceLogFileInfo.Open(FileMode.Append, FileAccess.Write, FileShare.Read));
            fileStream.AutoFlush = false;
        }

        private void RefreshFilePath()
        {
            traceLogFileInfo.Refresh(); // Permet d'obtenir la dernière valeur de length

            if (traceLogFileInfo.Length >= (MaxFileSize * 1024))
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                    traceLogFileInfo = null;
                }
                traceLogFileInfo = new FileInfo(GenerateFilePath(LogFilePath));
                fileStream = new StreamWriter(traceLogFileInfo.Open(FileMode.Append, FileAccess.Write, FileShare.Read));
                fileStream.AutoFlush = false;
            }
        }

        private string GenerateFilePath(string filePath)
        {
            string DirectoryPath = filePath.Replace(Path.GetFileName(filePath), "");
            string generatedLogFilePath = DirectoryPath +
                Path.GetFileNameWithoutExtension(filePath) +
                DateTime.Now.ToString("-yyyyMMdd-HHmm") +
                Path.GetExtension(filePath); // C:\LogFilePath-19870201-1200.ext

            return generatedLogFilePath;
        }

        private void CloseResources()
        {
            if (fileStream != null)
            {
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        ~AlphaTextLogProvider()
        {
            Dispose(false);
        }
    }
}
