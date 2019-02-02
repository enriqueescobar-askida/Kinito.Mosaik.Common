// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallManagement.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the InstallManagement type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.Win32;
using Translator.Common.Library;

namespace InstallHelper
{
    public class InstallManagement : IInstallManagementFeature
    {
        private const string ApplicationEventLogRegistryKeyPath = @"SYSTEM\CurrentControlSet\services\eventlog\Application";

        #region IInstallManagementFeature Members

        public void InstallManagementFeature(string applicationFolder)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                                                         {
                                                             RegisterEventViewerSource();

                                                             var solutionGuid = new Guid("8edc9014-59ed-4f9d-9e99-a750cda9dde1");
                                                             var featureGuid = new Guid("b242c6bd-bbe3-4fb3-8596-9fe9272ed698");

                                                             string varWspFile = applicationFolder + "\\" + "Alphamosaik.Translator.ApplicationFeatures.wsp";

                                                             if (SPFarm.Local.Solutions != null)
                                                             {
                                                                 var installer = new Installer();
                                                                 if (!installer.InstallSolution(varWspFile, solutionGuid, featureGuid, true))
                                                                 {
                                                                     throw new Exception("InstallSolution Failed!");
                                                                 }

                                                                 foreach (var solution in SPFarm.Local.Solutions)
                                                                 {
                                                                     if (solution.Id == solutionGuid)
                                                                     {
                                                                         if (solution.Deployed)
                                                                         {
                                                                            CreateNewLists(solution);
                                                                         }
                                                                     }
                                                                 }
                                                             }
                                                         });
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.InstallManagementFeature:  " + ex.Message);
                throw;
            }
        }

        public void UninstallManagementFeature(string applicationFolder)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                                                         {
                                                             var solutionGuid = new Guid("8edc9014-59ed-4f9d-9e99-a750cda9dde1");

                                                             var installer = new Installer();
                                                             if (!installer.UninstallSolution(solutionGuid))
                                                             {
                                                                 throw new Exception("UninstallSolution Failed!");
                                                             }
                                                         });
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.UninstallManagementFeature:  " + ex.Message);
                throw;
            }
        }

        #endregion

        public void InstallOceanikContentQueryWebpart(string applicationFolder)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    RegisterEventViewerSource();

                    var solutionGuid = new Guid("a1e66bc9-8bd0-4923-8d45-d1268b0e9cf1");
                    // var featureGuid = new Guid("133d2978-f877-423f-8dcc-df2dd7aa034c");

                    string varWspFile = applicationFolder + "\\" + "Oceanik.CQWP.WebPart.wsp";

                    if (SPFarm.Local.Solutions != null)
                    {
                        var installer = new Installer();
                        if (!installer.InstallSolution(varWspFile, solutionGuid))
                        {
                            throw new Exception("InstallSolution Failed!");
                        }

                        foreach (var solution in SPFarm.Local.Solutions)
                        {
                            if (solution.Id == solutionGuid)
                            {
                                if (solution.Deployed)
                                {
                                    CreateNewLists(solution);
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.InstallOceanikContentQueryWebpart:  " + ex.Message);
                throw;
            }
        }

        public void UninstallOceanikContentQueryWebpart(string path)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    var solutionGuid = new Guid("a1e66bc9-8bd0-4923-8d45-d1268b0e9cf1");

                    var installer = new Installer();
                    if (!installer.UninstallSolution(solutionGuid))
                    {
                        throw new Exception("UninstallSolution Failed!");
                    }
                });
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.UninstallOceanikContentQueryWebpart:  " + ex.Message);
                throw;
            }
        }

        private static void CreateNewLists(SPSolution solution)
        {
            foreach (SPWebApplication deployedWebApplication in solution.DeployedWebApplications)
            {
                foreach (SPFeature feature in deployedWebApplication.Features)
                {
                    if (feature.DefinitionId.Equals(new Guid("b242c6bd-bbe3-4fb3-8596-9fe9272ed698")))
                    {
                        foreach (SPSite site in deployedWebApplication.Sites)
                        {
                            SPWeb rootWeb = site.RootWeb;

                            if (rootWeb != null)
                            {
                                SPList list = rootWeb.Lists.TryGetList("TranslationContents");

                                if (list != null)
                                {
                                    var installerHelper = new InstallerHelper();

                                    installerHelper.CreateConfigurationStoreServersList(rootWeb.Url);
                                    installerHelper.CreateTroubleshootingStoreServersList(rootWeb.Url);
                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        private static void RegisterEventViewerSource()
        {
            try
            {
                foreach (SPServer server in SPFarm.Local.Servers)
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
            catch (Exception e)
            {
                Utilities.TraceNormalCaughtException("RegisterEventViewerSource Error: ", e);
            }
        }
    }
}