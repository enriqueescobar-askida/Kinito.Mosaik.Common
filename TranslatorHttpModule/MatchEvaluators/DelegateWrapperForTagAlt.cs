// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagAlt.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagAlt type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagAlt : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            string value = match.Groups["value"].Value.Trim();

            if (!string.IsNullOrEmpty(value))
            {
                string translated;
                if (Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                {
                    return match.Value.Replace(value, translated);
                }
            }

            return match.Value;
        }
    }
}
