// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultipleWordTranslationDictionaries.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the MultipleWordTranslationDictionaries type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

using Alphamosaik.Common.SharePoint.Library;

using Microsoft.SharePoint;

using Translator.Common.Library;

namespace TranslatorHttpHandler.Dictionary
{
    public class MultipleWordTranslationDictionaries : BaseDictionary
    {
        private readonly string _url;
        private readonly string _listName;

        // represent Dictionary<ID, Dictionary<LNG, VALUES>>
        private readonly Dictionary<string, BaseDictionary> _items;

        public MultipleWordTranslationDictionaries(Guid webApplicationId, Guid siteId, Guid webId, string dictionaryName, string url, int initCount, string listName)
            : base(webApplicationId, siteId, webId, dictionaryName, url, initCount)
        {
            _url = url;
            _listName = listName;
            _items = new Dictionary<string, BaseDictionary>(initCount);
        }

        public static BaseDictionary LoadDictionary(string dictionaryName, string url, string listName, string query)
        {
            using (var site = new SPSite(url))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list = web.Lists.TryGetList(listName);

                    if (list != null)
                    {
                        IEnumerable<CultureInfo> cultures = GetSiteLanguageInstalled(web);

                        MultipleWordTranslationDictionaries dictionary = Dictionaries.Instance.DictionaryExist(site.WebApplication.Id, site.ID, web.ID, dictionaryName) ? Dictionaries.Instance.GetDictionary(site.WebApplication.Id, site.ID, web.ID, dictionaryName) as MultipleWordTranslationDictionaries : new MultipleWordTranslationDictionaries(site.WebApplication.Id, site.ID, web.ID, dictionaryName, url, list.ItemCount, listName);
                        if (dictionary != null)
                        {
                            dictionary.Load(list, query, cultures);

                            if (dictionary._items.Count <= 0)
                            {
                                return null;
                            }

                            return dictionary;
                        }
                    }
                }
            }

            return null;
        }

        public override bool Translate(Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated)
        {
            Uri uri = HttpContext.Current.Request.Url;
            string fullUrl = uri.AbsoluteUri.ToLower();

            if (!string.IsNullOrEmpty(uri.Query))
            {
                fullUrl = fullUrl.Replace(uri.Query, string.Empty);
            }

            foreach (KeyValuePair<string, BaseDictionary> baseDictionary in _items)
            {
                if (fullUrl.Contains(baseDictionary.Key))
                {
                    if (baseDictionary.Value.Translate(webApplicationId, siteId, webId, text, languageSource, languageDestination, out translated))
                    {
                        return true;
                    }
                }
            }

            if (Connected != null)
            {
                return Connected.Translate(webApplicationId, siteId, webId, text, languageSource, languageDestination, out translated);
            }

            translated = string.Empty;

            return false;
        }

        public override bool ContainItem(int id, string text, string languageSource, string languageDestination, out string translated)
        {
            throw new NotImplementedException();
        }

        public override bool TranslateItem(int id, Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated)
        {
            throw new NotImplementedException();
        }

        public override void Load(SPList list, string query, IEnumerable<CultureInfo> cultures)
        {
            base.Load(list, query, cultures);

            _items.Clear();

            if (cultures != null)
            {
                if (list != null)
                {
                    var contentIterator = new ListContentIterator
                    {
                        StrictQuerySemantics = false
                    };

                    contentIterator.ProcessListItems(list, query, 200,
                                                     true,
                                                     delegate(SPListItemCollection items)
                                                     {
                                                         foreach (SPListItem item in items)
                                                         {
                                                             foreach (var cultureInfo in cultures)
                                                             {
                                                                 string twoLetterIsoLanguageName = Languages.Instance.GetBackwardCompatibilityLanguageCode(cultureInfo.TwoLetterISOLanguageName.ToUpper());

                                                                 if (item.Fields.ContainsField(twoLetterIsoLanguageName))
                                                                 {
                                                                     const string TagUrl = "$$SPS_URL:";

                                                                     string valueOriginal = item[twoLetterIsoLanguageName] != null ? item[twoLetterIsoLanguageName].ToString() : string.Empty;
                                                                     string siteUrl = valueOriginal.Replace(TagUrl, string.Empty);

                                                                     int indexUrl = siteUrl.IndexOf("$$");

                                                                     if (indexUrl != -1)
                                                                     {
                                                                         siteUrl = siteUrl.Substring(0, indexUrl).ToLower();
                                                                     }

                                                                     if (!string.IsNullOrEmpty(siteUrl))
                                                                     {
                                                                         if (!_items.ContainsKey(siteUrl))
                                                                         {
                                                                             string urlQuery = valueOriginal.Substring(0, siteUrl.Length + TagUrl.Length + 2);

                                                                             string defaultLanguage = query.Replace("<Where><Contains><FieldRef Name=\'", string.Empty);
                                                                             defaultLanguage = defaultLanguage.Substring(0, defaultLanguage.IndexOf("\'/><Value Type=\'Text\'>"));

                                                                             string subQuery = "<Where><Contains><FieldRef Name=\'" + defaultLanguage + "\'/><Value Type=\'Text\'>" + urlQuery + "</Value></Contains></Where>" + ListContentIterator.ItemEnumerationOrderById;
                                                                             var siteDictionary = MultipleWordTranslationDictionary.LoadDictionary(siteUrl, _url, _listName, subQuery);

                                                                             _items[siteUrl] = siteDictionary;
                                                                         }
                                                                     }
                                                                 }
                                                             }
                                                         }
                                                     }, null);
                }
            }
        }
    }
}