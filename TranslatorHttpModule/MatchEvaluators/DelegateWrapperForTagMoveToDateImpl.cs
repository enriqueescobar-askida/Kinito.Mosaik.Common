// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagMoveToDateImpl.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagMoveToDateImpl type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;
using Alphamosaik.Common.Library;
using Translator.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    using Microsoft.SharePoint;

    public class DelegateWrapperForTagMoveToDateImpl : DelegateWrapperForTagsBase
    {
        private readonly StringBuilder _valueGlobal = new StringBuilder();
        private int _lcidDefaultCulture;
        private int _lcidDestinationCulture;

        public DelegateWrapperForTagMoveToDateImpl()
        {
        }

        public DelegateWrapperForTagMoveToDateImpl(StringBuilder tempResponse, int extractorStatus, string url, SPContext current, string currentLccode, string languageCode)
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
            _valueGlobal.SetNewString(match.Value);

            MatchCollection result1 = TranslatorRegex.ArgumentBetweenQuotes.Matches(match.Value);

            foreach (Match currentMatchQuotes in result1)
            {
                string value = currentMatchQuotes.Value.Substring(1, currentMatchQuotes.Value.Length - 2).Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    string dateConverted = Utilities.ConvertDateUsingCulture(Regex.Unescape(value), _lcidDefaultCulture, _lcidDestinationCulture);

                    if (!string.IsNullOrEmpty(dateConverted))
                    {
                        return match.Value.Replace(value, Regex.Escape(dateConverted));
                    }
                }
            }

            return _valueGlobal.ToString();
        }
    }
}