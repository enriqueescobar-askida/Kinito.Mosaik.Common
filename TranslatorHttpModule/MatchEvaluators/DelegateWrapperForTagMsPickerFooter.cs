// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagMsPickerFooter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagMsPickerFooter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;
using Translator.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    using Microsoft.SharePoint;

    public class DelegateWrapperForTagMsPickerFooter : DelegateWrapperForTagsBase
    {
        private int _lcidDefaultCulture;
        private int _lcidDestinationCulture;

        public DelegateWrapperForTagMsPickerFooter()
        {
        }

        public DelegateWrapperForTagMsPickerFooter(StringBuilder tempResponse, int extractorStatus, string url, SPContext current, string currentLccode, string languageCode)
            : base(tempResponse, extractorStatus, url, current, currentLccode, languageCode)
        {
            _lcidDefaultCulture = Utilities.GetSiteWebDefaultLanguage();
            _lcidDestinationCulture = Languages.Instance.GetLcid(languageCode);
        }

        public new void Init(StringBuilder tempResponse, int extractorStatus, string url, SPContext current, string currentLccode, string languageCode)
        {
            base.Init(tempResponse, extractorStatus, url, current, currentLccode, languageCode);

            _lcidDefaultCulture = Utilities.GetSiteWebDefaultLanguage();
            _lcidDestinationCulture = Languages.Instance.GetLcid(languageCode);
        }

        public override string MatchEvaluatorTag(Match match)
        {
            MatchCollection result1 = TranslatorRegex.TagMainRegex.Matches(match.Value);

            foreach (Match currentMatchQuotes in result1)
            {
                string value = currentMatchQuotes.Groups[2].Value;

                if (!string.IsNullOrEmpty(value))
                {
                    string dateConverted = Utilities.ConvertDateUsingCulture(value, _lcidDefaultCulture, _lcidDestinationCulture);

                    if (!string.IsNullOrEmpty(dateConverted))
                    {
                        return match.Value.Replace(value, dateConverted);
                    }
                }
            }

            return match.Value;
        }
    }
}
