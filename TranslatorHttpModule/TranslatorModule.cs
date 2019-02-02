// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorModule.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the TranslatorModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using Alphamosaik.Common.Library;
using Alphamosaik.Common.Library.Licensing;
using Alphamosaik.Common.SharePoint.Library.ConfigStore;
using Alphamosaik.Oceanik.Sdk;
using Microsoft.SharePoint.ApplicationRuntime;
using Translator.Common.Library;
using TranslatorHttpHandler.ApplicationEvents;
using TranslatorHttpHandler.TranslatorHelper;

namespace TranslatorHttpHandler
{
    public class TranslatorModule : IHttpModule
    {
        private static readonly string ServerPrivateKey = PrivateKey.GetPrivateKey();
        private static readonly object LockThis = new object();
        private const string OceanikAutomaticTranslation = "Oceanik.AutomaticTranslation";
        private readonly IVaryByCustomHandler _varyByCustomHandler;
        
        // Holds the name of the HTTP Response header
        private static License _license;
        private static int _activateStatisticsLogDetails;
        
        private bool _init;
        private IHttpApplicationEvents _httpApplicationEvents;
        private TranslatorHelper.TranslatorHelper _translatorHelper;
        private IAutomaticTranslation _automaticTranslationPlugin;

        public TranslatorModule()
        {
            try
            {
                _varyByCustomHandler = new VaryByCultureHandler();
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in Oceanik.Licensing: " + ex.Message, EventLogEntryType.Warning);
            }
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            if (context != null)
            {
                if (BlackList.IsBlackListed(ServerPrivateKey))
                {
                    Utilities.LogException("Oceanik 2010 License is invalid!");
                    return;
                }

                context.ReleaseRequestState += OnContextReleaseRequestState;
                context.BeginRequest += OnContextBeginRequest;
                context.EndRequest += OnContextEndRequest;
                context.PreRequestHandlerExecute += OnPreProcessRequest;

                var application = context as SPHttpApplication;
                if (application != null)
                {
                    application.RegisterGetVaryByCustomStringHandler(_varyByCustomHandler);
                }
            }
        }

        private void OnPreProcessRequest(object sender, EventArgs e)
        {
            if (_httpApplicationEvents != null)
                _httpApplicationEvents.OnPreProcessRequest(sender, e);
        }

        
        private static void UpdateOldLicensing(string absoluteUri)
        {
            // Get Installation Path
            string alphamosaikInstallationPath = ConfigurationManager.AppSettings["AlphamosaikInstallationPath"] ?? @"C:\Program Files\Alphamosaik\SharepointTranslator2010";

            string fullPath = Path.Combine(alphamosaikInstallationPath, "license.dat");

            if (File.Exists(fullPath))
            {
                string license = File.ReadAllText(fullPath);

                byte[] bytes = Encoding.ASCII.GetBytes(license);
                ConfigStore.Instance.AddAttachment("Oceanik", Environment.MachineName, "license.dat", bytes, false, absoluteUri);
            }
        }
        

        private void InitializeApplication(HttpContext context)
        {
            try
            {
                lock (LockThis)
                {
                    if (!_init)
                    {
                        string absoluteUri = Alphamosaik.Common.SharePoint.Library.Utilities.GetAbsoluteUri(context.ApplicationInstance);

                        if (_license == null)
                        {
                            // We already use around 60
                            Regex.CacheSize = 90;

                            string value = ConfigStore.Instance.GetValue("Oceanik", Environment.MachineName, absoluteUri);

                            if (string.IsNullOrEmpty(value))
                            {
                                byte[] bytes = Encoding.ASCII.GetBytes(ServerPrivateKey);
                                ConfigStore.Instance.AddValue(Environment.MachineName, "Oceanik", "ServerPrivateKey", string.Empty, "license.key", bytes, false, absoluteUri);

                                UpdateOldLicensing(absoluteUri);
                            }

                            string license = ConfigStore.Instance.GetSpecificStringAttachment("Oceanik", Environment.MachineName, "license.dat", absoluteUri);

                            _license = new License();

                            if (!string.IsNullOrEmpty(ConfigStore.Instance.GetValue("Oceanik", "ActivateStatisticsLogDetails", absoluteUri)))
                            {
                                _activateStatisticsLogDetails = Convert.ToInt32(ConfigStore.Instance.GetValue("Oceanik", "ActivateStatisticsLogDetails", absoluteUri));
                            }

                            if (!string.IsNullOrEmpty(license))
                            {
                                _license = new License(ServerPrivateKey, license);
                            }

                            string automaticTranslationPlugin = ConfigStore.Instance.GetValue("Oceanik", "automaticTranslationPlugin", absoluteUri);

                            if (!string.IsNullOrEmpty(automaticTranslationPlugin) && HttpRuntime.Cache[OceanikAutomaticTranslation] == null)
                            {
                                _automaticTranslationPlugin = PlugInUtilities.GetPlugin<IAutomaticTranslation>(automaticTranslationPlugin);

                                if (_automaticTranslationPlugin != null)
                                    HttpRuntime.Cache.Add(OceanikAutomaticTranslation, _automaticTranslationPlugin, null,
                                                          Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                                          CacheItemPriority.NotRemovable, null);
                            }
                        }

                        if ((_automaticTranslationPlugin == null) && (HttpRuntime.Cache[OceanikAutomaticTranslation] != null))
                            _automaticTranslationPlugin = (IAutomaticTranslation)HttpRuntime.Cache[OceanikAutomaticTranslation];

                        _translatorHelper = _activateStatisticsLogDetails > 0 ? new StatisticsStandardTranslatorHelper() : new StandardTranslatorHelper();

                        _httpApplicationEvents = new HttpApplicationEvents(_translatorHelper, _license, _automaticTranslationPlugin);

                        if (_activateStatisticsLogDetails > 0)
                        {
                            _httpApplicationEvents = new StatisticsHttpApplicationEvents(_httpApplicationEvents, _activateStatisticsLogDetails, absoluteUri);
                        }

                        _init = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in Oceanik.Init: " + ex.Message, EventLogEntryType.Warning);
            }
        }

        private void OnContextEndRequest(object sender, EventArgs e)
        {
            if (_httpApplicationEvents != null)
                _httpApplicationEvents.ContextEndRequest(sender, e);
        }

        private void OnContextBeginRequest(object sender, EventArgs e)
        {
            if (!_init)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                InitializeApplication(context);
            }

            if (_httpApplicationEvents != null)
                _httpApplicationEvents.ContextBeginRequest(sender, e);
        }

        private void OnContextReleaseRequestState(object sender, EventArgs e)
        {
            if (_httpApplicationEvents != null)
                _httpApplicationEvents.ContextReleaseRequestState(sender, e);
        }
    }
}
