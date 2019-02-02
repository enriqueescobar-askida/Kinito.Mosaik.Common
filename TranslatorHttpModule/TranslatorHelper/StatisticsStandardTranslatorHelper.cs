// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatisticsStandardTranslatorHelper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StatisticsTranslatorHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using Alphamosaik.Common.Library.Licensing;
using Alphamosaik.Common.Library.Statistics;
using Alphamosaik.Oceanik.Sdk;
using Microsoft.SharePoint;

namespace TranslatorHttpHandler.TranslatorHelper
{
    public class StatisticsStandardTranslatorHelper : StandardTranslatorHelper
    {
        internal const int LowDetail = 1;
        internal const int FullDetail = 5;

        private const string StatisticsSlotName = "statisticsTime";

        public override void AddHelper(StringBuilder tempResponse, string lang, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, string currentLcCode, ref bool filteringDisplayCompleted, bool mobilePage, License.LicenseType licenseType, ref Dictionary<int, string> excludedPartsFromTranslation)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "AddHelper", LowDetail))
            {
                base.AddHelper(tempResponse, lang, viewAllItemsInEveryLanguages, completingDictionaryMode, currentLcCode, ref filteringDisplayCompleted, mobilePage, licenseType, ref excludedPartsFromTranslation);
            }
        }

        public override void ListFilteringDisplay(StringBuilder tempResponse, string lang, ref bool completed)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "ListFilteringDisplay", FullDetail))
            {
                base.ListFilteringDisplay(tempResponse, lang, ref completed);
            }
        }

        public override void SaveWebpartProperties(StringBuilder tempResponse)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "SaveWebpartProperties", FullDetail))
            {
                base.SaveWebpartProperties(tempResponse);
            }
        }

        public override void AddWebPartMenu(StringBuilder tempResponse, string lang)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "AddWebPartMenu", FullDetail))
            {
                base.AddWebPartMenu(tempResponse, lang);
            }
        }

        public override void AjaxFormating(StringBuilder tempResponse)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "AjaxFormating", FullDetail))
            {
                base.AjaxFormating(tempResponse);
            }
        }

        public override void RedirectToLinkedPage(string language)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "RedirectToLinkedPage", FullDetail))
            {
                base.RedirectToLinkedPage(language);
            }
        }

        public override void QuickLaunchFilter(StringBuilder tempResponse, string language)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "QuickLaunchFilter", FullDetail))
            {
                base.QuickLaunchFilter(tempResponse, language);
            }
        }

        public override void RemoveTranslatedTr(StringBuilder response, string itemLanguageToRemove)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "RemoveTranslatedTr", FullDetail))
            {
                base.RemoveTranslatedTr(response, itemLanguageToRemove);
            }
        }

        public override void RemoveTranslatedDiv(StringBuilder response, string language)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "RemoveTranslatedDiv", FullDetail))
            {
                base.RemoveTranslatedDiv(response, language);
            }
        }

        public override ArrayList RemoveTranslatedDivForCache(StringBuilder response, string cacheTag)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "RemoveTranslatedDivForCache", FullDetail))
            {
                return base.RemoveTranslatedDivForCache(response, cacheTag);
            }
        }

        public override Hashtable LoadWebPartHashTable(ref string spsWebpartLanguageList, ref bool spsContentExist, ref List<Guid> webPartListToRemove, string currentLanguage)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "LoadWebPartHashTable", FullDetail))
            {
                return base.LoadWebPartHashTable(ref spsWebpartLanguageList, ref spsContentExist, ref webPartListToRemove, currentLanguage);
            }
        }

        public override void ExcludeContentWebpartFromTrad(StringBuilder response, bool allWebPartsContentDisable, Hashtable webPartIdHashTable)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "ExcludeContentWebpartFromTrad", FullDetail))
            {
                base.ExcludeContentWebpartFromTrad(response, allWebPartsContentDisable, webPartIdHashTable);
            }
        }

        public override void UpdateMsoMenuWebPartMenu(StringBuilder response, string itemLanguageToRemove, string currentLanguage, bool allWebPartsContentDisable, Hashtable webPartIdHashTable)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "UpdateMsoMenuWebPartMenu", FullDetail))
            {
                base.UpdateMsoMenuWebPartMenu(response, itemLanguageToRemove, currentLanguage, allWebPartsContentDisable, webPartIdHashTable);
            }
        }

        public override void RemoveTranslatedWebPart(StringBuilder response, Guid webPartIdToRemove)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "RemoveTranslatedWebPart", FullDetail))
            {
                base.RemoveTranslatedWebPart(response, webPartIdToRemove);
            }
        }

        public override void RemoveUntil(StringBuilder response, string textToRemove, string until)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "RemoveUntil", FullDetail))
            {
                base.RemoveUntil(response, textToRemove, until);
            }
        }

        public override void Write(StringBuilder html, string languageCode, Stream responseStream, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, int extractorStatus, string url, int autocompletionStatus, bool mobilePage, License.LicenseType licenseType)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "Write", LowDetail))
            {
                base.Write(html, languageCode, responseStream, viewAllItemsInEveryLanguages, completingDictionaryMode, extractorStatus, url, autocompletionStatus, mobilePage, licenseType);
            }
        }

        public override void InitializeCache(SPContext current, string url, bool reloadCustomCache, bool reloadGlobalCache)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "InitializeCache", LowDetail))
            {
                base.InitializeCache(current, url, reloadCustomCache, reloadGlobalCache);
            }
        }

        public override string GetPageFromCache(string currentHashCode, string requestDigest, string viewState, string eventValidation, string userName, string currentUserId, ArrayList ctxIdList, string includeTimeValueDateTime, ArrayList imnTypeSmtpList, ArrayList removedTranslatedDivForCache, string accountName, StringCollection currentHashCodeArrayList, string url, int extractorStatus, string currentLcCode, string languageCode, ArrayList removedAlphaNoCacheDivForCache, string qLogEvent)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "GetPageFromCache", LowDetail))
            {
                return base.GetPageFromCache(currentHashCode, requestDigest, viewState, eventValidation, userName, currentUserId, ctxIdList, includeTimeValueDateTime, imnTypeSmtpList, removedTranslatedDivForCache, accountName, currentHashCodeArrayList, url, extractorStatus, currentLcCode, languageCode, removedAlphaNoCacheDivForCache, qLogEvent);
            }
        }

        public override void BuildPageForCache(StringBuilder tempResponse, string currentHashCode, string currentUserId, string userName, string includeTimeValueDateTime, string accountName, ArrayList removedTranslatedDivForCache, string language)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "BuildPageForCache", FullDetail))
            {
                base.BuildPageForCache(tempResponse, currentHashCode, currentUserId, userName, includeTimeValueDateTime, accountName, removedTranslatedDivForCache, language);
            }
        }

        public override void ConvertAsciiCode(StringBuilder tempResponse)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "ConvertAsciiCode", FullDetail))
            {
                base.ConvertAsciiCode(tempResponse);
            }
        }

        public override void ReplaceLinkedPagesUrl(StringBuilder tempResponse, string languageCode)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "ReplaceLinkedPagesUrl", FullDetail))
            {
                base.ReplaceLinkedPagesUrl(tempResponse, languageCode);
            }
        }

        public override void TranslateFromDictionary(StringBuilder tempResponse, string languageCode, string currentLcCode, int extractorStatus, string url)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "TranslateFromDictionary", LowDetail))
            {
                base.TranslateFromDictionary(tempResponse, languageCode, currentLcCode, extractorStatus, url);
            }
        }

        public override string GetPageHashCode(string languageCode, StringBuilder tempResponse, out string requestDigest, out string viewState, out string eventValidation, out string currentUserId, out string userName, out ArrayList ctxIdList, out string includeTimeValueDateTime, out ArrayList imnTypeSmtpList, out ArrayList removedTranslatedDivForCache, out string accountName, out string qLogEnv)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "GetPageHashCode", LowDetail))
            {
                return base.GetPageHashCode(languageCode, tempResponse, out requestDigest, out viewState, out eventValidation, out currentUserId, out userName, out ctxIdList, out includeTimeValueDateTime, out imnTypeSmtpList, out removedTranslatedDivForCache, out accountName, out qLogEnv);
            }
        }

        public override void AddMenuItem(StringBuilder tempResponse, string lang, string currentLcCode, bool completingDictionaryMode)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "AddMenuItem", FullDetail))
            {
                base.AddMenuItem(tempResponse, lang, currentLcCode, completingDictionaryMode);
            }
        }

        public override void AddCheckMarkToPersonalLanguageMenu(StringBuilder tempResponse, string lang)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "AddCheckMarkToPersonalLanguageMenu", FullDetail))
            {
                base.AddCheckMarkToPersonalLanguageMenu(tempResponse, lang);
            }
        }

        public override void DisableItemTrad(SPWeb objWeb, string webPartId)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "DisableItemTrad", FullDetail))
            {
                base.DisableItemTrad(objWeb, webPartId);
            }
        }

        public override void DisableItemTradFromList(SPWeb inputSpWeb, string listId, string url)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "DisableItemTradFromList", FullDetail))
            {
                base.DisableItemTradFromList(inputSpWeb, listId, url);
            }
        }

        public override void EnableItemTrad(SPWeb web, string webPartId)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "EnableItemTrad", FullDetail))
            {
                base.EnableItemTrad(web, webPartId);
            }
        }

        public override void EnableItemTradFromList(string listId, string url)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "EnableItemTradFromList", FullDetail))
            {
                base.EnableItemTradFromList(listId, url);
            }
        }

        public override bool IsPageToBeTranslated(string url)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "IsPageToBeTranslated", FullDetail))
            {
                return base.IsPageToBeTranslated(url);
            }
        }

        public override void AddPageUrlForExtractor(string url)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "AddPageUrlForExtractor", FullDetail))
            {
                base.AddPageUrlForExtractor(url);
            }
        }

        public override void ReloadWebpartProperties()
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "ReloadWebpartProperties", FullDetail))
            {
                base.ReloadWebpartProperties();
            }
        }

        public override void SwitchToLanguage(SPWeb web, string webPartId, string lang)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "SwitchToLanguage", FullDetail))
            {
                base.SwitchToLanguage(web, webPartId, lang);
            }
        }

        public override void DisableWebpartContentTrad(SPWeb web, string webPartId)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "DisableWebpartContentTrad", FullDetail))
            {
                base.DisableWebpartContentTrad(web, webPartId);
            }
        }

        public override void EnableWebpartContentTrad(SPWeb web, string webPartId)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "EnableWebpartContentTrad", FullDetail))
            {
                base.EnableWebpartContentTrad(web, webPartId);
            }
        }

        public override void HideFieldsForLinks(string listId, string url, string hide)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "HideFieldsForLinks", FullDetail))
            {
                base.HideFieldsForLinks(listId, url, hide);
            }
        }

        public override string AddToDictionary(string url, string defaultLang, string term, IAutomaticTranslation automaticTranslationPlugin)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "AddToDictionary", FullDetail))
            {
                return base.AddToDictionary(url, defaultLang, term, automaticTranslationPlugin);
            }
        }

        public override void ProcessTranslationExtractor(string url, string defaultLanguage)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "ProcessTranslationExtractor", FullDetail))
            {
                base.ProcessTranslationExtractor(url, defaultLanguage);
            }
        }

        public override void TranslateInplaceView(StringBuilder html, string languageCode, Stream responseStream, string siteUrl)
        {
            using (new Statistic(HttpContext.Current != null ? (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName] : null, "TranslateInplaceView", FullDetail))
            {
                base.TranslateInplaceView(html, languageCode, responseStream, siteUrl);
            }
        }
    }
}
