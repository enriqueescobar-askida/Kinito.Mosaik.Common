// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatisticsEntry.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StatisticsEntry type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Alphamosaik.Common.Library.Performance;

namespace Alphamosaik.Common.Library.Statistics
{
    /// <summary>
    /// Represents a log message.  Contains the common properties that are required for all log messages.
    /// </summary>
    [XmlRoot("logEntry")]
    [Serializable]
    public class StatisticsEntry : PerformanceEntry
    {
        private const string StatisticCategory = "Statistics";

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsEntry"/> class. 
        /// Initialize a new instance of a <see cref="StatisticsEntry"/> class.
        /// </summary>
        public StatisticsEntry()
        {
            BuildCategoriesCollection(StatisticCategory);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsEntry"/> class. 
        /// Create a new instance of <see cref="StatisticsEntry"/> with a full set of constructor parameters
        /// </summary>
        /// <param name="message">
        /// Message body to log.  Value from ToString() method from message object.
        /// </param>
        /// <param name="category">
        /// Category name used to route the log entry to a one or more trace listeners.
        /// </param>
        /// <param name="priority">
        /// Only messages must be above the minimum priority are processed.
        /// </param>
        /// <param name="eventId">
        /// Event number or identifier.
        /// </param>
        /// <param name="severity">
        /// Log entry severity as a <see cref="PerformanceEntry.Severity"/> enumeration. (Unspecified, Information, Warning or Error).
        /// </param>
        /// <param name="title">
        /// Additional description of the log entry message.
        /// </param>
        /// <param name="properties">
        /// Dictionary of key/value pairs to record.
        /// </param>
        public StatisticsEntry(object message, string category, int priority, int eventId,
        TraceEventType severity, string title, IDictionary<string, object> properties)
            : base(message, BuildCategoriesCollection(category), priority, eventId, severity, title, properties)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsEntry"/> class. 
        /// Create a new instance of <see cref="StatisticsEntry"/> with a full set of constructor parameters
        /// </summary>
        /// <param name="message">
        /// Message body to log.  Value from ToString() method from message object.
        /// </param>
        /// <param name="categories">
        /// Collection of category names used to route the log entry to a one or more sinks.
        /// </param>
        /// <param name="priority">
        /// Only messages must be above the minimum priority are processed.
        /// </param>
        /// <param name="eventId">
        /// Event number or identifier.
        /// </param>
        /// <param name="severity">
        /// Log entry severity as a <see cref="PerformanceEntry.Severity"/> enumeration. (Unspecified, Information, Warning or Error).
        /// </param>
        /// <param name="title">
        /// Additional description of the log entry message.
        /// </param>
        /// <param name="properties">
        /// Dictionary of key/value pairs to record.
        /// </param>
        public StatisticsEntry(object message, ICollection<string> categories, int priority, int eventId,
        TraceEventType severity, string title, IDictionary<string, object> properties)
            : base(message, categories, priority, eventId, severity, title, properties)
        {
        }
    }
}
