// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StandardTranslatorHelper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StandardTranslatorHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml;

using Alphamosaik.Common.Library;
using Alphamosaik.Common.Library.Licensing;
using Alphamosaik.Common.Library.Statistics;

using Alphamosaik.Common.SharePoint.Library.ConfigStore;
using Alphamosaik.Oceanik.Sdk;

using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;

using Translator.Common.Library;

using TranslatorHttpHandler.Dictionary;
using TranslatorHttpHandler.MatchEvaluators;

namespace TranslatorHttpHandler.TranslatorHelper
{
    /// <summary>
    /// All the functons are here ;-)
    /// </summary>
    public class StandardTranslatorHelper : TranslatorHelper
    {
        private static readonly DelegateWrapperForTagMain TagMainEvaluator = new DelegateWrapperForTagMain();
        private static readonly DelegateWrapperForTagText TagTextEvaluator = new DelegateWrapperForTagText();
        private static readonly DelegateWrapperForTagDescription TagDescriptionEvaluator = new DelegateWrapperForTagDescription();
        private static readonly DelegateWrapperForTagTitle TagTitleEvaluator = new DelegateWrapperForTagTitle();
        private static readonly DelegateWrapperForTagAlt TagAltEvaluator = new DelegateWrapperForTagAlt();
        private static readonly DelegateWrapperForTagButtons TagButtonsEvaluator = new DelegateWrapperForTagButtons();
        private static readonly DelegateWrapperForTagInnerHtml TagInnerHtmlEvaluator = new DelegateWrapperForTagInnerHtml();
        private static readonly DelegateWrapperForTagLayoutInfo TagLayoutInfoEvaluator = new DelegateWrapperForTagLayoutInfo();
        private static readonly DelegateWrapperForTagNavigationMode TagNavigationModeEvaluator = new DelegateWrapperForTagNavigationMode();
        private static readonly DelegateWrapperForTagMsPickerFooter TagMsPickerFooterEvaluator = new DelegateWrapperForTagMsPickerFooter();
        private static readonly DelegateWrapperForTagDateInSharePointList TagDateInSharePointListEvaluator = new DelegateWrapperForTagDateInSharePointList();
        private static readonly DelegateWrapperForTagRollover TagRolloverEvaluator = new DelegateWrapperForTagRollover();
        private static readonly DelegateWrapperForTagDatePicker TagDatePickerEvaluator = new DelegateWrapperForTagDatePicker();
        private static readonly DelegateWrapperForTagTocLayoutMain TagTocLayoutMain = new DelegateWrapperForTagTocLayoutMain();
        private static readonly DelegateWrapperForSearchBox TagSearchBox = new DelegateWrapperForSearchBox();
        private static readonly DelegateWrapperForConvertAsciiCode ConvertAsciiCodeEvaluator = new DelegateWrapperForConvertAsciiCode();
        private static readonly DelegateWrapperForTranslateCalendarView TagTranslateCalendarView = new DelegateWrapperForTranslateCalendarView();
        private static readonly DelegateWrapperForTagsMultiLookupPicker TagTranslateMultilookupPicker = new DelegateWrapperForTagsMultiLookupPicker();
        private static readonly DelegateWrapperForTagMoveToDateImpl TagMoveToDateEvaluator = new DelegateWrapperForTagMoveToDateImpl();
        private static readonly Dictionary<string, bool> DictionaryListExist = new Dictionary<string, bool>();

        private const string OceanikAutomaticTranslation = "Oceanik.AutomaticTranslation";

        private static string _autoCompletionPageToTreat;
        private static string _translatorVersion;

        private static bool _useCacheDisabled;

        public StandardTranslatorHelper()
        {
            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            // get the version object for this assembly
            _translatorVersion = "Oceanik 2010 V " + fv.ProductMajorPart + "." + fv.ProductMinorPart + "." + fv.ProductBuildPart + "." + fv.ProductPrivatePart + " ";
        }

        public static bool GetCacheMustBeReloaded(SPContext current, string siteUrl, out bool reloadCustomCache, out bool reloadGlobalCache)
        {
            reloadCustomCache = false;
            reloadGlobalCache = false;

            string rootUrl = Alphamosaik.Common.SharePoint.Library.Utilities.FilterUrl(current.Site.RootWeb.Url); // root server url

            string webId = "_" + Convert.ToString(SPContext.Current.Web.ID);

            var cacheIsLoaded = HttpContext.Current.Cache["SPS_TRANSLATION_CACHE_IS_LOADED" + webId] as string;

            if (!string.IsNullOrEmpty(cacheIsLoaded))
            {
                if (cacheIsLoaded == "2")
                {
                    reloadCustomCache = true;
                }
            }
            else
            {
                if (!Dictionaries.Instance.DictionaryExist(current.Site.WebApplication.Id, "Global"))
                {
                    reloadGlobalCache = true;
                }

                webId = "_" + current.Site.WebApplication.Id;
                var cacheWebApplicationIsLoaded = HttpContext.Current.Cache["SPS_TRANSLATION_CACHE_IS_LOADED" + webId] as string;

                if (!string.IsNullOrEmpty(cacheWebApplicationIsLoaded))
                {
                    if (cacheWebApplicationIsLoaded == "2")
                    {
                        reloadCustomCache = true;
                    }
                }
                else
                {
                    reloadCustomCache = true;
                }
            }

            if (!reloadCustomCache && SPContext.Current.Web != null &&
                !Dictionaries.Instance.DictionaryExist(current.Site.WebApplication.Id, current.Site.ID, current.Web.ID, "Custom"))
            {
                string key = current.Site.WebApplication.Id + current.Site.ID.ToString() + current.Web.ID;

                bool exist;
                DictionaryListExist.TryGetValue(key, out exist);

                if (!exist)
                {
                    string listName = "TranslationContentsSub";

                    if (rootUrl.IndexOf(siteUrl, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        listName = "TranslationContents";
                    }

                    SPList list = SPContext.Current.Web.Lists.TryGetList(listName);

                    if (list != null)
                    {
                        DictionaryListExist[key] = true;
                        reloadCustomCache = true;
                    }
                    else
                    {
                        DictionaryListExist[key] = false;
                    }
                }
            }

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            return reloadGlobalCache || reloadCustomCache;

            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        // ITranslator Implementation
        public override string Translate(string source, string languageDestination)
        {
            var htmlSource = new StringBuilder(source);

            if (!string.IsNullOrEmpty(source) && HttpContext.Current != null && SPContext.Current != null)
            {
                try
                {
                    var languageSource = (string)HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"];

                    try
                    {
                        languageSource = SPContext.Current.Web.UICulture.TwoLetterISOLanguageName.ToUpper();
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("Translate.TwoLetterISOLanguageName not set use default language", e, EventLogEntryType.Information);
                    }

                    string currentSiteUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.PathAndQuery, string.Empty);

                    TranslateFromDictionary(htmlSource, languageDestination, languageSource, -1, currentSiteUrl);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Translate", e, EventLogEntryType.Error);
                    return source;
                }
            }

            return htmlSource.ToString();
        }

        public override string Translate(StringBuilder source, string languageDestination)
        {
            return Translate(source.ToString(), languageDestination);
        }

        public override int GetLcid(string languageDestination)
        {
            return Languages.Instance.GetLcid(languageDestination);
        }

        public override string GetLanguageCode(string lcid)
        {
            return Languages.Instance.GetLanguageCode(lcid);
        }

        public override string GetLanguageCode(int lcid)
        {
            return Languages.Instance.GetLanguageCode(lcid);
        }

        public override string GetCurrrentLanguageCode()
        {
            return Utilities.GetLanguageCode(HttpContext.Current);
        }

        public override string GetWebPartLanguage(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            string language = string.Empty;

            try
            {
                var currentWebpart = webPart as WebPart;

                if (currentWebpart != null)
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        SPWeb web = Microsoft.SharePoint.WebControls.SPControl.GetContextWeb(HttpContext.Current);
                        string value;
                        Guid storageKey = currentWebpart.StorageKey;

                        if (web.AllProperties.ContainsKey("Alphamosaik.Translator.WebParts " + storageKey))
                        {
                            value = (string)web.AllProperties["Alphamosaik.Translator.WebParts " + storageKey];
                        }
                        else
                        {
                            value = string.Empty;
                        }

                        const string SpsWebPart = "_SPS_WEBPART_";

                        int index = value.IndexOf(SpsWebPart);

                        if (index != -1)
                        {
                            string subLanguage = value.Substring(index + SpsWebPart.Length, 2);

                            if (Languages.Instance.IsSupportedLanguage(subLanguage))
                            {
                                language = subLanguage;
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Utilities.TraceNormalCaughtException("GetWebPartLanguage", e);
            }

            return language;
        }

        /// <summary>
        /// Check if the page need to be translated
        /// </summary>
        /// <param name="url">url of the page</param>
        /// <returns>true if the page need to be translated, otherwise false</returns>
        public override bool IsPageToBeTranslated(string url)
        {
            try
            {
                if (HttpContext.Current.Cache["PagesToTranslate"] == null)
                {
                    AddPagesToTranslateToCache(url);
                }

                var arrPagesToTranslate = (ArrayList)HttpContext.Current.Cache["PagesToTranslate"];

                foreach (string arrayListItem in arrPagesToTranslate)
                {
                    if (HttpContext.Current.Request.Url.OriginalString.IndexOf(arrayListItem, StringComparison.OrdinalIgnoreCase) != -1)
                        return true;
                }

                if (HttpContext.Current.Cache["PagesNotToTranslate"] == null)
                {
                    AddPagesNotToTranslateToCache(url);
                }

                var arrPagesNotToTranslate = (ArrayList)HttpContext.Current.Cache["PagesNotToTranslate"];

                foreach (string arrayListItem in arrPagesNotToTranslate)
                {
                    if (HttpContext.Current.Request.Url.OriginalString.IndexOf(arrayListItem, StringComparison.OrdinalIgnoreCase) != -1)
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Utilities.LogException("IsPageToBeTranslated", e, EventLogEntryType.Warning);
                return true;
            }
        }

        /// <summary>
        /// Write an event in the trace log for an excetion
        /// </summary>
        /// <param name="exc">String to log</param>
        public void TraceError(Exception exc)
        {
            HttpContext.Current.Trace.Write("SharePoint Translator - Error:" + exc);
            Exception moreInnerException = exc.InnerException;
            while (moreInnerException != null)
            {
                HttpContext.Current.Trace.Warn("SharePoint Translator - Error:" + moreInnerException);
                moreInnerException = moreInnerException.InnerException;
            }
        }

        public override void AddHelper(StringBuilder tempResponse, string lang, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, string languageSource, ref bool filteringDisplayCompleted, bool mobilePage, License.LicenseType licenseType,
            ref Dictionary<int, string> excludedPartsFromTranslation)
        {
            // Javascript 
            const string FunctionSelectionChangeInitialDefinition =
                @"function OnSelectionChange(value)
{
	var today = new Date();
	var oneYear = new Date(today.getTime() + 365 * 24 * 60 * 60 * 1000);
	var url = window.location.href;
	document.cookie = ""lcid="" + value + "";path=/;expires="" + oneYear.toGMTString();
	window.location.href = url;
}
";

            const string FunctionSelectionChangeInitialDefinition2 =
                "\n function OnSelectionChange(value)\n{\n    var today = new Date();\n    var oneYear = new Date(today.getTime() + 365 * 24 * 60 * 60 * 1000);\n    var url = window.location.href;\n    document.cookie = 'lcid=' + value + ';path=/;expires=' + oneYear.toGMTString();\n    window.location.href = url;\n}\n";

            bool languagePackMsActivated = tempResponse.IndexOf(FunctionSelectionChangeInitialDefinition2) > -1;

            string changeLocationScript = "<script>";

            if (!languagePackMsActivated)
            {
                changeLocationScript += FunctionSelectionChangeInitialDefinition2;

                if (SPContext.Current != null && SPContext.Current.Web.IsMultilingual)
                    languagePackMsActivated = true;
            }

            changeLocationScript += @"function changeLocation(str,name,value) {
                    var result = '?';
                    var query = str.charAt(0) == '?' ? str.substring(1) : str;
                    var  found = '0';
                    ";

            if (mobilePage || ((languagePackMsActivated || Dictionaries.Instance.VisibleLanguages.Count > 1) && ((HttpContext.Current.Request.Url.ToString().IndexOf("EditForm.aspx") > -1) ||
                (HttpContext.Current.Request.Url.ToString().IndexOf("NewForm.aspx") > -1) ||
                (HttpContext.Current.Request.Url.ToString().IndexOf("DispForm.aspx") > -1) ||
                (HttpContext.Current.Request.Url.ToString().IndexOf("EditPost.aspx") > -1) ||
                (HttpContext.Current.Request.Url.ToString().IndexOf("/Post.aspx") > -1))))
            {
                changeLocationScript += @"if (name == 'SPSLanguage')
                    {
                        var cookie_value = 1033;";
                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                {
                    changeLocationScript += @"
                        if (value == '" + languageItem.LanguageDestination + "')";
                    changeLocationScript += @"                        {";
                    changeLocationScript += @"                            cookie_value = '" + languageItem.Lcid +
                                            "';";
                    changeLocationScript += @"                        }";
                }

                changeLocationScript += @"
                        var today = new Date();
                        var oneYear = new Date(today.getTime() + 365 * 24 * 60 * 60 * 1000);	                
                        document.cookie = 'lcid=' + cookie_value + ';path=/;expires=' + oneYear.toGMTString();
                    }";
            }

            changeLocationScript += @"
                    
                    if (query) {
                        var fields = query.split('&');
                        for (var f = 0; f < fields.length; f++) {
                        var field = fields[f].split('=');
                        var cname = field[0].replace(/\+/g, ' ');
                        var cvalue = field[1].replace(/\+/g, ' ');

                        if ( name == cname )
                        {
                            found = '1';
                            cvalue = value;
                        }

                        result = result + cname + '=' + cvalue + '&';
                        }


                    }
                        if ( found == '0' )
                        {
                            if (name != 'SPSLanguage')
                            {
                                result = result + name + '=' + value ;
                            }
                        }

                    result = result.charAt(result.length-1) == '&' ? result.substring(0,result.length-1) : result;
                    result = result.charAt(result.length-1) == '?' ? result.substring(0,result.length-1) : result;
                    return result;
                    }</script>";

            bool allWebPartsContentDisable = !(bool)HttpContext.Current.Cache["AlphamosaikWebPartContentTranslation"];

            var linkItemsId = new Dictionary<string, string>();
            var discussionParentIdLinkItems = new Dictionary<string, string>();

            try
            {
                bool isPageInEditMode = IsEditPageMode();
                bool isDiscussionBoard = false;

                if ((HttpContext.Current.Request.Url.ToString().IndexOf("EditForm.aspx") > -1) ||
                    (HttpContext.Current.Request.Url.ToString().IndexOf("DispForm.aspx") > -1) ||
                    (HttpContext.Current.Request.Url.ToString().IndexOf("EditPost.aspx") > -1) ||
                    (HttpContext.Current.Request.Url.ToString().IndexOf("/Post.aspx") > -1))
                {
                    HttpContext context = HttpContext.Current;
                    SPWeb web = Microsoft.SharePoint.WebControls.SPControl.GetContextWeb(context);

                    string itemId = context.Request.QueryString["ID"];

                    foreach (SPList current in web.Lists)
                    {
                        string formatedListName = "       ";

                        if (current.DefaultViewUrl.LastIndexOf("/Forms/") > -1)
                        {
                            formatedListName =
                                current.DefaultViewUrl.Remove(current.DefaultViewUrl.LastIndexOf("/Forms/"));
                            formatedListName =
                                formatedListName.Substring(formatedListName.LastIndexOf("/")).Replace("/",
                                                                                                      string.Empty);
                        }

                        if (current.DefaultViewUrl.LastIndexOf("/Lists/") > -1)
                        {
                            formatedListName =
                                current.DefaultViewUrl.Substring(current.DefaultViewUrl.LastIndexOf("/Lists/") +
                                                                 "/Lists/".Length);
                            formatedListName = formatedListName.Remove(formatedListName.IndexOf("/"));
                        }

                        if (context.Request.Url.ToString().Contains("/Lists/" + formatedListName + "/")
                            || context.Request.Url.ToString().Contains("/" + formatedListName + "/Forms/"))
                        {
                            SPListItem currentItem = current.GetItemById(Convert.ToInt32(itemId));

                            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                            {
                                if (current.Fields.ContainsField(languageItem.LanguageDestination + " version") &&
                                    !string.IsNullOrEmpty(currentItem.GetFormattedValue(languageItem.LanguageDestination + " version")))
                                {
                                    string itemIdToAdd = Convert.ToString(currentItem[languageItem.LanguageDestination + " version"]);
                                    itemIdToAdd = itemIdToAdd.Remove(itemIdToAdd.IndexOf(";"));
                                    linkItemsId.Add(languageItem.LanguageDestination, itemIdToAdd);
                                }
                                else
                                {
                                    linkItemsId.Add(languageItem.LanguageDestination, itemId);
                                }
                            }

                            break;
                        }
                    }

                    web.Dispose();
                }
                else
                {
                    try
                    {
                        if ((SPContext.Current != null) && (SPContext.Current.List != null))
                        {
                            if (SPContext.Current.List.BaseTemplate == SPListTemplateType.DiscussionBoard)
                            {
                                isDiscussionBoard = true;
                                string discussionUrl = string.Empty;

                                SPList list = SPContext.Current.List;

                                if (HttpContext.Current.Request.QueryString["RootFolder"] != null)
                                    discussionUrl = HttpContext.Current.Request.QueryString["RootFolder"];

                                SPListItem listItem = SPContext.Current.Web.GetListItem(discussionUrl);

                                if (list.Fields.ContainsField("SharePoint_Item_Language") && list.Fields.ContainsField("SharePoint_Group_Language"))
                                {
                                    if ((listItem["SharePoint_Group_Language"] != null) && (Convert.ToInt32(listItem["SharePoint_Group_Language"]) != 0))
                                    {
                                        var query = new SPQuery
                                        {
                                            Query =
                                                "<Where><Eq><FieldRef Name='SharePoint_Group_Language'/>"
                                                + "<Value Type='Number'>"
                                                + listItem["SharePoint_Group_Language"]
                                                + "</Value></Eq></Where>"
                                        };

                                        SPListItemCollection collListItems = list.GetItems(query);

                                        foreach (LanguageItem langTmp in Dictionaries.Instance.VisibleLanguages)
                                        {
                                            bool isLinkItemExist = false;
                                            foreach (SPListItem itemTmp in collListItems)
                                            {
                                                if ((itemTmp["SharePoint_Item_Language"] != null) && (("SPS_LNG_" + langTmp.LanguageDestination) == itemTmp["SharePoint_Item_Language"].ToString()))
                                                {
                                                    linkItemsId.Add(langTmp.LanguageDestination, itemTmp.Folder.Url);
                                                    isLinkItemExist = true;
                                                    break;
                                                }
                                            }

                                            if (!isLinkItemExist)
                                            {
                                                linkItemsId.Add(langTmp.LanguageDestination, discussionUrl);
                                            }
                                        }
                                    }

                                    if (HttpContext.Current.Request.QueryString["DiscussionParentID"] != null)
                                    {
                                        int discussionParentId = Convert.ToInt32(HttpContext.Current.Request.QueryString["DiscussionParentID"]);
                                        SPListItem parentDiscussion = list.GetItemById(discussionParentId);

                                        var query = new SPQuery
                                        {
                                            Query =
                                                "<Where><Eq><FieldRef Name='SharePoint_Group_Language'/>"
                                                + "<Value Type='Number'>"
                                                + parentDiscussion["SharePoint_Group_Language"]
                                                + "</Value></Eq></Where>"
                                        };

                                        if (parentDiscussion.Folder == null)
                                        {
                                            query.ViewAttributes = "Scope='Recursive'";
                                        }

                                        SPListItemCollection collListItems = list.GetItems(query);

                                        foreach (LanguageItem langTmp in Dictionaries.Instance.VisibleLanguages)
                                        {
                                            if (HttpContext.Current.Cache["SPS_Translator_" + langTmp.LanguageDestination] != null)
                                            {
                                                bool isLinkItemExist = false;
                                                foreach (SPListItem itemTmp in collListItems)
                                                {
                                                    if ((itemTmp["SharePoint_Item_Language"] != null) && (("SPS_LNG_" + langTmp.LanguageDestination) == itemTmp["SharePoint_Item_Language"].ToString()))
                                                    {
                                                        discussionParentIdLinkItems.Add(langTmp.LanguageDestination, itemTmp.ID.ToString());
                                                        isLinkItemExist = true;
                                                        break;
                                                    }
                                                }

                                                if (!isLinkItemExist)
                                                {
                                                    discussionParentIdLinkItems.Add(langTmp.LanguageDestination, discussionParentId.ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                HttpContext context = HttpContext.Current;
                                SPWeb web = Microsoft.SharePoint.WebControls.SPControl.GetContextWeb(context);
                                SPListItem currentItem = null;
                                SPList listForm = web.Lists.TryGetList("Forms");

                                if (listForm == null || listForm.ID != SPContext.Current.List.ID)
                                {
                                    SPList currentList = SPContext.Current.List;

                                    string pageName =
                                        HttpContext.Current.Request.Url.ToString().Substring(
                                            HttpContext.Current.Request.Url.ToString().LastIndexOf("/") + 1);

                                    if (pageName.Contains("?")) pageName = pageName.Remove(pageName.IndexOf('?'));

                                    SPFolder currentListFolder =
                                        Alphamosaik.Common.SharePoint.Library.Utilities.GetCurrentFolderInList(
                                            currentList);

                                    var query = new SPQuery
                                    {
                                        Query =
                                            "<Where><Eq><FieldRef Name='FileLeafRef'/>" + "<Value Type='File'>"
                                            + pageName + "</Value></Eq></Where>",
                                        ViewAttributes = "Scope=\"Recursive\"",
                                        QueryThrottleMode = SPQueryThrottleOption.Override
                                    };

                                    if (currentListFolder != null)
                                    {
                                        query.Folder = currentListFolder;
                                        query.ViewAttributes = "Scope=\"FilesOnly\"";
                                    }

                                    SPListItemCollection collListItems = currentList.GetItems(query);

                                    foreach (SPListItem tmpListItem in collListItems)
                                    {
                                        if (tmpListItem.Name.Equals(pageName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            currentItem = tmpListItem;
                                            break;
                                        }
                                    }

                                    if (currentItem != null)
                                    {
                                        foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                                        {
                                            if (
                                                currentList.Fields.ContainsField(
                                                    languageItem.LanguageDestination + " version")
                                                &&
                                                currentItem[languageItem.LanguageDestination + " version"] != null)
                                            {
                                                string itemIdToAdd =
                                                    Convert.ToString(
                                                        currentItem[languageItem.LanguageDestination + " version"]);
                                                itemIdToAdd = itemIdToAdd.Remove(itemIdToAdd.IndexOf(";"));
                                                linkItemsId.Add(
                                                    languageItem.LanguageDestination,
                                                    currentList.GetItemById(Convert.ToInt32(itemIdToAdd)).Name);
                                            }
                                            else
                                            {
                                                linkItemsId.Add(languageItem.LanguageDestination, pageName);
                                            }
                                        }
                                    }
                                }

                                web.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogException("AddHelper", ex, EventLogEntryType.Information);
                    }
                }

                // Generate translation banner
                string banner = string.Empty;

                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                {
                    // For DataSheet, transform before removing TR
                    tempResponse.Replace(";SPS_LNG_" + languageItem.LanguageDestination + "&", ";SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination + "&");
                    tempResponse.Replace("|SPS_LNG_" + languageItem.LanguageDestination + "&", "|SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination + "&");
                    tempResponse.Replace(";SPS_LNG_" + languageItem.LanguageDestination, ";SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination);
                    tempResponse.Replace("|SPS_LNG_" + languageItem.LanguageDestination, "|SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination);
                }

                tempResponse.Replace("_SharePoint_Item_Language", "_SharePoint_ALPHA_TEMP_Item Language");
                tempResponse.Replace(";SharePoint_Item_Language", ";SharePoint_ALPHA_TEMP_Item Language");

                string spsWebpartLanguageList = string.Empty;
                bool spsContentExist = allWebPartsContentDisable;
                var webPartListToRemove = new List<Guid>();
                Hashtable webPartHashTable = LoadWebPartHashTable(ref spsWebpartLanguageList, ref spsContentExist,
                                                                  ref webPartListToRemove, lang);
                foreach (var guid in webPartListToRemove)
                {
                    RemoveTranslatedWebPart(tempResponse, guid);
                }

                if (!spsWebpartLanguageList.Replace("_SPS_WEBPART_" + lang, string.Empty).Contains("_SPS_WEBPART_"))
                {
                    foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                    {
                        if (languageItem.LanguageDestination != lang)
                        {
                            UpdateMsoMenuWebPartMenu(tempResponse, "_SPS_WEBPART_" + languageItem.LanguageDestination, lang,
                                                     allWebPartsContentDisable, webPartHashTable);
                            break;
                        }
                    }
                }

                var bannerCssClassIsActivated = (bool)HttpContext.Current.Cache["AlphamosaikBannerCSSClass"];
                var bannerPipeIsActivated = (bool)HttpContext.Current.Cache["AlphamosaikBannerPipe"];

                string bannerCssClass = string.Empty;
                string bannerCssClassCurrentLang = string.Empty;

                string bannerPipe = string.Empty;
                if (bannerPipeIsActivated)
                    bannerPipe = " | ";

                var filteringDisplayActivated = (bool)HttpContext.Current.Cache["AlphamosaikListFiteringDisplay"];

                if (filteringDisplayActivated && (!viewAllItemsInEveryLanguages))
                    ListFilteringDisplay(tempResponse, lang, ref filteringDisplayCompleted);

                foreach (LanguageItem languageItem in Dictionaries.Instance.Languages)
                {
                    string notTranslateTag = string.Empty;

                    if (((bool)HttpContext.Current.Cache["AlphamosaikBannerWithNoTranslation"]) && (languageSource != lang))
                        notTranslateTag = "$$$SPSNOTRANSLATE$$$";

                    string displayLanguage = !string.IsNullOrEmpty(languageItem.Picture.Trim()) ? languageItem.Picture + notTranslateTag : languageItem.DisplayName + notTranslateTag;

                    if (bannerCssClassIsActivated)
                        bannerCssClass = "-" + languageItem.LanguageDestination;

                    if (languageItem.Visible)
                    {
                        if (languageItem.LanguageDestination != lang)
                        {
                            if (linkItemsId.ContainsKey(languageItem.LanguageDestination))
                            {
                                if ((HttpContext.Current.Request.Url.ToString().IndexOf("EditForm.aspx") > -1) ||
                                    (HttpContext.Current.Request.Url.ToString().IndexOf("DispForm.aspx") > -1) ||
                                    (HttpContext.Current.Request.Url.ToString().IndexOf("EditPost.aspx") > -1) ||
                                    (HttpContext.Current.Request.Url.ToString().IndexOf("/Post.aspx") > -1))
                                {
                                    if (languagePackMsActivated && !IsListUseDialogForms())
                                    {
                                    }
                                    else
                                    {
                                        banner += "<td class='ms-globallinks-translation" + bannerCssClass +
                                                  " ms-globallinks'  nowrap valign=middle><a onclick=\"window.location.search = changeLocation(changeLocation(window.location.search,'SPSLanguage','" +
                                                  languageItem.LanguageDestination + "'), 'ID', '" + linkItemsId[languageItem.LanguageDestination] +
                                                  "');\" class=\"ms-SPLink ms-hovercellinactive\" onmouseover=\"this.className='ms-SPLink ms-hovercellactive cursor'\" onmouseout=\"this.className='ms-SPLink ms-hovercellinactive'\" >" +
                                                  displayLanguage + bannerPipe + "</a>&nbsp;&nbsp;&nbsp; ";
                                    }
                                }
                                else
                                    if (isDiscussionBoard)
                                    {
                                        string targetPageName = linkItemsId[languageItem.LanguageDestination].Replace("'", "%27");
                                        string currentUrl = HttpContext.Current.Request.Url.ToString();
                                        string targetPageQuery = string.Empty;
                                        if (currentUrl.Contains("?"))
                                        {
                                            targetPageQuery = currentUrl.Substring(currentUrl.IndexOf('?'));
                                            targetPageQuery = targetPageQuery.Replace("?SPSLanguage=" + lang, "?SPSLanguage=" + languageItem.LanguageDestination);
                                            targetPageQuery = targetPageQuery.Replace("&SPSLanguage=" + lang, "&SPSLanguage=" + languageItem.LanguageDestination);
                                            if ((!targetPageQuery.Contains("?SPSLanguage=")) && (!targetPageQuery.Contains("&SPSLanguage=")))
                                                targetPageQuery = targetPageQuery + "&SPSLanguage=" + languageItem.LanguageDestination;
                                        }

                                        if ((!targetPageQuery.Contains("?SPSLanguage=")) && (!targetPageQuery.Contains("&SPSLanguage=")))
                                            targetPageQuery = targetPageQuery + "?SPSLanguage=" + languageItem.LanguageDestination;

                                        if (targetPageQuery.IndexOf("?") < targetPageQuery.LastIndexOf("?"))
                                            targetPageQuery = targetPageQuery.Replace("?SPSLanguage=", "&SPSLanguage=");

                                        if (targetPageQuery.Contains("&") && targetPageQuery.Contains("RootFolder="))
                                        {
                                            targetPageQuery = targetPageQuery.Replace(targetPageQuery.Substring(targetPageQuery.IndexOf("RootFolder="),
                                                targetPageQuery.IndexOf("&", targetPageQuery.IndexOf("RootFolder=")) - targetPageQuery.IndexOf("RootFolder=")), "RootFolder=" + targetPageName);

                                            if (targetPageQuery.IndexOf("RootFolder=") < targetPageQuery.LastIndexOf("RootFolder="))
                                            {
                                                targetPageQuery = targetPageQuery.Replace(targetPageQuery.Substring(targetPageQuery.LastIndexOf("RootFolder="),
                                                    targetPageQuery.IndexOf("&", targetPageQuery.LastIndexOf("RootFolder=")) - targetPageQuery.LastIndexOf("RootFolder=")), "RootFolder=" + targetPageName);
                                            }
                                        }

                                        if ((discussionParentIdLinkItems.Count != 0) && targetPageQuery.Contains("&") && targetPageQuery.Contains("DiscussionParentID="))
                                        {
                                            string discussionParentIdString = discussionParentIdLinkItems[languageItem.LanguageDestination];

                                            targetPageQuery = targetPageQuery.Replace(targetPageQuery.Substring(targetPageQuery.IndexOf("DiscussionParentID="),
                                                targetPageQuery.IndexOf("&", targetPageQuery.IndexOf("DiscussionParentID=")) - targetPageQuery.IndexOf("DiscussionParentID=")), "DiscussionParentID=" + discussionParentIdString);
                                        }

                                        currentUrl = HttpContext.Current.Request.Url.ToString().Remove(HttpContext.Current.Request.Url.ToString().IndexOf('?'));

                                        if (isDiscussionBoard && (HttpContext.Current.Request.Url.ToString().ToLower().Contains("?rootfolder=")
                                        || HttpContext.Current.Request.Url.ToString().ToLower().Contains("&rootfolder=")))
                                        {
                                            banner += "<td class='ms-globallinks-translation" + bannerCssClass +
                                                  " ms-globallinks'  nowrap valign=middle><a href=\"javascript:OnSelectionChange(" + languageItem.Lcid +
                                          ");\" class=\"ms-SPLink ms-hovercellinactive\" onmouseover=\"this.className='ms-SPLink ms-hovercellactive cursor'\" onmouseout=\"this.className='ms-SPLink ms-hovercellinactive'\" >" +
                                          displayLanguage + bannerPipe + "</a>&nbsp;&nbsp;&nbsp; ";
                                        }
                                        else
                                        {
                                            banner += "<td class='ms-globallinks-translation" + bannerCssClass + " ms-globallinks'  nowrap valign=middle><a href=\"" + currentUrl + targetPageQuery
                                                + "\" class=\"ms-SPLink ms-hovercellinactive\" onmouseover=\"this.className='ms-SPLink ms-hovercellactive cursor'\" onmouseout=\"this.className='ms-SPLink ms-hovercellinactive'\" >"
                                                + displayLanguage + bannerPipe + "</a>&nbsp;&nbsp;&nbsp; ";
                                        }
                                    }
                                    else
                                    {
                                        banner += "<td class='ms-globallinks-translation" + bannerCssClass
                                                  +
                                                  " ms-globallinks'  nowrap valign=middle><a href=\"javascript:OnSelectionChange("
                                                  + languageItem.Lcid + ");"
                                                  +
                                                  "\" class=\"ms-SPLink ms-hovercellinactive\" onmouseover=\"this.className='ms-SPLink ms-hovercellactive cursor'\" onmouseout=\"this.className='ms-SPLink ms-hovercellinactive'\" >"
                                                  + displayLanguage + bannerPipe + "</a>&nbsp;&nbsp;&nbsp; ";
                                    }
                            }
                            else
                            {
                                banner += "<td class='ms-globallinks-translation" + bannerCssClass +
                                                  " ms-globallinks'  nowrap valign=middle><a href=\"javascript:OnSelectionChange(" + languageItem.Lcid +
                                          ");\" class=\"ms-SPLink ms-hovercellinactive\" onmouseover=\"this.className='ms-SPLink ms-hovercellactive cursor'\" onmouseout=\"this.className='ms-SPLink ms-hovercellinactive'\" >" +
                                          displayLanguage + bannerPipe + "</a>&nbsp;&nbsp;&nbsp; ";
                            }

                            // Tag for master page
                            RemoveTranslatedTr(tempResponse, "SPS_STATIC_LNG_MASTERPAGE_" + languageItem.LanguageDestination);

                            if (!viewAllItemsInEveryLanguages)
                            {
                                if (tempResponse.IndexOf("action=\"NewForm.aspx") < 0 && tempResponse.IndexOf("action=\"EditForm.aspx") < 0 &&
                                    tempResponse.IndexOf("action=\"ViewEdit.aspx") < 0 && tempResponse.IndexOf("action=\"EditPost.aspx") < 0
                                        && !isPageInEditMode)
                                {
                                    HideItemCounterForGroupByFilter(tempResponse);
                                    RemoveTranslatedTr(tempResponse, "SPS_LNG_" + languageItem.LanguageDestination);
                                }

                                if (!isPageInEditMode)
                                {
                                    RemoveTranslatedDiv(tempResponse, "SPS_STATIC_LNG_" + languageItem.LanguageDestination);
                                    RemoveTranslatedTr(tempResponse, "SPS_STATIC_LNG_" + languageItem.LanguageDestination);
                                }
                            }

                            if (spsWebpartLanguageList.Contains("_SPS_WEBPART_" + languageItem.LanguageDestination))
                                UpdateMsoMenuWebPartMenu(tempResponse, "_SPS_WEBPART_" + languageItem.LanguageDestination, lang,
                                                         allWebPartsContentDisable, webPartHashTable);
                        }
                        else
                        {
                            if (bannerCssClassIsActivated)
                                bannerCssClassCurrentLang = "-currentLanguage";
                            RemoveUntil(tempResponse, "_SPS_WEBPART_" + languageItem.LanguageDestination, "MSOTlPn_TlPnCaptionSpan");
                            banner += "<td class='ms-globallinks-translation" + bannerCssClassCurrentLang +
                                      " ms-globallinks'  nowrap valign=middle><a onclick=\"window.location.search = changeLocation(window.location.search,'SPSLanguage','" +
                                      languageItem.LanguageDestination +
                                      "');\" class=\"ms-SPLink ms-hovercellinactive\" onmouseover=\"this.className='ms-SPLink ms-hovercellactive cursor'\" onmouseout=\"this.className='ms-SPLink ms-hovercellinactive'\" ><b>" +
                                      displayLanguage + "</b>" + bannerPipe + "</a>&nbsp;&nbsp;&nbsp;";
                        }
                    }
                    else
                    {
                        string sharePointItemLanguageToRemove = "<option value=\"SPS_LNG_" + languageItem.LanguageDestination +
                                                                "\">SPS_LNG_" + languageItem.LanguageDestination + "</option>";
                        tempResponse.Replace(sharePointItemLanguageToRemove, string.Empty);
                    }
                }

                if (filteringDisplayCompleted)
                {
                    foreach (Match match in TranslatorRegex.FilteringChangeLanguageRegex.Matches(banner))
                    {
                        banner = banner.Replace(match.Value,
                                                "href=\"" + HttpContext.Current.Request.Path + "?SPSLanguage=" +
                                                match.Groups["language"].Value + "\"");
                    }
                }

                const string UniquePlaceHolderId = "#SharePoint_Translation_Placeholder#";
                const string UniquePlaceHolderIdHidden =
                    "<span style=\"display:none\">#SharePoint_Translation_Placeholder#</span>";

                if ((!HttpContext.Current.Request.Path.EndsWith("/iframe.aspx", StringComparison.OrdinalIgnoreCase)
                     && (tempResponse.IndexOf(UniquePlaceHolderId) == -1))
                    &&
                    (((((HttpContext.Current.Request.Url.ToString().IndexOf("EditForm.aspx") > -1)
                        && (HttpContext.Current.Request.Url.ToString().IndexOf("/Pages/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) == -1)) ||
                            ((HttpContext.Current.Request.Url.ToString().IndexOf("NewForm.aspx") > -1) && (!isDiscussionBoard)) ||
                            (HttpContext.Current.Request.Url.ToString().IndexOf("DispForm.aspx") > -1) ||
                            (HttpContext.Current.Request.Url.ToString().IndexOf("EditPost.aspx") > -1) ||
                            (HttpContext.Current.Request.Url.ToString().IndexOf("/Post.aspx") > -1)) && IsListUseDialogForms()) ||
                             (!languagePackMsActivated)))
                {
                    if (tempResponse.IndexOf(UniquePlaceHolderId) == -1)
                    {
                        if ((tempResponse.IndexOf("</head>") < tempResponse.IndexOf("</HEAD>") &&
                             (tempResponse.IndexOf("</head>") != -1)) || (tempResponse.IndexOf("</HEAD>") == -1))
                        {
                            ReplaceFirstOccurrence(tempResponse, "</head>",
                                                   "</head>" + changeLocationScript +
                                                   "<style type=\"text/css\"><!-- .cursor {  cursor: hand} --> </style><table class='ms-globalbreadcrumb-translation ms-globalbreadcrumb' width=\"100%\" border=0 cellpadding=0 cellspacing=0><tr><td width=\"100%\" valign=center align=\"left\" nowrap><a></a></td>" +
                                                   banner + "</tr><!--" + _translatorVersion + "--></table>");
                        }
                        else
                        {
                            ReplaceFirstOccurrence(tempResponse, "</HEAD>",
                                                   "</HEAD>" + changeLocationScript +
                                                   "<style type=\"text/css\"><!-- .cursor {  cursor: hand} --> </style><table class='ms-globalbreadcrumb-translation ms-globalbreadcrumb' width=\"100%\" border=0 cellpadding=0 cellspacing=0><tr><td width=\"100%\" valign=center align=\"left\" nowrap><a></a></td>" +
                                                   banner + "</tr><!-- " + _translatorVersion + "--></table>");
                        }
                    }
                }
                else
                {
                    if (((tempResponse.IndexOf(UniquePlaceHolderId) > -1) && (HttpContext.Current.Request.Url.ToString().IndexOf("NewForm.aspx") > -1) && (!isDiscussionBoard))
                        || ((tempResponse.IndexOf(UniquePlaceHolderId) > -1) && (HttpContext.Current.Request.Url.ToString().IndexOf("NewForm.aspx") == -1)))
                    {
                        tempResponse.Replace(UniquePlaceHolderIdHidden,
                                             changeLocationScript +
                                             "<style type=\"text/css\"><!-- .cursor {  cursor: hand} --> </style><table class='ms-globalbreadcrumb-translation ms-globalbreadcrumb' width=\"100%\" border=0 cellpadding=0 cellspacing=0><tr><td width=\"100%\" valign=center align=\"left\" nowrap><a></a></td>" +
                                             banner + "</tr><!-- " + _translatorVersion + "--></table>");
                        tempResponse.Replace(UniquePlaceHolderId,
                                             changeLocationScript +
                                             "<style type=\"text/css\"><!-- .cursor {  cursor: hand} --> </style><table class='ms-globalbreadcrumb-translation ms-globalbreadcrumb' width=\"100%\" border=0 cellpadding=0 cellspacing=0><tr><td width=\"100%\" valign=center align=\"left\" nowrap><a></a></td>" +
                                             banner + "</tr><!-- " + _translatorVersion + "--></table>");
                    }
                    else
                    {
                        if ((tempResponse.IndexOf("</head>") < tempResponse.IndexOf("</HEAD>") &&
                         (tempResponse.IndexOf("</head>") != -1)) || (tempResponse.IndexOf("</HEAD>") == -1))
                        {
                            ReplaceFirstOccurrence(tempResponse, "</head>",
                                                   "</head>" + changeLocationScript + "\n<!--" + _translatorVersion +
                                                   "-->\n");
                        }
                        else
                        {
                            ReplaceFirstOccurrence(tempResponse, "</HEAD>",
                                                   "</HEAD>" + changeLocationScript + "\n<!--" + _translatorVersion +
                                                   "-->\n");
                        }
                    }
                }

                if (tempResponse.IndexOf(FunctionSelectionChangeInitialDefinition) > -1)
                {
                    string functionSelectionChangeNewDefinition = FunctionSelectionChangeInitialDefinition;
                    string partToAddInDefinition = string.Empty;

                    partToAddInDefinition += "var search=window.location.search;\n";
                    partToAddInDefinition +=
                        "var pos=search.indexOf('SPS_Trans_Code=Completing_Dictionary_Mode_Process2');\n";
                    partToAddInDefinition += "var ref=window.location.href;\n";

                    foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                    {
                        partToAddInDefinition += "	if (value == \"" + languageItem.Lcid + "\")\n";
                        partToAddInDefinition += "	{\n";

                        if (linkItemsId.ContainsKey(languageItem.LanguageDestination))
                        {
                            string targetPageName = linkItemsId[languageItem.LanguageDestination].Replace("'", "%27");
                            string currentUrl = HttpContext.Current.Request.Url.ToString();
                            string targetPageQuery = string.Empty;
                            if (currentUrl.Contains("?"))
                            {
                                targetPageQuery = currentUrl.Substring(currentUrl.IndexOf('?'));
                                targetPageQuery = targetPageQuery.Replace("?SPSLanguage=" + lang,
                                                                          "?SPSLanguage=" + languageItem.LanguageDestination);
                                targetPageQuery = targetPageQuery.Replace("&SPSLanguage=" + lang,
                                                                          "&SPSLanguage=" + languageItem.LanguageDestination);
                                targetPageQuery = targetPageQuery.Replace("?SPSLanguage=" + "XX&", "?");
                                targetPageQuery = targetPageQuery.Replace("?SPSLanguage=" + "XX",
                                                                          string.Empty);
                                targetPageQuery = targetPageQuery.Replace("&SPSLanguage=" + "XX",
                                                                          string.Empty);
                                if ((HttpContext.Current.Request.Url.ToString()
                                        .IndexOf("/Pages/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) >
                                    -1) || (languagePackMsActivated && !IsListUseDialogForms()))
                                {
                                    if (targetPageQuery.IndexOf("?ID=", StringComparison.OrdinalIgnoreCase) > -1)
                                    {
                                        if (targetPageQuery.IndexOf("&") > -1)
                                        {
                                            targetPageQuery =
                                                targetPageQuery.Remove(
                                                    targetPageQuery.IndexOf("?ID=",
                                                                            StringComparison.OrdinalIgnoreCase) + 1,
                                                    targetPageQuery.IndexOf("&", StringComparison.OrdinalIgnoreCase) -
                                                    targetPageQuery.IndexOf("?ID=",
                                                                            StringComparison.OrdinalIgnoreCase) - 1);
                                        }
                                        else
                                        {
                                            targetPageQuery =
                                                targetPageQuery.Remove(
                                                    targetPageQuery.IndexOf("?ID=",
                                                                            StringComparison.OrdinalIgnoreCase) + 1);
                                        }
                                    }

                                    targetPageQuery = targetPageQuery.Replace("?", "?ID=" + targetPageName);
                                }
                            }
                            else
                            {
                                if ((HttpContext.Current.Request.Url.ToString()
                                        .IndexOf("/Pages/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) >
                                    -1) || (languagePackMsActivated && !IsListUseDialogForms()))
                                {
                                    targetPageQuery = "?ID=" + targetPageName;
                                }
                            }

                            if ((HttpContext.Current.Request.Url.ToString()
                                    .IndexOf("/Pages/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) >
                                    -1) || (languagePackMsActivated && !IsListUseDialogForms()))
                            {
                                if (currentUrl.IndexOf("?") > -1)
                                {
                                    currentUrl =
                                        HttpContext.Current.Request.Url.ToString().Remove(
                                            HttpContext.Current.Request.Url.ToString().IndexOf('?'));
                                }
                            }
                            else
                            {
                                if (isDiscussionBoard && (HttpContext.Current.Request.Url.ToString().ToLower().Contains("?rootfolder=")
                                    || HttpContext.Current.Request.Url.ToString().ToLower().Contains("&rootfolder=")))
                                {
                                    currentUrl =
                                        HttpContext.Current.Request.Url.ToString().Remove(
                                            HttpContext.Current.Request.Url.ToString().LastIndexOf('?'));

                                    if (targetPageQuery.ToLower().Contains("?rootfolder="))
                                    {
                                        if (targetPageQuery.ToLower().IndexOf("&", targetPageQuery.IndexOf("?")) > -1)
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.IndexOf("?"), targetPageQuery.ToLower().IndexOf("&", targetPageQuery.IndexOf("?")) - targetPageQuery.IndexOf("?"));
                                            targetPageQuery = targetPageQuery.Insert(targetPageQuery.IndexOf("&"), "?RootFolder=" + targetPageName);
                                        }
                                        else
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.IndexOf("?")) + "?RootFolder=" + targetPageName;
                                        }
                                    }
                                    else
                                    {
                                        if (targetPageQuery.ToLower().IndexOf("&", targetPageQuery.ToLower().IndexOf("&rootfolder=") + 1) > -1)
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.ToLower().IndexOf("&rootfolder"), targetPageQuery.ToLower().IndexOf("&", targetPageQuery.ToLower().IndexOf("&rootfolder"))
                                                - targetPageQuery.ToLower().IndexOf("&rootfolder"));
                                            targetPageQuery += "&RootFolder=" + targetPageName;
                                        }
                                        else
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.ToLower().IndexOf("&rootfolder=")) + "&RootFolder=" + targetPageName;
                                        }
                                    }
                                }
                                else
                                {
                                    currentUrl =
                                        HttpContext.Current.Request.Url.ToString().Remove(
                                            HttpContext.Current.Request.Url.ToString().LastIndexOf('/') + 1) +
                                        targetPageName;
                                }
                            }

                            partToAddInDefinition += "		ref = '" + currentUrl + targetPageQuery + "';";
                            partToAddInDefinition += "		if(pos>=0)\n";
                            partToAddInDefinition += "		{\n";
                            partToAddInDefinition +=
                                "			ref = changeLocation(ref,'SPS_Trans_Code','Completing_Dictionary_Mode_OFF');\n";
                            partToAddInDefinition += "		}\n";
                            partToAddInDefinition += "		window.location.href = ref;";
                        }
                        else
                        {
                            partToAddInDefinition += "		if(pos>=0)\n";
                            partToAddInDefinition += "		{\n";
                            partToAddInDefinition +=
                                "			search = changeLocation(search,'SPS_Trans_Code','Completing_Dictionary_Mode_OFF');\n";
                            partToAddInDefinition += "		}\n";
                            partToAddInDefinition += "		window.location.href = url;";
                        }

                        partToAddInDefinition += "	}\n";
                    }

                    functionSelectionChangeNewDefinition =
                        functionSelectionChangeNewDefinition.Replace("	window.location.href = url;",
                                                                     partToAddInDefinition);
                    tempResponse.Replace(FunctionSelectionChangeInitialDefinition,
                                         functionSelectionChangeNewDefinition);
                }

                if (tempResponse.IndexOf(FunctionSelectionChangeInitialDefinition2) > -1)
                {
                    string functionSelectionChangeNewDefinition = FunctionSelectionChangeInitialDefinition2;
                    string partToAddInDefinition = string.Empty;

                    partToAddInDefinition += "var search=window.location.search;\n";
                    partToAddInDefinition +=
                        "var pos=search.indexOf('SPS_Trans_Code=Completing_Dictionary_Mode_Process2');\n";
                    partToAddInDefinition += "var ref=window.location.href;\n";

                    foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                    {
                        partToAddInDefinition += "	if (value == \"" + languageItem.Lcid + "\")\n";
                        partToAddInDefinition += "	{\n";

                        if (linkItemsId.ContainsKey(languageItem.LanguageDestination))
                        {
                            string targetPageName = linkItemsId[languageItem.LanguageDestination].Replace("'", "%27");
                            string currentUrl = HttpContext.Current.Request.Url.ToString();
                            string targetPageQuery = string.Empty;
                            if (currentUrl.Contains("?"))
                            {
                                targetPageQuery = currentUrl.Substring(currentUrl.IndexOf('?'));
                                targetPageQuery = targetPageQuery.Replace("?SPSLanguage=" + lang,
                                                                          "?SPSLanguage=" + languageItem.LanguageDestination);
                                targetPageQuery = targetPageQuery.Replace("&SPSLanguage=" + lang,
                                                                          "&SPSLanguage=" + languageItem.LanguageDestination);
                                targetPageQuery = targetPageQuery.Replace("?SPSLanguage=" + "XX&", "?");
                                targetPageQuery = targetPageQuery.Replace("?SPSLanguage=" + "XX",
                                                                          string.Empty);
                                targetPageQuery = targetPageQuery.Replace("&SPSLanguage=" + "XX",
                                                                          string.Empty);
                                if ((HttpContext.Current.Request.Url.ToString()
                                        .IndexOf("/Pages/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) >
                                    -1) || (languagePackMsActivated && !IsListUseDialogForms()))
                                {
                                    if (targetPageQuery.IndexOf("?ID=", StringComparison.OrdinalIgnoreCase) > -1)
                                    {
                                        if (targetPageQuery.IndexOf("&") > -1)
                                        {
                                            targetPageQuery =
                                                targetPageQuery.Remove(
                                                    targetPageQuery.IndexOf("?ID=",
                                                                            StringComparison.OrdinalIgnoreCase) + 1,
                                                    targetPageQuery.IndexOf("&", StringComparison.OrdinalIgnoreCase) -
                                                    targetPageQuery.IndexOf("?ID=",
                                                                            StringComparison.OrdinalIgnoreCase) - 1);
                                        }
                                        else
                                        {
                                            targetPageQuery =
                                                targetPageQuery.Remove(
                                                    targetPageQuery.IndexOf("?ID=",
                                                                            StringComparison.OrdinalIgnoreCase) + 1);
                                        }
                                    }

                                    targetPageQuery = targetPageQuery.Replace("?", "?ID=" + targetPageName);
                                }
                            }
                            else
                            {
                                if ((HttpContext.Current.Request.Url.ToString()
                                        .IndexOf("/Pages/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) >
                                    -1) || (languagePackMsActivated && !IsListUseDialogForms()))
                                {
                                    targetPageQuery = "?ID=" + targetPageName;
                                }
                            }

                            if ((HttpContext.Current.Request.Url.ToString()
                                    .IndexOf("/Pages/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) > -1) || (languagePackMsActivated && !IsListUseDialogForms()))
                            {
                                if (currentUrl.IndexOf("?") > -1)
                                {
                                    currentUrl =
                                        HttpContext.Current.Request.Url.ToString().Remove(
                                            HttpContext.Current.Request.Url.ToString().IndexOf('?'));
                                }
                            }
                            else
                            {
                                if (isDiscussionBoard && (HttpContext.Current.Request.Url.ToString().ToLower().Contains("?rootfolder=")
                                    || HttpContext.Current.Request.Url.ToString().ToLower().Contains("&rootfolder=")))
                                {
                                    currentUrl =
                                        HttpContext.Current.Request.Url.ToString().Remove(
                                            HttpContext.Current.Request.Url.ToString().LastIndexOf('?'));

                                    if (targetPageQuery.ToLower().Contains("?rootfolder="))
                                    {
                                        if (targetPageQuery.ToLower().IndexOf("&", targetPageQuery.IndexOf("?")) > -1)
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.IndexOf("?"), targetPageQuery.ToLower().IndexOf("&", targetPageQuery.IndexOf("?")) - targetPageQuery.IndexOf("?"));
                                            targetPageQuery = targetPageQuery.Insert(targetPageQuery.IndexOf("&"), "?RootFolder=" + targetPageName);
                                        }
                                        else
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.IndexOf("?")) + "?RootFolder=" + targetPageName;
                                        }
                                    }
                                    else
                                    {
                                        if (targetPageQuery.ToLower().IndexOf("&", targetPageQuery.ToLower().IndexOf("&rootfolder=") + 1) > -1)
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.ToLower().IndexOf("&rootfolder"), targetPageQuery.ToLower().IndexOf("&", targetPageQuery.ToLower().IndexOf("&rootfolder"))
                                                - targetPageQuery.ToLower().IndexOf("&rootfolder"));
                                            targetPageQuery += "&RootFolder=" + targetPageName;
                                        }
                                        else
                                        {
                                            targetPageQuery = targetPageQuery.Remove(targetPageQuery.ToLower().IndexOf("&rootfolder=")) + "&RootFolder=" + targetPageName;
                                        }
                                    }
                                }
                                else
                                {
                                    currentUrl =
                                        HttpContext.Current.Request.Url.ToString().Remove(
                                            HttpContext.Current.Request.Url.ToString().LastIndexOf('/') + 1) +
                                        targetPageName;
                                }
                            }

                            partToAddInDefinition += "		ref = '" + currentUrl + targetPageQuery + "';";
                            partToAddInDefinition += "		if(pos>=0)\n";
                            partToAddInDefinition += "		{\n";
                            partToAddInDefinition +=
                                "			ref = changeLocation(ref,'SPS_Trans_Code','Completing_Dictionary_Mode_OFF');\n";
                            partToAddInDefinition += "		}\n";
                            partToAddInDefinition += "		window.location.href = ref;";
                        }
                        else
                        {
                            partToAddInDefinition += "		if(pos>=0)\n";
                            partToAddInDefinition += "		{\n";
                            partToAddInDefinition +=
                                "			search = changeLocation(search,'SPS_Trans_Code','Completing_Dictionary_Mode_OFF');\n";
                            partToAddInDefinition += "		}\n";
                            partToAddInDefinition += "		window.location.href = url;";
                        }

                        partToAddInDefinition += "	}\n";
                    }

                    functionSelectionChangeNewDefinition =
                        functionSelectionChangeNewDefinition.Replace("    window.location.href = url;",
                                                                     partToAddInDefinition);
                    tempResponse.Replace(FunctionSelectionChangeInitialDefinition2,
                                         functionSelectionChangeNewDefinition);
                }

                // If we are not in creation mode or edit mode, hidde language information
                string currentRequestUrl = HttpContext.Current.Request.Url.ToString();

                bool editMode = IsEditPageMode() ||
                                currentRequestUrl.IndexOf("newform.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("editform.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("editpost.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("listedit.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("viewedit.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("viewnew.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("managecontenttype.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("changefieldorder.aspx", StringComparison.OrdinalIgnoreCase) != -1 ||
                                currentRequestUrl.IndexOf("/_layouts/", StringComparison.OrdinalIgnoreCase) != -1 ||
                                tempResponse.IndexOf("action=\"EditForm.aspx") != -1 ||
                                tempResponse.IndexOf("action=\"ViewEdit.aspx") != -1;

                RemoveSpsTagsBeforeAddingMenus(tempResponse, viewAllItemsInEveryLanguages, editMode);

                // Add menu option
                AddMenuOption(tempResponse, lang, completingDictionaryMode, languageSource, Alphamosaik.Common.SharePoint.Library.Utilities.IsUserHaveEditRight());

                AddHyperlinksInSiteSettings(tempResponse);

                RemoveSpsWebPartAfterAddingMenus(tempResponse);

                RemoveSpsTagsAfterAddingMenus(tempResponse, allWebPartsContentDisable, spsContentExist, webPartHashTable);

                AddEditPageOption(tempResponse);

                ReplaceLanguageMetadataLabel(tempResponse);

                AddMenuOptionExtractorTranslations(tempResponse);

                if (licenseType != License.LicenseType.Prod && currentRequestUrl.IndexOf("/_layouts/", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    int infoTagIndex = tempResponse.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                    if (infoTagIndex > -1)
                    {
                        string message = "- TRIAL edition - www.oceanik.com";

                        if (licenseType == License.LicenseType.Dev)
                        {
                            message = "- DEV edition unauthorized use in Production - www.oceanik.com";
                        }
                        else if (licenseType == License.LicenseType.Unlicensed)
                        {
                            message = "- Unlicensed edition unauthorized use in Production - www.oceanik.com";
                        }

                        tempResponse.Insert(infoTagIndex,
                                            "\n<div style=\"z-index: 1; position: absolute; width:50%; background: #001057;color:white;opacity: 0.1;-moz-opacity: 0.7;-khtml-opacity: 0.7;filter: alpha(opacity=70); right: 25%; left: 25%; bottom:0px; height: 20px;\"><table width=\"100%\"><tr><td align=\"center\">" +
                                            "AlphaMosaïk SharePoint " + _translatorVersion + message +
                                            "</td></tr></table></div>\n");
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("AddHelper", e, EventLogEntryType.Warning);
            }
        }

        public bool IsListUseDialogForms()
        {
            try
            {
                return !SPContext.Current.List.NavigateForFormsPages;
            }
            catch (Exception ex)
            {
                Utilities.TraceNormalCaughtException("IsListUseDialogForms", ex);
            }

            return true;
        }

        public bool IsEditPageMode()
        {
            try
            {
                if (SPContext.Current != null && SPContext.Current.FormContext != null && SPContext.Current.FormContext.FormMode == Microsoft.SharePoint.WebControls.SPControlMode.Edit)
                {
                    return true;
                }

                string webPartManagerDisplayModeNameValue = HttpContext.Current.Request.Form["MSOSPWebPartManager_DisplayModeName"];

                if (!string.IsNullOrEmpty(webPartManagerDisplayModeNameValue))
                {
                    if (webPartManagerDisplayModeNameValue.IndexOf("Design", StringComparison.OrdinalIgnoreCase) != -1 ||
                        webPartManagerDisplayModeNameValue.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        string webPartManagerStartEditNameValue = HttpContext.Current.Request.Form["MSOSPWebPartManager_StartWebPartEditingName"];

                        if (!string.IsNullOrEmpty(webPartManagerStartEditNameValue) &&
                            webPartManagerStartEditNameValue.IndexOf("true", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public override void ListFilteringDisplay(StringBuilder tempResponse, string lang, ref bool completed)
        {
            if (HttpContext.Current != null)
            {
                if (SPContext.Current.List != null)
                {
                    if (!SPContext.Current.List.Fields.ContainsField("SharePoint_Item_Language"))
                        return;

                    string viewName;

                    if (TranslatorRegex.SelectedViewRegex.IsMatch(tempResponse.ToString()))
                        viewName =
                            TranslatorRegex.SelectedViewRegex.Match(tempResponse.ToString()).Groups["viewName"].Value.Trim();
                    else
                        return;

                    SPView currentView = SPContext.Current.List.Views[viewName];

                    string currentViewQuery = currentView.Query;
                    if (currentViewQuery.ToLower().IndexOf("<where>") == -1)
                        currentViewQuery +=
                            "<Where><Or><Or><Eq><FieldRef Name=\"SharePoint_Item_Language\" /><Value Type=\"Text\">SPS_LNG_" +
                            lang +
                            "</Value></Eq><Eq><FieldRef Name=\"SharePoint_Item_Language\" /><Value Type=\"Text\">(SPS_LNG_ALL)</Value></Eq></Or><IsNull><FieldRef Name=\"SharePoint_Item_Language\" /></IsNull></Or></Where>";
                    else
                    {
                        currentViewQuery = currentViewQuery.Insert(currentViewQuery.ToLower().IndexOf("<where>") + 7,
                                                                   "<And><Or><Or><Eq><FieldRef Name=\"SharePoint_Item_Language\" /><Value Type=\"Text\">SPS_LNG_" +
                                                                   lang +
                                                                   "</Value></Eq><Eq><FieldRef Name=\"SharePoint_Item_Language\" /><Value Type=\"Text\">(SPS_LNG_ALL)</Value></Eq></Or><IsNull><FieldRef Name=\"SharePoint_Item_Language\" /></IsNull></Or>");
                        currentViewQuery = currentViewQuery.Insert(currentViewQuery.ToLower().IndexOf("</where>"), "</And>");
                    }

                    if (currentViewQuery.ToLower().IndexOf("<groupby ") > -1)
                        return;

                    currentView.Query = currentViewQuery;

                    string viewAsHtml = currentView.RenderAsHtml();

                    int indexBeginTable = tempResponse.IndexOf("ctx = new ContextInfo();");
                    if (indexBeginTable == -1)
                        return;

                    int tableBeforeViewAsHtmlIndex = tempResponse.LastIndexOf("</table>", indexBeginTable);

                    indexBeginTable = tempResponse.LastIndexOf("<TABLE ", indexBeginTable);
                    if (indexBeginTable == -1)
                        return;

                    if (tableBeforeViewAsHtmlIndex > indexBeginTable)
                        return;

                    int indexEndTable = tempResponse.IndexOf("</PlaceHolder>", indexBeginTable);
                    if (indexEndTable == -1)
                        return;

                    indexEndTable = tempResponse.LastIndexOf("</TABLE>", indexEndTable);
                    if (indexEndTable == -1)
                        return;

                    indexEndTable += "</TABLE>".Length;

                    foreach (Match match in TranslatorRegex.GroupByInViewAsHtmlRegex.Matches(viewAsHtml))
                    {
                        if (tempResponse.IndexOf("/>" + match.Value) != -1)
                        {
                            int matchInTempResponseBeginIndex = tempResponse.LastIndexOf("<input type=\"hidden\" id=",
                                                                                         tempResponse.IndexOf("/>" +
                                                                                                              match.Value));

                            if (matchInTempResponseBeginIndex > -1)
                            {
                                string matchInTempResponse = tempResponse.Substring(matchInTempResponseBeginIndex,
                                                                                    tempResponse.IndexOf("/>" + match.Value) -
                                                                                    matchInTempResponseBeginIndex + 2);
                                viewAsHtml = viewAsHtml.Replace(match.Value, matchInTempResponse + match.Value);
                            }
                        }
                    }

                    int urlPageToReplaceIndexEnd;
                    string urlPageToReplace = string.Empty;
                    int urlPageToReplaceIndexBegin = viewAsHtml.LastIndexOf("OnClick='javascript:SubmitFormPost(\"") + 36;
                    if (urlPageToReplaceIndexBegin > 35)
                    {
                        urlPageToReplaceIndexEnd = viewAsHtml.IndexOf("?", urlPageToReplaceIndexBegin);

                        if (urlPageToReplaceIndexEnd > -1)
                            urlPageToReplace = viewAsHtml.Substring(urlPageToReplaceIndexBegin,
                                                                    urlPageToReplaceIndexEnd - urlPageToReplaceIndexBegin);
                        if (!string.IsNullOrEmpty(urlPageToReplace))
                            viewAsHtml = viewAsHtml.Replace(urlPageToReplace, HttpContext.Current.Request.Url.AbsolutePath);
                    }

                    if (string.IsNullOrEmpty(urlPageToReplace))
                    {
                        urlPageToReplaceIndexBegin = viewAsHtml.LastIndexOf("onclick=\"javascript:EnterFolder('") + 33;

                        if (urlPageToReplaceIndexBegin > 33)
                        {
                            urlPageToReplaceIndexEnd = viewAsHtml.IndexOf("?", urlPageToReplaceIndexBegin);

                            if (urlPageToReplaceIndexEnd > -1)
                                urlPageToReplace = viewAsHtml.Substring(urlPageToReplaceIndexBegin,
                                                                        urlPageToReplaceIndexEnd -
                                                                        urlPageToReplaceIndexBegin);
                            if (!string.IsNullOrEmpty(urlPageToReplace))
                                viewAsHtml = viewAsHtml.Replace(urlPageToReplace,
                                                                HttpContext.Current.Request.Url.AbsolutePath);
                        }

                        foreach (Match match in TranslatorRegex.UrlPageToReplaceRegex.Matches(viewAsHtml))
                        {
                            viewAsHtml = viewAsHtml.Replace(match.Value,
                                                            " HREF=\"" + HttpContext.Current.Request.Url.AbsolutePath + "?");
                        }

                        if (TranslatorRegex.UrlPageToReplaceRegex.IsMatch(viewAsHtml))
                        {
                            urlPageToReplaceIndexEnd = viewAsHtml.IndexOf("?", urlPageToReplaceIndexBegin);

                            if (urlPageToReplaceIndexEnd > -1)
                                urlPageToReplace = viewAsHtml.Substring(urlPageToReplaceIndexBegin,
                                                                        urlPageToReplaceIndexEnd -
                                                                        urlPageToReplaceIndexBegin);
                            if (!string.IsNullOrEmpty(urlPageToReplace))
                                viewAsHtml = viewAsHtml.Replace(urlPageToReplace,
                                                                HttpContext.Current.Request.Url.AbsolutePath);
                        }
                    }

                    int topPagingIndexBegin = tempResponse.IndexOf("var topPagingCell = document.getElementById(\"") + 45;

                    int topPagingIndexEnd = -1;
                    if (topPagingIndexBegin != 44)
                        topPagingIndexEnd = tempResponse.IndexOf("\"", topPagingIndexBegin);

                    if (topPagingIndexEnd != -1)
                        viewAsHtml = viewAsHtml.Replace("\"topPagingCell\"",
                                                        "\"" +
                                                        tempResponse.Substring(topPagingIndexBegin,
                                                                               topPagingIndexEnd - topPagingIndexBegin) +
                                                        "\"");

                    completed = true;

                    tempResponse.Replace(tempResponse.Substring(indexBeginTable, indexEndTable - indexBeginTable),
                                         viewAsHtml);
                }
            }
        }

        public void AddMenuOptionExtractorTranslations(StringBuilder tempResponse)
        {
            if ((SPContext.Current.List != null) && (SPContext.Current.List.Title == "ExtractorTranslations"))
            {
                string listId = SPContext.Current.List.ID.ToString();
                string valueRegex = TranslatorRegex.SendExtractorTranslationsToDictionaryRegex.Match(tempResponse.ToString()).Value;

                if (!string.IsNullOrEmpty(listId))
                {
                    string menuSendExtractorTranslationsToDictionary = "<ie:menuitem id=\"SPS_MenuEnableItemTradFromList\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";
                    string sendExtractorTranslationsToDictionaryAction = @"javascript: if(confirm('Are you sure you want to copy the items of this list to the dictionary ?')) window.location.search = '?listForItemLanguageId=" + listId + "&SPS_Trans_Code=SendExtractorListToDictionary';";

                    menuSendExtractorTranslationsToDictionary += "onMenuClick=\"" + sendExtractorTranslationsToDictionaryAction + "\" ";
                    menuSendExtractorTranslationsToDictionary += "text=\"Send all the items of this list in the dictionary\" description=\"Complete the dictionary with content of this list.\" menuGroupId=\"100\">";

                    menuSendExtractorTranslationsToDictionary += "</ie:menuitem>" + Environment.NewLine;

                    if (!string.IsNullOrEmpty(valueRegex))
                        tempResponse.Replace(valueRegex, valueRegex + menuSendExtractorTranslationsToDictionary);
                }
            }
        }

        public void AddEditPageOption(StringBuilder tempResponse)
        {
            if ((HttpContext.Current.Request.Url.ToString().IndexOf("EditForm.aspx", StringComparison.OrdinalIgnoreCase) > -1) || (HttpContext.Current.Request.Url.ToString().IndexOf("NewForm.aspx", StringComparison.OrdinalIgnoreCase) > -1))
            {
                string valueFieldDisplay = string.Empty;
                string disabledField = string.Empty;
                var currentItem = SPContext.Current.Item as SPListItem;

                bool isDiscussionBoardList = false;
                if (SPContext.Current.List != null)
                    isDiscussionBoardList = (SPContext.Current.List.BaseTemplate == SPListTemplateType.DiscussionBoard)
                        && ((bool)HttpContext.Current.Cache["AlphamosaikDiscussionBoardTranslationOptionsHide"]);

                if (currentItem != null)
                    if (currentItem.Fields.ContainsField("SharePoint_Item_Language"))
                    {
                        if (((string)currentItem["SharePoint_Item_Language"] == "(SPS_LNG_ALL)") || string.IsNullOrEmpty((string)currentItem["SharePoint_Item_Language"]) || (currentItem["SharePoint_Item_Language"] == null))
                            valueFieldDisplay = "none";
                    }

                if (TranslatorRegex.HasSharepointItemLanguageFieldRegex.IsMatch(tempResponse.ToString()) && isDiscussionBoardList)
                {
                    valueFieldDisplay = "none";

                    int positionTrTag = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""SharePoint_Item_Language""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);

                    if (positionTrTag > -1)
                    {
                        tempResponse.Insert(positionTrTag + 3, " style=\"display:none\" ");
                    }

                    int positionSelectTag = tempResponse.IndexOf("<select ", tempResponse.IndexOf(@"<!-- FieldName=""SharePoint_Item_Language""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);

                    if (positionSelectTag > -1)
                    {
                        //tempResponse.Insert(positionSelectTag + 8, "disabled ");
                        //tempResponse.Insert(positionSelectTag + 8, "style=\"display:none\" ");
                    }

                    int positionOptionTag = tempResponse.IndexOf("<option selected=\"selected\" ", tempResponse.IndexOf(@"<!-- FieldName=""SharePoint_Item_Language""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);

                    if (positionOptionTag > -1)
                    {
                        tempResponse.Remove(positionOptionTag, "<option selected=\"selected\" ".Length);
                        tempResponse.Insert(positionOptionTag, "<option ");

                        int positionOptionCurrentLanguageTag = tempResponse.IndexOf("<option value=\"SPS_LNG_" + GetCurrrentLanguageCode() + "\">", tempResponse.IndexOf(@"<!-- FieldName=""SharePoint_Item_Language""",
                            StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);

                        if (positionOptionCurrentLanguageTag > -1)
                        {
                            tempResponse.Remove(positionOptionCurrentLanguageTag, ("<option value=\"SPS_LNG_" + GetCurrrentLanguageCode() + "\">").Length);
                            tempResponse.Insert(positionOptionCurrentLanguageTag, "<option selected=\"selected\" value=\"SPS_LNG_" + GetCurrrentLanguageCode() + "\">");
                        }
                    }
                }

                if (TranslatorRegex.HasSharepointGroupLanguageFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""SharePoint_Group_Language""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:none\"");
                }

                if (TranslatorRegex.HasAutoTranslationFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""AutoTranslation""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:" + valueFieldDisplay + "\" id=\"autotranslationtr\"");

                    if ((positionLastTr > -1) && isDiscussionBoardList)
                    {
                        int positionInputTag = tempResponse.IndexOf("<input ", positionLastTr, StringComparison.OrdinalIgnoreCase);

                        if (positionInputTag > -1)
                        {
                            //tempResponse.Insert(positionInputTag + 7, "disabled ");
                            //tempResponse.Insert(positionInputTag + 7, "style=\"display:none\" ");

                            positionInputTag = tempResponse.IndexOf("<input ", positionInputTag + 7, StringComparison.OrdinalIgnoreCase);

                            if (positionInputTag > -1)
                            {
                                //tempResponse.Insert(positionInputTag + 7, "disabled ");  
                                //tempResponse.Insert(positionInputTag + 7, "style=\"display:none\" ");
                            }
                        }
                    }
                }

                if (TranslatorRegex.HasItemsAutoCreationFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""ItemsAutoCreation""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:" + valueFieldDisplay + "\" id=\"autocreationtr\"");

                    if ((positionLastTr > -1) && isDiscussionBoardList)
                    {
                        int positionInputTag = tempResponse.IndexOf("<input ", positionLastTr, StringComparison.OrdinalIgnoreCase);

                        if (positionInputTag > -1)
                        {
                            //tempResponse.Insert(positionInputTag + 7, "disabled ");
                            //tempResponse.Insert(positionInputTag + 7, "style=\"display:none\" ");

                            positionInputTag = tempResponse.IndexOf("<input ", positionInputTag + 7, StringComparison.OrdinalIgnoreCase);

                            if (positionInputTag > -1)
                            {
                                //tempResponse.Insert(positionInputTag + 7, "disabled ");
                                //tempResponse.Insert(positionInputTag + 7, "style=\"display:none\" ");

                                positionInputTag = tempResponse.IndexOf("<input ", positionInputTag + 7, StringComparison.OrdinalIgnoreCase);

                                if (positionInputTag > -1)
                                {
                                    //tempResponse.Insert(positionInputTag + 7, "disabled ");
                                    //tempResponse.Insert(positionInputTag + 7, "style=\"display:none\" ");
                                }
                            }
                        }
                    }
                }

                if (TranslatorRegex.HasMetadataToDuplicateFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""MetadataToDuplicate""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:none\"");
                }

                var hasProfileFieldRegex = new Regex(@"<!-- FieldName=""Translation Profile""", RegexOptions.IgnoreCase);

                if (hasProfileFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int lastTr = tempResponse.LastIndexOf("<TR>", tempResponse.IndexOf(@"<!-- FieldName=""Translation Profile"""), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(lastTr + 3, " style=\"display:none\"");
                }

                string metadataChoiceJs = string.Empty;

                metadataChoiceJs += "<TR id=\"MetadataToUpdateChoiceLink\"  style=\"display:" + valueFieldDisplay + "\" ><td></td><td valign=\"top\" class=\"ms-formbody\" width=\"400px\"><a href=\"javascript:DisplayDiv('MetadataToUpdateChoice')\" >Define the metadata to duplicate</a></td></TR>\n";
                metadataChoiceJs += "<TR><TD></TD><TD>\n";
                metadataChoiceJs += "<table id=\"MetadataToUpdateChoice\" style=\"display:none\" >\n";

                if (currentItem != null)
                    foreach (SPField field in currentItem.Fields)
                    {
                        if (field.ShowInVersionHistory && (field.InternalName != "ItemsAutoCreation") && (field.InternalName != "AutoTranslation") && (field.InternalName != "SharePoint_Group_Language") && (field.InternalName != "SharePoint_Item_Language") && (field.InternalName != "ID") && (field.InternalName != "Body") && (field.InternalName != "Title"))
                        {
                            string checkedState = string.Empty;

                            if (TranslatorRegex.HasMetadataToDuplicateFieldRegex.IsMatch(tempResponse.ToString()) && (currentItem["MetadataToDuplicate"] != null))
                            {
                                string[] metadataToDuplicateArray = currentItem["MetadataToDuplicate"].ToString().Split(';');
                                foreach (string t in metadataToDuplicateArray)
                                {
                                    if (field.InternalName == t)
                                    {
                                        checkedState = " checked=\"true\"";
                                        break;
                                    }
                                }
                            }

                            metadataChoiceJs += "<TR>\n";
                            metadataChoiceJs += "<TD nowrap=\"true\" valign=\"top\" width=\"190px\" class=\"ms-formlabel\"><H3 class=\"ms-standardheader\">\n";
                            metadataChoiceJs += "<nobr>" + field.Title + "</nobr>\n";
                            metadataChoiceJs += "</H3></TD>\n";
                            metadataChoiceJs += "<TD valign=\"top\" class=\"ms-formbody\">\n";
                            metadataChoiceJs += "<span dir=\"none\">\n";
                            metadataChoiceJs += "<input" + checkedState + " OnChange=\"AddMetadata('" + field.InternalName + "', this.checked)\" id=\"Alpha" + field.InternalName + "\" type=\"checkbox\" name=\"Alpha" + field.InternalName + "\" /><br>\n";
                            metadataChoiceJs += "</span>\n";
                            metadataChoiceJs += "</TD>\n";
                            metadataChoiceJs += "</TR>\n";
                        }
                    }

                metadataChoiceJs += "</table>\n";
                metadataChoiceJs += "</TD></TR>\n";

                if (TranslatorRegex.HasMetadataToDuplicateFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionNextTr = tempResponse.IndexOf("</tr>", tempResponse.IndexOf(@"<!-- FieldName=""MetadataToDuplicate""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionNextTr + 5, metadataChoiceJs);
                }

                if (TranslatorRegex.HasSharepointItemLanguageFieldRegex.IsMatch(tempResponse.ToString()) && TranslatorRegex.HasBodyEndTagRegex.IsMatch(tempResponse.ToString())
                    && TranslatorRegex.HasItemsAutoCreationFieldRegex.IsMatch(tempResponse.ToString()) && TranslatorRegex.HasAutoTranslationFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    bool hasProfileField = hasProfileFieldRegex.IsMatch(tempResponse.ToString());

                    string displayHideTrJs = "<script LANGUAGE='JavaScript' >\n";

                    if (hasProfileField)
                        displayHideTrJs += "function DisplayRow(id1, id2, id3, id4, id5, value)\n";
                    else
                        displayHideTrJs += "function DisplayRow(id1, id2, id3, id4, value)\n";

                    displayHideTrJs += "{\n";
                    displayHideTrJs += "var row1 = document.getElementById(id1);\n";
                    displayHideTrJs += " if ((value == \"(SPS_LNG_ALL)\") || (value == \"\"))\n {\n row1.style.display = 'none';\n }\n";
                    displayHideTrJs += "else {\n row1.style.display = ''; }\n";
                    displayHideTrJs += "var row2 = document.getElementById(id2);\n";
                    displayHideTrJs += " if ((value == \"(SPS_LNG_ALL)\") || (value == \"\"))\n {\n row2.style.display = 'none';\n }\n";
                    displayHideTrJs += "else {\n row2.style.display = ''; }\n";
                    displayHideTrJs += "var row3 = document.getElementById(id3);\n";
                    displayHideTrJs += " if ((value == \"(SPS_LNG_ALL)\") || (value == \"\"))\n {\n row3.style.display = 'none';\n }\n";
                    displayHideTrJs += "else {\n row3.style.display = ''; }\n";
                    displayHideTrJs += "var row4 = document.getElementById(id4);\n";
                    displayHideTrJs += " if ((value == \"(SPS_LNG_ALL)\") || (value == \"\"))\n {\n row4.style.display = 'none';\n }\n";
                    displayHideTrJs += "else {\n row4.style.display = 'none'; }\n";

                    if (hasProfileField)
                    {
                        displayHideTrJs += "var row5 = document.getElementById(id5);\n";
                        displayHideTrJs += " if ((value == \"(SPS_LNG_ALL)\") || (value == \"\"))\n {\n row5.style.display = 'none';\n }\n";
                        displayHideTrJs += "else {\n row5.style.display = ''; }\n";
                    }

                    displayHideTrJs += "}\n";
                    displayHideTrJs += "function DisplayDiv(id1)\n";
                    displayHideTrJs += "{\n";
                    displayHideTrJs += "var row1 = document.getElementById(id1);\n";
                    displayHideTrJs += " if (row1.style.display == 'none')\n {\n row1.style.display = '';\n }\n";
                    displayHideTrJs += "else {\n row1.style.display = 'none'; }\n";
                    displayHideTrJs += "}\n";

                    if (TranslatorRegex.HasMetadataToDuplicateFieldRegex.IsMatch(tempResponse.ToString()))
                    {
                        int positionId = tempResponse.IndexOf(" id=", tempResponse.IndexOf(@"<!-- FieldName=""MetadataToDuplicate"""));
                        int positionIdEnd = tempResponse.IndexOf("\"", positionId + 5);
                        string idMetadataField = tempResponse.Substring(positionId + 5, positionIdEnd - positionId - 5);

                        displayHideTrJs += "function AddMetadata(metadata, value)\n";
                        displayHideTrJs += "{\n";
                        displayHideTrJs += "var field = document.getElementById('" + idMetadataField + "');\n";
                        displayHideTrJs += "if (value != false) {field.value = field.value + metadata + ';';} else {field.value = field.value.replace(metadata + ';', '');}\n";
                        displayHideTrJs += "}\n";
                    }

                    string hasBodyEndTagRegexReplaced = TranslatorRegex.HasBodyEndTagRegex.Replace(tempResponse.ToString(), displayHideTrJs + "\n</script>\n</body>");
                    tempResponse.Clear().Append(hasBodyEndTagRegexReplaced);

                    if (!isDiscussionBoardList)
                    {
                        int positionSelect = tempResponse.IndexOf("<select ", tempResponse.IndexOf(@"<!-- FieldName=""SharePoint_Item_Language"""));

                        if (hasProfileField)
                            tempResponse.Insert(positionSelect + 7, disabledField + " onchange=\"DisplayRow('autotranslationtr','autocreationtr', 'MetadataToUpdateChoiceLink', 'MetadataToUpdateChoice', 'TranslationProfile', this.value)\"");
                        else
                            tempResponse.Insert(positionSelect + 7, disabledField + " onchange=\"DisplayRow('autotranslationtr','autocreationtr', 'MetadataToUpdateChoiceLink', 'MetadataToUpdateChoice', this.value)\"");
                    }

                    DisplayProfileSelection(tempResponse, valueFieldDisplay);
                }
            }

            if (HttpContext.Current.Request.Url.ToString().IndexOf("DispForm.aspx", StringComparison.OrdinalIgnoreCase) > -1)
            {
                if (TranslatorRegex.HasSharepointGroupLanguageFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""SharePoint_Group_Language""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:none\"");
                }

                if (TranslatorRegex.HasAutoTranslationFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""AutoTranslation""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:none\"");
                }

                if (TranslatorRegex.HasItemsAutoCreationFieldRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""ItemsAutoCreation""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:none\"");
                }

                if (TranslatorRegex.HasEmptyFieldNameRegex.IsMatch(tempResponse.ToString()))
                {
                    int positionLastTr = tempResponse.LastIndexOf("<tr>", tempResponse.IndexOf(@"<!-- FieldName=""""", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
                    tempResponse.Insert(positionLastTr + 3, " style=\"display:none\"");
                }
            }
        }

        public override void SaveWebpartProperties(StringBuilder tempResponse)
        {
            string siteUrl = SPContext.Current.Web.Url;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (var sysSite = new SPSite(siteUrl))
                using (var web = sysSite.OpenWeb())
                {
                    int webPartIdIndexBegin =
                        tempResponse.ToUpper().IndexOf(
                            "_EditorZone_Edit1".ToUpper()) +
                        "_EditorZone_Edit1".Length;

                    string webpartId =
                        tempResponse.Substring(webPartIdIndexBegin, 38);

                    using (
                        SPLimitedWebPartManager manager =
                            web.GetLimitedWebPartManager(
                                HttpContext.Current.Request.RawUrl,
                                System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                    {
                        try
                        {
                            foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                            {
                                try
                                {
                                    if (current.ID == webpartId)
                                    {
                                        HttpContext.Current.Cache.Add(
                                            "SPS_FUNCT_WEBPART",
                                            webpartId + "  " + current.Title,
                                            null, Cache.NoAbsoluteExpiration,
                                            Cache.NoSlidingExpiration,
                                            CacheItemPriority.NotRemovable,
                                            null);
                                    }

                                    current.Dispose();
                                }
                                catch (Exception e)
                                {
                                    Utilities.LogException(
                                        "SaveWebpartProperties", e,
                                        EventLogEntryType.Warning);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Utilities.LogException("SaveWebpartProperties", e,
                                                   EventLogEntryType.Warning);
                        }
                        finally
                        {
                            manager.Web.Dispose();
                        }
                    }
                }
            });
        }

        public override void ReloadWebpartProperties()
        {
            string siteUrl = SPContext.Current.Web.Url;
            var webpartId = (string)HttpContext.Current.Cache["SPS_FUNCT_WEBPART"];
            webpartId = webpartId.Substring(0, webpartId.IndexOf("  "));

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (var sysSite = new SPSite(siteUrl))
                using (var sysWeb = sysSite.OpenWeb())
                {
                    using (var manager = sysWeb.GetLimitedWebPartManager(HttpContext.Current.Request.RawUrl, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                    {
                        try
                        {
                            foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                            {
                                try
                                {
                                    if (current.ID == webpartId)
                                    {
                                        var oldTitle = (string)HttpContext.Current.Cache["SPS_FUNCT_WEBPART"];
                                        oldTitle = oldTitle.Substring(oldTitle.IndexOf("  ") + 2);
                                        string newTitle = current.Title;

                                        if (oldTitle.Contains("_Item_Langage_Enabled"))
                                            newTitle += "_Item_Langage_Enabled";

                                        if (oldTitle.Contains("_SPS_CONTENT_WEBPART_ON"))
                                            newTitle += "_SPS_CONTENT_WEBPART_ON";

                                        if (oldTitle.Contains("_SPS_CONTENT_WEBPART_OFF"))
                                            newTitle += "_SPS_CONTENT_WEBPART_OFF";

                                        foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                                        {
                                            if (oldTitle.Contains("_SPS_WEBPART_" + languageItem.LanguageDestination))
                                                newTitle += "_SPS_WEBPART_" + languageItem.LanguageDestination;
                                        }

                                        current.Title = newTitle;
                                        manager.SaveChanges(current);
                                        current.Dispose();
                                    }
                                }
                                catch (ThreadAbortException e)
                                {
                                    Utilities.LogException("ReloadWebpartProperties", e, EventLogEntryType.Warning);

                                    // we'll throw a thread abort exception on redirect, so we can ignore it, but
                                    // record everything else
                                }
                                catch (Exception e)
                                {
                                    Utilities.LogException("ReloadWebpartProperties", e, EventLogEntryType.Warning);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Utilities.LogException("ReloadWebpartProperties", e, EventLogEntryType.Warning);
                        }
                        finally
                        {
                            manager.Web.Dispose();
                        }
                    }
                }
            });
        }

        public override void HideFieldsForLinks(string listId, string url, string hide)
        {
            try
            {
                using (var currentSite = new SPSite(url))
                using (var web = currentSite.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    SPList currentList = web.Lists[new Guid(listId)];

                    SPFieldCollection fields = currentList.Fields;

                    foreach (string languageDestination in Languages.Instance.AllLanguages)
                    {
                        if (fields.ContainsField(languageDestination + " version"))
                        {
                            if ((hide == "True") && (!fields[languageDestination + " version"].Hidden))
                                fields[languageDestination + " version"].Hidden = true;
                            else
                                if ((hide == "False") && fields[languageDestination + " version"].Hidden)
                                    fields[languageDestination + " version"].Hidden = false;
                            fields[languageDestination + " version"].Update();
                        }
                    }

                    HttpContext.Current.Cache.Remove("SPS_HASHCODES_PAGES");
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("HideFieldsForLinks", e, EventLogEntryType.Warning);
            }
        }

        public override void AddWebPartMenu(StringBuilder tempResponse, string lang)
        {
            try
            {
                SPWeb web = SPContext.Current.Web;

                IEnumerable<WebPartMenuToDisplayHelper> menusToDisplay = GetWebPartMenuToDisplay(web);

                string originalMenu = string.Empty;

                foreach (var webPartMenuToDisplayHelper in menusToDisplay)
                {
                    int webPartIdPosition = tempResponse.IndexOf(" WebPartID=\"" + webPartMenuToDisplayHelper.StorageKey);

                    if (webPartIdPosition != -1)
                    {
                        InsertMenuForWebpartId(tempResponse, webPartMenuToDisplayHelper, webPartIdPosition, ref originalMenu);
                    }
                    else
                    {
                        int storageKeyPosition = tempResponse.IndexOf("_WebPartStorageKey=\"" + webPartMenuToDisplayHelper.StorageKey + "\"");

                        if (storageKeyPosition != -1)
                        {
                        }
                    }
                }
            }
            catch (InvalidCastException)
            {
                // Silence: Case of custom WebPart for example
            }
            catch (Exception e)
            {
                Utilities.LogException("AddWebPartMenu", e, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Add menu option to existing menu
        /// </summary>
        /// <param name="tempResponse">
        /// Response to treat
        /// </param>
        /// <param name="lang">
        /// current language
        /// </param>
        /// <param name="completingDictionaryMode">
        /// true if completion mode is enabled
        /// </param>
        /// <param name="languageSource">
        /// current Lc Code
        /// </param>
        /// <param name="editMode">Told if we are in Edition mode</param>
        public void AddMenuOption(StringBuilder tempResponse, string lang, bool completingDictionaryMode, string languageSource, bool editMode)
        {
            if (editMode)
            {
                AddWebPartMenu(tempResponse, lang);
            }

            AddMenuItem(tempResponse, lang, languageSource, completingDictionaryMode);
        }

        public override void ProcessTranslationExtractor(string url, string defaultLanguage)
        {
            string redirectUrl = string.Empty;

            try
            {
                using (var currentSite = new SPSite(url))
                using (SPWeb web = currentSite.OpenWeb())
                {
                    SPList pagesToParseUrlList = web.GetList("/Lists/PagesToUpdateForTranslation");

                    if (pagesToParseUrlList.Fields.ContainsField("Pages"))
                    {
                        if (pagesToParseUrlList.Items.Count > 0)
                        {
                            string refLanguage = string.Empty;

                            foreach (LanguageItem languageItem in Dictionaries.Instance.Languages)
                            {
                                if (languageItem.LanguageDestination != defaultLanguage)
                                {
                                    refLanguage = languageItem.LanguageDestination;
                                    break;
                                }
                            }

                            foreach (SPListItem currentItem in pagesToParseUrlList.Items)
                            {
                                redirectUrl = currentItem["Pages"] + "?SPSLanguage=" + refLanguage + "&SPS_Trans_Code=Translation_Extractor&SPS_Extractor_Status=" + currentItem.ID;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("ProcessTranslationExtractor", e, EventLogEntryType.Warning);
            }

            if (!string.IsNullOrEmpty(redirectUrl))
                HttpContext.Current.Response.Redirect(redirectUrl, false);
        }

        public override void AjaxFormating(StringBuilder tempResponse)
        {
            try
            {
                int beginBlockValueIndex = -1;

                var tempResponseBeforeLengthModif = new StringBuilder(tempResponse.ToString());

                var blocksList = new ArrayList();

                var blocksIndexPair = new int[2];

                foreach (Match block in TranslatorRegex.BlockAjaxRegex.Matches(tempResponse.ToString()))
                {
                    if (block.Index == 0)
                    {
                        beginBlockValueIndex = 0;

                        // Cas de la réponse Ajax pour les update auto de WebParts
                        if (TranslatorRegex.BlockAjaxRegex.Matches(tempResponse.ToString()).Count == 1)
                        {
                            if (tempResponse.IndexOf("|0|hiddenField|__EVENTTARGET||") > -1)
                            {
                                blocksIndexPair[1] = tempResponse.IndexOf("|0|hiddenField|__EVENTTARGET||");
                                blocksList.Add(blocksIndexPair);
                            }
                        }
                    }
                    else
                    {
                        blocksIndexPair[0] = beginBlockValueIndex;
                        blocksIndexPair[1] = block.Index;
                        blocksList.Add(blocksIndexPair);
                        beginBlockValueIndex = block.Index;
                    }
                }

                foreach (int[] indexPair in blocksList)
                {
                    string blockString = tempResponseBeforeLengthModif.Substring(indexPair[0], indexPair[1] - indexPair[0]);
                    string blockHeader = TranslatorRegex.BlockAjaxRegex.Match(blockString).Value;
                    string blockValue = blockString.Substring(blockHeader.Length);
                    int newBlockValueLength = blockValue.Length;
                    string beginPipe = string.Empty;
                    if (TranslatorRegex.BlockAjaxRegex.Match(blockString).Value.StartsWith("|"))
                        beginPipe = "|";

                    string newBlockHeader = beginPipe + newBlockValueLength + TranslatorRegex.BlockAjaxRegex.Match(blockString).Groups["constant"].Value;
                    string newBlockString = newBlockHeader + blockValue;
                    tempResponse.Replace(blockString, newBlockString);
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("AjaxFormatting", e, EventLogEntryType.Warning);
            }
        }

        public override void RedirectToLinkedPage(string language)
        {
            if (HttpContext.Current != null)
            {
                string redirectUrl = string.Empty;

                try
                {
                    SPList currentList = SPContext.Current.List;
                    bool pagesListExist = false;
                    string currentUrl;
                    string currentPage;

                    if ((currentList != null) && (currentList.ToString().ToLower() == "pages"))
                        pagesListExist = true;

                    if (pagesListExist)
                    {
                        currentUrl = HttpContext.Current.Request.RawUrl;

                        if (currentUrl.ToLower().Contains("/pages/"))
                        {
                            currentPage = currentUrl.ToLower().Substring(currentUrl.ToLower().IndexOf("/pages/") + 7);

                            if (currentPage.Contains("?"))
                                currentPage = currentPage.Remove(currentPage.IndexOf("?"));
                        }
                        else
                            return;
                    }
                    else
                        return;

                    if (currentList.Fields.ContainsField("SharePoint_Item_Language") &&
                        currentList.Fields.ContainsField(language + " version"))
                    {
                        var query = new SPQuery
                        {
                            Query = "<Where><Eq><FieldRef Name='FileLeafRef'/>" +
                                    "<Value Type='File'>" + currentPage +
                                    "</Value></Eq></Where>",
                            QueryThrottleMode = SPQueryThrottleOption.Override
                        };
                        SPListItemCollection collListItems = currentList.GetItems(query);

                        if (collListItems.Count > 0)
                            foreach (SPListItem item in collListItems)
                            {
                                SPListItem linkedPage;
                                if ((item["SharePoint_Item_Language"] != null) &&
                                    (item["SharePoint_Item_Language"].ToString() != "(SPS_LNG_ALL)") &&
                                    (item[language + " version"] != null) &&
                                    (item["SharePoint_Item_Language"].ToString() != "SPS_LNG_" + language))
                                {
                                    try
                                    {
                                        linkedPage =
                                            currentList.GetItemById(
                                                Convert.ToInt32(
                                                    item[language + " version"].ToString().Remove(
                                                        item[language + " version"].ToString().IndexOf(";"))));
                                        redirectUrl = currentUrl.ToLower();
                                        if (redirectUrl.Contains("?"))
                                        {
                                            string queryString = redirectUrl.Substring(redirectUrl.IndexOf("?"));
                                            redirectUrl = redirectUrl.Remove(redirectUrl.IndexOf("?"));
                                            redirectUrl =
                                                redirectUrl.Substring(0, redirectUrl.Length - currentPage.Length) +
                                                linkedPage.Name + queryString;
                                        }
                                        else
                                        {
                                            redirectUrl =
                                                redirectUrl.Substring(0, redirectUrl.Length - currentPage.Length) +
                                                linkedPage.Name;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Utilities.LogException("RedirectToLinkedPage", e, EventLogEntryType.Warning);
                                        return;
                                    }
                                }
                                else
                                    return;
                            }
                    }
                    else
                        return;
                }
                catch (Exception e)
                {
                    Utilities.LogException("RedirectToLinkedPage", e, EventLogEntryType.Warning);
                }

                if (!string.IsNullOrEmpty(redirectUrl))
                    HttpContext.Current.Response.Redirect(redirectUrl, false);
            }
        }

        public override void QuickLaunchFilter(StringBuilder tempResponse, string language)
        {
            if (HttpContext.Current == null)
                return;

            try
            {
                var quickLaunchIsActivated = (bool)HttpContext.Current.Cache["AlphamosaikQuickLaunchFilter"];

                if (quickLaunchIsActivated && (TranslatorRegex.QuickLaunchRegEx.IsMatch(tempResponse.ToString()) || TranslatorRegex.QuickLaunchRegExV2.IsMatch(tempResponse.ToString())))
                {
                    using (SPSite currentSite = SPContext.Current.Site)
                    using (SPWeb web = currentSite.OpenWeb())
                    {
                        SPList pagesList = web.Lists.TryGetList("Pages");

                        if (pagesList != null && pagesList.Fields.ContainsField("SharePoint_Item_Language") && pagesList.Fields.ContainsField(language + " version"))
                        {
                            var query = new SPQuery
                            {
                                Query = "<Where><And><Neq><FieldRef Name='SharePoint_Item_Language'/>" +
                                        "<Value Type='File'>" + "(SPS_LNG_ALL)" +
                                        "</Value></Neq><Neq><FieldRef Name='SharePoint_Item_Language'/>" +
                                        "<Value Type='File'>" + string.Empty +
                                        "</Value></Neq></And></Where>",
                                QueryThrottleMode = SPQueryThrottleOption.Override
                            };

                            SPListItemCollection collListItems = pagesList.GetItems(query);

                            if (collListItems.Count > 0)
                            {
                                foreach (SPListItem item in collListItems)
                                {
                                    if ((item["SharePoint_Item_Language"] != null) && (item["SharePoint_Item_Language"].ToString() != "SPS_LNG_" + language))
                                    {
                                        try
                                        {
                                            string siteUrl = item.Web.ServerRelativeUrl;

                                            if (siteUrl == "/")
                                                siteUrl = string.Empty;

                                            // On récupère le contenu HTML de la QuickLaunch
                                            string quickLaunchHtml = TranslatorRegex.QuickLaunchRegEx.IsMatch(tempResponse.ToString()) ? TranslatorRegex.QuickLaunchRegEx.Match(tempResponse.ToString()).Groups["Content"].Value : TranslatorRegex.QuickLaunchRegExV2.Match(tempResponse.ToString()).Groups["Content"].Value;

                                            var quickLaunchXml = new XmlDocument();
                                            quickLaunchXml.LoadXml(quickLaunchHtml);

                                            XmlNode nodeToDelete = quickLaunchXml.SelectSingleNode("//a[@href=\"" + siteUrl + '/' + HttpUtility.UrlPathEncode(item.Url) + "\"]/.."); // on sélectionne le parent (<li>) du noeud <a> dont l'attribut href correspond à la page recherchée

                                            // On supprime le noeud <li> si trouvé
                                            if (nodeToDelete != null)
                                            {
                                                string stringToDelete = nodeToDelete.OuterXml;
                                                tempResponse.Replace(quickLaunchHtml, quickLaunchHtml.Replace(stringToDelete, string.Empty));
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Utilities.LogException("QuickLaunchFilter", e, EventLogEntryType.Warning);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("QuickLaunchFilter", e, EventLogEntryType.Warning);
            }
        }

        public override void TopNavigationBarFilter(StringBuilder tempResponse, string language)
        {
            if (HttpContext.Current == null)
                return;

            try
            {
                var topNavigationBarFilterIsActivated = (bool)HttpContext.Current.Cache["AlphamosaikTopNavigationBarFilter"];

                if (topNavigationBarFilterIsActivated && (tempResponse.IndexOf("_TopNavigationMenuV4") > -1) && TranslatorRegex.TopNavigationBarRegex.IsMatch(tempResponse.ToString()))
                {
                    using (SPSite currentSite = SPContext.Current.Site)
                    using (SPWeb web = currentSite.OpenWeb())
                    {
                        SPList pagesList = web.Lists.TryGetList("Pages");

                        if (pagesList != null && pagesList.Fields.ContainsField("SharePoint_Item_Language") && pagesList.Fields.ContainsField(language + " version"))
                        {
                            var query = new SPQuery
                            {
                                Query = "<Where><And><Neq><FieldRef Name='SharePoint_Item_Language'/>" +
                                        "<Value Type='File'>" + "(SPS_LNG_ALL)" +
                                        "</Value></Neq><Neq><FieldRef Name='SharePoint_Item_Language'/>" +
                                        "<Value Type='File'>" + string.Empty +
                                        "</Value></Neq></And></Where>",
                                QueryThrottleMode = SPQueryThrottleOption.Override
                            };

                            SPListItemCollection collListItems = pagesList.GetItems(query);

                            if (collListItems.Count > 0)
                            {
                                foreach (SPListItem item in collListItems)
                                {
                                    if ((item["SharePoint_Item_Language"] != null) && (item["SharePoint_Item_Language"].ToString() != "SPS_LNG_" + language))
                                    {
                                        try
                                        {
                                            string siteUrl = item.Web.ServerRelativeUrl;

                                            if (siteUrl == "/")
                                                siteUrl = string.Empty;

                                            string topNavigationBarHtml = TranslatorRegex.TopNavigationBarRegex.Match(tempResponse.ToString()).Groups["Content"].Value;

                                            var topNavigationBarXml = new XmlDocument();
                                            topNavigationBarXml.LoadXml(topNavigationBarHtml);

                                            XmlNode nodeToDelete = topNavigationBarXml.SelectSingleNode("//a[@href=\"" + siteUrl + '/' + HttpUtility.UrlPathEncode(item.Url) + "\"]/..");

                                            if (nodeToDelete != null)
                                            {
                                                string stringToDelete = nodeToDelete.OuterXml;
                                                tempResponse.Replace(topNavigationBarHtml, topNavigationBarHtml.Replace(stringToDelete, string.Empty));
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Utilities.LogException("TopNavigationBarFilter", e, EventLogEntryType.Warning);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("TopNavigationBarFilter", e, EventLogEntryType.Warning);
            }
        }

        public override string AddToDictionary(string url, string defaultLang, string term, IAutomaticTranslation automaticTranslationPlugin)
        {
            string resultMessage;

            using (var dictionary = new SpsDictionary(url, automaticTranslationPlugin))
            {
                string displayLanguage = "Languages updated : ";

                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                {
                    displayLanguage += " " + languageItem.DisplayName + " |";
                }

                resultMessage =
                        @"<html>
                        <body>
                            <div >
                                <table style=""border:solid 0px black; color:black; background:#F3F387;font-family:tahoma;font-size:13;"">
                                    <tr >
                                        <td>
                                            <img src=""/_layouts/images/alpha_logo_menu.png"" border=""0"" >
                                        </td>
                                        <td style=""text-align:center;border-bottom:1px solid black;font-weight:bold;"">Default language : ";
                resultMessage += defaultLang;
                resultMessage += @"</td>
                    </tr>";

                if (term.Contains(SpsDictionary.AlphaSeparator))
                {
                    string[] result = term.Split(new[] { SpsDictionary.AlphaSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in result)
                    {
                        try
                        {
                            SpsDictionary.ItemStatus currentTermStatus = dictionary.AddTerm(s, defaultLang);
                            switch (currentTermStatus)
                            {
                                case SpsDictionary.ItemStatus.Updated:
                                    resultMessage += @" <tr><td>Updated : </td><td style=""padding:2px;padding:2px;""><span style=""border:solid 1px black; color:black; background:#E8E8FF; padding:1px;""><b> ";
                                    resultMessage += s.Trim();
                                    resultMessage += @"</b></span></td></tr>";
                                    break;
                                case SpsDictionary.ItemStatus.Inserted:
                                    resultMessage += @" <tr><td>Added : </td><td style=""padding:2px;padding:2px;""><span style=""border:solid 1px black; color:black; background:#E8E8FF; padding:1px;""><b> ";
                                    resultMessage += s.Trim();
                                    resultMessage += @"</b></span></td></tr>";
                                    break;
                                case SpsDictionary.ItemStatus.Existing:
                                    resultMessage += @" <tr><td>Existing : </td><td style=""padding:2px;padding:2px;""><span style=""border:solid 1px black; color:black; background:#E8E8FF; padding:1px;""><b> ";
                                    resultMessage += s.Trim();
                                    resultMessage += @"</b></span></td></tr>";
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch
                        {
                            resultMessage += @" <tr>
                                                        <td>Error : </td>
                                                        <td style=""padding:5px;padding:20px;"">
                                                            <div>
                                                                <span style=""border:solid 1px black; color:black; background:#E8E8FF; padding:1px;"">
                                                                    <b> ";
                            resultMessage += s.Trim();
                            resultMessage += @"                 </b>
                                                                </span>
                                                            </div>
                                                        </td>
                                                    </tr>";
                        }
                    }
                }
                else
                {
                    try
                    {
                        SpsDictionary.ItemStatus currentTermStatus = dictionary.AddTerm(term, defaultLang);
                        switch (currentTermStatus)
                        {
                            case SpsDictionary.ItemStatus.Updated:
                                resultMessage += @" <tr>
                                                    <td></td>
                                                    <td style=""padding:5px;padding:20px;"">
                                                        <div>The expression : 
                                                            <span style=""border:solid 1px black; color:black; background:#E8E8FF; padding:1px;"">
                                                                <b> ";
                                resultMessage += term.Trim();
                                resultMessage += @"               </b>
                                                            </span>
                                                        </div>
                                                        <div> has been added in the dictionary successfully : 1 item updated.</div>
                                                    </td>
                                                </tr>
                                                <tr height=""40"" >
                                                    <td></td><td></td>
                                                </tr>";
                                break;
                            case SpsDictionary.ItemStatus.Inserted:
                                resultMessage += "<tr><td></td><td style=\"padding:5px;padding:20px;\"><div>The expression : <span style=\"border:solid 1px black; color:black; background:#E8E8FF; padding:1px;\"><b> " + term.Trim() + "</b></span></div><div> has been added in the dictionary successfully : 1 item added.</div></td></tr><tr height=\"40\" ><td></td>";
                                break;
                            case SpsDictionary.ItemStatus.Existing:
                                resultMessage += "<tr><td></td><td style=\"padding:5px;padding:20px;\"><div>The expression : <span style=\"border:solid 1px black; color:black; background:#E8E8FF; padding:1px;\"><b> " + term.Trim() + "</b></span></div><div> is already in the dictionary for all languages.</div></td></tr><tr height=\"40\" ><td></td>";
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        resultMessage = "<html><body><div ><table style=\"border:solid 0px black; color:black; background:#F3F387;font-family:tahoma;font-size:13;\"><tr ><td><img src=\"/_layouts/images/alpha_logo_menu.png\" border=\"0\" ></td><td style=\"text-align:center;border-bottom:1px solid black;font-weight:bold;\">" + defaultLang + "</td></tr><tr><td></td><td style=\"padding:5px;padding:20px;\"><div>Error while adding :<span style=\"border:solid 1px black; color:black; background:#E8E8FF; padding:1px;\"><b> " + term.Trim() + "</b></span></div><div> in the dictionary.</div></td></tr><tr height=\"40\" ><td></td><td>" + e.Message + "</td></tr><tr><td></td><td style=\"padding:10px;\"><div><input type=\"button\" name=\"reload\" value=\"Update dictionary cache\" onclick=\"javascript:window.location.search ='?SPS_Trans_Code=SPS_Reload_Dictionary'\" style=\"background-color:#3cb371;\" style=\"color:white; font-family:tahoma;font-size:13;\"/></div></td></table></div></body></html>";
                    }
                }

                resultMessage += @" <tr>
                                    <td></td>
                                    <td>";
                resultMessage += displayLanguage;
                resultMessage += @"     </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td style=""padding:10px;"">
                                        <div>
                                            <input type=""button"" 
                                                name=""reload"" 
                                                value=""Update dictionary cache"" 
                                                onclick=""javascript:window.location.search ='?SPS_Trans_Code=SPS_Reload_Dictionary'"" 
                                                style=""background-color:#3cb371; color:white; font-family:tahoma;font-size:13;""/>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </body>
                </html>";
            }

            return resultMessage;
        }

        /// <summary>
        /// Remove item that are not in the user's language
        /// </summary>
        /// <param name="response">
        /// Reponse to treat
        /// </param>
        /// <param name="itemLanguageToRemove">
        /// language to remove
        /// </param>
        public override void RemoveTranslatedTr(StringBuilder response, string itemLanguageToRemove)
        {
            var save = new StringBuilder(response.ToString());

            try
            {
                int index = response.IndexOf(itemLanguageToRemove);

                while (index > 0)
                {
                    //// For the Blog Webpart
                    int indexMsoZoneCell = response.ToLower().LastIndexOf("<td id=\"msozonecell_", index);
                    int indexClassMsPostWrapper = response.ToLower().LastIndexOf("<div class=\"ms-postwrapper\">", index);

                    if ((indexMsoZoneCell > -1) && (indexClassMsPostWrapper > indexMsoZoneCell))
                    {
                        int openingDivCount =
                            TranslatorRegex.RegexOpeningDiv.Matches(response.Substring(indexClassMsPostWrapper,
                                                                                       index - indexClassMsPostWrapper))
                                .Count;
                        int closingDivCount =
                            TranslatorRegex.RegexClosingDiv.Matches(response.Substring(indexClassMsPostWrapper,
                                                                                       index - indexClassMsPostWrapper))
                                .Count;

                        int closingDivAfterSpsLng = openingDivCount - closingDivCount;

                        int postToRemoveEndIndex = index;

                        while (closingDivAfterSpsLng > 0)
                        {
                            postToRemoveEndIndex = response.ToLower().IndexOf("</div>", postToRemoveEndIndex) + 6;
                            closingDivAfterSpsLng--;
                        }

                        if (
                            TranslatorRegex.RegexPostDate.IsMatch(response.Substring(indexClassMsPostWrapper,
                                                                                     postToRemoveEndIndex -
                                                                                     indexClassMsPostWrapper)))
                        {
                            if (
                                response.ToLower().IndexOf("<div class=\"ms-postwrapper\">", postToRemoveEndIndex, 100) >
                                -1)
                            {
                                string postDateToMove =
                                    TranslatorRegex.RegexPostDate.Match(response.Substring(indexClassMsPostWrapper,
                                                                                           postToRemoveEndIndex -
                                                                                           indexClassMsPostWrapper)).Value;
                                response.Insert(
                                    response.ToLower().IndexOf("<div class=\"ms-postwrapper\">", postToRemoveEndIndex),
                                    postDateToMove);
                            }
                        }

                        response.Remove(indexClassMsPostWrapper, postToRemoveEndIndex - indexClassMsPostWrapper);
                    }
                    else
                    {
                        //// For the Announcements WebPart                    
                        int indexClassAnnouncement =
                            response.ToLower().LastIndexOf("<span class=\"ms-announcementtitle\">", index);

                        if (indexClassAnnouncement > -1)
                        {
                            if (response.ToLower().LastIndexOf("<tr>", index) ==
                                response.ToLower().IndexOf("<tr>", indexClassAnnouncement))
                            {
                                response.Insert(response.ToLower().IndexOf("<tr>", index) + 4, itemLanguageToRemove);
                                response.Insert(indexClassAnnouncement, itemLanguageToRemove);
                            }
                        }

                        // remove line
                        int closeTrIndex = InsensitiveIndexOf(response, "</TR", index + 10);

                        string textToParse = response.Substring(0, closeTrIndex);

                        int closingTr = 1;
                        int openingTr = 0;

                        int beginingTrIndex = 0;

                        while (closingTr != openingTr)
                        {
                            int closingTrIndex = InsensitiveLastIndexOf(textToParse, "</TR");
                            int openingTrIndex = InsensitiveLastIndexOf(textToParse, "<TR");

                            if (closingTrIndex > openingTrIndex)
                            {
                                closingTr++;
                                textToParse = textToParse.Substring(0, closingTrIndex);
                            }
                            else
                            {
                                openingTr++;
                                textToParse = textToParse.Substring(0, openingTrIndex);
                                beginingTrIndex = openingTrIndex;
                            }
                        }

                        response.Remove(beginingTrIndex, closeTrIndex + 5 - beginingTrIndex);
                    }

                    index = response.IndexOf(itemLanguageToRemove);
                }
            }
            catch (Exception exc)
            {
                Utilities.LogException("RemoveTranslatedTr", exc, EventLogEntryType.Warning);
                TraceError(exc);

                response.Clear().Append(save);
            }
        }

        /// <summary>
        /// Remove item that are not in the user's language
        /// </summary>
        /// <param name="response">
        /// Reponse to treat
        /// </param>
        /// <param name="language">
        /// language to remove
        /// </param>
        public override void RemoveTranslatedDiv(StringBuilder response, string language)
        {
            var save = new StringBuilder(response.ToString());

            try
            {
                string itemLanguageToRemove = "<div class=\"" + language;

                int index = response.IndexOf(itemLanguageToRemove, StringComparison.OrdinalIgnoreCase);

                while (index > 0)
                {
                    int indexStart = response.IndexOf("<DIV", index + 1, StringComparison.OrdinalIgnoreCase);
                    int indexEnd = response.IndexOf("</DIV>", index + 1, StringComparison.OrdinalIgnoreCase);

                    while (indexStart > -1 && indexStart < indexEnd)
                    {
                        indexStart = response.IndexOf("<DIV", indexStart + 1, StringComparison.OrdinalIgnoreCase);
                        indexEnd = response.IndexOf("</DIV>", indexEnd + 1, StringComparison.OrdinalIgnoreCase);
                    }

                    response.Remove(index, indexEnd - index + 6);

                    index = response.IndexOf(itemLanguageToRemove);
                }
            }
            catch (Exception exc)
            {
                Utilities.LogException("RemoveTranslatedDiv", exc, EventLogEntryType.Warning);
                TraceError(exc);

                response.Clear().Append(save);
            }
        }

        public override ArrayList RemoveTranslatedDivForCache(StringBuilder response, string cacheTag)
        {
            var save = new StringBuilder(response.ToString());

            try
            {
                string itemCacheTagToRemove = "<div class=\"" + cacheTag;

                int index = response.IndexOf(itemCacheTagToRemove, StringComparison.OrdinalIgnoreCase);

                int cmpt = 0;

                var removedTranslatedDivForCache = new ArrayList();

                while (index > 0)
                {
                    int indexStart = response.IndexOf("<DIV", index + 1, StringComparison.OrdinalIgnoreCase);
                    int indexEnd = response.IndexOf("</DIV>", index + 1, StringComparison.OrdinalIgnoreCase);

                    while (indexStart > -1 && indexStart < indexEnd)
                    {
                        indexStart = response.IndexOf("<DIV", indexStart + 1, StringComparison.OrdinalIgnoreCase);
                        indexEnd = response.IndexOf("</DIV>", indexEnd + 1, StringComparison.OrdinalIgnoreCase);
                    }

                    removedTranslatedDivForCache.Add(response.Substring(index, indexEnd - index + 6));

                    response.Remove(index, indexEnd - index + 6);
                    response.Insert(index, "####" + cacheTag + cmpt + "####");

                    index = response.IndexOf(itemCacheTagToRemove);

                    cmpt++;
                }

                return removedTranslatedDivForCache;
            }
            catch (Exception exc)
            {
                Utilities.LogException("RemoveTranslatedDivForCache", exc, EventLogEntryType.Warning);
                TraceError(exc);

                response.Clear().Append(save);
                return new ArrayList();
            }
        }

        /// <summary>
        /// Insensitive index of
        /// </summary>
        /// <param name="str">source string</param>
        /// <param name="search">text to search</param>
        /// <param name="startIndex">starting index</param>
        /// <returns>return the position found</returns>
        public int InsensitiveIndexOf(StringBuilder str, string search, int startIndex)
        {
            return str.IndexOf(search, startIndex, StringComparison.InvariantCultureIgnoreCase);
        }

        public int InsensitiveIndexOf(string str, string search, int startIndex)
        {
            return str.IndexOf(search, startIndex, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Insensitive last index of
        /// </summary>
        /// <param name="str">source string</param>
        /// <param name="search">text to search</param>
        /// <returns>position text found</returns>
        public int InsensitiveLastIndexOf(string str, string search)
        {
            str = str.ToUpper();
            return str.LastIndexOf(search);
        }

        private static void UpgradeWebPartsProperties(SPWeb web, System.Web.UI.WebControls.WebParts.WebPart webPart, Guid storageKey, SPLimitedWebPartManager manager)
        {
            bool isManagerNeedUpdate = false;

            if (webPart.Title.Contains("_SPS_CONTENT_WEBPART_ON"))
            {
                webPart.Title = webPart.Title.Replace("_SPS_CONTENT_WEBPART_ON", string.Empty);
                isManagerNeedUpdate = true;
            }

            if (webPart.Title.Contains("_SPS_WEBPART_"))
            {
                foreach (string languageCode in Languages.Instance.AllLanguages)
                {
                    if (webPart.Title.IndexOf("_SPS_WEBPART_" + languageCode) > -1)
                    {
                        webPart.Title = webPart.Title.Replace("_SPS_WEBPART_" + languageCode, string.Empty);

                        try
                        {
                            bool webpartPropertiesExist = HasWebPartFunctionnality(web, storageKey.ToString());
                            string value = string.Empty;

                            if (webpartPropertiesExist)
                            {
                                value = GetWebPartFunctionnality(web, storageKey.ToString());

                                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                                    value = value.Replace("_SPS_WEBPART_" + languageItem.LanguageDestination, string.Empty);
                            }

                            if (languageCode != "ALL")
                                value += "_SPS_WEBPART_" + languageCode;

                            if (webpartPropertiesExist)
                            {
                                web.AllProperties["Alphamosaik.Translator.WebParts " + storageKey] = value;
                            }
                            else
                            {
                                web.AllProperties.Add("Alphamosaik.Translator.WebParts " + storageKey, value);
                            }

                            web.AllowUnsafeUpdates = true;
                            web.Update();
                            web.AllowUnsafeUpdates = false;
                        }
                        catch (Exception exc)
                        {
                        }

                        isManagerNeedUpdate = true;
                        break;
                    }
                }
            }

            if (webPart.Title.Contains("_SPS_CONTENT_WEBPART_OFF"))
            {
                webPart.Title = webPart.Title.Replace("_SPS_CONTENT_WEBPART_OFF", string.Empty);

                if (HasWebPartFunctionnality(web, storageKey.ToString()))
                {
                    web.AllProperties["Alphamosaik.Translator.WebParts " + storageKey] = ((string)web.AllProperties["Alphamosaik.Translator.WebParts " + storageKey])
                        .Replace("_SPS_CONTENT_WEBPART_OFF", string.Empty).Replace("_SPS_CONTENT_WEBPART_ON", string.Empty);
                    web.AllProperties["Alphamosaik.Translator.WebParts " + storageKey] += "_SPS_CONTENT_WEBPART_OFF";
                }
                else
                {
                    web.AllProperties.Add("Alphamosaik.Translator.WebParts " + storageKey, "_SPS_CONTENT_WEBPART_OFF");
                }

                try
                {
                    web.AllowUnsafeUpdates = true;
                    web.Update();
                    web.AllowUnsafeUpdates = false;
                }
                catch
                { }

                isManagerNeedUpdate = true;
            }

            if (webPart.Title.Contains("_Item_Langage_Enabled"))
            {
                webPart.Title = webPart.Title.Replace("_Item_Langage_Enabled", string.Empty);

                if (HasWebPartFunctionnality(web, storageKey.ToString()))
                {
                    if (!GetWebPartFunctionnality(web, storageKey.ToString()).Contains("_Item_Langage_Enabled"))
                    {
                        web.AllProperties["Alphamosaik.Translator.WebParts " + storageKey.ToString()] += "_Item_Langage_Enabled";
                    }
                }
                else
                {
                    web.AllProperties.Add("Alphamosaik.Translator.WebParts " + storageKey.ToString(), "_Item_Langage_Enabled");
                }

                web.AllowUnsafeUpdates = true;
                web.Update();
                web.AllowUnsafeUpdates = false;

                isManagerNeedUpdate = true;
            }

            if (isManagerNeedUpdate)
            {
                manager.SaveChanges(webPart);
            }
        }

        public override Hashtable LoadWebPartHashTable(ref string spsWebpartLanguageList, ref bool spsContentExist, ref List<Guid> webPartListToRemove, string currentLanguage)
        {
            var webPartIdHashTable = new Hashtable();

            string spsWebpartLanguageListCopy = spsWebpartLanguageList;
            bool spsContentExistCopy = spsContentExist;
            List<Guid> webPartListToRemoveCopy = webPartListToRemove;

            if (HttpContext.Current == null)
                return webPartIdHashTable;

            SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    HttpContext context = HttpContext.Current;

                    bool upgradeFromVersion2007 = (bool)(context.Cache["AlphamosaikUpgradeFromVersion2007"]);

                    SPWeb web = Microsoft.SharePoint.WebControls.SPControl.GetContextWeb(context);
                    
                    try
                    {
                        string value;

                        using (
                            SPLimitedWebPartManager manager =
                                web.GetLimitedWebPartManager(
                                    HttpContext.Current.Request.RawUrl,
                                    System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                        {
                            try
                            {
                                foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                                {
                                    try
                                    {
                                        Guid storageKey = manager.GetStorageKey(current);

                                        if (upgradeFromVersion2007 && web.CurrentUser.IsSiteAdmin)
                                        {
                                            try
                                            {
                                                UpgradeWebPartsProperties(web, current, storageKey, manager);
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        if (
                                            web.AllProperties.ContainsKey(
                                                "Alphamosaik.Translator.WebParts " + storageKey))
                                        {
                                            value =
                                                (string)
                                                web.AllProperties["Alphamosaik.Translator.WebParts " + storageKey];
                                        }
                                        else
                                        {
                                            value = string.Empty;
                                        }

                                        if (value.Contains("_SPS_WEBPART_"))
                                        {
                                            spsWebpartLanguageListCopy = spsWebpartLanguageListCopy + current.Title
                                                                         + value + " ";

                                            if (!value.Contains("_SPS_WEBPART_" + currentLanguage)) webPartListToRemoveCopy.Add(storageKey);
                                        }

                                        if (value.Contains("_SPS_CONTENT_WEBPART_OFF")) spsContentExistCopy = true;

                                        webPartIdHashTable.Remove(storageKey.ToString());
                                        webPartIdHashTable.Add(storageKey.ToString(), current);
                                    }
                                    catch (WebPartPageUserException webPartPageUserException)
                                    {
                                        Utilities.TraceNormalCaughtException(
                                            "LoadWebpartHashtable", webPartPageUserException);
                                    }
                                    catch (Exception e)
                                    {
                                        Utilities.TraceNormalCaughtException("LoadWebpartHashtable", e);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Utilities.TraceNormalCaughtException("LoadWebpartHashtable", e);
                            }
                            finally
                            {
                                manager.Web.Dispose();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utilities.TraceNormalCaughtException("LoadWebpartHashtable", e);
                    }
                });

            spsWebpartLanguageList = spsWebpartLanguageListCopy;
            spsContentExist = spsContentExistCopy;
            webPartListToRemove = webPartListToRemoveCopy;

            return webPartIdHashTable;
        }

        /// <summary>
        /// Exclude webparts content from translation process
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <param name="allWebPartsContentDisable">
        /// The all Web Parts Content Disable.
        /// </param>
        /// <param name="webPartIdHashTable">
        /// The web Part Id Hash Table.
        /// </param>
        public override void ExcludeContentWebpartFromTrad(StringBuilder response, bool allWebPartsContentDisable, Hashtable webPartIdHashTable)
        {
            var save = new StringBuilder(response.ToString());

            //// Fix for the webParts without Chrome
            HttpContext context = HttpContext.Current;
            SPWeb web = Microsoft.SharePoint.WebControls.SPControl.GetContextWeb(context);

            try
            {
                int indexMsoZoneCell = 0;
                int countMsoZoneCell = StringUtilities.CharacterCounter(response, "<TD ID=\"MSOZONECELL_",
                                                                        StringComparison.OrdinalIgnoreCase);

                while (countMsoZoneCell > 0)
                {
                    indexMsoZoneCell = response.ToUpper().IndexOf("<TD ID=\"MSOZONECELL_", indexMsoZoneCell + 11);

                    int indexWebPartId =
                        response.Substring(indexMsoZoneCell, response.Length - indexMsoZoneCell - 1).ToUpper().IndexOf(
                            "WebPartID=\"".ToUpper()) + indexMsoZoneCell;

                    string webPartId = response.Substring(indexWebPartId + 11,
                                                          response.Substring(indexWebPartId + 11).IndexOf("\""));

                    try
                    {
                        string functionnalityList = GetWebPartFunctionnality(web, webPartId);
                        bool hasFunctionnalityContentOff = functionnalityList.Contains("_SPS_CONTENT_WEBPART_OFF");
                        bool hasFunctionnalityContentOn = functionnalityList.Contains("_SPS_CONTENT_WEBPART_ON");

                        if (webPartIdHashTable.ContainsKey(webPartId))
                        {
                            var current = (System.Web.UI.WebControls.WebParts.WebPart)webPartIdHashTable[webPartId];

                            if (hasFunctionnalityContentOff || (allWebPartsContentDisable && !hasFunctionnalityContentOn))
                            {
                                response.Replace("WebPartID=\"" + webPartId,
                                                 "WebPartID=\"" + webPartId + "_SPS_CONTENT_WEBPART_OFF");
                            }

                            current.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("ExcludeContentWebpartFromtrad", e, EventLogEntryType.Warning);
                    }

                    countMsoZoneCell -= 1;
                }

                int index = response.IndexOf("_SPS_CONTENT_WEBPART_OFF");

                while (index > 0)
                {
                    int beginingTdIndex = InsensitiveLastIndexOf(response.Substring(0, index), "<TD ID=\"MSOZONECELL_");

                    string textToParse = response.Substring(beginingTdIndex + 1, response.Length - beginingTdIndex - 1);

                    int closingTd = 0;
                    int openingTd = 1;

                    int endingTdIndex = beginingTdIndex + 1;

                    bool searchWebpart = false;

                    while (closingTd != openingTd)
                    {
                        if (InsensitiveIndexOf(textToParse, "TD NOWRAP CLASS=\"MS-TOOLBAR\" ID=\"", 0) == 0)
                            closingTd += 2;

                        int closingTdIndex = InsensitiveIndexOf(textToParse, "</TD", 0);
                        int openingTdIndex = InsensitiveIndexOf(textToParse, "<TD", 0);

                        if (openingTdIndex == -1)
                        {
                            textToParse = textToParse.Substring(closingTdIndex + 1,
                                                                textToParse.Length - closingTdIndex - 1);
                            break;
                        }

                        if (closingTdIndex < openingTdIndex)
                        {
                            closingTd++;
                            textToParse = textToParse.Substring(closingTdIndex + 1,
                                                                textToParse.Length - closingTdIndex - 1);
                            endingTdIndex += closingTdIndex;
                        }
                        else
                        {
                            openingTd++;
                            textToParse = textToParse.Substring(openingTdIndex + 1,
                                                                textToParse.Length - openingTdIndex - 1);
                            endingTdIndex += openingTdIndex;
                        }

                        if (
                            (response.Substring(beginingTdIndex, endingTdIndex - beginingTdIndex + 4).IndexOf(
                                "<td class=\"ms-advsrchHeadingText\"") != -1) ||
                            (response.Substring(beginingTdIndex, endingTdIndex - beginingTdIndex + 4).IndexOf(
                                "<td class=\"ms-sbcell srch-options\"") != -1))
                        {
                            response.Replace(response.Substring(beginingTdIndex, endingTdIndex - beginingTdIndex + 4),
                                             response.Substring(beginingTdIndex, endingTdIndex - beginingTdIndex + 4).Replace("_SPS_CONTENT_WEBPART_OFF", string.Empty));
                            closingTd = openingTd;

                            searchWebpart = true;
                        }
                    }

                    endingTdIndex = response.IndexOf(textToParse);

                    if (!searchWebpart)
                    {
                        response.Insert(endingTdIndex + 4, "_SPS_END_CONTENT_WEBPART");

                        int endWebpartIndex = 0;

                        MatchCollection result =
                            TranslatorRegex.GreaterThanSmallerThanRegex.Matches(response.Substring(beginingTdIndex,
                                                                                                   endingTdIndex -
                                                                                                   beginingTdIndex + 4));

                        foreach (Match currentMatch in result)
                        {
                            endWebpartIndex = response.IndexOf("_SPS_END_CONTENT_WEBPART");

                            string subString1 = response.Substring(beginingTdIndex,
                                                                   endWebpartIndex - beginingTdIndex + 4);
                            string current1 = currentMatch.Value;
                            string current2 = ">_SPS_CONTENT_WEBPART_VALUE_OFF" +
                                              currentMatch.Value.Substring(1, currentMatch.Value.Length - 2) + "<";
                            string subString2 = subString1;
                            string subString3 = subString2.Replace(current1, current2);
                            response.Replace(subString1, subString3);
                        }

                        result =
                            TranslatorRegex.TextRegex.Matches(response.Substring(beginingTdIndex,
                                                                                 endWebpartIndex - beginingTdIndex + 4));

                        foreach (Match currentMatch in result)
                        {
                            endWebpartIndex = response.IndexOf("_SPS_END_CONTENT_WEBPART");

                            response.Replace(response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4),
                                             response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4).Replace(currentMatch.Value,
                                                         " text=\"_SPS_CONTENT_WEBPART_VALUE_OFF" +
                                                         currentMatch.Value.Substring(7, currentMatch.Value.Length - 8) +
                                                         "\""));
                        }

                        result =
                            TranslatorRegex.TitleRegex.Matches(response.Substring(beginingTdIndex,
                                                                                  endWebpartIndex - beginingTdIndex + 4));

                        foreach (Match currentMatch in result)
                        {
                            endWebpartIndex = response.IndexOf("_SPS_END_CONTENT_WEBPART");

                            response.Replace(response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4),
                                             response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4).Replace(currentMatch.Value,
                                                         " title=\"_SPS_CONTENT_WEBPART_VALUE_OFF" +
                                                         currentMatch.Value.Substring(8, currentMatch.Value.Length - 9) +
                                                         "\""));
                        }

                        result =
                            TranslatorRegex.AltRegex.Matches(response.Substring(beginingTdIndex,
                                                                                endWebpartIndex - beginingTdIndex + 4));

                        foreach (Match currentMatch in result)
                        {
                            endWebpartIndex = response.IndexOf("_SPS_END_CONTENT_WEBPART");

                            response.Replace(response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4),
                                             response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4).Replace(currentMatch.Value,
                                                         " alt=\"_SPS_CONTENT_WEBPART_VALUE_OFF" +
                                                         currentMatch.Value.Substring(6, currentMatch.Value.Length - 7) +
                                                         "\""));
                        }

                        //// To translate the toolbar (up)
                        result =
                            TranslatorRegex.DiidSort.Matches(response.Substring(beginingTdIndex,
                                                                                endWebpartIndex - beginingTdIndex + 4));

                        foreach (Match currentMatch in result)
                        {
                            endWebpartIndex = response.IndexOf("_SPS_END_CONTENT_WEBPART");

                            response.Replace(response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4),
                                             response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4).Replace(currentMatch.Value,
                                                         currentMatch.Value.Replace("_SPS_CONTENT_WEBPART_VALUE_OFF",
                                                                                    string.Empty)));
                        }

                        //// To translate the toolbar (down)
                        result =
                            TranslatorRegex.ImageSrcRect.Matches(response.Substring(beginingTdIndex,
                                                                                    endWebpartIndex - beginingTdIndex +
                                                                                    4));

                        foreach (Match currentMatch in result)
                        {
                            endWebpartIndex = response.IndexOf("_SPS_END_CONTENT_WEBPART");

                            response.Replace(response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4),
                                             response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4).Replace(currentMatch.Value,
                                                         currentMatch.Value.Replace("_SPS_CONTENT_WEBPART_VALUE_OFF",
                                                                                    string.Empty)));
                        }

                        response.Replace(response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4),
                                         response.Substring(beginingTdIndex, endWebpartIndex - beginingTdIndex + 4).Replace("_SPS_CONTENT_WEBPART_OFF",
                                                     string.Empty)).Replace("_SPS_END_CONTENT_WEBPART", string.Empty);
                    }

                    index = response.IndexOf("_SPS_CONTENT_WEBPART_OFF");
                }
            }
            catch (Exception exc)
            {
                Utilities.LogException("ExcludeContentWebpartFromTrad", exc, EventLogEntryType.Warning);
                TraceError(exc);
                response.Clear().Append(save);
            }
            finally
            {
                web.Dispose();
            }
        }

        /// <summary>
        /// Remove a webPart that is not in adequate language
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="itemLanguageToRemove">The item Language To Remove.</param>
        /// <param name="currentLanguage">The current Language.</param>
        /// <param name="allWebPartsContentDisable">The all Web Parts Content Disable.</param>
        /// <param name="webPartIdHashTable">The web Part Id Hash Table.</param>
        public override void UpdateMsoMenuWebPartMenu(StringBuilder response, string itemLanguageToRemove, string currentLanguage, bool allWebPartsContentDisable, Hashtable webPartIdHashTable)
        {
            if (response.IndexOf("MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu") == -1)
            {
                return;
            }

            var save = new StringBuilder(response.ToString());
            HttpContext context = HttpContext.Current;
            var web = Microsoft.SharePoint.WebControls.SPControl.GetContextWeb(context);
            
            try
            {
                int indexMsoZoneCell = response.IndexOf("<TD ID=\"MSOZONECELL_", StringComparison.OrdinalIgnoreCase);

                string value;

                while (indexMsoZoneCell != -1)
                {
                    int indexWebPartId = response.IndexOf(
                        "WebPartID=\"",
                        indexMsoZoneCell,
                        response.Length - indexMsoZoneCell,
                        StringComparison.OrdinalIgnoreCase);

                    if (indexMsoZoneCell < indexWebPartId)
                    {
                        string webPartId = response.Substring(
                            indexWebPartId + 11, response.IndexOf("\"", indexWebPartId + 11) - (indexWebPartId + 11));

                        if (web.AllProperties.ContainsKey("Alphamosaik.Translator.WebParts " + webPartId))
                        {
                            value = (string)web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId];
                        }
                        else
                        {
                            value = string.Empty;
                        }

                        if (value.Contains("_SPS_WEBPART_" + currentLanguage))
                        {
                            string subString = response.Substring(
                                indexMsoZoneCell, indexWebPartId - indexMsoZoneCell - 1);
                            string languageToCheck = currentLanguage;

                            if (value.Contains("_Item_Langage_Enabled"))
                            {
                                if (value.Contains("_SPS_CONTENT_WEBPART_OFF")
                                    || (allWebPartsContentDisable && !value.Contains("_SPS_CONTENT_WEBPART_ON")))
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + languageToCheck
                                            + "_Item_Translation_Enabled_ContentOFF" + ","));
                                }
                                else
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + languageToCheck
                                            + "_Item_Translation_Enabled_ContentON" + ","));
                                }
                            }
                            else
                            {
                                if (value.Contains("_SPS_CONTENT_WEBPART_OFF")
                                    || (allWebPartsContentDisable && !value.Contains("_SPS_CONTENT_WEBPART_ON")))
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + languageToCheck
                                            + "_Item_Translation_Disabled_ContentOFF" + ","));
                                }
                                else
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + languageToCheck
                                            + "_Item_Translation_Disabled_ContentON" + ","));
                                }
                            }
                        }
                        else
                        {
                            string webpartLanguage = "ALL";

                            if (value.Contains("_SPS_WEBPART_"))
                            {
                                webpartLanguage = value.Substring(value.IndexOf("_SPS_WEBPART_") + 13, 2);
                            }

                            string subString = response.Substring(
                                indexMsoZoneCell, indexWebPartId - indexMsoZoneCell - 1);

                            if (value.Contains("_Item_Langage_Enabled"))
                            {
                                if (value.Contains("_SPS_CONTENT_WEBPART_OFF")
                                    || (allWebPartsContentDisable && !value.Contains("_SPS_CONTENT_WEBPART_ON")))
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + webpartLanguage
                                            + "_Item_Translation_Enabled_ContentOFF" + ","));
                                }
                                else
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + webpartLanguage
                                            + "_Item_Translation_Enabled_ContentON" + ","));
                                }
                            }
                            else
                            {
                                if (value.Contains("_SPS_CONTENT_WEBPART_OFF")
                                    || (allWebPartsContentDisable && !value.Contains("_SPS_CONTENT_WEBPART_ON")))
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + webpartLanguage
                                            + "_Item_Translation_Disabled_ContentOFF" + ","));
                                }
                                else
                                {
                                    response.Replace(
                                        subString,
                                        subString.Replace(
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu,",
                                            "onclick=\"MSOWebPartPage_OpenMenu(MSOMenu_WebPartMenu_" + webpartLanguage
                                            + "_Item_Translation_Disabled_ContentON" + ","));
                                }
                            }
                        }
                        //// End We write the call of the appropriate webPart menu
                    }

                    indexMsoZoneCell = response.IndexOf(
                        "<TD ID=\"MSOZONECELL_", indexMsoZoneCell + 11, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Utilities.LogException("UpdateMsoMenuWebPartMenu", exc, EventLogEntryType.Warning);
                TraceError(exc);

                response.Clear().Append(save);
            }
        }

        public override void RemoveTranslatedWebPart(StringBuilder response, Guid webPartIdToRemove)
        {
            if (!IsEditPageMode())
            {
                int indexWebPartId = response.IndexOf("WebPartID=\"" + webPartIdToRemove,
                                                      StringComparison.OrdinalIgnoreCase);

                if (indexWebPartId == -1)
                {
                    return;
                }

                int index = response.LastIndexOf("<TD ID=\"MSOZONECELL_", indexWebPartId,
                                                 StringComparison.OrdinalIgnoreCase);

                if (index == -1)
                {
                    return;
                }

                int indexStart = response.IndexOf("<TD", index + 1, StringComparison.OrdinalIgnoreCase);
                int indexEnd = response.IndexOf("</TD>", index + 1, StringComparison.OrdinalIgnoreCase);

                while (indexStart > -1 && indexStart < indexEnd)
                {
                    indexStart = response.IndexOf("<TD", indexStart + 1, StringComparison.OrdinalIgnoreCase);
                    indexEnd = response.IndexOf("</TD>", indexEnd + 1, StringComparison.OrdinalIgnoreCase);
                }

                response.Remove(index, indexEnd - index + 5);
            }
        }

        /// <summary>
        /// Remove first occurence
        /// </summary>
        /// <param name="str">The string builder.</param>
        /// <param name="search">The text search.</param>
        public void RemoveFirst(StringBuilder str, string search)
        {
            int indexdebut = str.IndexOf(search);

            str.Remove(indexdebut, search.Length);
        }

        public void EnableAutoTranslation(SPList currentList)
        {
            try
            {
                const string AssemblyName = "UpdateItemAutoTranslation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be";
                const string ClassName = "UpdateItemAutoTranslation.UpdateItemAutoTranslationEvent";

                bool isDiscussionBoardList = currentList.BaseTemplate == SPListTemplateType.DiscussionBoard;

                if (!currentList.Fields.ContainsField("ItemsAutoCreation"))
                {
                    var choiceCollection = new StringCollection
                                               {
                                                   "None",
                                                   "Create items for all languages",
                                                   "Overwrite/Create items for all languages"
                                               };
                    currentList.Fields.Add("ItemsAutoCreation", SPFieldType.Choice, false, false, choiceCollection);

                    var fieldToAdd = currentList.Fields["ItemsAutoCreation"] as SPFieldChoice;
                    if (fieldToAdd != null)
                    {
                        fieldToAdd.EditFormat = SPChoiceFormatType.RadioButtons;

                        if (!isDiscussionBoardList)
                            fieldToAdd.DefaultValue = "None";
                        else
                            fieldToAdd.DefaultValue = "Overwrite/Create items for all languages";

                        fieldToAdd.Update();
                    }
                }

                if (!currentList.Fields.ContainsField("MetadataToDuplicate"))
                {
                    currentList.Fields.Add("MetadataToDuplicate", SPFieldType.Text, false);
                    var fieldToAdd = currentList.Fields["MetadataToDuplicate"] as SPFieldText;
                    if (fieldToAdd != null)
                    {
                        fieldToAdd.ShowInDisplayForm = false;
                        fieldToAdd.ShowInListSettings = true;
                        fieldToAdd.ShowInVersionHistory = false;
                        fieldToAdd.ShowInViewForms = false;
                        fieldToAdd.ShowInNewForm = true;
                        fieldToAdd.ShowInEditForm = true;
                        fieldToAdd.Update();
                    }
                }

                if (!currentList.Fields.ContainsField("AutoTranslation"))
                {
                    var choiceCollection = new StringCollection { "No", "Yes" };
                    currentList.Fields.Add("AutoTranslation", SPFieldType.Choice, false, false, choiceCollection);

                    var fieldToAdd = currentList.Fields["AutoTranslation"] as SPFieldChoice;
                    if (fieldToAdd != null)
                    {
                        fieldToAdd.EditFormat = SPChoiceFormatType.RadioButtons;

                        if (!isDiscussionBoardList)
                            fieldToAdd.DefaultValue = "No";
                        else
                            fieldToAdd.DefaultValue = "Yes";

                        fieldToAdd.Update();
                    }
                }

                if (currentList.BaseTemplate == SPListTemplateType.DocumentLibrary)
                {
                    var automaticTranslation = HttpRuntime.Cache[OceanikAutomaticTranslation] as IAutomaticTranslation;

                    if (automaticTranslation != null && automaticTranslation.SupportFileTranslation())
                    {
                        if (!currentList.Fields.ContainsField("Translation Profile"))
                        {
                            currentList.Fields.Add("Translation Profile", SPFieldType.Text, false);
                            var fieldToAdd = currentList.Fields["Translation Profile"] as SPFieldText;
                            if (fieldToAdd != null)
                            {
                                fieldToAdd.ShowInDisplayForm = false;
                                fieldToAdd.ShowInListSettings = true;
                                fieldToAdd.ShowInVersionHistory = false;
                                fieldToAdd.ShowInViewForms = false;
                                fieldToAdd.ShowInNewForm = true;
                                fieldToAdd.ShowInEditForm = true;
                                fieldToAdd.Update();
                            }
                        }
                    }
                }

                foreach (SPContentType contentType in currentList.ContentTypes)
                {
                    if ((!contentType.Fields.ContainsField("ItemsAutoCreation")) && (contentType.Sealed != true))
                    {
                        contentType.FieldLinks.Add(new SPFieldLink(currentList.Fields["ItemsAutoCreation"]));
                        contentType.Update();
                    }

                    if ((!contentType.Fields.ContainsField("MetadataToDuplicate")) && (contentType.Sealed != true))
                    {
                        contentType.FieldLinks.Add(new SPFieldLink(currentList.Fields["MetadataToDuplicate"]));
                        contentType.Update();
                    }

                    if ((!contentType.Fields.ContainsField("AutoTranslation")) && (contentType.Sealed != true))
                    {
                        contentType.FieldLinks.Add(new SPFieldLink(currentList.Fields["AutoTranslation"]));
                        contentType.Update();
                    }

                    if (currentList.Fields.ContainsField("Translation Profile") && (!contentType.Fields.ContainsField("Translation Profile")) && (contentType.Sealed != true))
                    {
                        contentType.FieldLinks.Add(new SPFieldLink(currentList.Fields["Translation Profile"]));
                        contentType.Update();
                    }
                }

                currentList.EventReceivers.Add(SPEventReceiverType.ItemUpdated, AssemblyName, ClassName);
                currentList.EventReceivers.Add(SPEventReceiverType.ItemAdded, AssemblyName, ClassName);

                currentList.Update();
            }
            catch (Exception e)
            {
                Utilities.LogException("EnableAutoTranslation", e, EventLogEntryType.Warning);
            }
        }

        public void DisableAutoTranslation(SPList currentList)
        {
            try
            {
                const string AssemblyName = "UpdateItemAutoTranslation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be";
                const string ClassName = "UpdateItemAutoTranslation.UpdateItemAutoTranslationEvent";

                if (currentList.Fields.ContainsField("AutoTranslation"))
                {
                    currentList.Fields.Delete("AutoTranslation");
                }

                if (currentList.Fields.ContainsField("ItemsAutoCreation"))
                {
                    currentList.Fields.Delete("ItemsAutoCreation");
                }

                if (currentList.Fields.ContainsField("MetadataToDuplicate"))
                {
                    currentList.Fields.Delete("MetadataToDuplicate");
                }

                SPEventReceiverDefinition eventReceiverDefinitionToDelete1 = null;

                foreach (SPEventReceiverDefinition eventReceiverDefinitionTmp in currentList.EventReceivers)
                {
                    if ((eventReceiverDefinitionTmp.Class == ClassName) && (eventReceiverDefinitionTmp.Assembly == AssemblyName))
                    {
                        eventReceiverDefinitionToDelete1 = eventReceiverDefinitionTmp;
                    }
                }

                if (eventReceiverDefinitionToDelete1 != null)
                    eventReceiverDefinitionToDelete1.Delete();

                SPEventReceiverDefinition eventReceiverDefinitionToDelete2 = null;

                foreach (SPEventReceiverDefinition eventReceiverDefinitionTmp in currentList.EventReceivers)
                {
                    if ((eventReceiverDefinitionTmp.Class == ClassName) && (eventReceiverDefinitionTmp.Assembly == AssemblyName))
                    {
                        eventReceiverDefinitionToDelete2 = eventReceiverDefinitionTmp;
                    }
                }

                if (eventReceiverDefinitionToDelete2 != null)
                    eventReceiverDefinitionToDelete2.Delete();

                currentList.Update();
            }
            catch (Exception e)
            {
                Utilities.LogException("DisableAutoTranslation", e, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Allow item translation : add column
        /// </summary>
        /// <param name="web">current WebSite</param>
        /// <param name="webPartId">WebPart Id to extend</param>
        public override void EnableItemTrad(SPWeb web, string webPartId)
        {
            try
            {
                using (SPLimitedWebPartManager manager = web.GetLimitedWebPartManager(HttpContext.Current.Request.RawUrl, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    try
                    {
                        foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                        {
                            try
                            {
                                Guid storageKey = manager.GetStorageKey(current);

                                if (storageKey.ToString() == webPartId)
                                {
                                    if (HasWebPartFunctionnality(web, webPartId))
                                    {
                                        if (!GetWebPartFunctionnality(web, webPartId).Contains("_Item_Langage_Enabled"))
                                        {
                                            web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] += "_Item_Langage_Enabled";
                                        }
                                    }
                                    else
                                    {
                                        web.AllProperties.Add("Alphamosaik.Translator.WebParts " + webPartId, "_Item_Langage_Enabled");
                                    }

                                    web.Update();

                                    var currentListView = current as XsltListViewWebPart;

                                    if (currentListView != null)
                                    {
                                        string listName = currentListView.ListName;

                                        SPListCollection listCollec = web.Lists;

                                        SPList currentList = null;

                                        foreach (SPList currentCList in listCollec)
                                        {
                                            if (currentCList.ID.ToString("B").ToUpper() == listName)
                                                currentList = currentCList;
                                        }

                                        if (currentList != null)
                                        {
                                            SPFieldCollection fields = currentList.Fields;
                                            try
                                            {
                                                string eraseField = string.Empty;

                                                foreach (SPField currentF in fields)
                                                {
                                                    if (currentF.StaticName == "SharePoint_Item_Language")
                                                        eraseField = currentF.InternalName;
                                                }

                                                if (eraseField.Length > 0)
                                                {
                                                    for (int v = 0; v < currentList.Views.Count; v++)
                                                    {
                                                        SPView view = currentList.Views[v];
                                                        SPViewFieldCollection viewFields = view.ViewFields;
                                                        if (!viewFields.Exists("SharePoint_Item_Language"))
                                                        {
                                                            viewFields.Add(currentList.Fields["SharePoint_Item_Language"]);
                                                            view.Update();
                                                        }
                                                    }

                                                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=EnableItemTrad&", "?")
                                                                                              .Replace("&SPS_Trans_Code=EnableItemTrad", string.Empty), false);
                                                    return;
                                                }

                                                var choiceCollection = new StringCollection { "(SPS_LNG_ALL)" };
                                                foreach (string currentLanguageChoice in Languages.Instance.AllLanguages)
                                                {
                                                    choiceCollection.Add("SPS_LNG_" + currentLanguageChoice);
                                                }

                                                currentList.Fields.Add("SharePoint_Item_Language", SPFieldType.Choice, true, false, choiceCollection);

                                                var fieldToAdd = currentList.Fields["SharePoint_Item_Language"] as SPFieldChoice;
                                                if (fieldToAdd != null)
                                                {
                                                    fieldToAdd.EditFormat = SPChoiceFormatType.Dropdown;
                                                    fieldToAdd.DefaultValue = "(SPS_LNG_ALL)";
                                                    fieldToAdd.Update();
                                                }

                                                currentList.Update();

                                                for (int v = 0; v < currentList.Views.Count; v++)
                                                {
                                                    SPView view = currentList.Views[v];
                                                    SPViewFieldCollection viewFields = view.ViewFields;
                                                    if (!viewFields.Exists("SharePoint_Item_Language"))
                                                    {
                                                        viewFields.Add(currentList.Fields["SharePoint_Item_Language"]);
                                                        view.Update();
                                                    }
                                                }

                                                foreach (SPContentType contentType in currentList.ContentTypes)
                                                {
                                                    try
                                                    {
                                                        if ((!contentType.Fields.ContainsField("SharePoint_Item_Language")) && (contentType.Sealed != true) && (contentType.ReadOnly != true))
                                                        {
                                                            contentType.FieldLinks.Add(new SPFieldLink(currentList.Fields["SharePoint_Item_Language"]));
                                                            contentType.Update();
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Utilities.TraceNormalCaughtException("contentType.Fields.ContainsField", e);
                                                    }
                                                }
                                            }
                                            catch (Exception exc)
                                            {
                                                Utilities.LogException("EnableItemTrad", exc, EventLogEntryType.Warning);
                                                TraceError(exc);
                                            }
                                        }

                                        try
                                        {
                                            if (currentList != null)
                                            {
                                                if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                                                {
                                                    SPField fieldGroupLanguage = currentList.Fields["SharePoint_Group_Language"];
                                                    if (fieldGroupLanguage.Required)
                                                    {
                                                        fieldGroupLanguage.Required = false;
                                                        fieldGroupLanguage.Update();
                                                    }
                                                }

                                                if (!currentList.Fields.ContainsField("SharePoint_Group_Language"))
                                                {
                                                    currentList.Fields.Add("SharePoint_Group_Language", SPFieldType.Number, false);

                                                    var fieldToAdd = currentList.Fields["SharePoint_Group_Language"] as SPFieldNumber;
                                                    if (fieldToAdd != null)
                                                    {
                                                        fieldToAdd.DefaultValue = "0";
                                                        fieldToAdd.Update();
                                                    }

                                                    currentList.Update();
                                                }

                                                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                                                {
                                                    if (!currentList.Fields.ContainsField(languageItem.LanguageDestination + " version"))
                                                    {
                                                        currentList.Fields.AddLookup(languageItem.LanguageDestination + " version",
                                                                                     currentList.ID, false);

                                                        var newLookupF = (SPFieldLookup)currentList.Fields[languageItem.LanguageDestination + " version"];
                                                        newLookupF.LookupField = "ID";
                                                        newLookupF.Hidden = true;
                                                        newLookupF.Update();
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Utilities.LogException("EnableItemTrad", e, EventLogEntryType.Warning);
                                        }

                                        try
                                        {
                                            if (currentList != null)
                                                if ((currentList.BaseTemplate == SPListTemplateType.Announcements) || (currentList.BaseTemplate == SPListTemplateType.DiscussionBoard) || (currentList.BaseTemplate == (SPListTemplateType)850)
                                                    || (currentList.BaseTemplate == SPListTemplateType.DocumentLibrary))
                                                {
                                                    var automaticTranslation = HttpRuntime.Cache[OceanikAutomaticTranslation] as IAutomaticTranslation;

                                                    if (automaticTranslation != null &&
                                                        (((currentList.BaseTemplate == SPListTemplateType.DocumentLibrary) && automaticTranslation.SupportFileTranslation()) ||
                                                        (currentList.BaseTemplate != SPListTemplateType.DocumentLibrary)))
                                                        EnableAutoTranslation(currentList);
                                                }
                                        }
                                        catch (Exception e)
                                        {
                                            Utilities.LogException("EnableItemTrad", e, EventLogEntryType.Warning);
                                        }
                                    }
                                }

                                current.Dispose();
                                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=EnableItemTrad&", "?")
                                    .Replace("&SPS_Trans_Code=EnableItemTrad", string.Empty), false);
                            }
                            catch (WebPartPageUserException webPartPageUserException)
                            {
                                Utilities.TraceNormalCaughtException("EnableItemTrad", webPartPageUserException);
                            }
                            catch (Exception e)
                            {
                                Utilities.LogException("EnableItemTrad", e, EventLogEntryType.Warning);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("EnableItemTrad", e, EventLogEntryType.Warning);
                    }
                    finally
                    {
                        manager.Web.Dispose();
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                Utilities.TraceNormalCaughtException("EnableItemTrad", fileNotFoundException);
            }
            catch (Exception exc)
            {
                Utilities.LogException("EnableItemTrad", exc, EventLogEntryType.Warning);
                TraceError(exc);
            }
        }

        /// <summary>
        /// Allow item translation FROM A LIST !: add column
        /// </summary>
        /// <param name="listId">The list Id.</param>
        /// <param name="url">The current url.</param>
        public override void EnableItemTradFromList(string listId, string url)
        {
            try
            {
                using (var currentSite = new SPSite(url))
                using (SPWeb web = currentSite.OpenWeb())
                {
                    try
                    {
                        web.AllowUnsafeUpdates = true;
                        SPList currentList = web.Lists[new Guid(listId)];

                        SPFieldCollection fields = currentList.Fields;
                        try
                        {
                            string eraseField = string.Empty;

                            foreach (SPField currentF in fields)
                            {
                                if (currentF.StaticName == "SharePoint_Item_Language")
                                    eraseField = currentF.InternalName;
                            }

                            if (eraseField.Length <= 0)
                            {
                                var choiceCollection = new StringCollection { "(SPS_LNG_ALL)" };
                                foreach (string currentLanguageChoice in Languages.Instance.AllLanguages)
                                {
                                    choiceCollection.Add("SPS_LNG_" + currentLanguageChoice);
                                }

                                currentList.Fields.Add("SharePoint_Item_Language", SPFieldType.Choice, true, false, choiceCollection);

                                var fieldToAdd = currentList.Fields["SharePoint_Item_Language"] as SPFieldChoice;
                                if (fieldToAdd != null)
                                {
                                    fieldToAdd.EditFormat = SPChoiceFormatType.Dropdown;
                                    fieldToAdd.DefaultValue = "(SPS_LNG_ALL)";
                                    fieldToAdd.Update();
                                }

                                currentList.Update();

                                foreach (SPContentType contentType in currentList.ContentTypes)
                                {
                                    try
                                    {
                                        if ((!contentType.Fields.ContainsField("SharePoint_Item_Language")) && (contentType.Sealed != true) && (contentType.ReadOnly != true))
                                        {
                                            contentType.FieldLinks.Add(new SPFieldLink(currentList.Fields["SharePoint_Item_Language"]));
                                            contentType.Update();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Utilities.TraceNormalCaughtException("contentType.Fields.ContainsField", e);
                                    }
                                }

                                for (int v = 0; v < currentList.Views.Count; v++)
                                {
                                    SPView view = currentList.Views[v];
                                    SPViewFieldCollection viewFields = view.ViewFields;
                                    viewFields.Add(currentList.Fields["SharePoint_Item_Language"]);
                                    view.Update();
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            Utilities.LogException("EnableItemTradFromList", exc, EventLogEntryType.Warning);
                            TraceError(exc);
                        }

                        try
                        {
                            if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                            {
                                SPField fieldGroupLanguage = currentList.Fields["SharePoint_Group_Language"];
                                if (fieldGroupLanguage.Required)
                                {
                                    fieldGroupLanguage.Required = false;
                                    fieldGroupLanguage.Update();
                                }
                            }

                            if (!currentList.Fields.ContainsField("SharePoint_Group_Language"))
                            {
                                currentList.Fields.Add("SharePoint_Group_Language", SPFieldType.Number, false);

                                var fieldToAdd = currentList.Fields["SharePoint_Group_Language"] as SPFieldNumber;
                                if (fieldToAdd != null)
                                {
                                    fieldToAdd.DefaultValue = "0";
                                    fieldToAdd.Update();
                                }

                                currentList.Update();
                            }

                            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                            {
                                if (!currentList.Fields.ContainsField(languageItem.LanguageDestination + " version"))
                                {
                                    currentList.Fields.AddLookup(languageItem.LanguageDestination + " version", currentList.ID, false);

                                    var newLookupField = (SPFieldLookup)currentList.Fields[languageItem.LanguageDestination + " version"];
                                    newLookupField.LookupField = "ID";
                                    newLookupField.Hidden = true;
                                    newLookupField.Update();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Utilities.LogException("EnableItemTradFromList", e, EventLogEntryType.Warning);
                        }

                        try
                        {
                            if (currentList.BaseTemplate == SPListTemplateType.Announcements ||
                                currentList.BaseTemplate == SPListTemplateType.DiscussionBoard ||
                                currentList.BaseTemplate == (SPListTemplateType)850 ||
                                currentList.BaseTemplate == SPListTemplateType.DocumentLibrary)
                            {
                                var automaticTranslation = HttpRuntime.Cache[OceanikAutomaticTranslation] as IAutomaticTranslation;

                                if (automaticTranslation != null &&
                                    (((currentList.BaseTemplate == SPListTemplateType.DocumentLibrary) && automaticTranslation.SupportFileTranslation()) ||
                                    (currentList.BaseTemplate != SPListTemplateType.DocumentLibrary)))
                                    EnableAutoTranslation(currentList);
                            }
                        }
                        catch (Exception e)
                        {
                            Utilities.LogException("EnableItemTradFromList", e, EventLogEntryType.Warning);
                        }

                        web.AllowUnsafeUpdates = false;
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("EnableItemTradFromList", e, EventLogEntryType.Warning);
                        web.AllowUnsafeUpdates = false;
                    }
                }

                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=EnableItemTrad&", "?")
                                            .Replace("&SPS_Trans_Code=EnableItemTrad", string.Empty), false);
            }
            catch (Exception exc)
            {
                Utilities.LogException("EnableItemTradFromList", exc, EventLogEntryType.Warning);
                TraceError(exc);
            }
        }

        /// <summary>
        /// Remove item translation feature
        /// </summary>
        /// <param name="web">The web object.</param>
        /// <param name="webPartId">The web Part Id.</param>
        public override void DisableItemTrad(SPWeb web, string webPartId)
        {
            try
            {
                using (SPLimitedWebPartManager manager = web.GetLimitedWebPartManager(HttpContext.Current.Request.RawUrl, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    try
                    {
                        foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                        {
                            try
                            {
                                Guid storageKey = manager.GetStorageKey(current);

                                if (storageKey.ToString() == webPartId)
                                {
                                    if (HasWebPartFunctionnality(web, webPartId))
                                    {
                                        if (GetWebPartFunctionnality(web, webPartId).Contains("_Item_Langage_Enabled"))
                                        {
                                            web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] = ((string)web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId]).Replace("_Item_Langage_Enabled", string.Empty);
                                            web.Update();
                                        }
                                    }

                                    var currentListView = current as XsltListViewWebPart;

                                    if (currentListView != null)
                                    {
                                        string listName = currentListView.ListName;

                                        SPListCollection listCollec = web.Lists;

                                        SPList currentList = null;

                                        foreach (SPList currentCList in listCollec)
                                        {
                                            if (currentCList.ID.ToString("B").ToUpper() == listName)
                                                currentList = currentCList;
                                        }

                                        if (currentList != null)
                                        {
                                            SPFieldCollection fields = currentList.Fields;
                                            try
                                            {
                                                string eraseField = string.Empty;

                                                foreach (SPField currentF in fields)
                                                {
                                                    if (currentF.StaticName == "SharePoint_Item_Language")
                                                        eraseField = currentF.InternalName;
                                                }

                                                if (eraseField.Length > 0)
                                                {
                                                    fields.Delete(eraseField);
                                                }

                                                SPField eraseFieldObj;

                                                if (fields.ContainsField("SharePoint_Group_Language"))
                                                {
                                                    fields.Delete(fields.GetField("SharePoint_Group_Language").InternalName);
                                                }

                                                foreach (string languageDestination in Languages.Instance.AllLanguages)
                                                {
                                                    if (fields.ContainsField(languageDestination + " version"))
                                                    {
                                                        eraseFieldObj = fields.GetField(languageDestination + " version");
                                                        eraseFieldObj.Hidden = false;
                                                        eraseFieldObj.Update();
                                                        eraseField = fields.GetField(languageDestination + " version").InternalName;
                                                        fields.Delete(eraseField);
                                                    }
                                                }

                                                DisableAutoTranslation(currentList);

                                                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=DisableItemTrad&", "?")
                                                                                          .Replace("&SPS_Trans_Code=DisableItemTrad", string.Empty), false);
                                                return;
                                            }
                                            catch (Exception exc)
                                            {
                                                Utilities.LogException("DisableItemTrad", exc, EventLogEntryType.Warning);
                                                TraceError(exc);
                                            }
                                        }
                                    }
                                }

                                current.Dispose();
                            }
                            catch (WebPartPageUserException webPartPageUserException)
                            {
                                Utilities.TraceNormalCaughtException("DisableItemTrad", webPartPageUserException);
                            }
                            catch (Exception e)
                            {
                                Utilities.LogException("DisableItemTrad", e, EventLogEntryType.Warning);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("DisableItemTrad", e, EventLogEntryType.Warning);
                    }
                    finally
                    {
                        manager.Web.Dispose();
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                Utilities.TraceNormalCaughtException("DisableItemTrad", fileNotFoundException);
            }
            catch (Exception exc)
            {
                Utilities.LogException("DisableItemTrad", exc, EventLogEntryType.Warning);
                TraceError(exc);
            }
        }

        public override void DisableItemTradFromList(SPWeb inputSpWeb, string listId, string url)
        {
            try
            {
                using (var currentSite = new SPSite(url))
                using (SPWeb web = currentSite.OpenWeb())
                using (SPLimitedWebPartManager manager = inputSpWeb.GetLimitedWebPartManager(HttpContext.Current.Request.RawUrl, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    try
                    {
                        web.AllowUnsafeUpdates = true;
                        SPList currentList = web.Lists[new Guid(listId)];
                        SPFieldCollection fields = currentList.Fields;
                        try
                        {
                            string eraseField = string.Empty;

                            foreach (SPField currentF in fields)
                            {
                                if (currentF.StaticName == "SharePoint_Item_Language")
                                    eraseField = currentF.InternalName;
                            }

                            if (eraseField.Length > 0)
                            {
                                fields.Delete(eraseField);
                            }

                            SPField eraseFieldObj;

                            if (fields.ContainsField("SharePoint_Group_Language"))
                            {
                                fields.Delete(fields.GetField("SharePoint_Group_Language").InternalName);
                            }

                            foreach (string languageDestination in Languages.Instance.AllLanguages)
                            {
                                if (fields.ContainsField(languageDestination + " version"))
                                {
                                    eraseFieldObj = fields.GetField(languageDestination + " version");
                                    eraseFieldObj.Hidden = false;
                                    eraseFieldObj.Update();

                                    eraseField = fields.GetField(languageDestination + " version").InternalName;
                                    fields.Delete(eraseField);
                                }
                            }

                            if (currentList.Fields.ContainsField("TitleForTranslator"))
                            {
                                currentList.Fields.Delete("TitleForTranslator");
                            }

                            DisableAutoTranslation(currentList);

                            HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=DisableItemTrad&", "?")
                                .Replace("&SPS_Trans_Code=DisableItemTrad", string.Empty), false);
                            return;
                        }
                        catch (Exception exc)
                        {
                            Utilities.LogException("DisableItemTradFromList", exc, EventLogEntryType.Warning);
                            TraceError(exc);
                        }
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("DisableItemTradFromList", e, EventLogEntryType.Warning);
                    }
                    finally
                    {
                        manager.Web.Dispose();
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                Utilities.TraceNormalCaughtException("DisableItemTradFromList", fileNotFoundException);
            }
            catch (Exception exc)
            {
                Utilities.LogException("DisableItemTradFromList", exc, EventLogEntryType.Warning);
                TraceError(exc);
            }
        }

        /// <summary>
        /// Swith a webpart to another language
        /// </summary>
        /// <param name="web">The web object.</param>
        /// <param name="webPartId">The web Part Id.</param>
        /// <param name="lang">The language to swicth to.</param>
        public override void SwitchToLanguage(SPWeb web, string webPartId, string lang)
        {
            try
            {
                bool webpartPropertiesExist = HasWebPartFunctionnality(web, webPartId);
                string value = string.Empty;

                if (webpartPropertiesExist)
                {
                    value = GetWebPartFunctionnality(web, webPartId);

                    foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                        value = value.Replace("_SPS_WEBPART_" + languageItem.LanguageDestination, string.Empty);
                }

                if (lang != "ALL")
                    value += "_SPS_WEBPART_" + lang;

                if (webpartPropertiesExist)
                {
                    web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] = value;
                }
                else
                {
                    web.AllProperties.Add("Alphamosaik.Translator.WebParts " + webPartId, value);
                }

                web.AllowUnsafeUpdates = true;
                web.Update();
                web.AllowUnsafeUpdates = false;
            }
            catch (Exception exc)
            {
                Utilities.LogException("SwitchToLanguage", exc, EventLogEntryType.Warning);
                TraceError(exc);
            }
        }

        /// <summary>
        /// Disable traduction of a WebPart content
        /// </summary>
        /// <param name="web">The web object.</param>
        /// <param name="webPartId">The web Part Id.</param>
        public override void DisableWebpartContentTrad(SPWeb web, string webPartId)
        {
            try
            {
                using (SPLimitedWebPartManager manager = web.GetLimitedWebPartManager(HttpContext.Current.Request.RawUrl, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    try
                    {
                        foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                        {
                            try
                            {
                                Guid storageKey = manager.GetStorageKey(current);

                                if (storageKey.ToString() == webPartId)
                                {
                                    if (HasWebPartFunctionnality(web, webPartId))
                                    {
                                        web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] = ((string)web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId])
                                            .Replace("_SPS_CONTENT_WEBPART_OFF", string.Empty).Replace("_SPS_CONTENT_WEBPART_ON", string.Empty);
                                        web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] += "_SPS_CONTENT_WEBPART_OFF";
                                    }
                                    else
                                    {
                                        web.AllProperties.Add("Alphamosaik.Translator.WebParts " + webPartId, "_SPS_CONTENT_WEBPART_OFF");
                                    }

                                    web.Update();
                                }

                                current.Dispose();
                            }
                            catch (WebPartPageUserException webPartPageUserException)
                            {
                                Utilities.TraceNormalCaughtException("DisableWebpartContentTrad", webPartPageUserException);
                            }
                            catch (Exception e)
                            {
                                Utilities.LogException("DisableWebpartContentTrad", e, EventLogEntryType.Warning);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("DisableWebpartContentTrad", e, EventLogEntryType.Warning);
                    }
                    finally
                    {
                        manager.Web.Dispose();
                    }
                }

                HttpContext.Current.Cache.Remove("SPS_HASHCODES_PAGES");
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                Utilities.TraceNormalCaughtException("DisableWebpartContentTrad", fileNotFoundException);
            }
            catch (Exception exc)
            {
                Utilities.LogException("DisableWebpartContentTrad", exc, EventLogEntryType.Warning);
                TraceError(exc);
            }
        }

        /// <summary>
        /// Enable traduction of a WebPart content
        /// </summary>
        /// <param name="web">The sp Web.</param>
        /// <param name="webPartId">The web Part Id.</param>
        public override void EnableWebpartContentTrad(SPWeb web, string webPartId)
        {
            try
            {
                using (SPLimitedWebPartManager manager = web.GetLimitedWebPartManager(HttpContext.Current.Request.RawUrl, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    try
                    {
                        foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                        {
                            try
                            {
                                Guid storageKey = manager.GetStorageKey(current);

                                if (storageKey.ToString() == webPartId)
                                {
                                    if (HasWebPartFunctionnality(web, webPartId))
                                    {
                                        web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] = ((string)web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId])
                                            .Replace("_SPS_CONTENT_WEBPART_OFF", string.Empty).Replace("_SPS_CONTENT_WEBPART_ON", string.Empty);
                                        web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] += "_SPS_CONTENT_WEBPART_ON";
                                    }
                                    else
                                    {
                                        web.AllProperties.Add("Alphamosaik.Translator.WebParts " + webPartId, "_SPS_CONTENT_WEBPART_ON");
                                    }

                                    web.Update();
                                }

                                current.Dispose();
                            }
                            catch (WebPartPageUserException webPartPageUserException)
                            {
                                Utilities.TraceNormalCaughtException("EnableWebpartContentTrad", webPartPageUserException);
                            }
                            catch (Exception e)
                            {
                                Utilities.LogException("EnableWebpartContentTrad", e, EventLogEntryType.Warning);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("EnableWebpartContentTrad", e, EventLogEntryType.Warning);
                    }
                    finally
                    {
                        manager.Web.Dispose();
                    }
                }

                HttpContext.Current.Cache.Remove("SPS_HASHCODES_PAGES");
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                Utilities.TraceNormalCaughtException("EnableWebpartContentTrad", fileNotFoundException);
            }
            catch (Exception exc)
            {
                Utilities.LogException("EnableWebpartContentTrad", exc, EventLogEntryType.Warning);
                TraceError(exc);
            }
        }

        public override void RemoveUntil(StringBuilder response, string textToRemove, string until)
        {
            if (response.IndexOf(until) == -1)
            {
                response.Replace(textToRemove, string.Empty);
                return;
            }

            int currentIndex = response.IndexOf(textToRemove);

            while (currentIndex != -1)
            {
                currentIndex = response.IndexOf(textToRemove);
                int currentUntil = response.IndexOf(until);

                if (currentIndex == -1)
                {
                    return;
                }

                if (currentUntil == -1 || currentIndex < currentUntil)
                    RemoveFirst(response, textToRemove);

                if (currentIndex > currentUntil)
                    return;
            }
        }

        public void TableTranslationsFilter(CustomStringCollection translationsSpsUrl)
        {
            if (HttpContext.Current == null)
                return;

            try
            {
                if (translationsSpsUrl.Count > 0)
                {
                    string urlFromTable;
                    string urlFull = HttpContext.Current.Request.Url.AbsolutePath.StartsWith("/_") ? HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Replace(HttpContext.Current.Request.Url.AbsolutePath.ToLower(), HttpContext.Current.Request.RawUrl.ToLower()) : HttpContext.Current.Request.Url.AbsoluteUri.ToLower();

                    for (int i = 0; i < translationsSpsUrl.Count; i++)
                    {
                        if (!TranslatorRegex.SpsUrlRegex.IsMatch(translationsSpsUrl[i]))
                        {
                            translationsSpsUrl[i] = string.Empty;
                        }
                        else
                        {
                            string[] translationsSpsUrlSplitted = translationsSpsUrl[i].Replace(" $$SPS_URL:", string.Empty).Replace("$$SPS_URL:", string.Empty).Split('$');
                            urlFromTable = translationsSpsUrlSplitted[0].Trim();
                            if (urlFull.Contains(urlFromTable.ToLower()) && (!string.IsNullOrEmpty(urlFromTable)))
                            {
                                translationsSpsUrl[i] = translationsSpsUrlSplitted[2].Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("TableTranslationFilter", e, EventLogEntryType.Warning);
            }
        }

        public override void AddPageUrlForExtractor(string url)
        {
            try
            {
                string currentUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);

                if (currentUrl.Contains(".") && (!currentUrl.Contains("/Lists/TranslationContents/")) && (!currentUrl.Contains("/Lists/ExtractorTranslations/")))
                    if ((currentUrl.ToLower().Substring(currentUrl.LastIndexOf("."), currentUrl.Length - currentUrl.LastIndexOf(".")) == ".aspx")
                        || (currentUrl.ToLower().Substring(currentUrl.LastIndexOf("."), currentUrl.Length - currentUrl.LastIndexOf(".")) == ".html")
                        || (currentUrl.ToLower().Substring(currentUrl.LastIndexOf("."), currentUrl.Length - currentUrl.LastIndexOf(".")) == ".htm"))
                    {
                        using (var currentSite = new SPSite(url))
                        using (SPWeb web = currentSite.OpenWeb())
                        {
                            SPList extractorTranslationsList = web.GetList("/Lists/PagesToUpdateForTranslation");

                            if (extractorTranslationsList.Fields.ContainsField("Pages"))
                            {
                                var query = new SPQuery
                                {
                                    Query = "<Where><Eq><FieldRef Name='" + "Pages" + "'/>" +
                                            "<Value Type='Text'>" + currentUrl.Trim() +
                                            "</Value></Eq></Where>",
                                    QueryThrottleMode = SPQueryThrottleOption.Override
                                };

                                SPListItemCollection collListItems = extractorTranslationsList.GetItems(query);

                                if (collListItems.Count == 0)
                                {
                                    SPListItem newItem = extractorTranslationsList.Items.Add();
                                    newItem["Pages"] = currentUrl;

                                    web.AllowUnsafeUpdates = true;
                                    newItem.SystemUpdate(false);
                                    web.AllowUnsafeUpdates = false;
                                }
                            }
                        }
                    }
            }
            catch (Exception e)
            {
                Utilities.LogException("AddPageUrlForExtractor", e, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Write a page in a given language
        /// </summary>
        /// <param name="html">
        /// The current html.
        /// </param>
        /// <param name="languageDestination">
        /// The language Code.
        /// </param>
        /// <param name="responseStream">
        /// The response Stream.
        /// </param>
        /// <param name="viewAllItemsInEveryLanguages">
        /// The view All Items In Every Languages.
        /// </param>
        /// <param name="completingDictionaryMode">
        /// The completing Dictionary Mode.
        /// </param>
        /// <param name="extractorStatus">
        /// The extractor Status.
        /// </param>
        /// <param name="url">
        /// The current url.
        /// </param>
        /// <param name="autocompletionStatus">
        /// The autocompletion Status.
        /// </param>
        /// <param name="mobilePage">
        /// The mobile Page.
        /// </param>
        /// <param name="licenseType">
        /// The is the license type.
        /// </param>
        public override void Write(StringBuilder html, string languageDestination, Stream responseStream, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, int extractorStatus, string url, int autocompletionStatus, bool mobilePage, License.LicenseType licenseType)
        {
            if (HttpContext.Current == null)
                return;

            bool filteringDisplayCompleted = false;
            bool ajaxResponse = false;

            int indexOfFirstPipe = html.IndexOf("|");

            if (indexOfFirstPipe != -1 && indexOfFirstPipe < 10)
            {
                int returnValue;
                string inputString = html.Substring(0, indexOfFirstPipe);
                ajaxResponse = int.TryParse(inputString, NumberStyles.Integer, null, out returnValue);
            }

            // essai filtration
            // html.Replace("queryString=\"?\"", "queryString=\"?FilterField1=SharePoint_Item_Language&FilterValue1=SPS_LNG_EN\"");

            // Test for inner frame -- popup style : if not then duplicate translation bar appears
            if ((ajaxResponse || html.LastIndexOf("</html>") != -1 || HttpContext.Current.Response.ContentType == "application/json") && html.IndexOf("<iframe id=\"innerFrame\"") == -1 &&
                !(HttpContext.Current.Request.Path.IndexOf("picker.aspx", StringComparison.OrdinalIgnoreCase) != -1 && HttpContext.Current.Request.HttpMethod.IndexOf("POST", StringComparison.OrdinalIgnoreCase) != -1))
            {
                // Not inner Frame
                if ((bool)HttpContext.Current.Cache["AlphamosaikRedirectToLinkedPage"])
                    RedirectToLinkedPage(languageDestination);

                const string HiddenTagTranslationsDictionary =
                    @"<input type=""hidden"" id=""Translation-Dictionary-Tag-2C4A35CB-EB45-4deb-A2A8-52621A0953D7""\>";

                if (html.IndexOf(HiddenTagTranslationsDictionary) == -1)
                {
                    var languageSource = (string)HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"];

                    try
                    {
                        languageSource = Languages.Instance.GetBackwardCompatibilityLanguageCode(SPContext.Current.Web.UICulture.TwoLetterISOLanguageName.ToUpper());
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("Write", e, EventLogEntryType.Warning);
                    }

                    FilterCalendarView(languageDestination, languageSource);

                    // Translation is needed
                    // Cas où on est sur la page de langue par défaut dans le processus de comparaison des 2 pages pour l'autocompletion
                    if (autocompletionStatus == 1)
                    {
                        if (HttpContext.Current.Request.QueryString["InitLanguage"] != null)
                        {
                            string initLanguage = HttpContext.Current.Request.QueryString["InitLanguage"];
                            AutoCompletionModeDefaultPageProcess(html, languageSource, initLanguage);
                        }
                    }

                    var removedAlphaNoCacheDivForCache = new ArrayList();

                    if (html.IndexOf("<div class=\"AlphaNoCache", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        removedAlphaNoCacheDivForCache = RemoveTranslatedDivForCache(html, "AlphaNoCache");
                    }

                    // Test if page is cached
                    string requestDigest;
                    string viewState;
                    string eventValidation;
                    string currentUserId;
                    string userName;
                    ArrayList ctxIdList;
                    ArrayList imnTypeSmtpList;
                    ArrayList removedTranslatedDivForCache;
                    string includeTimeValueDateTime;
                    string accountName;
                    string qLogEnv;

                    string currentHashCode = GetPageHashCode(
                        languageDestination,
                        html,
                        out requestDigest,
                        out viewState,
                        out eventValidation,
                        out currentUserId,
                        out userName,
                        out ctxIdList,
                        out includeTimeValueDateTime,
                        out imnTypeSmtpList,
                        out removedTranslatedDivForCache,
                        out accountName,
                        out qLogEnv);

                    // Page is in cache
                    if ((HttpContext.Current.Cache["SPS_HASHCODES_PAGES"] != null) &&
                        (HttpContext.Current.Cache[currentHashCode] != null) && (autocompletionStatus != 2) &&
                        !_useCacheDisabled)
                    {
                        var currentHashCodeArrayList =
                            HttpContext.Current.Cache["SPS_HASHCODES_PAGES"] as StringCollection;

                        bool pagesToReload = true;
                        if (currentHashCodeArrayList != null)
                        {
                            if (currentHashCodeArrayList.IndexOf(currentHashCode) >= 0)
                            {
                                pagesToReload = false;
                            }
                        }

                        if (extractorStatus != -1)
                            pagesToReload = true;

                        if (!pagesToReload)
                        {
                            html.SetNewString(GetPageFromCache(currentHashCode, requestDigest, viewState,
                                                                   eventValidation, userName, currentUserId,
                                                                   ctxIdList, includeTimeValueDateTime,
                                                                   imnTypeSmtpList, removedTranslatedDivForCache,
                                                                   accountName, currentHashCodeArrayList, url,
                                                                   extractorStatus, languageSource, languageDestination,
                                                                   removedAlphaNoCacheDivForCache, qLogEnv));

                            bool writedCache = WriteBuffersForOutputCache(html.ToString());

                            if (!writedCache)
                            {
                                HttpContext.Current.Response.ClearContent();

                                byte[] dataCached = Encoding.UTF8.GetBytes(html.ToString());

                                responseStream.Write(dataCached, 0, dataCached.Length);
                            }

                            return;
                        }
                    }

                    // Cas où on est sur la page de langue de départ dans le processus de comparaison des 2 pages pour l'autocompletion
                    if (autocompletionStatus == 2 && !ajaxResponse)
                    {
                        //// Translate the Html Tag
                        TranslateFromDictionary(html, languageDestination,
                                                languageSource, extractorStatus, url);
                        AutoCompletionModeInitPageProcess(html, languageDestination, languageSource);
                    }

                    var excludedPartsFromTranslation = new Dictionary<int, string>();
                    AddHelper(html, languageDestination, viewAllItemsInEveryLanguages, completingDictionaryMode,
                              languageSource,
                              ref filteringDisplayCompleted, mobilePage, licenseType, ref excludedPartsFromTranslation);

                    FormatRibbonUrl(html);

                    //// Clean up the codes for the functionalitie "WebParts content translation"
                    if (languageSource == languageDestination)
                    {
                        html.Replace("_SPS_CONTENT_WEBPART_VALUE_OFF", string.Empty);
                    }

                    html.Replace("_SPS_CONTENT_WEBPART_ON", string.Empty);
                    html.Replace("_SPS_CONTENT_WEBPART_OFF", string.Empty);

                    ConvertAsciiCode(html);

                    QuickLaunchFilter(html, languageDestination);

                    TopNavigationBarFilter(html, languageDestination);

                    FilterTocLayoutMain(html, languageDestination, url);

                    ReplaceLinkedPagesUrl(html, languageDestination);

                    AddCheckMarkToPersonalLanguageMenu(html, languageDestination);

                    if (languageSource != languageDestination)
                    {
                        bool editing = IsEditPageMode() && (HttpContext.Current.Request.Url.ToString().IndexOf("EditForm.aspx", StringComparison.OrdinalIgnoreCase) == -1)
                            && (HttpContext.Current.Request.Url.ToString().IndexOf("NewForm.aspx", StringComparison.OrdinalIgnoreCase) == -1);

                        //// Translate the Html Tag
                        if (autocompletionStatus != 2 && !ajaxResponse && !editing)
                        {
                            TranslateFromDictionary(html, languageDestination, languageSource, extractorStatus, url);
                        }

                        //// Clean up the codes for the functionalitie "WebParts content translation"
                        html.Replace("_SPS_CONTENT_WEBPART_VALUE_OFF", string.Empty);

                        // set language right to left for arabic
                        const string DirRtl = "dir=\"rtl\"";
                        const string DirLtr = "dir=\"ltr\"";
                        if (languageDestination == "AR")
                            html.Replace(DirLtr, DirRtl);
                    }

                    InsertExcludedPartsFromTranslation(html, excludedPartsFromTranslation);

                    if (ajaxResponse)
                    {
                        AjaxFormating(html);
                    }

                    RemoveNoTranslateTag(html);

                    if (HttpContext.Current.Request.Path.Contains("calendar.aspx"))
                    {
                        TranslateTagMoveToDate(html, languageSource, extractorStatus, url, languageDestination);
                    }

                    if (html.IndexOf("####AlphaNoCache") > 0)
                    {
                        int cmpt = 0;
                        foreach (string removedCachedString in removedAlphaNoCacheDivForCache)
                        {
                            html.Replace("####AlphaNoCache" + cmpt + "####", removedCachedString);
                            cmpt++;
                        }
                    }

                    if (!filteringDisplayCompleted)
                    {
                        BuildPageForCache(
                            html,
                            currentHashCode,
                            currentUserId,
                            userName,
                            includeTimeValueDateTime,
                            accountName,
                            removedTranslatedDivForCache,
                            languageDestination);
                    }
                }

                bool writed = WriteBuffersForOutputCache(html.ToString());

                if (!writed)
                {
                    HttpContext.Current.Response.ClearContent();

                    byte[] dataTranslated = Encoding.UTF8.GetBytes(html.ToString());

                    responseStream.Write(dataTranslated, 0, dataTranslated.Length);
                }

                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(html.ToString());
            responseStream.Write(data, 0, data.Length);
        }

        public override void InitializeCache(SPContext current, string siteUrl, bool reloadCustomCache, bool reloadGlobalCache)
        {
            try
            {
                string rootUrl = Alphamosaik.Common.SharePoint.Library.Utilities.FilterUrl(current.Site.RootWeb.Url); // root server url

                if (reloadGlobalCache)
                {
                    InitializeGlobalDictionaryCache(rootUrl);
                    InitializeCustomDictionaryCache(rootUrl, "TranslationContents", reloadCustomCache);
                }

                SPList list = current.Web.Lists.TryGetList("TranslationContentsSub");

                if (list != null)
                {
                    InitializeCustomDictionaryCache(siteUrl, "TranslationContentsSub", reloadCustomCache);
                }
                else if (!reloadGlobalCache)
                {
                    InitializeCustomDictionaryCache(siteUrl, "TranslationContents", reloadCustomCache);
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("InitializeCache", e, EventLogEntryType.Warning);
            }
        }

        public override void RobotWrite(StringBuilder tempResponse, Stream responseStream, string siteUrl)
        {
            if (HttpContext.Current == null)
                return;

            var languageSource = (string)HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"];

            try
            {
                languageSource = Languages.Instance.GetBackwardCompatibilityLanguageCode(SPContext.Current.Web.UICulture.TwoLetterISOLanguageName.ToUpper());
            }
            catch (Exception e)
            {
                Utilities.LogException("RobotWrite", e, EventLogEntryType.Warning);
            }

            bool viewAllItemsInEveryLanguages = (bool)HttpContext.Current.Cache["AlphamosaikItemFiltering"] == false;

            if (HttpContext.Current.Request.UrlReferrer != null)
                if (HttpContext.Current.Request.UrlReferrer.ToString().Contains("SPS_Trans_Code_Pers=Unfiltering"))
                {
                    // View All Items In Every Language
                    viewAllItemsInEveryLanguages = true;
                }

            ConvertAsciiCode(tempResponse);

            bool editMode = IsEditPageMode() || tempResponse.IndexOf("action=\"EditForm.aspx") != -1 ||
                            tempResponse.IndexOf("action=\"ViewEdit.aspx") != -1;

            RemoveSpsTagsBeforeAddingMenus(tempResponse, viewAllItemsInEveryLanguages, editMode);
            RemoveSpsWebPartAfterAddingMenus(tempResponse);
            RemoveSpsTagsAfterAddingMenus(tempResponse, false, false, null);

            var tempResponseMultiLanguage = new StringBuilder(tempResponse.ToString());
            var tempResponseBeforeLastTreatment = new StringBuilder(tempResponse.ToString());

            foreach (LanguageItem languageItem1 in Dictionaries.Instance.VisibleLanguages)
            {
                string languageDestination = languageItem1.LanguageDestination;

                foreach (LanguageItem languageItem2 in Dictionaries.Instance.VisibleLanguages)
                {
                    string language = languageItem2.LanguageDestination;

                    if (language != languageDestination)
                    {
                        // Tag for master page
                        RemoveTranslatedTr(tempResponse, "SPS_STATIC_LNG_MASTERPAGE_" + language);

                        if (!viewAllItemsInEveryLanguages && !editMode)
                        {
                            RemoveTranslatedTr(tempResponse, "SPS_LNG_" + language);
                            RemoveTranslatedDiv(tempResponse, "SPS_STATIC_LNG_" + language);
                            RemoveTranslatedTr(tempResponse, "SPS_STATIC_LNG_" + language);
                        }
                    }
                    else
                    {
                        string sharePointItemLanguageToRemove = "<option value=\"SPS_LNG_" + language +
                                                                "\">SPS_LNG_" + language + "</option>";
                        tempResponse.Replace(sharePointItemLanguageToRemove, string.Empty);
                    }
                }

                if (languageSource != languageDestination)
                {
                    //// Translate the Html Tag
                    if (!IsEditPageMode())
                    {
                        TranslateFromDictionary(tempResponse, languageDestination, languageSource, -1, siteUrl);
                    }

                    // set language right to left for arabic
                    const string DirRtl = "dir=\"rtl\"";
                    const string DirLtr = "dir=\"ltr\"";
                    if (languageDestination == "AR")
                        tempResponse.Replace(DirLtr, DirRtl);
                }

                tempResponseMultiLanguage.Append(tempResponse);
                tempResponse = new StringBuilder(tempResponseBeforeLastTreatment.ToString());
            }

            byte[] data = Encoding.UTF8.GetBytes(tempResponseMultiLanguage.ToString());

            HttpContext.Current.Response.ClearContent();

            responseStream.Write(data, 0, data.Length);
        }

        public override void TranslateInplaceView(StringBuilder html, string languageDestination, Stream responseStream, string siteUrl)
        {
            if (HttpContext.Current == null)
                return;

            var languageSource = (string)HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"];

            try
            {
                languageSource = Languages.Instance.GetBackwardCompatibilityLanguageCode(SPContext.Current.Web.UICulture.TwoLetterISOLanguageName.ToUpper());
            }
            catch (Exception e)
            {
                Utilities.LogException("Write", e, EventLogEntryType.Warning);
            }

            bool viewAllItemsInEveryLanguages = (bool)HttpContext.Current.Cache["AlphamosaikItemFiltering"] == false;

            if (HttpContext.Current.Request.UrlReferrer != null)
                if (HttpContext.Current.Request.UrlReferrer.ToString().Contains("SPS_Trans_Code_Pers=Unfiltering"))
                {
                    // View All Items In Every Language
                    viewAllItemsInEveryLanguages = true;
                }

            ConvertAsciiCode(html);

            bool editMode = IsEditPageMode() || html.IndexOf("action=\"EditForm.aspx") != -1 ||
                            html.IndexOf("action=\"ViewEdit.aspx") != -1;

            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
            {
                if (languageItem.LanguageDestination != languageDestination)
                {
                    // Tag for master page
                    RemoveTranslatedTr(html, "SPS_STATIC_LNG_MASTERPAGE_" + languageItem.LanguageDestination);

                    if (!viewAllItemsInEveryLanguages && !editMode)
                    {
                        RemoveTranslatedTr(html, "SPS_LNG_" + languageItem.LanguageDestination);
                        RemoveTranslatedDiv(html, "SPS_STATIC_LNG_" + languageItem.LanguageDestination);
                        RemoveTranslatedTr(html, "SPS_STATIC_LNG_" + languageItem.LanguageDestination);
                    }
                }
                else
                {
                    string sharePointItemLanguageToRemove = "<option value=\"SPS_LNG_" + languageItem.LanguageDestination +
                                                            "\">SPS_LNG_" + languageItem.LanguageDestination + "</option>";
                    html.Replace(sharePointItemLanguageToRemove, string.Empty);
                }
            }

            RemoveSpsTagsBeforeAddingMenus(html, viewAllItemsInEveryLanguages, editMode);
            RemoveSpsWebPartAfterAddingMenus(html);
            RemoveSpsTagsAfterAddingMenus(html, false, false, null);

            if (languageSource != languageDestination)
            {
                //// Translate the Html Tag
                if (!IsEditPageMode())
                {
                    TranslateFromDictionary(html, languageDestination, languageSource, -1, siteUrl);
                }

                // set language right to left for arabic
                const string DirRtl = "dir=\"rtl\"";
                const string DirLtr = "dir=\"ltr\"";
                if (languageDestination == "AR")
                    html.Replace(DirLtr, DirRtl);
            }

            byte[] data = Encoding.UTF8.GetBytes(html.ToString());

            HttpContext.Current.Response.ClearContent();

            responseStream.Write(data, 0, data.Length);
        }

        public override string GetPageFromCache(string currentHashCode, string requestDigest, string viewState, string eventValidation, string userName, string currentUserId, ArrayList ctxIdList, string includeTimeValueDateTime, ArrayList imnTypeSmtpList,
            ArrayList removedTranslatedDivForCache, string accountName, StringCollection currentHashCodeArrayList, string url, int extractorStatus, string languageSource, string languageDestination, ArrayList removedAlphaNoCacheDivForCache, string qLogEnv)
        {
            string cachedString = HttpContext.Current.Cache[currentHashCode].ToString();
            int cacheBlocksCount = 0;

            int nindexdebut = cachedString.IndexOf("__REQUESTDIGEST");
            int nindexfin = cachedString.IndexOf("/>", nindexdebut + 1);
            string nrequestDigest = string.Empty;

            if ((nindexdebut > 0) && (nindexfin > 0))
                nrequestDigest = cachedString.Substring(nindexdebut, nindexfin - nindexdebut);

            int nindexdebutViewState = cachedString.IndexOf("__VIEWSTATE");
            int nindexfinViewState = cachedString.IndexOf("/>", nindexdebutViewState + 1);
            string nviewState = string.Empty;

            if ((nindexdebutViewState > 0) && (nindexfinViewState > 0))
                nviewState = cachedString.Substring(nindexdebutViewState, nindexfinViewState - nindexdebutViewState);

            int nindexdebutEventValidation = cachedString.IndexOf("__EVENTVALIDATION");
            int nindexfinEventValidation = cachedString.IndexOf("/>", nindexdebutEventValidation + 1);
            string neventValidation = string.Empty;

            if ((nindexdebutEventValidation > 0) && (nindexfinEventValidation > 0))
                neventValidation = cachedString.Substring(nindexdebutEventValidation,
                                                          nindexfinEventValidation - nindexdebutEventValidation);

            if (imnTypeSmtpList.Count > 0)
            {
                int indexdebutImnTypeSmtp = cachedString.IndexOf("####id='imn_0,type=");
                int indexdebutImnTypeSmtp2 = cachedString.IndexOf("####id=\"imn_0,type=");

                if ((indexdebutImnTypeSmtp > 0) || (indexdebutImnTypeSmtp2 > 0))
                {
                    int cmptimn = 0;
                    foreach (string currentId in imnTypeSmtpList)
                    {
                        cachedString = cachedString.Replace("####id='imn_" + cmptimn + ",type=smtp'####",
                                                            "id='imn_" + currentId + ",type=smtp'")
                                                    .Replace("####id=\"imn_" + cmptimn + ",type=smtp'####",
                                                            "id=\"imn{" + currentId + "},type=smtp\"")
                                                    .Replace("####id='imn_" + cmptimn + ",type=sip'####",
                                                            "id='imn_" + currentId + ",type=sip'")
                                                    .Replace("####id=\"imn_" + cmptimn + ",type=sip'####",
                                                            "id=\"imn{" + currentId + "},type=sip\"");
                        cmptimn++;
                    }
                }
            }

            if (!string.IsNullOrEmpty(qLogEnv))
            {
                cachedString = cachedString.Replace("####qLogEnv####", qLogEnv);
            }

            if (!string.IsNullOrEmpty(includeTimeValueDateTime))
            {
                int indexdebutDateTime = cachedString.IndexOf("####IncludeTimeValueDateTime####");

                if (indexdebutDateTime > 0)
                {
                    cachedString = cachedString.Replace("####IncludeTimeValueDateTime####", includeTimeValueDateTime);
                }
            }

            if (!string.IsNullOrEmpty(currentUserId) &&
                cachedString.IndexOf("####SharePoint.OpenDocuments####") > -1)
            {
                cachedString = cachedString.Replace("####SharePoint.OpenDocuments####",
                                                    "'SharePoint.OpenDocuments','','','','" + currentUserId);
            }

            if (!string.IsNullOrEmpty(currentUserId) && cachedString.IndexOf("####userId:####") > -1)
            {
                cachedString = cachedString.Replace("####userId:####", "userId:" + currentUserId);
            }

            int nindexUserName = cachedString.IndexOf("<span>####username####</span>");
            int nindexAccountName = cachedString.IndexOf("?accountname=####accountname####\')');");
            int nindexCurrentUserId = cachedString.IndexOf("####currentuserid####");

            if ((nindexdebut > 0) && (nindexfin > 0))
                cachedString = cachedString.Replace(nrequestDigest, requestDigest);

            if ((nindexdebutViewState > 0) && (nindexfinViewState > 0))
                cachedString = cachedString.Replace(nviewState, viewState);

            if ((nindexdebutEventValidation > 0) && (nindexfinEventValidation > 0))
                cachedString = cachedString.Replace(neventValidation, eventValidation);

            if ((nindexUserName > 0) && (!string.IsNullOrEmpty(userName)))
                cachedString = cachedString.Replace("<span>####username####</span>", "<span>" + userName + "</span>");

            if ((nindexAccountName > 0) && (!string.IsNullOrEmpty(accountName)))
                cachedString = cachedString.Replace("?accountname=####accountname####\')');",
                                                    "?accountname=" + accountName + "\')');");

            if ((nindexCurrentUserId > 0) && (!string.IsNullOrEmpty(currentUserId)))
                cachedString = cachedString.Replace("####currentuserid####", currentUserId);

            int cmpt = 0;
            foreach (string ctxId in ctxIdList)
            {
                cachedString = cachedString.Replace("####ctxId####" + cmpt, ctxId);
                cmpt++;
            }

            int notInCacheCount = 0;
            if (cachedString.IndexOf("####AlphaNoCache") > 0)
            {
                cmpt = 0;
                foreach (string removedCachedString in removedAlphaNoCacheDivForCache)
                {
                    cachedString = cachedString.Replace("####AlphaNoCache" + cmpt + "####", removedCachedString);
                    cmpt++;
                }

                notInCacheCount = cmpt;
            }

            if (cachedString.IndexOf("####AlphaBlockCache") > 0)
            {
                cmpt = 0;
                foreach (string removedCachedString in removedTranslatedDivForCache)
                {
                    // Code pour appeler la traduction du noCache
                    string cacheBlockHashCode = (languageDestination + "-" + removedCachedString).GetHashCode().ToString();
                    string cacheBlockToInsert;

                    if (currentHashCodeArrayList.Contains(cacheBlockHashCode) &&
                        (HttpContext.Current.Cache[cacheBlockHashCode] != null))
                    {
                        cacheBlockToInsert = HttpContext.Current.Cache[cacheBlockHashCode].ToString();
                        cacheBlocksCount++;
                    }
                    else
                    {
                        currentHashCodeArrayList.Add(cacheBlockHashCode);

                        HttpContext.Current.Cache.Insert("SPS_HASHCODES_PAGES", currentHashCodeArrayList);

                        var removedCachedStringSb = new StringBuilder(removedCachedString);

                        if (languageDestination != languageSource)
                        {
                            ConvertAsciiCode(removedCachedStringSb);
                            TranslateFromDictionary(removedCachedStringSb, languageDestination, languageSource, extractorStatus, url);

                            cacheBlockToInsert = removedCachedStringSb.ToString();
                        }
                        else
                        {
                            cacheBlockToInsert = removedCachedString;
                        }

                        HttpContext.Current.Cache.Insert(cacheBlockHashCode, removedCachedStringSb, null,
                                                         Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                                         CacheItemPriority.Low, null);
                    }

                    // fin Code pour appeler la traduction du noCache
                    cachedString = cachedString.Replace("####AlphaBlockCache" + cmpt + "####", cacheBlockToInsert);
                    cmpt++;
                }
            }

            // Affichage d'un message si le cache est utilisé
            int infoTagIndex = cachedString.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (infoTagIndex > -1)
            {
                cachedString = cachedString.Insert(infoTagIndex,
                                                   "\n<!-- <tr><td>" + "Oceanik retrieved Page from second level cache - " +
                                                   (cacheBlocksCount + 1) +
                                                   " cache block(s) : 1 main page cache block + " + cacheBlocksCount +
                                                   " translated cache sub-block(s) + " + notInCacheCount +
                                                   " no-translated sub-block(s) </td></tr> -->\n");
            }

            return cachedString;
        }

        public override void BuildPageForCache(StringBuilder tempResponse, string currentHashCode, string currentUserId, string userName, string includeTimeValueDateTime, string accountName, ArrayList removedTranslatedDivForCache, string language)
        {
            var currentHashCodesPages = new StringCollection();

            if (HttpContext.Current.Cache["SPS_HASHCODES_PAGES"] != null)
                currentHashCodesPages = HttpContext.Current.Cache["SPS_HASHCODES_PAGES"] as StringCollection;

            if (currentHashCodesPages != null)
            {
                currentHashCodesPages.Add(currentHashCode);

                HttpContext.Current.Cache.Insert("SPS_HASHCODES_PAGES", currentHashCodesPages);
            }

            var tempResponseForCache = new StringBuilder(tempResponse.ToString());

            if (!string.IsNullOrEmpty(userName))
            {
                tempResponseForCache.Replace("<span>" + userName + "</span>", "<span>####username####</span>");
            }

            if (!string.IsNullOrEmpty(accountName))
            {
                tempResponseForCache.Replace("?accountname=" + accountName + "\')');",
                                             "?accountname=####accountname####\')');");
            }

            if (tempResponseForCache.IndexOf("ctx.CurrentUserId = ") > -1)
                if (TranslatorRegex.CurrentUserIdRegex.IsMatch(tempResponseForCache.ToString()))
                {
                    tempResponseForCache.Replace(TranslatorRegex.CurrentUserIdRegex.Match(tempResponseForCache.ToString()).Value,
                                                                        TranslatorRegex.CurrentUserIdRegex.Match(tempResponseForCache.ToString()).Value.Replace(currentUserId, "####currentuserid####"));
                }

            if (tempResponseForCache.IndexOf("var _spUserId=") > -1)
                if (TranslatorRegex.UserIdRegex.IsMatch(tempResponseForCache.ToString()))
                {
                    tempResponseForCache.Replace(TranslatorRegex.UserIdRegex.Match(tempResponseForCache.ToString()).Value,
                                                                        TranslatorRegex.UserIdRegex.Match(tempResponseForCache.ToString()).Value.Replace(currentUserId, "####currentuserid####"));
                }

            if (tempResponseForCache.IndexOf("return DispEx(") > -1)
                if (TranslatorRegex.DispExRegex.IsMatch(tempResponseForCache.ToString()))
                {
                    foreach (Match currentMatch in TranslatorRegex.DispExRegex.Matches(tempResponseForCache.ToString()))
                    {
                        tempResponseForCache.Replace(currentMatch.Value,
                                                     currentMatch.Groups["return"].Value +
                                                     currentMatch.Groups["param2_11"].Value
                                                     + currentMatch.Groups["tild"].Value + "####currentuserid####" +
                                                     currentMatch.Groups["tild2"].Value
                                                     + currentMatch.Groups["param13_14"].Value +
                                                     currentMatch.Groups["param15"].Value);
                    }
                }

            // id='imn_473,type=smtp'
            if ((tempResponseForCache.IndexOf(",type=smtp") > 0) || (tempResponseForCache.IndexOf(",type=sip") > 0))
            {
                if (TranslatorRegex.ImnTypeSmtpRegex.IsMatch(tempResponseForCache.ToString()))
                {
                    int cmpt = 0;
                    foreach (
                        Match currentMatch in TranslatorRegex.ImnTypeSmtpRegex.Matches(tempResponseForCache.ToString()))
                    {
                        if (currentMatch.Value.Contains("},type=smtp"))
                        {
                            tempResponseForCache.Replace(currentMatch.Value, "####id=\"imn_" + cmpt + ",type=smtp'####");
                        }
                        else
                        {
                            if (currentMatch.Value.Contains("',type=smtp"))
                            {
                                tempResponseForCache.Replace(currentMatch.Value, "####id='imn_" + cmpt + ",type=smtp'####");
                            }
                            else
                            {
                                if (currentMatch.Value.Contains("},type=sip"))
                                {
                                    tempResponseForCache.Replace(currentMatch.Value, "####id=\"imn_" + cmpt + ",type=sip'####");
                                }
                                else
                                {
                                    tempResponseForCache.Replace(currentMatch.Value, "####id='imn_" + cmpt + ",type=sip'####");
                                }
                            }
                        }

                        cmpt++;
                    }
                }
            }

            int indexdebutDateTime = tempResponseForCache.IndexOf("<Value IncludeTimeValue='TRUE' Type='DateTime'>");

            if (indexdebutDateTime > 0)
            {
                tempResponseForCache.Replace(includeTimeValueDateTime, "####IncludeTimeValueDateTime####");
            }

            if (!string.IsNullOrEmpty(currentUserId) && tempResponseForCache.IndexOf("'SharePoint.OpenDocuments','','','','" + currentUserId) > -1)
            {
                tempResponseForCache.Replace("'SharePoint.OpenDocuments','','','','" + currentUserId, "####SharePoint.OpenDocuments####");
            }

            if (!string.IsNullOrEmpty(currentUserId) && tempResponseForCache.IndexOf("userId:" + currentUserId) > -1)
            {
                tempResponseForCache.Replace("userId:" + currentUserId, "####userId:####");
            }

            // Début adaptation cache 2010
            var ctxIdList = new ArrayList();
            if (tempResponseForCache.IndexOf("      ctx.ctxId = ") > -1)
            {
                int cmpt = 0;
                foreach (Match currentMatch in TranslatorRegex.CtxIdRegex.Matches(tempResponseForCache.ToString()))
                {
                    if (!ctxIdList.Contains(currentMatch.Groups["ctxId"].Value))
                    {
                        tempResponseForCache.Replace(
                            currentMatch.Value, currentMatch.Groups["constant"].Value + "####ctxId####" + cmpt + ";");
                        ctxIdList.Add(currentMatch.Groups["ctxId"].Value);
                        cmpt++;
                    }
                }
            }

            if (ctxIdList.Count > 0)
            {
                int cmpt = 0;
                foreach (string ctxId in ctxIdList)
                {
                    if (tempResponseForCache.IndexOf("g_ViewIdToViewCounterMap") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(),
                                              "(?<constant>(g_ViewIdToViewCounterMap\\[\"{[^}]+}\"\\]= ))" + ctxId + ";",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, currentMatch.Groups["constant"].Value + "####ctxId####" + cmpt + ";");
                        }

                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(),
                                              "g_ctxDict\\['ctx" + ctxId + "'\\] = ctx;", RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, "g_ctxDict['ctx" + "####ctxId####" + cmpt + "'] = ctx;");
                        }
                    }

                    if (tempResponseForCache.IndexOf("ctx" + ctxId + " = ctx;") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(), "ctx" + ctxId + " = ctx;",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, "ctx" + "####ctxId####" + cmpt + " = ctx;");
                        }
                    }

                    if (tempResponseForCache.IndexOf("id=\"FilterIframe" + ctxId + "\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(), "id=\"FilterIframe" + ctxId + "\"",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, "id=\"FilterIframe" + "####ctxId####" + cmpt + "\"");
                        }
                    }

                    if (tempResponseForCache.IndexOf(" name=\"FilterIframe" + ctxId + "\" ") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(), " name=\"FilterIframe" + ctxId + "\" ",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, " name=\"FilterIframe" + "####ctxId####" + cmpt + "\" ");
                        }
                    }

                    if (tempResponseForCache.IndexOf("\"EnsureSelectionHandler(event,this," + ctxId + ")\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(),
                                              "\"EnsureSelectionHandler\\(event,this," + ctxId + "\\)\"",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value,
                                "\"EnsureSelectionHandler(event,this," + "####ctxId####" + cmpt + ")\"");
                        }
                    }

                    if (tempResponseForCache.IndexOf("\"ToggleAllItems(event,this," + ctxId + ")\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(),
                                              "\"ToggleAllItems\\(event,this," + ctxId + "\\)\"",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, "\"ToggleAllItems(event,this," + "####ctxId####" + cmpt + ")\"");
                        }
                    }

                    if (tempResponseForCache.IndexOf("\"EnsureSelectionHandlerOnFocus(event,this," + ctxId + ")\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(),
                                              "\"EnsureSelectionHandlerOnFocus\\(event,this," + ctxId + "\\)\"",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value,
                                "\"EnsureSelectionHandlerOnFocus(event,this," + "####ctxId####" + cmpt + ")\"");
                        }
                    }

                    if (tempResponseForCache.IndexOf(" CTXNum=\"" + ctxId + "\" ", StringComparison.OrdinalIgnoreCase) >
                        -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(), " CTXNum=\"" + ctxId + "\" ",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, " CTXNum=\"" + "####ctxId####" + cmpt + "\" ");
                        }
                    }

                    if (tempResponseForCache.IndexOf(" iid=\"" + ctxId + ",") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(), " iid=\"" + ctxId + ",", RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value,
                                " iid=\"" + "####ctxId####" + cmpt + ",");
                        }
                    }

                    if (tempResponseForCache.IndexOf(" CTXName=\"ctx" + ctxId + "\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(), " CTXName=\"ctx" + ctxId + "\"",
                                              RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, " CTXName=\"ctx" + "####ctxId####" + cmpt + "\"");
                        }
                    }

                    if (tempResponseForCache.IndexOf(" onclick=\"EditLink2(this," + ctxId + ");") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(tempResponseForCache.ToString(),
                                              " onclick=\"EditLink2\\(this," + ctxId + "\\);", RegexOptions.IgnoreCase))
                        {
                            tempResponseForCache.Replace(
                                currentMatch.Value, " onclick=\"EditLink2(this," + "####ctxId####" + cmpt + ");");
                        }
                    }

                    cmpt++;
                }
            }

            if (tempResponseForCache.IndexOf("<div class=\"AlphaNoCache", StringComparison.OrdinalIgnoreCase) > -1)
            {
                RemoveTranslatedDivForCache(tempResponseForCache, "AlphaNoCache");
            }

            if (tempResponseForCache.IndexOf("<div class=\"AlphaBlockCache", StringComparison.OrdinalIgnoreCase) > -1)
            {
                var cacheBlocksArrayList = RemoveTranslatedDivForCache(tempResponseForCache, "AlphaBlockCache");
                object[] cacheBlocksArray = cacheBlocksArrayList.ToArray();
                int index = 0;

                if (cacheBlocksArrayList.Count == removedTranslatedDivForCache.Count)
                {
                    foreach (string removedCachedStringBefore in removedTranslatedDivForCache)
                    {
                        // Code pour appeler la traduction du noCache
                        string notInCacheHashCode =
                            (language + "-" + removedCachedStringBefore).GetHashCode().ToString();

                        if (currentHashCodesPages != null)
                            if (!currentHashCodesPages.Contains(notInCacheHashCode))
                            {
                                currentHashCodesPages.Add(notInCacheHashCode);
                                HttpContext.Current.Cache.Insert("SPS_HASHCODES_PAGES", currentHashCodesPages);
                            }

                        if (HttpContext.Current.Cache[notInCacheHashCode] == null)
                        {
                            var translatedCacheBlock = (string)cacheBlocksArray.GetValue(index);
                            var translatedCacheBlockSb = new StringBuilder(translatedCacheBlock);
                            HttpContext.Current.Cache.Insert(notInCacheHashCode, translatedCacheBlockSb, null,
                                                             Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                                             CacheItemPriority.Low, null);
                        }

                        index++;
                    }
                }
            }

            /*SPSecurity.RunWithElevatedPrivileges(delegate
                                                             {
                                                                string time = String.Format("{0:yyyy-MM-dd-hh-mm-fffffff}", DateTime.Now);
                                                                var f7 = new FileInfo(@"C:\Program Files\Alphamosaik\SharepointTranslator2010\logs\StoredCachedString_" + time + ".txt");

                                                                if (!f7.Exists)
                                                                {
                                                                    // Create a file to write to.
                                                                    using (f7.CreateText())
                                                                    {
                                                                    }
                                                                }

                                                                using (StreamWriter swriterAppend = f7.AppendText())
                                                                {
                                                                    swriterAppend.WriteLine(tempResponseForCache);
                                                                }
                                                             });*/

            HttpContext.Current.Cache.Insert(currentHashCode, tempResponseForCache, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Low, null);
        }

        /// <summary>
        /// Hotfix for ASCII Code
        /// </summary>
        /// <param name="tempResponse">The temp Response.</param>
        public override void ConvertAsciiCode(StringBuilder tempResponse)
        {
            // HotFix for ASCII Code
            tempResponse.SetNewString(TranslatorRegex.AsciiCodeRegex.Replace(tempResponse.ToString(),
                                                                             ConvertAsciiCodeEvaluator.MatchEvaluatorTag));
        }

        public override void ReplaceLinkedPagesUrl(StringBuilder tempResponse, string languageDestination)
        {
            if (HttpContext.Current == null)
                return;

            try
            {
                var replaceLinkedPagesUrlIsActivated =
                    (bool)HttpContext.Current.Cache["AlphamosaikReplaceLinkedPagesUrl"];

                if (replaceLinkedPagesUrlIsActivated && !IsEditPageMode())
                {
                    using (var siteCollection = new SPSite(SPContext.Current.Site.Url))
                    {
                        SPList currentList = null;
                        MatchCollection result = Regex.Matches(tempResponse.ToString(),
                                                               "HREF=\"(" + siteCollection.Url +
                                                               ")?(/[^/<>\"';,:{}]+)*/pages/[^\\./]+\\.aspx",
                                                               RegexOptions.IgnoreCase);
                        foreach (Match currentMatch in result)
                        {
                            try
                            {
                                bool listExist = true;
                                string listUrl =
                                    currentMatch.Value.ToLower().Substring(
                                        currentMatch.Value.ToLower().LastIndexOf("href=\"") + 6,
                                        currentMatch.Value.ToLower().LastIndexOf("/pages/"));
                                if (!listUrl.ToLower().Contains(siteCollection.Url.ToLower()))
                                    listUrl = siteCollection.Url + listUrl;

                                try
                                {
                                    string siteUrl = listUrl.Remove(listUrl.ToLower().LastIndexOf("/pages"));
                                    using (var newSiteCollection = new SPSite(siteUrl))
                                    {
                                        using (var newWeb = newSiteCollection.OpenWeb())
                                        {
                                            currentList = newWeb.GetList(listUrl);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utilities.LogException("ReplaceLinkedpagesUrl", e, EventLogEntryType.Warning);
                                    listExist = false;
                                }

                                if (listExist)
                                {
                                    string relativeUrlItem =
                                        currentMatch.Value.Substring(
                                            currentMatch.Value.ToLower().LastIndexOf("/pages/") + 7);
                                    SPListItem currentItem = null;
                                    string newPage = string.Empty;

                                    var query = new SPQuery
                                    {
                                        Query = "<Where><Eq><FieldRef Name='FileLeafRef'/>" +
                                                "<Value Type='File'>" + relativeUrlItem +
                                                "</Value></Eq></Where>",
                                        QueryThrottleMode = SPQueryThrottleOption.Override
                                    };

                                    SPListItemCollection collListItems = currentList.GetItems(query);

                                    foreach (SPListItem tmpListItem in collListItems)
                                    {
                                        if (tmpListItem.Name.ToLower() == relativeUrlItem.ToLower())
                                        {
                                            currentItem = tmpListItem;
                                            break;
                                        }
                                    }

                                    if (currentItem != null)
                                    {
                                        if (currentList.Fields.ContainsField(languageDestination + " version") &&
                                            (!string.IsNullOrEmpty(
                                                currentItem.GetFormattedValue(languageDestination + " version"))))
                                        {
                                            newPage =
                                                currentList.GetItemById(
                                                    Convert.ToInt32(
                                                        currentItem.GetFormattedValue(languageDestination + " version"))).Name;
                                        }

                                        if (!string.IsNullOrEmpty(newPage))
                                        {
                                            if (
                                                !HttpContext.Current.Request.Url.ToString().ToLower().Contains(
                                                    listUrl.ToLower() + "/" + relativeUrlItem.ToLower()) &&
                                                !HttpContext.Current.Request.Url.ToString().ToLower().Contains(
                                                    listUrl.ToLower() + "/" + newPage.ToLower()))
                                            {
                                                tempResponse.Replace(currentMatch.Value,
                                                                     currentMatch.Value.ToLower().Replace(
                                                                         "/pages/" + relativeUrlItem.ToLower(),
                                                                         "/pages/" + newPage));
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Utilities.LogException("ReplaceLinkedPagesUrl", e, EventLogEntryType.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("ReplaceLinkedPagesUrl", e, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Translate values from the dictionary content
        /// </summary>
        /// <param name="tempResponse">The temp Response.</param>
        /// <param name="languageDestination">The language Code.</param>
        /// <param name="languageSource">The current Lc Code.</param>
        /// <param name="extractorStatus">The extractor Status.</param>
        /// <param name="url">The current url.</param>
        public override void TranslateFromDictionary(StringBuilder tempResponse, string languageDestination, string languageSource, int extractorStatus, string url)
        {
            if (HttpContext.Current == null)
                return;

            TranslateTagMain(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagText(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagDescription(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagTitle(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagAlt(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagButtons(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagInnerHtml(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagLayoutInfo(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagNavigationNode(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateTagMsPickerFooter(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateDateInSharePointList(tempResponse, languageSource, extractorStatus, url, languageDestination);

            if (HttpContext.Current.Request.Path.ToLower().EndsWith("/create.aspx") ||
                HttpContext.Current.Request.Path.ToLower().EndsWith("/newsbweb.aspx"))
                TranslateTagRollover(tempResponse, languageSource, extractorStatus, url, languageDestination);

            if (HttpContext.Current.Request.Path.ToLower().EndsWith("/iframe.aspx"))
                TranslateDatePicker(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateCalendarView(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateSearchBox(tempResponse, languageSource, extractorStatus, url, languageDestination);

            TranslateMultilookupPicker(tempResponse, languageSource, extractorStatus, url, languageDestination);
        }

        public override string GetPageHashCode(string languageDestination, StringBuilder tempResponse, out string requestDigest, out string viewState, out string eventValidation, out string currentUserId, out string userName,
            out ArrayList ctxIdList, out string includeTimeValueDateTime, out ArrayList imnTypeSmtpList, out ArrayList removedTranslatedDivForCache, out string accountName, out string qLogEnv)
        {
            string currentHashCode = languageDestination + "-" + tempResponse;

            removedTranslatedDivForCache = new ArrayList();

            // Remove request digest and view state
            int indexdebut = tempResponse.IndexOf("__REQUESTDIGEST");
            requestDigest = string.Empty;
            if (indexdebut > 0)
            {
                int indexfin = tempResponse.IndexOf("/>", indexdebut + 1);
                requestDigest = tempResponse.Substring(indexdebut, indexfin - indexdebut);
                currentHashCode = currentHashCode.Replace(requestDigest, string.Empty);
            }

            int indexdebutViewState = tempResponse.IndexOf("__VIEWSTATE");
            int indexfinViewState = tempResponse.IndexOf("/>", indexdebutViewState + 1);
            viewState = string.Empty;
            if ((indexdebutViewState > 0) && (indexfinViewState > 0))
            {
                viewState = tempResponse.Substring(indexdebutViewState, indexfinViewState - indexdebutViewState);
                currentHashCode = currentHashCode.Replace(viewState, string.Empty);
            }

            int indexdebutQLogEnv = tempResponse.IndexOf("QLogEnv_");
            
            qLogEnv = string.Empty;
            if (indexdebutQLogEnv > 0)
            {
                int indexfinQLogEnv = tempResponse.IndexOf("/>", indexdebutQLogEnv + 1);

                if (indexfinQLogEnv > 0)
                {
                    qLogEnv = tempResponse.Substring(indexdebutQLogEnv, indexfinQLogEnv - indexdebutQLogEnv);
                    currentHashCode = currentHashCode.Replace(qLogEnv, "####qLogEnv####");
                }
            }

            int indexdebutEventValidation = tempResponse.IndexOf("__EVENTVALIDATION");
            int indexfinEventValidation = tempResponse.IndexOf("/>", indexdebutEventValidation + 1);
            eventValidation = string.Empty;
            if ((indexdebutEventValidation > 0) && (indexfinEventValidation > 0))
            {
                eventValidation = tempResponse.Substring(indexdebutEventValidation,
                                                         indexfinEventValidation - indexdebutEventValidation);
                currentHashCode = currentHashCode.Replace(eventValidation, string.Empty);
            }

            userName = string.Empty;

            // PersonalActionMenu" serverclientid="zz17_Menu"><span>Sylvain-PC\sylvain</span>
            int indexPersonalMenu = currentHashCode.IndexOf("PersonalActionMenu\" serverclientid=\"zz",
                                                            StringComparison.OrdinalIgnoreCase);

            if (indexPersonalMenu > -1)
            {
                const string TextToSearch = "Menu\"><span>";

                int startSpan = currentHashCode.IndexOf(TextToSearch, indexPersonalMenu);

                if (startSpan != -1)
                {
                    startSpan += TextToSearch.Length;

                    int endSpan = currentHashCode.IndexOf("</span>", startSpan);

                    if (endSpan > startSpan)
                    {
                        userName = currentHashCode.Substring(startSpan, endSpan - startSpan);
                        currentHashCode = currentHashCode.Replace("<span>" + userName + "</span>",
                                                                  "<span>####username####</span>");
                    }
                }
            }

            // ?accountname=ALPHA\u00255Cco079\')');
            accountName = string.Empty;

            int startAccountName = currentHashCode.IndexOf("?accountname=");

            if (startAccountName > -1)
            {
                startAccountName += 13;

                int endAccountName = currentHashCode.IndexOf("\')');", startAccountName);
                int endBadAccountName = currentHashCode.IndexOf("\"", startAccountName);

                while ((startAccountName > -1) && (endBadAccountName > -1) && (endAccountName > endBadAccountName))
                {
                    startAccountName = currentHashCode.IndexOf("?accountname=", endBadAccountName);
                    if (startAccountName > -1)
                    {
                        startAccountName += 13;
                        endAccountName = currentHashCode.IndexOf("\')');", startAccountName);
                        endBadAccountName = currentHashCode.IndexOf("\"", startAccountName);
                    }
                }

                if ((startAccountName > -1) && (endAccountName > startAccountName))
                {
                    accountName = currentHashCode.Substring(startAccountName, endAccountName - startAccountName);
                    currentHashCode = currentHashCode.Replace("?accountname=" + accountName + "\')');",
                                                              "?accountname=####accountname####\')');");
                }
            }

            currentUserId = string.Empty;
            if (currentHashCode.IndexOf("ctx.CurrentUserId = ") > -1)
            {
                if (TranslatorRegex.CurrentUserIdRegex.IsMatch(currentHashCode))
                {
                    currentUserId =
                        TranslatorRegex.CurrentUserIdRegex.Match(currentHashCode).Value.Substring(
                            TranslatorRegex.CurrentUserIdRegex.Match(currentHashCode).Value.LastIndexOf(" ") + 1).Replace(";", string.Empty);
                    currentHashCode =
                        currentHashCode.Replace(TranslatorRegex.CurrentUserIdRegex.Match(currentHashCode).Value,
                                                TranslatorRegex.CurrentUserIdRegex.Match(currentHashCode).Value.Replace(
                                                    currentUserId, "####currentuserid####"));
                }
            }

            int startSpUserId = currentHashCode.IndexOf("var _spUserId=");

            if (startSpUserId > -1)
            {
                startSpUserId += 14;

                int endSpUserId = currentHashCode.IndexOf(";", startSpUserId);

                if (endSpUserId > startSpUserId)
                {
                    if (string.IsNullOrEmpty(currentUserId))
                        currentUserId = currentHashCode.Substring(startSpUserId, endSpUserId - startSpUserId);

                    currentHashCode = currentHashCode.Replace("var _spUserId=" + currentUserId + ";",
                                                              "var _spUserId=####currentuserid####;");
                }
            }

            if (currentHashCode.IndexOf("return DispEx(") > -1)
            {
                if (TranslatorRegex.DispExRegex.IsMatch(currentHashCode))
                {
                    if (string.IsNullOrEmpty(currentUserId))
                        currentUserId = TranslatorRegex.DispExRegex.Match(currentHashCode).Groups["userid"].Value;

                    foreach (Match currentMatch in TranslatorRegex.DispExRegex.Matches(currentHashCode))
                    {
                        currentHashCode = currentHashCode.Replace(currentMatch.Value,
                                                                  currentMatch.Groups["return"].Value
                                                                  + currentMatch.Groups["param2_11"].Value +
                                                                  currentMatch.Groups["tild"].Value
                                                                  + "####currentuserid####" +
                                                                  currentMatch.Groups["tild2"].Value
                                                                  + currentMatch.Groups["param13_14"].Value +
                                                                  currentMatch.Groups["param15"].Value);
                    }
                }
            }

            // 'SharePoint.OpenDocuments','','','','1073741823',
            if (!string.IsNullOrEmpty(currentUserId) &&
                currentHashCode.IndexOf("'SharePoint.OpenDocuments','','','','" + currentUserId) > -1)
            {
                currentHashCode = currentHashCode.Replace("'SharePoint.OpenDocuments','','','','" + currentUserId,
                                                          "####SharePoint.OpenDocuments####");
            }

            int indexUserId = currentHashCode.IndexOf("userId:" + currentUserId);

            // userId:1073741823,
            if (indexUserId > -1)
            {
                if (string.IsNullOrEmpty(currentUserId))
                {
                    int indexCurrentUserId = currentHashCode.IndexOf(',', indexUserId);

                    if (indexCurrentUserId > -1)
                    {
                        indexUserId += 7; // skip userId:
                        currentUserId = currentHashCode.Substring(indexUserId, indexCurrentUserId - indexUserId);
                    }
                }

                currentHashCode = currentHashCode.Replace("userId:" + currentUserId, "####userId:####");
            }

            // Ignore <Value IncludeTimeValue='TRUE' Type='DateTime'>2010-11-12T15:54:35Z</Value>
            int indexdebutDateTime = tempResponse.IndexOf("<Value IncludeTimeValue='TRUE' Type='DateTime'>");
            includeTimeValueDateTime = string.Empty;

            if (indexdebutDateTime > 0)
            {
                int indexfinDateTime = tempResponse.IndexOf("</Value>", indexdebutDateTime + 1);
                includeTimeValueDateTime = tempResponse.Substring(indexdebutDateTime,
                                                                  indexfinDateTime - indexdebutDateTime);
                currentHashCode = currentHashCode.Replace(includeTimeValueDateTime, "####IncludeTimeValueDateTime####");
            }

            // id='imn_473,type=smtp'
            imnTypeSmtpList = new ArrayList();

            if (currentHashCode.IndexOf(",type=") > 0)
            {
                if (TranslatorRegex.ImnTypeSmtpRegex.IsMatch(currentHashCode))
                {
                    int cmpt = 0;
                    foreach (Match currentMatch in TranslatorRegex.ImnTypeSmtpRegex.Matches(currentHashCode))
                    {
                        imnTypeSmtpList.Add(currentMatch.Groups["id"].Value);
                        if (currentMatch.Value.Contains("},type=smtp"))
                        {
                            currentHashCode = currentHashCode.Replace(currentMatch.Value,
                                                                      "####id=\"imn_" + cmpt + ",type=smtp'####");
                        }
                        else
                        {
                            if (currentMatch.Value.Contains("',type=smtp"))
                            {
                                currentHashCode = currentHashCode.Replace(currentMatch.Value, "####id='imn_" + cmpt + ",type=smtp'####");
                            }
                            else
                            {
                                if (currentMatch.Value.Contains("},type=sip"))
                                {
                                    currentHashCode = currentHashCode.Replace(currentMatch.Value, "####id=\"imn_" + cmpt + ",type=sip'####");
                                }
                                else
                                {
                                    currentHashCode = currentHashCode.Replace(currentMatch.Value, "####id='imn_" + cmpt + ",type=sip'####");
                                }
                            }
                        }

                        cmpt++;
                    }
                }
            }

            // Début des adaptations pour SP 2010
            ctxIdList = new ArrayList();

            if (currentHashCode.IndexOf("      ctx.ctxId = ") > -1)
            {
                int cmpt = 0;
                foreach (Match currentMatch in TranslatorRegex.CtxIdRegex.Matches(currentHashCode))
                {
                    if (!ctxIdList.Contains(currentMatch.Groups["ctxId"].Value))
                    {
                        currentHashCode = currentHashCode.Replace(
                            currentMatch.Value,
                            currentMatch.Groups["constant"].Value + "####ctxId####" + cmpt +
                            ";");
                        ctxIdList.Add(currentMatch.Groups["ctxId"].Value);
                        cmpt++;
                    }
                }
            }

            if (ctxIdList.Count > 0)
            {
                int cmpt = 0;
                foreach (string ctxId in ctxIdList)
                {
                    if (currentHashCode.IndexOf("g_ViewIdToViewCounterMap") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode,
                                              "(?<constant>(g_ViewIdToViewCounterMap\\[\"{[^}]+}\"\\]= ))" + ctxId + ";",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, currentMatch.Groups["constant"].Value + "####ctxId####" + cmpt + ";");
                        }

                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, "g_ctxDict\\['ctx" + ctxId + "'\\] = ctx;",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, "g_ctxDict['ctx" + "####ctxId####" + cmpt + "'] = ctx;");
                        }
                    }

                    if (currentHashCode.IndexOf("ctx" + ctxId + " = ctx;") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, "ctx" + ctxId + " = ctx;", RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, "ctx" + "####ctxId####" + cmpt + " = ctx;");
                        }
                    }

                    if (currentHashCode.IndexOf("id=\"FilterIframe" + ctxId + "\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, "id=\"FilterIframe" + ctxId + "\"",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, "id=\"FilterIframe" + "####ctxId####" + cmpt + "\"");
                        }
                    }

                    if (currentHashCode.IndexOf(" name=\"FilterIframe" + ctxId + "\" ") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, " name=\"FilterIframe" + ctxId + "\" ",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, " name=\"FilterIframe" + "####ctxId####" + cmpt + "\" ");
                        }
                    }

                    if (currentHashCode.IndexOf("\"EnsureSelectionHandler(event,this," + ctxId + ")\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode,
                                              "\"EnsureSelectionHandler\\(event,this," + ctxId + "\\)\"",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value,
                                "\"EnsureSelectionHandler(event,this," + "####ctxId####" + cmpt + ")\"");
                        }
                    }

                    if (currentHashCode.IndexOf("\"ToggleAllItems(event,this," + ctxId + ")\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, "\"ToggleAllItems\\(event,this," + ctxId + "\\)\"",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, "\"ToggleAllItems(event,this," + "####ctxId####" + cmpt + ")\"");
                        }
                    }

                    if (currentHashCode.IndexOf("\"EnsureSelectionHandlerOnFocus(event,this," + ctxId + ")\"") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode,
                                              "\"EnsureSelectionHandlerOnFocus\\(event,this," + ctxId + "\\)\"",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value,
                                "\"EnsureSelectionHandlerOnFocus(event,this," + "####ctxId####" + cmpt + ")\"");
                        }
                    }

                    if (currentHashCode.IndexOf(" CTXNum=\"" + ctxId + "\" ", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, " CTXNum=\"" + ctxId + "\" ", RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, " CTXNum=\"" + "####ctxId####" + cmpt + "\" ");
                        }
                    }

                    if (currentHashCode.IndexOf(" iid=\"" + ctxId + ",") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, " iid=\"" + ctxId + ",", RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(currentMatch.Value, " iid=\"" + "####ctxId####" + cmpt + ",");
                        }
                    }

                    if (currentHashCode.IndexOf(" CTXName=\"ctx" + ctxId + "\"") > -1)
                    {
                        foreach (Match currentMatch in Regex.Matches(currentHashCode, " CTXName=\"ctx" + ctxId + "\"", RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, " CTXName=\"ctx" + "####ctxId####" + cmpt + "\"");
                        }
                    }

                    if (currentHashCode.IndexOf(" onclick=\"EditLink2(this," + ctxId + ");") > -1)
                    {
                        foreach (
                            Match currentMatch in
                                Regex.Matches(currentHashCode, " onclick=\"EditLink2\\(this," + ctxId + "\\);",
                                              RegexOptions.IgnoreCase))
                        {
                            currentHashCode = currentHashCode.Replace(
                                currentMatch.Value, " onclick=\"EditLink2(this," + "####ctxId####" + cmpt + ");");
                        }
                    }

                    cmpt++;
                }
            }

            if (currentHashCode.IndexOf("<div class=\"AlphaBlockCache", StringComparison.OrdinalIgnoreCase) > -1)
            {
                var currentHashCodeSb = new StringBuilder(currentHashCode);
                removedTranslatedDivForCache = RemoveTranslatedDivForCache(currentHashCodeSb, "AlphaBlockCache");
                currentHashCode = currentHashCodeSb.ToString();
            }

            /*SPSecurity.RunWithElevatedPrivileges(delegate
                                                             {
                                                                string time = String.Format("{0:yyyy-MM-dd-hh-mm-fffffff}", DateTime.Now);
                                                                var f7 = new FileInfo(@"C:\Program Files\Alphamosaik\Oceanik\logs\CalculatedCachedString_" + time + ".txt");

                                                                if (!f7.Exists)
                                                                {
                                                                    // Create a file to write to.
                                                                    using (f7.CreateText())
                                                                    {
                                                                    }
                                                                }

                                                                using (StreamWriter swriterAppend = f7.AppendText())
                                                                {
                                                                    swriterAppend.WriteLine(currentHashCode);
                                                                }
                                                             });*/

            return currentHashCode.GetHashCode().ToString();
        }

        public override void AddMenuItem(StringBuilder tempResponse, string lang, string languageSource, bool completingDictionaryMode)
        {
            if (HttpContext.Current == null)
                return;

            bool userHaveEditPermission = false;
            bool filteringButton = false;
            if ((bool)HttpContext.Current.Cache["AlphamosaikFilteringButton"])
                filteringButton = true;

            if (SPContext.Current != null)
                userHaveEditPermission = SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.EditListItems);

            //// Create the items menu

            if (((HttpContext.Current.Request.Url.ToString().IndexOf("/Lists/") > -1) || (HttpContext.Current.Request.Url.ToString().IndexOf("/Forms/") > -1)) && ((tempResponse.IndexOf(" src=\"/_layouts/images/edititem.gif\"") > -1)
                                                                                                                                                                   || (tempResponse.IndexOf(" iconsrc=\"/_layouts/images/EditItem.gif\"") > -1) || (tempResponse.IndexOf("_layouts/listedit.aspx") > -1) || (tempResponse.IndexOf("\\u002fEditForm.aspx\"") > -1)))
            {
                string menuLinkWith = string.Empty;

                HttpContext context = HttpContext.Current;
                SPWeb objSpWeb = Microsoft.SharePoint.WebControls.SPControl.GetContextWeb(context);
                string listIdForEnableTradFromList = string.Empty;

                string language;
                string listId = string.Empty;
                bool mustUpgrade = false;
                string unfilteringQueryString = string.Empty;
                bool announcementList = false;

                if ((HttpContext.Current.Request.QueryString["SPS_Trans_Code_Pers"] != null) &&
                    (HttpContext.Current.Request.QueryString["SPS_Trans_Code_Pers"] == "Unfiltering"))
                    unfilteringQueryString = "&SPS_Trans_Code_Pers=Unfiltering";

                const string TextLinkWith = "Link this item with...";
                const string TextDefineLanguage = "Define item language...";
                const string TextDeleteLinks = "Delete all links for this item";
                const string TextHideFiels = "Hide the \\\"Version\\\" fields";
                const string TextShowFields = "Display the \\\"Version\\\" fields";
                const string TextLanguageNotDef = "Language not defined";
                const string TextShowLinkedItemsDashBoard = "Open linked items dashboard";
                const string TextSetLinkable = "Set this item as linkable";
                const string TextRemovetLinkable = "Set this item as no linkable";

                var groupLanguageListArrayList = new ArrayList();
                var itemLanguageListHashTable = new Hashtable { { "(SPS_LNG_ALL)", new ArrayList() } };

                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                {
                    itemLanguageListHashTable.Add("SPS_LNG_" + languageItem.LanguageDestination, new ArrayList());
                }

                bool itemMenuPerformanceMode = false;
                bool addedMenuItem = false;

                SPListItemCollection currentItemsCollection = null;

                foreach (SPList current in objSpWeb.Lists)
                {
                    string formatedListName = "       ";

                    if (current.DefaultViewUrl.LastIndexOf("/Forms/") > -1)
                    {
                        formatedListName = current.DefaultViewUrl.Remove(current.DefaultViewUrl.LastIndexOf("/Forms/"));
                        formatedListName = formatedListName.Substring(formatedListName.LastIndexOf("/")).Replace("/", string.Empty);
                    }

                    if (current.DefaultViewUrl.LastIndexOf("/Lists/") > -1)
                    {
                        formatedListName =
                            current.DefaultViewUrl.Substring(current.DefaultViewUrl.LastIndexOf("/Lists/") +
                                                             "/Lists/".Length);
                        if (formatedListName.IndexOf("/") > -1)
                            formatedListName = formatedListName.Remove(formatedListName.IndexOf("/"));
                    }

                    if ((context.Request.Url.ToString().Contains("/Lists/" + formatedListName + "/")
                         || context.Request.Url.ToString().Contains("/" + formatedListName + "/Forms/"))
                        && current.DoesUserHavePermissions(SPBasePermissions.EditListItems))
                    {
                        listIdForEnableTradFromList = current.ID.ToString();
                        if (current.Fields.ContainsField("SharePoint_Item_Language"))
                        {
                            itemMenuPerformanceMode = current.Fields.ContainsField("Linkable");
                            SPFolder currentListFolder = Alphamosaik.Common.SharePoint.Library.Utilities.GetCurrentFolderInList(current);

                            SPQuery query;

                            if (!itemMenuPerformanceMode)
                            {
                                query = new SPQuery
                                {
                                    Query = "<Query></Query>",
                                    ViewAttributes = "Scope=\"FilesOnly\"",
                                    QueryThrottleMode = SPQueryThrottleOption.Override,
                                };
                            }
                            else
                            {
                                query = new SPQuery
                                {
                                    Query = "<Where><Eq><FieldRef Name='Linkable'/>" +
                                            "<Value Type='Boolean'>" + "1" + "</Value></Eq></Where>",
                                    ViewAttributes = "Scope=\"FilesOnly\"",
                                    QueryThrottleMode = SPQueryThrottleOption.Override,
                                };
                            }

                            if (currentListFolder != null)
                            {
                                query.Folder = currentListFolder;
                            }

                            currentItemsCollection = current.GetItems(query);

                            const int Limit = 20;
                            int subMenuNb = currentItemsCollection.Count / Limit;

                            if (current.BaseTemplate == SPListTemplateType.DiscussionBoard)
                                subMenuNb = current.ItemCount / Limit;

                            listId = current.ID.ToString();
                            announcementList = current.BaseTemplate == SPListTemplateType.Announcements;

                            if (!current.Fields.ContainsField("SharePoint_Group_Language"))
                                mustUpgrade = true;
                            else if (current.Fields["SharePoint_Group_Language"].Required)
                                mustUpgrade = true;

                            if (announcementList &&
                                (!current.Fields.ContainsField("ItemsAutoCreation") ||
                                 !current.Fields.ContainsField("MetadataToDuplicate") ||
                                 !current.Fields.ContainsField("AutoTranslation")))
                            {
                                mustUpgrade = true;
                            }

                            menuLinkWith = context.Request.Url.ToString().Contains("/Lists/" + formatedListName)
                                               ? "<script language=\"JavaScript\"> \n function Custom_AddListMenuItems(m, ctx) { \n "
                                               : "<script language=\"JavaScript\"> \n function Custom_AddDocLibMenuItems(m, ctx) { \n ";

                            menuLinkWith += "var LinkMenu = CASubM(m, \"" + TextLinkWith + "\", \"\"); \n ";

                            if (!itemMenuPerformanceMode)
                                menuLinkWith +=
                                    "LinkMenu.setAttribute(\"enabled\", GetLanguage(currentItemID,\"ALL\",\"false\",\"true\")); \n ";

                            while (subMenuNb >= 0)
                            {
                                int cmpt = (currentItemsCollection.Count / Limit) - subMenuNb;
                                int cmpt2 = 0;

                                if (subMenuNb > 0)
                                {
                                    menuLinkWith += "var PagedMenu1 = CASubM(LinkMenu, \"" +
                                                    Convert.ToString((cmpt * Limit) + 1) + " - " +
                                                    Convert.ToString((cmpt * Limit) + Limit) + "\", \"\"); \n ";
                                }
                                else if ((currentItemsCollection.Count % Limit) > 0)
                                {
                                    // string displayedItemsNb = Convert.ToString((currentItemsCollection.Count % Limit) + currentItemsCollection.Count);
                                    string displayedItemsNb =
                                        Convert.ToString((currentItemsCollection.Count % Limit) +
                                                         ((currentItemsCollection.Count / Limit) * Limit));
                                    menuLinkWith += "var PagedMenu1 = CASubM(LinkMenu, \"" +
                                                    Convert.ToString((currentItemsCollection.Count / Limit) + 1) + " - " +
                                                    displayedItemsNb + "\", \"\"); \n ";
                                }

                                if (itemMenuPerformanceMode && (currentItemsCollection.Count > 0))
                                    menuLinkWith +=
                                        "PagedMenu1.setAttribute(\"enabled\", ReadyToLink(currentItemID)); \n ";

                                foreach (SPListItem currentListItem in currentItemsCollection)
                                {
                                    if ((cmpt2 >= (cmpt * Limit)) && (cmpt2 < ((cmpt * Limit) + Limit)))
                                    {
                                        language = currentListItem.GetFormattedValue("SharePoint_Item_Language");
                                        language = language.Replace("(SPS_LNG_ALL)", "ALL").Replace("SPS_LNG_",
                                                                                                    string.Empty);

                                        if (current.Fields.ContainsField("SharePoint_Group_Language"))
                                            if ((currentListItem["SharePoint_Group_Language"] != null) &&
                                                (Convert.ToInt32(currentListItem["SharePoint_Group_Language"]) != 0))
                                                groupLanguageListArrayList.Add(new[]
                                                                                   {
                                                                                       currentListItem.ID.ToString(),
                                                                                       currentListItem["SharePoint_Group_Language"].ToString()
                                                                                   });
                                            else
                                                groupLanguageListArrayList.Add(new[] { currentListItem.ID.ToString(), "-1" });

                                        if (currentListItem["SharePoint_Item_Language"] != null)
                                            if ((!Equals(currentListItem["SharePoint_Item_Language"], "(SPS_LNG_ALL)")) &&
                                                (!Equals(currentListItem["SharePoint_Item_Language"], string.Empty)))
                                                ((ArrayList)
                                                 itemLanguageListHashTable[currentListItem["SharePoint_Item_Language"].ToString()]).Add(currentListItem.ID.ToString());
                                            else
                                                ((ArrayList)itemLanguageListHashTable["(SPS_LNG_ALL)"]).Add(currentListItem.ID.ToString());
                                        else
                                            ((ArrayList)itemLanguageListHashTable["(SPS_LNG_ALL)"]).Add(currentListItem.ID.ToString());

                                        if (!string.IsNullOrEmpty(language))
                                        {
                                            if (language == "ALL")
                                            {
                                                menuLinkWith += "var LanguageNotDefinedMenu = CAMOpt(PagedMenu1, \"" +
                                                                currentListItem.GetFormattedValue("ID") + " : " +
                                                                currentListItem.Name.Replace("\\", "\\\\").Replace("\"", "\\\"") + " | " + language +
                                                                "\", \"javascript:window.location.search = '?SPS_ListID=" +
                                                                current.ID +
                                                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=LinkListItem" +
                                                                unfilteringQueryString + "&SPS_LinkWith=" +
                                                                currentListItem.GetFormattedValue("ID") + "'\"); \n ";
                                                menuLinkWith +=
                                                    "LanguageNotDefinedMenu.setAttribute(\"enabled\", \"false\"); \n ";
                                            }
                                            else
                                            {
                                                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                                                {
                                                    if (languageItem.LanguageDestination == language)
                                                    {
                                                        menuLinkWith += "var ItemDisplayState = CAMOpt(PagedMenu1, \"" +
                                                                currentListItem.GetFormattedValue("ID") + " : " +
                                                                currentListItem.Name.Replace("\\", "\\\\").Replace("\"", "\\\"") + " | " +
                                                                languageItem.DisplayName +
                                                                "\", \"javascript:window.location.search = '?SPS_ListID=" +
                                                                current.ID +
                                                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=LinkListItem" +
                                                                unfilteringQueryString + "&SPS_LinkWith=" +
                                                                currentListItem.GetFormattedValue("ID") + "'\"); \n ";
                                                        menuLinkWith +=
                                                            "ItemDisplayState.setAttribute(\"enabled\", GetLanguage(currentItemID,\"" +
                                                            language + "\",\"false\",\"true\")); \n ";
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            menuLinkWith += "var LanguageNotDefinedMenu = CAMOpt(PagedMenu1, \"" +
                                                            currentListItem.GetFormattedValue("ID") + " : " +
                                                            currentListItem.Name.Replace("\\", "\\\\").Replace("\"", "\\\"") + " | " + language + TextLanguageNotDef +
                                                            "\", \"\"); \n ";
                                            menuLinkWith +=
                                                "LanguageNotDefinedMenu.setAttribute(\"enabled\", \"false\"); \n ";
                                        }

                                        cmpt2++;
                                    }
                                    else
                                    {
                                        cmpt2++;
                                    }
                                }

                                subMenuNb--;
                            }

                            if (itemMenuPerformanceMode)
                            {
                                menuLinkWith += "strImageSetLinkablePath = ctx.imagesPath + \"CHECKOUT.gif\";\n";
                                menuLinkWith += "strImageSetRemoveLinkablePath = ctx.imagesPath + \"CHECKIN.gif\";\n";
                                menuLinkWith += "var ItemLinkable = CAMOpt(LinkMenu, \"" + TextSetLinkable +
                                                "\", \"javascript:window.location.search = '?SPS_ListID=" + current.ID +
                                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=LinkListItem" +
                                                unfilteringQueryString + "&SPS_LinkWith=" + "Linkable" +
                                                "'\", strImageSetLinkablePath, null, 300); \n ";
                                menuLinkWith +=
                                    "ItemLinkable.setAttribute(\"enabled\", IsLinkable(currentItemID, \"false\", \"true\")); \n ";
                                menuLinkWith += "var ItemNoLinkable = CAMOpt(LinkMenu, \"" + TextRemovetLinkable +
                                                "\", \"javascript:window.location.search = '?SPS_ListID=" + current.ID +
                                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=LinkListItem" +
                                                unfilteringQueryString + "&SPS_LinkWith=" + "Linkable" +
                                                "'\", strImageSetRemoveLinkablePath, null, 300); \n ";
                                menuLinkWith +=
                                    "ItemNoLinkable.setAttribute(\"enabled\", IsLinkable(currentItemID, \"true\", \"false\")); \n ";
                            }

                            menuLinkWith += "strImagePath = ctx.imagesPath + \"delitem.gif\";\n";
                            menuLinkWith += "strImageCheckPath = ctx.imagesPath + \"CHKMRK.GIF\";\n";

                            menuLinkWith += "CAMOpt(LinkMenu, \"" + TextDeleteLinks +
                                            "\", \"javascript:window.location.search = '?SPS_ListID=" + current.ID +
                                            "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=LinkListItem" +
                                            unfilteringQueryString + "&SPS_LinkWith=" + "Delete" +
                                            "'\", strImagePath, null, 300); \n ";

                            menuLinkWith += "CAMSep(LinkMenu); \n ";

                            if (current.Fields.ContainsField(lang + " version"))
                            {
                                if (current.Fields[lang + " version"].Hidden == false)
                                    menuLinkWith += "CAMOpt(LinkMenu, \"" + TextHideFiels +
                                                    "\", \"javascript:window.location.search = '?SPS_ListID=" +
                                                    current.ID + "&SPS_Trans_Code=HideFieldsForLinks" +
                                                    "&SPS_Hide_Action=True" + "'\"); \n ";
                                else
                                    menuLinkWith += "CAMOpt(LinkMenu, \"" + TextShowFields +
                                                    "\", \"javascript:window.location.search = '?SPS_ListID=" +
                                                    current.ID + "&SPS_Trans_Code=HideFieldsForLinks" +
                                                    "&SPS_Hide_Action=False" + "'\"); \n ";

                                menuLinkWith += "CAMSep(LinkMenu); \n ";

                                if ((bool)HttpContext.Current.Cache["AlphamosaikDisplayItemDashboard"] &&
                                    (!itemMenuPerformanceMode))
                                {
                                    if (current.Fields.ContainsField("SharePoint_Group_Language"))
                                        menuLinkWith += "CAMOpt(LinkMenu, \"" + TextShowLinkedItemsDashBoard +
                                                        "\", \"javascript:window.open('" +
                                                        context.Request.Url.GetLeftPart(UriPartial.Path) +
                                                        "?FilterField1=SharePoint%5FGroup%5FLanguage&FilterValue1='+GetGroupLanguage(currentItemID)+'&SPS_Trans_Code_Pers=Unfiltering', 'Alphamosaik_dashboard', config='height=700, width=1000, toolbar=no, menubar=no, scrollbars=no, resizable=yes, location=yes, directories=no, status=no')\"); \n ";
                                    else
                                    {
                                        menuLinkWith += "var DashBoardMenu = CAMOpt(LinkMenu, \"" +
                                                        TextShowLinkedItemsDashBoard + "\", \"javascript:window.open('" +
                                                        context.Request.Url.GetLeftPart(UriPartial.Path) +
                                                        "?FilterField1=SharePoint%5FGroup%5FLanguage&FilterValue1='+GetGroupLanguage(currentItemID)+'&SPS_Trans_Code_Pers=Unfiltering', 'Alphamosaik_dashboard', config='height=700, width=1000, toolbar=no, menubar=no, scrollbars=no, resizable=yes, location=yes, directories=no, status=no')\"); \n ";
                                        menuLinkWith += "DashBoardMenu.setAttribute(\"enabled\", \"false\"); \n ";
                                    }
                                }
                            }

                            addedMenuItem = true;
                        }

                        break;
                    }
                }

                objSpWeb.Dispose();

                menuLinkWith += "var LangMenu = CASubM(m, \"" + TextDefineLanguage + "\", \"\"); \n ";

                if (addedMenuItem && !string.IsNullOrEmpty(listId))
                {
                    AddMenuItemForList(tempResponse, itemLanguageListHashTable, itemMenuPerformanceMode,
                                       currentItemsCollection, announcementList, menuLinkWith,
                                       groupLanguageListArrayList, listId);
                }

                AddMenuEnableItemTradFromList(tempResponse, listId, listIdForEnableTradFromList, mustUpgrade);
            }

            string menuUnfiltered = AddMenuItemUnfiltred();

            string valueRegexFiltering = string.Empty;

            if (filteringButton)
            {
                valueRegexFiltering = TranslatorRegex.ServerMenuRegex.Match(tempResponse.ToString()).Value;
            }

            if (!string.IsNullOrEmpty(valueRegexFiltering))
                tempResponse.Replace(valueRegexFiltering, valueRegexFiltering + menuUnfiltered);

            if (userHaveEditPermission && (bool)HttpContext.Current.Cache["AlphamosaikDictionaryAccessButton"])
            {
                AddMenuDictionaryAccess(tempResponse);
            }

            if (userHaveEditPermission && (bool)HttpContext.Current.Cache["AlphamosaikCompletingMode"])
            {
                AddMenuCompletingMode(tempResponse, lang, languageSource, completingDictionaryMode);
            }
        }

        public override void AddCheckMarkToPersonalLanguageMenu(StringBuilder tempResponse, string lang)
        {
            var menuItemIdToSearch = new StringBuilder("ie:menuitem id=\"zz.*?_");

            for (int index = 0; index < Languages.Instance.AllLanguages.Length; index++)
            {
                if (lang.Equals(Languages.Instance.AllLanguages[index], StringComparison.OrdinalIgnoreCase))
                {
                    menuItemIdToSearch.Append(Languages.Instance.Lcid[index]);
                    menuItemIdToSearch.Append("\"");
                    break;
                }
            }

            MatchCollection result = Regex.Matches(tempResponse.ToString(), menuItemIdToSearch.ToString());

            foreach (Match currentMatch in result)
            {
                int position = tempResponse.IndexOf(currentMatch.Value, StringComparison.OrdinalIgnoreCase);

                if (position != -1)
                {
                    string newMenuItem = currentMatch.Value + " iconSrc=\"/_layouts/images/alpha_logo_menu.png\"";
                    tempResponse.Replace(currentMatch.Value, newMenuItem);
                }
            }
        }

        private static void DisplayProfileSelection(StringBuilder tempResponse, string valueFieldDisplay)
        {
            try
            {
                string siteUrl = SPContext.Current.Site.Url;
                SPUser currentUser = SPContext.Current.Web.CurrentUser;
                var userProfileList = new List<string>();
                using (var sysSite = new SPSite(siteUrl))
                using (SPWeb web = sysSite.OpenWeb())
                {
                    bool isUserExistOnRoot = false;
                    foreach (SPUser user in web.SiteUsers)
                    {
                        if (user.ID == currentUser.ID)
                        {
                            isUserExistOnRoot = true;
                            break;
                        }
                    }

                    if (isUserExistOnRoot)
                    {
                        if (HttpRuntime.Cache != null && HttpRuntime.Cache[OceanikAutomaticTranslation] != null)
                        {
                            var automaticTranslation = HttpRuntime.Cache[OceanikAutomaticTranslation] as IAutomaticTranslation;

                            if (automaticTranslation != null)
                            {
                                userProfileList = automaticTranslation.LoadUserAccount(web, null).Profiles;
                            }
                        }
                    }

                    string metadataChoiceJs = string.Empty;

                    metadataChoiceJs += "<TR><TD></TD><TD>\n";
                    metadataChoiceJs += "<table id=\"TranslationProfileTable\"  >\n";

                    metadataChoiceJs += "<TR style=\"display:" + valueFieldDisplay + "\" id=\"TranslationProfile\">\n";
                    metadataChoiceJs +=
                        "<TD nowrap=\"true\" valign=\"top\" width=\"190px\" class=\"ms-formlabel\"><H3 class=\"ms-standardheader\">\n";
                    metadataChoiceJs += "<nobr>" + "Translation Profile" + "</nobr>\n";
                    metadataChoiceJs += "</H3></TD>\n";
                    metadataChoiceJs += "<TD valign=\"top\" class=\"ms-formbody\">\n";
                    metadataChoiceJs += "<span dir=\"none\">\n";

                    metadataChoiceJs +=
                        "<select onchange=\"AddProfile(this.value,this.selected)\" id=\"AlphaProfile\" name=\"AlphaProfile\" class=\"ms-RadioText\" title=\"Translation Profile\" />\n ";

                    if (userProfileList.Count == 0) metadataChoiceJs += "<option value=\"" + "No Profile" + "\">" + "Not available" + "</option>\n";

                    foreach (string profile in userProfileList)
                    {
                        {
                            string checkedState = string.Empty;

                            if (SPContext.Current.List.Fields.ContainsField("Translation Profile"))
                                if ((SPContext.Current.ListItem != null)
                                    && (SPContext.Current.ListItem["Translation Profile"] != null)
                                    && (SPContext.Current.ListItem["Translation Profile"].ToString() == profile))
                                {
                                    checkedState = " selected=\"selected\"";
                                }

                            metadataChoiceJs += "<option" + checkedState + " value=\"" + profile + "\">" + profile
                                                + "</option>\n";
                        }
                    }

                    if (SPContext.Current.List.Fields.ContainsField("Translation Profile") && userProfileList.Count > 0)
                        if ((SPContext.Current.ListItem != null)
                            && ((SPContext.Current.ListItem["Translation Profile"] == null)
                            || (SPContext.Current.ListItem["Translation Profile"].ToString() == string.Empty)))
                        {
                            metadataChoiceJs += "<option" + " selected=\"selected\"" + " value=\"" + string.Empty + "\">" + "None"
                                                + "</option>\n";
                        }

                    metadataChoiceJs += "</select><br>\n";

                    metadataChoiceJs += "</span>\n";
                    metadataChoiceJs += "</TD>\n";
                    metadataChoiceJs += "</TR>\n";

                    metadataChoiceJs += "</table>\n";
                    metadataChoiceJs += "</TD></TR>\n";

                    if (TranslatorRegex.HasMetadataToDuplicateFieldRegex.IsMatch(tempResponse.ToString()))
                    {
                        int positionNextTr = tempResponse.IndexOf(
                            "</TR>",
                            tempResponse.IndexOf(@"<!-- FieldName=""Translation Profile"""),
                            StringComparison.OrdinalIgnoreCase);
                        tempResponse.Insert(positionNextTr + 5, metadataChoiceJs);
                    }

                    string displayAddProfileJs = string.Empty;
                    if (TranslatorRegex.HasMetadataToDuplicateFieldRegex.IsMatch(tempResponse.ToString()))
                    {
                        int positionId = tempResponse.IndexOf(
                            " id=",
                            tempResponse.IndexOf(@"<!-- FieldName=""Translation Profile"""),
                            StringComparison.OrdinalIgnoreCase);
                        int positionIdEnd = tempResponse.IndexOf("\"", positionId + 5);
                        string idMetadataField = tempResponse.Substring(positionId + 5, positionIdEnd - positionId - 5);

                        displayAddProfileJs += "<script LANGUAGE='JavaScript' >\n";
                        displayAddProfileJs += "function AddProfile(profile,value)\n";
                        displayAddProfileJs += "{\n";
                        displayAddProfileJs += "var field = document.getElementById('" + idMetadataField + "');\n";
                        displayAddProfileJs += "if (value!=false) {field.value = profile;}\n";
                        displayAddProfileJs += "}\n";
                    }

                    string hasBodyEndTagRegexReplaced =
                        TranslatorRegex.HasBodyEndTagRegex.Replace(
                            tempResponse.ToString(), displayAddProfileJs + "\n</script>\n</body>");
                    tempResponse.Clear().Append(hasBodyEndTagRegexReplaced);
                }
            }
            catch (Exception ex)
            {
                Utilities.TraceNormalCaughtException("DisplayProfileSelection", ex);
            }
        }

        private static bool WriteBuffersForOutputCache(string tempResponse)
        {
            if (HttpContext.Current == null)
                return false;

            bool responseWrited = false;

            FieldInfo httpWritterInfo = HttpContext.Current.Response.GetType().GetField("_httpWriter", BindingFlags.NonPublic | BindingFlags.Instance);

            if (httpWritterInfo != null)
            {
                var httpWritter = (HttpWriter)httpWritterInfo.GetValue(HttpContext.Current.Response);

                FieldInfo substElementInfo = httpWritter.GetType().GetField("_substElements", BindingFlags.NonPublic | BindingFlags.Instance);

                if (substElementInfo != null)
                {
                    var substElement = (ArrayList)substElementInfo.GetValue(httpWritter);

                    if (substElement != null && substElement.Count > 0)
                    {
                        int startIndex = 0;

                        var substElementCloned = (ArrayList)substElement.Clone();

                        HttpContext.Current.Response.ClearContent();

                        foreach (var element in substElementCloned)
                        {
                            FieldInfo callbackInfo = element.GetType().GetField("_callback", BindingFlags.NonPublic | BindingFlags.Instance);

                            if (callbackInfo != null)
                            {
                                var callback = (HttpResponseSubstitutionCallback)callbackInfo.GetValue(element);

                                if (callback != null)
                                {
                                    string callbackText = callback(HttpContext.Current);

                                    int currenytIndex = tempResponse.IndexOf(callbackText, startIndex);

                                    if (currenytIndex != -1)
                                    {
                                        httpWritter.Write(tempResponse.Substring(startIndex, currenytIndex - startIndex));
                                        HttpContext.Current.Response.WriteSubstitution(callback);

                                        startIndex = currenytIndex + callbackText.Length;
                                    }
                                }
                            }
                        }

                        if (startIndex < tempResponse.Length)
                        {
                            httpWritter.Write(tempResponse.Substring(startIndex));
                        }

                        responseWrited = true;
                    }
                }
            }

            return responseWrited;
        }

        /// <summary>
        /// Replace the first occurrence of a string in a string
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="oldValue">The old Value.</param>
        /// <param name="newValue">The new Value.</param>
        private static void ReplaceFirstOccurrence(StringBuilder original, string oldValue, string newValue)
        {
            int loc = original.IndexOf(oldValue);
            if (loc != -1)
            {
                original.Remove(loc, oldValue.Length).Insert(loc, newValue);
            }
        }

        /// <summary>
        /// Veirfy if webpart contains properties
        /// </summary>
        /// <param name="web">The web object.</param>
        /// <param name="webPartId">The web Part Id.</param>
        /// <returns>The has web part functionnality.</returns>
        private static bool HasWebPartFunctionnality(SPWeb web, string webPartId)
        {
            try
            {
                return web.AllProperties.ContainsKey("Alphamosaik.Translator.WebParts " + webPartId);
            }
            catch (Exception e)
            {
                Utilities.LogException("HasWebpartFunctionnality", e, EventLogEntryType.Warning);
                return false;
            }
        }

        /// <summary>
        /// Get Web part properties
        /// </summary>
        /// <param name="web">The web object.</param>
        /// <param name="webPartId">The web Part Id.</param>
        /// <returns>The get web part functionnality.</returns>
        private static string GetWebPartFunctionnality(SPWeb web, string webPartId)
        {
            try
            {
                if (web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId] != null)
                    return (string)web.AllProperties["Alphamosaik.Translator.WebParts " + webPartId];
            }
            catch (Exception e)
            {
                Utilities.LogException("GetWebpartFunctionnality", e, EventLogEntryType.Warning);
            }

            return string.Empty;
        }

        private static void TranslateTagMain(StringBuilder tempResponse, string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagMainEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.TagMainRegex.Replace(tempResponse.ToString(), TagMainEvaluator.MatchEvaluatorTag));
        }

        /// <summary>
        /// Traitement de la page en langue par défaut, pour le mode auto complétion
        /// Le traitement est appelé après le clic sur le bouton 'display completing mode on this page'
        /// </summary>
        /// <param name="tempResponse">réponse html</param>
        /// <param name="defaultLanguage">langue par défaut</param>
        /// <param name="initLanguage">langue de la page sur laquelle on a cliqué sur le bouton 'display completing mode on this page'</param>
        private static void AutoCompletionModeDefaultPageProcess(StringBuilder tempResponse, string defaultLanguage, string initLanguage)
        {
            try
            {
                // idProcess = DateTime.Now.TimeOfDay.TotalMilliseconds;
                _autoCompletionPageToTreat = tempResponse.ToString();

                string lcid = string.Empty;
                string defaultPageUrl = HttpContext.Current.Request.RawUrl;

                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                {
                    if (initLanguage == languageItem.LanguageDestination)
                    {
                        lcid = languageItem.Lcid.ToString();
                        break;
                    }
                }

                var cookie = new HttpCookie("lcid", lcid)
                {
                    Expires = DateTime.MaxValue
                };
                HttpContext.Current.Response.AppendCookie(cookie);

                string initPageUrl = defaultPageUrl.Replace("SPSLanguage=" + defaultLanguage, "SPSLanguage=" + initLanguage).Replace("&SPS_Trans_Code=Completing_Dictionary_Mode_Process1", "&SPS_Trans_Code=Completing_Dictionary_Mode_Process2");
                HttpContext.Current.Response.Redirect(initPageUrl, false);
            }
            catch (Exception e)
            {
                Utilities.LogException("AutoCompletionModeDefaultPageProcess", e, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Traitement de la page sur laquelle on a cliqué sur le bouton 'display completing mode on this page', pour le mode auto complétion
        /// Le traitement est appelé après celui opéré sur la page en langue par défaut
        /// </summary>
        /// <param name="tempResponse">The temp Response.</param>
        /// <param name="languageDestination">The language Code.</param>
        /// <param name="languageSource">default site language</param>
        private static void AutoCompletionModeInitPageProcess(StringBuilder tempResponse, string languageDestination, string languageSource)
        {
            try
            {
                AutoCompletionModeFinalProcess(tempResponse, _autoCompletionPageToTreat, languageDestination, languageSource);
            }
            catch (Exception e)
            {
                Utilities.LogException("AutoCompletionModeInitPageProcess", e, EventLogEntryType.Warning);

                _autoCompletionPageToTreat = string.Empty;
            }
        }

        /// <summary>
        /// Traitements de flux Html pour le mode auto complétion : on compare les 2 flux, puis on ajoute les surlignements dans le html, puis on clean up les effets de bords
        /// dans le html.
        /// </summary>
        /// <param name="tempResponse">Flux Html de la page sur laquelle on a cliqué sur le bouton 'display completing mode on this page' 
        ///   : c'est ce flux qui est modifié pour le surlignement des termes non traduits</param>
        /// <param name="defaultPageTempResponse">Flux Html de la page en langue par défaut : on le compare à html</param>
        /// <param name="languageDestination">The language Code.</param>
        /// <param name="languageSource">default site language</param>
        private static void AutoCompletionModeFinalProcess(StringBuilder tempResponse, string defaultPageTempResponse, string languageDestination, string languageSource)
        {
            try
            {
                MatchCollection defaultPageTempResponseResult = TranslatorRegex.TagMainRegex.Matches(defaultPageTempResponse);
                MatchCollection tempResponseResult = TranslatorRegex.TagMainRegex.Matches(tempResponse.ToString());

                var matchTempResponseResultList = new ArrayList(tempResponseResult);
                var matchDefaultPageTempResponseResultList = new ArrayList(defaultPageTempResponseResult);

                matchTempResponseResultList.Reverse();
                matchDefaultPageTempResponseResultList.Reverse();

                int cmpt = 0;

                BaseDictionary dictionary = Dictionaries.Instance.GetRootDictionary(SPContext.Current.Site.WebApplication.Id, SPContext.Current.Site.ID, SPContext.Current.Web.ID);

                foreach (Match match in matchTempResponseResultList)
                {
                    if (matchDefaultPageTempResponseResultList.Count > cmpt)
                    {
                        string defaultPageValue = ((Match)matchDefaultPageTempResponseResultList[cmpt]).Value.Substring(1, ((Match)matchDefaultPageTempResponseResultList[cmpt]).Value.Length - 2);

                        if ((match.Value == ((Match)matchDefaultPageTempResponseResultList[cmpt]).Value) && (!string.IsNullOrEmpty(match.Value.Substring(1, match.Value.Length - 2).Replace("\\r", string.Empty).Replace("\\n", string.Empty).Replace("//", string.Empty).Replace("&#160;", string.Empty).Trim())))
                        {
                            string colorHightlighting = "yellow";

                            string translated;
                            if (dictionary.ContainText(defaultPageValue, languageSource, languageDestination, out translated))
                            {
                                if (string.IsNullOrEmpty(translated.Trim()))
                                {
                                    colorHightlighting = "#E68DC3";
                                }
                                else if (translated.Trim() == defaultPageValue)
                                {
                                    cmpt++;
                                    continue;
                                }
                            }

                            string highlightingTag = "<span alphaSpan=\"true\" style=\"border:solid 1px black; color:black; background:" + colorHightlighting + "; \" >";

                            if (!match.Value.Equals(">&nbsp;<", StringComparison.OrdinalIgnoreCase))
                            {
                                tempResponse.Insert(match.Index + match.Length - 1, "</span>").Insert(match.Index + 1, highlightingTag);
                            }
                        }

                        cmpt++;
                    }
                }

                // Clean des régions JS
                string tempResponseInit = tempResponse.ToString();

                foreach (Match javascriptAreaBegin in TranslatorRegex.JavascriptAreasBeginRegex.Matches(tempResponseInit))
                {
                    int javascriptAreaEndIndex = tempResponseInit.IndexOf("</script>", javascriptAreaBegin.Index);
                    string javascriptArea = tempResponseInit.Substring(javascriptAreaBegin.Index, javascriptAreaEndIndex - javascriptAreaBegin.Index);
                    string javascriptAreaInit = javascriptArea;

                    var spanTagEndArrayList = new ArrayList();
                    foreach (Match spanTagBegin in TranslatorRegex.SpanRegex.Matches(javascriptArea))
                    {
                        int spanTagEndIndex = javascriptArea.IndexOf("</span>", spanTagBegin.Index);
                        spanTagEndArrayList.Add(spanTagEndIndex);
                    }

                    spanTagEndArrayList.Reverse();

                    foreach (int spanTagEndIndex in spanTagEndArrayList)
                    {
                        javascriptArea = javascriptArea.Remove(spanTagEndIndex, 7);
                    }

                    foreach (Match spanTagBegin in TranslatorRegex.SpanRegex.Matches(javascriptArea))
                    {
                        javascriptArea = javascriptArea.Replace(spanTagBegin.Value, string.Empty);
                    }

                    tempResponse.Replace(javascriptAreaInit, javascriptArea);
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("AutoCompletionModeFinalProcess", e, EventLogEntryType.Warning);
            }
        }

        private static void TranslateTagText(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagTextEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.TextRegex.Replace(tempResponse.ToString(), TagTextEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagMsPickerFooter(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagMsPickerFooterEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.MsPickerFooterRegex.Replace(tempResponse.ToString(), TagMsPickerFooterEvaluator.MatchEvaluatorTag));
        }

        /*
         * WebForm_InitCallback();var _fV4Calendar= true;function _spAjaxOnLoadWaitWPQ2(){
         * function _spAjaxCalendarInitWPQ2(){
         * SP.UI.ApplicationPages.CalendarContainerFactory.create(document.getElementById('ctl00_m_g_108d85d4_d560_41d5_b511_6ba382184976_ctl01_ctl00_ctl00'),{ctxId:'WPQ2',dataSources:[{id:'00000000-0000-0000-0000-000000000000',name:'',
         * color:'',formUrl:'\u002fLists\u002fCalendar\u002fDispForm.aspx',primary:true, disableDrag:false}],userInfo:{current:{id:'1073741823',loginName:'SHAREPOINT\\system',displayName:'System Account',email:''}},enablePeople:false,enableResource:
         * false,usePostBack:false,canUserCreateItem:true,sharedPickerClientId:null,reservationContentTypeId:'0x0102004F51EFDEA49C49668EF9C6744C8CF87D',aM12String:'12:00 am',serviceUrl:'\u002f_layouts\u002fCalendarService.ashx'},'month','',
         * [{"Options":41,"Table":null,"DatePicker":null,"Dates":["8/29/2010","8/30/2010","8/31/2010","9/1/2010","9/2/2010","9/3/2010","9/4/2010","9/5/2010","9/6/2010","9/7/2010","9/8/2010","9/9/2010","9/10/2010","9/11/2010",
         * "9/12/2010","9/13/2010","9/14/2010","9/15/2010","9/16/2010","9/17/2010","9/18/2010","9/19/2010","9/20/2010","9/21/2010","9/22/2010","9/23/2010","9/24/2010","9/25/2010","9/26/2010","9/27/2010","9/28/2010","9/29/2010","9/30/2010",
         * "10/1/2010","10/2/2010","10/3/2010"],"RangeJDay":[149624,149658],"Navs":null,"Items":{"Data":[[0,1,2,149633,149633,3,3,4,5,10,0,60,0,0,0,2,6],[7,8,2,149640,149640,9,9,4,5,10,0,60,0,0,0,2,6],[10,11,2,149647,149647,12,12,4,5,10,0,60,
         * 0,0,0,2,6],[13,14,2,149648,149648,15,15,5,16,11,0,60,0,0,0,2,6]],"Strings":["1","Bon matin","","9/7/2010","10:00 am","11:00 am","0x7fffffffffffffff","2","Good night","9/14/2010","3","Test []","9/21/2010","4","teddwefwe","9/22/2010",
         * "12:00 pm"]}}]); 
         * 
         * */

        private static void TranslateCalendarView(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            if (SPContext.Current.List == null || !SPContext.Current.List.BaseTemplate.Equals(SPListTemplateType.Events))
                return;

            TagTranslateCalendarView.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.StringsRegex.Replace(tempResponse.ToString(), TagTranslateCalendarView.MatchEvaluatorTag));
        }

        private static void TranslateMultilookupPicker(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            // translate datas
            TagTranslateMultilookupPicker.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);
            tempResponse.SetNewString(TranslatorRegex.MultilookupPickerRegex.Replace(tempResponse.ToString(), TagTranslateMultilookupPicker.MatchEvaluatorTag));

            // translate initial datas
            TagTranslateMultilookupPicker.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);
            tempResponse.SetNewString(TranslatorRegex.MultilookupPickerInitialRegex.Replace(tempResponse.ToString(), TagTranslateMultilookupPicker.MatchEvaluatorTag));
        }

        private static void FilterCalendarView(string languageDestination, string defaultLanguage)
        {
            try
            {
                if (HttpContext.Current.Request.Path.EndsWith("/viewedit.aspx", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (SPContext.Current.List == null || !SPContext.Current.List.BaseTemplate.Equals(SPListTemplateType.Events))
                {
                    return;
                }

                if (SPContext.Current.ViewContext.View != null)
                {
                    if (SPContext.Current.ViewContext.View.Type != "CALENDAR")
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

                if (!SPContext.Current.List.Fields.ContainsField("SharePoint_Item_Language"))
                {
                    return;
                }

                SPView currentView = SPContext.Current.ViewContext.View;
                string currentViewUrl = currentView.Url;
                string currentViewUrlBase = string.Empty;

                string currentViewLanguage = defaultLanguage;

                if (TranslatorRegex.LangViewRegex.IsMatch(currentViewUrl))
                {
                    currentViewLanguage = TranslatorRegex.LangViewRegex.Match(currentViewUrl).Groups["lang"].Value;
                    currentViewUrlBase = TranslatorRegex.LangViewRegex.Match(currentViewUrl).Groups["urlBase"].Value;
                }
                else
                {
                    if (languageDestination == defaultLanguage)
                        return;

                    if (TranslatorRegex.LangViewRegex2.IsMatch(currentViewUrl))
                    {
                        currentViewUrlBase = TranslatorRegex.LangViewRegex2.Match(currentViewUrl).Groups["urlBase"].Value;
                    }
                }

                if (currentViewLanguage == languageDestination)
                {
                    return;
                }

                string redirectUrl = string.Empty;
                SPViewCollection viewsCollection = SPContext.Current.List.Views;

                foreach (SPView view in viewsCollection)
                {
                    if (view.Url.ToLower().Contains((currentViewUrl + languageDestination).ToLower()))
                    {
                        redirectUrl = view.Url;
                        break;
                    }

                    if (view.Url.ToLower().Contains((currentViewUrlBase + languageDestination).ToLower()))
                    {
                        redirectUrl = view.Url;
                        break;
                    }

                    string viewUrl = view.Url.ToLower();
                    string currentViewUrlBaseAspx = currentViewUrlBase.ToLower().Trim() + ".aspx";
                    if ((viewUrl == currentViewUrlBaseAspx) && (currentViewUrlBaseAspx != currentViewUrl.ToLower()))
                    {
                        redirectUrl = view.Url;
                        break;
                    }
                }

                if ((!string.IsNullOrEmpty(redirectUrl)) && (currentViewUrl != redirectUrl))
                    HttpContext.Current.Response.Redirect(SPContext.Current.Web.Url + "/" + redirectUrl, false);
            }
            catch (Exception e)
            {
                Utilities.LogException("FilterCalendarView", e, EventLogEntryType.Warning);
            }
        }

        private static void TranslateDateInSharePointList(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagDateInSharePointListEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);
            tempResponse.SetNewString(TranslatorRegex.TagDateInSharepointListRegex.Replace(tempResponse.ToString(), TagDateInSharePointListEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagLayoutInfo(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagLayoutInfoEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.LayoutInfoRegex.Replace(tempResponse.ToString(), TagLayoutInfoEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagMoveToDate(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagMoveToDateEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.MoveToDateRegex.Replace(tempResponse.ToString(), TagMoveToDateEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagNavigationNode(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagNavigationModeEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.NavigationNodeRegex.Replace(tempResponse.ToString(), TagNavigationModeEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagDescription(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagDescriptionEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.DescriptionRegex.Replace(tempResponse.ToString(), TagDescriptionEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagTitle(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagTitleEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.TagTitleRegex.Replace(tempResponse.ToString(), TagTitleEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagAlt(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagAltEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.TagAltRegex.Replace(tempResponse.ToString(), TagAltEvaluator.MatchEvaluatorTag));
        }

        private static void FilterTocLayoutMain(StringBuilder tempResponse, string languageDestination, string url)
        {
            if (tempResponse.IndexOf("div class=\"toc-layout-main\"") > -1)
            {
                TagTocLayoutMain.Init(tempResponse, url, SPContext.Current, languageDestination);

                tempResponse.SetNewString(TranslatorRegex.DfwpListRegex.Replace(tempResponse.ToString(), TagTocLayoutMain.MatchEvaluatorTag));
            }
        }

        private static void TranslateTagButtons(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            // For Buttons
            TagButtonsEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.TypeButtonSubmitResetRegex.Replace(tempResponse.ToString(), TagButtonsEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagInnerHtml(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            // For innerHTML areas (Body of announcements for example)
            if (tempResponse.IndexOf(".innerHTML = \"") == -1)
                return;

            TagInnerHtmlEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.InnerHtmlRegex.Replace(tempResponse.ToString(), TagInnerHtmlEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateSearchBox(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            if (tempResponse.IndexOf("class=\"ms-sbplain\"") > -1)
            {
                TagSearchBox.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

                tempResponse.SetNewString(TranslatorRegex.InputSbPlainTagRegex.Replace(tempResponse.ToString(), TagSearchBox.MatchEvaluatorTag));
            }
        }

        private static void TranslateDatePicker(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagDatePickerEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.DayHeaderRegex.Replace(tempResponse.ToString(), TagDatePickerEvaluator.MatchEvaluatorTag));
        }

        private static void TranslateTagRollover(StringBuilder tempResponse,
            string currentLccode, int extractorStatus, string url, string languageDestination)
        {
            TagRolloverEvaluator.Init(tempResponse, extractorStatus, url, SPContext.Current, currentLccode, languageDestination);

            tempResponse.SetNewString(TranslatorRegex.TagRolloverRegex.Replace(tempResponse.ToString(), TagRolloverEvaluator.MatchEvaluatorTag));
        }

        private static void SaveApplicationSettingsToHttpContextCache()
        {
            string messageForAutotranslateText = ConfigStore.Instance.GetValue("Oceanik", "MessageForAutotranslateText", SPContext.Current.Site.Url) ?? string.Empty;
            string bingApplicationId = string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "BingApplicationId", SPContext.Current.Site.Url)) ? "674781504275C8FC33D2AE3FF4345CBAA4979EF7"
                : ConfigStore.Instance.GetValue("Oceanik", "BingApplicationId", SPContext.Current.Site.Url);

            bool extractor;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "Extractor", SPContext.Current.Site.Url), out extractor);

            bool completingMode = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "CompletingMode", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "CompletingMode", SPContext.Current.Site.Url), out completingMode))
                {
                    completingMode = true;
                }
            }

            bool webPartContentTranslation = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "WebPartContentTranslation", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "WebPartContentTranslation", SPContext.Current.Site.Url), out webPartContentTranslation))
                {
                    webPartContentTranslation = true;
                }
            }

            bool bannerCssClass;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "BannerCSSClass", SPContext.Current.Site.Url), out bannerCssClass);

            bool replaceLinkedPagesUrl;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "ReplaceLinkedPagesUrl", SPContext.Current.Site.Url), out replaceLinkedPagesUrl);

            bool bannerPipe = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "BannerPipe", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "BannerPipe", SPContext.Current.Site.Url), out bannerPipe))
                {
                    bannerPipe = true;
                }
            }

            bool defaultLangDeactivation;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "DefaultLangDeactivation", SPContext.Current.Site.Url), out defaultLangDeactivation);

            bool itemFiltering = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "ItemFiltering", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "ItemFiltering", SPContext.Current.Site.Url), out itemFiltering))
                {
                    itemFiltering = true;
                }
            }

            bool displayItemDashboard = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "DisplayItemDashboard", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "DisplayItemDashboard", SPContext.Current.Site.Url), out displayItemDashboard))
                {
                    displayItemDashboard = true;
                }
            }

            bool redirectToLinkedPage;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "RedirectToLinkedPage", SPContext.Current.Site.Url), out redirectToLinkedPage);

            bool activatedLog;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "ActivatedLog", SPContext.Current.Site.Url), out activatedLog);

            bool deactivateEventHandlerOnList;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "DeactivateEventHandlerOnList", SPContext.Current.Site.Url), out deactivateEventHandlerOnList);

            bool wildcardsFeature = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "WildcardsFeature", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "WildcardsFeature", SPContext.Current.Site.Url), out wildcardsFeature))
                {
                    wildcardsFeature = true;
                }
            }

            bool filteringButton = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "FilteringButton", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "FilteringButton", SPContext.Current.Site.Url), out filteringButton))
                {
                    filteringButton = true;
                }
            }

            bool quickLaunchFilter;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "QuickLaunchFilter", SPContext.Current.Site.Url), out quickLaunchFilter);

            bool listFiteringDisplay;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "ListFiteringDisplay", SPContext.Current.Site.Url), out listFiteringDisplay);

            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "UseCacheDisabled", SPContext.Current.Site.Url)))
            {
                bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "UseCacheDisabled", SPContext.Current.Site.Url), out _useCacheDisabled);
            }

            bool resxFilesUpdate;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "ResxFilesUpdate", SPContext.Current.Site.Url), out resxFilesUpdate);

            bool topNavigationBarFilter;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "TopNavigationBarFilter", SPContext.Current.Site.Url), out topNavigationBarFilter);

            bool bannerWithNoTranslation;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "BannerWithNoTranslation", SPContext.Current.Site.Url), out bannerWithNoTranslation);

            string languageFieldLabel = ConfigStore.Instance.GetValue("Oceanik", "LanguageFieldLabel", SPContext.Current.Site.Url) ?? string.Empty;

            bool upgradeFromVersion2007;
            bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "UpgradeFromVersion2007", SPContext.Current.Site.Url), out upgradeFromVersion2007);

            bool dictionaryAccessButton = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "DictionaryAccessButton", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "DictionaryAccessButton", SPContext.Current.Site.Url), out dictionaryAccessButton))
                {
                    dictionaryAccessButton = true;
                }
            }

            bool discussionBoardTranslationOptionsHide = true;
            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "DiscussionBoardTranslationOptionsHide", SPContext.Current.Site.Url)))
            {
                if (!bool.TryParse(ConfigStore.Instance.GetValue("Oceanik", "DiscussionBoardTranslationOptionsHide", SPContext.Current.Site.Url), out discussionBoardTranslationOptionsHide))
                {
                    discussionBoardTranslationOptionsHide = true;
                }
            }

            HttpContext.Current.Cache.Remove("AlphamosaikReplaceLinkedPagesUrl");
            HttpContext.Current.Cache.Add("AlphamosaikReplaceLinkedPagesUrl", replaceLinkedPagesUrl, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikWebPartContentTranslation");
            HttpContext.Current.Cache.Add("AlphamosaikWebPartContentTranslation", webPartContentTranslation, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikBannerCSSClass");
            HttpContext.Current.Cache.Add("AlphamosaikBannerCSSClass", bannerCssClass, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikBannerPipe");
            HttpContext.Current.Cache.Add("AlphamosaikBannerPipe", bannerPipe, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikDefaultLangDeactivation");
            HttpContext.Current.Cache.Add("AlphamosaikDefaultLangDeactivation", defaultLangDeactivation, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikItemFiltering");
            HttpContext.Current.Cache.Add("AlphamosaikItemFiltering", itemFiltering, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikDisplayItemDashboard");
            HttpContext.Current.Cache.Add("AlphamosaikDisplayItemDashboard", displayItemDashboard, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikMessageForAutotranslateText");
            HttpContext.Current.Cache.Add("AlphamosaikMessageForAutotranslateText", messageForAutotranslateText, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikBingApplicationId");
            HttpContext.Current.Cache.Add("AlphamosaikBingApplicationId", bingApplicationId, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikExtractor");
            HttpContext.Current.Cache.Add("AlphamosaikExtractor", extractor, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikCompletingMode");
            HttpContext.Current.Cache.Add("AlphamosaikCompletingMode", completingMode, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikRedirectToLinkedPage");
            HttpContext.Current.Cache.Add("AlphamosaikRedirectToLinkedPage", redirectToLinkedPage, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikActivatedLog");
            HttpContext.Current.Cache.Add("AlphamosaikActivatedLog", activatedLog, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikDeactivateEventHandlerOnList");
            HttpContext.Current.Cache.Add("AlphamosaikDeactivateEventHandlerOnList", deactivateEventHandlerOnList, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikWildcardsFeature");
            HttpContext.Current.Cache.Add("AlphamosaikWildcardsFeature", wildcardsFeature, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikFilteringButton");
            HttpContext.Current.Cache.Add("AlphamosaikFilteringButton", filteringButton, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikQuickLaunchFilter");
            HttpContext.Current.Cache.Add("AlphamosaikQuickLaunchFilter", quickLaunchFilter, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikListFiteringDisplay");
            HttpContext.Current.Cache.Add("AlphamosaikListFiteringDisplay", listFiteringDisplay, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikResxFilesUpdate");
            HttpContext.Current.Cache.Add("AlphamosaikResxFilesUpdate", resxFilesUpdate, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikBannerWithNoTranslation");
            HttpContext.Current.Cache.Add("AlphamosaikBannerWithNoTranslation", bannerWithNoTranslation, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikLanguageFieldLabel");
            HttpContext.Current.Cache.Add("AlphamosaikLanguageFieldLabel", languageFieldLabel, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikUpgradeFromVersion2007");
            HttpContext.Current.Cache.Add("AlphamosaikUpgradeFromVersion2007", upgradeFromVersion2007, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikTopNavigationBarFilter");
            HttpContext.Current.Cache.Add("AlphamosaikTopNavigationBarFilter", topNavigationBarFilter, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikDictionaryAccessButton");
            HttpContext.Current.Cache.Add("AlphamosaikDictionaryAccessButton", dictionaryAccessButton, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            HttpContext.Current.Cache.Remove("AlphamosaikDiscussionBoardTranslationOptionsHide");
            HttpContext.Current.Cache.Add("AlphamosaikDiscussionBoardTranslationOptionsHide", discussionBoardTranslationOptionsHide, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
        }

        private static string GetLanguagesFromLanguageVisibilityList(string url, List<LanguageItem> visibleLanguages)
        {
            using (var site = new SPSite(url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = web.Lists.TryGetList("LanguagesVisibility");

                    if (list != null)
                    {
                        string q = string.Format("<Where><Eq><FieldRef Name=\"DefaultLanguage\"/><Value Type=\"Boolean\">{0}</Value></Eq></Where>", "1");

                        var sharepointQuery = new SPQuery
                        {
                            Query = q,
                        };

                        SPListItemCollection items = list.GetItems(sharepointQuery);
                        SPListItem varItem = null;

                        if (items.Count > 0)
                        {
                            varItem = items[0];
                        }

                        if (varItem != null)
                        {
                            string defaultLanguage = varItem["LanguageCode"].ToString();

                            SPListItemCollection varLangVisibilityItemCollection = list.Items;

                            foreach (SPListItem item in varLangVisibilityItemCollection)
                            {
                                bool visible = Convert.ToBoolean(item["IsVisible"]);

                                if (visible)
                                {
                                    string languageDestination = item["LanguageCode"].ToString();

                                    var languageItem = new LanguageItem(languageDestination, Languages.Instance.GetLcid(languageDestination),
                                                                        Convert.ToString(item["LanguagesDisplay"]),
                                                                        item.Fields.ContainsField("LanguagesPicture")
                                                                            ? Convert.ToString(
                                                                                item["LanguagesPicture"])
                                                                            : string.Empty, true);
                                    visibleLanguages.Add(languageItem);
                                }
                            }

                            return defaultLanguage;
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Add Pages To be Translated from the SP list PagesTranslations To the Cache
        /// </summary>
        /// <param name="url">Url of the site</param>
        private static void AddPagesNotToTranslateToCache(string url)
        {
            var arrPagesNotToTranslate = new ArrayList();

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                var objParent = new HelperParent(url, "/", "PagesTranslations");

                SPListItemCollection itemCollection = objParent.GetItemCollection();

                foreach (SPListItem item in itemCollection)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(item["Title"])))
                    {
                        if (!Convert.ToBoolean(item["ToTranslate"]))
                            arrPagesNotToTranslate.Add(item["Title"].ToString());
                    }
                }
            });

            HttpContext.Current.Cache.Add("PagesNotToTranslate", arrPagesNotToTranslate, null, Cache.NoAbsoluteExpiration,
                                            Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// Add Pages Not To be Translated from the SP list PagesTranslations To the Cache
        /// </summary>
        /// <param name="url">url of the site</param>
        private static void AddPagesToTranslateToCache(string url)
        {
            var arrPagesToTranslate = new ArrayList();

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                var objParent = new HelperParent(url, "/", "PagesTranslations");

                SPListItemCollection itemCollection = objParent.GetItemCollection();

                foreach (SPListItem item in itemCollection)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(item["Title"])))
                    {
                        if (Convert.ToBoolean(item["ToTranslate"]))
                            arrPagesToTranslate.Add(item["Title"].ToString());
                    }
                }
            });

            HttpContext.Current.Cache.Add("PagesToTranslate", arrPagesToTranslate, null, Cache.NoAbsoluteExpiration,
                                            Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
        }

        private static void InsertMenuForWebpartId(StringBuilder tempResponse, WebPartMenuToDisplayHelper webPartMenuToDisplayHelper, int webPartIdPosition, ref string originalMenu)
        {
            int msoZoneCellPosition = tempResponse.LastIndexOf("id=\"MSOZoneCell_", webPartIdPosition);

            if (msoZoneCellPosition != -1)
            {
                const string ClickOpenWebpartMenu = "onclick=\"OpenWebPartMenu(";

                int openWebPartMenuIndex = tempResponse.IndexOf(ClickOpenWebpartMenu, msoZoneCellPosition);

                if (openWebPartMenuIndex > msoZoneCellPosition && openWebPartMenuIndex < webPartIdPosition)
                {
                    OpenMenuParemeters openMenuParameters = GetOpenMenuParemeters(openWebPartMenuIndex, tempResponse);

                    int webpartMenuDiv = tempResponse.IndexOf("<div class=\"ms-WPMenuDiv\"", openWebPartMenuIndex);

                    if (webpartMenuDiv != -1 && webpartMenuDiv < webPartIdPosition)
                    {
                        const string ServerMenu = "class=\"ms-SrvMenuUI\">";
                        const string MenuId = "<menu id=\"";
                        int menuId = tempResponse.IndexOf(MenuId, openWebPartMenuIndex);

                        if (menuId == -1)
                        {
                            if (string.IsNullOrEmpty(originalMenu))
                            {
                                // Find the original menu
                                originalMenu = GetOriginalMenu(tempResponse, originalMenu, openMenuParameters.MenuName);
                            }

                            // Section menu id did not exist create new one
                            string menuIdSection = Environment.NewLine + MenuId + openMenuParameters.MenuName + "\" class=\"ms-SrvMenuUI\">" + Environment.NewLine + originalMenu + Environment.NewLine + "</menu>" + Environment.NewLine;
                            tempResponse.Insert(webpartMenuDiv, menuIdSection);

                            // Update index after inserting new text
                            webPartIdPosition = tempResponse.IndexOf(" WebPartID=\"" + webPartMenuToDisplayHelper.StorageKey);

                            string menuToReplace = tempResponse.Substring(openWebPartMenuIndex, webPartIdPosition - openWebPartMenuIndex);

                            string webpartNewMenuName = "MSOMenu_WebPartMenu_" + openMenuParameters.WebpartName;
                            menuToReplace = menuToReplace.Replace(openMenuParameters.MenuName, webpartNewMenuName);
                            tempResponse.Remove(openWebPartMenuIndex, webPartIdPosition - openWebPartMenuIndex);
                            tempResponse.Insert(openWebPartMenuIndex, menuToReplace);

                            menuId = tempResponse.IndexOf(MenuId, openWebPartMenuIndex);
                        }
                        else
                        {
                            string originalWebpartMenuName = GetWebPartMenuNameFromOpenMenu(openWebPartMenuIndex + ClickOpenWebpartMenu.Length, tempResponse);

                            int startIndex = menuId + MenuId.Length;
                            int indexOfLastQuote = tempResponse.IndexOf("\"", startIndex);

                            string currentMenuIdName = tempResponse.Substring(startIndex, indexOfLastQuote - startIndex);

                            if (string.IsNullOrEmpty(originalMenu))
                            {
                                // Find the original menu
                                originalMenu = GetOriginalMenu(tempResponse, originalMenu, originalWebpartMenuName);
                            }

                            if (openMenuParameters.MergeMenu && originalWebpartMenuName != currentMenuIdName)
                            {
                                string menuIdSection = Environment.NewLine + "<menu id=\"" + openMenuParameters.MenuName + "\" class=\"ms-SrvMenuUI\">" + Environment.NewLine + originalMenu + Environment.NewLine + "</menu>" + Environment.NewLine;
                                tempResponse.Insert(menuId, menuIdSection);

                                // Update index after inserting new text
                                webPartIdPosition = tempResponse.IndexOf(" WebPartID=\"" + webPartMenuToDisplayHelper.StorageKey);

                                string menuToReplace = tempResponse.Substring(openWebPartMenuIndex, webPartIdPosition - openWebPartMenuIndex);

                                string webpartNewMenuName = "MSOMenu_WebPartMenu_" + openMenuParameters.WebpartName;
                                menuToReplace = menuToReplace.Replace(openMenuParameters.MenuName, webpartNewMenuName);
                                tempResponse.Remove(openWebPartMenuIndex, webPartIdPosition - openWebPartMenuIndex);
                                tempResponse.Insert(openWebPartMenuIndex, menuToReplace);

                                menuId = tempResponse.IndexOf(MenuId, openWebPartMenuIndex);
                            }
                        }

                        int positionToInsert = tempResponse.IndexOf(ServerMenu, menuId);

                        if (positionToInsert != -1)
                        {
                            StringBuilder menuToCreate = GetMenuToCreate(webPartMenuToDisplayHelper);
                            tempResponse.Insert(positionToInsert + ServerMenu.Length, Environment.NewLine + menuToCreate);
                        }
                    }
                }
            }
        }

        private static string GetOriginalMenu(StringBuilder tempResponse, string originalMenu, string originalWebpartMenuName)
        {
            int originalMenuId = tempResponse.IndexOf("<menu id=\"" + originalWebpartMenuName);

            if (originalMenuId != -1)
            {
                int endMenu = tempResponse.IndexOf("</menu>", originalMenuId);
                int startMenuItem = tempResponse.IndexOf("<ie:menuitem ", originalMenuId);

                if (endMenu != -1 && startMenuItem != -1 && startMenuItem > originalMenuId && startMenuItem < endMenu)
                {
                    originalMenu = tempResponse.Substring(startMenuItem, endMenu - startMenuItem);
                }
            }

            return originalMenu;
        }

        private static void AddMenuItemForList(StringBuilder tempResponse, Hashtable itemLanguageListHashTable, bool itemMenuPerformanceMode, SPListItemCollection currentItemsCollection, bool announcementList, string menuLinkWith, ArrayList groupLanguageListArrayList, string listId)
        {
            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
            {
                if (!itemMenuPerformanceMode)
                    menuLinkWith += "CAMOpt(LangMenu, \"" + languageItem.DisplayName +
                                    "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                                    "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=SetLanguage&SPS_SetLanguage=" +
                                    languageItem.LanguageDestination + "'\", GetLanguage(currentItemID,\"" + languageItem.LanguageDestination +
                                    "\",strImageCheckPath,null)); \n ";
                else
                    menuLinkWith += "CAMOpt(LangMenu, \"" + languageItem.DisplayName +
                                    "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                                    "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=SetLanguage&SPS_SetLanguage=" +
                                    languageItem.LanguageDestination + "'\"); \n ";
            }

            menuLinkWith += "CAMSep(LangMenu); \n ";

            if (!itemMenuPerformanceMode)
                menuLinkWith += "CAMOpt(LangMenu, \"" + "ALL" +
                                "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=SetLanguage&SPS_SetLanguage=" +
                                "ALL" + "'\", GetLanguage(currentItemID,\"" + "ALL" +
                                "\",strImageCheckPath,null)); \n ";
            else
                menuLinkWith += "CAMOpt(LangMenu, \"" + "ALL" +
                                "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=SetLanguage&SPS_SetLanguage=" +
                                "ALL" + "'\"); \n ";

            if (announcementList)
            {
                menuLinkWith = AddMenuForAnnouncementList(menuLinkWith, listId);
            }

            menuLinkWith += "CAMSep(m); \n ";

            menuLinkWith += "return false; \n } \n ";

            menuLinkWith = AddJavaScriptForGetGroupLanguage(itemMenuPerformanceMode, currentItemsCollection, menuLinkWith, groupLanguageListArrayList);

            if (itemMenuPerformanceMode)
            {
                menuLinkWith = AddJavaScriptForReadyToLinkAndIsLinkable(currentItemsCollection, menuLinkWith);
            }

            menuLinkWith = AddJavaScriptForGetLanguage(itemLanguageListHashTable, itemMenuPerformanceMode, menuLinkWith);
            menuLinkWith += " \n </script>";

            string body = TranslatorRegex.EndBodyRegex.Replace(tempResponse.ToString(), menuLinkWith + "\n\n</BODY>");

            tempResponse.Clear().Append(body);
        }

        private static string AddMenuItemUnfiltred()
        {
            string menuUnfiltered = "<ie:menuitem id=\"SPS_MenuUnfiltering\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";
            if (HttpContext.Current.Request.QueryString["SPS_Trans_Code_Pers"] != null)
            {
                if (HttpContext.Current.Request.QueryString["SPS_Trans_Code_Pers"] == "Unfiltering")
                {
                    menuUnfiltered += "onMenuClick=\"window.location.search = changeLocation(window.location.search,'SPS_Trans_Code_Pers','Filtering');\" ";
                    menuUnfiltered += "text=\"Enable Language Filtering on items\" description=\"Reactivate the language filtering on items, displaying only items for the language.\" menuGroupId=\"100\">";
                }
                else
                    if (HttpContext.Current.Request.QueryString["SPS_Trans_Code_Pers"] == "Filtering")
                    {
                        menuUnfiltered += "onMenuClick=\"window.location.search = changeLocation(window.location.search,'SPS_Trans_Code_Pers','Unfiltering');\" ";
                        menuUnfiltered += "text=\"Disable Language Filtering on items\" description=\"Deactivate the language filtering on items, displaying all items of the page.\" menuGroupId=\"100\">";
                    }
                    else
                    {
                        menuUnfiltered += "onMenuClick=\"window.location.search = changeLocation(window.location.search,'SPS_Trans_Code_Pers','Unfiltering');\" ";
                        menuUnfiltered += "text=\"Disable Language Filtering on items\" description=\"Deactivate the language filtering on items, displaying all items of the page.\" menuGroupId=\"100\">";
                    }
            }
            else
            {
                menuUnfiltered += "onMenuClick=\"window.location.search = changeLocation(window.location.search,'SPS_Trans_Code_Pers','Unfiltering');\" ";
                menuUnfiltered += "text=\"Disable Language Filtering on items\" description=\"Deactivate the language filtering on items, displaying all items of the page.\" menuGroupId=\"100\">";
            }

            menuUnfiltered += "</ie:menuitem>" + Environment.NewLine;
            return menuUnfiltered;
        }

        private static void AddMenuEnableItemTradFromList(StringBuilder tempResponse, string listId, string listIdForEnableTradFromList, bool mustUpgrade)
        {
            if (!string.IsNullOrEmpty(listIdForEnableTradFromList))
            {
                string valueRegex = TranslatorRegex.EnableItemTradFromListRegex.Match(tempResponse.ToString()).Value;

                string menuUpgradeItemTradFromList = string.Empty;

                string enableItemTradFromListAction;
                string menuEnableItemTradFromList = "<ie:menuitem id=\"SPS_MenuEnableItemTradFromList\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";

                if (!string.IsNullOrEmpty(listId))
                {
                    if (mustUpgrade)
                    {
                        string upgradeItemTradFromListAction = @"javascript:window.location.search = '?listForItemLanguageId=" + listIdForEnableTradFromList + "&SPS_Trans_Code=EnableItemTrad';";
                        menuUpgradeItemTradFromList = "<ie:menuitem id=\"SPS_MenuUpgradeItemTradFromList\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";
                        menuUpgradeItemTradFromList += "onMenuClick=\"" + upgradeItemTradFromListAction + "\" ";
                        menuUpgradeItemTradFromList += "text=\"Upgrade Item Language on this list\" description=\"Update/create the language fields for items filtering and linking.\" menuGroupId=\"100\">";
                        menuUpgradeItemTradFromList += "</ie:menuitem>" + Environment.NewLine;
                    }

                    enableItemTradFromListAction = @"javascript: if(confirm('Are you sure you want to permanently delete the items functionality for this List ?')) window.location.search = '?listForItemLanguageId=" + listIdForEnableTradFromList + "&SPS_Trans_Code=DisableItemTrad';";

                    menuEnableItemTradFromList += "onMenuClick=\"" + enableItemTradFromListAction + "\" ";
                    menuEnableItemTradFromList += "text=\"Disable Item Language on this list\" description=\"Delete the language fields and all their content.\" menuGroupId=\"100\">";
                }
                else
                {
                    enableItemTradFromListAction = @"javascript:window.location.search = '?listForItemLanguageId=" + listIdForEnableTradFromList + "&SPS_Trans_Code=EnableItemTrad';";
                    menuEnableItemTradFromList += "onMenuClick=\"" + enableItemTradFromListAction + "\" ";
                    menuEnableItemTradFromList += "text=\"Enable Item Language on this list\" description=\"Create the language fields for items filtering and linking.\" menuGroupId=\"100\">";
                }

                menuEnableItemTradFromList += "</ie:menuitem>" + Environment.NewLine;

                if (!string.IsNullOrEmpty(valueRegex))
                    tempResponse.Replace(valueRegex, valueRegex + menuUpgradeItemTradFromList + menuEnableItemTradFromList);
            }
        }

        private static string AddJavaScriptForGetLanguage(Hashtable itemLanguageListHashTable, bool itemMenuPerformanceMode, string menuLinkWith)
        {
            menuLinkWith += "function HasArrayValue(array,value)\n { \n var i; \n for (var i = 0, loopCnt = array.length; i < loopCnt; i++) \n { \n if (array[i] === value) { \n  return true;\n }\n }\n return false;\n }\n";

            menuLinkWith +=
                "function GetLanguage(id,lang,result1,result2)\n {\n var listId=[\"-1\"];\n switch(lang)\n { \n ";

            if (((ArrayList)itemLanguageListHashTable["(SPS_LNG_ALL)"]).Count > 0)
            {
                menuLinkWith += "case \"" + "ALL" + "\": \n listId=[";

                foreach (string itemId in (ArrayList)itemLanguageListHashTable["(SPS_LNG_ALL)"])
                    menuLinkWith += "\"" + itemId + "\",";

                menuLinkWith += "\"-1\"]; \n ";

                menuLinkWith += "if (HasArrayValue(listId,id)) return result1; \n else return result2; \n break; \n ";
            }
            else
            {
                menuLinkWith += "case \"" + "ALL" + "\": \n listId=[";

                menuLinkWith += "\"-1\"]; \n ";

                menuLinkWith += "if (HasArrayValue(listId,id)) return result1; \n else return result2; \n break; \n ";
            }

            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
            {
                if (((ArrayList)itemLanguageListHashTable["SPS_LNG_" + languageItem.LanguageDestination]).Count > 0)
                {
                    menuLinkWith += "case \"" + languageItem.LanguageDestination + "\": \n listId=[";

                    foreach (string itemId in (ArrayList)itemLanguageListHashTable["SPS_LNG_" + languageItem.LanguageDestination])
                        menuLinkWith += "\"" + itemId + "\",";

                    menuLinkWith += "\"-1\"]; \n ";

                    menuLinkWith += "if (HasArrayValue(listId,id)) return result1; \n else return result2; \n break; \n ";
                }
            }

            if (!itemMenuPerformanceMode)
                menuLinkWith += "default : return null; \n";
            else
                menuLinkWith += "default : return \"false\"; \n";

            menuLinkWith += "}\n}\n";
            return menuLinkWith;
        }

        private static string AddJavaScriptForGetGroupLanguage(bool itemMenuPerformanceMode, SPListItemCollection currentItemsCollection, string menuLinkWith, ArrayList groupLanguageListArrayList)
        {
            if (!itemMenuPerformanceMode)
            {
                menuLinkWith += "function GetGroupLanguage(id)\n {\n switch(id)\n { \n ";

                foreach (string[] currentPair in groupLanguageListArrayList)
                {
                    if (currentPair[1] != "-1")
                        menuLinkWith += "case \"" + currentPair[0] + "\": \n return \"" + currentPair[1] +
                                        "\"; \n break; \n ";
                }

                menuLinkWith += "default : return \"-1\"; \n";

                menuLinkWith += "}\n}\n";
            }
            else
            {
                menuLinkWith += "function GetGroupLanguage(id, type)\n {\n switch(id)\n { \n ";

                if (currentItemsCollection.Count == 0)
                {
                    menuLinkWith += "case \"-1\": \n ";
                    menuLinkWith += "if (type == 1)  {return \"false\"} else {return \"true\"}; \n ";
                    menuLinkWith += "break; \n ";
                }
                else
                {
                    foreach (string[] currentPair in groupLanguageListArrayList)
                    {
                        if (currentPair[1] != "-1")
                            menuLinkWith += "case \"" + currentPair[0] + "\": \n  if (type == 1)  {return \"" +
                                            "false" + "\"} else {return \"" + "true" + "\"}; \n break; \n ";
                    }
                }

                menuLinkWith += "default : if (type == 1) { return \"true\";} else { return \"false\";} \n";

                menuLinkWith += "}\n}\n";
            }

            return menuLinkWith;
        }

        private static string AddJavaScriptForReadyToLinkAndIsLinkable(SPListItemCollection currentItemsCollection, string menuLinkWith)
        {
            menuLinkWith += "function ReadyToLink(id)\n {\n ";
            if (currentItemsCollection.Count == 0)
            {
                menuLinkWith += "return \"false\"; \n";
            }
            else
            {
                menuLinkWith +=
                    "if ((IsLinkable(id, \"true\", \"false\") == \"true\") && (GetLanguage(id,\"ALL\",\"false\",\"true\")) == \"true\")\n";
                menuLinkWith += "{ return \"true\";} else { return \"false\";} \n";
            }

            menuLinkWith += "}\n\n";

            menuLinkWith += "function IsLinkable(id, result1, result2)\n {\n var listId=[\"-1\"];\n ";
            menuLinkWith += "listId=[";

            foreach (SPListItem currentItem in currentItemsCollection)
            {
                menuLinkWith += "\"" + currentItem.ID + "\",";
            }

            menuLinkWith += "\"-1\"]; \n ";
            menuLinkWith += "if (HasArrayValue(listId,id)) return result1; \n else return result2; \n\n ";
            menuLinkWith += "}\n\n";
            return menuLinkWith;
        }

        private static string AddMenuForAnnouncementList(string menuLinkWith, string listId)
        {
            const string TextCreateItem = "Copy the item in a language...";
            const string TextChoiceTranslation = "With content auto-translation";
            const string TextChoiceNoTranslation = "Without auto-translation";
            menuLinkWith += "var CreateItemMenu = CASubM(m, \"" + TextCreateItem + "\", \"\"); \n ";
            menuLinkWith += "CreateItemMenu.setAttribute(\"enabled\", GetLanguage(currentItemID,\"ALL\",\"false\",\"true\")); \n ";

            menuLinkWith += "var AutoTranslationMenu = CASubM(CreateItemMenu, \"" + TextChoiceTranslation + "\", \"\"); \n ";
            menuLinkWith += "var NoTranslationMenu = CASubM(CreateItemMenu, \"" + TextChoiceNoTranslation + "\", \"\"); \n ";

            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
            {
                menuLinkWith += "CAMOpt(AutoTranslationMenu, \"" + languageItem.DisplayName +
                                "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=CreateClonedMultilingualItem&SPS_NewItemLang=" +
                                languageItem.LanguageDestination + "&SPS_AutoTranslation=true'\"); \n ";
                menuLinkWith += "CAMOpt(NoTranslationMenu, \"" + languageItem.DisplayName +
                                "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                                "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=CreateClonedMultilingualItem&SPS_NewItemLang=" +
                                languageItem.LanguageDestination + "&SPS_AutoTranslation=false'\"); \n ";
            }

            menuLinkWith += "CAMSep(CreateItemMenu); \n ";

            menuLinkWith += "CAMOpt(AutoTranslationMenu, \"" + "All languages" +
                            "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                            "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=CreateClonedMultilingualItem&SPS_NewItemLang=" +
                            "ALL" + "&SPS_AutoTranslation=true'\"); \n ";
            menuLinkWith += "CAMOpt(NoTranslationMenu, \"" + "All languages" +
                            "\", \"javascript:window.location.search = '?SPS_ListID=" + listId +
                            "&SPS_ItemID='+currentItemID+'&SPS_Trans_Code=CreateClonedMultilingualItem&SPS_NewItemLang=" +
                            "ALL" + "&SPS_AutoTranslation=false'\"); \n ";
            return menuLinkWith;
        }

        private static void AddMenuDictionaryAccess(StringBuilder tempResponse)
        {
            if (SPContext.Current != null)
            {
                string siteCollectionRootUrl = SPContext.Current.Site.Url;

                string[] sections = siteCollectionRootUrl.Split('/');

                if (sections.Length > 2)
                {
                    siteCollectionRootUrl = sections[0] + "//" + sections[2];
                }

                string menuDictionaryAccess = "<ie:menuitem id=\"SPS_MenuDictionaryAccess\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";
                menuDictionaryAccess += "onMenuClick=\"window.location.href='" + siteCollectionRootUrl + "/Lists/TranslationContents/CustomizedPhrasesGridView.aspx'" + ";\" ";
                menuDictionaryAccess += "text=\"Access Global Dictionary\" description=\"Access Global Dictionary, to edit, add or remove translations.\" >";

                menuDictionaryAccess += "</ie:menuitem>" + Environment.NewLine;

                int menuDictionaryAccessIndex =
                    tempResponse.IndexOf(" onMenuClick=\"STSNavigate2(event,'" + "/_layouts/settings.aspx');\"");

                if (menuDictionaryAccessIndex == -1)
                    if (TranslatorRegex.AutoCompletingModeButtonRegex.IsMatch(tempResponse.ToString()))
                    {
                        menuDictionaryAccessIndex = TranslatorRegex.AutoCompletingModeButtonRegex.Match(tempResponse.ToString()).Index;
                    }

                if (menuDictionaryAccessIndex > -1)
                {
                    menuDictionaryAccessIndex = tempResponse.LastIndexOf("<ie:menuitem ", menuDictionaryAccessIndex);
                    if (menuDictionaryAccessIndex > -1)
                    {
                        tempResponse.Insert(menuDictionaryAccessIndex, menuDictionaryAccess);
                    }
                }
            }
        }

        private static StringBuilder GetMenuToCreate(WebPartMenuToDisplayHelper webPartMenuToDisplayHelper)
        {
            var menuToCreate = new StringBuilder();
            var languageMenu = new StringBuilder();

            foreach (LanguageItem languageItem in Dictionaries.Instance.Languages)
            {
                if (webPartMenuToDisplayHelper.Language.Contains(languageItem.LanguageDestination))
                {
                    languageMenu.Append(
                        "<ie:menuitem  type=\"option\"  Disabled=\"true\"  iconSrc=\"/_layouts/images/CHKMRK.GIF\"  onMenuClick=\"javascript:window.location.search ='?SPS_MenuWebPartID='+MenuWebPartID+'&SPS_Trans_Code=Switch" +
                        languageItem.LanguageDestination + "';\"  >Only display in " + languageItem.DisplayName + "</ie:menuitem>" + Environment.NewLine);
                }
                else
                {
                    if (languageItem.Visible)
                    {
                        languageMenu.Append("<ie:menuitem  type=\"option\"  onMenuClick=\"javascript:window.location.search ='?SPS_MenuWebPartID='+MenuWebPartID+'&SPS_Trans_Code=Switch" + languageItem.LanguageDestination + "';\"  >Only display in " + languageItem.DisplayName + "</ie:menuitem>" + Environment.NewLine);
                    }
                }
            }

            // string switchALL = String.Empty;
            const string SwitchAll = @"javascript:window.location.search = '?SPS_MenuWebPartID='+MenuWebPartID+'&SPS_Trans_Code=SwitchALL';";

            if (webPartMenuToDisplayHelper.Language.Contains("ALL"))
            {
                menuToCreate.Append("<ie:menuitem  type=\"option\"  Disabled=\"true\"  iconSrc=\"/_layouts/images/CHKMRK.GIF\"  onMenuClick=\"" + SwitchAll + "\"  >Display in every language</ie:menuitem>" + Environment.NewLine + languageMenu);
            }
            else
            {
                menuToCreate.Append("<ie:menuitem  type=\"option\"  onMenuClick=\"" + SwitchAll + "\"  >Display in every language</ie:menuitem>" + Environment.NewLine + languageMenu);
            }

            const string AddItemTranslation = @"javascript:window.location.search = '?SPS_MenuWebPartID='+MenuWebPartID+'&SPS_Trans_Code=EnableItemTrad';";

            const string RemoveItemTranslation = @"javascript: if(confirm('Are you sure you want to permanently delete the items functionality for this WebPart and for the linked list ? \n All language content will be deleted')) 
                    window.location.search = '?SPS_MenuWebPartID='+MenuWebPartID+'&SPS_Trans_Code=DisableItemTrad';";

            if (webPartMenuToDisplayHelper.ItemLanguageEnabled)
            {
                menuToCreate.Append("<ie:menuitem type=\"separator\"></ie:menuitem>" + Environment.NewLine + "<ie:menuitem  type=\"option\"  Disabled=\"true\"  iconSrc=\"/_layouts/images/CHKMRK.GIF\"  onMenuClick=\"" + AddItemTranslation + "\"  >Enable item language</ie:menuitem>" + Environment.NewLine + "<ie:menuitem  type=\"option\"  onMenuClick=\"" + RemoveItemTranslation + "\"  >Disable item language</ie:menuitem>" + Environment.NewLine + "<ie:menuitem type=\"separator\"></ie:menuitem>" + Environment.NewLine);
            }
            else
            {
                menuToCreate.Append("<ie:menuitem type=\"separator\"></ie:menuitem>" + Environment.NewLine + "<ie:menuitem  type=\"option\"  onMenuClick=\"" + AddItemTranslation + "\"  >Enable item language</ie:menuitem>" + Environment.NewLine + "<ie:menuitem  type=\"option\"  Disabled=\"true\"  iconSrc=\"/_layouts/images/CHKMRK.GIF\" onMenuClick=\"" + RemoveItemTranslation + "\"  >Disable item language</ie:menuitem>" + Environment.NewLine + "<ie:menuitem type=\"separator\"></ie:menuitem>" + Environment.NewLine);
            }

            const string EnableWebpartContentTrad = @"javascript:window.location.search = '?SPS_MenuWebPartID='+MenuWebPartID+'&SPS_Trans_Code=EnableWebpartContentTrad';";
            const string DisableWebpartContentTrad = @"javascript:window.location.search = '?SPS_MenuWebPartID='+MenuWebPartID+'&SPS_Trans_Code=DisableWebpartContentTrad';";

            if (webPartMenuToDisplayHelper.ContentWebpartEnabled)
            {
                menuToCreate.Append("<ie:menuitem  type=\"option\" Disabled=\"true\"  iconSrc=\"/_layouts/images/CHKMRK.GIF\"   onMenuClick=\"" + EnableWebpartContentTrad + "\"  >Enable content translation</ie:menuitem>" + Environment.NewLine + "<ie:menuitem  type=\"option\" onMenuClick=\"" + DisableWebpartContentTrad + "\"  >Disable content translation</ie:menuitem>" + Environment.NewLine + "<ie:menuitem type=\"separator\"></ie:menuitem>" + Environment.NewLine);
            }
            else
            {
                menuToCreate.Append("<ie:menuitem  type=\"option\"  onMenuClick=\"" + EnableWebpartContentTrad + "\"  >Enable content translation</ie:menuitem>" + Environment.NewLine + "<ie:menuitem  type=\"option\" Disabled=\"true\"  iconSrc=\"/_layouts/images/CHKMRK.GIF\"  onMenuClick=\"" + DisableWebpartContentTrad + "\"  >Disable content translation</ie:menuitem>" + Environment.NewLine + "<ie:menuitem type=\"separator\"></ie:menuitem>" + Environment.NewLine);
            }

            return menuToCreate;
        }

        private static IEnumerable<WebPartMenuToDisplayHelper> GetWebPartMenuToDisplay(SPWeb web)
        {
            var menusToDisplay = new List<WebPartMenuToDisplayHelper>();

            try
            {
                using (SPLimitedWebPartManager manager = web.GetLimitedWebPartManager(HttpContext.Current.Request.RawUrl, System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    try
                    {
                        foreach (System.Web.UI.WebControls.WebParts.WebPart current in manager.WebParts)
                        {
                            try
                            {
                                string webpartStorageKey = manager.GetStorageKey(current).ToString();
                                string propertiesValue = string.Empty;
                                string language = string.Empty;
                                SPList list = null;

                                if (HasWebPartFunctionnality(web, webpartStorageKey))
                                {
                                    propertiesValue = GetWebPartFunctionnality(web, webpartStorageKey);
                                }

                                try
                                {
                                    var currentListView = current as XsltListViewWebPart;

                                    if (currentListView != null)
                                    {
                                        string listName = currentListView.ListName;

                                        foreach (SPList currentCList in web.Lists)
                                        {
                                            if (currentCList.ID.ToString("B").ToUpper() == listName)
                                                list = currentCList;
                                        }

                                        bool hasTheFieldInCurrentView = false;

                                        if (list != null)
                                        {
                                            hasTheFieldInCurrentView =
                                                list.GetView(new Guid(currentListView.ViewGuid)).ViewFields.Exists(
                                                    "SharePoint_Item_Language");
                                        }

                                        if (HasWebPartFunctionnality(web, webpartStorageKey))
                                        {
                                            if (hasTheFieldInCurrentView)
                                            {
                                                if (!propertiesValue.Contains("_Item_Langage_Enabled"))
                                                {
                                                    web.AllProperties["Alphamosaik.Translator.WebParts " + webpartStorageKey] += "_Item_Langage_Enabled";
                                                    web.AllowUnsafeUpdates = true;
                                                    web.Update();
                                                    web.AllowUnsafeUpdates = false;
                                                }
                                            }
                                            else
                                            {
                                                if (propertiesValue.Contains("_Item_Langage_Enabled"))
                                                {
                                                    web.AllProperties["Alphamosaik.Translator.WebParts " + webpartStorageKey] =
                                                        ((string)
                                                         web.AllProperties["Alphamosaik.Translator.WebParts " + webpartStorageKey]).Replace("_Item_Langage_Enabled", string.Empty);
                                                    web.AllowUnsafeUpdates = true;
                                                    web.Update();
                                                    web.AllowUnsafeUpdates = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (hasTheFieldInCurrentView)
                                            {
                                                web.AllProperties.Add("Alphamosaik.Translator.WebParts " + webpartStorageKey, "_Item_Langage_Enabled");
                                                web.AllowUnsafeUpdates = true;
                                                web.Update();
                                                web.AllowUnsafeUpdates = false;
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utilities.TraceNormalCaughtException("GetWebPartMenuToDisplay", e);
                                }

                                if (HasWebPartFunctionnality(web, webpartStorageKey))
                                {
                                    propertiesValue = GetWebPartFunctionnality(web, webpartStorageKey);
                                }

                                if (!propertiesValue.Contains("_SPS_WEBPART_"))
                                {
                                    language = "ALL";
                                }
                                else
                                {
                                    foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                                    {
                                        if (propertiesValue.Contains("_SPS_WEBPART_" + languageItem.LanguageDestination))
                                        {
                                            language = languageItem.LanguageDestination;
                                            break;
                                        }
                                    }
                                }

                                bool webpartContentEnabled = propertiesValue.Contains("_SPS_CONTENT_WEBPART_ON") || (bool)HttpContext.Current.Cache["AlphamosaikWebPartContentTranslation"];

                                if (propertiesValue.Contains("_SPS_CONTENT_WEBPART_OFF"))
                                {
                                    webpartContentEnabled = false;
                                }

                                var webPartMenuToDisplayHelper = new WebPartMenuToDisplayHelper(propertiesValue.Contains("_Item_Langage_Enabled"),
                                    webpartContentEnabled, language, manager.GetStorageKey(current));

                                menusToDisplay.Add(webPartMenuToDisplayHelper);
                            }
                            catch (WebPartPageUserException webPartPageUserException)
                            {
                                Utilities.TraceNormalCaughtException("GetWebPartMenuToDisplay", webPartPageUserException);
                            }
                            catch (Exception e)
                            {
                                Utilities.LogException("GetWebPartMenuToDisplay", e, EventLogEntryType.Warning);
                            }
                        }
                    }
                    finally
                    {
                        manager.Web.Dispose();
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                Utilities.TraceNormalCaughtException("GetWebPartMenuToDisplay", fileNotFoundException);
            }
            catch (SPException ex)
            {
                Utilities.TraceNormalCaughtException("GetWebPartMenuToDisplay", ex);
            }

            return menusToDisplay;
        }

        private static OpenMenuParemeters GetOpenMenuParemeters(int openWebPartMenuIndex, StringBuilder tempResponse)
        {
            OpenMenuParemeters openMenuparameters = null;

            int closingOpenMenuFunction = tempResponse.IndexOf(");", openWebPartMenuIndex);

            if (closingOpenMenuFunction != -1)
            {
                string openMenuFunction = tempResponse.Substring(openWebPartMenuIndex, closingOpenMenuFunction - openWebPartMenuIndex);

                var separators = new[] { ',', '(' };

                string[] parameters = openMenuFunction.Split(separators);

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = parameters[i].Replace("&#39;", string.Empty);
                    parameters[i] = parameters[i].Replace("'", string.Empty);
                    parameters[i] = parameters[i].Trim();
                }

                openMenuparameters = new OpenMenuParemeters(parameters[3], parameters[1], Convert.ToBoolean(parameters[4]));
            }

            return openMenuparameters;
        }

        private static string GetWebPartMenuNameFromOpenMenu(int openWebPartMenuIndex, StringBuilder tempResponse)
        {
            string webpartMenuName = string.Empty;

            int closingOpenMenuFunction = tempResponse.IndexOf(");", openWebPartMenuIndex);

            if (closingOpenMenuFunction != -1)
            {
                string openMenuFunction = tempResponse.Substring(openWebPartMenuIndex, closingOpenMenuFunction - openWebPartMenuIndex);

                var separators = new[] { ',' };

                string[] parameters = openMenuFunction.Split(separators);
                webpartMenuName = parameters[0];
                webpartMenuName = webpartMenuName.Replace("&#39;", string.Empty);
                webpartMenuName = webpartMenuName.Replace("'", string.Empty);
            }

            return webpartMenuName;
        }

        /// <summary>
        /// Ajoute les 4 hyperliens pointant vers les listes de configuration du module multilingue, sur les pages de settings des sites
        /// </summary>
        /// <param name="tempResponse">
        /// Source Html de la page
        /// </param>
        private static void AddHyperlinksInSiteSettings(StringBuilder tempResponse)
        {
            try
            {
                if (HttpContext.Current.Request.Path.ToLower().EndsWith("/_layouts/settings.aspx") && SPContext.Current.Web.UserIsWebAdmin)
                {
                    string siteCollectionRootUrl = SPContext.Current.Site.Url;

                    string[] sections = siteCollectionRootUrl.Split('/');

                    if (sections.Length > 2)
                    {
                        siteCollectionRootUrl = sections[0] + "//" + sections[2];
                    }

                    var category = new StringBuilder(Environment.NewLine + "<tr class=\"ms-linksection-level1\">" + Environment.NewLine);
                    category.AppendLine("<td valign=\"top\" style=\"width:60px; height:32px\">");
                    category.AppendLine("<img alt=\"\" src=\"/_layouts/images/logoMM.png\" style=\"border-width:0px;\" />");
                    category.AppendLine("</td>");
                    category.AppendLine("<td valign=\"top\">");
                    category.AppendLine("<h3>");
                    category.AppendLine("Oceanik");
                    category.AppendLine("</h3>");
                    category.AppendLine();
                    category.AppendLine("<ul>");

                    var linksToInsert = new StringBuilder("<li>" + Environment.NewLine);
                    linksToInsert.Append(
                        "<a id=\"ctl00_PlaceHolderMain_ctl01_SiteAdministration_RptControls_AlphamosaikTranslationContents\" title=\"Manage Alphamosaïk Translator dictionary list\" href=\"" + siteCollectionRootUrl + "/Lists/TranslationContents/CustomizedPhrasesGridView.aspx\">Dictionary</a>" + Environment.NewLine);
                    linksToInsert.Append("</li>" + Environment.NewLine + Environment.NewLine);

                    linksToInsert.Append("<li>" + Environment.NewLine);
                    linksToInsert.Append(
                        "<a id=\"ctl00_PlaceHolderMain_ctl01_SiteAdministration_RptControls_AlphamosaikLanguagesVisibility\" title=\"Manage Alphamosaïk Translator languages visibility list\" href=\"" + siteCollectionRootUrl + "/Lists/LanguagesVisibility/ClassicView.aspx\">Languages Visibility</a>" + Environment.NewLine);
                    linksToInsert.Append("</li>" + Environment.NewLine + Environment.NewLine);

                    linksToInsert.Append("<li>" + Environment.NewLine);
                    linksToInsert.Append(
                        "<a id=\"ctl00_PlaceHolderMain_ctl01_SiteAdministration_RptControls_AlphamosaikPagesTranslations\" title=\"Manage Alphamosaïk Translator pages translation list\" href=\"" + siteCollectionRootUrl + "/Lists/PagesTranslations/ClassicView.aspx\">Pages Translations</a>" + Environment.NewLine);
                    linksToInsert.Append("</li>" + Environment.NewLine + Environment.NewLine);

                    linksToInsert.Append("<li>" + Environment.NewLine);
                    linksToInsert.Append(
                        "<a id=\"ctl00_PlaceHolderMain_ctl01_SiteAdministration_RptControls_AlphamosaikConfigStore\" title=\"Manage Alphamosaïk Configuration Store list\" href=\"" + siteCollectionRootUrl + "/Lists/Configuration%20Store/Configuration%20Store.aspx\">Configuration Store</a>" + Environment.NewLine);
                    linksToInsert.Append("</li>" + Environment.NewLine + Environment.NewLine);

                    linksToInsert.Append("<li>" + Environment.NewLine);
                    linksToInsert.Append(
                        "<a id=\"ctl00_PlaceHolderMain_ctl01_SiteAdministration_RptControls_AlphamosaikTroubleshootingStore\" title=\"Manage Alphamosaïk Troubleshooting Store list\" href=\"" + siteCollectionRootUrl + "/Lists/Troubleshooting%20Store/AllItems.aspx\">Troubleshooting Store</a>" + Environment.NewLine);
                    linksToInsert.Append("</li>" + Environment.NewLine + Environment.NewLine);

                    category.AppendLine(linksToInsert.ToString());
                    category.AppendLine();
                    category.AppendLine("</ul>");
                    category.AppendLine();
                    category.AppendLine("</td>");
                    category.AppendLine("</tr>");

                    int linksToInsertIndex = tempResponse.IndexOf("<img alt=\"\" src=\"/_layouts/images/SiteSettings_SiteAdmin_48x48.png\"");
                    if (linksToInsertIndex > -1)
                    {
                        linksToInsertIndex = tempResponse.IndexOf("</tr>", linksToInsertIndex);
                        if (linksToInsertIndex > -1)
                        {
                            tempResponse.Insert(linksToInsertIndex + 5, category);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("AddHyperlinksInSiteSettings", e, EventLogEntryType.Warning);
            }
        }

        private static void FormatRibbonUrl(StringBuilder tempResponse)
        {
            if (SPContext.Current.List != null)
            {
                if (tempResponse.IndexOf("{var toolbarData = new Object();") > -1)
                {
                    int partToFormatBeginIndex = tempResponse.IndexOf("{var toolbarData = new Object();");

                    int partToFormatEndIndex = tempResponse.IndexOf("}]\";", partToFormatBeginIndex);
                    string partToFormat = tempResponse.Substring(
                        partToFormatBeginIndex, partToFormatEndIndex - partToFormatBeginIndex);
                    partToFormat = partToFormat.Replace("\\\\\\\\u002525", "\\\\\\\\u0025");
                    tempResponse.Remove(partToFormatBeginIndex, partToFormatEndIndex - partToFormatBeginIndex);
                    tempResponse.Insert(partToFormatBeginIndex, partToFormat);
                }
            }
        }

        private static void RemoveSpsTagsBeforeAddingMenus(StringBuilder tempResponse, bool viewAllItemsInEveryLanguages, bool editMode)
        {
            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                tempResponse.Replace("SPS_STATIC_LNG_MASTERPAGE_" + languageItem.LanguageDestination, string.Empty);

            if (!editMode)
            {
                tempResponse.Replace(";SharePoint_Item_Language&", ";SharePoint_ALPHA_TEMP_Item Language&");
                tempResponse.Replace(";(SPS_LNG_ALL)&", ";(SPS_ALPHA_TEMP_LNG_ALL)&");

                tempResponse.Replace("SharePoint_Item_Language:", string.Empty);

                if (tempResponse.IndexOf("TITLE_EDITOR\"", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    tempResponse.Replace("SharePoint_Item_Language", string.Empty);
                    tempResponse.Replace("(SPS_LNG_ALL)", string.Empty);
                }

                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                    tempResponse.Replace(";SPS_LNG_" + languageItem.LanguageDestination + "&", ";SPS_LNG_TEMP_ALPHA" + languageItem.LanguageDestination + "&");

                if (tempResponse.IndexOf("TITLE_EDITOR\"", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    if (!viewAllItemsInEveryLanguages)
                        foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                            tempResponse.Replace("SPS_LNG_" + languageItem.LanguageDestination, string.Empty);
                }

                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                    tempResponse.Replace(";SPS_LNG_TEMP_ALPHA" + languageItem.LanguageDestination + "&", ";SPS_LNG_" + languageItem.LanguageDestination + "&");

                tempResponse.Replace(";SharePoint_ALPHA_TEMP_Item Language&", ";SharePoint_Item_Language&");
                tempResponse.Replace(";(SPS_ALPHA_TEMP_LNG_ALL)&", ";(SPS_LNG_ALL)&");
            }
        }

        private static void RemoveSpsWebPartAfterAddingMenus(StringBuilder tempResponse)
        {
            if (tempResponse.IndexOf(":TITLE_EDITOR\"", StringComparison.OrdinalIgnoreCase) < 0)
            {
                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                    tempResponse.Replace("_SPS_WEBPART_" + languageItem.LanguageDestination, string.Empty);

                tempResponse.Replace("_Item_Langage_Enabled", string.Empty);
            }
        }

        private static void HideItemCounterForGroupByFilter(StringBuilder tempResponse)
        {
            if ((tempResponse.IndexOf("javascript:ExpCollGroup(") > -1) && (tempResponse.IndexOf("&#8206;(") > -1) && (tempResponse.IndexOf("SharePoint_Item_Language") > -1))
            {
                var counterRegex = new Regex("&#8206;\\([0-9]+\\)", RegexOptions.IgnoreCase);
                foreach (Match counter in counterRegex.Matches(tempResponse.ToString()))
                {
                    tempResponse.Replace(counter.Value, "&#8206;");
                }
            }
        }

        private static void LoadGlobalDictionaries(string url, string listName)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items["statisticsTime"] : null, "LoadWebApplicationDictionaries", 1))
            {
                string query = "<Where><Eq><FieldRef Name=\'isCustomize\'/><Value Type=\'Boolean\'>0</Value></Eq></Where>" + Alphamosaik.Common.SharePoint.Library.ListContentIterator.ItemEnumerationOrderById;
                var global = StandardDictionary.LoadDictionary("Global", url, listName, query);

                Dictionaries.Instance.RegisterDictionary(global);
            }
        }

        private static void LoadCustomDictionaries(string url, string listName, string defaultLanguage)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items["statisticsTime"] : null, "LoadWebApplicationDictionaries", 1))
            {
                string query = "<Where><Neq><FieldRef Name=\'isCustomize\'/><Value Type=\'Boolean\'>0</Value></Neq></Where>" + Alphamosaik.Common.SharePoint.Library.ListContentIterator.ItemEnumerationOrderById;
                var custom = StandardDictionary.LoadDictionary("Custom", url, listName, query);

                if (custom == null)
                {
                    return;
                }

                Dictionaries.Instance.RegisterDictionary(custom);

                LoadMultipleTranslationDictionaries(url, listName, defaultLanguage);

                BaseDictionary wildCard = null;

                if (HttpContext.Current != null)
                {
                    if ((bool) HttpContext.Current.Cache["AlphamosaikWildcardsFeature"])
                    {
                        query = "<Where><Contains><FieldRef Name=\'" + defaultLanguage +
                                "\'/><Value Type=\'Text\'>***</Value></Contains></Where>" +
                                Alphamosaik.Common.SharePoint.Library.ListContentIterator.ItemEnumerationOrderById;
                        wildCard = WildcardDictionary.LoadDictionary("Wildcard", url, listName, query);

                        if (wildCard != null)
                        {
                            Dictionaries.Instance.RegisterDictionary(wildCard);
                        }
                    }
                }

                // Connect Dictionaries
                if (Dictionaries.Instance.DictionaryExist(custom.WebApplicationId, custom.SiteId, custom.WebId, "Global"))
                {
                    custom.Connect(Dictionaries.Instance.GetDictionary(custom.WebApplicationId, custom.SiteId, custom.WebId, "Global"));

                    if (wildCard != null)
                    {
                        Dictionaries.Instance.GetDictionary(wildCard.WebApplicationId, wildCard.SiteId, wildCard.WebId, "Global").Connect(wildCard);
                    }
                }
                else if (wildCard != null)
                {
                    custom.Connect(wildCard);
                }

                if (!Dictionaries.Instance.DictionaryExist(custom.WebApplicationId, custom.SiteId, custom.WebId, "MultipleWordTranslationDictionaries"))
                {
                    string rootUrl = string.Empty;

                    if (HttpContext.Current != null)
                    {
                        rootUrl =
                            Alphamosaik.Common.SharePoint.Library.Utilities.GetAbsoluteUri(
                                HttpContext.Current.ApplicationInstance);
                        rootUrl = Alphamosaik.Common.SharePoint.Library.Utilities.FilterUrl(rootUrl); // root server url
                    }

                    Dictionaries.Instance.RegisterDictionaryAsRootForUrl(custom, rootUrl.IndexOf(url, StringComparison.OrdinalIgnoreCase) != -1);
                }

                if (listName.IndexOf("TranslationContentsSub") != -1)
                {
                    // Connect last dictionaries structures to the WebApplication Dictionary
                    if (wildCard != null)
                    {
                        wildCard.Connect(Dictionaries.Instance.GetWebApplicationDictionary(custom.WebApplicationId));
                    }
                    else
                    {
                        custom.Connect(Dictionaries.Instance.GetWebApplicationDictionary(custom.WebApplicationId));
                    }
                }
            }
        }

        private static void LoadMultipleTranslationDictionaries(string url, string listName, string defaultLanguage)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items["statisticsTime"] : null, "LoadMultipleTranslationDictionaries", 1))
            {
                string query = "<Where><Contains><FieldRef Name=\'" + defaultLanguage + "\'/><Value Type=\'Text\'>$$SPS_URL:</Value></Contains></Where>" + Alphamosaik.Common.SharePoint.Library.ListContentIterator.ItemEnumerationOrderById;
                var multipleWordTranslationDictionaries = MultipleWordTranslationDictionaries.LoadDictionary("MultipleWordTranslationDictionaries", url, listName, query);

                if (multipleWordTranslationDictionaries != null)
                {
                    Dictionaries.Instance.RegisterDictionary(multipleWordTranslationDictionaries);

                    multipleWordTranslationDictionaries.Connect(Dictionaries.Instance.GetDictionary(multipleWordTranslationDictionaries.WebApplicationId, multipleWordTranslationDictionaries.SiteId, multipleWordTranslationDictionaries.WebId, "Custom"));

                    Dictionaries.Instance.RegisterDictionaryAsRootForUrl(multipleWordTranslationDictionaries, true);
                }
            }
        }

        private static void InsertExcludedPartsFromTranslation(StringBuilder tempResponse, Dictionary<int, string> excludedPartsFromTranslation)
        {
            if (excludedPartsFromTranslation.Count > 0)
            {
                for (int i = 0; i < excludedPartsFromTranslation.Count; i++)
                {
                    tempResponse.Replace("$$$$WEBPARTNOTRANSLATE" + i + "$$$$", excludedPartsFromTranslation[i]);
                }
            }
        }

        private static void InitializeCustomDictionaryCache(string siteUrl, string listName, bool reloadCustomCache)
        {
            Utilities.LogException("InitializeCustomDictionaryCache");

            HttpContext.Current.Cache.Remove("SPS_HASHCODES_PAGES");

            string webId = "_" + Convert.ToString(SPContext.Current.Web.ID);

            if (listName.Equals("TranslationContents"))
            {
                webId = "_" + SPContext.Current.Site.WebApplication.Id;
            }

            HttpContext.Current.Cache.Remove("SPS_TRANSLATION_CACHE_IS_LOADED" + webId);
            HttpContext.Current.Cache.Add("SPS_TRANSLATION_CACHE_IS_LOADED" + webId,
                                          "1", null,
                                          Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                          CacheItemPriority.NotRemovable, null);

            if (reloadCustomCache)
            {
                LoadCustomDictionaries(siteUrl, listName, Dictionaries.Instance.DefaultLanguage);
            }

            // Create new hashtable for translated pages
            HttpContext.Current.Cache.Remove("TRANSLATED_PAGES");
            HttpContext.Current.Cache.Add("TRANSLATED_PAGES", new Hashtable(), null,
                                          Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                          CacheItemPriority.NotRemovable, null);
        }

        private static void InitializeGlobalDictionaryCache(string rootUrl)
        {
            var visibleLanguages = new List<LanguageItem>();

            Dictionaries.Instance.DefaultLanguage = GetLanguagesFromLanguageVisibilityList(rootUrl, visibleLanguages);

            Dictionaries.Instance.VisibleLanguages = visibleLanguages;

            SaveApplicationSettingsToHttpContextCache();

            // Save default language
            HttpContext.Current.Cache.Remove("SPS_TRANSLATION_DEFAULT_LANGUAGE");
            HttpContext.Current.Cache.Add("SPS_TRANSLATION_DEFAULT_LANGUAGE", Dictionaries.Instance.DefaultLanguage, null,
                                          Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                          CacheItemPriority.NotRemovable, null);

            LoadGlobalDictionaries(rootUrl, "TranslationContents");

            HttpContext.Current.Cache.Insert("OCEANIK_DICTIONARIES", Dictionaries.Instance, null,
                                                     Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                                     CacheItemPriority.NotRemovable, null);
        }

        private static void ReplaceLanguageMetadataLabel(StringBuilder tempResponse)
        {
            if ((HttpContext.Current != null) && (!string.IsNullOrEmpty((string)HttpContext.Current.Cache["AlphamosaikLanguageFieldLabel"])))
            {
                if ((HttpContext.Current.Request.Url.ToString().IndexOf("EditForm.aspx", StringComparison.OrdinalIgnoreCase) > -1) ||
                (HttpContext.Current.Request.Url.ToString().IndexOf("NewForm.aspx", StringComparison.OrdinalIgnoreCase) > -1) ||
                (HttpContext.Current.Request.Url.ToString().IndexOf("DispForm.aspx", StringComparison.OrdinalIgnoreCase) > -1))
                {
                    tempResponse.Replace(">SharePoint_Item_Language<", ">" + (string)HttpContext.Current.Cache["AlphamosaikLanguageFieldLabel"] + "<");
                }
            }
        }

        private static void RemoveNoTranslateTag(StringBuilder tempResponse)
        {
            if ((bool)HttpContext.Current.Cache["AlphamosaikBannerWithNoTranslation"])
                tempResponse.Replace("$$$SPSNOTRANSLATE$$$", string.Empty);
            else
                return;
        }

        private void AddMenuCompletingMode(StringBuilder tempResponse, string lang, string languageSource, bool completingDictionaryMode)
        {
            string menuAddToDictionary;
            string menuAddAllToDictionary = string.Empty;
            bool defaultLanguagePage = false;

            if (languageSource == lang)
            {
                menuAddToDictionary =
                    "<ie:menuitem id=\"SPS_MenuAddToDictionary\" Disabled=\"true\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";

                defaultLanguagePage = languageSource == lang;
            }
            else
            {
                menuAddToDictionary =
                    "<ie:menuitem id=\"SPS_MenuAddToDictionary\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";
            }

            if (defaultLanguagePage)
            {
                menuAddToDictionary +=
                    "onMenuClick=\"window.location.search = changeLocation(window.location.search,'SPS_Trans_Code','Completing_Dictionary_Mode_ON');\" ";
                menuAddToDictionary +=
                    "text=\"Display dictionary completing mode (Unavailable on the default language page)\" description=\"Display dictionary completing mode, with highlighting of phrases which are not in dictionary, and tool to add them.\" >";
            }
            else
            {
                if (completingDictionaryMode)
                {
                    menuAddToDictionary +=
                        "onMenuClick=\"window.location.search = changeLocation(window.location.search,'SPS_Trans_Code','Completing_Dictionary_Mode_OFF');\" ";
                    menuAddToDictionary +=
                        "text=\"Exit dictionary completing mode\" description=\"Exit dictionary completing mode.\" >";

                    // Add menu option Add all to dictionary
                    menuAddAllToDictionary =
                        "<ie:menuitem id=\"SPS_MenuAddAllToDictionary\" type=\"option\" iconSrc=\"/_layouts/images/alpha_logo_menu.png\" ";
                    menuAddAllToDictionary += "onMenuClick=\"AddAllItemToDictionary();\" ";
                    menuAddAllToDictionary +=
                        "text=\"Add all items to dictionary\" description=\"Add all items to dictionary.\" >";
                    menuAddAllToDictionary += "</ie:menuitem>" + Environment.NewLine;
                }
                else
                {
                    string lcidValue = string.Empty;
                    foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                    {
                        if (languageItem.LanguageDestination == languageSource)
                        {
                            lcidValue = languageItem.Lcid.ToString();
                        }
                    }

                    menuAddToDictionary += "onMenuClick=\"window.location = 'javascript:GoCompletingMode(" + lcidValue +
                                           ");';\" ";

                    menuAddToDictionary +=
                        "text=\"Display dictionary completing mode\" description=\"Display dictionary completing mode, with highlighting of phrases which are not in dictionary, and tool to add them.\" >";
                }
            }

            menuAddToDictionary += "</ie:menuitem>" + Environment.NewLine;

            int addToDictionaryIndex =
                tempResponse.IndexOf(" onMenuClick=\"STSNavigate2(event,'" + "/_layouts/settings.aspx');\"");

            if (addToDictionaryIndex == -1)
                if (TranslatorRegex.AutoCompletingModeButtonRegex.IsMatch(tempResponse.ToString()))
                {
                    addToDictionaryIndex = TranslatorRegex.AutoCompletingModeButtonRegex.Match(tempResponse.ToString()).Index;
                }

            if (addToDictionaryIndex > -1)
            {
                addToDictionaryIndex = tempResponse.LastIndexOf("<ie:menuitem ", addToDictionaryIndex);
                if (addToDictionaryIndex > -1)
                {
                    if (string.IsNullOrEmpty(menuAddAllToDictionary))
                    {
                        tempResponse.Insert(addToDictionaryIndex, menuAddToDictionary);
                    }
                    else
                        tempResponse.Insert(addToDictionaryIndex, menuAddToDictionary + menuAddAllToDictionary);
                }
            }

            if (HttpContext.Current.Request.QueryString["SPS_Trans_Code"] != null)
            {
                if (HttpContext.Current.Request.QueryString["SPS_Trans_Code"] == "Completing_Dictionary_Mode_Process2")
                {
                    if (tempResponse.ToLower().Contains("</script>"))
                        tempResponse.Insert(tempResponse.ToLower().IndexOf("</script>") + 9,
                                            "<script type=\"text/javascript\" src=\"/_layouts/alphamosaik.translator.js\"></script>");

                    var dictionaryUrl = SPContext.Current.Site.Url;
                    string[] urlSections = dictionaryUrl.Split('/');
                    dictionaryUrl = urlSections[0] + "//" + urlSections[2] + "/" +
                                    "Lists/TranslationContents/CustomizedPhrasesGridView.aspx";
                    string dictionaryQueryString = "?SPS_Trans_Code=AddToDictionary&SPS_Default_Lang=" + languageSource +
                                                   "&SPS_Dest_Lang=" + lang + "&SPS_Phrase_To_Add=";

                    if (tempResponse.ToLower().Contains(" onload=\""))
                        tempResponse.Insert(tempResponse.ToLower().IndexOf(" onload=\"") + 9,
                                            "javascript:alpha_initialize('" + dictionaryUrl + dictionaryQueryString +
                                            "');");
                }
            }

            int codeToInsertIndex = tempResponse.IndexOf("<script type=\"text/javascript\">RegisterSod");

            if (codeToInsertIndex > -1)
            {
                var codeToInsert = new StringBuilder();
                codeToInsert.Clear();

                codeToInsert.Append(
                    "<script type=\"text/javascript\" src=\"/_layouts/alphamosaik.translator/scripts/alphamosaik.translator.JQuery.js\"></script>" + Environment.NewLine +
                    "<link rel=\"stylesheet\" type=\"text/css\" href=\"/_layouts/alphamosaik.translator/themes/alphamosaik.translator.JQuery.css\"/>" + Environment.NewLine +
                    "<script type=\"text/javascript\">var Jq = jQuery.noConflict(); var CurrentLcCode = '" + languageSource + "'; var Lang = '" + lang + "';</script>" + Environment.NewLine +
                    "<div id='DivAddItemtoDictionary' style=\"display:none; cursor: default\"><table width=\"100%\"><tr><td><table id='TableAddItemtoDictionary' width=\"100%\"><thead><tr><th></th></tr></thead><tbody></tbody></table></td></tr><tr><td><input type=\"button\" id=\"DoneAddItemToDictionary\" value=\"Done\" /><input type=\"button\" id=\"ExecuteAddItemToDictionary\" value=\"Add selected item\" /><input type=\"button\" id=\"CancelAddItemToDictionary\" value=\"Cancel\" /></td></tr></table></div>" + Environment.NewLine +

                    // Section pour le message Please Wait durant le traitement des tags SPAN
                    "<div id=\"pleasewaitScreen\" STYLE=\"position:absolute;z-index:5;top:30%;left:42%;visibility:hidden\">" + Environment.NewLine +
                    "<TABLE BGCOLOR=\"#000000\" BORDER=\"1\" BORDERCOLOR=\"#000000\" CELLPADDING=\"0\" CELLSPACING=\"0\" HEIGHT=\"100\" WIDTH=\"300\" ID=\"Table1\">" + Environment.NewLine +
                    "<TR><TD WIDTH=\"100%\" HEIGHT=\"100%\" BGCOLOR=\"white\" ALIGN=\"CENTER\" VALIGN=\"MIDDLE\"><FONT FACE=\"Arial\" SIZE=\"4\" COLOR=\"black\"><B>Creating items list<BR>Please Wait...</B></FONT></TD></TR>" + Environment.NewLine +
                    "</TABLE></DIV>" + Environment.NewLine +

                    // ---
                    "<script type=\"text/javascript\">" + Environment.NewLine + "// <![CDATA[" + Environment.NewLine +
                    "document.write('<script type=\"text/javascript\" src=\"/_layouts/alphamosaik.translator/scripts/alphamosaik.translator.JQuery.DataTables.js\"></' + 'script>');" + Environment.NewLine +
                    "document.write('<script type=\"text/javascript\" src=\"/_layouts/alphamosaik.translator/scripts/alphamosaik.translator.JQuery.BlockUI.js\"></' + 'script>');" + Environment.NewLine +
                    "document.write('<script type=\"text/javascript\" src=\"/_layouts/alphamosaik.translator/scripts/alphamosaik.translator.CompletionMode.js\"></' + 'script>');"
                    + Environment.NewLine + "// ]]></script>" + Environment.NewLine);

                // Traduction des items ajoutes
                const int ExtractorStatus = -1; // On ne l'utilise pas !
                string currentSiteUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.PathAndQuery, string.Empty);

                TranslateFromDictionary(codeToInsert, lang, languageSource, ExtractorStatus, currentSiteUrl);
                tempResponse.Insert(codeToInsertIndex, codeToInsert);
            }
        }

        private void RemoveSpsTagsAfterAddingMenus(StringBuilder tempResponse, bool allWebPartsContentDisable, bool spsContentExist, Hashtable webPartHashTable)
        {
            if (HttpContext.Current == null)
                return;

            if (tempResponse.IndexOf("TITLE_EDITOR\"", StringComparison.OrdinalIgnoreCase) > -1)
            {
                SaveWebpartProperties(tempResponse);
            }
            else
            {
                if (!IsEditPageMode())
                    foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                        tempResponse.Replace("SPS_STATIC_LNG_" + languageItem.LanguageDestination, string.Empty);
            }

            tempResponse.Replace("value=\"(SPS_LNG_ALL)\">(SPS_LNG_ALL)</option>",
                                 "value=\"(SPS_LNG_ALL)\">All Languages</option>");

            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
            {
                string sharePointItemLanguageToReplace = "value=\"SPS_LNG_" + languageItem.LanguageDestination + "\">SPS_LNG_" +
                                                         languageItem.LanguageDestination + "</option>";
                tempResponse.Replace(sharePointItemLanguageToReplace,
                                     sharePointItemLanguageToReplace.Replace(">SPS_LNG_" + languageItem.LanguageDestination + "<",
                                                                             ">" + languageItem.DisplayName + "<"));
            }

            if (spsContentExist)
                ExcludeContentWebpartFromTrad(tempResponse, allWebPartsContentDisable, webPartHashTable);

            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
            {
                // Re replace values
                tempResponse.Replace(";SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination + "&", ";SPS_LNG_" + languageItem.LanguageDestination + "&");
                tempResponse.Replace("|SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination + "&", "|SPS_LNG_" + languageItem.LanguageDestination + "&");
                tempResponse.Replace(";SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination, ";SPS_LNG_" + languageItem.LanguageDestination);
                tempResponse.Replace("|SPS_ALPHA_TEMP_LNG_" + languageItem.LanguageDestination, "|SPS_LNG_" + languageItem.LanguageDestination);
            }

            tempResponse.Replace("_SharePoint_ALPHA_TEMP_Item Language", "_SharePoint_Item_Language");
            tempResponse.Replace(";SharePoint_ALPHA_TEMP_Item Language", ";SharePoint_Item_Language");
        }
    }
}
