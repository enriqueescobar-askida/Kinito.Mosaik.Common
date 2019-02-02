// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Translator.EventReceiver.cs" company="AlphaMosaik">
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

namespace Alphamosaik.Translator.ApplicationFeatures.Features.Translator.UI
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("3fbf2654-6f78-45ac-abdc-829809318f54")]
    public class TranslatorEventReceiver : SPFeatureReceiver
    {
        private const string ApplicationEventLogRegistryKeyPath = @"SYSTEM\CurrentControlSet\services\eventlog\Application";

        private readonly SPWebConfigModification _webConfigModification = new SPWebConfigModification
                                                 {
                                                     Path = "configuration/system.webServer/modules",
                                                     Name = "add [@name='AlphaMosaikTranslation']",
                                                     Sequence = 0,
                                                     Owner = "AlphaMosaik",
                                                     Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                                                     Value = "<add name=\"AlphaMosaikTranslation\" type=\"TranslatorHttpHandler.TranslatorModule, TranslatorHttpHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be\" />"
                                                 };

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    // get references to target web application and feature definition
                    var webApp = (SPWebApplication)properties.Feature.Parent;

                    bool webConfigModified = false;

                    

                    if (!webApp.WebConfigModifications.Contains(_webConfigModification))
                    {
                        webApp.WebConfigModifications.Add(_webConfigModification);
                        webConfigModified = true;
                    }

                    if (webConfigModified)
                    {
                        webApp.Update();
                        webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
                    }

                    // Ensure the job is not already registered.
                    foreach (SPJobDefinition job in webApp.JobDefinitions)
                        if (job.Name == ReloadCacheJobDefinition.JobName) job.Delete();

                    // Install job.
                    var reloadCacheJob = new ReloadCacheJobDefinition(webApp);

                    // Schedule the job to run every minutes all the time.
                    var schedule = new SecondSchedule();
                    reloadCacheJob.Schedule = schedule;

                    // Save changes.
                    reloadCacheJob.Update();

                    RegisterEventViewerSource(properties);
                });
            }
            catch (Exception ex)
            {
                SPSecurity.RunWithElevatedPrivileges(() => EventLog.WriteEntry("Oceanik.FeatureActivated", ex.Message, EventLogEntryType.Error));
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

                        if (configMod.Owner == "AlphaMosaik") 
                            collection.Remove(configMod); 
                    }

                    // Apply changes only if any items were removed. 
                    if (startCount > collection.Count) 
                    { 
                        webApp.Update();
                        webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications(); 
                    }

                    // Ensure the job is not already registered.
                    foreach (SPJobDefinition job in webApp.JobDefinitions)
                    {
                        if (job.Name == ReloadCacheJobDefinition.JobName)
                            job.Delete();
                    }
                });
            }
            catch (Exception ex)
            {
                SPSecurity.RunWithElevatedPrivileges(() => EventLog.WriteEntry("Oceanik.RemoveHttpHandlerFromWebConfig", ex.Message, EventLogEntryType.Information));
            }
        }

        private static void RegisterEventViewerSource(SPFeatureReceiverProperties properties)
        {
            foreach (SPServer server in properties.Definition.Farm.Servers)
            {
                if (server.Role != SPServerRole.Invalid)
                {
                    // Register source for Application event log
                    RegistryKey hklmRegistryKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, server.Address);
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
}
