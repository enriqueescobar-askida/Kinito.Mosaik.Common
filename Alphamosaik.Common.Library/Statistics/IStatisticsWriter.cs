// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStatisticsWriter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IStatisticsWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library.Statistics
{
    public interface IStatisticsWriter
    {
        /// <summary>
        /// Formats a performance entry and return a string to be outputted.
        /// </summary>
        /// <param name="statistics">
        /// Log entry to format.
        /// </param>
        void Write(StatisticsEntry statistics);

        void Flush();
    }
}
