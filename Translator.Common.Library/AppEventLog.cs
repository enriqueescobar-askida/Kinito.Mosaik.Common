// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppEventLog.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the AppEventLog type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace Translator.Common.Library
{
    public class AppEventLog
    {
        /// <summary>Initializes a new instance of the <see cref="AppEventLog"/> class.</summary>
        /// <param name="source">the title of the log</param>
        /// <param name="log">the log section</param>
        /// <param name="logEvent">the contents of the log</param>
        public AppEventLog(string source, string log, string logEvent)
        {
            Event = logEvent;
            Log = log;
            Source = source;
        }

        public AppEventLog(string logEvent)
        {
            Event = logEvent;
            Log = "Application";
            Source = "AlphamosaikLib";
        }

        public AppEventLog()
        {
        }

        public string Source { get; set; }

        public string Log { get; set; }

        public string Event { get; set; }

        /// <summary>
        /// write event
        /// </summary>
        public void WriteToLog()
        {
            if (!EventLog.SourceExists(Source))
                EventLog.CreateEventSource(Source, Log);

            EventLog.WriteEntry(Source, Event, EventLogEntryType.Warning);
        }
    }
}