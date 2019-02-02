// -----------------------------------------------------------------------------------public public public ---------------------------------
// <copyright file="EventLogPerformanceWriter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the EventLogPerformanceWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace Alphamosaik.Common.Library.Performance
{
    public class EventLogPerformanceWriter : IPerformanceWriter
    {
        public void Write(PerformanceEntry performance)
        {
            EventLog.WriteEntry(performance.Title, performance.Message, GetEventLogEntryType(performance));
        }

        private static EventLogEntryType GetEventLogEntryType(PerformanceEntry performance)
        {
            switch (performance.Severity)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    return EventLogEntryType.Error;
                case TraceEventType.Warning:
                    return EventLogEntryType.Warning;
                default:
                    return EventLogEntryType.Information;
            }
        }
    }
}