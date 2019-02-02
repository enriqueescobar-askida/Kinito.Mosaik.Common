// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForSearchBox.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForSearchBox type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForSearchBox : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            string value = match.Value;

            foreach (Match valueInputMatch in TranslatorRegex.ValueInputRegex.Matches(match.Value))
            {
                string inputValue = valueInputMatch.Groups["value"].Value;

                if (!string.IsNullOrEmpty(inputValue))
                {
                    string translated;
                    if (Dictionary.Translate(CurrentContext, inputValue, LanguageSource, LanguageDestination, out translated))
                    {
                        value = TranslatorRegex.BlurRegex.IsMatch(match.Value) ? match.Value.Replace(valueInputMatch.Value, "value=\"" + translated + "\"").Replace(TranslatorRegex.BlurRegex.Match(match.Value).Value, "{this.value='" + translated + "';") : match.Value.Replace(valueInputMatch.Value, "value=\"" + translated + "\"");
                    }
                }
            }

            return value;
        }
    }
}
