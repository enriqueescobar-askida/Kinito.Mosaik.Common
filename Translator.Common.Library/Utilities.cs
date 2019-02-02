// ----------------------------------------------------------public ----------------------------------------------------------
// <copyright file="Utilities.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Utilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Alphamosaik.Common.Library;
using Alphamosaik.Common.SharePoint.Library;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Exception = System.Exception;

namespace Translator.Common.Library
{
    public static class Utilities
    {
        private static List<EventReceiverDefinition> _events;

        /// <summary>
        /// this function split a string that contains a delimeter char
        /// </summary>
        /// <param name="originalString">the complete string</param>
        /// <param name="delimeter">delimiter to use</param>
        /// <param name="index">which part of the string you want to get (0/1)</param>
        /// <returns>return the value found</returns>
        public static string GetPartOfString(string originalString, char delimeter, int index)
        {
            string functionReturn = String.Empty;
            try
            {
                string[] stringArray = originalString.Split(delimeter);
                functionReturn = stringArray[index];
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetItemCollection", "ClsUtilities"));
                log.WriteToLog();
            }

            return functionReturn;
        }

        /// <summary>
        /// If a job is running on the solution, this method waits it to finish.
        /// </summary>
        /// <param name="solution">solution object</param>
        public static void WaitForJobToFinish(SPSolution solution)
        {
            if (solution == null) return;

            try
            {
                while (solution.JobExists
                    && (solution.JobStatus == SPRunningJobStatus.Initialized
                    || solution.JobStatus == SPRunningJobStatus.Scheduled))
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception ee)
            {
                throw new Exception("Error while waiting to finish running jobs.", ee);
            }
        }

        public static void ExecuteJobDefinitions()
        {
            if (SPFarm.Local.TimerService.JobDefinitions != null)
            {
                foreach (SPJobDefinition job in SPFarm.Local.TimerService.JobDefinitions)
                {
                    try
                    {
                        if (job.DisplayName.IndexOf("alphamosaik", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            job.Execute(SPServer.Local.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("Oceanik", "Error in Utilities.ExecuteJobDefinitions:  " + ex.Message);
                    }
                }
            }
        }

        /// <summary>this function add days but do not include the days of the weekend</summary>
        /// <param name="startDate">The Start date</param>
        /// <param name="days">the number of days to add</param>
        /// <returns>return new date</returns>
        public static DateTime BusinessDateAdd(DateTime startDate, int days) 
        {
            try
            {
                for (int i = 0; i < days; i++)
                {
                    startDate = startDate.AddDays(1);
                    if (startDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        startDate = startDate.AddDays(2);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetItemCollection", "ClsUtilities"));
                log.WriteToLog();
            }

            return startDate;
        }

        public static bool ContainsSlashAtTheEnd(string url)
        {
            if (url.EndsWith("/"))
                return true;
            return false;
        }

        public static string RemoveSlashAtTheEnd(string url)
        {
            string functionReturn = url;

            if (ContainsSlashAtTheEnd(url))
                functionReturn = url.Remove(url.LastIndexOf("/"));
            return functionReturn;
        }

        public static IEnumerable<CultureInfo> GetSiteLanguageInstalled(string siteUrl)
        {
            using (var site = new SPSite(siteUrl))
            {
                using (var web = site.RootWeb)
                {
                    // Display the alternate languages for the Web site.
                    if (web.IsMultilingual)
                    {
                        Console.WriteLine("\nAlternate Languages");

                       return web.SupportedUICultures;
                    }
                }
            }

            return null;
        }

        public static void AddToNewPhrasesList(string phraseToAdd, string url, string defaultLang, StringBuilder tempResponse, string currentMatch)
        {
            try
            {
                using (var currentSite = new SPSite(url))
                using (SPWeb web = currentSite.OpenWeb())
                {
                    SPList extractorTranslationsList = web.GetList("/Lists/ExtractorTranslations");

                    if (!String.IsNullOrEmpty(phraseToAdd))
                    {
                        var query = new SPQuery
                                        {
                                            Query = "<Where><Eq><FieldRef Name='" + defaultLang + "'/>" +
                                                    "<Value Type='Note'>" + phraseToAdd.Trim() +
                                                    "</Value></Eq></Where>",
                                            QueryThrottleMode = SPQueryThrottleOption.Override
                                        };

                        SPListItemCollection collListItems = extractorTranslationsList.GetItems(query);

                        if (collListItems.Count == 0)
                        {
                            bool mustExclude = false;

                            foreach (Match match in Regex.Matches(tempResponse.ToString(), "(<((script)|(style)))[^>]*(>)(\\s)*[^<]*(</)", RegexOptions.IgnoreCase))
                            {
                                if (match.Value.Contains(currentMatch))
                                    mustExclude = true;
                            }

                            if ((!Regex.IsMatch(phraseToAdd, "(\\s)*[0-9]+(\\s)*", RegexOptions.IgnoreCase)) && (!mustExclude) && (!Regex.IsMatch(phraseToAdd, "^(\\s)*((https?):/)?(/[^\\s]*)(\\s)*$", RegexOptions.IgnoreCase)))
                            {
                                SPListItem newItem = extractorTranslationsList.Items.Add();

                                if (extractorTranslationsList.Fields.ContainsField(defaultLang))
                                    newItem[defaultLang] = phraseToAdd.Trim();

                                if (extractorTranslationsList.Fields.ContainsField("Page"))
                                    newItem["Page"] = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);

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
                LogException("AddToNewPhrasesList", e, EventLogEntryType.Warning);
            }

            return;
        }

        public static void LogException(string eventMessage)
        {
            Alphamosaik.Common.SharePoint.Library.Exception.LogException("Oceanik", eventMessage);
        }

        public static void LogException(string eventMessage, EventLogEntryType type)
        {
            Alphamosaik.Common.SharePoint.Library.Exception.LogException("Oceanik", eventMessage, type);
        }

        public static void LogException(string eventMessage, Exception e, EventLogEntryType type)
        {
            Alphamosaik.Common.SharePoint.Library.Exception.LogException("Oceanik", eventMessage, e, type);
        }

        public static string TranslateWildcards(StringBuilder tempResponse, Match currentMatch, string value, Dictionary<string, int> hashTable, int extractorStatus, string url, 
                                               string currentLccode, CustomStringCollection translatedValue, Hashtable wildcardsHashTable, ArrayList translatedRegexpValue)
        {
            string result = String.Empty;

            if (wildcardsHashTable[value[0]] != null)
            {
                var char1ArrayList = wildcardsHashTable[value[0]] as ArrayList;
                result = TranslateWildcardHelper(value, char1ArrayList, translatedRegexpValue, currentMatch, extractorStatus, hashTable, tempResponse, url, currentLccode, translatedValue);
            }

            if (String.IsNullOrEmpty(result) && wildcardsHashTable['*' + value[value.Length - 1]] != null)
            {
                var char1ArrayList = wildcardsHashTable['*' + value[value.Length - 1]] as ArrayList;
                result = TranslateWildcardHelper(value, char1ArrayList, translatedRegexpValue, currentMatch, extractorStatus, hashTable, tempResponse, url, currentLccode, translatedValue);
            }

            return result;
        }

        public static int GetSiteWebDefaultLanguage()
        {
            int defaultCulture;

            string siteUrl = SPContext.Current.Web.Url;

            using (var site = new SPSite(siteUrl))
            {
                using (SPWeb web = site.RootWeb)
                {
                    defaultCulture = web.UICulture.LCID;
                }
            }

            return defaultCulture;
        }

        public static string ConvertDateUsingCulture(string sourceDate, int lcidDefaultCulture, int lcidDestinationCulture)
        {
            string destinationDate = String.Empty;

            var allStandardFormats = new[] { 'd', 'D', 'f', 'F', 'g', 'G', 'm', 'M', 'o', 'O', 'r', 'R', 's', 't', 'T', 'u', 'U', 'y', 'Y' };

            var sourceCultureInfo = CultureInfo.GetCultureInfo(lcidDefaultCulture);

            DateTime currentValue;
            if (DateTime.TryParse(sourceDate, out currentValue))
            {
                for (int i = 0; i < allStandardFormats.Length; i++)
                {
                    string temp = currentValue.ToString(allStandardFormats[i].ToString(), sourceCultureInfo);

                    if (sourceDate.Equals(temp))
                    {
                        var destinationCultureInfo = CultureInfo.GetCultureInfo(lcidDestinationCulture);
                        destinationDate = currentValue.ToString(allStandardFormats[i].ToString(), destinationCultureInfo);
                        break;
                    }
                }
            }

            return destinationDate;
        }

        public static string TranslateWildcardHelper(string value, ArrayList char1ArrayList, ArrayList translatedRegexpValue, Match currentMatch, int extractorStatus, Dictionary<string, int> hashTable, StringBuilder tempResponse, string url, string currentLccode, CustomStringCollection translatedValue)
        {
            foreach (string[] array in char1ArrayList)
            {
                string[] currentRegValSplitted = array;

                if (currentRegValSplitted.Length > 2)
                {
                    if (((value.IndexOf(currentRegValSplitted[1]) != -1) && (value.Substring(0, currentRegValSplitted[1].Length).Trim() == currentRegValSplitted[1]))
                        && ((value.IndexOf(currentRegValSplitted[2]) != -1) && (value.Substring(value.Length - currentRegValSplitted[2].Length, currentRegValSplitted[2].
                                                                                                                                                    Length).Trim() == currentRegValSplitted[2])))
                    {
                        if (translatedRegexpValue != null)
                        {
                            var currentTranslatedValSplitted = translatedRegexpValue[Convert.ToInt32(currentRegValSplitted[0])] as Array;

                            if (currentTranslatedValSplitted != null)
                                if ((currentTranslatedValSplitted.Length > 1) &&
                                    (!(String.IsNullOrEmpty(currentTranslatedValSplitted.GetValue(0).ToString()) &&
                                       String.IsNullOrEmpty(currentTranslatedValSplitted.GetValue(1).ToString()))))
                                {
                                    string wordUnderStars = currentMatch.Value.Replace("&nbsp;", String.Empty).Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").
                                        Replace("&amp;", "&").Substring(1, currentMatch.Value.Replace("&nbsp;", String.Empty).Replace("&lt;", "<").
                                                                               Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&amp;", "&").Length - 1).Trim().Substring(currentRegValSplitted[1].
                                                                                                                                                                                   Length + 0, currentMatch.Value.Replace("&nbsp;", String.Empty).Trim().Replace("&lt;", "<").Replace("&gt;", ">").
                                                                                                                                                                                                   Replace("&quot;", "\"").Replace("&amp;", "&").Substring(1, currentMatch.Value.Replace("&nbsp;", String.Empty).Trim().
                                                                                                                                                                                                                                                                  Replace("&nbsp;", String.Empty).Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&amp;", "&").
                                                                                                                                                                                                                                                                  Length - 2).Trim().Length - currentRegValSplitted[1].Length - currentRegValSplitted[2].Length - 0).
                                        Replace("&nbsp;", String.Empty).Replace("&quot;", "\"");

                                    // Verify wether the wordUnderStars is in the dictionary or not
                                    string valueForStars = wordUnderStars.Trim();

                                    if ((!String.IsNullOrEmpty(valueForStars)) && (extractorStatus != -1) && !hashTable.ContainsKey(valueForStars))
                                    {
                                        var response1 = new StringBuilder(tempResponse.ToString());
                                        Match match1 = currentMatch;

                                        SPSecurity.RunWithElevatedPrivileges(() => AddToNewPhrasesList(valueForStars, url, currentLccode, response1, match1.Value));
                                    }

                                    if (!String.IsNullOrEmpty(valueForStars) && hashTable.ContainsKey(valueForStars))
                                    {
                                        var indexOfTranslation = hashTable[valueForStars];

                                        // get translated value
                                        if (translatedValue != null)
                                            if (!String.IsNullOrEmpty(translatedValue[indexOfTranslation]))
                                                wordUnderStars = wordUnderStars.Replace(valueForStars, translatedValue[indexOfTranslation]);
                                    }

                                    return currentMatch.Value.Trim().Replace(currentMatch.Value.Trim().Substring(1, currentMatch.Value.Trim().Length - 2).
                                                                                 Trim(), currentTranslatedValSplitted.GetValue(0).ToString().Substring(0, currentTranslatedValSplitted.GetValue(0).
                                                                                                                                                              ToString().Length).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&quot;", "\"") + " " +
                                                                                         wordUnderStars.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + " " + currentTranslatedValSplitted.
                                                                                                                                                                                    GetValue(1).ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&quot;", "\""));
                                }
                        }
                    }
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Get language code from query string
        /// </summary>
        /// <returns>
        /// The get language code from query string.
        /// </returns>
        public static string GetLanguageCodeFromQueryString()
        {
            string result;

            if (HttpContext.Current.Request.QueryString["SPSLanguage"] != null)
                result = HttpContext.Current.Request.QueryString["SPSLanguage"];
            else
                return null;

            if (BaseStaticOverride<Languages>.Instance.IsSupportedLanguage(result))
                return result;

            int intIdx = HttpContext.Current.Request.Url.ToString().LastIndexOf("SPSLanguage") + "SPSLanguage".Length + 1;
            result = HttpContext.Current.Request.Url.ToString().Substring(intIdx, 2);

            if (BaseStaticOverride<Languages>.Instance.IsSupportedLanguage(result))
                return result;

            if (HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"] != null)
                return HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"].ToString();
            return null;
        }

        public static void SetLanguageFromMsCookie(HttpContext context, string language)
        {
            try
            {
                // set for response
                var cookie = new HttpCookie("lcid", BaseStaticOverride<Languages>.Instance.GetLcid(language).ToString())
                {
                    Expires = DateTime.Now.AddYears(1)
                };
                context.Response.AppendCookie(cookie);

                // set for request
                var reqCookie = context.Request.Cookies.Get("lcid");
                if (reqCookie != null)
                {
                    reqCookie.Value = BaseStaticOverride<Languages>.Instance.GetLcid(language).ToString();
                }
            }
            catch (Exception e)
            {
                LogException("SetLanguageFromMsCookie", e, EventLogEntryType.Warning);
            }
        }

        public static string GetLanguageCode(HttpContext context)
        {
            string languageCode;
            HttpCookie httpCookie = context.Request.Cookies.Get("SharePointTranslator");

            // On se base en priorité sur la langue du language pack MS utilisé
            if (!String.IsNullOrEmpty(Alphamosaik.Common.SharePoint.Library.Utilities.GetLanguageFromMsCookie(context)))
            {
                languageCode = Alphamosaik.Common.SharePoint.Library.Utilities.GetLanguageFromMsCookie(context);
            }
            else if (GetLanguageCodeFromQueryString() != null &&
                     GetLanguageCodeFromQueryString().ToUpper() != "XX")
            {
                languageCode = GetLanguageCodeFromQueryString();
            }
            else if (httpCookie != null && BaseStaticOverride<Languages>.Instance.IsSupportedLanguage(httpCookie.Value))
            {
                languageCode = httpCookie.Value;
            }
            else
            {
                string browserLanguage = String.Empty;
                if (context.Request.UserLanguages != null)
                {
                    browserLanguage = context.Request.UserLanguages[0].Substring(0, 2).ToUpper();
                }

                if (BaseStaticOverride<Dictionaries>.Instance.LanguageVisible(browserLanguage) && context.Request.UserLanguages != null)
                {
                    languageCode = browserLanguage;
                }
                else if (HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"] != null)
                {
                    languageCode = HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"].ToString();
                }
                else
                {
                    languageCode = "EN";
                }
            }

            languageCode = BaseStaticOverride<Languages>.Instance.GetBackwardCompatibilityLanguageCode(languageCode);

            if (!BaseStaticOverride<Dictionaries>.Instance.LanguageVisible(languageCode) && HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"] != null)
            {
                languageCode = HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"].ToString();
            }

            SetLanguage(context, languageCode);

            return languageCode;
        }

        public static void SetLanguage(HttpContext context, string language)
        {
            var cookie = new HttpCookie("SharePointTranslator", language) { Expires = DateTime.MaxValue };
            context.Response.AppendCookie(cookie);
            SetLanguageFromMsCookie(context, language);
        }

        public static int IsCrawler(HttpContext context)
        {
            if (context.Request.Browser != null)
            {
                if (context.Request.Browser.Crawler)
                {
                    return 1;
                }
            }

            // Microsoft doesn't properly detect several crawlers 
            if (!String.IsNullOrEmpty(context.Request.UserAgent))
            {
                // Cas du crawl de recherche SharePoint
                if (context.Request.UserAgent.IndexOf("MS Search ", StringComparison.OrdinalIgnoreCase) > -1 ||
                    context.Request.UserAgent.IndexOf("Coveo", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return 1;
                }
            }

            if (!String.IsNullOrEmpty(context.Request.UserAgent))
            {
                // Cas de l'éditeur SharePoint Designer
                if (context.Request.UserAgent.IndexOf("FrontPage", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return 2;
                }
            }

            return 0;
        }

        /// <summary>
        /// Trace normal caught exception that can occur and that we know that exception can be throw
        /// </summary>
        /// <param name="eventMessage">title to be displayed in the log</param>
        /// <param name="e">The exception to be logged</param>
        public static void TraceNormalCaughtException(string eventMessage, Exception e)
        {
            Trace.WriteLine("Oceanik : " + eventMessage + Environment.NewLine + e);
        }

        public static void SetItemLanguage(SPWeb inputSpWeb, string listId, string url, string itemId, string language)
        {
            try
            {
                using (var currentSite = new SPSite(url))
                using (SPWeb web = currentSite.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    SPList currentList = web.Lists[new Guid(listId)];

                    try
                    {
                        SPListItem listItem = currentList.GetItemById(Convert.ToInt32(itemId));

                        if (language == "ALL")
                        {
                            LinkItemWith(inputSpWeb, listId, url, itemId, "Delete");
                            listItem["SharePoint_Item_Language"] = "(SPS_LNG_ALL)";
                        }
                        else
                        {
                            listItem["SharePoint_Item_Language"] = "SPS_LNG_" + language;
                        }

                        listItem.SystemUpdate(false);
                    }
                    catch (Exception e)
                    {
                        LogException("SetItemLanguage", e, EventLogEntryType.Warning);
                    }
                }

                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=SetLanguage&", "?")
                                                              .Replace("&SPS_Trans_Code=SetLanguage", String.Empty), false);
                }
            }
            catch (Exception e)
            {
                LogException("SetItemLanguage", e, EventLogEntryType.Warning);
            }
        }

        public static void LinkItemWith(SPWeb inputSpWeb, string listId, string url, string itemId, string itemTargetId)
        {
            try
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Cache.Remove("SPS_HASHCODES_PAGES");

                using (var currentSite = new SPSite(url))
                using (SPWeb web = currentSite.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    SPList currentList = web.Lists[new Guid(listId)];

                    //var query = new SPQuery
                    //                {
                    //                    Query = "<Query></Query>",
                    //                    QueryThrottleMode = SPQueryThrottleOption.Override,
                    //                    ViewAttributes = "Scope='Recursive'",                                        
                    //                };
                                        
                    //SPListItemCollection currentItemsCollection = currentList.GetItems(query);

                    SPListItem currentItem = currentList.GetItemById(Convert.ToInt32(itemId));

                    bool isDiscussionFolder = false;

                    if (currentItem.Folder != null)
                        isDiscussionFolder = true;

                    bool deactivateEh = false;
                    if (HttpContext.Current != null)
                        if (HttpContext.Current.Cache["AlphamosaikDeactivateEventHandlerOnList"] != null)
                            deactivateEh = (bool)HttpContext.Current.Cache["AlphamosaikDeactivateEventHandlerOnList"];

                    if (deactivateEh)
                        DeactivateEvents(currentList);

                    var itemToUpdateCollection = new ArrayList();

                    string currentItemLanguage = String.Empty;
                    string itemTargetLanguage = String.Empty;
                    string currentItemGroupLanguage = String.Empty;
                    string newIdForLinkedItems = String.Empty;
                    string itemTargetIniGroupLang = String.Empty;

                    SPListItem itemTarget = null;

                    if (currentList.Fields.ContainsField("SharePoint_Item_Language"))
                    {
                        currentItemLanguage = (string)currentItem["SharePoint_Item_Language"];

                        if ((itemTargetId != "Delete") && (itemTargetId != "Linkable"))
                        {
                            //var query = new SPQuery
                            //            {
                            //                Query = "<Query></Query>",
                            //                QueryThrottleMode = SPQueryThrottleOption.Override,
                            //                ViewAttributes = "Scope='Recursive'"
                            //            };
                            //SPListItemCollection currentItemsCollection = currentList.GetItems(query);

                            //itemTarget = currentItemsCollection.GetItemById(Convert.ToInt32(itemTargetId));

                            itemTarget = currentList.GetItemById(Convert.ToInt32(itemTargetId));

                            if (currentList.Fields.ContainsField("Linkable"))
                            {
                                if ((bool)itemTarget["Linkable"])
                                    itemTarget["Linkable"] = false;

                                if (deactivateEh)
                                    if (!itemToUpdateCollection.Contains(itemTarget.ID))
                                        itemToUpdateCollection.Add(itemTarget.ID);
                                itemTarget.SystemUpdate(false);
                            }

                            if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                            {
                                itemTargetIniGroupLang = itemTarget["SharePoint_Group_Language"] == null ? "0" : itemTarget["SharePoint_Group_Language"].ToString();
                                newIdForLinkedItems = currentItem["SharePoint_Group_Language"] == null ? "0" : currentItem["SharePoint_Group_Language"].ToString();
                            }

                            itemTargetLanguage = (string)itemTarget["SharePoint_Item_Language"];
                            itemTargetLanguage = itemTargetLanguage.Replace("(SPS_LNG_ALL)", String.Empty).Replace("SPS_LNG_", String.Empty);
                        }

                        if (currentItemLanguage != null)
                            currentItemLanguage = currentItemLanguage.Replace("(SPS_LNG_ALL)", String.Empty).Replace("SPS_LNG_", String.Empty);
                    }

                    if (itemTargetId == "Linkable")
                    {
                        if (currentList.Fields.ContainsField("Linkable"))
                        {
                            if (currentItem["Linkable"] != null)
                                currentItem["Linkable"] = !(bool)currentItem["Linkable"];
                            else
                                currentItem["Linkable"] = true;

                            if (deactivateEh)
                                if (!itemToUpdateCollection.Contains(currentItem.ID))
                                    itemToUpdateCollection.Add(currentItem.ID);

                            currentItem.SystemUpdate(false);

                            if (HttpContext.Current != null)
                            {
                                HttpContext.Current.RewritePath((HttpContext.Current.Request.Url.AbsolutePath + "?" + HttpContext.Current.Request.QueryString).Replace("?SPS_Trans_Code=LinkListItem&", "?")
                                                                    .Replace("?SPS_Trans_Code=LinkListItem", String.Empty).Replace("&SPS_Trans_Code=LinkListItem", String.Empty));
                                HttpContext.Current.Response.Redirect((HttpContext.Current.Request.Url.AbsolutePath + "?" + HttpContext.Current.Request.QueryString).Replace("?SPS_Trans_Code=LinkListItem&", "?")
                                                                          .Replace("?SPS_Trans_Code=LinkListItem", String.Empty).Replace("&SPS_Trans_Code=LinkListItem", String.Empty), false);
                            }
                        }

                        if (deactivateEh)
                        {
                            ActivateEvents(currentList);

                            foreach (int currentItemId in itemToUpdateCollection)
                            {
                                currentList.GetItemById(currentItemId).SystemUpdate(false);
                            }
                        }

                        return;
                    }

                    if (itemTargetId == "Delete")
                    {
                        if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                        {
                            if (currentItem["SharePoint_Group_Language"] != null)
                                if (Convert.ToInt32(currentItem["SharePoint_Group_Language"]) != 0)
                                {
                                    var query = new SPQuery
                                                {
                                                    Query =
                                                        "<Where><Eq><FieldRef Name='SharePoint_Group_Language'/>" +
                                                        "<Value Type='Number'>" +
                                                        currentItem["SharePoint_Group_Language"] +
                                                        "</Value></Eq></Where>",
                                                    QueryThrottleMode = SPQueryThrottleOption.Override
                                                };
                                    if (!isDiscussionFolder)
                                        query.ViewAttributes = "Scope='Recursive'";

                                    SPListItemCollection collListItems = currentList.GetItems(query);

                                    foreach (SPListItem itemTmp in collListItems)
                                    {
                                        if (itemTmp["SharePoint_Group_Language"] != null)
                                        {
                                            newIdForLinkedItems = itemTmp.ID.ToString();
                                            if (newIdForLinkedItems != currentItem.ID.ToString())
                                                break;
                                        }
                                    }

                                    foreach (SPListItem itemTmp in collListItems)
                                    {
                                        itemTmp["SharePoint_Group_Language"] = Convert.ToInt32(newIdForLinkedItems);

                                        if (currentList.Fields.ContainsField(currentItemLanguage + " version"))
                                            itemTmp[currentItemLanguage + " version"] = String.Empty;

                                        if (deactivateEh)
                                            if (!itemToUpdateCollection.Contains(itemTmp.ID))
                                                itemToUpdateCollection.Add(itemTmp.ID);

                                        itemTmp.SystemUpdate(false);
                                    }

                                    if (collListItems.Count <= 2)
                                        foreach (SPListItem itemTmp in collListItems)
                                        {
                                            itemTmp["SharePoint_Group_Language"] = 0;
                                            itemTmp.SystemUpdate(false);
                                        }

                                    currentItem["SharePoint_Group_Language"] = 0; // null;

                                    if (deactivateEh)
                                        if (!itemToUpdateCollection.Contains(currentItem.ID))
                                            itemToUpdateCollection.Add(currentItem.ID);

                                    currentItem.SystemUpdate(false);
                                }
                        }

                        foreach (string languageDestination in BaseStaticOverride<Languages>.Instance.AllLanguages)
                        {
                            if (currentList.Fields.ContainsField(languageDestination + " version"))
                            {
                                currentItem[languageDestination + " version"] = String.Empty;
                            }
                        }

                        if (deactivateEh)
                            if (!itemToUpdateCollection.Contains(currentItem.ID))
                                itemToUpdateCollection.Add(currentItem.ID);

                        currentItem.SystemUpdate(false);

                        if (HttpContext.Current != null)
                        {
                            HttpContext.Current.RewritePath((HttpContext.Current.Request.Url.AbsolutePath + "?" + HttpContext.Current.Request.QueryString).Replace("?SPS_Trans_Code=LinkListItem&", "?")
                                                                .Replace("?SPS_Trans_Code=LinkListItem", String.Empty).Replace("&SPS_Trans_Code=LinkListItem", String.Empty));
                            HttpContext.Current.Response.Redirect((HttpContext.Current.Request.Url.AbsolutePath + "?" + HttpContext.Current.Request.QueryString).Replace("?SPS_Trans_Code=LinkListItem&", "?")
                                                                      .Replace("?SPS_Trans_Code=LinkListItem", String.Empty).Replace("&SPS_Trans_Code=LinkListItem", String.Empty), false);
                        }

                        if (deactivateEh)
                        {
                            ActivateEvents(currentList);

                            foreach (int currentItemId in itemToUpdateCollection)
                            {
                                currentList.GetItemById(currentItemId).SystemUpdate(false);
                            }
                        }

                        return;
                    }

                    //// Case of relink (update the language group for the current linked items)
                    if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                    {
                        if (currentItem["SharePoint_Group_Language"] != null)
                        {
                            currentItemGroupLanguage = currentItem["SharePoint_Group_Language"].ToString();

                            if (currentItem["SharePoint_Group_Language"].ToString() == currentItem.ID.ToString())
                            {
                                var query = new SPQuery
                                            {
                                                Query = "<Where><Eq><FieldRef Name='SharePoint_Group_Language'/>" +
                                                        "<Value Type='Number'>" +
                                                        currentItem["SharePoint_Group_Language"] +
                                                        "</Value></Eq></Where>",
                                                QueryThrottleMode = SPQueryThrottleOption.Override
                                            };

                                if (!isDiscussionFolder)
                                    query.ViewAttributes = "Scope='Recursive'";

                                SPListItemCollection collListItems = currentList.GetItems(query);

                                foreach (SPListItem itemTmp in collListItems)
                                {
                                    if (itemTmp["SharePoint_Group_Language"] != null)
                                    {
                                        newIdForLinkedItems = itemTmp.ID.ToString();
                                        if (newIdForLinkedItems != currentItem.ID.ToString())
                                            break;
                                    }
                                }

                                foreach (SPListItem itemTmp in collListItems)
                                {
                                    itemTmp["SharePoint_Group_Language"] = Convert.ToInt32(newIdForLinkedItems);

                                    if (deactivateEh)
                                        if (!itemToUpdateCollection.Contains(itemTmp.ID))
                                            itemToUpdateCollection.Add(itemTmp.ID);

                                    itemTmp.SystemUpdate(false);
                                }
                            }
                        }
                    }

                    if (currentList.Fields.ContainsField(currentItemLanguage + " version"))
                    {
                        currentItem[currentItemLanguage + " version"] = itemId;

                        if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                            currentItem["SharePoint_Group_Language"] = Convert.ToInt32(itemId);

                        if (deactivateEh)
                            if (!itemToUpdateCollection.Contains(currentItem.ID))
                                itemToUpdateCollection.Add(currentItem.ID);

                        currentItem.SystemUpdate(false);
                    }

                    if (currentList.Fields.ContainsField(itemTargetLanguage + " version"))
                    {
                        currentItem[itemTargetLanguage + " version"] = itemTargetId;

                        if (deactivateEh)
                            if (!itemToUpdateCollection.Contains(currentItem.ID))
                                itemToUpdateCollection.Add(currentItem.ID);

                        currentItem.SystemUpdate(false);
                    }

                    foreach (string languageDestination in BaseStaticOverride<Languages>.Instance.AllLanguages)
                    {
                        if (currentList.Fields.ContainsField(languageDestination + " version"))
                        {
                            if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                                if (itemTarget != null)
                                    itemTarget["SharePoint_Group_Language"] = Convert.ToInt32(itemId);

                            if (itemTarget != null)
                            {
                                itemTarget[languageDestination + " version"] = currentItem[languageDestination + " version"];

                                if (deactivateEh)
                                    if (!itemToUpdateCollection.Contains(itemTarget.ID))
                                        itemToUpdateCollection.Add(itemTarget.ID);

                                itemTarget.SystemUpdate(false);
                            }
                        }
                    }

                    foreach (string languageDestination in BaseStaticOverride<Languages>.Instance.AllLanguages)
                    {
                        if (currentList.Fields.ContainsField(languageDestination + " version"))
                        {
                            if (currentItem[languageDestination + " version"] != null)
                            {
                                string itemRecordedTargetId = currentItem[languageDestination + " version"].ToString().Remove(currentItem[languageDestination + " version"].ToString().IndexOf(";"));

                                //var query = new SPQuery
                                //            {
                                //                Query = "<Query></Query>",
                                //                QueryThrottleMode = SPQueryThrottleOption.Override,
                                //                ViewAttributes = "Scope='Recursive'"
                                //            };
                                //SPListItemCollection currentItemsCollection = currentList.GetItems(query);

                                //SPListItem itemRecordedTarget = currentItemsCollection.GetItemById(Convert.ToInt32(itemRecordedTargetId));

                                SPListItem itemRecordedTarget = currentList.GetItemById(Convert.ToInt32(itemRecordedTargetId));

                                foreach (string languageCode2 in BaseStaticOverride<Languages>.Instance.AllLanguages)
                                {
                                    if (itemRecordedTarget.Fields.ContainsField(languageCode2 + " version"))
                                    {
                                        {
                                            if (currentList.Fields.ContainsField("SharePoint_Group_Language"))
                                                itemRecordedTarget["SharePoint_Group_Language"] = Convert.ToInt32(itemId);

                                            itemRecordedTarget[languageCode2 + " version"] = currentItem[languageCode2 + " version"];

                                            if (deactivateEh)
                                                if (!itemToUpdateCollection.Contains(itemRecordedTarget.ID))
                                                    itemToUpdateCollection.Add(itemRecordedTarget.ID);

                                            itemRecordedTarget.SystemUpdate(false);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(currentItemGroupLanguage))
                    {
                        var query = new SPQuery
                                    {
                                        Query = "<Where><Eq><FieldRef Name='SharePoint_Group_Language'/>" +
                                                "<Value Type='Number'>" + currentItemGroupLanguage +
                                                "</Value></Eq></Where>",
                                        QueryThrottleMode = SPQueryThrottleOption.Override
                                    };

                        if (!isDiscussionFolder)
                            query.ViewAttributes = "Scope='Recursive'";

                        SPListItemCollection collListItems = currentList.GetItems(query);

                        if (collListItems.Count == 1)
                            foreach (SPListItem itemTmp in collListItems)
                                itemTmp["SharePoint_Group_Language"] = 0;
                    }

                    var queryClean = new SPQuery
                                         {
                                             Query = "<Where><Eq><FieldRef Name='SharePoint_Group_Language'/>" +
                                                     "<Value Type='Number'>" + newIdForLinkedItems +
                                                     "</Value></Eq></Where>",
                                             QueryThrottleMode = SPQueryThrottleOption.Override
                                         };

                    if (!isDiscussionFolder)
                        queryClean.ViewAttributes = "Scope='Recursive'";

                    SPListItemCollection collCleanListItems = currentList.GetItems(queryClean);

                    if (collCleanListItems.Count == 1)
                        foreach (SPListItem itemTmp in collCleanListItems)
                            LinkItemWith(inputSpWeb, listId, url, itemTmp.ID.ToString(), "Delete");

                    if (itemTargetId != "Delete")
                    {
                        var queryCleanTarget = new SPQuery
                                                   {
                                                       Query =
                                                           "<Where><Eq><FieldRef Name='SharePoint_Group_Language'/>" +
                                                           "<Value Type='Number'>" + itemTargetIniGroupLang +
                                                           "</Value></Eq></Where>",
                                                       QueryThrottleMode = SPQueryThrottleOption.Override
                                                   };

                        if (!isDiscussionFolder)
                            queryCleanTarget.ViewAttributes = "Scope='Recursive'";

                        SPListItemCollection collCleanTargetListItems = currentList.GetItems(queryCleanTarget);

                        if (collCleanTargetListItems.Count == 1)
                            foreach (SPListItem itemTmp in collCleanTargetListItems)
                                LinkItemWith(inputSpWeb, listId, url, itemTmp.ID.ToString(), "Delete");
                    }

                    if (deactivateEh)
                    {
                        ActivateEvents(currentList);

                        foreach (int currentItemId in itemToUpdateCollection)
                        {
                            currentList.GetItemById(currentItemId).SystemUpdate(false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogException("LinkItemWith", e, EventLogEntryType.Warning);
            }
        }

        private static void DeactivateEvents(SPList list)
        {
            if (_events == null)
            {
                _events = new List<EventReceiverDefinition>();
            }

            for (int i = list.EventReceivers.Count - 1; i >= 0; i--)
            {
                SPEventReceiverDefinition definition = list.EventReceivers[i];
                _events.Add(new EventReceiverDefinition
                                {
                                    Assembly = definition.Assembly,
                                    ClassName = definition.Class,
                                    Type = definition.Type
                                });

                list.EventReceivers[i].Delete();
            }
        }

        private static void ActivateEvents(SPList list)
        {
            foreach (EventReceiverDefinition receiverDefinition in _events)
            {
                list.EventReceivers.Add(
                    receiverDefinition.Type,
                    receiverDefinition.Assembly,
                    receiverDefinition.ClassName);
            }

            _events.Clear();
        }
    }
}