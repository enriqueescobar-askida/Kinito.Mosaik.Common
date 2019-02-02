// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SPSDictionary.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SpsDictionary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using Alphamosaik.Oceanik.Sdk;
using Microsoft.SharePoint;
using Translator.Common.Library;
using System.Web;

namespace TranslatorHttpHandler.Dictionary
{
    public class SpsDictionary : IDisposable
    {
        public const string AlphaSeparator = "|ALPHA_SEP|";

        private const string ListName = "/Lists/TranslationContents";
        private const string QueryString = "<Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where>";
        private readonly IAutomaticTranslation _automaticTranslationPlugin;

        public SpsDictionary(IAutomaticTranslation automaticTranslationPlugin)
        {
            _automaticTranslationPlugin = automaticTranslationPlugin;
            WebUrl = SPContext.Current.Site.Url;
            Web = SPContext.Current.Site.OpenWeb();
            List = Web.GetList(ListName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpsDictionary"/> class. 
        /// </summary>
        /// <param name="webUrl">
        ///   url of the SPWeb containing the dictonary
        /// </param>
        /// <param name="automaticTranslationPlugin">
        /// Set web service to use if loaded
        /// </param>
        public SpsDictionary(string webUrl, IAutomaticTranslation automaticTranslationPlugin)
        {
            _automaticTranslationPlugin = automaticTranslationPlugin;
            using (var sysSite = new SPSite(webUrl))
            {
                Web = sysSite.OpenWeb();
            }
            
            WebUrl = webUrl;
            List = Web.GetList(ListName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpsDictionary"/> class. 
        /// </summary>
        /// <param name="web">
        /// SPWeb containing the dictionary
        /// </param>
        /// /// <param name="automaticTranslationPlugin">
        /// Set web service to use if loaded
        /// </param>
        public SpsDictionary(SPWeb web, IAutomaticTranslation automaticTranslationPlugin)
        {
            _automaticTranslationPlugin = automaticTranslationPlugin;
            Web = web;
            WebUrl = Web.Site.Url;
            List = Web.GetList(ListName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpsDictionary"/> class. 
        /// </summary>
        /// <param name="site">
        /// SPSite containing the dictionary
        /// </param>
        public SpsDictionary(SPSite site)
        {
            Web = site.OpenWeb();
            WebUrl = Web.Site.Url;
            List = Web.GetList(ListName);
        }

        ~SpsDictionary()
        {
            Dispose(false);
        }

        public enum ItemStatus
        {
            None,
            Existing,
            Updated,
            Inserted,
            Deleted
        }

        public string WebUrl { get; private set; }

        public SPWeb Web { get; private set; }

        public SPList List { get; private set; }

        /// <summary>
        /// Get an item list match the term in the language specified
        /// </summary>
        /// <param name="term">Term to search.</param>
        /// <param name="language">The Language parameter.</param>
        /// <returns>Return a collection of SPListItemCollection.</returns>
        public SPListItemCollection GetTerms(string term, string language)
        {
            return List.GetItems(TermQuery(term, language));
        }

        /// <summary>
        /// Add a term to the dictionary
        /// </summary>
        /// <param name="term">Term to add</param>
        /// <param name="defaultLang">Default language</param>
        /// <returns>ItemStatus after the add</returns>
        public ItemStatus AddTerm(string term, string defaultLang)
        {
            try
            {
                SPListItemCollection collListItems = GetTerms(term, defaultLang);

                var resultQueryList = new ArrayList();

                if (collListItems.Count > 0)
                {
                    if (List.Fields.ContainsField(defaultLang))
                        foreach (SPListItem currentItem in collListItems)
                        {
                            if (term.Trim() == currentItem[defaultLang].ToString().Trim())
                            {
                                resultQueryList.Add(currentItem);
                            }
                        }
                }

                ItemStatus itemStatus = ItemStatus.None;

                if (resultQueryList.Count > 0)
                {
                    foreach (SPListItem currentItem in resultQueryList)
                    {
                        foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                        {
                            if (currentItem[languageItem.LanguageDestination] == null ||
                                string.IsNullOrEmpty(currentItem[languageItem.LanguageDestination].ToString()))
                            {
                                if (_automaticTranslationPlugin != null)
                                {
                                    TranslationUserAccount translationUserAccount = _automaticTranslationPlugin.LoadUserAccount(Web, null);
                                    currentItem[languageItem.LanguageDestination] = "SPS_ADDED_" + _automaticTranslationPlugin.TranslateText(term.Trim(), defaultLang, languageItem.LanguageDestination, null, translationUserAccount, false);
                                }
                                else
                                {
                                    currentItem[languageItem.LanguageDestination] = "SPS_ADDED_" + term.Trim();
                                }
                                
                                itemStatus = ItemStatus.Updated;
                            }
                        }

                        if (itemStatus == ItemStatus.Updated)
                        {
                            if (List.Fields.ContainsField("isCustomize"))
                                currentItem["isCustomize"] = true;

                            Web.AllowUnsafeUpdates = true;
                            currentItem.SystemUpdate(false);
                            Web.AllowUnsafeUpdates = false;

                            return itemStatus;
                        }

                        break;
                    }

                    return ItemStatus.Existing;
                }

                SPListItem newItem = List.Items.Add();

                if (List.Fields.ContainsField("isCustomize"))
                    newItem["isCustomize"] = true;

                if (List.Fields.ContainsField(defaultLang))
                    newItem[defaultLang] = "SPS_ADDED_" + term.Trim();

                foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
                {
                    if (List.Fields.ContainsField(languageItem.LanguageDestination))
                    {
                        if (_automaticTranslationPlugin != null)
                        {
                            TranslationUserAccount translationUserAccount = _automaticTranslationPlugin.LoadUserAccount(Web, null);
                            newItem[languageItem.LanguageDestination] = "SPS_ADDED_" + _automaticTranslationPlugin.TranslateText(term.Trim(), defaultLang, languageItem.LanguageDestination, null, translationUserAccount, false);
                        }
                        else
                        {
                            newItem[languageItem.LanguageDestination] = "SPS_ADDED_" + term.Trim();
                        }
                        
                        itemStatus = ItemStatus.Inserted;
                    }
                }

                Web.AllowUnsafeUpdates = true;
                newItem.SystemUpdate(false);
                Web.AllowUnsafeUpdates = false;

                return itemStatus;
            }
            catch (Exception e)
            {
                Utilities.LogException("AddToDictionary", e, EventLogEntryType.Warning);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Web != null)
            {
                Web.Dispose();
            }
        }

        /// <summary>
        /// Construct a SPQuery to search for a term in a specified language
        /// </summary>
        /// <param name="term">
        /// The term parameter.
        /// </param>
        /// <param name="language">
        /// The language parameter.
        /// </param>
        /// <returns>
        /// Return a SPQuery.
        /// </returns>
        protected SPQuery TermQuery(string term, string language)
        {
            return new SPQuery
            {
                Query = string.Format(QueryString, language, term.Trim()),
                QueryThrottleMode = SPQueryThrottleOption.Override
            };
        }
    }
}
