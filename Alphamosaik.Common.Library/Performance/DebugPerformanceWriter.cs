// -----------------------------------------------------------------------------------public ---------------------------------
// <copyright file="DebugPerformanceWriter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DebugPerformanceWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace Alphamosaik.Common.Library.Performance
{
    public class DebugPerformanceWriter : IPerformanceWriter
    {
        public void Write(PerformanceEntry performance)
        {
            Trace.WriteLine(performance.Message, performance.Title);
        }
    }
}