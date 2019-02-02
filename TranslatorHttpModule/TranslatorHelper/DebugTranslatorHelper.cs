// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugTranslatorHelper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DebugTranslatorHelper type.
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
using Translator.Common.Library.DebuggingHelper;

namespace TranslatorHttpHandler.TranslatorHelper
{
    public class DebugTranslatorHelper : TranslatorHelper
    {
        private readonly TranslatorHelper _currentHelper;

        public DebugTranslatorHelper(TranslatorHelper currentHelper)
        {
            _currentHelper = currentHelper;
        }

        public override void AddHelper(StringBuilder tempResponse, string lang, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, string currentLcCode, ref bool filteringDisplayCompleted, bool mobilePage, License.LicenseType licenseType, ref Dictionary<int, string> excludedPartsFromTranslation)
        {
            _currentHelper.AddHelper(tempResponse, lang, viewAllItemsInEveryLanguages, completingDictionaryMode, currentLcCode, ref filteringDisplayCompleted, mobilePage, licenseType, ref excludedPartsFromTranslation);
        }

        public override void ListFilteringDisplay(StringBuilder tempResponse, string lang, ref bool completed)
        {
            _currentHelper.ListFilteringDisplay(tempResponse, lang, ref completed);
        }

        public override void SaveWebpartProperties(StringBuilder tempResponse)
        {
            _currentHelper.SaveWebpartProperties(tempResponse);
        }

        public override void AddWebPartMenu(StringBuilder tempResponse, string lang)
        {
            _currentHelper.AddWebPartMenu(tempResponse, lang);
        }

        public override void AjaxFormating(StringBuilder tempResponse)
        {
            _currentHelper.AjaxFormating(tempResponse);
        }

        public override void RedirectToLinkedPage(string language)
        {
            _currentHelper.RedirectToLinkedPage(language);
        }

        public override void QuickLaunchFilter(StringBuilder tempResponse, string language)
        {
            _currentHelper.QuickLaunchFilter(tempResponse, language);
        }

        public override void TopNavigationBarFilter(StringBuilder tempResponse, string language)
        {
            _currentHelper.TopNavigationBarFilter(tempResponse, language);
        }

        public override void RemoveTranslatedTr(StringBuilder response, string itemLanguageToRemove)
        {
            _currentHelper.RemoveTranslatedTr(response, itemLanguageToRemove);
        }

        public override void RemoveTranslatedDiv(StringBuilder response, string language)
        {
            _currentHelper.RemoveTranslatedDiv(response, language);
        }

        public override ArrayList RemoveTranslatedDivForCache(StringBuilder response, string cacheTag)
        {
            return _currentHelper.RemoveTranslatedDivForCache(response, cacheTag);
        }

        public override Hashtable LoadWebPartHashTable(ref string spsWebpartLanguageList, ref bool spsContentExist, ref List<Guid> webPartListToRemove, string currentLanguage)
        {
            return _currentHelper.LoadWebPartHashTable(ref spsWebpartLanguageList, ref spsContentExist,
                                                       ref webPartListToRemove, currentLanguage);
        }

        public override void ExcludeContentWebpartFromTrad(StringBuilder response, bool allWebPartsContentDisable, Hashtable webPartIdHashTable)
        {
            _currentHelper.ExcludeContentWebpartFromTrad(response, allWebPartsContentDisable, webPartIdHashTable);
        }

        public override void UpdateMsoMenuWebPartMenu(StringBuilder response, string itemLanguageToRemove, string currentLanguage, bool allWebPartsContentDisable, Hashtable webPartIdHashTable)
        {
            _currentHelper.UpdateMsoMenuWebPartMenu(response, itemLanguageToRemove, currentLanguage,
                                                    allWebPartsContentDisable, webPartIdHashTable);
        }

        public override void RemoveTranslatedWebPart(StringBuilder response, Guid webPartIdToRemove)
        {
            _currentHelper.RemoveTranslatedWebPart(response, webPartIdToRemove);
        }

        public override void RemoveUntil(StringBuilder response, string textToRemove, string until)
        {
            _currentHelper.RemoveUntil(response, textToRemove, until);
        }

        public override void Write(StringBuilder html, string languageCode, Stream responseStream, bool viewAllItemsInEveryLanguages, bool completingDictionaryMode, int extractorStatus, string url, int autocompletionStatus, bool mobilePage, License.LicenseType licenseType)
        {
            using (var debug = new DebugHtmlToList(html, url))
            {
                _currentHelper.Write(html, languageCode, responseStream, viewAllItemsInEveryLanguages,
                                     completingDictionaryMode, extractorStatus, url, autocompletionStatus, mobilePage,
                                     licenseType);

                debug.Html = html.ToString();
            }
        }

        public override void InitializeCache(SPContext current, string url, bool reloadCustomCache, bool reloadGlobalCache)
        {
            _currentHelper.InitializeCache(current, url, reloadCustomCache, reloadGlobalCache);
        }

        public override string GetPageFromCache(string currentHashCode, string requestDigest, string viewState, string eventValidation, string userName, string currentUserId, ArrayList ctxIdList, string includeTimeValueDateTime, ArrayList imnTypeSmtpList, ArrayList removedTranslatedDivForCache, string accountName, StringCollection currentHashCodeArrayList, string url, int extractorStatus, string currentLcCode, string languageCode, ArrayList removedAlphaNoCacheDivForCache, string qLogEvent)
        {
            return _currentHelper.GetPageFromCache(currentHashCode, requestDigest, viewState, eventValidation, userName,
                                                   currentUserId, ctxIdList, includeTimeValueDateTime, imnTypeSmtpList,
                                                   removedTranslatedDivForCache, accountName, currentHashCodeArrayList,
                                                   url, extractorStatus, currentLcCode, languageCode, removedAlphaNoCacheDivForCache, qLogEvent);
        }

        public override void BuildPageForCache(StringBuilder tempResponse, string currentHashCode, string currentUserId, string userName, string includeTimeValueDateTime, string accountName, ArrayList removedTranslatedDivForCache, string language)
        {
            _currentHelper.BuildPageForCache(tempResponse, currentHashCode, currentUserId, userName,
                                             includeTimeValueDateTime, accountName, removedTranslatedDivForCache,
                                             language);
        }

        public override void ConvertAsciiCode(StringBuilder tempResponse)
        {
            _currentHelper.ConvertAsciiCode(tempResponse);
        }

        public override void ReplaceLinkedPagesUrl(StringBuilder tempResponse, string languageCode)
        {
            _currentHelper.ReplaceLinkedPagesUrl(tempResponse, languageCode);
        }

        public override void TranslateFromDictionary(StringBuilder tempResponse, string languageCode, string currentLcCode, int extractorStatus, string url)
        {
            _currentHelper.TranslateFromDictionary(tempResponse, languageCode, currentLcCode, extractorStatus, url);
        }

        public override string GetPageHashCode(string languageCode, StringBuilder tempResponse, out string requestDigest, out string viewState, out string eventValidation, out string currentUserId, out string userName, out ArrayList ctxIdList, out string includeTimeValueDateTime, out ArrayList imnTypeSmtpList, out ArrayList removedTranslatedDivForCache, out string accountName, out string qLogEnv)
        {
            return _currentHelper.GetPageHashCode(languageCode, tempResponse, out requestDigest, out viewState,
                                                  out eventValidation, out currentUserId, out userName, out ctxIdList,
                                                  out includeTimeValueDateTime, out imnTypeSmtpList,
                                                  out removedTranslatedDivForCache, out accountName, out qLogEnv);
        }

        public override void AddMenuItem(StringBuilder tempResponse, string lang, string currentLcCode, bool completingDictionaryMode)
        {
            _currentHelper.AddMenuItem(tempResponse, lang, currentLcCode, completingDictionaryMode);
        }

        public override void AddCheckMarkToPersonalLanguageMenu(StringBuilder tempResponse, string lang)
        {
            _currentHelper.AddCheckMarkToPersonalLanguageMenu(tempResponse, lang);
        }

        public override void DisableItemTrad(SPWeb objWeb, string webPartId)
        {
            _currentHelper.DisableItemTrad(objWeb, webPartId);
        }

        public override void DisableItemTradFromList(SPWeb inputSpWeb, string listId, string url)
        {
            _currentHelper.DisableItemTradFromList(inputSpWeb, listId, url);
        }

        public override void EnableItemTrad(SPWeb web, string webPartId)
        {
            _currentHelper.EnableItemTrad(web, webPartId);
        }

        public override void EnableItemTradFromList(string listId, string url)
        {
            _currentHelper.EnableItemTradFromList(listId, url);
        }

        public override bool IsPageToBeTranslated(string url)
        {
            return _currentHelper.IsPageToBeTranslated(url);
        }

        public override void AddPageUrlForExtractor(string url)
        {
            _currentHelper.AddPageUrlForExtractor(url);
        }

        public override void ReloadWebpartProperties()
        {
            _currentHelper.ReloadWebpartProperties();
        }

        public override void SwitchToLanguage(SPWeb web, string webPartId, string lang)
        {
            _currentHelper.SwitchToLanguage(web, webPartId, lang);
        }

        public override void DisableWebpartContentTrad(SPWeb web, string webPartId)
        {
            _currentHelper.DisableWebpartContentTrad(web, webPartId);
        }

        public override void EnableWebpartContentTrad(SPWeb web, string webPartId)
        {
            _currentHelper.EnableWebpartContentTrad(web, webPartId);
        }

        public override void HideFieldsForLinks(string listId, string url, string hide)
        {
            _currentHelper.HideFieldsForLinks(listId, url, hide);
        }

        public override string AddToDictionary(string url, string defaultLang, string term, IAutomaticTranslation automaticTranslationPlugin)
        {
            return _currentHelper.AddToDictionary(url, defaultLang, term, automaticTranslationPlugin);
        }

        public override void ProcessTranslationExtractor(string url, string defaultLanguage)
        {
            _currentHelper.ProcessTranslationExtractor(url, defaultLanguage);
        }

        public override void TranslateInplaceView(StringBuilder html, string languageCode, Stream responseStream, string siteUrl)
        {
            using (var debug = new DebugHtmlToList(html, siteUrl))
            {
                _currentHelper.TranslateInplaceView(html, languageCode, responseStream, siteUrl);

                debug.Html = html.ToString();
            }
        }

        public override void RobotWrite(StringBuilder html, Stream responseStream, string siteUrl)
        {
            using (var debug = new DebugHtmlToList(html, siteUrl))
            {
                _currentHelper.RobotWrite(html, responseStream, siteUrl);

                debug.Html = html.ToString();
            }
        }

        public override string Translate(string source, string languageDestination)
        {
            return _currentHelper.Translate(source, languageDestination);
        }

        public override string Translate(StringBuilder source, string languageDestination)
        {
            return _currentHelper.Translate(source, languageDestination);
        }

        public override int GetLcid(string languageCode)
        {
            return _currentHelper.GetLcid(languageCode);
        }

        public override string GetLanguageCode(string lcid)
        {
            return _currentHelper.GetLanguageCode(lcid);
        }

        public override string GetLanguageCode(int lcid)
        {
            return _currentHelper.GetLanguageCode(lcid);
        }

        public override string GetCurrrentLanguageCode()
        {
            return _currentHelper.GetCurrrentLanguageCode();
        }

        public override string GetWebPartLanguage(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            return _currentHelper.GetWebPartLanguage(webPart);
        }
    }
}
