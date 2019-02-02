using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Storage;
using Microsoft.SharePoint;
using AlphaMosaik.Logger.Diagnostics;
using AlphaMosaik.Logger.Configuration;
using System.Diagnostics;

namespace AlphaMosaik.Logger
{
    /// <summary>
    /// Permet de journaliser un évènement
    /// </summary>
    public class Logger : IDisposable
    {
        public StorageManager StorageManager { get; internal set; }
        public ConfManager ConfigurationManager { get; private set; }
        public EventLog LoggerEventLog { get; set; }

        private LoggerDispatcher Dispatcher;
        private bool disposed;

        public Logger(EventLog eventLog)
        {
            LoggerEventLog = eventLog;

            if(ConfigurationManager == null)
            {
                ConfigurationManager = new ConfManager(this);
            }
            if(StorageManager == null)
            {
                StorageManager = new StorageManager(this);
            }
            if(Dispatcher == null)
            {
                Dispatcher = new LoggerDispatcher(this);
                Dispatcher.Start();
            }
        }

        public void WriteEntry(LogEntry entry)
        {
            if(entry.Level >= ConfigurationManager.Logger.LogLevel) // Permet de logguer uniquement les évènements d'un certain niveau
            {
                Dispatcher.SubmitEntry(entry);
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void Reload()
        {
            Dispatcher.Pause(); // Opération synchrone, retourne lorsque le traitement en cours est terminé.
            StorageManager.Dispose();
            StorageManager = new StorageManager(this);
            Dispatcher.Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    StorageManager.Dispose();
                }
                disposed = true;
            }
        }
    }
}
