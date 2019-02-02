// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagsMultiLookupPicker.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagsMultiLookupPicker type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;
using Alphamosaik.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagsMultiLookupPicker : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            return TranslatorRegex.ValueInputRegex.Replace(match.Value, MatchEvaluatorTagValue);
        }

        public string MatchEvaluatorTagValue(Match match)
        {
            string value = match.Groups["value"].Value;

            if (!string.IsNullOrEmpty(value))
            {
                var stringSeparators = new[] { "|t" };
                string[] split = value.Split(stringSeparators, StringSplitOptions.None);

                var translateds = new StringBuilder();
                foreach (string s in split)
                {
                    string s1 = s.Replace("&nbsp;", string.Empty).Replace("&lt;", "<").Replace("&gt;", ">").Replace(
                        "&quot;", "\"").Replace("&amp;", "&").Replace(char.ConvertFromUtf32(160), " ").Trim();

                    string translated;
                    if (!string.IsNullOrEmpty(s1) && Dictionary.Translate(CurrentContext, s1, LanguageSource, LanguageDestination, out translated))
                    {
                        translateds.Append(translated + "|t");
                    }
                    else
                    {
                        translateds.Append(s1 + "|t");
                    }
                }

                translateds.Remove(translateds.LastIndexOf("|t"), 2);

                string converted = translateds.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

                return match.Value.Replace(value, converted);
            }

            return match.Value;
        }
    }
}