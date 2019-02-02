// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="AlphaMosaik">
//   Copyright © AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Utilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace Alphamosaik.Common.SharePoint.Library
{
    public class Utilities
    {
        public static string GetHttpContextPath()
        {
            string path = string.Empty;

            try
            {
                if (HttpContext.Current != null)
                {
                    path = HttpContext.Current.Request.Path;
                }
            }
            catch (System.Exception e)
            {
                Trace.WriteLine(e);
            }

            return path;
        }

        public static void CreateEventLogSource(string source)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
                                                     {
                                                         // Create the source, if it does not already exist.
                                                         if (!EventLog.SourceExists(source))
                                                         {
                                                             // An event log source should not be created and immediately used.
                                                             // There is a latency time to enable the source, it should be created
                                                             // prior to executing the application that uses the source.
                                                             // Execute this sample a second time to use the new source.
                                                             EventLog.CreateEventSource(source, "Application");
                                                             Trace.WriteLine("CreatingEventSource");
                                                             Trace.WriteLine("Exiting, execute the application a second time to use the source.");

                                                             // The source is created.  Exit the application to allow it to be registered.
                                                             return;
                                                         }
                                                     });
        }

        public static SPBasePermissions GetPermissionsForUser(SPUser user, SPWeb web)
        {
            SPUserToken userToken = user.UserToken;

#if SP2007
            MethodInfo getPermissions = typeof (SPUtility).GetMethod("GetPermissions",
                                                                                                    BindingFlags.NonPublic | BindingFlags.Instance |
                                                                                                    BindingFlags.InvokeMethod | BindingFlags.Static,
                                                                                                    null, new[] {
                                                                                                            typeof(SPUserToken),
                                                                                                            typeof(ISecurableObject) }, null);
#else
            MethodInfo getPermissions = typeof(SPUtility).GetMethod(
                "GetPermissions",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Static,
                null,
                new[] { typeof(SPUserToken), typeof(SPSecurableObject) },
                null);
#endif

            var perms = (SPBasePermissions)getPermissions.Invoke(null, new object[] { userToken, web });

            return perms;
        }

        public static bool IsUserHaveEditRight()
        {
            if (SPContext.Current != null && SPContext.Current.Web != null)
            {
                if (SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.EditListItems))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetAbsoluteUri(HttpApplication context)
        {
            Uri url = null;

            try
            {
                url = context.Context.Request.Url;
            }
            catch (HttpException)
            {
                // Request is not set. Use Reflection to get Path.
                FieldInfo requestInfo = context.Context.GetType().GetField("_request", BindingFlags.NonPublic | BindingFlags.Instance);

                if (requestInfo != null)
                {
                    var request = requestInfo.GetValue(context.Context) as HttpRequest;

                    if (request != null)
                    {
                        url = request.Url;
                    }
                }
            }

            if (url != null)
            {
                string absoluteUri = url.AbsoluteUri.Substring(0, url.AbsoluteUri.Length - url.PathAndQuery.Length);

                if (!absoluteUri.EndsWith("/"))
                {
                    absoluteUri = absoluteUri + "/";
                }

                return absoluteUri;
            }

            return string.Empty;
        }

        public static string GetLanguageFromMsCookie(HttpContext context)
        {
            try
            {
                HttpCookie httpCookie = context.Request.Cookies.Get("lcid");

                if (httpCookie != null)
                {
                    CultureInfo cultureInfo = CultureInfo.GetCultureInfo(Convert.ToInt32(httpCookie.Value));
                    return cultureInfo.TwoLetterISOLanguageName.ToUpper();
                }
            }
            catch (System.Exception e)
            {
                Trace.WriteLine(e);
            }

            return string.Empty;
        }

        public static int GetLcidFromMsCookie(HttpContext context)
        {
            try
            {
                HttpCookie httpCookie = context.Request.Cookies.Get("lcid");

                if (httpCookie != null)
                {
                    return Convert.ToInt32(httpCookie.Value);
                }
            }
            catch (System.Exception e)
            {
                Trace.WriteLine(e);
            }

            return -1;
        }

        public static string GetLanguageCode(HttpContext context)
        {
            // On se base en priorité sur la langue du language pack MS utilisé
            if (!string.IsNullOrEmpty(GetLanguageFromMsCookie(context)))
            {
                return GetLanguageFromMsCookie(context);
            }

            HttpCookie httpCookie = context.Request.Cookies.Get("SharePointTranslator");

            if (httpCookie != null)
            {
                return httpCookie.Value;
            }

            if (context.Request.UserLanguages != null)
            {
                return context.Request.UserLanguages[0].Substring(0, 2).ToUpper();
            }

            return HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"] != null ? HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"].ToString() : "EN";
        }

        public static string GetUrlWithNoQueries(HttpContext context)
        {
            Uri uri = context.Request.Url;
            string fullUrl = uri.AbsoluteUri.ToLower();

            if (!string.IsNullOrEmpty(uri.Query))
            {
                fullUrl = fullUrl.Replace(uri.Query, string.Empty);
            }

            return fullUrl;
        }

        public static string FilterUrl(string url)
        {
            string[] sections = url.Split('/');

            if (sections.Length < 3)
            {
                return string.Empty;
            }

            return sections[0] + "//" + sections[2] + "/";
        }

        public static string GetRootUrl()
        {
            return FilterUrl(SPContext.Current.Site.Url);
        }

        public static string GetRootUrlFromConfig()
        {
            return FilterUrl(ConfigStore.ConfigStore.Instance.GetGlobalConfigSiteAppSettingsKey());
        }

        public static string GetUrlCurrentList(SPList currentList)
        {
            if (SPContext.Current != null && currentList != null)
            {
                string listurl = SPContext.Current.Web.ServerRelativeUrl; 

                if (listurl == "/") 
                    listurl = string.Empty;

                return SPContext.Current.Web.Url + listurl + "/" + currentList.RootFolder.Url;
            }

            return string.Empty;
        }

        public static SPFolder GetCurrentFolderInList(SPList currentList)
        {
            SPFolder folder = null;

            string rootFolder = HttpContext.Current.Request.QueryString["RootFolder"];

            if (SPContext.Current != null && currentList != null && !string.IsNullOrEmpty(rootFolder))
            {
                folder = SPContext.Current.Web.GetFolder(rootFolder);

                try
                {
                    // try Get the folder's Guid to verify folder exist
                    Guid id = folder.UniqueId;
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    folder = currentList.RootFolder;
                }
            }

            return folder;
        }
    }
}
