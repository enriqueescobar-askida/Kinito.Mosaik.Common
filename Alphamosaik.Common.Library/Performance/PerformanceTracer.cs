// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformanceTracer.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Represents a performance tracing class to log method entry/exit and duration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Represents a performance tracing class to log method entry/exit and duration.
    /// </summary>
    /// <remarks>
    /// <para>Lifetime of the PerformanceTracer object will determine the beginning and the end of
    /// the trace.  The trace message will include, method being traced, start time, end time 
    /// and duration.</para>
    /// <para>Since PerformanceTracer uses the logging block to log the trace message, you can include application
    /// data as part of your trace message. Configured items from call context will be logged as
    /// part of the message.</para>
    /// <para>Trace message will be logged to the log category with the same name as the tracer operation name.
    /// You must configure the operation categories, or the catch-all categories, with desired log sinks to log 
    /// the trace messages.</para>
    /// </remarks>
    public class PerformanceTracer : IDisposable
    {
        /// <summary>
        /// Priority value for Trace messages
        /// </summary>
        public const int Priority = 5;

        /// <summary>
        /// Event id for Trace messages
        /// </summary>
        public const int EventId = 1;

        /// <summary>
        /// Title for operation start Trace messages
        /// </summary>
        public const string StartTitle = "TracerEnter";

        /// <summary>
        /// Title for operation end Trace messages
        /// </summary>
        public const string EndTitle = "TracerExit";

        /// <summary>
        /// Name of the entry in the ExtendedProperties having the activity id
        /// </summary>
        public const string ActivityIdPropertyKey = "TracerActivityId";

        private Stopwatch _stopwatch;
        private long _tracingStartTicks;
        private bool _tracerDisposed;
        private bool _tracingAvailable;

        private IPerformanceWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTracer"/> class with the given logical operation name.
        /// </summary>
        /// <remarks>
        /// If an existing activity id is already set, it will be kept. Otherwise, a new activity id will be created.
        /// </remarks>
        /// <param name="operation">The operation for the <see cref="PerformanceTracer"/></param>
        public PerformanceTracer(string operation)
        {
            if (CheckTracingAvailable())
            {
                if (GetActivityId().Equals(Guid.Empty))
                {
                    SetActivityId(Guid.NewGuid());
                }

                Initialize(operation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTracer"/> class with the given logical operation name and activity id.
        /// </summary>
        /// <remarks>
        /// The activity id will override a previous activity id
        /// </remarks>
        /// <param name="operation">The operation for the <see cref="PerformanceTracer"/></param>
        /// <param name="activityId">The activity id</param>
        public PerformanceTracer(string operation, Guid activityId)
        {
            if (CheckTracingAvailable())
            {
                SetActivityId(activityId);

                Initialize(operation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTracer"/> class with the given logical operation name.
        /// </summary>
        /// <remarks>
        /// If an existing activity id is already set, it will be kept. Otherwise, a new activity id will be created.
        /// </remarks>
        /// <param name="operation">The operation for the <see cref="PerformanceTracer"/></param>
        /// <param name="writer">The <see cref="IPerformanceWriter"/> that is used to write trace messages</param>
        public PerformanceTracer(string operation, IPerformanceWriter writer)
        {
            if (CheckTracingAvailable())
            {
                if (writer == null) throw new ArgumentNullException("writer");

                _writer = writer;

                if (GetActivityId().Equals(Guid.Empty))
                {
                    SetActivityId(Guid.NewGuid());
                }

                Initialize(operation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTracer"/> class with the given logical operation name and activity id.
        /// </summary>
        /// <remarks>
        /// The activity id will override a previous activity id
        /// </remarks>
        /// <param name="operation">The operation for the <see cref="PerformanceTracer"/></param>
        /// <param name="activityId">The activity id</param>
        /// <param name="writer">The <see cref="IPerformanceWriter"/> that is used to write trace messages</param>
        public PerformanceTracer(string operation, Guid activityId, IPerformanceWriter writer)
        {
            if (CheckTracingAvailable())
            {
                if (writer == null) throw new ArgumentNullException("writer");

                SetActivityId(activityId);

                _writer = writer;

                Initialize(operation);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PerformanceTracer"/> class. 
        /// <para>
        /// Releases unmanaged resources and performs other cleanup operations before the <see cref="PerformanceTracer"/> is 
        /// reclaimed by garbage collection
        /// </para>
        /// </summary>
        ~PerformanceTracer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Causes the <see cref="PerformanceTracer"/> to output its closing message.
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
        /// <para>Releases the unmanaged resources used by the <see cref="PerformanceTracer"/> and optionally releases 
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
                if (_tracingAvailable)
                {
                    try
                    {
                        if (IsTracingEnabled()) WriteTraceEndMessage(EndTitle);
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
                }

                _tracerDisposed = true;
            }
        }

        private static decimal GetSecondsElapsed(long milliseconds)
        {
            decimal result = Convert.ToDecimal(milliseconds) / 1000m;
            return Math.Round(result, 6);
        }

        private static Guid GetActivityId()
        {
            return Trace.CorrelationManager.ActivityId;
        }

        private static void SetActivityId(Guid activityId)
        {
            Trace.CorrelationManager.ActivityId = activityId;
        }

        private static void StartLogicalOperation(string operation)
        {
            Trace.CorrelationManager.StartLogicalOperation(operation);
        }

        private static void StopLogicalOperation()
        {
            Trace.CorrelationManager.StopLogicalOperation();
        }

        private static object PeekLogicalOperationStack()
        {
            return Trace.CorrelationManager.LogicalOperationStack.Peek();
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
                _stopwatch = Stopwatch.StartNew();
                _tracingStartTicks = Stopwatch.GetTimestamp();

                WriteTraceStartMessage(StartTitle);
            }
        }

        private void WriteTraceStartMessage(string entryTitle)
        {
            string methodName = GetExecutingMethodName();
            string message = string.Format("Start Trace: Activity {0} in method {1} at {2} ticks.", GetActivityId(), methodName, _tracingStartTicks);

            WriteTraceMessage(message, entryTitle, TraceEventType.Start);
        }

        private void WriteTraceEndMessage(string entryTitle)
        {
            long tracingEndTicks = Stopwatch.GetTimestamp();
            decimal secondsElapsed = GetSecondsElapsed(_stopwatch.ElapsedMilliseconds);

            string methodName = GetExecutingMethodName();
            string message = string.Format("End Trace: Activity {0} in method {1} at {2} ticks (elapsed time: {3} seconds).", GetActivityId(), methodName, tracingEndTicks, secondsElapsed);
            WriteTraceMessage(message, entryTitle, TraceEventType.Stop);
        }

        private void WriteTraceMessage(string message, string entryTitle, TraceEventType eventType)
        {
            var extendedProperties = new Dictionary<string, object>();
            var entry = new PerformanceEntry(message, PeekLogicalOperationStack() as string, Priority, EventId, eventType, entryTitle, extendedProperties);

            GetWriter().Write(entry);
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

        private IPerformanceWriter GetWriter()
        {
            return _writer ?? (_writer = new DebugPerformanceWriter());
        }
    }
}
