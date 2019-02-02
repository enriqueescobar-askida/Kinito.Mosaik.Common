// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorHelper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the TranslatorHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Alphamosaik.Common.Library.Licensing;
using Alphamosaik.Oceanik.Sdk;
using Microsoft.SharePoint;

namespace TranslatorHttpHandler.TranslatorHelper
{
    public abstract class TranslatorHelper : ITranslator
    {
        public abstract void AddHelper(StringBuilder tempResponse, string lang, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, string currentLcCode,
                                     ref bool filteringDisplayCompleted, bool mobilePage, License.LicenseType licenseType, ref Dictionary<int, string> excludedPartsFromTranslation);

        public abstract void ListFilteringDisplay(StringBuilder tempResponse, string lang, ref bool completed);

        public abstract void SaveWebpartProperties(StringBuilder tempResponse);

        public abstract void AddWebPartMenu(StringBuilder tempResponse, string lang);

        public abstract void AjaxFormating(StringBuilder tempResponse);

        public abstract void RedirectToLinkedPage(string language);

        public abstract void QuickLaunchFilter(StringBuilder tempResponse, string language);

        public abstract void TopNavigationBarFilter(StringBuilder tempResponse, string language);

        public abstract void RemoveTranslatedTr(StringBuilder response, string itemLanguageToRemove);

        public abstract void RemoveTranslatedDiv(StringBuilder response, string language);

        public abstract ArrayList RemoveTranslatedDivForCache(StringBuilder response, string cacheTag);

        public abstract Hashtable LoadWebPartHashTable(ref string spsWebpartLanguageList, ref bool spsContentExist, ref List<Guid> webPartListToRemove, string currentLanguage);

        public abstract void ExcludeContentWebpartFromTrad(StringBuilder response, bool allWebPartsContentDisable, Hashtable webPartIdHashTable);

        public abstract void UpdateMsoMenuWebPartMenu(StringBuilder response, string itemLanguageToRemove, string currentLanguage, bool allWebPartsContentDisable, Hashtable webPartIdHashTable);

        public abstract void RemoveTranslatedWebPart(StringBuilder response, Guid webPartIdToRemove);

        public abstract void RemoveUntil(StringBuilder response, string textToRemove, string until);

        public abstract void Write(StringBuilder html, string languageCode, Stream responseStream, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, int extractorStatus, string url, int autocompletionStatus, bool mobilePage, License.LicenseType licenseType);

        public abstract void InitializeCache(SPContext current, string url, bool reloadCustomCache, bool reloadGlobalCache);

        public abstract string GetPageFromCache(string currentHashCode, string requestDigest, string viewState, string eventValidation, string userName, string currentUserId, ArrayList ctxIdList, string includeTimeValueDateTime, ArrayList imnTypeSmtpList, ArrayList removedTranslatedDivForCache, string accountName, StringCollection currentHashCodeArrayList, string url, int extractorStatus, string currentLcCode, string languageCode, ArrayList removedAlphaNoCacheDivForCache, string qLogEvent);

        public abstract void BuildPageForCache(StringBuilder tempResponse, string currentHashCode, string currentUserId, string userName, string includeTimeValueDateTime, string accountName, ArrayList removedTranslatedDivForCache, string language);

        public abstract void ConvertAsciiCode(StringBuilder tempResponse);

        public abstract void ReplaceLinkedPagesUrl(StringBuilder tempResponse, string languageCode);

        public abstract void TranslateFromDictionary(StringBuilder tempResponse, string languageCode, string currentLcCode, int extractorStatus, string url);

        public abstract string GetPageHashCode(string languageCode, StringBuilder tempResponse, out string requestDigest, out string viewState, out string eventValidation, out string currentUserId, out string userName, out ArrayList ctxIdList, out string includeTimeValueDateTime, out ArrayList imnTypeSmtpList, out ArrayList removedTranslatedDivForCache, out string accountName, out string qLogEnv);

        public abstract void AddMenuItem(StringBuilder tempResponse, string lang, string currentLcCode, bool completingDictionaryMode);

        public abstract void AddCheckMarkToPersonalLanguageMenu(StringBuilder tempResponse, string lang);

        public abstract void DisableItemTrad(SPWeb objWeb, string webPartId);

        public abstract void DisableItemTradFromList(SPWeb inputSpWeb, string listId, string url);

        public abstract void EnableItemTrad(SPWeb web, string webPartId);

        public abstract void EnableItemTradFromList(string listId, string url);

        public abstract bool IsPageToBeTranslated(string url);

        public abstract void AddPageUrlForExtractor(string url);

        public abstract void ReloadWebpartProperties();

        public abstract void SwitchToLanguage(SPWeb web, string webPartId, string lang);

        public abstract void DisableWebpartContentTrad(SPWeb web, string webPartId);

        public abstract void EnableWebpartContentTrad(SPWeb web, string webPartId);

        public abstract void HideFieldsForLinks(string listId, string url, string hide);

        public abstract string AddToDictionary(string url, string defaultLang, string term, IAutomaticTranslation automaticTranslationPlugin);

        public abstract void ProcessTranslationExtractor(string url, string defaultLanguage);

        public abstract void TranslateInplaceView(StringBuilder html, string languageCode, Stream responseStream, string siteUrl);

        public abstract void RobotWrite(StringBuilder html, Stream responseStream, string siteUrl);

        public abstract string Translate(string source, string languageDestination);

        public abstract string Translate(StringBuilder source, string languageDestination);

        public abstract int GetLcid(string languageCode);

        public abstract string GetLanguageCode(string lcid);

        public abstract string GetLanguageCode(int lcid);

        public abstract string GetCurrrentLanguageCode();

        public abstract string GetWebPartLanguage(System.Web.UI.WebControls.WebParts.WebPart webPart);
    }
}
