// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugHtml.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DebugHtml type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using Alphamosaik.Common.SharePoint.Library;

namespace Translator.Common.Library.DebuggingHelper
{
    public abstract class DebugHtml<T> : BaseStaticOverride<T> where T : DebugHtml<T>, IDisposable
    {
        protected DebugHtml()
        {
        }

        protected DebugHtml(string html)
        {
            Init(html);
        }

        protected DebugHtml(StringBuilder html)
        {
            Init(html.ToString());
        }

        ~DebugHtml()
        {
            Dispose(false);
        }

        public enum DebugHtmlType
        {
            /// <summary>
            /// Local store type
            /// </summary>
            Original,

            /// <summary>
            /// Global store type
            /// </summary>
            Modified
        }

        public string Html { get; set; }

        public bool LogHtml { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && LogHtml)
            {
                LogInputHtml(Html, DebugHtmlType.Modified);
            }
        }

        protected abstract void LogInputHtml(string html, DebugHtmlType type);

        protected void Init(string html)
        {
            if (HttpContext.Current.Request.QueryString["SPS_Debug_Html"] != null)
            {
                string result = HttpContext.Current.Request.QueryString["SPS_Debug_Html"];

                HttpContext.Current.Cache.Remove("SPS_DEBUG_HTML");

                if (result.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    LogHtml = true;
                    HttpContext.Current.Cache.Add("SPS_DEBUG_HTML", result, null, Cache.NoAbsoluteExpiration,
                                                  Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                }
            }

            if (HttpContext.Current.Cache["SPS_DEBUG_HTML"] != null)
            {
                var result = (string)HttpContext.Current.Cache["SPS_DEBUG_HTML"];

                if (result.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    LogHtml = true;
                }
            }

            if (LogHtml)
            {
                Html = html;

                LogInputHtml(Html, DebugHtmlType.Original);
            }
        }

        protected string GetCurrentPageName()
        {
            string path = HttpContext.Current.Request.Url.AbsolutePath;
            var info = new FileInfo(path);
            string ret = info.Name;
            return ret;
        }
    }
}
