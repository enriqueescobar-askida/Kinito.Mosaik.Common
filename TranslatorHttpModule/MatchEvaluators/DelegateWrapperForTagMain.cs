// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagMain.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagMain type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagMain : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            string value =
                match.Groups[2].Value.Replace("&nbsp;", string.Empty).Replace("&lt;", "<").Replace("&gt;", ">").Replace(
                    "&quot;", "\"").Replace("&amp;", "&").Replace(Char.ConvertFromUtf32(160), " ").Trim();

            if (!string.IsNullOrEmpty(value))
            {
                if (ExtractorStatus == -1)
                {
                    string translated;
                    if (Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                    {
                        return match.Value.Replace("&nbsp;", string.Empty).Replace("&lt;", "<").Replace("&gt;", ">").Replace(char.ConvertFromUtf32(160), " ").Replace("&quot;", "\"").Replace("&amp;", "&").Replace(value, translated.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;"));
                    }
                }
                else
                {
                    string value1 = value;
                    var response = new StringBuilder(TempResponse.ToString());

                    SPSecurity.RunWithElevatedPrivileges(() => Utilities.AddToNewPhrasesList(value1, Url, LanguageSource, response, match.Value));
                }
            }

            return match.Value;
        }
    }
}