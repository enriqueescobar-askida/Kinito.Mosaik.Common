// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Dictionaries.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Dictionaries type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Alphamosaik.Common.SharePoint.Library;

namespace Translator.Common.Library
{
    public class Dictionaries : BaseStaticOverride<Dictionaries>
    {
        private readonly Dictionary<string, BaseDictionary> _dictionaries = new Dictionary<string, BaseDictionary>();
        private readonly Dictionary<string, BaseDictionary> _urlDictionaries = new Dictionary<string, BaseDictionary>();
        private BaseDictionary _webApplicationDictionary;
        private List<LanguageItem> _visibleLanguages;

        public Dictionaries()
        {
            _visibleLanguages = new List<LanguageItem>();
            Languages = new List<LanguageItem>();

            foreach (string language in Library.Languages.Instance.AllLanguages)
            {
                var languageItem = new LanguageItem(language, Library.Languages.Instance.GetLcid(language), language, string.Empty, false);

                Languages.Add(languageItem);
            }
        }

        public List<LanguageItem> VisibleLanguages
        {
            get
            {
                return _visibleLanguages;
            }

            set
            {
                _visibleLanguages = value;

                foreach (LanguageItem visibleLanguage in _visibleLanguages)
                {
                    for (int index = 0; index < Languages.Count; index++)
                    {
                        LanguageItem languageItem = Languages[index];
                        if (visibleLanguage.Lcid == languageItem.Lcid)
                        {
                            Languages[index] = visibleLanguage;
                            break;
                        }
                    }
                }
            }
        }

        public string DefaultLanguage { get; set; }

        public List<LanguageItem> Languages { get; private set; }

        public bool LanguageVisible(string languageCode)
        {
            foreach (LanguageItem visibleLanguage in VisibleLanguages)
            {
                if (visibleLanguage.LanguageDestination.IndexOf(languageCode, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return visibleLanguage.Visible;
                }
            }

            return false;
        }

        public void RegisterDictionaryAsRootForUrl(BaseDictionary standardDictionary, bool webApplicationDictionary)
        {
            if (webApplicationDictionary)
            {
                this._webApplicationDictionary = standardDictionary;
            }

            string key = standardDictionary.WebApplicationId + "_" + standardDictionary.SiteId + "_" + standardDictionary.WebId;

            _urlDictionaries[key] = standardDictionary;
        }

        public void RegisterDictionary(BaseDictionary standardDictionary)
        {
            if (standardDictionary != null)
                _dictionaries[standardDictionary.DictionaryName] = standardDictionary;
        }

        public BaseDictionary GetRootDictionary(Guid webApplicationId, Guid siteId, Guid webId)
        {
            string key = webApplicationId + "_" + siteId + "_" + webId;

            if (!_urlDictionaries.ContainsKey(key))
                return this._webApplicationDictionary;

            return _urlDictionaries[key];
        }

        public BaseDictionary GetRootDictionary(Guid webApplicationId, Guid siteId, Guid webId, bool returnWebApplicationIfNotFound)
        {
            string key = webApplicationId + "_" + siteId + "_" + webId;

            if (!_urlDictionaries.ContainsKey(key) && !returnWebApplicationIfNotFound)
                return null;

            return _urlDictionaries[key];
        }

        public bool RootDictionaryExist(Guid webApplicationId, Guid siteId, Guid webId)
        {
            string key = webApplicationId + "_" + siteId + "_" + webId;

            return _urlDictionaries.ContainsKey(key);
        }

        public BaseDictionary GetDictionary(Guid webApplicationId, Guid siteId, Guid webId, string dictionaryName)
        {
            string key = webApplicationId + "_" + siteId + "_" + webId + "_" + dictionaryName;

            if (!_dictionaries.ContainsKey(key))
                return this._webApplicationDictionary;

            return _dictionaries[key];
        }

        public BaseDictionary GetGlobalDictionary(Guid webApplicationId, string dictionaryName)
        {
            foreach (KeyValuePair<string, BaseDictionary> dictionary in _dictionaries)
            {
                if (dictionary.Key.Contains(webApplicationId.ToString()) && dictionary.Key.Contains(dictionaryName))
                {
                    return dictionary.Value;
                }
            }

            return this._webApplicationDictionary;
        }

        public BaseDictionary GetWebApplicationDictionary(Guid webApplicationId)
        {
            return this._webApplicationDictionary;
        }

        public bool DictionaryExist(Guid webApplicationId, Guid siteId, Guid webId, string dictionaryName)
        {
            string key = webApplicationId + "_" + siteId + "_" + webId + "_" + dictionaryName;

            return _dictionaries.ContainsKey(key);
        }

        public bool DictionaryExist(Guid webApplicationId, string dictionaryName)
        {
            foreach (KeyValuePair<string, BaseDictionary> dictionary in _dictionaries)
            {
                if (dictionary.Key.Contains(webApplicationId.ToString()) && dictionary.Key.Contains(dictionaryName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
