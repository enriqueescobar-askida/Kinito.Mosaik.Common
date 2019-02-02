// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WildcardDictionary.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the WildcardDictionary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Alphamosaik.Common.SharePoint.Library;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace TranslatorHttpHandler.Dictionary
{
    public class WildcardDictionary : BaseDictionary
    {
        // represent Dictionary<ID, Dictionary<LNG, VALUES>>
        private readonly Dictionary<int, Dictionary<string, string[]>> _items;

        // represent Dictionary<Hashcode, List<ID>>
        private readonly IndexedList<int> _startingWildcardIndex;

        public WildcardDictionary(Guid webApplicationId, Guid siteId, Guid webId, string dictionaryName, string url, int initCount)
            : base(webApplicationId, siteId, webId, dictionaryName, url, initCount)
        {
            _items = new Dictionary<int, Dictionary<string, string[]>>(initCount);
            _startingWildcardIndex = new IndexedList<int>(initCount);
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

                        WildcardDictionary dictionary = Dictionaries.Instance.DictionaryExist(site.WebApplication.Id, site.ID, web.ID, dictionaryName) ? Dictionaries.Instance.GetDictionary(site.WebApplication.Id, site.ID, web.ID, dictionaryName) as WildcardDictionary : new WildcardDictionary(site.WebApplication.Id, site.ID, web.ID, dictionaryName, url, list.ItemCount);
                        
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
            if (string.IsNullOrEmpty(text))
            {
                translated = string.Empty;
                return false;
            }

            return Translate(text.GetHashCode(), webApplicationId, siteId, webId, text, languageSource, languageDestination, out translated);
        }

        public override bool Translate(int hashCode, Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated)
        {
            if (string.IsNullOrEmpty(text))
            {
                translated = string.Empty;
                return false;
            }

            List<int> list;

            if (Index.TryGetValue(text.Substring(0, 1).GetHashCode(), out list))
            {
                foreach (int id in list)
                {
                    if (TranslateItem(id, webApplicationId, siteId, webId, text, languageSource, languageDestination, out translated))
                    {
                        return true;
                    }
                }
            }

            if (Connected != null)
            {
                return Connected.Translate(hashCode, webApplicationId, siteId, webId, text, languageSource, languageDestination, out translated);
            }

            foreach (KeyValuePair<int, List<int>> keyValuePair in _startingWildcardIndex)
            {
                foreach (int id in keyValuePair.Value)
                {
                    if (TranslateItem(id, webApplicationId, siteId, webId, text, languageSource, languageDestination, out translated))
                    {
                        return true;
                    }
                }
            }

            translated = string.Empty;

            return false;
        }

        public override bool ContainItem(int id, string text, string languageSource, string languageDestination, out string translated)
        {
            translated = string.Empty;

            return false;
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
                                                             var translationItems = new Dictionary<string, string[]>();

                                                             foreach (var cultureInfo in cultures)
                                                             {
                                                                 string twoLetterIsoLanguageName = Languages.Instance.GetBackwardCompatibilityLanguageCode(cultureInfo.TwoLetterISOLanguageName.ToUpper());

                                                                 if (item.Fields.ContainsField(twoLetterIsoLanguageName))
                                                                 {
                                                                     string valueOriginal = item[twoLetterIsoLanguageName] != null ? item[twoLetterIsoLanguageName].ToString() : string.Empty;
                                                                     string value = valueOriginal.Replace("***", ";");

                                                                     if (!string.IsNullOrEmpty(value))
                                                                     {
                                                                         string[] subValues = value.Split(';');

                                                                         for (int i = 0; i < subValues.Length; i++)
                                                                         {
                                                                             subValues[i] = subValues[i].Trim();
                                                                         }

                                                                         translationItems[twoLetterIsoLanguageName] = subValues;

                                                                         if (valueOriginal.Substring(0, 1).Contains("*"))
                                                                         {
                                                                             _startingWildcardIndex.IndexItem(item.ID, valueOriginal.Substring(0, 1));
                                                                         }
                                                                         else
                                                                         {
                                                                             Index.IndexItem(item.ID, valueOriginal.Substring(0, 1));
                                                                         }
                                                                     }
                                                                 }
                                                             }

                                                             _items[item.ID] = translationItems;
                                                         }
                                                     }, null);
                }
            }
        }

        public override bool TranslateItem(int id, Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated)
        {
            if (_items.ContainsKey(id) && !string.IsNullOrEmpty(text))
            {
                string[] sourceItems;

                // Cas typique:
                // Hello***Word -> sourceItems[0] = "Hello"  et sourceItems[1] = "Word"
                // ***Word      -> sourceItems[0] = ""       et sourceItems[1] = "Word"
                // Hello***     -> sourceItems[0] = "Hello"  et sourceItems[1] = ""

                bool itemLanguageFound = _items[id].TryGetValue(languageSource, out sourceItems);
                bool firstPartOfStringFoundInDico = false;
                bool secondPartOfStringFoundInDico = false;

                if(itemLanguageFound)
                {
                    bool firstPartOfStringIsEmpty = true;
                    bool secondPartOfStringIsEmpty = true;

                    firstPartOfStringIsEmpty = string.IsNullOrEmpty(sourceItems[0]);
                    secondPartOfStringIsEmpty = string.IsNullOrEmpty(sourceItems[1]);

                    if (!firstPartOfStringIsEmpty || !secondPartOfStringIsEmpty)
                    {
                        if (!firstPartOfStringIsEmpty)
                        {
                            if ((text.IndexOf(sourceItems[0]) != -1) && (text.Substring(0, sourceItems[0].Length).Trim() == sourceItems[0]))
                            {
                                firstPartOfStringFoundInDico = true;
                            }
                        }

                        if (!secondPartOfStringIsEmpty)
                        {
                            if ((text.IndexOf(sourceItems[1]) != -1) && (text.Substring(text.Length - sourceItems[1].Length, sourceItems[1].Length).Trim() == sourceItems[1]))
                            {
                                secondPartOfStringFoundInDico = true;
                            }
                        }

                        bool wildcardMatchIsValid = true;

                        if( !firstPartOfStringIsEmpty && !firstPartOfStringFoundInDico ||
                            firstPartOfStringIsEmpty && firstPartOfStringFoundInDico)
                        {
                            wildcardMatchIsValid = false;
                        }
                        if (!secondPartOfStringIsEmpty && !secondPartOfStringFoundInDico ||
                            secondPartOfStringIsEmpty && secondPartOfStringFoundInDico)
                        {
                            wildcardMatchIsValid = false;
                        }
                             
                        if (!string.IsNullOrEmpty(text) && wildcardMatchIsValid)
                        {
                            text = text.Replace("&nbsp;", string.Empty).Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&amp;", "&");

                            string wordUnderStars = text.Substring(sourceItems[0].Length, text.Length - sourceItems[1].Length - sourceItems[0].Length);

                            // Verify wether the wordUnderStars is in the dictionary or not
                            string valueForStars = wordUnderStars.Trim();

                            if (!string.IsNullOrEmpty(valueForStars))
                            {
                                string valueForStarsTranslated;

                                BaseDictionary dictionary = Dictionaries.Instance.GetRootDictionary(webApplicationId, siteId, webId);

                                dictionary.Translate(webApplicationId, siteId, webId, valueForStars, languageSource, languageDestination, out valueForStarsTranslated);

                                if (string.IsNullOrEmpty(valueForStarsTranslated))
                                {
                                    valueForStarsTranslated = valueForStars;
                                }

                                valueForStarsTranslated = wordUnderStars.Replace(valueForStars, valueForStarsTranslated);

                                string[] translationItems;
                                if (_items[id].TryGetValue(languageDestination, out translationItems))
                                {
                                    string sourceString = sourceItems[0] + wordUnderStars;
                                    if (!secondPartOfStringIsEmpty)
                                    {
                                        sourceString += sourceItems[1];
                                    }

                                    string translatedString = string.Empty;

                                    // Cas: ***Word
                                    if (firstPartOfStringIsEmpty && !secondPartOfStringIsEmpty)
                                    {
                                        translatedString = valueForStarsTranslated + translationItems[0];
                                    }
                                    // Cas: Hello***
                                    else if (!firstPartOfStringIsEmpty && secondPartOfStringIsEmpty)
                                    {
                                        translatedString = translationItems[0] + valueForStarsTranslated;
                                    }
                                    // Cas: Hello***Word
                                    else if (!firstPartOfStringIsEmpty && !secondPartOfStringIsEmpty)
                                    {
                                        translatedString = translationItems[0] + valueForStarsTranslated + translationItems[1];
                                    }

                                    translated = text.Replace(sourceString, translatedString);

                                    translated = translated.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&quot;", "\"");
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            translated = string.Empty;

            return false;
        }
    }
}