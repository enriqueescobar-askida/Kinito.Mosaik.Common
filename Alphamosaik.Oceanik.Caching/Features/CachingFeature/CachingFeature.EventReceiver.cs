// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingFeature.EventReceiver.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.Win32;

namespace Alphamosaik.Oceanik.Caching.Features.CachingFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("64c48860-6d78-4316-90c2-e5eab79f0f72")]
    public class CachingFeatureEventReceiver : SPFeatureReceiver
    {
        private const string ApplicationEventLogRegistryKeyPath = @"SYSTEM\CurrentControlSet\services\eventlog\Application";

        private readonly SPWebConfigModification _webConfigModification = new SPWebConfigModification
        {
            Path = "configuration/system.webServer/modules",
            Name = "add [@name='AlphaMosaikCachingHttpModule']",
            Sequence = 0,
            Owner = "AlphaMosaikCaching",
            Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
            Value = "<add name=\"AlphaMosaikCachingHttpModule\" type=\"Alphamosaik.Oceanik.Caching.CachingHttpModule, Alphamosaik.Oceanik.Caching, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be\" />"
        };

        private readonly SPWebConfigModification _webConfigModificationHandler = new SPWebConfigModification
        {
            Path = "configuration/system.webServer/handlers",
            Name = "add [@name='AlphaMosaikCachingHttpModule']",
            Sequence = 0,
            Owner = "AlphaMosaikCaching",
            Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
            Value = "<add name=\"AlphaMosaikCachingHttpModule\" verb=\"POST\" path=\"OceanikCacheSync.ashx\" type=\"Alphamosaik.Oceanik.Caching.Layouts.OceanikCacheSync, Alphamosaik.Oceanik.Caching, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be\" />"
        };

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    // get references to target web application and feature definition
                    var webApp = (SPWebApplication)properties.Feature.Parent;

                    string rootUrl = string.Empty;
                    foreach (SPSite site in webApp.Sites)
                    {
                        if (site.RootWeb != null)
                        {
                            rootUrl = site.RootWeb.Url;
                            break;
                        }
                    }

                    rootUrl = FilterUrl(rootUrl); // root server url

                    bool webConfigModified = false;


                    if (!webApp.WebConfigModifications.Contains(_webConfigModification))
                    {
                        webApp.WebConfigModifications.Add(_webConfigModification);
                        webConfigModified = true;
                    }

                    if (!webApp.WebConfigModifications.Contains(_webConfigModificationHandler))
                    {
                        webApp.WebConfigModifications.Add(_webConfigModificationHandler);
                        webConfigModified = true;
                    }
                    
                    if (webConfigModified)
                    {
                        webApp.Update();
                        webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
                    }

                    RegisterEventViewerSource(properties);
                });
            }
            catch (Exception ex)
            {
                SPSecurity.RunWithElevatedPrivileges(() => EventLog.WriteEntry("Oceanik.Caching.FeatureActivated", ex.Message, EventLogEntryType.Error));
            }
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            RemoveHttpHandlerFromWebConfig(properties);
        }

        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            RemoveHttpHandlerFromWebConfig(properties);
        }

        private static void RemoveHttpHandlerFromWebConfig(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    // get references to target web application and feature definition
                    var webApp = (SPWebApplication)properties.Feature.Parent;

                    Collection<SPWebConfigModification> collection = webApp.WebConfigModifications;
                    int startCount = collection.Count;

                    // Remove any modifications that were originally created by the owner. 
                    for (int c = startCount - 1; c >= 0; c--)
                    {
                        SPWebConfigModification configMod = collection[c];

                        if (configMod.Owner == "AlphaMosaikCaching")
                            collection.Remove(configMod);
                    }

                    // Apply changes only if any items were removed. 
                    if (startCount > collection.Count)
                    {
                        webApp.Update();
                        webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
                    }
                });
            }
            catch (Exception ex)
            {
                SPSecurity.RunWithElevatedPrivileges(() => EventLog.WriteEntry("Oceanik.Caching.RemoveHttpHandlerFromWebConfig", ex.Message, EventLogEntryType.Information));
            }
        }

        private static void RegisterEventViewerSource(SPFeatureReceiverProperties properties)
        {
            foreach (SPServer server in properties.Definition.Farm.Servers)
            {
                if (server.Role != SPServerRole.Invalid)
                {
                    // Register source for Application event log
                    using (RegistryKey hklmRegistryKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, server.Address))
                    {
                        RegistryKey applicationEventLogRegistryKey = hklmRegistryKey.OpenSubKey(ApplicationEventLogRegistryKeyPath, true);

                        if (applicationEventLogRegistryKey != null)
                        {
                            RegistryKey mySourceRegistryKey = applicationEventLogRegistryKey.OpenSubKey("Oceanik");
                            if (mySourceRegistryKey == null)
                            {
                                mySourceRegistryKey = applicationEventLogRegistryKey.CreateSubKey("Oceanik", RegistryKeyPermissionCheck.ReadWriteSubTree);

                                // HACK: hard coded path and name of resource DLL
                                if (mySourceRegistryKey != null)
                                {
                                    mySourceRegistryKey.SetValue("EventMessageFile", @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\EventLogMessages.dll", RegistryValueKind.String);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string FilterUrl(string url)
        {
            string[] sections = url.Split('/');

            if (sections.Length < 3)
            {
                return String.Empty;
            }

            return sections[0] + "//" + sections[2] + "/";
        }
    }
}
