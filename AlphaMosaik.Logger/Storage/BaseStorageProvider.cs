using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Configuration;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;

namespace AlphaMosaik.Logger.Storage
{
    public abstract class BaseStorageProvider : IStorageProvider
    {
        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if(EnabledChanged != null)
                {
                    EnabledChanged(this, EventArgs.Empty);
                }
            }
        }

        public virtual string Name { get; set; }
        public ProviderDefinitionElement Definition { get; set; }
        public virtual event EventHandler EnabledChanged;
        public ConfManager ConfigurationManager { get; set; }
        public EventLog LoggerEventLog { get; set; }

        public BaseStorageProvider()
        {
            _enabled = false;
        }

        public abstract void AddEntry(LogEntry entry);
        public abstract LogEntry GetEntry(int idx);
        public abstract List<LogEntry> GetEntries(DateTime from, DateTime to);
        public abstract List<LogEntry> GetAllEntries();
        public abstract void LoadSettings();
        public abstract void SaveSettings();
    }
}
