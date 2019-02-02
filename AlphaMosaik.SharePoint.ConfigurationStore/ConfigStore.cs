// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigStore.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Utility class for retrieving configuration values within a SharePoint site. Where multiple values need to
//   be retrieved, the GetMultipleValues() method should be used where possible.
//   Created by Chris O'Brien (www.sharepointnutsandbolts.com).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AlphaMosaik.SharePoint.ConfigurationStore
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Caching;

    using Microsoft.SharePoint;

    /// <summary>
    /// Utility class for retrieving configuration values within a SharePoint site. Where multiple values need to
    /// be retrieved, the GetMultipleValues() method should be used where possible.
    /// Created by Chris O'Brien (www.sharepointnutsandbolts.com).
    /// </summary>
    public class ConfigStore
    {
        #region -- Private fields --

        private static readonly AppSettingsReader m_Reader = new AppSettingsReader();
            
        private const string m_GlobalConfigSiteAppSettingsKey = "ConfigSiteUrl";
        private const string m_GlobalConfigWebAppSettingsKey = "ConfigWebName";
        private const string m_GlobalConfigListAppSettingsKey = "ConfigListName";
        private const string m_CacheFilePathKey = "ConfigStoreCacheDependencyFile";
        
        private const string m_DefaultListName = "Configuration Store";

        private static readonly TraceSwitch traceSwitch = new TraceSwitch("AlphaMosaik.SharePoint.ConfigurationStore",
            "Trace switch for Config Store");

        private static readonly ConfigTraceHelper trace = new ConfigTraceHelper("AlphaMosaik.SharePoint.ConfigurationStore");

        #endregion

        #region -- Constructor (private)

        private ConfigStore() 
        {
        }

        #endregion

        #region -- Public fields --

        public static readonly string CategoryField = "ConfigCategory";
        public static readonly string KeyField = "Title";
        public static readonly string ValueField = "ConfigValue";
        public static readonly string CacheDependencyFilePath = (string)m_Reader.GetValue(m_CacheFilePathKey, typeof(string));

        public enum ConfigStoreType
        {
            Local,
            Global
        }

        #endregion

        /// <summary>
        /// Retrieves a single value from the config store list.
        /// </summary>
        /// <param name="Category">Category of the item to retrieve.</param>
        /// <param name="Key">Key (item name) of the item to retrieve.</param>
        /// <returns>The config item's value.</returns>
        public static string GetValue(string Category, string Key)
        {
            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Entered with Category '{0}' and Key '{1}', values will be trimmed.", 
                Category, Key);

            // first let's trim the supplied values..
            Category = Category.Trim();
            Key = Key.Trim();

            bool bListFoundFromSPContext = true;
            string sValue = null;
            
            // attempt retrieval from cache..
            //HttpContext httpCtxt = HttpContext.Current;
            //SPContext spCtxt = SPContext.Current;
            //string sSiteCacheKey = getSiteCacheKey(spCtxt);

            //string sCacheKey = GetCacheKey(Category, Key, sSiteCacheKey);

            //if (httpCtxt != null)
            //{
            //    // first check memory cache for current site's Config Store..
            //    string sCachedValue = httpCtxt.Cache[sCacheKey] as string;
                
            //    if (!string.IsNullOrEmpty(sCachedValue))
            //    {
            //        // ensure the correct behaviour is traced here..
            //        if (traceSwitch.TraceInfo)
            //        {
            //            if (spCtxt == null)
            //            {
            //                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Found value '{0}' in memory cache for global " +
            //                    "Config Store using cache key '{1}' - no SPContext so not checking cache for any cache for any 'local' Config Store.", 
            //                    sCachedValue, sCacheKey);
            //            }
            //            else
            //            {
            //                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Found value '{0}' in memory cache for current site's " +
            //                    "Config Store using cache key '{1}'.", sCachedValue, sCacheKey);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        // if no value found, now check memory cache for global Config Store..
            //        sCacheKey = GetCacheKey(Category, Key, null);
            //        sCachedValue = httpCtxt.Cache[sCacheKey] as string;

            //        if (!string.IsNullOrEmpty(sCachedValue))
            //        {
            //            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Found value '{0}' in memory cache for global " +
            //            "Config Store using cache key '{1}'.", sCachedValue, sCacheKey);
            //        }
            //    }

            //    if (!string.IsNullOrEmpty(sCachedValue))
            //    {
            //        trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Returning '{0}'.", sCachedValue);
            //        return sCachedValue;
            //    }
            //    else
            //    {
            //        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Value not found in memory cache, query will be executed.");
            //    }
            //}

            // no value found, proceed with query..
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                SPQuery query = getSingleItemQuery(Category, Key);
                
                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get list from context.");
                SPList configStoreList = null;
                try
                {
                    configStoreList = attemptGetLocalConfigStoreListFromContext();
                    if (configStoreList != null)
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection has local Config Store, will " +
                            "first attempt query against this list - '{0}'.", configStoreList.DefaultViewUrl);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No local config store list found in current site collection.");
                }

                if (configStoreList == null)
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection does not have local Config Store, falling " +
                        "back to global Config Store.");
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get config store from config.");
                    bListFoundFromSPContext = false;
                    configStoreList = attemptGetGlobalConfigStoreListFromConfig();
                }

                // will have list or have thrown exception by now..               
                try
                {
                    sValue = executeSingleItemQuery(Category, Key, configStoreList, query, false);
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Retrieved value '{0}' from Config Store '{1}' for " +
                        "category '{2}' and key '{3}'.", sValue, configStoreList.DefaultViewUrl, Category, Key);
                }
                finally
                {
                    if (!bListFoundFromSPContext)
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Disposing SPSite and SPWeb objects.");

                        // disposals are required.. 
                        configStoreList.ParentWeb.Site.Dispose();
                        configStoreList.ParentWeb.Dispose();
                    }
                    else
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No disposals required, list found from SPContext.");
                    }
                }

                if (string.IsNullOrEmpty(sValue))
                {
                    // now try query against global Config Store..
                    if (bListFoundFromSPContext && !IsGlobalConfigStore(configStoreList))
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No value found in local Config Store, now looking " +
                            "up config item for category '{0}' and key '{1}' in global Config Store.", 
                            Category, Key);

                        configStoreList = attemptGetGlobalConfigStoreListFromConfig();
                        bListFoundFromSPContext = false;
                        sValue = executeSingleItemQuery(Category, Key, configStoreList, query, true);
                    }
                }

                // finally if we found a value let's cache it..
                //if (!string.IsNullOrEmpty(sValue))
                //{
                //    // add to cache..
                //    sCacheKey = (bListFoundFromSPContext && !IsGlobalConfigStore(configStoreList)) ? GetCacheKey(Category, Key, spCtxt.Site.Url) :
                //        GetCacheKey(Category, Key, null);
                //    ConfigStore.CacheConfigStoreItem(sValue, sCacheKey);
                //}
                //else
                //{
                //    string sMessage = string.Format("No config item was found for category '{0}' and key '{1}'.", Category, Key);
                //    trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "GetValue(): No value found - throwing exception - '{0}'.",
                //        sMessage);
                //    throw new InvalidConfigurationException(sMessage);
                //}
            });

            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Returning '{0}'.", sValue);

            return sValue;
        }

        /// <summary>
        /// Adds an item to the configuration store.
        /// </summary>
        /// <param name="title">Name of the config item to add</param>
        /// <param name="category">Category of the config item to add</param>
        /// <param name="value">Value of the config item to add</param>
        /// <param name="description">Description of the config item to add</param>
        /// <param name="overwrite">Overwrite the item if it's already present in the configuration store</param>
        public static void AddValue(string title, string category, string value, string description, bool overwrite)
        {
            SPList configStoreList = attemptGetLocalConfigStoreListFromContext() ??
                                     attemptGetGlobalConfigStoreListFromConfig();

            // Est-ce que l'item est déjà présent?
            SPQuery query = getSingleItemQuery(category, title);
            SPListItemCollection items = configStoreList.GetItems(query);
            
            if (items.Count > 0) // Ok y'en a plus qu'un, qu'est-ce qu'on fait?
            {
                if (overwrite)
                {
                    // Si y'en a plus qu'un, ça va mal en partant: on va tous les effacer
                    foreach (SPListItem spListItem in items)
                    {
                        configStoreList.Items.DeleteItemById(spListItem.ID);
                    }
                }
                else
                {
                    // Y'en a au moins déjà un mais on a comme instruction de rien faire.
                    return;
                }
            }

            // On ajoute l'item
            SPItem item = configStoreList.Items.Add();
            item["Title"] = title;
            item["ConfigCategory"] = category;
            item["ConfigValue"] = value;
            item["ConfigItemDescription"] = description;

            item.Update();
        }

        private static string getSiteCacheKey(SPContext context)
        {
            string sSiteCacheKey = null;
            if (context != null)
            {
                sSiteCacheKey = context.Site.Url;
            }
            return sSiteCacheKey;
        }

        private static SPQuery getSingleItemQuery(string Category, string Key)
        {
            SPQuery query = new SPQuery();
            query.Query = string.Format("<Where><And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"Text\">{1}</Value></Eq><Eq><FieldRef Name=\"{2}\" /><Value Type=\"Text\">{3}</Value></Eq></And></Where>",
                 ConfigStore.CategoryField, Category, ConfigStore.KeyField, Key);
            query.ViewFields = string.Format("<FieldRef Name=\"{0}\" />", ConfigStore.ValueField);
            return query;
        }

        private static string executeSingleItemQuery(string Category, string Key, SPList configStoreList, SPQuery query, bool bThrowOnNoResults)
        {
            string sValue = null;

            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Executing query '{0}'.", query.Query);
            SPListItemCollection items = configStoreList.GetItems(query);

            if (items.Count == 1)
            {
                sValue = items[0][0].ToString();
                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Found '{0}' as config value for Category '{1}' " +
                   "and Key '{2}'.", sValue, Category, Key);
            }
            else if (items.Count > 1)
            {
                string sMessage =
                    string.Format("Multiple config items were found for the requested item. Please check " +
                                  "config store settings list for category '{0}' and key '{1}'.", Category, Key);
                trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): Multiple config values found for the requested Category/Key! Throwing " +
                    "exception - '{0}'.", sMessage);
                throw new InvalidConfigurationException(sMessage);
            }
            else if (items.Count == 0)
            {
                string sMessage = string.Format("No config item was found for category '{0}' and key '{1}'.", Category, Key);
                if (bThrowOnNoResults)
                {
                    trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): No value found - throwing exception - '{0}'.",
                        sMessage);
                    throw new InvalidConfigurationException(sMessage);
                }
                else
                {
                    trace.WriteLineIf(traceSwitch.TraceWarning, TraceLevel.Warning, "executeSingleItemQuery(): No config value found for category '{0}' " +
                        "and key '{1}'. Returning null, but not throwing exception since caller specified not to.",
                        Category, Key);
                }
            }

            return sValue;
        }

        #region -- GetMultipleValues() --

        /// <summary>
        /// Retrieves multiple config values with a single query. 
        /// </summary>
        /// <param name="ConfigIdentifiers">List of ConfigIdentifier objects to retrieve.</param>
        /// <returns>A Dictionary object containing the requested config values. Items are keyed by ConfigIdentifier.</returns>
        public static Dictionary<ConfigIdentifier, string> GetMultipleValues(List<ConfigIdentifier> ConfigIdentifiers)
        {
            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetMultipleValues(): Entered with '{0}' config values requested.",
                ConfigIdentifiers.Count);

            // first let's trim the supplied values..
            trimDictionaryEntries(ConfigIdentifiers);

            var configDictionary = new Dictionary<ConfigIdentifier, string>();

            // attempt retrieval from cache..
            HttpContext httpCtxt = HttpContext.Current;
            SPContext spCtxt = SPContext.Current;
            string sSiteCacheKey = getSiteCacheKey(spCtxt);

            if (httpCtxt != null)
            {
                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Have HttpContext, checking memory cache for config values.");
                string sCacheKey = null;
                string sCachedValue = null;
                bool bFoundAllValuesInCache = true;

                foreach (ConfigIdentifier configID in ConfigIdentifiers)
                {
                    sCacheKey = GetCacheKey(configID.Category, configID.Key, sSiteCacheKey);
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Checking in memory for config value with Category '{0}' and " +
                        "Key '{1}'. Cache key '{2}' will be used.", configID.Category, configID.Key, sCacheKey);
                    sCachedValue = httpCtxt.Cache[sCacheKey] as string;

                    if (sCachedValue != null)
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Found value '{0}' in memory for cache key '{1}'. " +
                            "Adding to dictionary.", sCachedValue, sCacheKey);
                        configDictionary.Add(configID, sCachedValue);
                    }
                    else
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Did not find value for cache key '{0}' in memory " +
                            "Query will be executed, not checking memory for any further values.", sCacheKey);
                        bFoundAllValuesInCache = false;
                        break;
                    }
                }

                if (bFoundAllValuesInCache)
                {
                    trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetMultipleValues(): Found all values in memory, returning dictionary.");
                    return configDictionary;
                }
                else
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Clearing dictionary prior to query.");
                    // clear the dictionary since we'll add fresh config items to it from our query..
                    configDictionary.Clear();
                }
            }

            // no value found, proceed with query..
            if (ConfigIdentifiers.Count < 2)
            {
                const string sMessage = "Invalid use of config store - the GetMultipleValues() method " +
                                        "must only be used to retrieve multiple config values.";
                trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "GetMultipleValues(): '{0}'.", sMessage);
                throw new InvalidConfigurationException(sMessage);
            }

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                bool bListFoundFromSPContext = true;
                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Attempting to get list from context.");
                SPList configStoreList = null;
                try
                {
                    configStoreList = attemptGetLocalConfigStoreListFromContext();
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    trace.WriteLineIf(traceSwitch.TraceWarning, TraceLevel.Warning, "GetMultipleValues(): Failed to get config store list from current context.");
                }

                if (configStoreList == null)
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Attempting to get config store from config.");
                    bListFoundFromSPContext = false;
                    configStoreList = attemptGetGlobalConfigStoreListFromConfig();
                }

                if (configStoreList != null)
                {
                    try
                    {
                        SPListItemCollection items = null;
                        Int32 configIdCounter = 0;
                        StringBuilder sbQuery = new StringBuilder();
                        StringBuilder sbQueryStart = new StringBuilder();
                        sbQuery.Append("<Where>");

                        if (ConfigIdentifiers.Count > 1)
                        {
                            for (int iOrCount = 0; iOrCount < ConfigIdentifiers.Count - 1; iOrCount++)
                            {
                                sbQuery.Append("<Or>");
                            }
                        }

                        foreach (ConfigIdentifier configID in ConfigIdentifiers)
                        {
                            sbQuery.AppendFormat("<And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"Text\">{1}</Value></Eq>" +
                                "<Eq><FieldRef Name=\"{2}\" /><Value Type=\"Text\">{3}</Value></Eq></And>",
                                 ConfigStore.CategoryField, configID.Category, ConfigStore.KeyField, configID.Key);

                            if (ConfigIdentifiers.Count > 1 && configIdCounter > 0)
                            {
                                sbQuery.Append("</Or>");
                            }

                            configIdCounter++;
                        }

                        sbQuery.Append("</Where>");

                        SPQuery query = new SPQuery();
                        query.Query = sbQuery.ToString();
                        query.ViewFields = string.Format("<FieldRef Name=\"{0}\" /><FieldRef Name=\"{1}\" /><FieldRef Name=\"{2}\" />",
                            ConfigStore.CategoryField, ConfigStore.KeyField, ConfigStore.ValueField);

                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Executing query '{0}'.", query.Query);
                        items = configStoreList.GetItems(query);
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Query returned '{0}' items.",
                            items.Count);

                        string sCategory = string.Empty;
                        string sKey = string.Empty;
                        string sConfigValue = string.Empty;
                        string sCacheKey = string.Empty;

                        foreach (SPListItem item in items)
                        {
                            foreach (ConfigIdentifier configID in ConfigIdentifiers)
                            {
                                if ((item[ConfigStore.CategoryField].ToString() == configID.Category) && (item[ConfigStore.KeyField].ToString() == configID.Key))
                                {
                                    sCategory = item[ConfigStore.CategoryField].ToString();
                                    sKey = item[ConfigStore.KeyField].ToString();
                                    sConfigValue = item[ConfigStore.ValueField].ToString();

                                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Retrieved config value '{0}' for " +
                                        "Category '{1}' and Key '{2}'.", sConfigValue, sCategory, sKey);

                                    if (!configDictionary.ContainsKey(configID))
                                    {
                                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Adding to dictionary.");
                                        configDictionary.Add(configID, sConfigValue);

                                        // also add to cache..
                                        if (httpCtxt != null)
                                        {
                                            sCacheKey = GetCacheKey(configID.Category, configID.Key, sSiteCacheKey);
                                            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Have HttpContext " +
                                                "adding config value '{0}' to cache with key '{1}'.", sConfigValue, sCacheKey);
                                            httpCtxt.Cache.Insert(sCacheKey, sConfigValue, null, DateTime.MaxValue, Cache.NoSlidingExpiration);
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        string sMessage =
                                            string.Format(
                                                "Multiple config items were found for the requested item. Please check " +
                                                "config store settings list for category '{0}' and key '{1}'.",
                                                configID.Category, configID.Key);
                                        trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "GetMultipleValues(): '{0}'.", sMessage);
                                        throw new InvalidConfigurationException(sMessage);
                                    }
                                }
                            }
                        }
                    }

                    finally
                    {
                        if (!bListFoundFromSPContext)
                        {
                            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Disposing SPSite and SPWeb objects.");
                            // disposals are required.. 
                            configStoreList.ParentWeb.Site.Dispose();
                            configStoreList.ParentWeb.Dispose();
                        }
                        else
                        {
                            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): No disposals required, list found from SPContext.");
                        }
                    }
                }
            });

            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetMultipleValues(): Returning dictionary with '{0}' entries.", 
                configDictionary.Count);
            return configDictionary;
        }

        #endregion

        public static string GetCacheKey(string Category, string Key, string SiteCacheKey)
        {
            string sCacheKey = string.Format("{0}|{1}", Category, Key);

            // if no SiteCacheKey is passed (e.g. we have no SPContext), then the cache key is not tied to a local site collection - the memory cache value for the 
            // global Config Store will be returned (if found in cache)..
            if (!string.IsNullOrEmpty(SiteCacheKey))
            {
                sCacheKey += "|" + SiteCacheKey;
            }

            return sCacheKey;
        }

        public static bool IsGlobalConfigStore(SPList ConfigStoreList)
        {
            string sGlobalConfigStoreSite = (string)m_Reader.GetValue(m_GlobalConfigSiteAppSettingsKey, typeof(string));
            string sGlobalConfigStoreWeb = (string)m_Reader.GetValue(m_GlobalConfigWebAppSettingsKey, typeof(string));
            string sGlobalConfigStoreListName = (string)m_Reader.GetValue(m_GlobalConfigListAppSettingsKey, typeof(string));

            if (string.Compare(ConfigStoreList.ParentWeb.Site.Url, sGlobalConfigStoreSite, true) == 0 &&
                string.Compare(ConfigStoreList.ParentWeb.Name, sGlobalConfigStoreWeb, true) == 0 &&
                string.Compare(ConfigStoreList.Title, sGlobalConfigStoreListName, true) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region -- Private helper methods --

        private static void trimDictionaryEntries(List<ConfigIdentifier> ConfigIdentifiers)
        {
            foreach (ConfigIdentifier configId in ConfigIdentifiers)
            {
                configId.Category = configId.Category.Trim();
                configId.Key = configId.Key.Trim();
            }
        }

        private static SPList attemptGetLocalConfigStoreListFromContext()
        {
            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromContext(): Entered.");

            SPList configList = null;
            SPContext currentContext = SPContext.Current;

            if (currentContext != null)
            {
                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreListFromContext(): We have SPContext, " +
                     "will first search for Config Store in root web of current site '{0}'.", currentContext.Site.Url);

                SPSite currentSite = currentContext.Site;
                configList = attemptGetConfigStoreList(currentSite.RootWeb, ConfigStoreType.Local, false);
                
                if (configList != null)
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreListFromContext(): Successfully found config list " +
                        "with name '{0}', returning.", configList.Title);
                }
                else
                {
                    trace.WriteLineIf(traceSwitch.TraceWarning, TraceLevel.Warning, "attemptGetConfigStoreListFromContext(): No config list found, " +
                        "returning null.");
                }
            }

            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromContext(): Leaving.");
            return configList;
        }

        private static SPList attemptGetGlobalConfigStoreListFromConfig()
        {
            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromConfig(): Entered.");

            SPList configStoreList = null;

            // ensure we throw exceptions if the *global* config store cannot be found since this is the last resort..
            bool bThrowOnNotFound = true;
            
            SPSite configSite = getGlobalConfigSiteFromConfiguredUrl();
            SPWeb configStoreWeb = attemptGetConfigStoreWeb(configSite, ConfigStoreType.Global, bThrowOnNotFound);
            configStoreList = attemptGetConfigStoreList(configStoreWeb, ConfigStoreType.Global, bThrowOnNotFound);

            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromConfig(): Leaving.");

            return configStoreList;
        }

        private static SPList attemptGetConfigStoreList(SPWeb configStoreWeb, ConfigStoreType configStoreType, bool bThrowOnNotFound)
        {
            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreList(): Entered with SPWeb named '{0}', " +
                "configStoreType '{1}' and 'throw exception on not found' param of '{2}'.", configStoreWeb.Title, configStoreType, bThrowOnNotFound);

            SPList configStoreList = null;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string sListName = null;

                if (configStoreType == ConfigStoreType.Global)
                {
                    string sOverrideList = getAppSettingsValue(m_GlobalConfigListAppSettingsKey);
                    if (string.IsNullOrEmpty(sOverrideList))
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Found override list name '{0}' " +
                            "specified in config, will attempt to find Config Store list with this name.", sOverrideList);
                        sListName = sOverrideList;
                    }
                    else
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): No override list name found in config, " +
                            "will attempt to find Config Store list with default list name '{0}'.", m_DefaultListName);
                        sListName = m_DefaultListName;
                    }
                }
                else
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Will attempt to find local Config " +
                        "Store list with default list name '{0}'.", m_DefaultListName);
                    sListName = m_DefaultListName;
                }

                try
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Fetching list from web...");
                    configStoreList = configStoreWeb.Lists[sListName];
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Successfully found Config Store " +
                        "list named '{0}'.", sListName);
                }
                catch (ArgumentException argExc)
                {
                    trace.WriteLineIf(traceSwitch.TraceWarning, TraceLevel.Warning, "attemptGetConfigStoreList(): Failed to find list named '{0}' " +
                        "in web '{1}'!", sListName, configStoreWeb.Title);
                    if (bThrowOnNotFound)
                    {
                        string sMessage = string.Format("Unable to find configuration list with name '{0}'.", sListName);
                        trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "attemptGetConfigStoreList(): Since 'throw exception on not found' param was true, " +
                            "throwing exception with message '{0}'.", sMessage);
                        throw new InvalidConfigurationException(sMessage, argExc);
                    }
                    else
                    {
                        trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Returning null.");
                    }
                }
            });

            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreList(): Leaving.");

            return configStoreList;
        }

        private static SPWeb attemptGetConfigStoreWeb(SPSite configSite, ConfigStoreType configStoreType, bool bThrowOnNotFound)
        {
            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreWeb(): Entered with SPSite named '{0}' " +
                ", configStoreType '{1}' and 'throw exception on not found' param of '{2}'.", configSite, configStoreType, bThrowOnNotFound);

            // if we're looking for the global Config Store do we have an override web name specified in config? 
            
            string sWebName = null;
            bool bUseRootWeb = false;
            string sOverrideWeb = null;

            if (configStoreType == ConfigStoreType.Global)
            {
                sOverrideWeb = getAppSettingsValue(m_GlobalConfigWebAppSettingsKey);
                
                // if so, find web with this name. If not, default to root web..
                if (!string.IsNullOrEmpty(sOverrideWeb))
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): Found override web name '{0}' " +
                            "specified in config, will attempt to find Config Store web with this name.", sOverrideWeb);
                }
                else
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): No override web name found in config, " +
                        "will use root web of site '{0}'.", configSite.RootWeb.Url);
                    bUseRootWeb = true;
                }
            }
            else
            {
                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): Will attempt to find local Config " +
                    "Store list in root web of site '{0}'.", configSite.RootWeb.Url);
                bUseRootWeb = true;
            }

            SPWeb configStoreWeb = null;
            if (bUseRootWeb)
            {
                configStoreWeb = configSite.RootWeb;
            }
            else
            {
                try
                {
                    configStoreWeb = configSite.AllWebs[sOverrideWeb];
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): Successfully found web with name '{0}'.",
                        configStoreWeb.Name);
                }
                catch (ArgumentException argExc)
                {
                    trace.WriteLineIf(traceSwitch.TraceWarning, TraceLevel.Warning, "attemptGetConfigStoreWeb(): Failed to find web named '{0}' " +
                        "in site '{1}'!", sOverrideWeb, configSite.Url);
                    if (bThrowOnNotFound)
                    {
                        string sMessage =
                            string.Format(
                                "Unable to find configuration web in current site collection with name '{0}'.",
                                sOverrideWeb);
                        trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "attemptGetConfigStoreWeb(): Since 'throw exception on not found' param was true, " +
                            "throwing exception with message '{0}'.", sMessage);
                        throw new InvalidConfigurationException(sMessage, argExc);
                    }
                }
            }

            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreWeb(): Leaving.");
            return configStoreWeb;
        }

        private static SPSite getGlobalConfigSiteFromConfiguredUrl()
        {
            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "getConfigSiteFromConfiguredUrl(): Entered.");

            SPSite configSite = null;
            string sOverrideConfigSiteUrl = getAppSettingsValue(m_GlobalConfigSiteAppSettingsKey);
           
            if (sOverrideConfigSiteUrl == null)
            {
                string sMessage =
                    string.Format("The Config Store is not properly configured in web.config. Alternatively, you are using the Config Store where no SPContext is present " +
                                  "and the host process does not have a '**.exe.config' file which contains Config Store configuration. The config file must contain " +
                                  "an appSettings key named '{0}' which contains the URL of the parent site collection for the Config Store list.", 
                                  m_GlobalConfigSiteAppSettingsKey);
                trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "getConfigSiteFromConfiguredUrl(): No Config Store site URL found " +
                    "in config - throwing exception - '{0}'.", sMessage);
                throw new InvalidConfigurationException(sMessage);
            }
            else
            {
                if (sOverrideConfigSiteUrl.Length == 0)
                {
                    string sMessage = "An override URL for the config site was specified but it was invalid. " +
                                      "Please check your configuration.";
                    trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "getConfigSiteFromConfiguredUrl(): Config Store site URL " +
                        "specified in config was invalid - throwing exception - '{0}'.", sMessage);
                    throw new InvalidConfigurationException(sMessage);
                }

                try
                {
                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "getConfigSiteFromConfiguredUrl(): Attempting to create SPSite " +
                        "with URL '{0}'.", sOverrideConfigSiteUrl);

                    configSite = new SPSite(sOverrideConfigSiteUrl);

                    trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "getConfigSiteFromConfiguredUrl(): Successfully created SPSite, returning.");
                }
                catch (FileNotFoundException e)
                {
                    string sMessage = string.Format("Unable to contact site '{0}' specified in appSettings/{1} as " +
                                                    "URL for Config Store site. Please check the URL.",
                        sOverrideConfigSiteUrl, m_GlobalConfigSiteAppSettingsKey);
                    trace.WriteLineIf(traceSwitch.TraceError, TraceLevel.Error, "getConfigSiteFromConfiguredUrl(): Failed to contact site - " +
                        "throwing exception - '{0}'.", sMessage);
                    throw new InvalidConfigurationException(sMessage, e);
                }
            }

            trace.WriteLineIf(traceSwitch.TraceVerbose, TraceLevel.Verbose, "getConfigSiteFromConfiguredUrl(): Leaving.");

            return configSite;
        }

        private static string getAppSettingsValue(string sKey)
        {
            string sValue = null;

            try
            {
                sValue = m_Reader.GetValue(sKey, typeof(string)) as string;
            }
            catch (InvalidOperationException)
            {
            }

            return sValue;
        }
        
        #endregion

        internal static void CacheConfigStoreItem(string Value, string CacheKey)
        {
            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Adding item to memory cache with key '{0}' " +
                        "and value '{1}'.", CacheKey, Value);
            CacheDependency fileDep = new CacheDependency(CacheDependencyFilePath);
            HttpRuntime.Cache.Insert(CacheKey, Value, fileDep, DateTime.MaxValue, Cache.NoSlidingExpiration);
        }
    }
}
