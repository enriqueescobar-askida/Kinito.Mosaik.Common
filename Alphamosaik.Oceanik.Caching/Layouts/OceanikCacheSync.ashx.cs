// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikCacheSync.ashx.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the OceanikCacheSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Caching;
using Alphamosaik.Common.Library;

namespace Alphamosaik.Oceanik.Caching.Layouts
{
    public class OceanikCacheSync : IHttpHandler
    {
        private string _cacheLevel;
        private string _key;
        private string _pathUrl;
        private string _currentSiteUrl;

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.InputStream.Length == 0)
                throw new ArgumentException("No file input");

            GetQueryStringParameters(context);

            RawResponse rawResponse = GetRawResponse(context.Request.InputStream);

            CacheDependency dependencies = GetDependencies(context, _pathUrl, _currentSiteUrl);

            context.Cache.Insert(_key, rawResponse, dependencies, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.Low, null);
        }

        private static RawResponse GetRawResponse(Stream stream)
        {
            var buffer = new byte[stream.Length];

            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            RawResponse rawResponse = SerializeObject<RawResponse>.ByteArray2Object(buffer);

            return rawResponse;
        }

        private static CacheDependency GetDependencies(HttpContext context, string url, string currentSiteUrl)
        {
            string pathDependencyKey = "CachingHttpModule_" + url + "_";

            if (context.Cache.Get(pathDependencyKey) == null)
            {
                context.Cache.Insert(pathDependencyKey, pathDependencyKey, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.Low, null);
            }

            string siteDependencyKey = "CachingHttpModule_" + currentSiteUrl + "_";

            if (context.Cache.Get(siteDependencyKey) == null)
            {
                context.Cache.Insert(siteDependencyKey, siteDependencyKey, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.Low, null);
            }

            return new CacheDependency(null, new[] { pathDependencyKey, siteDependencyKey }, DateTime.Now.AddSeconds(10));
        }

        private void GetQueryStringParameters(HttpContext context)
        {
            _cacheLevel = context.Request.QueryString["CacheLevel"];
            _key = context.Request.QueryString["key"];
            _pathUrl = context.Request.QueryString["pathUrl"];
            _currentSiteUrl = context.Request.QueryString["currentSiteUrl"];
        }
    }
}
