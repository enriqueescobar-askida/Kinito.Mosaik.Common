// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatisticsHttpApplicationEvents.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StatisticsHttpApplicationEvents type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Web;
using System.IO;
using Alphamosaik.Common.Library.Statistics;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace TranslatorHttpHandler.ApplicationEvents
{
    public class StatisticsHttpApplicationEvents : IHttpApplicationEvents
    {
        internal const int LowDetail = 1;
        internal const int FullDetail = 5;

        private readonly IHttpApplicationEvents _httpApplicationEvents;
        private readonly int _activateStatisticsLogDetails;
        private readonly string _absoluteUri;

        // Holds the key of item to store the beginning time of request processing
        private const string StatisticsSlotName = "statisticsTime";

        private static bool _troubleshootingStoreListCreated;

        public StatisticsHttpApplicationEvents(IHttpApplicationEvents httpApplicationEvents, int activateStatisticsLogDetails, string absoluteUri)
        {
            _httpApplicationEvents = httpApplicationEvents;
            _activateStatisticsLogDetails = activateStatisticsLogDetails;
            _absoluteUri = absoluteUri;
        }

        public void ContextBeginRequest(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            if (!_troubleshootingStoreListCreated)
            {
                SPSecurity.RunWithElevatedPrivileges(() => CreateTroubleshootingStoreServersList(_absoluteUri));
            }

            // Store the time of request beginning
            if (context != null && HttpContext.Current.Request.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                var info = new FileInfo(HttpContext.Current.Request.Path);
                string logFilename = String.Format("C:\\Logs\\Statistics {0:yyyy-MM-dd-hh-mm-fffffff}_", DateTime.Now) + info.Name;
                context.Items.Add(StatisticsSlotName, StatisticsTracer.NewStatisticsTracer("TranslatorModule", new FileLogStatisticsWriter(logFilename), _activateStatisticsLogDetails));
                //// context.Items.Add(StatisticsSlotName, StatisticsTracer.NewStatisticsTracer("TranslatorModule", new DebugStatisticsWriter(), _activateStatisticsLogDetails));
            }

            _httpApplicationEvents.ContextBeginRequest(sender, e);
        }

        public void ContextEndRequest(object sender, EventArgs e)
        {
            // Create HttpApplication, HttpContext and HttpRespoonse objects to access request and response properties.
            var app = (HttpApplication)sender;
            var context = app.Context;

            if (context == null)
                return;

            _httpApplicationEvents.ContextEndRequest(sender, e);

            // Get the time of request beginning
            var statisticsTracer = (StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName];

            if (statisticsTracer != null)
            {
                try
                {
                    HttpContext.Current.Items.Remove(StatisticsSlotName);
                    statisticsTracer.Dispose();
                }
                catch (HttpException ex)
                {
                    Utilities.LogException("Error in Oceanik.TranslatorModule.ContextEndRequest: " + ex.Message, EventLogEntryType.Warning);
                }
            }
        }

        public void ContextReleaseRequestState(object sender, EventArgs e)
        {
            using (new Statistic((StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName], "ContextReleaseRequestState", FullDetail))
            {
                _httpApplicationEvents.ContextReleaseRequestState(sender, e);
            }
        }

        public void OnPreProcessRequest(object sender, EventArgs eventArgs)
        {
            using (new Statistic((StatisticsTracer)HttpContext.Current.Items[StatisticsSlotName], "OnPreProcessRequest", FullDetail))
            {
                _httpApplicationEvents.OnPreProcessRequest(sender, eventArgs);
            }
        }

        private static void CreateTroubleshootingStoreServersList(string url)
        {
            try
            {
                using (var site = new SPSite(url))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPList list = web.Lists.TryGetList("Troubleshooting Store");

                        if (list == null)
                        {
                            web.AllowUnsafeUpdates = true;
                            web.Lists.Add("Troubleshooting Store", string.Empty, SPListTemplateType.GenericList);
                            web.AllowUnsafeUpdates = false;

                            list = web.Lists.TryGetList("Troubleshooting Store");

                            list.ParentWeb.AllowUnsafeUpdates = true;

                            if (list.ContentTypesEnabled == false)
                            {
                                list.ContentTypesEnabled = true;
                                list.Update();
                            }

                            SPContentType contentType = web.Site.RootWeb.ContentTypes["Configuration Item"];
                            list.ContentTypes.Add(contentType);
                            SPContentType itemContentType = list.ContentTypes["Item"];
                            list.ContentTypes.Delete(itemContentType.Id);
                            list.Update();

                            list.ParentWeb.AllowUnsafeUpdates = false;
                        }

                        _troubleshootingStoreListCreated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.TraceNormalCaughtException("CreateTroubleshootingStoreServersList", ex);
            }
        }
    }
}
