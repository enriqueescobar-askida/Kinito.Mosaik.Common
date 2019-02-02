// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectedPropertyToken.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Token Function for Adding Reflected Property Values.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Token Function for Adding Reflected Property Values.
    /// </summary>
    public class ReflectedPropertyToken : TokenFunction
    {
        private const string StartDelimiter = "{property(";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedPropertyToken"/> class. 
        /// Constructor that initializes the token with the token name
        /// </summary>
        public ReflectedPropertyToken()
            : base(StartDelimiter)
        {
        }

        /// <summary>
        /// Searches for the reflected property and returns its value as a string
        /// </summary>
        /// <param name="tokenTemplate">
        /// The token Template.
        /// </param>
        /// <param name="performance">
        /// The performance.
        /// </param>
        /// <returns>
        /// The format token.
        /// </returns>
        public override string FormatToken(string tokenTemplate, PerformanceEntry performance)
        {
            Type logType = performance.GetType();
            PropertyInfo property = logType.GetProperty(tokenTemplate);
            if (property != null)
            {
                object value = property.GetValue(performance, null);
                return value != null ? value.ToString() : string.Empty;
            }

            return String.Format("Error: property {0} not found", tokenTemplate);
        }
    }
}
