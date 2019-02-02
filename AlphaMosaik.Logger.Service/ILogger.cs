using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace AlphaMosaik.Logger.Service
{
    [ServiceContract]
    public interface ILogger
    {
        [OperationContract(Name = "LogEntry")]
        void LogEntry(LogEntry entry);

        [OperationContract(Name = "LogMessage")]
        void LogEntry(string message, LogEntryLevel level);

        [OperationContract(Name = "LogException")]
        void LogEntry(Exception exception);
    }
    [DataContract]
    public class LogEntry
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public LogEntryLevel Level { get; set; }

        public static explicit operator AlphaMosaik.Logger.LogEntry(LogEntry serviceEntry)
        {
            AlphaMosaik.Logger.LogEntry coreEntry = new AlphaMosaik.Logger.LogEntry(serviceEntry.Message, serviceEntry.Level);
            return coreEntry;
        }
    }
}
