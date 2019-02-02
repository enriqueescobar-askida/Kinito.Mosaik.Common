// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseDictionary.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the BaseDictionary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;

namespace Translator.Common.Library
{
    using System;

    public abstract class BaseDictionary : ConnectedObject<BaseDictionary>, ICommonDictionary
    {
        // represent Dictionary<Hashcode, List<ID>>
        protected readonly IndexedList<int> Index;

        protected BaseDictionary(Guid webApplicationId, Guid siteId, Guid webId, string dictionaryName, string url, int initCount)
        {
            WebApplicationId = webApplicationId;
            SiteId = siteId;
            WebId = webId;
            DictionaryName = WebApplicationId + "_" + this.SiteId + "_" + this.WebId + "_" + dictionaryName;
            Index = new IndexedList<int>(initCount);
        }

        public string DictionaryName { get; set; }

        public Guid WebApplicationId { get; set; }

        public Guid SiteId { get; set; }

        public Guid WebId { get; set; }

        public static IEnumerable<CultureInfo> GetSiteLanguageInstalled(SPWeb web)
        {
            // Display the alternate languages for the Web site.
            if (web.IsMultilingual)
            {
                return web.SupportedUICultures;
            }

            var list = new List<CultureInfo>();

            foreach (LanguageItem languageItem in Dictionaries.Instance.VisibleLanguages)
            {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(languageItem.Lcid);

                list.Add(cultureInfo);
            }

            return list;
        }

        public abstract bool Translate(Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated);

        public bool Translate(SPContext current, string text, string languageSource, string languageDestination, out string translated)
        {
            return Translate(
                current.Site.WebApplication.Id,
                current.Site.ID,
                current.Web.ID,
                text,
                languageSource,
                languageDestination,
                out translated);
        }

        public virtual bool Translate(int hashCode, Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated)
        {
            List<int> list;

            if (Index.TryGetValue(hashCode, out list))
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

            translated = string.Empty;

            return false;
        }

        public bool ContainText(string text, string languageSource, string languageDestination, out string translated)
        {
            int hashCode = text.GetHashCode();

            List<int> list;

            if (Index.TryGetValue(hashCode, out list))
            {
                foreach (int id in list)
                {
                    if (ContainItem(id, text, languageSource, languageDestination, out translated))
                    {
                        return true;
                    }
                }
            }

            if (Connected != null)
            {
                return Connected.ContainText(text, languageSource, languageDestination, out translated);
            }

            translated = string.Empty;

            return false;
        }

        public abstract bool ContainItem(int id, string text, string languageSource, string languageDestination, out string translated);

        public virtual void Load(SPList list, string query, IEnumerable<CultureInfo> cultures)
        {
            Index.Clear();
        }

        public abstract bool TranslateItem(int id, Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated);
    }
}
