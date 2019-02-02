// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeStampToken.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Formats a timestamp token with a custom date time format string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Globalization;

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Formats a timestamp token with a custom date time format string.
    /// </summary>
    public class TimeStampToken : TokenFunction
    {
        private const string LocalStartDelimiter = "local";
        private const string LocalStartDelimiterWithFormat = "local:";

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeStampToken"/> class. 
        /// Initializes a new instance of a <see cref="TimeStampToken"/>.
        /// </summary>
        public TimeStampToken()
            : base("{timestamp(")
        {
        }

        /// <summary>
        /// Formats the timestamp property with the specified date time format string.
        /// </summary>
        /// <param name="tokenTemplate">Date time format string.</param>
        /// <param name="performance">Log entry containing the timestamp.</param>
        /// <returns>Returns the formatted time stamp.</returns>
        public override string FormatToken(string tokenTemplate, PerformanceEntry performance)
        {
            string result;
            if (tokenTemplate.Equals(LocalStartDelimiter, System.StringComparison.InvariantCultureIgnoreCase))
            {
                System.DateTime localTime = performance.TimeStamp.ToLocalTime();
                result = localTime.ToString();
            }
            else if (tokenTemplate.StartsWith(LocalStartDelimiterWithFormat, System.StringComparison.InvariantCultureIgnoreCase))
            {
                string formatTemplate = tokenTemplate.Substring(LocalStartDelimiterWithFormat.Length);
                System.DateTime localTime = performance.TimeStamp.ToLocalTime();
                result = localTime.ToString(formatTemplate, CultureInfo.CurrentCulture);
            }
            else
            {
                result = performance.TimeStamp.ToString(tokenTemplate, CultureInfo.CurrentCulture);
            }

            return result;
        }
    }
}