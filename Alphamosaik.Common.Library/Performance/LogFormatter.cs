// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogFormatter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Abstract implememtation of the <see cref="ILogFormatter" /> interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Abstract implememtation of the <see cref="ILogFormatter"/> interface.
    /// </summary>
    public abstract class LogFormatter : ILogFormatter
    {
        /// <summary>
        /// Formats a performance entry and return a string to be outputted.
        /// </summary>
        /// <param name="performance">Log entry to format.</param>
        /// <returns>A string representing the performance entry.</returns>
        public abstract string Format(PerformanceEntry performance);
    }
}
