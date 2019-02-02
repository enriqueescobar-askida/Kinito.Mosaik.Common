// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagsBase.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagsBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;
using Translator.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    using Microsoft.SharePoint;

    public class DelegateWrapperForTagsBase
    {
        public DelegateWrapperForTagsBase()
        {
        }

        public DelegateWrapperForTagsBase(StringBuilder tempResponse, int extractorStatus, string url, SPContext current, string currentLccode, string languageCode)
        {
            Init(tempResponse, extractorStatus, url, current, currentLccode, languageCode);
        }

        public StringBuilder TempResponse { get; private set; }

        public string LanguageDestination { get; private set; }

        public string LanguageSource { get; private set; }

        public int ExtractorStatus { get; private set; }

        public string Url { get; private set; }

        public SPContext CurrentContext { get; private set; }

        public BaseDictionary Dictionary { get; private set; }

        public void Init(StringBuilder tempResponse, int extractorStatus, string url, SPContext current, string currentLccode, string languageCode)
        {
            TempResponse = tempResponse;
            ExtractorStatus = extractorStatus;
            LanguageSource = currentLccode;
            LanguageDestination = languageCode;
            Url = url;
            CurrentContext = current;
            Dictionary = Dictionaries.Instance.GetRootDictionary(current.Site.WebApplication.Id, current.Site.ID, current.Web.ID);
        }

        public virtual string MatchEvaluatorTag(Match match)
        {
            throw new NotImplementedException();
        }
    }
}