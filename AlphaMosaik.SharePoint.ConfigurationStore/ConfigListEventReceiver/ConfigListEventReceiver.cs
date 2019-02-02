using System;

using Microsoft.SharePoint;

namespace AlphaMosaik.SharePoint.ConfigurationStore.ConfigListEventReceiver
{
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// This list event receiver manages the memory cache used by the Config Store. In the current implementation 
    /// it is essential that the Config Store list is configured with this event receiver, otherwise stale 
    /// values will be retrieved.
    /// </summary>
    public class ConfigListEventReceiver : SPItemEventReceiver
    {
        private static readonly TraceSwitch traceSwitch = new TraceSwitch("AlphaMosaik.SharePoint.ConfigurationStore",
    "Trace switch for Config Store");

        private static readonly ConfigTraceHelper trace = new ConfigTraceHelper("AlphaMosaik.SharePoint.ConfigurationStore.ConfigStoreListEventReceiver");

        // Tout est commenté parce que c'était plus de trouble a configurer que le réel avantage d'utiliser du caching avec un fichier

        /*
        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "ItemAdded(): Entered - item will be added to cache.");

            // here there is no need to invalidate the existing items in the cache, but we can proactively add this item 
            // prior to it's first request..
            addItemToCache(properties);

            base.ItemAdded(properties);
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "ItemUpdated(): Entered - cache will be flushed and item added to cache.");

            // here we need to flush the cache across all app pools..
            flushCache();

            // .. but whilst we're here let's also add the updated item to the current app pool's cache..
            addItemToCache(properties);

            base.ItemUpdated(properties);
        }

        /// <summary>
        /// An item was deleted.
        /// </summary>
        public override void ItemDeleted(SPItemEventProperties properties)
        {
            trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "ItemDeleted(): Entered - item will be deleted and cache flushed.");

            // here we need to flush the cache across all app pools..
            flushCache();

            base.ItemDeleted(properties);
        }

        private static void addItemToCache(SPItemEventProperties properties)
        {
            string sCategory = properties.ListItem[ConfigStore.CategoryField] as string;
            string sKey = properties.ListItem[ConfigStore.KeyField] as string;
            string sValue = properties.ListItem[ConfigStore.ValueField] as string;

            string sCacheKey = (ConfigStore.IsGlobalConfigStore(properties.ListItem.ParentList)) ?
                ConfigStore.GetCacheKey(sCategory, sKey, null) :
                ConfigStore.GetCacheKey(sCategory, sKey, properties.ListItem.ParentList.ParentWeb.Site.Url);

            ConfigStore.CacheConfigStoreItem(sValue, sCacheKey);
        }

        private static void flushCache()
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(
                    () => File.WriteAllText(ConfigStore.CacheDependencyFilePath, DateTime.Now.ToString()));
                trace.WriteLineIf(traceSwitch.TraceInfo, TraceLevel.Info, "flushCache(): Successfully flushed Config Store cache - wrote " +
                    "to cache dependency file at '{0}'.", ConfigStore.CacheDependencyFilePath);
            }
            catch (Exception e)
            {
                // many IO exception types can occur here, so we catch general exception..
                trace.WriteLineIf(traceSwitch.TraceWarning, TraceLevel.Warning, "flushCache(): Failed to flush Config Store cache - unable " +
                    "to write to cache dependency file at '{0}'. Config Store may return stale values! Exception details '{1}'.",
                    ConfigStore.CacheDependencyFilePath, e);
            }
        } 
         */
    }
}
