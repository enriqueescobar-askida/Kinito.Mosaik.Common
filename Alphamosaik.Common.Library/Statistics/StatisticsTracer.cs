// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatisticsTracer.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StatisticsTracer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Alphamosaik.Common.Library.Statistics
{
    /// <summary>
    /// Represents a statistics tracing class to log method entry/exit and duration.
    /// </summary>
    /// <remarks>
    /// <para>Lifetime of the StatisticsTracer object will determine the beginning and the end of
    /// the trace.  The trace message will include, method being traced, start time, end time 
    /// and duration.</para>
    /// <para>Since StatisticsTracer uses the logging block to log the trace message, you can include application
    /// data as part of your trace message. Configured items from call context will be logged as
    /// part of the message.</para>
    /// <para>Trace message will be logged to the log category with the same name as the tracer operation name.
    /// You must configure the operation categories, or the catch-all categories, with desired log sinks to log 
    /// the trace messages.</para>
    /// </remarks>
    public class StatisticsTracer : IDisposable
    {
        /// <summary>
        /// Event id for Trace messages
        /// </summary>
        public const int EventId = 1;

        /// <summary>
        /// Title for operation start Trace messages
        /// </summary>
        public const string StartTitle = "StatisticsEnter";

        /// <summary>
        /// Title for operation end Trace messages
        /// </summary>
        public const string EndTitle = "StatisticsExit";

        private readonly List<StatisticsEntry> _statisticEntries = new List<StatisticsEntry>();

        /// <summary>
        /// Priority value for Statistic messages
        /// </summary>
        private int _priority = 5;

        private bool _tracerDisposed;
        private bool _tracingAvailable;

        private string _tabulation;

        private IStatisticsWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsTracer"/> class with the given logical operation name.
        /// </summary>
        /// <remarks>
        /// If an existing activity id is already set, it will be kept. Otherwise, a new activity id will be created.
        /// </remarks>
        /// <param name="operation">
        /// The operation for the <see cref="StatisticsTracer"/>
        /// </param>
        /// <param name="priority">
        /// The priority.
        /// </param>
        public StatisticsTracer(string operation, int priority)
        {
            Priority = priority;

            if (CheckTracingAvailable())
            {
                Initialize(operation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsTracer"/> class with the given logical operation name.
        /// </summary>
        /// <remarks>
        /// If an existing activity id is already set, it will be kept. Otherwise, a new activity id will be created.
        /// </remarks>
        /// <param name="operation">
        /// The operation for the <see cref="StatisticsTracer"/>
        /// </param>
        /// <param name="writer">
        /// The <see cref="StatisticsTracer"/> that is used to write trace messages
        /// </param>
        /// <param name="priority">
        /// The priority.
        /// </param>
        public StatisticsTracer(string operation, IStatisticsWriter writer, int priority)
        {
            Priority = priority;

            if (CheckTracingAvailable())
            {
                if (writer == null)
                    throw new ArgumentNullException("writer");

                _writer = writer;

                Initialize(operation);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="StatisticsTracer"/> class. 
        /// <para>
        /// Releases unmanaged resources and performs other cleanup operations before the <see cref="StatisticsTracer"/> is 
        /// reclaimed by garbage collection
        /// </para>
        /// </summary>
        ~StatisticsTracer()
        {
            Dispose(false);
        }

        public Stopwatch Watch { get; private set; }

        /// <summary>
        /// Gets or sets Priority value for Statistic messages
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public static StatisticsTracer NewStatisticsTracer(string operation, int priority)
        {
            return new StatisticsTracer(operation, priority);
        }

        public static StatisticsTracer NewStatisticsTracer(string operation, IStatisticsWriter writer, int priority)
        {
            return new StatisticsTracer(operation, writer, priority);
        }

        /// <summary>
        /// Causes the <see cref="StatisticsTracer"/> to output its closing message.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Answers whether tracing is enabled
        /// </summary>
        /// <returns>true if tracing is enabled</returns>
        public bool IsTracingEnabled()
        {
            return true;
        }

        public void WriteTraceMessage(string message, string entryTitle, TraceEventType eventType)
        {
            var extendedProperties = new Dictionary<string, object>();
            var entry = new StatisticsEntry(message, PeekLogicalOperationStack() as string, Priority, EventId, eventType, _tabulation + entryTitle, extendedProperties);

            _statisticEntries.Add(entry);
         }

        public void WriteTraceStartMessage(string entryTitle, string methodName)
        {
            string message = string.Format("in method {0} at {1} ticks.", methodName, Stopwatch.GetTimestamp());

            WriteTraceMessage(message, entryTitle, TraceEventType.Start);

            _tabulation += "\t";
        }

        public void WriteTraceEndMessage(string entryTitle, string methodName)
        {
            WriteTraceEndMessage(entryTitle, methodName, Watch.ElapsedMilliseconds);
        }

        public void WriteTraceEndMessage(string entryTitle, string methodName, long ellapsedMilliseconds)
        {
            int lastIndex = _tabulation.LastIndexOf("\t");

            if (lastIndex != -1)
            {
                _tabulation = _tabulation.Remove(lastIndex);
            }

            long tracingEndTicks = Stopwatch.GetTimestamp();
            decimal secondsElapsed = GetSecondsElapsed(ellapsedMilliseconds);

            string message = string.Format("in method {0} at {1} ticks (elapsed time: {2} seconds).", methodName, tracingEndTicks, secondsElapsed);
            WriteTraceMessage(message, entryTitle, TraceEventType.Stop);
        }

        internal static bool IsTracingAvailable()
        {
            bool tracingAvailable = false;

            try
            {
                tracingAvailable = SecurityManager.IsGranted(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            }
            catch (SecurityException)
            {
            }

            return tracingAvailable;
        }

        /// <summary>
        /// <para>Releases the unmanaged resources used by the <see cref="StatisticsTracer"/> and optionally releases 
        /// the managed resources.</para>
        /// </summary>
        /// <param name="disposing">
        /// <para><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> 
        /// to release only unmanaged resources.</para>
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_tracerDisposed)
            {
                try
                {
                    if (IsTracingEnabled())
                    {
                        WriteTraceEndMessage(EndTitle, GetExecutingMethodName(), Watch.ElapsedMilliseconds);

                        foreach (var statisticsEntry in _statisticEntries)
                        {
                            GetWriter().Write(statisticsEntry);
                        }

                        GetWriter().Flush();
                    }
                }
                finally
                {
                    try
                    {
                        StopLogicalOperation();
                    }
                    catch (SecurityException)
                    {
                    }
                }

                _tracerDisposed = true;
            }
        }

        private static decimal GetSecondsElapsed(long milliseconds)
        {
            decimal result = Convert.ToDecimal(milliseconds) / 1000m;
            return Math.Round(result, 6);
        }

        private static object PeekLogicalOperationStack()
        {
            return Trace.CorrelationManager.LogicalOperationStack.Peek();
        }

        private static void StartLogicalOperation(string operation)
        {
            Trace.CorrelationManager.StartLogicalOperation(operation);
        }

        private static void StopLogicalOperation()
        {
            Trace.CorrelationManager.StopLogicalOperation();
        }

        private bool CheckTracingAvailable()
        {
            _tracingAvailable = IsTracingAvailable();

            return _tracingAvailable;
        }

        private void Initialize(string operation)
        {
            StartLogicalOperation(operation);
            if (IsTracingEnabled())
            {
                Watch = Stopwatch.StartNew();
                WriteTraceStartMessage(StartTitle, GetExecutingMethodName());
            }
        }

        private string GetExecutingMethodName()
        {
            string result = "Unknown";
            var trace = new StackTrace(false);

            for (int index = 0; index < trace.FrameCount; ++index)
            {
                StackFrame frame = trace.GetFrame(index);
                MethodBase method = frame.GetMethod();
                if (method.DeclaringType != GetType())
                {
                    result = string.Concat(method.DeclaringType.FullName, ".", method.Name);
                    break;
                }
            }

            return result;
        }

        private IStatisticsWriter GetWriter()
        {
            return _writer ?? (_writer = new DebugStatisticsWriter());
        }
    }
}
