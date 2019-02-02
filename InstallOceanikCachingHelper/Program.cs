// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Alphamosaik.Common.Library.Licensing;
using InstallHelper;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.Win32;
using Exception = System.Exception;

namespace InstallOceanikCachingHelper
{
    public class Program
    {
        private const string ApplicationEventLogRegistryKeyPath = @"SYSTEM\CurrentControlSet\services\eventlog\Application";

        public static int Main(string[] args)
        {
            // Test if input arguments were supplied:
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a command argument.");
                Console.WriteLine("Usage: InstallHelperConsole <install> or <uninstall>.");
                return 1;
            }

            string command = args[0];
            string path = Environment.CurrentDirectory;

            if (!DirExists(path))
            {
                Console.WriteLine("application path doest not exist!");
                return 2;
            }

            try
            {
                if (command.Equals("install", StringComparison.OrdinalIgnoreCase))
                {
                    InstallManagementFeature(path);

                    // Generate license key file to be send to Alphamosaik if it not exist
                    GenerateLicenseKeyFile(path);
                }
                else if (command.Equals("uninstall", StringComparison.OrdinalIgnoreCase))
                {
                    UninstallManagementFeature();

                    // clean uninstall
                    string fullPath = Path.Combine(path, "license.key");
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 3;
            }

            return 0;
        }

        private static void GenerateLicenseKeyFile(string path)
        {
            string actualEncryptedHex = PrivateKey.GetPrivateKey();
            WriteLicenseKey(path, actualEncryptedHex);
        }

        //-----------------------------------------------------------
        // FUNCTION: DirExists
        // Determines whether the specified directory name exists.
        // IN: [dirName] - name of directory to check for
        // Returns: True if the directory exists, False otherwise
        //-----------------------------------------------------------
        private static bool DirExists(string dirName)
        {
            try
            {
                // Check for file
                return Directory.Exists(dirName);
            }
            catch (Exception)
            {
                // Exception occured, return False
                return false;
            }
        }

        private static void WriteLicenseKey(string path, string licenceKey)
        {
            try
            {
                string fullPath = Path.Combine(path, "license.key");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var f7 = new FileInfo(fullPath);

                if (!f7.Exists)
                {
                    // Create a file to write to.
                    using (f7.CreateText())
                    {
                    }

                    using (StreamWriter swriterAppend = f7.AppendText())
                    {
                        swriterAppend.WriteLine(licenceKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void InstallManagementFeature(string applicationFolder)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    RegisterEventViewerSource();

                    var solutionGuid = new Guid("e62b8555-2314-4488-994a-c072d2410362");
                    var featureGuid = new Guid("92904c28-6cfa-4f9e-b25d-ac0a19eba679");

                    string varWspFile = applicationFolder + "\\" + "Alphamosaik.Oceanik.Caching.wsp";

                    if (SPFarm.Local.Solutions != null)
                    {
                        var installer = new Installer();
                        if (!installer.InstallSolution(varWspFile, solutionGuid, featureGuid, true))
                        {
                            throw new Exception("InstallSolution Failed!");
                        }

                        if (!installer.DeploySolution(varWspFile, solutionGuid, true, false))
                        {
                            throw new Exception("InstallSolution Failed!");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Alphamosaik.Common.SharePoint.Library.Exception.LogException("Oceanik", "Error in InstallHelper.InstallManagementFeature:  " + ex.Message);
                throw;
            }
        }

        private static void UninstallManagementFeature()
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    var solutionGuid = new Guid("e62b8555-2314-4488-994a-c072d2410362");

                    var installer = new Installer();
                    if (!installer.UninstallSolution(solutionGuid))
                    {
                        throw new Exception("UninstallSolution Failed!");
                    }
                });
            }
            catch (Exception ex)
            {
                Alphamosaik.Common.SharePoint.Library.Exception.LogException("Oceanik", "Error in InstallHelper.UninstallManagementFeature:  " + ex.Message);
                throw;
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
                Alphamosaik.Common.SharePoint.Library.Exception.LogException("Oceanik", "RegisterEventViewerSource Error: " + e.Message);
            }
        }
    }
}
