using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Configuration;
using AlphaMosaik.Logger.Storage.LoggerDataSetTableAdapters;
using System.Data.SqlClient;
using Microsoft.SharePoint.Administration;

namespace AlphaMosaik.Logger.Storage
{
    public class SqlLogStorageProvider : BaseStorageProvider
    {
        public const string ConnectionStringName = "AlphaMosaikLoggerSqlLogStorageProvider";

        public string ConnectionString { get; set; }

        public override void AddEntry(LogEntry entry)
        {
            SqlConnection connection = new SqlConnection(this.ConnectionString);
            LogEntriesTableAdapter tableAdapter = new LogEntriesTableAdapter();
            tableAdapter.Connection = connection;

            tableAdapter.Insert(entry.Id, entry.Message, (byte)entry.Level, entry.TimeStamp);
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

        public override void LoadSettings(SPWebApplication webApp)
        {
            throw new NotImplementedException();
            //string enabledAsString;
            //if (ConfManager.TryGetSetting(webApp, Properties.Settings.Default.Settings_Storage_SqlProvider_Enabled, out enabledAsString))
            //{
            //    bool enabled;

            //    if (bool.TryParse(enabledAsString, out enabled))
            //    {
            //        Enabled = enabled;
            //    }
            //    else
            //    {
            //        Enabled = false; // Valeur par défaut
            //    }
            //}
            //else
            //{
            //    Enabled = false; // Valeur par défaut
            //}

            //if (Enabled)
            //{
            //    try
            //    {
            //        ConnectionString = ConfManager.GetConnectionString(webApp, ConnectionStringName).ConnectionString;
            //    }
            //    catch(ArgumentException)
            //    {
            //        Enabled = false;
            //    }
            //}
        }

        public override void SaveSettings(SPWebApplication webApp)
        {
            throw new NotImplementedException();
            //ConfManager.SetSetting(webApp, Properties.Settings.Default.Settings_Storage_SqlProvider_Enabled, this.Enabled.ToString());
            //ConfManager.SaveSettings(webApp);
        }
    }
}
