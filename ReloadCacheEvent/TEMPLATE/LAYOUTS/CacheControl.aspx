<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%> 
<%@ Page Language="C#" %> 
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %> 
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="Microsoft.SharePoint" %>

<% 
    if (Request.QueryString.Get("list") == "TranslationContentsOrLanguagesVisibility")
    {
        SPSecurity.RunWithElevatedPrivileges(delegate()
        {
            StringCollection cacheLoadedKey = new StringCollection();

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

            string currentSiteUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, string.Empty);

            if (!currentSiteUrl.EndsWith("/"))
            {
                currentSiteUrl = currentSiteUrl + "/";
            }

            string siteDependencyKey = "CachingHttpModule_" + currentSiteUrl + "_";

            HttpRuntime.Cache.Remove(siteDependencyKey);
        });
    }

    if (Request.QueryString.Get("list") == "TranslationContents" || Request.QueryString.Get("list") == "TranslationContentsSub")
    {
        string webId = string.Empty;

        if (Request.QueryString["webId"] != null)
        {
            webId = Request.QueryString.Get("webId");
        }

        SPSecurity.RunWithElevatedPrivileges(delegate()
        {
            HttpRuntime.Cache.Remove("SPS_TRANSLATION_CACHE_IS_LOADED" + webId);
            HttpRuntime.Cache.Add("SPS_TRANSLATION_CACHE_IS_LOADED" + webId, "2", null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            string currentSiteUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, string.Empty);

            if (!currentSiteUrl.EndsWith("/"))
            {
                currentSiteUrl = currentSiteUrl + "/";
            }

            string siteDependencyKey = "CachingHttpModule_" + currentSiteUrl + "_";

            HttpRuntime.Cache.Remove(siteDependencyKey);
        });
    }

    if (Request.QueryString.Get("list") == "PagesTranslations")
    {
        SPSecurity.RunWithElevatedPrivileges(delegate()
        {
            HttpRuntime.Cache.Remove("AdminPagesToTranslate");
            HttpRuntime.Cache.Remove("PagesNotToTranslate");
            HttpRuntime.Cache.Remove("PagesToTranslate");
        });
    }
  
%>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
    </head>
    <body>
    </body>
</html>
    