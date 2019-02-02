// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugHtmlToList.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DebugHtmlToList type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;
using Alphamosaik.Common.SharePoint.Library.ConfigStore;

namespace Translator.Common.Library.DebuggingHelper
{
    public class DebugHtmlToList : DebugHtml<DebugHtmlToList>, IDisposable
    {
        private readonly object _lockDebugHtml = new object();
        private readonly string _absoluteUri;

        public DebugHtmlToList(string html, string absoluteUri)
        {
            _absoluteUri = absoluteUri;
            Init(html);
        }

        public DebugHtmlToList(StringBuilder html, string absoluteUri)
        {
            _absoluteUri = absoluteUri;
            Init(html.ToString());
        }

        protected override void LogInputHtml(string html, DebugHtmlType type)
        {
            lock (_lockDebugHtml)
            {
                try
                {
                    string currentPage = GetCurrentPageName();

                    if (string.IsNullOrEmpty(currentPage))
                    {
                        currentPage = "UnidentifiedPage";
                    }

                    string logFilename = String.Format("{0:yyyy-MM-dd-hh-mm-fffffff} ", DateTime.Now) + type + "_" + currentPage + ".txt";
                    byte[] bytes = Encoding.ASCII.GetBytes(Html);

                    string value = TroubleshootingStore.Instance.GetValue("DebugHtml", currentPage);

                    if (string.IsNullOrEmpty(value))
                    {
                        TroubleshootingStore.Instance.AddValue(currentPage, "DebugHtml", currentPage, string.Empty, logFilename, bytes, false, _absoluteUri);
                    }
                    else
                    {
                        TroubleshootingStore.Instance.AddAttachment("DebugHtml", currentPage, logFilename, bytes, false, _absoluteUri);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("ListLogStatisticsWriter", ex.Message + Environment.NewLine + ex, EventLogEntryType.Warning);
                }
            }
        }
    }
}