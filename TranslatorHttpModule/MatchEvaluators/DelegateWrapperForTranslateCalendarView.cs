// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTranslateCalendarView.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTranslateCalendarView type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTranslateCalendarView : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            return TranslatorRegex.ArgumentBetweenQuotesRegex.Replace(match.Value, MatchEvaluatorTagArgumentBetweenQuotes);
        }

        public string MatchEvaluatorTagArgumentBetweenQuotes(Match match)
        {
            string value = match.Groups["value"].Value;

            if (!string.IsNullOrEmpty(value))
            {
                string translated;
                if (Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                {
                    return match.Value.Replace(value, value.Replace(",\"" + value + "\",", ",\"" + translated + "\","));
                }
            }

            return match.Value;
        }
    }
}