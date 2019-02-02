using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Configuration;
using AlphaMosaik.Logger.Providers.AlphaSqlLogProviderDataSetTableAdapters;
using AlphaMosaik.Logger.Storage;
using System.Data.SqlClient;
using System.Diagnostics;

namespace AlphaMosaik.Logger.Providers
{
    public class AlphaSqlLogProvider : BaseStorageProvider, IDisposable
    {
        public string ConnectionString { get; set; }

        private SqlConnection sqlConnection;
        private LogEntriesTableAdapter tableAdapter;
        private bool disposed;

        public override void AddEntry(LogEntry entry)
        {
            tableAdapter.Insert(entry.Id, entry.Message, (int)entry.Level, entry.TimeStamp);
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
                string enabledAsString = ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaSqlLogProviderIsEnabled].Value;
                Enabled = bool.Parse(enabledAsString);
            }
            catch (Exception)
            {
                LoggerEventLog.WriteEntry("Configuration error for the provider " + Name + ". The provider will be disabled.", EventLogEntryType.Error);
                Enabled = false;
                return;
            }

            if(Enabled)
            {
                ConnectionString = ConfigurationManager.ConnectionStrings[Properties.Settings.Default.AlphaSqlLogProviderConnectionStringName].ConnectionString;
                sqlConnection = new SqlConnection(ConnectionString);
                tableAdapter = new LogEntriesTableAdapter();
                tableAdapter.Connection = sqlConnection;
            }
        }

        public override void SaveSettings()
        {
            ConfigurationManager.Logger.ProviderSettings[Properties.Settings.Default.KeyAlphaSqlLogProviderIsEnabled].Value = Enabled.ToString();
            ConfigurationManager.SaveSettings();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this); // Evite de rappeler le destructeur
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    CloseResources();
                }
                disposed = true;
            }
        }

        private void CloseResources()
        {
            if(sqlConnection != null)
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
                sqlConnection = null;
            }
        }

        ~AlphaSqlLogProvider()
        {
            Dispose(false);
        }
    }
}
