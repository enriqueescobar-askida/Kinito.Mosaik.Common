// ---------------------------------------------------------------------------public public -----------------------------------------
// <copyright file="Program.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;

using InstallHelper;
using ListsInstallation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace InstallHelperConsole
{
    public class Program
    {
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
                var installManagement = new InstallManagement();

                if (command.Equals("install", StringComparison.OrdinalIgnoreCase))
                {
                    installManagement.InstallManagementFeature(path);
                    installManagement.InstallOceanikContentQueryWebpart(path);

                    CleanOldHandlers();
                }
                else if (command.Equals("uninstall", StringComparison.OrdinalIgnoreCase))
                {
                    installManagement.UninstallManagementFeature(path);
                    installManagement.UninstallOceanikContentQueryWebpart(path);

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

        private static void CleanOldHandlers()
        {
            var webServices = new SPWebServiceCollection(SPFarm.Local);

            foreach (SPWebService webService in webServices)
            {
                foreach (SPWebApplication webApp in webService.WebApplications)
                {
                    if (!webApp.IsAdministrationWebApplication)
                    {
                        try
                        {
                            // Get the filepath location for the web.config
                            SPIisSettings settings = webApp.GetIisSettingsWithFallback(SPUrlZone.Default);
                            DirectoryInfo iisPath = settings.Path;

                            string webConfigpath = iisPath.FullName + "\\web.config";

                            // start reading the web.config in order to add appropriate text
                            var configReader = new StreamReader(webConfigpath);
                            string configContents = configReader.ReadToEnd();
                            configReader.Close();

                            string moduleText = "<add name=\"AlphaMosaikTranslation\" type=\"TranslatorHttpHandler.TranslatorModule, TranslatorHttpHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a65beeb1a6acb37a\" />" + Environment.NewLine;
                            if (configContents.IndexOf(moduleText) != -1)
                            {
                                // remove old handler
                                configContents = configContents.Replace(moduleText, string.Empty);

                                var configWriter = new StreamWriter(webConfigpath);
                                configWriter.Write(configContents);
                                configWriter.Flush();
                                configWriter.Close();

                                foreach (SPSite site in webApp.Sites)
                                {
                                    var list = new List(site.Url, "/", "LanguagesVisibility");

                                    // Remove Old Assembly
                                    list.UnlinkEventHandlerToList("CheckDefaultLangEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7b49392f9e06d3f3", "CheckDefaultLangEvent.ItemEvent", "ItemUpdated");
                                    list.UnlinkEventHandlerToList("CreateGUIDEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4a041d2cfd549304", "CreateGUIDEvent.GenerateId", "ItemAdded");

                                    const string OldAssemblyName = "ReloadCacheEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=45fd9f0eaea17ba9";
                                    const string ClassName = "ReloadCacheEvent.CacheEvent";

                                    list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemAdded");
                                    list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemDeleting");
                                    list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemUpdated");
                                    list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemUpdated");
                                    list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemUpdated");
                                    list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemDeleting");
                                    list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemAdded");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
    }
}
