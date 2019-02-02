// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPerformanceWriter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IPerformanceWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library.Performance
{
    public interface IPerformanceWriter
    {
        /// <summary>
        /// Formats a performance entry and return a string to be outputted.
        /// </summary>
        /// <param name="performance">Log entry to format.</param>
        void Write(PerformanceEntry performance);
    }
}
