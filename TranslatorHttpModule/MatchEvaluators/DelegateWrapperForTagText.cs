// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagText.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagText type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagText : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            string value = match.Groups["value"].Value;

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
