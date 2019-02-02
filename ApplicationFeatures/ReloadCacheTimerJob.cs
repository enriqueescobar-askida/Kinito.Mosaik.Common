// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReloadCacheTimerJob.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ReloadCacheTimerJob type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Translator.ApplicationFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using Microsoft.SharePoint.Administration;

    [Guid("9573FAD9-ED89-45E8-BD8B-6A5034E03895")]
    public class ReloadCacheTimerJob : SPJobDefinition
    {
        public ReloadCacheTimerJob()
            : base()
        {
        }

        public ReloadCacheTimerJob(string name, SPWebApplication webApp, SPServer server, SPJobLockType lockType)
            : base(name, webApp, server, lockType)
        {
            DeleteJob(webApp.JobDefinitions);

            /*FieldInfo headersInfo = GetType().GetBaseField("m_SkipPersistedStoreWriteCheck", BindingFlags.NonPublic | BindingFlags.Instance);

            if (headersInfo != null)
            {
                headersInfo.SetValue(this, true);
            }*/
        }

        public override string DisplayName
        {
            get
            {
                return "Oceanik Reload Cache Event";
            }
        }

        public override string Description
        {
            get
            {
                return "Oceanik Reload Cache Event.";
            }
        }

        public override void Execute(Guid targetInstanceId)
        {
#pragma warning disable 612,618
            ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
#pragma warning restore 612,618

            var req = (HttpWebRequest)WebRequest.Create(Properties["CMD"].ToString());
            req.Method = "GET";
            req.Credentials = CredentialCache.DefaultCredentials;
            req.GetResponse();
        }

        private void DeleteJob(IEnumerable<SPJobDefinition> jobs)
        {
            foreach (SPJobDefinition job in jobs)
            {
                if (job.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                {
                    job.Delete();
                }
            }
        }
    }
}