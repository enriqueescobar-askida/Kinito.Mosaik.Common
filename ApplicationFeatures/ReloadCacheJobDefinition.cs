// -----------------------------------------------------------------------
// <copyright file="ReloadCacheJobDefinition.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Alphamosaik.Translator.ApplicationFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ReloadCacheJobDefinition : SPWorkItemJobDefinition
    {
        public const string JobName = "Oceanik Reload Cache Definition";
        public const string WorkItemId = "{da3939d5-7944-4dd8-bdab-b0bfd729ec19}";

        public ReloadCacheJobDefinition()
            : base()
        {
            this.Title = JobName;
        }

        public ReloadCacheJobDefinition(SPWebApplication app)
            : base(JobName, app)
        {
        }

        public override Guid WorkItemType()
        {
            return new Guid(WorkItemId);
        }

        protected override bool ProcessWorkItem(SPContentDatabase contentDatabase, SPWorkItemCollection workItems, SPWorkItem workItem, SPJobState jobState)
        {
            using (var site = new SPSite(workItem.SiteId))
            {
                using (SPWeb web = site.OpenWeb(workItem.WebId))
                {
                    workItems.SubCollection(site, web, 0, (uint)workItems.Count).DeleteWorkItem(workItem.Id);
                }
            }

            try
            {
                var stringSeparators = new string[] { "-#####-" };

                string[] cmds = workItem.TextPayload.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                SPWebApplication webApp = SPWebApplication.Lookup(new Uri(cmds[0]));

                SPIisSettings iis = webApp.IisSettings[SPUrlZone.Default];

                string port;

                if (iis.ServerBindings.Count > 0)
                {
                    port = iis.ServerBindings[0].Port.ToString();
                }
                else
                {
                    string path = iis.Path.ToString();
                    port = path.Substring(path.LastIndexOf('\\') + 1);
                }

                if (!string.IsNullOrEmpty(port))
                {
                    port = ":" + port;
                }

                var reloadCacheTimerJob = new ReloadCacheTimerJob("Oceanik.ReloadCacheJob" + Guid.NewGuid(), webApp, null, SPJobLockType.None) { Schedule = new SPOneTimeSchedule(DateTime.Now) };

                string command = "http://localhost" + port + cmds[1];
                reloadCacheTimerJob.Properties.Add("CMD", command);

                reloadCacheTimerJob.Update();

                SPSecurity.RunWithElevatedPrivileges(() => EventLog.WriteEntry("ReloadCacheJobDefinition.ProcessWorkItem.ExecuteCommand", command, EventLogEntryType.Information));
            }
            catch (Exception ex)
            {
                SPSecurity.RunWithElevatedPrivileges(() => EventLog.WriteEntry("ReloadCacheJobDefinition.ProcessWorkItem", ex.Message, EventLogEntryType.Error));
            }

            return true;
        }
    }
}
