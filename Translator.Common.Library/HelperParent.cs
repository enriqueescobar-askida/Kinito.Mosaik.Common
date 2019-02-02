// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelperParent.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the HelperParent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;

namespace Translator.Common.Library
{
    public class HelperParent
    {
        public HelperParent(string siteUrl, string siteName, string listName)
        {
            SiteUrl = siteUrl;
            SiteName = siteName;
            ListName = listName;
        }

        public HelperParent()
        {
            SiteUrl = string.Empty;
            SiteName = string.Empty;
            ListName = string.Empty;
        }

        public HelperParent(string siteUrl, string siteName)
        {
            SiteUrl = siteUrl;
            SiteName = siteName;
            ListName = string.Empty;
        }

        public string SiteUrl { get; set; }

        public string SiteName { get; set; }

        public string ListName { get; set; }

        public SPSite GetSiteCollection()
        {
            if (!IsSiteCollectionProcessValid())
                return null;
            SPSite siteCollection = null;
            try
            {
                siteCollection = new SPSite(SiteUrl);
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetSiteCollection", "ClsHelperParent"));
                log.WriteToLog();
            }
            finally
            {
                if (siteCollection != null)
                    siteCollection.Dispose();
            }

            return siteCollection;
        }

        public SPWeb GetWebSite()
        {
            if (!IsWebProcessValid())
                return null;
            SPWeb site = null;
            try
            {
                using (SPSite siteCollection = GetSiteCollection())
                {
                    if (siteCollection != null)
                        site = siteCollection.AllWebs[SiteName];
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetWebSite", "ClsHelperParent"));
                log.WriteToLog();
            }
            finally
            {
                if (site != null)
                    site.Dispose();
            }

            return site;
        }

        public SPList GetList()
        {
            if (!IsListProcessValid())
                return null;

            SPList list = null;
            try
            {
                using (SPWeb site = GetWebSite())
                {
                    if (site != null)
                        list = site.Lists[ListName];
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetList", "ClsHelperParent"));
                log.WriteToLog();
            }

            return list;
        }

        public SPListItemCollection GetItemCollection()
        {
            SPListItemCollection items = null;
            try
            {
                if (GetList() == null)
                    return null;

                SPList list = GetList();

                if (list != null)
                    items = list.Items;
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetItemCollection", "ClsHelperParent"));
                log.WriteToLog();
            }

            return items;
        }

        public SPListItemCollection GetItemCollection(string query)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            SPListItemCollection items = null;
            try
            {
                SPList sharepointList = GetList();

                if (sharepointList == null)
                    return null;

                /*bool enabledThrottling = sharepointList.EnableThrottling;

                if (enabledThrottling)
                {
                    sharepointList.EnableThrottling = false;
                }*/

                var sharepointQuery = new SPQuery
                {
                    Query = query,
                    // QueryThrottleMode = SPQueryThrottleOption.Override
                };

                items = sharepointList.GetItems(sharepointQuery);

                /*if (enabledThrottling)
                {
                    sharepointList.EnableThrottling = true;
                }*/
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetItemCollection", "ClsHelperParent"));
                log.WriteToLog();
            }

            return items;
        }

        public UserProfileManager GetProfileManager()
        {
            UserProfileManager profileManager = null;
            try
            {
                using (SPSite siteCollection = GetSiteCollection())
                {
                    if (siteCollection == null)
                        return null;

                    var context = SPServiceContext.GetContext(siteCollection);
                    profileManager = new UserProfileManager(context);
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetProfileManager", "ClsHelperParent"));
                log.WriteToLog();
            }

            return profileManager;
        }

        private bool IsListProcessValid()
        {
            if (!IsWebProcessValid())
                return false;
            if (string.IsNullOrEmpty(ListName))
                return false;

            // process is valid
            return true;
        }

        private bool IsWebProcessValid()
        {
            if (!IsSiteCollectionProcessValid())
                return false;
            if (string.IsNullOrEmpty(SiteName))
                return false;

            // process is valid
            return true;
        }

        private bool IsSiteCollectionProcessValid()
        {
            if (string.IsNullOrEmpty(SiteUrl))
                return false;

            // process is valid
            return true;
        }
    }
}