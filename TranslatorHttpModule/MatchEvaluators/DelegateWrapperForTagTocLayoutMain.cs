// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagTocLayoutMain.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagTocLayoutMain type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagTocLayoutMain : DelegateWrapperForTagsBase
    {
        public DelegateWrapperForTagTocLayoutMain()
        {
        }

        public DelegateWrapperForTagTocLayoutMain(StringBuilder tempResponse, string url, SPContext current, string currentLccode)
            : base(tempResponse, 0, url, current, currentLccode, string.Empty)
        {
        }

        public void Init(StringBuilder tempResponse, string url, SPContext current, string currentLccode)
        {
            Init(tempResponse, 0, url, current, currentLccode, string.Empty);
        }

        public override string MatchEvaluatorTag(Match match)
        {
            return TranslatorRegex.PatternLiRegex.Replace(match.Value, MatchEvaluatorLi);
        }

        private string MatchEvaluatorLi(Match match)
        {
            MatchCollection resulthRef = TranslatorRegex.PatternHrefRegex.Matches(match.Value);
            foreach (Match currentMatchHref in resulthRef)
            {
                if (currentMatchHref.Value.IndexOf("aspx", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    if (!IsPageDisplayInCurrentLanguage(currentMatchHref.Groups["url"].Value, LanguageSource))
                    {
                        return string.Empty;
                    }
                }
            }

            return match.Value;
        }

        private bool IsPageDisplayInCurrentLanguage(string url, string currentLccode)
        {
            string pageLanguage = GetPageLanguage(url);

            if (!string.IsNullOrEmpty(pageLanguage) && !pageLanguage.Equals("SPS_LNG_" + currentLccode))
                return false;

            return true;
        }

        private string GetPageLanguage(string pageUrl)
        {
            string fullUrl = SPUtility.ConcatUrls(Url, pageUrl);

            using (var site = new SPSite(fullUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList pageList = web.Lists.TryGetList("Pages") ?? web.Lists.TryGetList("SitePages");

                    if (pageList != null && pageList.Fields.ContainsField("SharePoint_Item_Language"))
                    {
                        string pageName = SPUtility.GetUrlFileName(fullUrl);

                        var query = new SPQuery
                                        {
                                            Query = "<Where><Eq><FieldRef Name='FileLeafRef'/>" +
                                                    "<Value Type='File'>" + pageName +
                                                    "</Value></Eq></Where>",
                                            QueryThrottleMode = SPQueryThrottleOption.Override
                                        };

                        SPListItemCollection collListItems = pageList.GetItems(query);

                        if (collListItems.Count > 0)
                        {
                            foreach (SPListItem item in collListItems)
                            {
                                if ((item["SharePoint_Item_Language"] != null) && (item["SharePoint_Item_Language"].ToString() != "(SPS_LNG_ALL)"))
                                {
                                    return item["SharePoint_Item_Language"].ToString();
                                }
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
