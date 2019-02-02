// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheEvent.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the CacheEvent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace ReloadCacheEvent
{
    using Microsoft.SharePoint.Administration;

    using ThreadState = System.Threading.ThreadState;

    public class CacheEvent : SPItemEventReceiver
    {
        public const string WorkItemId = "{da3939d5-7944-4dd8-bdab-b0bfd729ec19}";

        private static readonly Queue<WebDefinitionHelper> WebToUpdate = new Queue<WebDefinitionHelper>();
        private static readonly SyncEvents SyncEvent = new SyncEvents();
        private static readonly WebUpdateConsumer Consumer = new WebUpdateConsumer(WebToUpdate, SyncEvent);
        private static readonly Thread ConsumerThread = new Thread(Consumer.ThreadRun) { Priority = ThreadPriority.BelowNormal };

        public CacheEvent()
        {
            if (!ConsumerThread.IsAlive &&
                !(ConsumerThread.ThreadState == ThreadState.Running ||
                  ConsumerThread.ThreadState == ThreadState.WaitSleepJoin))
            {
                ConsumerThread.Start();
            }
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
                                                     {
                                                         EventFiringEnabled = false;
                                                         CheckDefaultLang(properties);
                                                         EventFiringEnabled = true;
                                                     });
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
                                                     {
                                                         EventFiringEnabled = false;
                                                         CheckDefaultLang(properties);
                                                         EventFiringEnabled = true;
                                                     });
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
                                                     {
                                                         EventFiringEnabled = false;
                                                         CheckDefaultLang(properties);
                                                         EventFiringEnabled = true;
                                                     });
        }

        private static void CheckDefaultLang(SPItemEventProperties property)
        {
            try
            {
                SPWeb currentWeb = property.OpenWeb();
                SPSite currentSite = property.OpenSite();

                bool loadBalancingServersListExist = false;

                foreach (SPList list in currentWeb.Lists)
                {
                    if (list.ToString() == "LoadBalancingServers")
                    {
                        if (list.ItemCount > 1)
                        {
                            loadBalancingServersListExist = true;
                        }

                        break;
                    }
                }

                if (loadBalancingServersListExist)
                {
                    SPList listLoadBalancingServers = currentWeb.Lists["LoadBalancingServers"];

                    SPListItem item = property.ListItem;

                    SPList parentList = item.ParentList;

                    if (parentList.ToString() == "TranslationContents" || (parentList.ToString() == "TranslationContentsSub"))
                    {
                        bool reloadCache = true;

                    foreach (SPField currentField in item.Fields)
                    {
                        if (item[currentField.InternalName] != null)
                            if (item[currentField.InternalName].ToString().Contains("SPS_ADDED_"))
                            {
                                item[currentField.InternalName] = item[currentField.InternalName].ToString().Replace(
                                    "SPS_ADDED_", string.Empty);
                                currentWeb.AllowUnsafeUpdates = true;
                                item.SystemUpdate(false);
                                currentWeb.AllowUnsafeUpdates = false;
                                reloadCache = false;
                            }
                    }

                        if (reloadCache)
                        {
                            foreach (SPListItem server in listLoadBalancingServers.Items)
                            {
#pragma warning disable 612,618
                                ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
#pragma warning restore 612,618
                                string webId = string.Empty;
                                if (parentList.ToString() == "TranslationContentsSub")
                                {
                                    webId = "_" + item.Web.ID;
                                }

                                var req = (HttpWebRequest)WebRequest.Create(server["Title"] + "/_layouts/CacheControl.aspx?list=TranslationContents&webId=" + webId);
                                req.Method = "GET";
                                req.Credentials = CredentialCache.DefaultCredentials;
                                req.GetResponse();
                            }
                        }
                    }

                    if (parentList.ToString() == "LanguagesVisibility" || parentList.ToString() == "Configuration Store")
                    {
                        foreach (SPListItem server in listLoadBalancingServers.Items)
                        {
#pragma warning disable 612,618
                            ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
#pragma warning restore 612,618

                            var req = (HttpWebRequest)WebRequest.Create(server["Title"] + "/_layouts/CacheControl.aspx?list=TranslationContentsOrLanguagesVisibility");
                            req.Method = "GET";
                            req.Credentials = CredentialCache.DefaultCredentials;
                            req.GetResponse();
                        }
                    }

                    if (parentList.ToString() == "PagesTranslations")
                    {
                        foreach (SPListItem server in listLoadBalancingServers.Items)
                        {
#pragma warning disable 612,618
                            ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
#pragma warning restore 612,618

                            var req = (HttpWebRequest)WebRequest.Create(server["Title"] + "/_layouts/CacheControl.aspx?list=PagesTranslations");
                            req.Method = "GET";
                            req.Credentials = CredentialCache.DefaultCredentials;
                            req.GetResponse();
                        }
                    }
                }
                else
                {
                    int serversCount = GetNumberOfFrontEndServer();

                    SPListItem item = property.ListItem;

                    SPList parentList = item.ParentList;

                    if ((parentList.ToString() == "TranslationContents") || (parentList.ToString() == "TranslationContentsSub") || (parentList.ToString() == "LanguagesVisibility"))
                    {
                        bool reloadCache = true;

                        foreach (SPField currentField in item.Fields)
                        {
                            if (item[currentField.InternalName] != null)
                            {
                                if (item[currentField.InternalName].ToString().Contains("SPS_ADDED_"))
                                {
                                    item[currentField.InternalName] = item[currentField.InternalName].ToString().Replace("SPS_ADDED_", string.Empty);
                                    currentWeb.AllowUnsafeUpdates = true;
                                    item.SystemUpdate(false);
                                    currentWeb.AllowUnsafeUpdates = false;
                                    reloadCache = false;
                                }
                            }
                        }

                        if (reloadCache)
                        {
                            if (serversCount > 1)
                            {
                                string webId = "_" + item.Web.ID;

                                if (parentList.ToString().Equals("TranslationContents"))
                                {
                                    webId = "_" + currentWeb.Site.WebApplication.Id;
                                }

                                CreateReloadCacheTimer(currentSite, currentWeb, "/_layouts/CacheControl.aspx?list=TranslationContents&webId=" + webId);
                            }
                            else
                            {
                                ResetSiteFirstLevelCache(currentWeb);

                                if (parentList.ToString() == "TranslationContents" || parentList.ToString() == "TranslationContentsSub")
                                {
                                    string webId = "_" + item.Web.ID;
                                    HttpRuntime.Cache.Remove("SPS_TRANSLATION_CACHE_IS_LOADED" + webId);
                                    HttpRuntime.Cache.Add("SPS_TRANSLATION_CACHE_IS_LOADED" + webId, "2", null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                                }
                                else
                                {
                                    var cacheLoadedKey = new StringCollection();

                                    IDictionaryEnumerator cacheEnum = HttpRuntime.Cache.GetEnumerator();
                                    while (cacheEnum.MoveNext())
                                    {
                                        string key = cacheEnum.Key.ToString();

                                        if (key.IndexOf("SPS_TRANSLATION_CACHE_IS_LOADED") != -1)
                                        {
                                            cacheLoadedKey.Add(key);
                                        }
                                    }

                                    foreach (string key in cacheLoadedKey)
                                    {
                                        HttpRuntime.Cache.Remove(key);
                                    }
                                }
                            }
                        }

                        // Traitement pour update des resx
                        if ((HttpRuntime.Cache["AlphamosaikResxFilesUpdate"] != null) && (bool)HttpRuntime.Cache["AlphamosaikResxFilesUpdate"])
                            UpdateResxFiles(item);
                    }

                    if (parentList.ToString() == "PagesTranslations")
                    {
                        if (serversCount > 1)
                        {
                            CreateReloadCacheTimer(currentSite, currentWeb, "/_layouts/CacheControl.aspx?list=PagesTranslations");
                        }
                        else
                        {
                            HttpRuntime.Cache.Remove("AdminPagesToTranslate");
                            HttpRuntime.Cache.Remove("PagesNotToTranslate");
                            HttpRuntime.Cache.Remove("PagesToTranslate");
                        }
                    }

                    if (parentList.ToString() == "Configuration Store")
                    {
                        if (serversCount > 1)
                        {
                            CreateReloadCacheTimer(currentSite, currentWeb, "/_layouts/CacheControl.aspx?list=TranslationContentsOrLanguagesVisibility");
                        }
                        else
                        {
                            var cacheLoadedKey = new StringCollection();

                            IDictionaryEnumerator cacheEnum = HttpRuntime.Cache.GetEnumerator();
                            while (cacheEnum.MoveNext())
                            {
                                string key = cacheEnum.Key.ToString();

                                if (key.IndexOf("SPS_TRANSLATION_CACHE_IS_LOADED") != -1)
                                {
                                    cacheLoadedKey.Add(key);
                                }
                            }

                            foreach (string key in cacheLoadedKey)
                            {
                                HttpRuntime.Cache.Remove(key);
                            }
                        }
                    }
                }

                currentWeb.Dispose();
                currentWeb.Close();
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in ReloadCacheEvent: " + ex.Message);
            }
        }

        private static void ResetSiteFirstLevelCache(SPWeb currentWeb)
        {
            string currentSiteUrl = currentWeb.Site.Url;

            if (!currentSiteUrl.EndsWith("/"))
            {
                currentSiteUrl = currentSiteUrl + "/";
            }

            string siteDependencyKey = "CachingHttpModule_" + currentSiteUrl + "_";

            HttpRuntime.Cache.Remove(siteDependencyKey);
        }

        private static void CreateReloadCacheTimer(SPSite currentSite, SPWeb currentWeb, string cmd)
        {
            string cmdParameters = currentWeb.Url + "-#####-" + cmd;

            currentSite.AddWorkItem(Guid.Empty, DateTime.Now.ToUniversalTime(), new Guid(WorkItemId), currentWeb.ID,
                        currentWeb.ID, 1, false, Guid.Empty, Guid.Empty, currentWeb.CurrentUser.ID, null, cmdParameters, Guid.Empty);
        }

        private static int GetNumberOfFrontEndServer()
        {
            int serversCount = 0;

            if (SPFarm.Joined)
            {
                foreach (SPServer server in SPFarm.Local.Servers)
                {
                    if (server.Status == SPObjectStatus.Online)
                    {
                        if (server.Role == SPServerRole.WebFrontEnd)
                        {
                            serversCount++;
                        }
                        else if (server.Role == SPServerRole.Application)
                        {
                            foreach (SPServiceInstance serviceInstance in server.ServiceInstances)
                            {
                                if (serviceInstance.Status == SPObjectStatus.Online)
                                {
                                    var webServiceInstance = serviceInstance as SPWebServiceInstance;

                                    if (webServiceInstance != null && webServiceInstance.Name.IndexOf("WSS_Administration", StringComparison.OrdinalIgnoreCase) == -1)
                                    {
                                        serversCount++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return serversCount;
        }

        /// <summary>
        /// Update Resx files for all webs of an item
        /// </summary>
        /// <param name="item">item of the list</param>
        private static void UpdateResxFiles(SPListItem item)
        {
            try
            {
                string siteUrl = item.Web.Site.Url;

                using (var currentSite = new SPSite(siteUrl))
                {
                    foreach (SPWeb web in currentSite.AllWebs)
                    {
                        try
                        {
                            var webHelper = new WebDefinitionHelper(item, web);

                            lock (((ICollection)WebToUpdate).SyncRoot)
                            {
                                WebToUpdate.Enqueue(webHelper);
                            }

                            SyncEvent.NewItemEvent.Set();
                        }
                        catch (Exception e)
                        {
                            Utilities.LogException("Error in ReloadCacheEvent: UpdateResxFiles " + e.Message);
                        }
                        finally
                        {
                            if (web != null)
                                web.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in ReloadCacheEvent: UpdateResxFiles " + e.Message);
            }
        }

        public class SyncEvents
        {
            private readonly EventWaitHandle _newItemEvent;
            private readonly EventWaitHandle _exitThreadEvent;
            private readonly WaitHandle[] _eventArray;

            public SyncEvents()
            {
                _newItemEvent = new AutoResetEvent(false);
                _exitThreadEvent = new ManualResetEvent(false);
                _eventArray = new WaitHandle[2];
                _eventArray[0] = _newItemEvent;
                _eventArray[1] = _exitThreadEvent;
            }

            public EventWaitHandle ExitThreadEvent
            {
                get { return _exitThreadEvent; }
            }

            public EventWaitHandle NewItemEvent
            {
                get { return _newItemEvent; }
            }

            public WaitHandle[] EventArray
            {
                get { return _eventArray; }
            }
        }

        public class WebDefinitionHelper
        {
            public WebDefinitionHelper(SPListItem item, SPWeb web)
            {
                InternalItem = item;
                InternalWeb = web;
            }

            public SPWeb InternalWeb { get; set; }

            public SPListItem InternalItem { get; set; }
        }

        public class WebUpdateConsumer
        {
            private readonly Queue<WebDefinitionHelper> _queue;
            private readonly SyncEvents _syncEvents;

            public WebUpdateConsumer(Queue<WebDefinitionHelper> q, SyncEvents e)
            {
                _queue = q;
                _syncEvents = e;
            }

            public void ThreadRun()
            {
                WebDefinitionHelper webDefinitionHelper;

                while (WaitHandle.WaitAny(_syncEvents.EventArray) != 1)
                {
                    while (_queue.Count > 0)
                    {
                        lock (((ICollection)_queue).SyncRoot)
                        {
                            webDefinitionHelper = _queue.Peek();
                        }

                        UpdateResxFilesForWeb(webDefinitionHelper.InternalItem, webDefinitionHelper.InternalWeb);
                    }
                }
            }

            private static void UpdateResxFilesForWeb(SPListItem item, SPWeb web)
            {
                if (web.IsMultilingual)
                {
                    try
                    {
                        var outputFileStreamTarget = new MemoryStream();
                        var outputFileStreamTargetFinal = new MemoryStream();
                        var outputFileStreamDefault = new MemoryStream();

                        System.Globalization.CultureInfo defaultCulture = web.UICulture;
                        var defaultLanguageEntry = (string)item[Languages.Instance.GetBackwardCompatibilityLanguageCode(defaultCulture.TwoLetterISOLanguageName.ToUpper())];

                        if (Regex.IsMatch(defaultLanguageEntry.Trim(), "^[0-9]+$"))
                        {
                            outputFileStreamTarget.Dispose();
                            outputFileStreamTargetFinal.Dispose();
                            outputFileStreamDefault.Dispose();
                            web.Dispose();
                            return;
                        }

                        web.ExportUserResources(System.Globalization.CultureInfo.CreateSpecificCulture(defaultCulture.Name), true, outputFileStreamDefault);

                        string newResxFileString;
                        var keyListArray = new ArrayList();

                        outputFileStreamDefault.Position = 0;

                        using (var sr2 = new StreamReader(outputFileStreamDefault))
                        {
                            newResxFileString = sr2.ReadToEnd();

                            foreach (Match match in Regex.Matches(newResxFileString, "<data name=\\\"(?<key>([^\"]+))\\\"><value>(?<value>([^<]+))</value>"))
                            {
                                if (match.Groups["value"].Value == defaultLanguageEntry)
                                {
                                    keyListArray.Add(match.Groups["key"].Value);
                                }
                            }
                        }

                        if (keyListArray.Count > 0)
                        {
                            var dictionaries = HttpRuntime.Cache["OCEANIK_DICTIONARIES"] as Dictionaries;
                            
                            if (dictionaries != null)
                                foreach (LanguageItem languageItem in dictionaries.VisibleLanguages)
                                {
                                    if (languageItem.LanguageDestination != defaultCulture.TwoLetterISOLanguageName.ToUpper())
                                    {
                                        try
                                        {
                                            var value = (string)item[languageItem.LanguageDestination];

                                            if (string.IsNullOrEmpty(value))
                                            {
                                                value = defaultLanguageEntry;
                                            }

                                            web.ExportUserResources(System.Globalization.CultureInfo.GetCultureInfo(languageItem.Lcid), true, outputFileStreamTarget);

                                            outputFileStreamTarget.Position = 0;
                                            using (var sr2 = new StreamReader(outputFileStreamTarget))
                                            {
                                                newResxFileString = sr2.ReadToEnd();

                                                foreach (string key in keyListArray)
                                                {
                                                    foreach (Match match in Regex.Matches(newResxFileString, "<data name=\\\"" + key + "\\\"><value>(?<value>([^<]+))</value>"))
                                                    {
                                                        newResxFileString = newResxFileString.Replace(match.Value, "<data name=\"" + key + "\"><value>" + value + "</value>");
                                                    }

                                                    foreach (Match match in Regex.Matches(newResxFileString, "<data name=\\\"" + key + "\\\"><value />"))
                                                    {
                                                        newResxFileString = newResxFileString.Replace(match.Value, "<data name=\"" + key + "\"><value>" + value + "</value>");
                                                    }
                                                }

                                                outputFileStreamTargetFinal.Position = 0;
                                                byte[] data = Encoding.UTF8.GetBytes(newResxFileString);
                                                outputFileStreamTargetFinal.Write(data, 0, data.Length);
                                                outputFileStreamTargetFinal.Position = 0;

                                                web.ImportUserResources(System.Globalization.CultureInfo.GetCultureInfo(languageItem.Lcid), outputFileStreamTargetFinal);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Utilities.LogException("Error in ReloadCacheEvent: UpdateResxFilesForWeb for web " + web.Name + " and language " + languageItem.LanguageDestination + " and culture " + languageItem.Lcid + " " + e.Message);
                                        }
                                    }
                                }
                        }

                        web.Dispose();
                        outputFileStreamTarget.Dispose();
                        outputFileStreamTargetFinal.Dispose();
                        outputFileStreamDefault.Dispose();
                    }
                    catch (Exception e)
                    {
                        Utilities.LogException("Error in ReloadCacheEvent: UpdateResxFilesForWeb for web " + web.Name + " and Culture Name " + web.UICulture.Name + " and Two letters " + web.UICulture.TwoLetterISOLanguageName.ToUpper() + " " + e.Message);
                    }
                }

                lock (((ICollection)WebToUpdate).SyncRoot)
                {
                    WebToUpdate.Dequeue();
                }
            }
        }
    }
}
