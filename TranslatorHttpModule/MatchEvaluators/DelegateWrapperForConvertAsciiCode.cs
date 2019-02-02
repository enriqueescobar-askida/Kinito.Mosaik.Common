// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForConvertAsciiCode.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForConvertAsciiCode type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Translator.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForConvertAsciiCode
    {
        public string MatchEvaluatorTag(Match match)
        {
            if (match.Value != "&#8206;")
            {
                string currentMatchAsciiChar = match.Value.Replace("&#", string.Empty).Replace(";", string.Empty);
                try
                {
                    int currentMatchAsciiInt = Convert.ToInt32(currentMatchAsciiChar);

                    char currentMatchAsciiCharChar = Convert.ToChar(currentMatchAsciiInt);
                    var currentMatchAsciiCharByte = (byte)currentMatchAsciiCharChar;
                    return match.Value.Replace(match.Value, Convert.ToString((char)currentMatchAsciiCharByte));
                }
                catch (Exception e)
                {
                    Utilities.LogException("ConvertAsciiCode", e, EventLogEntryType.Warning);
                }
            }

            return match.Value;
        }
    }
}
