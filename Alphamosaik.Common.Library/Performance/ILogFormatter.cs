// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogFormatter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Represents the interface for formatting performance entry messsages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Represents the interface for formatting performance entry messsages.
    /// </summary>
    public interface ILogFormatter
    {
        /// <summary>
        /// Formats a performance entry and return a string to be outputted.
        /// </summary>
        /// <param name="performance">Log entry to format.</param>
        /// <returns>String representing the performance entry.</returns>
        string Format(PerformanceEntry performance);
    }
}