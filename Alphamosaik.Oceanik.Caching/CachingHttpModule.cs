// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingHttpModule.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the CachingHttpModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.UI.WebControls.WebParts;
using Alphamosaik.Common.Library;
using Alphamosaik.Common.Library.Licensing;
using Alphamosaik.Common.SharePoint.Library;
using Alphamosaik.Common.SharePoint.Library.ConfigStore;
using Microsoft.Office.Server.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.WebControls;
using Microsoft.SharePoint.WebPartPages;
using Exception = System.Exception;
using WebPart = Microsoft.SharePoint.WebPartPages.WebPart;

namespace Alphamosaik.Oceanik.Caching
{
    public class CachingHttpModule : IHttpModule
    {
#if !SP2007
        private const string EventSourceName = "Oceanik";
        private static readonly string ServerPrivateKey = PrivateKey.GetPrivateKey("CachingHttpModule");
        private static License _license;
#else
        private const string EventSourceName = "Translator2009";
#endif
        private static readonly object LockThis = new object();
        private static StringCollection _servers = new StringCollection();
        private static bool _serverListLoaded;

        internal CachingHttpModule()
        {
        }

        public void Init(HttpApplication context)
        {
#if !SP2007
            if (BlackList.IsBlackListed(ServerPrivateKey))
            {
                Common.SharePoint.Library.Exception.LogException(EventSourceName, "CachingHttpModule 2011 License is invalid!");
                return;
            }
#endif
            try
            {
                lock (LockThis)
                {
#if !SP2007
                    string absoluteUri = Utilities.GetAbsoluteUri(context);

                    if (_license == null)
                    {
                        string value = ConfigStore.Instance.GetValue("CachingHttpModule", Environment.MachineName, absoluteUri);

                        if (string.IsNullOrEmpty(value))
                        {
                            byte[] bytes = Encoding.ASCII.GetBytes(ServerPrivateKey);
                            ConfigStore.Instance.AddValue(Environment.MachineName, "CachingHttpModule", "ServerPrivateKey", string.Empty, "license.key", bytes, false, absoluteUri);
                        }

                        string license = ConfigStore.Instance.GetSpecificStringAttachment("CachingHttpModule", Environment.MachineName, "license.dat", absoluteUri);

                        _license = new License();

                        if (!string.IsNullOrEmpty(license))
                        {
                            _license = new License(ServerPrivateKey, license);
                        }
                    }
#endif
                    
                    if (!IsTrialFinished())
                    {
                        context.ResolveRequestCache += OnResolveRequestCache;
                        context.UpdateRequestCache += OnUpdateRequestCache;
                        context.ReleaseRequestState += OnContextReleaseRequestState;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.SharePoint.Library.Exception.LogException(EventSourceName, "Error in CachingHttpModule.Init: " + ex.Message, EventLogEntryType.Warning);
            }
        }

        public void Dispose()
        {
        }

        internal string CreateOutputCachedItemKey(string path, HttpContext context)
        {
            var builder = new StringBuilder("a2_", path.Length + "a2_".Length);

            builder.Append("p:" + CultureInfo.InvariantCulture.TextInfo.ToLower(path) + "_");

            string langCookie = Utilities.GetLanguageCode(context);

#if SP2007
            if (context.Request.QueryString["SPSLanguage"] != null)
                langCookie = context.Request.QueryString["SPSLanguage"];
#endif

            builder.Append("c:" + langCookie + "_");

            // Use Query String to build key
            if (context.Request.QueryString.Count > 0)
            {
                string[] allKeys = context.Request.QueryString.AllKeys;

                Array.Sort(allKeys);

                foreach (string key in allKeys)
                {
#if SP2007
                    if (key.IndexOf("SPSLanguage", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        continue;
                    }
#endif
                    builder.Append("k:" + CultureInfo.InvariantCulture.TextInfo.ToLower(key) + "_v:" +
                                   CultureInfo.InvariantCulture.TextInfo.ToLower(context.Request.QueryString[key]) + "_");
                }
            }

            // Use User Right to build key
            if (context.User.Identity.IsAuthenticated)
            {
                if (SPContext.Current != null && SPContext.Current.Web.CurrentUser != null)
                {
                    SPRoleDefinitionBindingCollection roleDefinitionBindingCollection =
                        SPContext.Current.Web.AllRolesForCurrentUser;

                    var roleDefinitionNameCollection = new StringCollection();

                    foreach (SPRoleDefinition roleDefinition in roleDefinitionBindingCollection)
                    {
                        roleDefinitionNameCollection.Add(roleDefinition.Name);
                    }

                    ArrayList.Adapter(roleDefinitionNameCollection).Sort();

                    builder.Append("p:");

                    foreach (string permission in roleDefinitionNameCollection)
                    {
                        builder.Append(permission + ",");
                    }

                    builder.Append("_");

                    var groupCollection = new StringCollection();

                    GetUserGroupsCollection(context, groupCollection);

                    if (groupCollection.Count > 0)
                    {
                        ArrayList.Adapter(groupCollection).Sort();

                        foreach (string groupName in groupCollection)
                        {
                            builder.Append("sp:" + CultureInfo.InvariantCulture.TextInfo.ToLower(groupName) + "_");
                        }
                    }
                }
            }

            // The cache is varied by browser name and major version information.
            builder.Append("b:" + CultureInfo.InvariantCulture.TextInfo.ToLower(context.Request.Browser.Browser) + "_v:" +
                           context.Request.Browser.MajorVersion + "_");

            Trace.WriteLine(builder);

            return builder.ToString();
        }

        internal void OnUpdateRequestCache(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            HttpContext context = application.Context;

#if DEBUG
            bool isAspx = context.Request.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase);

            if (isAspx)
            {
                lock (LockThis)
                {
                    Trace.WriteLine("------------------------------------------------START CONTEXT-------------------------------------------------------------------");

                    var objectDump = new StringBuilder();
                    DumpObject.Dump(context, objectDump, BindingFlags.Public | BindingFlags.NonPublic);

                    Trace.WriteLine(objectDump.ToString());

                    Trace.WriteLine("------------------------------------------------END CONTEXT--------------------------------------------------------------------");

                    Trace.WriteLine("------------------------------------------------START REQUEST-------------------------------------------------------------------");

                    objectDump.Clear();
                    DumpObject.Dump(context.Request, objectDump, BindingFlags.Public | BindingFlags.NonPublic);

                    Trace.WriteLine(objectDump.ToString());

                    Trace.WriteLine("------------------------------------------------END REQUEST--------------------------------------------------------------------");

                    Trace.WriteLine("------------------------------------------------START RESPONSE-------------------------------------------------------------------");

                    objectDump.Clear();
                    DumpObject.Dump(context.Response, objectDump, BindingFlags.Public | BindingFlags.NonPublic);

                    Trace.WriteLine(objectDump.ToString());

                    Trace.WriteLine("------------------------------------------------END RESPONSE--------------------------------------------------------------------");
                }
            }
#endif

            string url = Utilities.GetUrlWithNoQueries(context);

            if (IsValidRequestToCache(context, url))
            {
                bool flag = IsCacheable(context);

                if (flag)
                {
                    string key = CreateOutputCachedItemKey(url, context);

                    RawResponse rawResponse = GetSnapshot(context.Response);

                    CacheDependency dependencies = GetDependencies(context, url);

                    context.Cache.Insert(key, rawResponse, dependencies, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.Low, null);

                    if (!_serverListLoaded)
                    {
                        SPSecurity.RunWithElevatedPrivileges(delegate
                        {
                            _servers = LoadBalancingServerList(application);
                        });
                    }

                    if (_servers.Count > 0)
                    {
                        string currentSiteUrl = SPContext.Current.Site.Url;

                        if (!currentSiteUrl.EndsWith("/"))
                        {
                            currentSiteUrl = currentSiteUrl + "/";
                        }

                        try
                        {
                            SPSecurity.RunWithElevatedPrivileges(
                                () => UpdateServersCache(key, rawResponse, url, currentSiteUrl));
                        }
                        catch (Exception ex)
                        {
                            Common.SharePoint.Library.Exception.LogException(EventSourceName,
                    "Error in Alphamosaik.Translator.Caching.CachingHttpModule.GetUserGroupsCollection: " + ex.Message,
                    EventLogEntryType.Warning);
                        }
                    }

                    Trace.WriteLine("EffectivePercentagePhysicalMemoryLimit: " +
                                    context.Cache.EffectivePercentagePhysicalMemoryLimit);
                    Trace.WriteLine("EffectivePrivateBytesLimit: " + context.Cache.EffectivePrivateBytesLimit);
                }
            }
        }

        internal void OnResolveRequestCache(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            HttpContext context = application.Context;

            string url = Utilities.GetUrlWithNoQueries(context);

            if (IsValidRequestToCache(context, url))
            {
                string key = CreateOutputCachedItemKey(url, context);

                var rawResponse = context.Cache.Get(key) as RawResponse;

                if (rawResponse != null)
                {
                    Trace.WriteLine("Found: " + key);

                    context.Response.ClearHeaders();
                    context.Response.Clear();
                    context.Response.StatusCode = rawResponse.StatusCode;
                    context.Response.StatusDescription = rawResponse.StatusDescription;

                    NameValueCollection headers = rawResponse.Headers;
                    int num = (headers != null) ? headers.Count : 0;
                    for (int i = 0; i < num; i++)
                    {
                        if (headers != null)
                            context.Response.AppendHeader(headers.GetKey(i), headers.Get(i));
                    }

                    foreach (var buffer in rawResponse.Buffers)
                    {
                        var responseSubstBlockElement = buffer as ResponseSubstBlockElement;

                        if (responseSubstBlockElement != null)
                        {
                            context.Response.WriteSubstitution(responseSubstBlockElement.Callback);
                        }
                        else
                        {
                            var responseBufferElement = buffer as ResponseBufferElement;

                            if (responseBufferElement != null)
                            {
                                context.Response.BinaryWrite(responseBufferElement.GetBytes());
                            }
                        }
                    }

                    context.Response.Write(string.Format(Thread.CurrentThread.CurrentUICulture,
                                                                     "<!-- Rendered using " + EventSourceName +
                                                                     " First Level Cache created: {0} -->",
                                                                     new object[]
                                                                         {
                                                                             rawResponse.Created.ToString("s",
                                                                                                          CultureInfo.
                                                                                                              InvariantCulture)
                                                                         }));

                    /*FieldInfo httpWritterInfo = context.Response.GetType().GetField("_httpWriter",
                                                                                    BindingFlags.NonPublic |
                                                                                    BindingFlags.Instance);

                    if (httpWritterInfo != null)
                    {
                        var httpWritter = (HttpWriter)httpWritterInfo.GetValue(context.Response);

                        if (httpWritter != null)
                        {
                            MethodInfo useSnapshot = httpWritter.GetType().GetMethod("UseSnapshot",
                                                                                     BindingFlags.NonPublic |
                                                                                     BindingFlags.Instance |
                                                                                     BindingFlags.InvokeMethod);

                            if (useSnapshot != null)
                            {
                                useSnapshot.Invoke(httpWritter, new object[] { rawResponse.Buffers });
                                context.Response.Write(string.Format(Thread.CurrentThread.CurrentUICulture,
                                                                     "<!-- Rendered using " + EventSourceName +
                                                                     " First Level Cache created: {0} -->",
                                                                     new object[]
                                                                         {
                                                                             rawResponse.Created.ToString("s",
                                                                                                          CultureInfo.
                                                                                                              InvariantCulture)
                                                                         }));
                            }
                        }
                    }*/

                    context.Response.SuppressContent = false;
                    application.CompleteRequest();
                }
            }
        }

        internal void OnContextReleaseRequestState(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            HttpContext context = application.Context;

            string url = Utilities.GetUrlWithNoQueries(context);

            if (IsValidRequestToCache(context, url))
            {
                context.Response.Filter = new ResponseFilterCacheStream(context.Response.Filter);
            }
        }

        internal bool IsCacheable(HttpContext context)
        {
            if (context.Response.StatusCode == 200)
            {
                // anonymous
                if (!context.User.Identity.IsAuthenticated)
                    return true;

                if (IsBuffered(context.Response) && context.Items.Contains("OceanikPageCache"))
                {
                    var enable = (bool)context.Items["OceanikPageCache"];

                    return enable;
                }
            }

            return false;
        }

        internal bool IsBuffered(HttpResponse response)
        {
            bool isBuffered = false;

            MethodInfo isBufferedInfo = response.GetType().GetMethod("IsBuffered",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.InvokeMethod);

            if (isBufferedInfo != null)
            {
                isBuffered = (bool)isBufferedInfo.Invoke(response, null);
            }

            return isBuffered;
        }

        internal HttpCacheability GetCacheability(HttpCachePolicy cache)
        {
            FieldInfo cacheabilityInfo = cache.GetType().GetField("_cacheability",
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.Instance);

            if (cacheabilityInfo != null)
            {
                return (HttpCacheability)Enum.Parse(typeof(HttpCacheability), cacheabilityInfo.GetValue(cache).ToString());
            }

            return HttpCacheability.NoCache;
        }

        internal bool IsModified(HttpCachePolicy cache)
        {
            MethodInfo isModifiedInfo = cache.GetType().GetMethod("IsModified",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.InvokeMethod);

            if (isModifiedInfo != null)
            {
                return (bool)isModifiedInfo.Invoke(cache, null);
            }

            return false;
        }

        internal bool HasCachePolicy(HttpResponse response)
        {
            PropertyInfo hasCachePolicyInfo = response.GetType().GetProperty("HasCachePolicy",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance);

            if (hasCachePolicyInfo != null)
            {
                return (bool)hasCachePolicyInfo.GetValue(response, null);
            }

            return false;
        }

        internal bool RequestRequiresAuthorization(HttpContext context)
        {
            MethodInfo requestRequiresAuthorizationInfo = context.GetType().GetMethod("RequestRequiresAuthorization",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.InvokeMethod);

            if (requestRequiresAuthorizationInfo != null)
            {
                return (bool)requestRequiresAuthorizationInfo.Invoke(context, null);
            }

            return false;
        }

        internal bool GetNoServerCaching(HttpCachePolicy cache)
        {
            MethodInfo getNoServerCachingInfo = cache.GetType().GetMethod("GetNoServerCaching",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.InvokeMethod);

            if (getNoServerCachingInfo != null)
            {
                return (bool)getNoServerCachingInfo.Invoke(cache, null);
            }

            return true;
        }

        internal bool HasExpirationPolicy(HttpCachePolicy cache)
        {
            MethodInfo hasExpirationPolicyInfo = cache.GetType().GetMethod("HasExpirationPolicy",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.InvokeMethod);

            if (hasExpirationPolicyInfo != null)
            {
                return (bool)hasExpirationPolicyInfo.Invoke(cache, null);
            }

            return false;
        }

        internal bool HasValidationPolicy(HttpCachePolicy cache)
        {
            MethodInfo hasValidationPolicyInfo = cache.GetType().GetMethod("HasValidationPolicy",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.InvokeMethod);

            if (hasValidationPolicyInfo != null)
            {
                return (bool)hasValidationPolicyInfo.Invoke(cache, null);
            }

            return false;
        }

        internal void OnPostReleaseRequestState(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            if (context != null)
            {
                foreach (DictionaryEntry dictionaryEntry in HttpContext.Current.Items)
                {
                    Trace.WriteLine(dictionaryEntry.Key);
                }

                if (context.Request.RequestType != "POST" && !context.Request.Url.AbsolutePath.StartsWith("/_LAYOUTS/", StringComparison.OrdinalIgnoreCase))
                {
                    var loader = (AudienceLoader)HttpContext.Current.Items["__SPS_AudienceLoader_Audiences"];

                    string audienceVaryByCustom = GetAudienceVaryByCustom();
                    Trace.WriteLine(audienceVaryByCustom);

                    //// context.Response.Cache.SetVaryByCustom(audienceVaryByCustom);
                }
            }
        }

        internal ArrayList GetCurrentUserAdGroups()
        {
            // Get the current groups for the logged in user;
            var groups = new ArrayList();
            if (HttpContext.Current.Request.LogonUserIdentity.Groups != null)
                foreach (System.Security.Principal.IdentityReference group in HttpContext.Current.Request.LogonUserIdentity.Groups)
                {
                    groups.Add(group.Translate(typeof(System.Security.Principal.NTAccount)).ToString());
                }

            return groups;
        }

        internal ArrayList GetSnapshot(HttpResponse response, out bool hasSubstBlocks)
        {
            hasSubstBlocks = false;

            MethodInfo getSnapshot = response.GetType().GetMethod("GetSnapshot",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.InvokeMethod);

            if (getSnapshot != null)
            {
                var httpRawResponse = getSnapshot.Invoke(response, null);

                PropertyInfo hasSubstBlocksInfo = httpRawResponse.GetType().GetProperty("HasSubstBlocks",
                                                                                        BindingFlags.NonPublic |
                                                                                        BindingFlags.Instance);

                if (hasSubstBlocksInfo != null)
                {
                    hasSubstBlocks = (bool)hasSubstBlocksInfo.GetValue(httpRawResponse, null);
                }

                PropertyInfo buffersInfo = httpRawResponse.GetType().GetProperty("Buffers", BindingFlags.NonPublic |
                                                                                            BindingFlags.Instance);

                if (buffersInfo != null)
                {
                    return (ArrayList)buffersInfo.GetValue(httpRawResponse, null);
                }
            }

            return null;
        }

        internal RawResponse GetSnapshot(HttpResponse response)
        {
            bool hasSubstBlocks;
            ArrayList buffers = GetSnapshot(response, out hasSubstBlocks);
            int statusCode = response.StatusCode;
            string statusDescription = response.StatusDescription;
            var headers  = new NameValueCollection();

            FieldInfo headersInfo = response.GetType().GetField("_headers",
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.Instance);

            if (headersInfo != null)
            {
                headers = (NameValueCollection)headersInfo.GetValue(response);
            }

            return new RawResponse(statusCode, statusDescription, headers, buffers, hasSubstBlocks);
        }

        internal bool IsValidRequestToCache(HttpContext context, string url)
        {
            bool isAspx = context.Request.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase);
            bool isNotStartWithLayouts = context.Request.Path.StartsWith("/_layouts");
            bool isCrawler = IsCrawler(context);

            if (isAspx && !isNotStartWithLayouts && !isCrawler)
            {
                if (context.Request.RequestType.IndexOf("POST", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    // Page have been modified clear cache for this page
                    string dependencyKey = "CachingHttpModule_" + url + "_";
                    context.Cache.Remove(dependencyKey);
                    return false;
                }

                if (context.Request.RequestType.IndexOf("GET", StringComparison.OrdinalIgnoreCase) > -1 &&
                    !IsEditPageMode() && !Utilities.IsUserHaveEditRight())
                {
                    return true;
                }
            }

            return false;
        }

        internal bool IsCrawler(HttpContext context)
        {
            bool requestFromCrawler = false;
            if (context.Request.Browser != null)
            {
                requestFromCrawler = context.Request.Browser.Crawler;
            }

            // Microsoft doesn't properly detect several crawlers 
            if ((!requestFromCrawler) && (!string.IsNullOrEmpty(context.Request.UserAgent)))
            {
                if (context.Request.UserAgent.IndexOf("MS Search ", StringComparison.OrdinalIgnoreCase) > -1 ||
                    context.Request.UserAgent.IndexOf("FrontPage", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    requestFromCrawler = true;
                }
            }

            return requestFromCrawler;
        }

        internal bool IsEditPageMode()
        {
            try
            {
                if (SPContext.Current != null && SPContext.Current.FormContext != null &&
                    SPContext.Current.FormContext.FormMode == Microsoft.SharePoint.WebControls.SPControlMode.Edit)
                {
                    return true;
                }

                string webPartManagerDisplayModeNameValue =
                    HttpContext.Current.Request.Form["MSOSPWebPartManager_DisplayModeName"];

                if (!string.IsNullOrEmpty(webPartManagerDisplayModeNameValue))
                {
                    if (webPartManagerDisplayModeNameValue.IndexOf("Design", StringComparison.OrdinalIgnoreCase) != -1 ||
                        webPartManagerDisplayModeNameValue.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        string webPartManagerStartEditNameValue =
                            HttpContext.Current.Request.Form["MSOSPWebPartManager_StartWebPartEditingName"];

                        if (!string.IsNullOrEmpty(webPartManagerStartEditNameValue) &&
                            webPartManagerStartEditNameValue.IndexOf("true", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        internal StringCollection LoadBalancingServerList(HttpApplication context)
        {
            var listServer = new StringCollection();

            string rootUrl = Utilities.GetAbsoluteUri(context);

            rootUrl = Utilities.FilterUrl(rootUrl); // root server url

            using (var site = new SPSite(rootUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
#if !SP2007
                    SPList list = web.Lists.TryGetList("LoadBalancingServers");
#else
                    SPList list = web.Lists["LoadBalancingServers"];
#endif

                    if (list != null)
                    {
                        foreach (SPListItem server in list.Items)
                        {
                            listServer.Add(server["Title"].ToString());
                        }

                        _serverListLoaded = true;
                    }
                }
            }

            return listServer;
        }

        internal void UpdateServersCache(string key, RawResponse rawResponse, string pathUrl, string currentSiteUrl)
        {
            foreach (string server in _servers)
            {
#pragma warning disable 612,618
                ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
#pragma warning restore 612,618

                NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["CacheLevel"] = "First";
                queryString["key"] = key;
                queryString["pathUrl"] = pathUrl;
                queryString["currentSiteUrl"] = currentSiteUrl;

                queryString.ToString();

                var httpHandlerUrlBuilder = new UriBuilder(server + "_layouts/OceanikCacheSync.ashx")
                                                {
                                                    Query = queryString.ToString()
                                                };

                var req = (HttpWebRequest)WebRequest.Create(httpHandlerUrlBuilder.Uri);
                req.Method = "POST";
                req.Credentials = CredentialCache.DefaultCredentials;
                
                byte[] rawResponseBytes = SerializeObject<RawResponse>.Object2ByteArray(rawResponse);
                req.ContentLength = rawResponseBytes.Length;
                req.ContentType = "text/xml";

                Stream requestStream = req.GetRequestStream();
                requestStream.Write(rawResponseBytes, 0, rawResponseBytes.Length);
                requestStream.Close();
                req.GetResponse();
            }
        }

        private static void GetUserGroupsCollection(HttpContext context, StringCollection groupCollection)
        {
            try
            {
                SPLimitedWebPartManager limitedWebPartManager =
                    SPContext.Current.Web.GetLimitedWebPartManager(context.Request.RawUrl, PersonalizationScope.User);

                if (limitedWebPartManager != null)
                {
                    foreach (WebPart webPart in limitedWebPartManager.WebParts)
                    {
                        if (!string.IsNullOrEmpty(webPart.AuthorizationFilter))
                        {
                            foreach (SPGroup group in SPContext.Current.Web.CurrentUser.Groups)
                            {
                                if (!groupCollection.Contains(group.Name))
                                {
                                    if (
                                        webPart.AuthorizationFilter.IndexOf(group.Name,
                                                                            StringComparison.OrdinalIgnoreCase) > -1)
                                    {
                                        groupCollection.Add(group.Name);
                                    }
                                }
                            }
                        }

                        var contentByQueryWebPart = webPart as ContentByQueryWebPart;

                        if (contentByQueryWebPart != null && contentByQueryWebPart.FilterByAudience)
                        {
                            // if a CQWP is present and Filter By Audience is On, add all groups and stop processing
                            foreach (SPGroup group in SPContext.Current.Web.CurrentUser.Groups)
                            {
                                if (!groupCollection.Contains(group.Name))
                                {
                                    groupCollection.Add(group.Name);
                                }
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.SharePoint.Library.Exception.LogException(EventSourceName, 
                    "Error in Alphamosaik.Translator.Caching.CachingHttpModule.GetUserGroupsCollection: " + ex.Message,
                    EventLogEntryType.Warning);
            }
        }

        private static bool IsTrialFinished()
        {
#if !SP2007
            if (_license != null && _license.IsValide)
            {
                if ((DateTime.Now.Date >= _license.TrialStart.Date) && (DateTime.Now.Date <= _license.TrialEnd.Date))
                {
                    return false;
                }

                Common.SharePoint.Library.Exception.LogException(EventSourceName, "CachingHttpModule 2011 License has expired");
                return true;
            }
#endif
            // Grace Period if no license was provided to client
            var trialBeginDate = new DateTime(2010, 11, 01);

            // Set To DateMax for non trial version
            var trialEndDate = new DateTime(2011, 04, 01);

            // Set To DateMin for non trial version
            if ((DateTime.Now.Date < trialBeginDate.Date) || (DateTime.Now.Date > trialEndDate.Date))
            {
                Common.SharePoint.Library.Exception.LogException(EventSourceName, "CachingHttpModule 2011 License has expired");
                return true;
            }

            return false;
        }

        private static CacheDependency GetDependencies(HttpContext context, string url)
        {
            string pathDependencyKey = "CachingHttpModule_" + url + "_";

            if (context.Cache.Get(pathDependencyKey) == null)
            {
                context.Cache.Insert(pathDependencyKey, pathDependencyKey, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.Low, null);
            }

            string currentSiteUrl = SPContext.Current.Site.Url;

            if (!currentSiteUrl.EndsWith("/"))
            {
                currentSiteUrl = currentSiteUrl + "/";
            }

            string siteDependencyKey = "CachingHttpModule_" + currentSiteUrl + "_";

            if (context.Cache.Get(siteDependencyKey) == null)
            {
                context.Cache.Insert(siteDependencyKey, siteDependencyKey, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.Low, null);
            }

            return new CacheDependency(null, new[] { pathDependencyKey, siteDependencyKey }, DateTime.Now.AddSeconds(10));
        }

        private static string GetAudienceVaryByCustom()
        {
            var audiences = new Dictionary<Guid, bool>();
            var distributionLists = new Dictionary<string, bool>();
            var siteGroups = new Dictionary<string, bool>();

            MethodInfo getAudiencesFetchedDuringPageRequest = typeof(AudienceLoader).GetMethod("GetAudiencesFetchedDuringPageRequest",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

            if (getAudiencesFetchedDuringPageRequest != null)
            {
                var args = new object[] { audiences, distributionLists, siteGroups };
                getAudiencesFetchedDuringPageRequest.Invoke(null, args);

                audiences = (Dictionary<Guid, bool>)args[0];
                distributionLists = (Dictionary<string, bool>)args[1];
                siteGroups = (Dictionary<string, bool>)args[2];
            }

            var builder = new StringBuilder();
            if (audiences.Count > 0)
            {
                builder.Append(";");
                builder.Append("audiencetargeting");
                foreach (Guid guid in audiences.Keys)
                {
                    builder.Append("A" + guid);
                    builder.Append("|");
                }
            }

            if (distributionLists.Count > 0)
            {
                if (builder.Length == 0)
                {
                    builder.Append(";");
                    builder.Append("audiencetargeting");
                }

                foreach (string str in distributionLists.Keys)
                {
                    builder.Append("D" + str);
                    builder.Append("|");
                }
            }

            if (siteGroups.Count > 0)
            {
                if (builder.Length == 0)
                {
                    builder.Append(";");
                    builder.Append("audiencetargeting");
                }

                foreach (string str2 in siteGroups.Keys)
                {
                    builder.Append("G" + str2);
                    builder.Append("|");
                }
            }

            return builder.ToString();
        }
    }
}
