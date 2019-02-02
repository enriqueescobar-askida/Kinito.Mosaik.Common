// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Statistic.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Statistic type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Alphamosaik.Common.Library.Statistics
{
    public class Statistic : IStatisticFormatter, IDisposable
    {
        /// <summary>
        /// Title for operation start Trace messages
        /// </summary>
        private const string StartTitle = "Enter";

        /// <summary>
        /// Title for operation end Trace messages
        /// </summary>
        private const string EndTitle = "Exit";

        private readonly StatisticsTracer _tracer;
        private readonly string _methodName;

        private readonly long _ellapsedMilliseconds;

        public Statistic(StatisticsTracer tracer, string methodName, int priority)
        {
            _methodName = methodName;

            if (tracer != null && priority <= tracer.Priority)
            {
                _tracer = tracer;
                _ellapsedMilliseconds = _tracer.Watch.ElapsedMilliseconds;
                _tracer.WriteTraceStartMessage(StartTitle, _methodName);
            }
        }

        ~Statistic()
        {
            Dispose(false);
        }

        public string Format()
        {
            return string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _tracer != null)
            {
                _tracer.WriteTraceEndMessage(EndTitle, _methodName, _tracer.Watch.ElapsedMilliseconds - _ellapsedMilliseconds);
            }
        }
    }
}
