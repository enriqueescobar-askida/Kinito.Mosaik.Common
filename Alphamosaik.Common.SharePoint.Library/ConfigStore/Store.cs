// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Store.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Store type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using Microsoft.SharePoint;

namespace Alphamosaik.Common.SharePoint.Library.ConfigStore
{
    public abstract class Store<T> : BaseStaticOverride<T> where T : Store<T>
    {
        public readonly string CategoryField = "ConfigCategory";
        public readonly string KeyField = "Title";
        public readonly string ValueField = "ConfigValue";

        private const string GlobalConfigSiteAppSettingsKey = "ConfigSiteUrl";
        private const string GlobalConfigWebAppSettingsKey = "ConfigWebName";
        private const string GlobalConfigListAppSettingsKey = "ConfigListName";
        
        private readonly AppSettingsReader _reader = new AppSettingsReader();

        protected readonly TraceSwitch _traceSwitch = new TraceSwitch("AlphaMosaik.SharePoint.ConfigurationStore",
            "Trace switch for Config Store");

        private readonly ConfigTraceHelper _trace = new ConfigTraceHelper("AlphaMosaik.SharePoint.ConfigurationStore");

        public enum ConfigStoreType
        {
            /// <summary>
            /// Local store type
            /// </summary>
            Local,

            /// <summary>
            /// Global store type
            /// </summary>
            Global
        }

        protected abstract string DefaultListName
        {
            get;
        }

        /// <summary>
        /// Retrieves a single value from the config store list.
        /// </summary>
        /// <param name="category">Category of the item to retrieve.</param>
        /// <param name="key">Key (item name) of the item to retrieve.</param>
        /// <returns>The config item's value.</returns>
        public string GetValue(string category, string key)
        {
            return GetValue(category, key, string.Empty);
        }

        public string GetValue(string category, string key, string url)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Entered with Category '{0}' and Key '{1}', values will be trimmed.",
                category, key);

            // first let's trim the supplied values..
            category = category.Trim();
            key = key.Trim();

            bool listFoundFromSpContext = true;
            string value = null;

            // no value found, proceed with query..
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                SPQuery query = GetSingleItemQuery(category, key);

                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get list from context.");
                SPList configStoreList = null;
                try
                {
                    configStoreList = AttemptGetLocalConfigStoreListFromContext(ref listFoundFromSpContext);
                    if (configStoreList != null)
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection has local Config Store, will " +
                            "first attempt query against this list - '{0}'.", configStoreList.DefaultViewUrl);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No local config store list found in current site collection.");
                }

                if (configStoreList == null)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection does not have local Config Store, falling " +
                        "back to global Config Store.");
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get config store from config.");
                    listFoundFromSpContext = false;
                    configStoreList = AttemptGetGlobalConfigStoreListFromConfig(url);
                }

                if (configStoreList != null)
                {
                    // will have list or have thrown exception by now..               
                    try
                    {
                        value = ExecuteSingleItemQuery(category, key, configStoreList, query, false);
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Retrieved value '{0}' from Config Store '{1}' for " +
                            "category '{2}' and key '{3}'.", value, configStoreList.DefaultViewUrl, category, key);
                    }
                    finally
                    {
                        if (!listFoundFromSpContext)
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Disposing SPSite and SPWeb objects.");

                            // disposals are required.. 
                            configStoreList.ParentWeb.Site.Dispose();
                            configStoreList.ParentWeb.Dispose();
                        }
                        else
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No disposals required, list found from SPContext.");
                        }
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        // now try query against global Config Store..
                        if (listFoundFromSpContext && !IsGlobalConfigStore(configStoreList))
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No value found in local Config Store, now looking " +
                                "up config item for category '{0}' and key '{1}' in global Config Store.",
                                category, key);

                            configStoreList = AttemptGetGlobalConfigStoreListFromConfig(url);
                            listFoundFromSpContext = false;

                            if (configStoreList != null)
                            {
                                value = ExecuteSingleItemQuery(category, key, configStoreList, query, false);
                            }
                        }
                    }
                }
            });

            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Returning '{0}'.", value);

            return value;
        }

        /// <summary>
        /// Retrieves a single value from the config store list.
        /// </summary>
        /// <param name="category">Category of the item to retrieve.</param>
        /// <param name="key">Key (item name) of the item to retrieve.</param>
        /// <returns>The config item's value.</returns>
        public byte[] GetAttachment(string category, string key)
        {
            return GetAttachment(category, key, string.Empty);
        }

        public byte[] GetAttachment(string category, string key, string url)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Entered with Category '{0}' and Key '{1}', values will be trimmed.",
                category, key);

            // first let's trim the supplied values..
            category = category.Trim();
            key = key.Trim();

            bool listFoundFromSpContext = true;
            byte[] value = null;

            // no value found, proceed with query..
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                SPQuery query = GetSingleAttachmentItemQuery(category, key);

                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get list from context.");
                SPList configStoreList = null;
                try
                {
                    configStoreList = AttemptGetLocalConfigStoreListFromContext(ref listFoundFromSpContext);
                    if (configStoreList != null)
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection has local Config Store, will " +
                            "first attempt query against this list - '{0}'.", configStoreList.DefaultViewUrl);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No local config store list found in current site collection.");
                }

                if (configStoreList == null)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection does not have local Config Store, falling " +
                        "back to global Config Store.");
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get config store from config.");
                    listFoundFromSpContext = false;
                    configStoreList = AttemptGetGlobalConfigStoreListFromConfig(url);
                }

                if (configStoreList != null)
                {
                    // will have list or have thrown exception by now..               
                    try
                    {
                        value = ExecuteAttachmentSingleItemQuery(category, key, configStoreList, query, false);
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Retrieved value '{0}' from Config Store '{1}' for " +
                            "category '{2}' and key '{3}'.", value, configStoreList.DefaultViewUrl, category, key);
                    }
                    finally
                    {
                        if (!listFoundFromSpContext)
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Disposing SPSite and SPWeb objects.");

                            // disposals are required.. 
                            configStoreList.ParentWeb.Site.Dispose();
                            configStoreList.ParentWeb.Dispose();
                        }
                        else
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No disposals required, list found from SPContext.");
                        }
                    }

                    if (value == null)
                    {
                        // now try query against global Config Store..
                        if (listFoundFromSpContext && !IsGlobalConfigStore(configStoreList))
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No value found in local Config Store, now looking " +
                                "up config item for category '{0}' and key '{1}' in global Config Store.",
                                category, key);

                            configStoreList = AttemptGetGlobalConfigStoreListFromConfig(url);

                            if (configStoreList != null)
                            {
                                listFoundFromSpContext = false;
                                value = ExecuteAttachmentSingleItemQuery(category, key, configStoreList, query, false);
                            }
                        }
                    }
                }
            });

            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Returning '{0}'.", value);

            return value;
        }

        public byte[] GetAttachment(string category, string key, string attachmentName, string url)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Entered with Category '{0}' and Key '{1}', values will be trimmed.",
                category, key);

            // first let's trim the supplied values..
            category = category.Trim();
            key = key.Trim();

            bool listFoundFromSpContext = true;
            byte[] value = null;

            // no value found, proceed with query..
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                SPQuery query = GetSingleAttachmentItemQuery(category, key);

                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get list from context.");
                SPList configStoreList = null;
                try
                {
                    configStoreList = AttemptGetLocalConfigStoreListFromContext(ref listFoundFromSpContext);
                    if (configStoreList != null)
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection has local Config Store, will " +
                            "first attempt query against this list - '{0}'.", configStoreList.DefaultViewUrl);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No local config store list found in current site collection.");
                }

                if (configStoreList == null)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Current site collection does not have local Config Store, falling " +
                        "back to global Config Store.");
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Attempting to get config store from config.");
                    listFoundFromSpContext = false;
                    configStoreList = AttemptGetGlobalConfigStoreListFromConfig(url);
                }

                if (configStoreList != null)
                {
                    // will have list or have thrown exception by now..               
                    try
                    {
                        value = ExecuteSpecificAttachmentItemQuery(category, key, attachmentName, configStoreList, query, false);
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Retrieved value '{0}' from Config Store '{1}' for " +
                            "category '{2}' and key '{3}'.", value, configStoreList.DefaultViewUrl, category, key);
                    }
                    finally
                    {
                        if (!listFoundFromSpContext)
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): Disposing SPSite and SPWeb objects.");

                            // disposals are required.. 
                            configStoreList.ParentWeb.Site.Dispose();
                            configStoreList.ParentWeb.Dispose();
                        }
                        else
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No disposals required, list found from SPContext.");
                        }
                    }

                    if (value == null)
                    {
                        // now try query against global Config Store..
                        if (listFoundFromSpContext && !IsGlobalConfigStore(configStoreList))
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetValue(): No value found in local Config Store, now looking " +
                                "up config item for category '{0}' and key '{1}' in global Config Store.",
                                category, key);

                            configStoreList = AttemptGetGlobalConfigStoreListFromConfig(url);

                            if (configStoreList != null)
                            {
                                listFoundFromSpContext = false;
                                value = ExecuteSpecificAttachmentItemQuery(category, key, attachmentName, configStoreList, query, false);
                            }
                        }
                    }
                }
            });

            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetValue(): Returning '{0}'.", value);

            return value;
        }

        /// <summary>
        /// Retrieves a single value from the config store list.
        /// </summary>
        /// <param name="category">Category of the item to retrieve.</param>
        /// <param name="key">Key (item name) of the item to retrieve.</param>
        /// <returns>The config item's value.</returns>
        public string GetStringAttachment(string category, string key)
        {
            return GetStringAttachment(category, key, string.Empty);
        }

        public string GetSpecificStringAttachment(string category, string key, string attachmentName)
        {
            return GetSpecificStringAttachment(category, key, attachmentName, string.Empty);
        }

        public string GetStringAttachment(string category, string key, string url)
        {
            string value = null;

            byte[] attachment = GetAttachment(category, key, url);

            if (attachment != null)
            {
                value = Encoding.ASCII.GetString(attachment);
            }

            return value;
        }

        public string GetSpecificStringAttachment(string category, string key, string attachmentName, string url)
        {
            string value = null;

            byte[] attachment = GetAttachment(category, key, attachmentName, url);

            if (attachment != null)
            {
                value = Encoding.ASCII.GetString(attachment);
            }

            return value;
        }

        public void AddAttachment(string category, string key, string attachmentName, byte[] attachment, bool overwrite, string url)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
                                                     {
                                                         bool listFoundFromSpContext = true;
                SPList configStoreList = AttemptGetLocalConfigStoreListFromContext(ref listFoundFromSpContext) ??
                                         AttemptGetGlobalConfigStoreListFromConfig(url);

                if (configStoreList != null)
                {
                    // Est-ce que l'item est déjà présent?
                    SPQuery query = GetSingleAttachmentItemQuery(category, key);
                    SPListItemCollection items = configStoreList.GetItems(query);

                    // Ok y'en a plus qu'un, qu'est-ce qu'on fait?
                    if (items.Count == 1)
                    {
                        for (int i = 0; i < items[0].Attachments.Count; i++)
                        {
                            if (items[0].Attachments[i].IndexOf(attachmentName, StringComparison.OrdinalIgnoreCase) > -1)
                            {
                                if (overwrite)
                                {
                                    items[0].Attachments.DeleteNow(attachmentName);
                                    break;
                                }

                                // Y'en a au moins déjà un mais on a comme instruction de rien faire.
                                return;
                            }
                        }

                        configStoreList.ParentWeb.AllowUnsafeUpdates = true;
                        items[0].Attachments.AddNow(attachmentName, attachment);
                        configStoreList.ParentWeb.AllowUnsafeUpdates = false;
                    }
                }
            });
        }

        /// <summary>
        /// Adds an item to the configuration store.
        /// </summary>
        /// <param name="title">Name of the config item to add</param>
        /// <param name="category">Category of the config item to add</param>
        /// <param name="value">Value of the config item to add</param>
        /// <param name="description">Description of the config item to add</param>
        /// <param name="overwrite">Overwrite the item if it's already present in the configuration store</param>
        public void AddValue(string title, string category, string value, string description, bool overwrite)
        {
            AddValue(title, category, value, description, overwrite, string.Empty);
        }

        public void AddValue(string title, string category, string value, string description, bool overwrite, string url)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
                                                     {
                                                         bool listFoundFromSpContext = true;
                SPList configStoreList = AttemptGetLocalConfigStoreListFromContext(ref listFoundFromSpContext) ??
                                         AttemptGetGlobalConfigStoreListFromConfig(url);

                if (configStoreList != null)
                {
                    // Est-ce que l'item est déjà présent?
                    SPQuery query = GetSingleItemQuery(category, title);
                    SPListItemCollection items = configStoreList.GetItems(query);

                    // Ok y'en a plus qu'un, qu'est-ce qu'on fait?
                    if (items.Count > 0)
                    {
                        if (overwrite)
                        {
                            // Si y'en a plus qu'un, ça va mal en partant: on va tous les effacer
                            foreach (SPListItem listItem in items)
                            {
                                configStoreList.Items.DeleteItemById(listItem.ID);
                            }
                        }
                        else
                        {
                            // Y'en a au moins déjà un mais on a comme instruction de rien faire.
                            return;
                        }
                    }

                    configStoreList.ParentWeb.AllowUnsafeUpdates = true;

                    // On ajoute l'item
                    SPListItem item = configStoreList.Items.Add();
                    item["Title"] = title;
                    item["ConfigCategory"] = category;
                    item["ConfigValue"] = value;
                    item["ConfigItemDescription"] = description;

                    item.Update();

                    configStoreList.ParentWeb.AllowUnsafeUpdates = true;
                }
            });
        }

        /// <summary>
        /// Adds an item to the configuration store.
        /// </summary>
        /// <param name="title">Name of the config item to add</param>
        /// <param name="category">Category of the config item to add</param>
        /// <param name="value">Value of the config item to add</param>
        /// <param name="description">Description of the config item to add</param>
        /// <param name="filename">Name of the attachment</param>
        /// <param name="attachment">A byte array that contains the attachment.</param>
        /// <param name="overwrite">Overwrite the item if it's already present in the configuration store</param>
        public void AddValue(string title, string category, string value, string description, string filename, byte[] attachment, bool overwrite)
        {
            AddValue(title, category, value, description, filename, attachment, overwrite, string.Empty);
        }

        public void AddValue(string title, string category, string value, string description, string filename, byte[] attachment, bool overwrite, string url)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
                                                     {
                                                         bool listFoundFromSpContext = true;
                SPList configStoreList = AttemptGetLocalConfigStoreListFromContext(ref listFoundFromSpContext) ??
                                         AttemptGetGlobalConfigStoreListFromConfig(url);

                if (configStoreList != null)
                {
                    // Est-ce que l'item est déjà présent?
                    SPQuery query = GetSingleItemQuery(category, title);
                    SPListItemCollection items = configStoreList.GetItems(query);

                    // Ok y'en a plus qu'un, qu'est-ce qu'on fait?
                    if (items.Count > 0)
                    {
                        if (overwrite)
                        {
                            // Si y'en a plus qu'un, ça va mal en partant: on va tous les effacer
                            foreach (SPListItem listItem in items)
                            {
                                configStoreList.Items.DeleteItemById(listItem.ID);
                            }
                        }
                        else
                        {
                            // Y'en a au moins déjà un mais on a comme instruction de rien faire.
                            return;
                        }
                    }

                    configStoreList.ParentWeb.AllowUnsafeUpdates = true;

                    // On ajoute l'item
                    SPListItem item = configStoreList.Items.Add();
                    item["Title"] = title;
                    item["ConfigCategory"] = category;
                    item["ConfigValue"] = value;
                    item["ConfigItemDescription"] = description;
                    item.Attachments.Add(filename, attachment);

                    item.Update();
                    configStoreList.ParentWeb.AllowUnsafeUpdates = false;
                }
            });
        }

        /// <summary>
        /// Retrieves multiple config values with a single query. 
        /// </summary>
        /// <param name="configIdentifiers">List of ConfigIdentifier objects to retrieve.</param>
        /// <returns>A Dictionary object containing the requested config values. Items are keyed by ConfigIdentifier.</returns>
        public Dictionary<ConfigIdentifier, string> GetMultipleValues(List<ConfigIdentifier> configIdentifiers)
        {
            return GetMultipleValues(configIdentifiers, string.Empty);
        }

        public Dictionary<ConfigIdentifier, string> GetMultipleValues(List<ConfigIdentifier> configIdentifiers, string url)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetMultipleValues(): Entered with '{0}' config values requested.",
                configIdentifiers.Count);

            // first let's trim the supplied values..
            TrimDictionaryEntries(configIdentifiers);

            var configDictionary = new Dictionary<ConfigIdentifier, string>();

            // attempt retrieval from cache..
            HttpContext httpCtxt = HttpContext.Current;
            SPContext context = SPContext.Current;
            string siteCacheKey = GetSiteCacheKey(context);

            if (httpCtxt != null)
            {
                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Have HttpContext, checking memory cache for config values.");
                bool foundAllValuesInCache = true;

                foreach (ConfigIdentifier configId in configIdentifiers)
                {
                    string cacheKey = GetCacheKey(configId.Category, configId.Key, siteCacheKey);
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Checking in memory for config value with Category '{0}' and " +
                        "Key '{1}'. Cache key '{2}' will be used.", configId.Category, configId.Key, cacheKey);
                    var cachedValue = httpCtxt.Cache[cacheKey] as string;

                    if (cachedValue != null)
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Found value '{0}' in memory for cache key '{1}'. " +
                            "Adding to dictionary.", cachedValue, cacheKey);
                        configDictionary.Add(configId, cachedValue);
                    }
                    else
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Did not find value for cache key '{0}' in memory " +
                            "Query will be executed, not checking memory for any further values.", cacheKey);
                        foundAllValuesInCache = false;
                        break;
                    }
                }

                if (foundAllValuesInCache)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetMultipleValues(): Found all values in memory, returning dictionary.");
                    return configDictionary;
                }

                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Clearing dictionary prior to query.");

                // clear the dictionary since we'll add fresh config items to it from our query..
                configDictionary.Clear();
            }

            // no value found, proceed with query..
            if (configIdentifiers.Count < 2)
            {
                const string Message = "Invalid use of config store - the GetMultipleValues() method " +
                                        "must only be used to retrieve multiple config values.";
                _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "GetMultipleValues(): '{0}'.", Message);
                throw new InvalidConfigurationException(Message);
            }

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                bool listFoundFromSpContext = true;
                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Attempting to get list from context.");
                SPList configStoreList = null;
                try
                {
                    configStoreList = AttemptGetLocalConfigStoreListFromContext(ref listFoundFromSpContext);
                }
                catch (UnauthorizedAccessException)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "GetMultipleValues(): Failed to get config store list from current context.");
                }

                if (configStoreList == null)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Attempting to get config store from config.");
                    listFoundFromSpContext = false;
                    configStoreList = AttemptGetGlobalConfigStoreListFromConfig(url);
                }

                if (configStoreList != null)
                {
                    try
                    {
                        int configIdCounter = 0;
                        var queryBuilder = new StringBuilder();
                        queryBuilder.Append("<Where>");

                        if (configIdentifiers.Count > 1)
                        {
                            for (int count = 0; count < configIdentifiers.Count - 1; count++)
                            {
                                queryBuilder.Append("<Or>");
                            }
                        }

                        foreach (ConfigIdentifier configId in configIdentifiers)
                        {
                            queryBuilder.AppendFormat("<And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"Text\">{1}</Value></Eq>" +
                                "<Eq><FieldRef Name=\"{2}\" /><Value Type=\"Text\">{3}</Value></Eq></And>",
                                 CategoryField, configId.Category, KeyField, configId.Key);

                            if (configIdentifiers.Count > 1 && configIdCounter > 0)
                            {
                                queryBuilder.Append("</Or>");
                            }

                            configIdCounter++;
                        }

                        queryBuilder.Append("</Where>");

                        var query = new SPQuery
                        {
                            Query = queryBuilder.ToString(),
                            ViewFields =
                                string.Format(
                                    "<FieldRef Name=\"{0}\" /><FieldRef Name=\"{1}\" /><FieldRef Name=\"{2}\" />",
                                    CategoryField, KeyField,
                                    ValueField)
                        };

                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Executing query '{0}'.", query.Query);
                        SPListItemCollection items = configStoreList.GetItems(query);
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Query returned '{0}' items.",
                            items.Count);

                        foreach (SPListItem item in items)
                        {
                            foreach (ConfigIdentifier configId in configIdentifiers)
                            {
                                if ((item[CategoryField].ToString() == configId.Category) && (item[KeyField].ToString() == configId.Key))
                                {
                                    string category = item[CategoryField].ToString();
                                    string key = item[KeyField].ToString();
                                    string configValue = item[ValueField].ToString();

                                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Retrieved config value '{0}' for " +
                                        "Category '{1}' and Key '{2}'.", configValue, category, key);

                                    if (!configDictionary.ContainsKey(configId))
                                    {
                                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Adding to dictionary.");
                                        configDictionary.Add(configId, configValue);

                                        // also add to cache..
                                        if (httpCtxt != null)
                                        {
                                            string cacheKey = GetCacheKey(configId.Category, configId.Key, siteCacheKey);
                                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Have HttpContext " +
                                                "adding config value '{0}' to cache with key '{1}'.", configValue, cacheKey);
                                            httpCtxt.Cache.Insert(cacheKey, configValue, null, DateTime.MaxValue, Cache.NoSlidingExpiration);
                                        }

                                        break;
                                    }

                                    string message =
                                        string.Format(
                                            "Multiple config items were found for the requested item. Please check " +
                                            "config store settings list for category '{0}' and key '{1}'.",
                                            configId.Category, configId.Key);
                                    _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "GetMultipleValues(): '{0}'.", message);
                                    throw new InvalidConfigurationException(message);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (!listFoundFromSpContext)
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): Disposing SPSite and SPWeb objects.");

                            // disposals are required.. 
                            configStoreList.ParentWeb.Site.Dispose();
                            configStoreList.ParentWeb.Dispose();
                        }
                        else
                        {
                            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "GetMultipleValues(): No disposals required, list found from SPContext.");
                        }
                    }
                }
            });

            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "GetMultipleValues(): Returning dictionary with '{0}' entries.",
                configDictionary.Count);
            return configDictionary;
        }

        public string GetCacheKey(string category, string key, string siteCacheKey)
        {
            string cacheKey = string.Format("{0}|{1}", category, key);

            // if no SiteCacheKey is passed (e.g. we have no SPContext), then the cache key is not tied to a local site collection - the memory cache value for the 
            // global Config Store will be returned (if found in cache)..
            if (!string.IsNullOrEmpty(siteCacheKey))
            {
                cacheKey += "|" + siteCacheKey;
            }

            return cacheKey;
        }

        public bool IsGlobalConfigStore(SPList configStoreList)
        {
            try
            {
                var globalConfigStoreSite = (string)_reader.GetValue(GlobalConfigSiteAppSettingsKey, typeof(string));
                var globalConfigStoreWeb = (string)_reader.GetValue(GlobalConfigWebAppSettingsKey, typeof(string));
                var globalConfigStoreListName = (string)_reader.GetValue(GlobalConfigListAppSettingsKey, typeof(string));

                if (string.Compare(configStoreList.ParentWeb.Site.Url, globalConfigStoreSite, true) == 0 &&
                    string.Compare(configStoreList.ParentWeb.Name, globalConfigStoreWeb, true) == 0 &&
                    string.Compare(configStoreList.Title, globalConfigStoreListName, true) == 0)
                {
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "IsGlobalConfigStore(): No value found - catching exception - '{0}'.", ex.Message);
            }

            return false;
        }

        public string GetGlobalConfigSiteAppSettingsKey()
        {
            return GetAppSettingsValue(GlobalConfigSiteAppSettingsKey);
        }

        private static string GetSiteCacheKey(SPContext context)
        {
            string siteCacheKey = null;
            if (context != null)
            {
                siteCacheKey = context.Site.Url;
            }

            return siteCacheKey;
        }

        private static void TrimDictionaryEntries(IEnumerable<ConfigIdentifier> configIdentifiers)
        {
            foreach (ConfigIdentifier configId in configIdentifiers)
            {
                configId.Category = configId.Category.Trim();
                configId.Key = configId.Key.Trim();
            }
        }

        private SPQuery GetSingleItemQuery(string category, string key)
        {
            var query = new SPQuery
            {
                Query =
                    string.Format(
                        "<Where><And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"Text\">{1}</Value></Eq><Eq><FieldRef Name=\"{2}\" /><Value Type=\"Text\">{3}</Value></Eq></And></Where>",
                        CategoryField, category, KeyField, key),
                ViewFields = string.Format("<FieldRef Name=\"{0}\" />", ValueField)
            };
            return query;
        }

        private SPQuery GetSingleAttachmentItemQuery(string category, string key)
        {
            var query = new SPQuery
            {
                Query =
                    string.Format(
                        "<Where><And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"Text\">{1}</Value></Eq><Eq><FieldRef Name=\"{2}\" /><Value Type=\"Text\">{3}</Value></Eq></And></Where>",
                        CategoryField, category, KeyField, key)
            };
            return query;
        }

        private string ExecuteSingleItemQuery(string category, string key, SPList configStoreList, SPQuery query, bool throwOnNoResults)
        {
            string value = null;

            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Executing query '{0}'.", query.Query);
            SPListItemCollection items = configStoreList.GetItems(query);

            if (items.Count == 1)
            {
                value = (items[0][0] ?? string.Empty).ToString();
                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Found '{0}' as config value for Category '{1}' " +
                   "and Key '{2}'.", value, category, key);
            }
            else if (items.Count > 1)
            {
                string message =
                    string.Format("Multiple config items were found for the requested item. Please check " +
                                  "config store settings list for category '{0}' and key '{1}'.", category, key);
                _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): Multiple config values found for the requested Category/Key! Throwing " +
                    "exception - '{0}'.", message);
                throw new InvalidConfigurationException(message);
            }
            else if (items.Count == 0)
            {
                string message = string.Format("No config item was found for category '{0}' and key '{1}'.", category, key);
                if (throwOnNoResults)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): No value found - throwing exception - '{0}'.",
                        message);
                    throw new InvalidConfigurationException(message);
                }

                _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "executeSingleItemQuery(): No config value found for category '{0}' " +
                                                                                "and key '{1}'. Returning null, but not throwing exception since caller specified not to.",
                                  category, key);
            }

            return value;
        }

        private byte[] ExecuteAttachmentSingleItemQuery(string category, string key, SPList configStoreList, SPQuery query, bool throwOnNoResults)
        {
            byte[] value = null;

            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Executing query '{0}'.", query.Query);
            SPListItemCollection items = configStoreList.GetItems(query);

            if (items.Count == 1 && items[0].Attachments.Count > 0)
            {
                SPFile file = items[0].ParentList.ParentWeb.GetFile(items[0].Attachments.UrlPrefix + items[0].Attachments[0]);
                value = file.OpenBinary();

                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Found '{0}' as config value for Category '{1}' " +
                   "and Key '{2}'.", value, category, key);
            }
            else if (items.Count > 1)
            {
                string message =
                    string.Format("Multiple config items were found for the requested item. Please check " +
                                  "config store settings list for category '{0}' and key '{1}'.", category, key);
                _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): Multiple config values found for the requested Category/Key! Throwing " +
                    "exception - '{0}'.", message);
                throw new InvalidConfigurationException(message);
            }
            else if (items.Count == 0)
            {
                string message = string.Format("No config item was found for category '{0}' and key '{1}'.", category, key);
                if (throwOnNoResults)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): No value found - throwing exception - '{0}'.",
                        message);
                    throw new InvalidConfigurationException(message);
                }

                _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "executeSingleItemQuery(): No config value found for category '{0}' " +
                                                                                "and key '{1}'. Returning null, but not throwing exception since caller specified not to.",
                                  category, key);
            }

            return value;
        }

        private byte[] ExecuteSpecificAttachmentItemQuery(string category, string key, string attachmentName, SPList configStoreList, SPQuery query, bool throwOnNoResults)
        {
            byte[] value = null;

            _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Executing query '{0}'.", query.Query);
            SPListItemCollection items = configStoreList.GetItems(query);

            if (items.Count == 1 && items[0].Attachments.Count > 0)
            {
                for (int i = 0; i < items[0].Attachments.Count; i++)
                {
                    if (items[0].Attachments[i].IndexOf(attachmentName, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        SPFile file = items[0].ParentList.ParentWeb.GetFile(items[0].Attachments.UrlPrefix + items[0].Attachments[i]);
                        value = file.OpenBinary();

                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "executeSingleItemQuery(): Found '{0}' as config value for Category '{1}' " +
                           "and Key '{2}'.", value, category, key);

                        break;
                    }
                }
            }
            else if (items.Count > 1)
            {
                string message =
                    string.Format("Multiple config items were found for the requested item. Please check " +
                                  "config store settings list for category '{0}' and key '{1}'.", category, key);
                _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): Multiple config values found for the requested Category/Key! Throwing " +
                    "exception - '{0}'.", message);
                throw new InvalidConfigurationException(message);
            }
            else if (items.Count == 0)
            {
                string message = string.Format("No config item was found for category '{0}' and key '{1}'.", category, key);
                if (throwOnNoResults)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "executeSingleItemQuery(): No value found - throwing exception - '{0}'.",
                        message);
                    throw new InvalidConfigurationException(message);
                }

                _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "executeSingleItemQuery(): No config value found for category '{0}' " +
                                                                                "and key '{1}'. Returning null, but not throwing exception since caller specified not to.",
                                  category, key);
            }

            return value;
        }

        private SPList AttemptGetLocalConfigStoreListFromContext(ref bool listFoundFromSpContext)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromContext(): Entered.");

            SPList configList = null;
            SPContext currentContext = SPContext.Current;

            if (currentContext != null)
            {
                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreListFromContext(): We have SPContext, " +
                     "will first search for Config Store in root web of current site '{0}'.", currentContext.Site.Url);

                SPSite currentSite = currentContext.Site;
                configList = AttemptGetConfigStoreList(currentSite.RootWeb, ConfigStoreType.Local, false);
                
                if (configList != null)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreListFromContext(): Successfully found config list " +
                        "with name '{0}', returning.", configList.Title);
                }
                else
                {
                    // Try Root Web Application
                    string currentSiteUrl = currentSite.Url;

                    if (!currentSiteUrl.EndsWith("/"))
                    {
                        currentSiteUrl = currentSiteUrl + "/";
                    }

                    string rootUrl = Utilities.FilterUrl(currentSiteUrl); // root server url

                    configList = AttemptGetGlobalConfigStoreListFromConfig(rootUrl);

                    if (configList != null)
                    {
                        listFoundFromSpContext = false;
                    }
                    else
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "attemptGetConfigStoreListFromContext(): No config list found, " +
                        "returning null.");
                    }
                }
            }
            
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromContext(): Leaving.");
            return configList;
        }

        private SPList AttemptGetGlobalConfigStoreListFromConfig(string url)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromConfig(): Entered.");

            // ensure we throw exceptions if the *global* config store cannot be found since this is the last resort..
            const bool ThrowOnNotFound = false;

            try
            {
                using (SPSite configSite = GetGlobalConfigSiteFromConfiguredUrl(url))
                {
                    SPWeb configStoreWeb = AttemptGetConfigStoreWeb(configSite, ConfigStoreType.Global, ThrowOnNotFound);

                    configStoreWeb.AllowUnsafeUpdates = true;
                    SPList configStoreList = AttemptGetConfigStoreList(configStoreWeb, ConfigStoreType.Global, ThrowOnNotFound);

                    _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreListFromConfig(): Leaving.");

                    return configStoreList;
                }
            }
            catch (InvalidConfigurationException ex)
            {
                _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "attemptGetConfigStoreListFromConfig(): Failed!");
            }

            return null;
        }

        private SPList AttemptGetConfigStoreList(SPWeb configStoreWeb, ConfigStoreType configStoreType, bool throwOnNotFound)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreList(): Entered with SPWeb named '{0}', " +
                "configStoreType '{1}' and 'throw exception on not found' param of '{2}'.", configStoreWeb.Title, configStoreType, throwOnNotFound);

            SPList configStoreList = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
                                                     {
                string listName;

                if (configStoreType == ConfigStoreType.Global)
                {
                    string overrideList = GetAppSettingsValue(GlobalConfigListAppSettingsKey);
                    if (!string.IsNullOrEmpty(overrideList))
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Found override list name '{0}' " +
                            "specified in config, will attempt to find Config Store list with this name.", overrideList);
                        listName = overrideList;
                    }
                    else
                    {
                        _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): No override list name found in config, " +
                            "will attempt to find Config Store list with default list name '{0}'.", DefaultListName);
                        listName = DefaultListName;
                    }
                }
                else
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Will attempt to find local Config " +
                        "Store list with default list name '{0}'.", DefaultListName);
                    listName = DefaultListName;
                }

                try
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Fetching list from web...");
                    configStoreList = configStoreWeb.Lists[listName];
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Successfully found Config Store " +
                        "list named '{0}'.", listName);
                }
                catch (ArgumentException argExc)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "attemptGetConfigStoreList(): Failed to find list named '{0}' " +
                        "in web '{1}'!", listName, configStoreWeb.Title);
                    if (throwOnNotFound)
                    {
                        string message = string.Format("Unable to find configuration list with name '{0}'.", listName);
                        _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "attemptGetConfigStoreList(): Since 'throw exception on not found' param was true, " +
                            "throwing exception with message '{0}'.", message);
                        throw new InvalidConfigurationException(message, argExc);
                    }

                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreList(): Returning null.");
                }
            });

            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreList(): Leaving.");

            return configStoreList;
        }

        private SPWeb AttemptGetConfigStoreWeb(SPSite configSite, ConfigStoreType configStoreType, bool throwOnNotFound)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreWeb(): Entered with SPSite named '{0}' " +
                ", configStoreType '{1}' and 'throw exception on not found' param of '{2}'.", configSite, configStoreType, throwOnNotFound);

            // if we're looking for the global Config Store do we have an override web name specified in config? 
            bool useRootWeb = false;
            string overrideWeb = null;

            if (configStoreType == ConfigStoreType.Global)
            {
                overrideWeb = GetAppSettingsValue(GlobalConfigWebAppSettingsKey);
                
                // if so, find web with this name. If not, default to root web..
                if (!string.IsNullOrEmpty(overrideWeb))
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): Found override web name '{0}' " +
                            "specified in config, will attempt to find Config Store web with this name.", overrideWeb);
                }
                else
                {
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): No override web name found in config, " +
                        "will use root web of site '{0}'.", configSite.RootWeb.Url);
                    useRootWeb = true;
                }
            }
            else
            {
                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): Will attempt to find local Config " +
                    "Store list in root web of site '{0}'.", configSite.RootWeb.Url);
                useRootWeb = true;
            }

            SPWeb configStoreWeb = null;
            if (useRootWeb)
            {
                configStoreWeb = configSite.RootWeb;
            }
            else
            {
                try
                {
                    configStoreWeb = configSite.AllWebs[overrideWeb];
                    _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "attemptGetConfigStoreWeb(): Successfully found web with name '{0}'.",
                        configStoreWeb.Name);
                }
                catch (ArgumentException argExc)
                {
                    _trace.WriteLineIf(_traceSwitch.TraceWarning, TraceLevel.Warning, "attemptGetConfigStoreWeb(): Failed to find web named '{0}' " +
                        "in site '{1}'!", overrideWeb, configSite.Url);
                    if (throwOnNotFound)
                    {
                        string message =
                            string.Format(
                                "Unable to find configuration web in current site collection with name '{0}'.",
                                overrideWeb);
                        _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "attemptGetConfigStoreWeb(): Since 'throw exception on not found' param was true, " +
                            "throwing exception with message '{0}'.", message);
                        throw new InvalidConfigurationException(message, argExc);
                    }
                }
            }

            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "attemptGetConfigStoreWeb(): Leaving.");
            return configStoreWeb;
        }

        private SPSite GetGlobalConfigSiteFromConfiguredUrl(string url)
        {
            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "getConfigSiteFromConfiguredUrl(): Entered.");

            SPSite configSite;
            string overrideConfigSiteUrl = GetAppSettingsValue(GlobalConfigSiteAppSettingsKey);

            if (overrideConfigSiteUrl == null)
            {
                if (string.IsNullOrEmpty(url))
                {
                    string message =
                    string.Format("The Config Store is not properly configured in web.config. Alternatively, you are using the Config Store where no SPContext is present " +
                                  "and the host process does not have a '**.exe.config' file which contains Config Store configuration. The config file must contain " +
                                  "an appSettings key named '{0}' which contains the URL of the parent site collection for the Config Store list.",
                                  GlobalConfigSiteAppSettingsKey);
                    _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "getConfigSiteFromConfiguredUrl(): No Config Store site URL found " +
                        "in config - throwing exception - '{0}'.", message);
                    throw new InvalidConfigurationException(message);
                }

                overrideConfigSiteUrl = url;
            }

            if (overrideConfigSiteUrl.Length == 0)
            {
                const string Message = "An override URL for the config site was specified but it was invalid. " +
                                       "Please check your configuration.";
                _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "getConfigSiteFromConfiguredUrl(): Config Store site URL " +
                                                                            "specified in config was invalid - throwing exception - '{0}'.", Message);
                throw new InvalidConfigurationException(Message);
            }

            try
            {
                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "getConfigSiteFromConfiguredUrl(): Attempting to create SPSite " +
                                                                          "with URL '{0}'.", overrideConfigSiteUrl);

                configSite = new SPSite(overrideConfigSiteUrl);

                _trace.WriteLineIf(_traceSwitch.TraceInfo, TraceLevel.Info, "getConfigSiteFromConfiguredUrl(): Successfully created SPSite, returning.");
            }
            catch (FileNotFoundException e)
            {
                string message = string.Format("Unable to contact site '{0}' specified in appSettings/{1} as " +
                                               "URL for Config Store site. Please check the URL.",
                                               overrideConfigSiteUrl, GlobalConfigSiteAppSettingsKey);
                _trace.WriteLineIf(_traceSwitch.TraceError, TraceLevel.Error, "getConfigSiteFromConfiguredUrl(): Failed to contact site - " +
                                                                            "throwing exception - '{0}'.", message);
                throw new InvalidConfigurationException(message, e);
            }

            _trace.WriteLineIf(_traceSwitch.TraceVerbose, TraceLevel.Verbose, "getConfigSiteFromConfiguredUrl(): Leaving.");

            return configSite;
        }

        private string GetAppSettingsValue(string key)
        {
            string value = null;

            try
            {
                value = _reader.GetValue(key, typeof(string)) as string;
            }
            catch (InvalidOperationException)
            {
            }

            return value;
        }
    }
}
