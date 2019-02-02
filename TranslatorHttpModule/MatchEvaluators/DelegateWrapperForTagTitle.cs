// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagTitle.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagTitle type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagTitle : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            string value =
                match.Value.Substring(1, match.Value.Length - 1).Trim().Replace("Title=\"", string.Empty).Replace("Title= \"", string.Empty).Replace("title= \"", string.Empty).Replace("title=\"",
                                                                                                  string.Empty).Replace(
                                                                                                      "\"", string.Empty);

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
