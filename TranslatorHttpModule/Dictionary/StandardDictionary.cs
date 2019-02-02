// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StandardDictionary.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StandardDictionary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using Alphamosaik.Common.SharePoint.Library;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace TranslatorHttpHandler.Dictionary
{
    using System;

    public class StandardDictionary : BaseDictionary
    {
        // represent Dictionary<ID, Dictionary<LNG, VALUE>>
        protected Dictionary<int, Dictionary<string, string>> Items;

        public StandardDictionary(Guid webApplicationId, Guid siteId, Guid webId, string dictionaryName, string url, int initCount)
            : base(webApplicationId, siteId, webId, dictionaryName, url, initCount)
        {
            Items = new Dictionary<int, Dictionary<string, string>>(initCount);
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

                        BaseDictionary dictionary = Dictionaries.Instance.DictionaryExist(site.WebApplication.Id, site.ID, web.ID, dictionaryName) ? Dictionaries.Instance.GetDictionary(site.WebApplication.Id, site.ID, web.ID, dictionaryName) : new StandardDictionary(site.WebApplication.Id, site.ID, web.ID, dictionaryName, url, list.ItemCount);

                        dictionary.Load(list, query, cultures);

                        return dictionary;
                    }
                }
            }

            return null;
        }

        public override bool Translate(Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated)
        {
            return Translate(text.GetHashCode(), webApplicationId, siteId, webId, text, languageSource, languageDestination, out translated);
        }

        public override bool ContainItem(int id, string text, string languageSource, string languageDestination, out string translated)
        {
            if (Items.ContainsKey(id))
            {
                string source;
                Items[id].TryGetValue(languageSource, out source);

                if (source.Equals(text))
                {
                    return Items[id].TryGetValue(languageDestination, out translated);
                }
            }

            translated = string.Empty;

            return false;
        }

        public override void Load(SPList list, string query, IEnumerable<CultureInfo> cultures)
        {
            base.Load(list, query, cultures);

            Items.Clear();

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
                                                         SPSecurity.RunWithElevatedPrivileges(delegate
                                                         {
                                                             foreach (SPListItem item in items)
                                                             {
                                                                 var translationItems = new Dictionary<string, string>();

                                                                 foreach (var cultureInfo in cultures)
                                                                 {
                                                                     string twoLetterIsoLanguageName = Languages.Instance.GetBackwardCompatibilityLanguageCode(cultureInfo.TwoLetterISOLanguageName.ToUpper());

                                                                     if (item.Fields.ContainsField(twoLetterIsoLanguageName))
                                                                     {
                                                                         string value = item[twoLetterIsoLanguageName] != null ? item[twoLetterIsoLanguageName].ToString() : string.Empty;

                                                                         if (!string.IsNullOrEmpty(value))
                                                                         {
                                                                             translationItems[twoLetterIsoLanguageName] = value;
                                                                             Index.IndexItem(item.ID, value);
                                                                         }
                                                                     }
                                                                 }

                                                                 Items[item.ID] = translationItems;
                                                             }
                                                         });
                                                     }, null);
                }
            }
        }

        public override bool TranslateItem(int id, Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated)
        {
            try
            {
                if (Items.ContainsKey(id))
                {
                    string source;
                    Items[id].TryGetValue(languageSource, out source);

                    if (!string.IsNullOrEmpty(source) && source.Equals(text))
                    {
                        return Items[id].TryGetValue(languageDestination, out translated);
                    }
                }

                translated = string.Empty;
            }
            catch (Exception ex)
            {
                translated = string.Empty;
            }

            return false;
        }
    }
}
