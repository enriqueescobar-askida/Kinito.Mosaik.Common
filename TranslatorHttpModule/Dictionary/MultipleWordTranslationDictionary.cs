// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultipleWordTranslationDictionary.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the MultipleWordTranslationDictionary type.
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

    public class MultipleWordTranslationDictionary : StandardDictionary
    {
        public MultipleWordTranslationDictionary(Guid webApplicationId, Guid siteId, Guid webId, string dictionaryName, string url, int initCount)
            : base(webApplicationId, siteId, webId, dictionaryName, url, initCount)
        {
        }

        public static new BaseDictionary LoadDictionary(string dictionaryName, string url, string listName, string query)
        {
            using (var site = new SPSite(url))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list = web.Lists.TryGetList(listName);

                    if (list != null)
                    {
                        IEnumerable<CultureInfo> cultures = GetSiteLanguageInstalled(web);

                        BaseDictionary dictionary = Dictionaries.Instance.DictionaryExist(site.WebApplication.Id, site.ID, web.ID, dictionaryName) ? Dictionaries.Instance.GetDictionary(site.WebApplication.Id, site.ID, web.ID, dictionaryName) : new MultipleWordTranslationDictionary(site.WebApplication.Id, site.ID, web.ID, dictionaryName, url, list.ItemCount);
                        dictionary.Load(list, query, cultures);

                        return dictionary;
                    }
                }
            }

            return null;
        }

        public override void Load(SPList list, string query, IEnumerable<CultureInfo> cultures)
        {
            Index.Clear();
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
                                                         foreach (SPListItem item in items)
                                                         {
                                                             var translationItems = new Dictionary<string, string>();

                                                             foreach (var cultureInfo in cultures)
                                                             {
                                                                 string twoLetterIsoLanguageName = Languages.Instance.GetBackwardCompatibilityLanguageCode(cultureInfo.TwoLetterISOLanguageName.ToUpper());

                                                                 if (item.Fields.ContainsField(twoLetterIsoLanguageName))
                                                                 {
                                                                     string valueOriginal = item[twoLetterIsoLanguageName] != null ? item[twoLetterIsoLanguageName].ToString() : string.Empty;
                                                                     string value = valueOriginal.Replace("$$SPS_URL:", string.Empty);

                                                                     int indexUrl = value.IndexOf("$$");

                                                                     if (indexUrl != -1)
                                                                     {
                                                                         value = value.Substring(indexUrl + 2);
                                                                     }

                                                                     if (!string.IsNullOrEmpty(value))
                                                                     {
                                                                         translationItems[twoLetterIsoLanguageName] = value;
                                                                         Index.IndexItem(item.ID, value);
                                                                     }
                                                                 }
                                                             }

                                                             Items[item.ID] = translationItems;
                                                         }
                                                     }, null);
                }
            }
        }
    }
}