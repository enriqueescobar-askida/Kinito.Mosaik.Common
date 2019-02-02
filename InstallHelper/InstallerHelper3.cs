using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using ListsInstallation;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace InstallHelper
{
    [RunInstaller(true)]
    public partial class InstallerHelper : Installer
    {
        public InstallerHelper()
        {
            InitializeComponent();
        }

        


        #region Install
       
        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
            //LinkCheckDefaultLangEvent(GetUrl());
            //LinkCreateGuidEvent(GetUrl());
            //LinkReloadCacheEvent(GetUrl());
        }
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            try
            {
                base.Install(stateSaver);
              
                string VarApplicationFolder = Context.Parameters["APPFOLDER"];
                //string VarWwwRootPath = Context.Parameters["WWWROOTPATH"];
                VarApplicationFolder = AppFolderPathFilter(VarApplicationFolder);
                //VarWwwRootPath = AppFolderPathFilter(VarWwwRootPath);

               

                //CreateTransList(VarApplicationFolder + "\\translations.txt",GetUrl());
                //CreatePageTransList(VarApplicationFolder + "\\pagesNotToTranslate.txt", VarApplicationFolder + "\\adminPagesToTranslate.txt", GetUrl());
                //CreateLangVisibilityList(GetUrl());
                
                //// Create the views
                //CreateTransListNormView(GetUrl());
                //CreateTransListDataSheetView(GetUrl());
                //CreateTransListNonCustomizeNormalView(GetUrl());
                //CreateTransListNonCustomizeGridView(GetUrl());
                //CreateTransListCustomizeNormalView(GetUrl());
                //CreateTransListCustomizeGridView(GetUrl());
                //CreatePagesTransListNormalView(GetUrl());
                //CreatePagesTransListGridView(GetUrl());
                //CreateLanguagesVisibilityNormalView(GetUrl());
                //CreateLanguagesVisibilityGridView(GetUrl());


                InstallManagementFeature(VarApplicationFolder);

                //SaveInfoToFile(VarWwwRootPath, VarApplicationFolder);
                //AddTranslatorToWebConfig(VarWwwRootPath);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
            
        }

        #endregion
        #region Uninstall
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            try
            {            
                string VarApplicationFolder = Context.Parameters["APPFOLDER"];
                VarApplicationFolder = AppFolderPathFilter(VarApplicationFolder);
                //string VarUrl = string.Empty;
                //string VarWwwRoot = string.Empty;
                //string FilePath = VarApplicationFolder + "\\WebPath.txt";

                
                //read configuration from the text file
                //if (File.Exists(FilePath))
                //{
                //    StreamReader VarFileReader = new StreamReader(FilePath);
                //    VarUrl = VarFileReader.ReadLine();
                //    VarWwwRoot = VarFileReader.ReadLine();
                //    UninstallManagementFeature(VarUrl, VarApplicationFolder);
                    UninstallManagementFeature(VarApplicationFolder);    

                    Thread.Sleep(30000);
                //    VarFileReader.Close();

                //    //delete the file
                //    File.Delete(FilePath);
                //    RemoveTranslatorFromWebConfig(VarWwwRoot);
                //    RemoveLists(VarUrl);                    
                //}
                //else
                //    MessageBox.Show("webpath.txt does not exist");
              

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

        
        #endregion
        #region Methods

        private void UninstallManagementFeature(string aApplicationFolder)
        {
            string VarBatchFile = aApplicationFolder + @"\Management Uninstall.bat";

            if (VarBatchFile.Contains("/"))
            {
                int VarSlashIndex = VarBatchFile.IndexOf("/");
                VarBatchFile = VarBatchFile.Remove(VarSlashIndex, 1);
            }

            //string VarAguments = aUrl;
            //ExecProcess(VarBatchFile, VarAguments);
            string VarAguments = string.Empty;
            ExecProcess(VarBatchFile, VarAguments);

        }

        public void DeActivateManagementFeature(string aUrl, string aApplicationFolder)
        {
            string VarBatchFile = aApplicationFolder + @"\Management Deactivate.bat";

            if (VarBatchFile.Contains("/"))
            {
                int VarSlashIndex = VarBatchFile.IndexOf("/");
                VarBatchFile = VarBatchFile.Remove(VarSlashIndex, 1);
            }

            string VarAguments = aUrl;
            ExecProcess(VarBatchFile, VarAguments);

        }
       

        #region Delete Lists
        public void RemoveLists(string aUrl)
        {
            DialogResult result = MessageBox.Show("Do you wish to delete the translator lists on the " + aUrl + " Web Application ?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DeletePagesTransList(aUrl);
                DeleteTransList(aUrl);
                DeleteVisibilityList(aUrl);
                DeleteLoadBalancingServersList(aUrl);
            }
        }
        private void DeleteTransList(string aUrl)
        {
            try
            {
                CLists obj = new CLists(aUrl, "/", "TranslationContents");
                obj.DeleteList();
            }
            catch (Exception)
            {

                MessageBox.Show("cannot delete the TranslationContents List");
            }

        }
        private void DeleteVisibilityList(string aUrl)
        {
            try
            {
                CLists obj = new CLists(aUrl, "/", "LanguagesVisibility");
                obj.DeleteList();
            }
            catch (Exception)
            {

                MessageBox.Show("cannot delete the LanguagesVisibility List");
            }

        }
        private void DeletePagesTransList(string aUrl)
        {
            try
            {
                CLists obj = new CLists(aUrl, "/", "PagesTranslations");
                obj.DeleteList();
            }
            catch (Exception)
            {

                MessageBox.Show("cannot delete the PagesTranslations List");
            }

        }
        private void DeleteLoadBalancingServersList(string aUrl)
        {
            try
            {
                CLists obj = new CLists(aUrl, "/", "LoadBalancingServers");
                obj.DeleteList();
            }
            catch (Exception)
            {

                MessageBox.Show("cannot delete the LoadBalancingServers List");
            }

        }

        #endregion
      

        
        public void RemoveTranslatorFromWebConfig(string aWWWRoot)
        {
            //make a copy of the web.config in order to have a backup 
            string VarBackupConfig = aWWWRoot + "\\web.config.beforeUninstallTransModule";
            string VarConfig = aWWWRoot + "\\web.config";

            if (File.Exists(VarConfig))
            {
                File.Copy(VarConfig, VarBackupConfig, true);

                //start reading the web.config in order to remove appropriate text
                StreamReader VarConfigReader = new StreamReader(VarConfig);
                string VarConfigContents = VarConfigReader.ReadToEnd();
                VarConfigReader.Close();

                //prepare for remove
                string VarModuleText = "<add name=\"AlphaMosaikTranslation\" type=\"TranslatorHttpHandler.TranslatorModule, TranslatorHttpHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a65beeb1a6acb37a\" />" + Environment.NewLine;
                if (VarConfigContents.Contains(VarModuleText))
                {
                    VarConfigContents = VarConfigContents.Replace(VarModuleText, "");
                    StreamWriter VarConfigWriter = new StreamWriter(VarConfig);
                    VarConfigWriter.Write(VarConfigContents);
                    VarConfigWriter.Flush();
                    VarConfigWriter.Close();

                }
            }else
            {
                MessageBox.Show("Web.config doesn't exist. Please verify the path of the Website physical path");
            }


        }
        public void AddTranslatorToWebConfig(string aWWWRoot)
        {
            StreamReader VarConfigReader=null;
            StreamWriter VarConfigWriter=null;

            try
            {
                //make a copy of the web.config in order to have a backup 
                string VarBackupConfig = aWWWRoot + "\\web.config.beforeInstallTransModule";
                string VarConfig = aWWWRoot + "\\web.config";
                File.Copy(VarConfig, VarBackupConfig, true);

                //start reading the web.config in order to add appropriate text
                 VarConfigReader = new StreamReader(VarConfig);
                string VarConfigContents = VarConfigReader.ReadToEnd();
                VarConfigReader.Close();

                //detect the location where you should insert your text
                string VarModuleText = "<add name=\"AlphaMosaikTranslation\" type=\"TranslatorHttpHandler.TranslatorModule, TranslatorHttpHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a65beeb1a6acb37a\" />" + Environment.NewLine;
                if (VarConfigContents.IndexOf(VarModuleText) == -1)
                {
                    int VarEndOfTheSectionIndex = VarConfigContents.IndexOf("</httpModules>");
                    VarConfigContents = VarConfigContents.Insert(VarEndOfTheSectionIndex, VarModuleText);
                    //start writing to webconfig
                    VarConfigWriter = new StreamWriter(VarConfig);
                    VarConfigWriter.Write(VarConfigContents);
                }
            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.AddTranslatorToWebConfig:  " + ex.Message);
            }
            finally
            {
                if (VarConfigWriter != null)
                {
                    VarConfigWriter.Flush();
                    VarConfigWriter.Close();
                }
            }

            
        }
        private string GetUrl()
        {
            string sInstallationUrl = string.Empty;

            //string sPortNumber = Context.Parameters["PORTNUMBER"];

            //sInstallationUrl = "http://localhost:" + sPortNumber;
            sInstallationUrl = Context.Parameters["WEBAPPURL"];
            if (sInstallationUrl.EndsWith("/"))
            {
                sInstallationUrl = sInstallationUrl.Substring(0, sInstallationUrl.Length - 1);
            }
            return sInstallationUrl;
        }
        private string AppFolderPathFilter(string aApplicationFolder)
        {
            string VarReturn = string.Empty;
            if (aApplicationFolder.EndsWith("/") || aApplicationFolder.EndsWith(@"\"))
            {
                VarReturn = aApplicationFolder;
                while (VarReturn.EndsWith("/") || VarReturn.EndsWith(@"\"))
                {
                    int Length = VarReturn.Length;
                    VarReturn = VarReturn.Remove(Length - 1);
                }
            }
            else
            {
                VarReturn = aApplicationFolder;
            }
            return VarReturn;
        }

        private void InstallManagementFeature(string aApplicationFolder)
        {
            string VarBatchFile = aApplicationFolder + "\\" + "Management Install.bat";
            string VarWSPFile = aApplicationFolder + "\\" + "Translator2009Admin.wsp";
            if (VarBatchFile.Contains("/"))
            {
                int VarSlashIndex = VarBatchFile.IndexOf("/");
                VarBatchFile = VarBatchFile.Remove(VarSlashIndex, 1);
            }

            //string VarAguments = GetUrl() + " " + "\"" + VarWSPFile + "\"";
            //ExecProcess(VarBatchFile, VarAguments);

            string VarAguments = "\"" + VarWSPFile + "\"";
            ExecProcess(VarBatchFile, VarAguments);
        }

        public void ActivateManagementFeature(string aApplicationFolder, string aUrl)
        {
            string VarBatchFile = aApplicationFolder + "\\" + "Management Activate.bat";
            string VarWSPFile = aApplicationFolder + "\\" + "Translator2009Admin.wsp";
            if (VarBatchFile.Contains("/"))
            {
                int VarSlashIndex = VarBatchFile.IndexOf("/");
                VarBatchFile = VarBatchFile.Remove(VarSlashIndex, 1);
            }

            string VarAguments = aUrl + " " + "\"" + VarWSPFile + "\"";
            ExecProcess(VarBatchFile, VarAguments);
        }

        private void ExecProcess(string aFileName, string aArguments)
        {
            try
            {
                Process Dimp = new Process();
                Dimp.StartInfo.FileName = aFileName;
                Dimp.StartInfo.Arguments = aArguments;                
                Dimp.Start();

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.ExecProcess: " + ex.Message);
            }

        }

        #region Create Lists
        public void CreateTransList(string aPath,string aUrl)
        {
            CLists objLists = new CLists(aUrl,"/", aPath, "TranslationContents");
            objLists.CreateTranslatorList();
        }
        public void CreatePageTransList(string aPathNotTo, string aPathAdmin,string aUrl)
        {
            CLists objLists = new CLists(aUrl, "/", "PagesTranslations");
            objLists.CreatePagesTransList(aPathNotTo, aPathAdmin);
        }

        public void CreateLangVisibilityList(string aUrl,string aValue)
        {
            CLists objLists = new CLists(aUrl, "/", "LanguagesVisibility");
            objLists.CreateLangVisibilityList(aValue);
        }

        public void CreateLoadBalancingServersList(string aUrl)
        {
            CLists objLists = new CLists(aUrl, "/", "LoadBalancingServers");
            objLists.CreateLoadBalancingServersList();
        }
        #endregion

        #region CreateViews
        
        #region TranslationContentsViews
        public void CreateTransListNormView(string aUrl)
        {
            try 
            {
                CLists objLists = new CLists(aUrl, "/", "TranslationContents");
                
                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("EN");
                VarCollViewFields.Add("FR");
                VarCollViewFields.Add("ES");
                VarCollViewFields.Add("KO");
                VarCollViewFields.Add("AR");
                VarCollViewFields.Add("JP");
                VarCollViewFields.Add("CH");
                VarCollViewFields.Add("PT");
                VarCollViewFields.Add("DE");
                VarCollViewFields.Add("IT");
                VarCollViewFields.Add("RU");
                VarCollViewFields.Add("LinkTitle");

                objLists.CreateViews(String.Empty, "AllPhrasesClassicView", VarCollViewFields, 100, true, false, false, false);
        		
            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateTransListNormView: " + ex.Message);
            }
            


        }

        public void CreateTransListDataSheetView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "TranslationContents");
                
                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("EN");
                VarCollViewFields.Add("FR");
                VarCollViewFields.Add("ES");
                VarCollViewFields.Add("KO");
                VarCollViewFields.Add("AR");
                VarCollViewFields.Add("JP");
                VarCollViewFields.Add("CH");
                VarCollViewFields.Add("PT");
                VarCollViewFields.Add("DE");
                VarCollViewFields.Add("IT");
                VarCollViewFields.Add("RU");
                
                objLists.CreateViews(String.Empty, "AllPhrasesGridView", VarCollViewFields, 100, true, false, true, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateTransListDataSheetView: " + ex.Message);
            }



        }

        public void CreateTransListNonCustomizeNormalView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "TranslationContents");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("EN");
                VarCollViewFields.Add("FR");
                VarCollViewFields.Add("ES");
                VarCollViewFields.Add("KO");
                VarCollViewFields.Add("AR");
                VarCollViewFields.Add("JP");
                VarCollViewFields.Add("CH");
                VarCollViewFields.Add("PT");
                VarCollViewFields.Add("DE");
                VarCollViewFields.Add("IT");
                VarCollViewFields.Add("RU");
                VarCollViewFields.Add("LinkTitle");

                string VarQuery = "<Where><Eq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Eq></Where>";


                objLists.CreateViews(VarQuery, "NativePhrasesClassicView", VarCollViewFields, 100, true, false, false, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateTransListNonCustomizeNormalView: " + ex.Message);
            }
        }

        public void CreateTransListNonCustomizeGridView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "TranslationContents");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("EN");
                VarCollViewFields.Add("FR");
                VarCollViewFields.Add("ES");
                VarCollViewFields.Add("KO");
                VarCollViewFields.Add("AR");
                VarCollViewFields.Add("JP");
                VarCollViewFields.Add("CH");
                VarCollViewFields.Add("PT");
                VarCollViewFields.Add("DE");
                VarCollViewFields.Add("IT");
                VarCollViewFields.Add("RU");
                
                string VarQuery = "<Where><Eq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Eq></Where>";


                objLists.CreateViews(VarQuery, "NativePhrasesGridView", VarCollViewFields, 100, true, false, true, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateTransListNonCustomizeGridView: " + ex.Message);
            }
        }

        public void CreateTransListCustomizeNormalView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "TranslationContents");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("EN");
                VarCollViewFields.Add("FR");
                VarCollViewFields.Add("ES");
                VarCollViewFields.Add("KO");
                VarCollViewFields.Add("AR");
                VarCollViewFields.Add("JP");
                VarCollViewFields.Add("CH");
                VarCollViewFields.Add("PT");
                VarCollViewFields.Add("DE");
                VarCollViewFields.Add("IT");
                VarCollViewFields.Add("RU");
                VarCollViewFields.Add("LinkTitle");

                string VarQuery = "<Where><Neq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Neq></Where>";


                objLists.CreateViews(VarQuery, "CustomizedPhrasesClassicView", VarCollViewFields, 100, true, false, false, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateTransListCustomizeNormalView: " + ex.Message);
            }
        }

        public void CreateTransListCustomizeGridView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "TranslationContents");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("EN");
                VarCollViewFields.Add("FR");
                VarCollViewFields.Add("ES");
                VarCollViewFields.Add("KO");
                VarCollViewFields.Add("AR");
                VarCollViewFields.Add("JP");
                VarCollViewFields.Add("CH");
                VarCollViewFields.Add("PT");
                VarCollViewFields.Add("DE");
                VarCollViewFields.Add("IT");
                VarCollViewFields.Add("RU");
                
                string VarQuery = "<Where><Neq><FieldRef Name=\"isCustomize\" /><Value Type=\"Boolean\">0</Value></Neq></Where>";


                objLists.CreateViews(VarQuery, "CustomizedPhrasesGridView", VarCollViewFields, 100, true, true, true, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateTransListCustomizeGridView: " + ex.Message);
            }
        }

        #endregion

        #region PagesTransListViews

        public void CreatePagesTransListNormalView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "PagesTranslations");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("LinkTitle");
                VarCollViewFields.Add("ToTranslate");

                objLists.CreateViews(String.Empty, "ClassicView", VarCollViewFields, 100, true, true, false, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreatePagesTransListNormalView: " + ex.Message);
            }
        }

        public void CreatePagesTransListGridView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "PagesTranslations");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("LinkTitle");
                VarCollViewFields.Add("ToTranslate");

                objLists.CreateViews(String.Empty, "GridView", VarCollViewFields, 100, true, false, true, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreatePagesTransListGridView: " + ex.Message);
            }
        }

        #endregion

        #region LanguagesVisibilityListViews

        public void CreateLanguagesVisibilityNormalView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "LanguagesVisibility");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("LinkTitle");                
                VarCollViewFields.Add("LanguagesDisplay");
                VarCollViewFields.Add("IsVisible");
                VarCollViewFields.Add("DefaultLanguage");

                objLists.CreateViews(String.Empty, "ClassicView", VarCollViewFields, 100, true, true, false, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateLanguagesVisibilityNormalView: " + ex.Message);
            }
        }

        public void CreateLanguagesVisibilityGridView(string aUrl)
        {
            try
            {
                CLists objLists = new CLists(aUrl, "/", "LanguagesVisibility");

                System.Collections.Specialized.StringCollection VarCollViewFields = new System.Collections.Specialized.StringCollection();
                VarCollViewFields.Add("LinkTitle");                
                VarCollViewFields.Add("LanguagesDisplay");
                VarCollViewFields.Add("IsVisible");
                VarCollViewFields.Add("DefaultLanguage");

                objLists.CreateViews(String.Empty, "GridView", VarCollViewFields, 100, true, false, true, false);

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Translator 2009", "Error in InstallHelper.CreateLanguagesVisibilityGridView: " + ex.Message);
            }
        }

        #endregion

        #endregion

        #region LinkEventHandlerToLists

        public void LinkCheckDefaultLangEvent(string aUrl)
        {
            string aAssemblyName = "CheckDefaultLangEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7b49392f9e06d3f3";
            string aClassName = "CheckDefaultLangEvent.CEvent";
            CLists objLists = new CLists(aUrl, "/", "LanguagesVisibility");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemUpdated");
        }

        public void LinkCreateGuidEvent(string aUrl)
        {
            string aAssemblyName = "CreateGUIDEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4a041d2cfd549304";
            string aClassName = "CreateGUIDEvent.CGenerateID";
            CLists objLists = new CLists(aUrl, "/", "TranslationContents");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemAdded");
        }

        public void LinkReloadCacheEvent(string aUrl)
        {
            string aAssemblyName = "ReloadCacheEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=45fd9f0eaea17ba9";
            string aClassName = "ReloadCacheEvent.CacheEvent";

            ////
            CLists objLists = new CLists(aUrl, "/", "TranslationContents");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemAdded");

            objLists = new CLists(aUrl, "/", "TranslationContents");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemDeleting");

            objLists = new CLists(aUrl, "/", "TranslationContents");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemUpdated");

            ////
            objLists = new CLists(aUrl, "/", "LanguagesVisibility");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemUpdated");

            ////
            objLists = new CLists(aUrl, "/", "PagesTranslations");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemUpdated");

            objLists = new CLists(aUrl, "/", "PagesTranslations");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemDeleting");

            objLists = new CLists(aUrl, "/", "PagesTranslations");
            objLists.LinkEventHandlerToList(aAssemblyName, aClassName, "ItemAdded");

        }

       

        #endregion

        public void SaveInfoToFile(string aWWWRootPath, string aApplicationFolder)
        {
            StreamWriter webPathWriter = new StreamWriter(aApplicationFolder + "\\WebPath.txt");
            
            webPathWriter.WriteLine(GetUrl());
            webPathWriter.WriteLine(aWWWRootPath);
            webPathWriter.Flush();
            webPathWriter.Close();
        }
        #endregion
        
       
       
    }
}