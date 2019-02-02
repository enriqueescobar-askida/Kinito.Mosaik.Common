// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStatisticFormatter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IStatistic type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library.Statistics
{
    /// <summary>
    /// Represents the interface for formatting statistics entry messsages.
    /// </summary>
    public interface IStatisticFormatter
    {
        /// <summary>
        /// Formats a statistic entry and return a string to be outputted.
        /// </summary>
        /// <returns>String representing the statistic entry.</returns>
        string Format();
    }
}
