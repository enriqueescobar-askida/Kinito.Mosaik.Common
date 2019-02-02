// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagNavigationMode.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagNavigationMode type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;
using Alphamosaik.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagNavigationMode : DelegateWrapperForTagsBase
    {
        private readonly StringBuilder _valueGlobal = new StringBuilder();

        public override string MatchEvaluatorTag(Match match)
        {
            _valueGlobal.SetNewString(match.Value);

            MatchCollection result1 = TranslatorRegex.ArgumentBetweenQuotes.Matches(match.Value);

            // Only transalte the second parameter.
            if (result1.Count > 1)
            {
                string value = result1[1].Value.Substring(1, result1[1].Value.Length - 2).Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    string translated;
                    if (Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                    {
                        _valueGlobal.Replace(value, translated.Replace("\'", "\\u0027").Replace("(", "\\u0028").Replace(")", "\\u0029").Replace("\"", "\\u0022"));
                    }
                }
            }

            return _valueGlobal.ToString();
        }
    }
}
