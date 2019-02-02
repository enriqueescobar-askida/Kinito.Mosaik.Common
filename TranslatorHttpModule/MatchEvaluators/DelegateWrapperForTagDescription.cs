// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagDescription.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagDescription type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagDescription : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            string value = match.Value.Substring(1, match.Value.Length - 1).Trim().Replace("description=\"", string.Empty).Replace("\"", string.Empty);

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
