// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenFunction.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Abstract base for all <see cref="TokenFunction"></see>-derived classes.
//   Provides default algorithm for formatting a token.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Abstract base for all <see cref="TokenFunction"></see>-derived classes. 
    /// Provides default algorithm for formatting a token.
    /// </summary>
    public abstract class TokenFunction
    {
        private readonly string _startDelimiter = string.Empty;
        private readonly string _endDelimiter = ")}";

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenFunction"/> class. 
        /// Initializes an instance of a TokenFunction with a start delimiter and the default end delimiter.
        /// </summary>
        /// <param name="tokenStartDelimiter">
        /// Start delimiter.
        /// </param>
        protected TokenFunction(string tokenStartDelimiter)
        {
            if (string.IsNullOrEmpty(tokenStartDelimiter))
            {
                throw new ArgumentNullException("tokenStartDelimiter");
            }

            _startDelimiter = tokenStartDelimiter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenFunction"/> class. 
        /// Initializes an instance of a TokenFunction with a start and end delimiter.
        /// </summary>
        /// <param name="tokenStartDelimiter">
        /// Start delimiter.
        /// </param>
        /// <param name="tokenEndDelimiter">
        /// End delimiter.
        /// </param>
        protected TokenFunction(string tokenStartDelimiter, string tokenEndDelimiter)
        {
            if (string.IsNullOrEmpty(tokenStartDelimiter))
            {
                throw new ArgumentNullException("tokenStartDelimiter");
            }

            if (string.IsNullOrEmpty(tokenEndDelimiter))
            {
                throw new ArgumentNullException("tokenEndDelimiter");
            }

            _startDelimiter = tokenStartDelimiter;
            _endDelimiter = tokenEndDelimiter;
        }

        /// <summary>
        /// Searches for token functions in the message and replace all with formatted values.
        /// </summary>
        /// <param name="messageBuilder">Message template containing tokens.</param>
        /// <param name="performance">Log entry containing properties to format.</param>
        public virtual void Format(StringBuilder messageBuilder, PerformanceEntry performance)
        {
            int pos = 0;
            while (pos < messageBuilder.Length)
            {
                string messageString = messageBuilder.ToString();
                if (messageString.IndexOf(_startDelimiter) == -1)
                {
                    break;
                }

                string tokenTemplate = GetInnerTemplate(pos, messageString);
                string tokenToReplace = _startDelimiter + tokenTemplate + _endDelimiter;
                pos = messageBuilder.ToString().IndexOf(tokenToReplace);

                string tokenValue = FormatToken(tokenTemplate, performance);

                messageBuilder.Replace(tokenToReplace, tokenValue);
            }
        }

        /// <summary>
        /// Abstract method to process the token value between the start and end delimiter.
        /// </summary>
        /// <param name="tokenTemplate">Token value between the start and end delimiters.</param>
        /// <param name="performance">Log entry to process.</param>
        /// <returns>Formatted value to replace the token.</returns>
        public abstract string FormatToken(string tokenTemplate, PerformanceEntry performance);

        /// <summary>
        /// Returns the template in between the paratheses for a token function.
        /// Expecting tokens in this format: {keyvalue(myKey1)}.
        /// </summary>
        /// <param name="startPos">Start index to search for the next token function.</param>
        /// <param name="message">Message template containing tokens.</param>
        /// <returns>Inner template of the function.</returns>
        protected virtual string GetInnerTemplate(int startPos, string message)
        {
            int tokenStartPos = message.IndexOf(_startDelimiter, startPos) + _startDelimiter.Length;
            int endPos = message.IndexOf(_endDelimiter, tokenStartPos);
            return message.Substring(tokenStartPos, endPos - tokenStartPos);
        }
    }
}