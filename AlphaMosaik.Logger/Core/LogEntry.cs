using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlphaMosaik.Logger
{
    public class LogEntry
    {
        public string Message { get; set; }
        public LogEntryLevel Level { get; set; }
        public bool StrictDateTimeEnabled { get; set; }
        public string ProductName { get; set; }

        private DateTime timeStamp;
        public DateTime TimeStamp
        {
            get
            {
                if(StrictDateTimeEnabled)
                {
                    timeStamp = DateTime.Now;
                }
                return timeStamp;
            }
            set
            {
                timeStamp = value;
            }
        }

        private readonly Guid id;
        public Guid Id
        {
            get
            {
                return id;
            }
        }

        public LogEntry()
        {
            this.TimeStamp = DateTime.Now;
            this.id = Guid.NewGuid();
            this.ProductName = Properties.Settings.Default.DefaultProductName;
        }

        public LogEntry(string message)
        {
            this.Message = message;
            this.Level = LogEntryLevel.Information;
            this.TimeStamp = DateTime.Now;
            this.id = Guid.NewGuid();
            this.ProductName = Properties.Settings.Default.DefaultProductName;
        }

        public LogEntry(string message, LogEntryLevel level)
        {
            this.Message = message;
            this.Level = level;
            this.TimeStamp = DateTime.Now;
            this.id = Guid.NewGuid();
            this.ProductName = Properties.Settings.Default.DefaultProductName;
        }

        public LogEntry(Exception exception)
        {
            this.Message = exception.Message;
            this.Level = LogEntryLevel.Error;
            this.TimeStamp = DateTime.Now;
            this.id = Guid.NewGuid();
            this.ProductName = Properties.Settings.Default.DefaultProductName;
        }

        public override string ToString()
        {
            string timeStamp = TimeStamp.ToString("yyyy/MM/dd - HH:mm:ss.ffff");
            return Id + " - " + timeStamp + " - " + Level + " - " + ProductName + " - " + Message;
        }
    }

    public enum LogEntryLevel
    {
        Debugging = 0,
        Trace = 100,
        Information = 200,
        Warning = 300,
        Error = 400
    }
}
