// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallerHelper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IInstallManagementFeature type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using ListsInstallation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Translator.Common.Library;

namespace InstallHelper
{
    [RunInstaller(true)]
    public partial class InstallerHelper : System.Configuration.Install.Installer
    {
        private List.UiUpdateProgressDelegate uiUpdateProgressMethod;

        private readonly InstallManagement _management = new InstallManagement();

        public InstallerHelper()
        {
            InitializeComponent();
        }

        public InstallerHelper(List.UiUpdateProgressDelegate progressMethod)
        {
            uiUpdateProgressMethod = progressMethod;
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            try
            {
                base.Install(stateSaver);
                string applicationFolder = Context.Parameters["APPFOLDER"];
                applicationFolder = AppFolderPathFilter(applicationFolder);
                _management.InstallManagementFeature(applicationFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            try
            {
                string applicationFolder = Context.Parameters["APPFOLDER"];
                applicationFolder = AppFolderPathFilter(applicationFolder);
                _management.UninstallManagementFeature(applicationFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                base.Uninstall(savedState);
            }
        }

        public bool CheckPermissions(string url)
        {
            try
            {
                bool result = false;
                SPSecurity.RunWithElevatedPrivileges(delegate
                                                         {
                        using (var currentSite = new SPSite(url))
                        using (SPWeb web = currentSite.OpenWeb())
                        {                    
                            result = web.UserIsWebAdmin;
                        }
                    });
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error:  " + ex.Message);
                throw;
            }
        }

        public void DeActivateManagementFeature(string url, string applicationFolder)
        {
            using (var site = new SPSite(url))
            {
                var webapps = new Collection<SPWebApplication> { site.WebApplication };

                try
                {
                    var solutionGuid = new Guid("8edc9014-59ed-4f9d-9e99-a750cda9dde1");

                    foreach (SPSolution solution in SPFarm.Local.Solutions)
                    {
                        if (solution.Id == solutionGuid)
                        {
                            if (solution.DeploymentState != SPSolutionDeploymentState.NotDeployed)
                            {
                                site.WebApplication.Features.Remove(new Guid("b242c6bd-bbe3-4fb3-8596-9fe9272ed698"), true);

                                solution.Retract(DateTime.Now, webapps);

                                Utilities.WaitForJobToFinish(solution);
                            }

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogException("Error in InstallHelper.UninstallManagementFeature:  " + ex.Message);
                    throw;
                }
            }
        }

        public void DeActivateOceanikContentQueryWebpartFeature(string url, string applicationFolder)
        {
            using (var site = new SPSite(url))
            {
                var webapps = new Collection<SPWebApplication> { site.WebApplication };

                try
                {
                    var solutionGuid = new Guid("a1e66bc9-8bd0-4923-8d45-d1268b0e9cf1");
                    var featureGuid = new Guid("133d2978-f877-423f-8dcc-df2dd7aa034c");

                    foreach (SPSite siteCollection in site.WebApplication.Sites)
                    {
                        siteCollection.Features.Remove(featureGuid);
                    }

                    foreach (SPSolution solution in SPFarm.Local.Solutions)
                    {
                        if (solution.Id == solutionGuid)
                        {
                            if (solution.DeploymentState != SPSolutionDeploymentState.NotDeployed)
                            {
                                solution.Retract(DateTime.Now, webapps);

                                Utilities.WaitForJobToFinish(solution);
                            }

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogException("Error in InstallHelper.DeActivateOceanikContentQueryWebpartFeature:  " + ex.Message);
                    throw;
                }
            }
        }

        public void RemoveLists(string url)
        {
            DeletePagesTransList(url);
            DeleteTransList(url);
            DeleteVisibilityList(url);
            DeleteLoadBalancingServersList(url);
            DeleteTroubleshootingStoreServersList(url);
            DeleteConfigurationStoreServersList(url);
            DeleteExtractorTranslationsList(url);
            DeletePagesToUpdateForTranslationList(url);
        }

        public void RemoveTranslatorFromWebConfig(string wwwRoot)
        {
            using (var site = new SPSite(wwwRoot))
            {
                var webService = SPWebService.ContentService;

                SPWebConfigModification configModFound = null;
                Collection<SPWebConfigModification> modsCollection = site.WebApplication.WebConfigModifications;

                // Find the most recent modification of a specified owner
                int modsCount1 = modsCollection.Count;
                for (int i = modsCount1 - 1; i > -1; i--)
                {
                    if (modsCollection[i].Name == "add [@name='AlphaMosaikTranslation']")
                    {
                        configModFound = modsCollection[i];
                        break;
                    }
                }

                // Remove it and save the change to the configuration database  
                modsCollection.Remove(configModFound);
                webService.Update();
                webService.ApplyWebConfigModifications();
            }
        }

        public void AddTranslatorToWebConfig(string wwwRoot, bool contentWebPartTrad, bool extractor, string startupPath)
        {
            try
            {
                using (var site = new SPSite(wwwRoot))
                {
                    string currentUserName = string.Empty;

                    if (WindowsIdentity.GetCurrent() != null)
                    {
// ReSharper disable PossibleNullReferenceException
                        currentUserName = WindowsIdentity.GetCurrent().Name;

// ReSharper restore PossibleNullReferenceException
                    }

                    var webConfigModification = new SPWebConfigModification
                    {
                        Path = "configuration/system.webServer/modules",
                        Name = "add [@name='AlphaMosaikTranslation']",
                        Sequence = 0,
                        Owner = currentUserName,
                        Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                        Value = @"<add name='AlphaMosaikTranslation' type='TranslatorHttpHandler.TranslatorModule, TranslatorHttpHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be' />"
                    };

                    site.WebApplication.WebConfigModifications.Add(webConfigModification);

                    var webService = SPWebService.ContentService;
                    webService.Update();
                    webService.ApplyWebConfigModifications();
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.AddTranslatorToWebConfig:  " + ex.Message);
                throw;
            }
        }

        public void ActivateManagementFeature(string applicationFolder, string url)
        {
            try
            {
                var solutionGuid = new Guid("8edc9014-59ed-4f9d-9e99-a750cda9dde1");

                foreach (SPSolution solution in SPFarm.Local.Solutions)
                {
                    if (solution.Id == solutionGuid)
                    {
                        SPWebApplication webapp = SPWebApplication.Lookup(new Uri(url));

                        if (!solution.DeployedWebApplications.Contains(webapp))
                        {
                            var webapps = new Collection<SPWebApplication> { webapp };

                            solution.Deploy(DateTime.Now, true, webapps, true);

                            Utilities.WaitForJobToFinish(solution);

                            webapp.Features.Add(new Guid("b242c6bd-bbe3-4fb3-8596-9fe9272ed698"), true);
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.ActivateManagementFeature:  " + ex.Message);
                throw;
            }
        }

        public void ActivateOceanikContentQueryWebpartFeature(string applicationFolder, string url)
        {
            try
            {
                var solutionGuid = new Guid("a1e66bc9-8bd0-4923-8d45-d1268b0e9cf1");
                var featureGuid = new Guid("133d2978-f877-423f-8dcc-df2dd7aa034c");

                foreach (SPSolution solution in SPFarm.Local.Solutions)
                {
                    if (solution.Id == solutionGuid)
                    {
                        SPWebApplication webapp = SPWebApplication.Lookup(new Uri(url));

                        if (!solution.DeployedWebApplications.Contains(webapp))
                        {
                            var webapps = new Collection<SPWebApplication> { webapp };

                            solution.Deploy(DateTime.Now, true, webapps, true);

                            Utilities.WaitForJobToFinish(solution);

                            foreach (SPSite siteCollection in webapp.Sites)
                            {
                                siteCollection.Features.Add(featureGuid, true);
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.ActivateOceanikContentQueryWebpartFeature:  " + ex.Message);
                throw;
            }
        }

        public void CreateTranslationList(string path, string url)
        {
            var objLists = new List(url, "/", path, "TranslationContents", uiUpdateProgressMethod);
            objLists.CreateTranslatorList();
        }

        public void CreatePageTransList(string pathNotTo, string pathAdmin, string url)
        {
            var objLists = new List(url, "/", "PagesTranslations", uiUpdateProgressMethod);
            objLists.CreatePagesTransList(pathNotTo, pathAdmin);
        }

        public void CreateLangVisibilityList(string url, string value)
        {
            var objLists = new List(url, "/", "LanguagesVisibility", uiUpdateProgressMethod);
            objLists.CreateLangVisibilityList(value);
        }

        public void CreateLoadBalancingServersList(string url)
        {
            var objLists = new List(url, "/", "LoadBalancingServers");
            objLists.CreateLoadBalancingServersList();
        }

        public void CreateConfigurationStoreServersList(string url)
        {
            var objLists = new List(url, "/", "Configuration Store", uiUpdateProgressMethod);
            objLists.CreateConfigurationStoreServersList();
        }

        public void CreateTroubleshootingStoreServersList(string url)
        {
            var objLists = new List(url, "/", "Troubleshooting Store", uiUpdateProgressMethod);
            objLists.CreateTroubleshootingStoreServersList();
        }

        public void CreateExtractorTranslationsList(string url)
        {
            var objLists = new List(url, "/", "ExtractorTranslations", uiUpdateProgressMethod);
            objLists.CreateExtractorTranslationsList();
        }

        public void CreatePagesToUpdateForTranslationList(string url)
        {
            var objLists = new List(url, "/", "PagesToUpdateForTranslation", uiUpdateProgressMethod);
            objLists.CreatePagesToUpdateForTranslationList();
        }

        public void CreateTransListNormView(string url)
        {
            try
            {
                var list = new List(url, "/", "TranslationContents", uiUpdateProgressMethod);

                var collViewFields = new StringCollection();
                
                try
                {
                    collViewFields.AddRange(Languages.Instance.AllLanguages);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in InstallHelper.CreateTransListDataSheetView: " + e.Message);
                }

                collViewFields.Add("LinkTitle");

                list.CreateViews(String.Empty, "AllPhrasesClassicView", collViewFields, 100, true, false, false, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateTransListNormView: " + ex.Message);
                throw;
            }
        }

        public void CreateTransListDataSheetView(string url)
        {
            try
            {
                var list = new List(url, "/", "TranslationContents", uiUpdateProgressMethod);

                var collViewFields = new StringCollection();

                try
                {
                    collViewFields.AddRange(Languages.Instance.AllLanguages);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in InstallHelper.CreateTransListDataSheetView: " + e.Message);
                }

                list.CreateViews(String.Empty, "AllPhrasesGridView", collViewFields, 100, true, false, true, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateTransListDataSheetView: " + ex.Message);
                throw;
            }
        }

        public void CreateTransListNonCustomizeNormalView(string url)
        {
            try
            {
                var list = new List(url, "/", "TranslationContents", uiUpdateProgressMethod);

                var collViewFields = new StringCollection();

                try
                {
                    collViewFields.AddRange(Languages.Instance.AllLanguages);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in InstallHelper.CreateTransListNonCustomizeNormalView: " + e.Message);
                }
                
                collViewFields.Add("LinkTitle");

                const string Query = "<Where><Eq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Eq></Where>";

                list.CreateViews(Query, "NativePhrasesClassicView", collViewFields, 100, true, false, false, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateTransListNonCustomizeNormalView: " + ex.Message);
                throw;
            }
        }

        public void CreateTransListNonCustomizeGridView(string url)
        {
            try
            {
                var list = new List(url, "/", "TranslationContents", uiUpdateProgressMethod);

                var collViewFields = new StringCollection();

                try
                {
                    collViewFields.AddRange(Languages.Instance.AllLanguages);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in InstallHelper.CreateTransListNonCustomizeGridView: " + e.Message);
                }

                const string Query = "<Where><Eq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Eq></Where>";

                list.CreateViews(Query, "NativePhrasesGridView", collViewFields, 100, true, false, true, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateTransListNonCustomizeGridView: " + ex.Message);
                throw;
            }
        }

        public void CreateTransListCustomizeNormalView(string url)
        {
            try
            {
                var list = new List(url, "/", "TranslationContents", uiUpdateProgressMethod);

                var collViewFields = new StringCollection();

                try
                {
                    collViewFields.AddRange(Languages.Instance.AllLanguages);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in InstallHelper.CreateTransListCustomizeNormalView: " + e.Message);
                }
                
                collViewFields.Add("LinkTitle");

                const string VarQuery = "<Where><Neq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Neq></Where>";

                list.CreateViews(VarQuery, "CustomizedPhrasesClassicView", collViewFields, 100, true, false, false, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateTransListCustomizeNormalView: " + ex.Message);
                throw;
            }
        }

        public void CreateTransListCustomizeGridView(string url)
        {
            try
            {
                var list = new List(url, "/", "TranslationContents", uiUpdateProgressMethod);

                var collViewFields = new StringCollection();

                try
                {
                    collViewFields.AddRange(Languages.Instance.AllLanguages);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in InstallHelper.CreateTransListCustomizeGridView: " + e.Message);
                }

                const string Query = "<Where><Neq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Neq></Where>";

                list.CreateViews(Query, "CustomizedPhrasesGridView", collViewFields, 100, true, true, true, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateTransListCustomizeGridView: " + ex.Message);
                throw;
            }
        }

        public void CreatePagesTransListNormalView(string url)
        {
            try
            {
                var list = new List(url, "/", "PagesTranslations", uiUpdateProgressMethod);

                var collViewFields = new StringCollection { "LinkTitle", "ToTranslate" };

                list.CreateViews(String.Empty, "ClassicView", collViewFields, 100, true, true, false, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreatePagesTransListNormalView: " + ex.Message);
                throw;
            }
        }

        public void CreatePagesTransListGridView(string url)
        {
            try
            {
                var list = new List(url, "/", "PagesTranslations", uiUpdateProgressMethod);

                var collViewFields = new StringCollection { "LinkTitle", "ToTranslate" };

                list.CreateViews(String.Empty, "GridView", collViewFields, 100, true, false, true, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreatePagesTransListGridView: " + ex.Message);
                throw;
            }
        }

        public void CreateLanguagesVisibilityNormalView(string url)
        {
            try
            {
                var list = new List(url, "/", "LanguagesVisibility", uiUpdateProgressMethod);

                var varCollViewFields = new StringCollection
                                            {
                                                "LinkTitle",
                                                "LanguagesDisplay",
                                                "IsVisible",
                                                "DefaultLanguage",
                                                "LanguagesPicture"
                                            };

                list.CreateViews(String.Empty, "ClassicView", varCollViewFields, 100, true, true, false, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateLanguagesVisibilityNormalView: " + ex.Message);
                throw;
            }
        }

        public void CreateLanguagesVisibilityGridView(string url)
        {
            try
            {
                var list = new List(url, "/", "LanguagesVisibility", uiUpdateProgressMethod);

                var collViewFields = new StringCollection
                                         {
                                             "LinkTitle",
                                             "LanguagesDisplay",
                                             "IsVisible",
                                             "DefaultLanguage",
                                             "LanguagesPicture"
                                         };

                list.CreateViews(String.Empty, "GridView", collViewFields, 100, true, false, true, false);
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in InstallHelper.CreateLanguagesVisibilityGridView: " + ex.Message);
                throw;
            }
        }

        public void CreateExtractorTranslationsView(string url)
        {
            try
            {
                var list = new List(url, "/", "ExtractorTranslations", uiUpdateProgressMethod);

                var collViewFields = new StringCollection
                                         {
                                             "Page"
                                         };
                try
                {
                    collViewFields.AddRange(Languages.Instance.AllLanguages);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in InstallHelper.CreateExtractorTranslationsView: " + e.Message);
                }

                list.CreateViews(string.Empty, "ExtractorTranslationsView", collViewFields, 100, true, true, true, false);
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in InstallHelper.CreateExtractorTranslationsView: " + e.Message);
                throw;
            }
        }

        public void CreatePagesToUpdateForTranslationView(string url)
        {
            try
            {
                var lists = new List(url, "/", "PagesToUpdateForTranslation", uiUpdateProgressMethod);

                var collViewFields = new StringCollection { "Pages" };

                lists.CreateViews(string.Empty, "PagesToUpdateForTranslation", collViewFields, 100, true, true, true, false);
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in InstallHelper.CreatePagesToUpdateForTranslationView: " + e.Message);
                throw;
            }
        }

        public void CreateConfigurationStoreView(string url)
        {
            try
            {
                var list = new List(url, "/", "Configuration Store", uiUpdateProgressMethod);

                var collViewFields = new StringCollection
                                         {
                                             "Title",
                                             "Config Category",
                                             "Config Item Description",
                                             "Config Value"                                             
                                         };

                list.CreateViews(string.Empty, "Configuration Store", collViewFields, 100, true, true, false, false);
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in InstallHelper.CreateConfigurationStoreView: " + e.Message);
                throw;
            }
        }

        public void LinkCheckDefaultLangEvent(string url)
        {
            const string AssemblyName = "CheckDefaultLangEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be";
            const string ClassName = "CheckDefaultLangEvent.ItemEvent";
            var list = new List(url, "/", "LanguagesVisibility");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemUpdated");

            // Remove Old Assembly
            const string OldAssemblyName = "CheckDefaultLangEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7b49392f9e06d3f3";
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemUpdated");
        }

        public void LinkCreateGuidEvent(string url)
        {
            const string AssemblyName = "CreateGUIDEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be";
            const string ClassName = "CreateGUIDEvent.GenerateId";
            var list = new List(url, "/", "TranslationContents", uiUpdateProgressMethod);
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemAdded");

            // Remove Old Assembly
            const string OldAssemblyName = "CreateGUIDEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4a041d2cfd549304";
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemAdded");
        }

        public void LinkReloadCacheEvent(string url)
        {
            const string AssemblyName = "ReloadCacheEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be";
            const string OldAssemblyName = "ReloadCacheEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=45fd9f0eaea17ba9";
            const string ClassName = "ReloadCacheEvent.CacheEvent";

            ////
            var list = new List(url, "/", "TranslationContents");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemAdded");

            // Remove Old Assembly
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemAdded");

            list = new List(url, "/", "TranslationContents");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemDeleting");

            // Remove Old Assembly
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemDeleting");

            list = new List(url, "/", "TranslationContents");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemUpdated");

            // Remove Old Assembly
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemUpdated");

            ////
            list = new List(url, "/", "LanguagesVisibility");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemUpdated");

            // Remove Old Assembly
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemUpdated");

            ////
            list = new List(url, "/", "PagesTranslations");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemUpdated");

            // Remove Old Assembly
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemUpdated");

            list = new List(url, "/", "PagesTranslations");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemDeleting");

            // Remove Old Assembly
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemDeleting");

            list = new List(url, "/", "PagesTranslations");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemAdded");

            // Remove Old Assembly
            list.UnlinkEventHandlerToList(OldAssemblyName, ClassName, "ItemAdded");

            ////
            list = new List(url, "/", "Configuration Store");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemUpdated");

            list = new List(url, "/", "Configuration Store");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemDeleting");

            list = new List(url, "/", "Configuration Store");
            list.LinkEventHandlerToList(AssemblyName, ClassName, "ItemAdded");
        }

        public void SaveInfoToFile(string wwwRootPath, string applicationFolder)
        {
            var webPathWriter = new StreamWriter(applicationFolder + "\\WebPath.txt");

            webPathWriter.WriteLine(GetUrl());
            webPathWriter.WriteLine(wwwRootPath);
            webPathWriter.Flush();
            webPathWriter.Close();
        }

        private static void DeleteTransList(string url)
        {
            try
            {
                var obj = new List(url, "/", "TranslationContents");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the TranslationContents List:  " + e.Message);
                throw;
            }
        }

        private static void DeleteVisibilityList(string url)
        {
            try
            {
                var obj = new List(url, "/", "LanguagesVisibility");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the LanguagesVisibility List:  " + e.Message);
                throw;
            }
        }

        private static void DeletePagesTransList(string url)
        {
            try
            {
                var obj = new List(url, "/", "PagesTranslations");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the PagesTranslations List:  " + e.Message);
                throw;
            }
        }

        private static void DeleteLoadBalancingServersList(string url)
        {
            try
            {
                var obj = new List(url, "/", "LoadBalancingServers");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the LoadBalancingServers List:  " + e.Message);
                throw;
            }
        }

        private static void DeleteConfigurationStoreServersList(string url)
        {
            try
            {
                var obj = new List(url, "/", "Configuration Store");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the Configuration Store List:  " + e.Message);
                throw;
            }
        }

        private static void DeleteTroubleshootingStoreServersList(string url)
        {
            try
            {
                var obj = new List(url, "/", "Troubleshooting Store");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the Troubleshooting Store List:  " + e.Message);
                throw;
            }
        }

        private static void DeleteExtractorTranslationsList(string url)
        {
            try
            {
                var obj = new List(url, "/", "ExtractorTranslations");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the ExtractorTranslations List:  " + e.Message);
                throw;
            }
        }

        private static string AppFolderPathFilter(string applicationFolder)
        {
            string functionReturn;
            if (applicationFolder.EndsWith("/") || applicationFolder.EndsWith(@"\"))
            {
                functionReturn = applicationFolder;
                while (functionReturn.EndsWith("/") || functionReturn.EndsWith(@"\"))
                {
                    int length = functionReturn.Length;
                    functionReturn = functionReturn.Remove(length - 1);
                }
            }
            else
            {
                functionReturn = applicationFolder;
            }

            return functionReturn;
        }

        private static void DeletePagesToUpdateForTranslationList(string url)
        {
            try
            {
                var obj = new List(url, "/", "PagesToUpdateForTranslation");
                obj.DeleteList();
            }
            catch (Exception e)
            {
                Utilities.LogException("cannot delete the PagesToUpdateForTranslation List:  " + e.Message);
                throw;
            }
        }

        private string GetUrl()
        {
            string installationUrl = Context.Parameters["WEBAPPURL"];
            if (installationUrl.EndsWith("/"))
            {
                installationUrl = installationUrl.Substring(0, installationUrl.Length - 1);
            }

            return installationUrl;
        }
    }
}
