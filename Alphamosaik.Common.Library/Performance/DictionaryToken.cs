// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryToken.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Formats a dictionary token by iterating through the dictionary and displays
//   the key and value for each entry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Formats a dictionary token by iterating through the dictionary and displays 
    /// the key and value for each entry.
    /// </summary>
    public class DictionaryToken : TokenFunction
    {
        private const string DictionaryKeyToken = "{key}";
        private const string DictionaryValueToken = "{value}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryToken"/> class. 
        /// Initializes a new instance of a <see cref="DictionaryToken"/>.
        /// </summary>
        public DictionaryToken() : base("{dictionary(")
        {
        }

        /// <summary>
        /// Iterates through each entry in the dictionary and display the key and/or value.
        /// </summary>
        /// <param name="tokenTemplate">Template to repeat for each key/value pair.</param>
        /// <param name="performance">Log entry containing the extended properties dictionary.</param>
        /// <returns>Repeated template for each key/value pair.</returns>
        public override string FormatToken(string tokenTemplate, PerformanceEntry performance)
        {
            var dictionaryBuilder = new StringBuilder();
            foreach (KeyValuePair<string, object> entry in performance.ExtendedProperties)
            {
                var singlePair = new StringBuilder(tokenTemplate);
                string keyName = string.Empty;
                if (entry.Key != null)
                {
                    keyName = entry.Key;
                }

                singlePair.Replace(DictionaryKeyToken, keyName);

                string keyValue = string.Empty;
                if (entry.Value != null)
                {
                    keyValue = entry.Value.ToString();
                }

                singlePair.Replace(DictionaryValueToken, keyValue);

                dictionaryBuilder.Append(singlePair.ToString());
            }

            return dictionaryBuilder.ToString();
        }
    }
}