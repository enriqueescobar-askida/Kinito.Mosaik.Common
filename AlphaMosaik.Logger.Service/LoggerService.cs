using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AlphaMosaik.Logger.Configuration;
using AlphaMosaik.Logger.Diagnostics;
using System.ServiceModel;

namespace AlphaMosaik.Logger.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LoggerService : ILogger
    {
        public EventLog LoggerEventLog { get; set; }

        private Logger logger;

        public LoggerService(EventLog loggerEventLog)
        {
            if(logger == null)
            {
                logger = new Logger(loggerEventLog);
            }
            LoggerEventLog = loggerEventLog;
        }

        public void LogEntry(LogEntry entry)
        {
            logger.WriteEntry((AlphaMosaik.Logger.LogEntry)entry);
        }

        public void LogEntry(string message)
        {
            AlphaMosaik.Logger.LogEntry entry = new AlphaMosaik.Logger.LogEntry(message, LogEntryLevel.Information);
            logger.WriteEntry(entry);
        }

        public void LogEntry(string message, LogEntryLevel level)
        {
            AlphaMosaik.Logger.LogEntry entry = new AlphaMosaik.Logger.LogEntry(message, level);
            logger.WriteEntry(entry);
        }

        public void LogEntry(Exception exception)
        {
            string message = "Exception : " + exception.Message;
            message += " - StackTrace : " + exception.StackTrace;

            AlphaMosaik.Logger.LogEntry entry = new AlphaMosaik.Logger.LogEntry(message, LogEntryLevel.Error);
            logger.WriteEntry(entry);
        }
    }
}
