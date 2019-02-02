// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagButtons.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagButtons type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagButtons : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            MatchCollection result = TranslatorRegex.ValueButtonRegex.Matches(match.Value);

            foreach (Match currentMatch in result)
            {
                string value = currentMatch.Value.Substring(1, currentMatch.Value.Length - 2).Trim().Replace("value=\"", string.Empty).Replace("Value=\"", string.Empty).Replace("VALUE=\"", string.Empty).Replace("\"", string.Empty);

                if (!string.IsNullOrEmpty(value))
                {
                    string translated;
                    if (Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                    {
                        return match.Value.Replace(currentMatch.Value, currentMatch.Value.Replace(value, translated));
                    }
                }
            }

            return match.Value;
        }
    }
}
