// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyValueToken.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Formats a keyvalue token and displays the dictionary entries value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Formats a keyvalue token and displays the dictionary entries value.
    /// </summary>
    public class KeyValueToken : TokenFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueToken"/> class. 
        /// Initializes a new instance of a <see cref="TimeStampToken"/>.
        /// </summary>
        public KeyValueToken() : base("{keyvalue(")
        {
        }

        /// <summary>
        /// Gets the value of a property from the performance entry.
        /// </summary>
        /// <param name="tokenTemplate">Dictionary key name.</param>
        /// <param name="performance">Log entry containing with extended properties dictionary values.</param>
        /// <returns>The value of the key from the extended properties dictionary, or <see langword="null"/> 
        /// (Nothing in Visual Basic) if there is no entry with that key.</returns>
        public override string FormatToken(string tokenTemplate, PerformanceEntry performance)
        {
            string propertyString = string.Empty;
            object propertyObject;

            if (performance.ExtendedProperties.TryGetValue(tokenTemplate, out propertyObject))
            {
                propertyString = propertyObject.ToString();
            }

            return propertyString;
        }
    }
}