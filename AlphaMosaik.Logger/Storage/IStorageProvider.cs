using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Configuration;
using Microsoft.SharePoint.Administration;
using System.Runtime.Serialization;

namespace AlphaMosaik.Logger.Storage
{
    public interface IStorageProvider
    {
        string Name
        {
            get; set;
        }

        bool Enabled
        {
            get; set;
        }

        ProviderDefinitionElement Definition { get; set; }

        event EventHandler EnabledChanged;

        void AddEntry(LogEntry entry);
        LogEntry GetEntry(int idx);
        List<LogEntry> GetEntries(DateTime from, DateTime to);
        List<LogEntry> GetAllEntries();

        void LoadSettings();
        void SaveSettings();
    }
}
