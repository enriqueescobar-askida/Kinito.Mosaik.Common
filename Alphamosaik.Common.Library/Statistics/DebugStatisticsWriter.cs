// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugStatisticsWriter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DebugStatisticsWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Text;

namespace Alphamosaik.Common.Library.Statistics
{
    public class DebugStatisticsWriter : IStatisticsWriter
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        public void Write(StatisticsEntry statistics)
        {
            _buffer.AppendLine(statistics.Title + " : " + statistics.Message);
        }

        public void Flush()
        {
            Trace.WriteLine(_buffer.ToString());
        }
    }
}
