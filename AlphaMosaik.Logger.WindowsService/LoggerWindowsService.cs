using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using AlphaMosaik.Logger.Service;
using System.IO;

namespace AlphaMosaik.Logger.WindowsService
{
    partial class LoggerWindowsService : ServiceBase
    {
        private ServiceHost host;

        public LoggerWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Debugger.Break();
            try
            {
                LoggerService service = new LoggerService(EventLog);

                host = new ServiceHost(service);
                host.Open();
            }
            catch (UnauthorizedAccessException ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
                ExitCode = 5; // Access Denied
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
                ExitCode = 1; // A modifier correspond à Incorrect function
                Stop();
            }
        }

        protected override void OnStop()
        {
            if (host != null)
            {
                host.Close();
                host = null;
            }
        }
    }
}
