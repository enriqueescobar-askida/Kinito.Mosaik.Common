// --------------------------------------------------------------------------------------------------------------------
// <copyright file="List.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the List type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace ListsInstallation
{
    public class List
    {
        public delegate void UiUpdateProgressDelegate(String progressText);
        private UiUpdateProgressDelegate uiUpdateProgressDelegate;

        public List(string siteUrl, string webUrl, string txtPath, string listName)
        {
            SiteUrl = siteUrl;
            WebUrl = webUrl;
            TxtPathValue = txtPath;
            ListName = listName;
        }

        public List(string siteUrl, string webUrl, string txtPath, string listName, UiUpdateProgressDelegate progressDelegate)
        {
            SiteUrl = siteUrl;
            WebUrl = webUrl;
            TxtPathValue = txtPath;
            ListName = listName;
            uiUpdateProgressDelegate = progressDelegate;
        }

        public List(string siteUrl, string webUrl, string listName)
        {
            SiteUrl = siteUrl;
            WebUrl = webUrl;
            ListName = listName;
        }

        public List(string siteUrl, string webUrl, string listName, UiUpdateProgressDelegate progressDelegate)
        {
            SiteUrl = siteUrl;
            WebUrl = webUrl;
            ListName = listName;
            uiUpdateProgressDelegate = progressDelegate;
        }
                
        public string ListName { get; set; }

        public string TxtPathValue { get; set; }

        public string SiteUrl { get; set; }

        public string WebUrl { get; set; }

        public void UpdateProgress(string progressText)
        {
            if (uiUpdateProgressDelegate != null)
            {
                uiUpdateProgressDelegate(progressText);
            }
        }

        public void CreateTranslatorList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                UpdateProgress("Creating Translator List");
                CreateList();

                UpdateProgress("Creating Translator List Columns");
                CreateTranslatorListColumns();

                UpdateProgress("Adding dictionary file content to list");
                ReadTranslatorTxt();
            }
        }

        public void CreatePagesTransList(string pageNotToTransPath, string adminPageToTransPath)
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                UpdateProgress("Creating Page Translator List");
                CreateList();

                UpdateProgress("Creating Page Translator List Columns");
                CreatePagesTransListColumns();

                UpdateProgress("Adding dictionary file content to page list");
                ReadPagesTransTxt(pageNotToTransPath, adminPageToTransPath);
            }
        }

        public void CreateLoadBalancingServersList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                CreateList();
            }
        }

        public void CreateConfigurationStoreServersList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                UpdateProgress("Creating Configuration Store List");
                CreateList();

                using (var site = new SPSite(SiteUrl))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPList list = obj.GetList();

                        if (list.ContentTypesEnabled == false)
                        {
                            list.ContentTypesEnabled = true;
                            list.Update();
                        }

                        SPContentTypeCollection contentTypes = web.ContentTypes;

                        if (contentTypes["Configuration Item"] == null)
                        {
                            var newContentType = new SPContentType(contentTypes["Item"], contentTypes, "Configuration Item")
                            {
                                Group = "Config Store content types",
                                Description = "Represents an item in the config store."
                            };

                            web.Fields.Add("ConfigCategory", SPFieldType.Text, true);
                            var configCategoryFieldLink = new SPFieldLink(web.Fields["ConfigCategory"])
                            {
                                DisplayName = "Config Category"
                            };

                            newContentType.FieldLinks.Add(configCategoryFieldLink);

                            web.Fields.Add("ConfigValue", SPFieldType.Text, true);
                            var configValueFieldLink = new SPFieldLink(web.Fields["ConfigValue"])
                            {
                                DisplayName = "Config Value",
                                Required = false
                            };
                            newContentType.FieldLinks.Add(configValueFieldLink);

                            web.Fields.Add("ConfigItemDescription", SPFieldType.Text, false);
                            var configDescFieldLink = new SPFieldLink(web.Fields["ConfigItemDescription"])
                            {
                                DisplayName = "Config Item Description",
                                Required = false
                            };
                            newContentType.FieldLinks.Add(configDescFieldLink);

                            web.ContentTypes.Add(newContentType);
                            newContentType.Update();
                        }

                        SPContentType contentType = web.Site.RootWeb.ContentTypes["Configuration Item"];
                        list.ContentTypes.Add(contentType);
                        SPContentType itemContentType = list.ContentTypes["Item"];
                        list.ContentTypes.Delete(itemContentType.Id);
                        list.Update();
                    }
                }

                CreateConfigurationStoreItems();
            }
        }        

        public void CreateTroubleshootingStoreServersList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                UpdateProgress("Creating Troubleshooting Store List");
                CreateList();

                using (var site = new SPSite(SiteUrl))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPList list = obj.GetList();

                        if (list.ContentTypesEnabled == false)
                        {
                            list.ContentTypesEnabled = true;
                            list.Update();
                        }

                        SPContentType contentType = web.Site.RootWeb.ContentTypes["Configuration Item"];
                        list.ContentTypes.Add(contentType);
                        SPContentType itemContentType = list.ContentTypes["Item"];
                        list.ContentTypes.Delete(itemContentType.Id);
                        list.Update();
                    }
                }
            }
        }

        public void CreateExtractorTranslationsList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                UpdateProgress("Creating Extractor Translations List");

                CreateList();
                CreateExtractorTranslationsColumns();
            }
        }

        public void CreatePagesToUpdateForTranslationList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                UpdateProgress("Creating Page to Update for Translations List");
                CreateList();
                CreatePagesToUpdateForTranslationColumns();
            }
        }

        public void CreateLangVisibilityList(string value)
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsListExist())
            {
                UpdateProgress("Creating Language Visibility List");
                CreateList();
                CreateLangVisibilityColumns();
                CreateLangVisibilityItems(value);
            }
        }

        public void LinkEventHandlerToList(string assemblyName, string className, string aspEventReceiverType)
        {
            try
            {
                var obj = new Helper(SiteUrl, WebUrl, ListName);

                UpdateProgress("Linking Event Handler to List");

                SPList list = obj.GetList();
                if (aspEventReceiverType == "ItemUpdated")
                {
                    list.EventReceivers.Add(SPEventReceiverType.ItemUpdated, assemblyName, className);
                }

                if (aspEventReceiverType == "ItemAdded")
                {
                    list.EventReceivers.Add(SPEventReceiverType.ItemAdded, assemblyName, className);
                }

                if (aspEventReceiverType == "ItemDeleting")
                {
                    list.EventReceivers.Add(SPEventReceiverType.ItemDeleting, assemblyName, className);
                }

                list.Update();
            }
            catch (SPException ex)
            {
                Utilities.LogException("Error in Clist.LinkEventHandlerToList: " + ex.Message);
            }
        }

        public void UnlinkEventHandlerToList(string assemblyName, string className, string aspEventReceiverType)
        {
            try
            {
                var obj = new Helper(SiteUrl, WebUrl, ListName);

                SPList list = obj.GetList();

                var erdc = list.EventReceivers;
                var eventsToDelete = new List<SPEventReceiverDefinition>();

                SPEventReceiverType eventType = SPEventReceiverType.InvalidReceiver;

                if (aspEventReceiverType == "ItemUpdated")
                {
                    eventType = SPEventReceiverType.ItemUpdated;
                }

                if (aspEventReceiverType == "ItemAdded")
                {
                    eventType = SPEventReceiverType.ItemAdded;
                }

                if (aspEventReceiverType == "ItemDeleting")
                {
                    eventType = SPEventReceiverType.ItemDeleting;
                }

                foreach (SPEventReceiverDefinition erd in erdc)
                {
                    if (erd != null && erd.Assembly == assemblyName && erd.Type == eventType && erd.Class == className)
                    {
                        try
                        {
                            eventsToDelete.Add(erd);
                        }
                        catch (Exception e)
                        {
                            Utilities.LogException("Error in Clist.UnlinkEventHandlerToList: " + e.Message);
                        }
                    }
                }

                foreach (SPEventReceiverDefinition er in eventsToDelete)
                {
                    er.Delete();
                }  
            }
            catch (SPException ex)
            {
                Utilities.LogException("Error in Clist.UnlinkEventHandlerToList: " + ex.Message);
            }
        }

        public void CreateViews(string query, string viewName, StringCollection collViewFields, uint rowLimit, bool isPaged, bool isMakeViewDefault, bool isGridViewType, bool isPersonalView)
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            if (!obj.IsViewExist(viewName))
            {
                UpdateProgress("Creating Views");

                obj.CreateListView(viewName, query, collViewFields, rowLimit, isPaged, isMakeViewDefault,
                                   isGridViewType ? SPViewCollection.SPViewType.Grid : SPViewCollection.SPViewType.Html,
                                   isPersonalView);
            }
        }

        public void DeleteList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            obj.DeleteList();
        }

        private static bool IsLanguageInstalled(IEnumerable<CultureInfo> cultures, int languageLcid)
        {
            if (cultures != null)
            {
                foreach (var cultureInfo in cultures)
                {
                    if (cultureInfo.LCID == languageLcid)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CreateTranslatorListColumns()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            var ht = new Hashtable();

            foreach (string twoLetterIso in Languages.Instance.AllLanguages)
            {
                var fieldTwoLetterIso = new FieldObject(string.Empty, FieldObject.Types.Note, false);
                ht.Add(twoLetterIso, fieldTwoLetterIso);
            }

            var field = new FieldObject(string.Empty, FieldObject.Types.Boolean, false);
            ht.Add("isCustomize", field);
            obj.AddFields(ht);

            using (var site = new SPSite(SiteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList sharepointList = web.GetList("Lists/TranslationContents/AllItems.aspx");

                    SPFieldCollection fieldCollection = sharepointList.Fields;
                    var fieldTitle = new SPField(fieldCollection, "Title") { ReadOnlyField = true, Required = false };
                    fieldTitle.Update();
                }
            }
        }

        private void ReadTranslatorTxt()
        {
            SPSite currentsitecollection = null;
            SPWeb currentWeb = null;
            try
            {
                if (File.Exists(TxtPathValue))
                {
                    string currentLine;

                    var readerTemp = new StreamReader(TxtPathValue);
                    var nbItemInDico = 0;
                    while ((currentLine = readerTemp.ReadLine()) != null)
                    {
                        nbItemInDico++;
                    }
                    readerTemp.Close();
                    
                    currentsitecollection = new SPSite(SiteUrl);
                    currentWeb = currentsitecollection.OpenWeb();
                    SPList translationsList = currentWeb.Lists[ListName];
                    
                    var reader = new StreamReader(TxtPathValue);
                    var readItemInDico = 0;
                    while ((currentLine = reader.ReadLine()) != null)
                    {
                        readItemInDico++;
                        UpdateProgress("Adding item " + readItemInDico.ToString() + " of " + nbItemInDico.ToString() + " to Dictionnary");

                        string[] languages = currentLine.Split(';');

                        SPListItem currentTranslation = translationsList.Items.Add();
                        currentTranslation["Title"] = Guid.NewGuid().ToString();

                        for (int i = 0; i < languages.Length; i++)
                        {
                            currentTranslation[Languages.Instance.AllLanguages[i]] = languages[i];
                        }

                        currentTranslation["isCustomize"] = "false";

                        try
                        {
                            currentTranslation.Update();
                        }
                        catch (Exception e)
                        {
                            Utilities.LogException("Error in ReadTranslatorTxt: " + e.Message);
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in ReadTranslatorTxt: " + e.Message);
            }
            finally
            {
                if (currentWeb != null)
                {
                    currentWeb.Dispose();
                }

                if (currentsitecollection != null)
                {
                    currentsitecollection.Dispose();
                }
            }
        }

        private void CreatePagesTransListColumns()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            var ht = new Hashtable();
            var field = new FieldObject(string.Empty, FieldObject.Types.Boolean, true);
            ht.Add("ToTranslate", field);
            obj.AddFields(ht);
        }

        private void ReadPagesTransTxt(string pageNotToTransPath, string adminPageToTransPath)
        {
            StreamReader reader;
            Hashtable ht;
            FieldObject field;
            string currentLine;
            Helper obj;

            if (File.Exists(pageNotToTransPath))
            {
                reader = new StreamReader(pageNotToTransPath);

                obj = new Helper(SiteUrl, WebUrl, ListName);

                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        ht = new Hashtable();
                        field = new FieldObject(currentLine, FieldObject.Types.Text);
                        ht.Add("Title", field);
                        field = new FieldObject("false", FieldObject.Types.Boolean);
                        ht.Add("ToTranslate", field);
                        obj.AddItemToList(ht);
                    }
                }

                reader.Close();
            }

            if (File.Exists(adminPageToTransPath))
            {
                reader = new StreamReader(adminPageToTransPath);
                obj = new Helper(SiteUrl, WebUrl, ListName);
                while ((currentLine = reader.ReadLine()) != null)
                {
                    ht = new Hashtable();
                    field = new FieldObject(currentLine, FieldObject.Types.Text);
                    ht.Add("Title", field);
                    field = new FieldObject("true", FieldObject.Types.Boolean);
                    ht.Add("ToTranslate", field);
                    obj.AddItemToList(ht);
                }

                reader.Close();
            }
        }

        private void CreateExtractorTranslationsColumns()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            var ht = new Hashtable();

            foreach (string twoLetterIso in Languages.Instance.AllLanguages)
            {
                var fieldTwoLetterIso = new FieldObject(string.Empty, FieldObject.Types.Note, false);
                ht.Add(twoLetterIso, fieldTwoLetterIso);
            }
            
            obj.AddFields(ht);

            using (var site = new SPSite(SiteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList sharepointList = web.GetList("Lists/ExtractorTranslations/AllItems.aspx");

                    SPFieldCollection fieldCollection = sharepointList.Fields;
                    var sharepointFieldTitle = new SPField(fieldCollection, "Title")
                                                   {
                                                       ReadOnlyField = true,
                                                       Required = false
                                                   };
                    sharepointFieldTitle.Update();
                }
            }
        }

        private void CreatePagesToUpdateForTranslationColumns()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            var ht = new Hashtable();

            var field = new FieldObject(string.Empty, FieldObject.Types.Text, false);
            ht.Add("Pages", field);
            obj.AddFields(ht);

            using (var site = new SPSite(SiteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = web.GetList("Lists/PagesToUpdateForTranslation/AllItems.aspx");

                    SPFieldCollection fieldCollection = list.Fields;
                    var fieldTitle = new SPField(fieldCollection, "Title") { ReadOnlyField = true, Required = false };
                    fieldTitle.Update();
                }
            }
        }

        private void CreateLangVisibilityColumns()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            var ht = new Hashtable();
            var field = new FieldObject(string.Empty, FieldObject.Types.Text, true);

            ht.Add("LanguageCode", field);
            field = new FieldObject(string.Empty, FieldObject.Types.Boolean, true);
            ht.Add("IsVisible", field);
            field = new FieldObject(string.Empty, FieldObject.Types.Boolean, true);
            ht.Add("DefaultLanguage", field);
            field = new FieldObject(string.Empty, FieldObject.Types.Text, true);
            ht.Add("LanguagesDisplay", field);
            field = new FieldObject(string.Empty, FieldObject.Types.Text, false);
            ht.Add("LanguagesPicture", field);
            obj.AddFields(ht);

            using (var site = new SPSite(SiteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = web.GetList("Lists/LanguagesVisibility/AllItems.aspx");

                    SPFieldCollection fieldCollection = list.Fields;
                    var fieldLanguageCode = new SPField(fieldCollection, "LanguageCode") { ReadOnlyField = true };
                    fieldLanguageCode.Update();
                }
            }
        }

        private void CreateLangVisibilityItems(string value)
        {
            IEnumerable<CultureInfo> cultures = Utilities.GetSiteLanguageInstalled(SiteUrl);

            var obj = new Helper(SiteUrl, WebUrl, ListName);
            Hashtable ht;

            foreach (string language in Languages.Instance.AllLanguages)
            {
                string isLanguageInstalled = Convert.ToString(IsLanguageInstalled(cultures, Languages.Instance.GetLcid(language)));
                ht = new Hashtable();

                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(Languages.Instance.GetLcid(language));

                var field = new FieldObject(cultureInfo.EnglishName, FieldObject.Types.Text);
                ht.Add("Title", field);
                field = new FieldObject(language, FieldObject.Types.Text);
                ht.Add("LanguageCode", field);
                field = new FieldObject(isLanguageInstalled, FieldObject.Types.Boolean);
                ht.Add("IsVisible", field);
                field = cultureInfo.EnglishName.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1 ? new FieldObject("true", FieldObject.Types.Boolean) : new FieldObject("false", FieldObject.Types.Boolean);

                ht.Add("DefaultLanguage", field);
                field = new FieldObject(language, FieldObject.Types.Text);
                ht.Add("LanguagesDisplay", field);
                obj.AddItemToList(ht);
            }
        }

        private void CreateConfigurationStoreItems()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);

            var ht = new Hashtable();

            var field = new FieldObject("BannerPipe", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("True", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Display the symbole \"Pipe\" in the language banner", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);            
                        
            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("BannerCSSClass", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("False", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Create specific CSS class to each language link in the banner", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("LanguageFieldLabel", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject(string.Empty, FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Customize SharePoint_Item_Language label", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("BannerWithNoTranslation", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("False", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Deactivate translation of the language banner labels", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("DisplayItemDashboard", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("True", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Display the option \"Open linked items dasboard\" in items menu", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);
                                    
            ht = new Hashtable();
            field = new FieldObject("RedirectToLinkedPage", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("False", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Redirect pages acording to multilingual links", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("FilteringButton", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("True", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Display the button which deactivates multilingual filtering on pages", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("DictionaryAccessButton", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("True", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Display the dictionary access button in site actions menu", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);            

            ht = new Hashtable();
            field = new FieldObject("CompletingMode", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("True", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Display the button to access Completing mode", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("QuickLaunchFilter", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("True", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Activate the QuickLaunch multilingual filtering", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("TopNavigationBarFilter", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("True", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Activate the Top Navigation Bar multilingual filtering", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("MessageForAutotranslateText", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject(string.Empty, FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Message to add at the end of the announcements auto-translated with web service", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("BingApplicationId", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject(string.Empty, FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Bing Translator Application ID", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);

            ht = new Hashtable();
            field = new FieldObject("ResxFilesUpdate", FieldObject.Types.Text);
            ht.Add("Title", field);
            field = new FieldObject("Oceanik", FieldObject.Types.Text);
            ht.Add("Config Category", field);
            field = new FieldObject("False", FieldObject.Types.Text);
            ht.Add("Config Value", field);
            field = new FieldObject("Activate the ressource files update", FieldObject.Types.Text);
            ht.Add("Config Item Description", field);

            obj.AddItemToList(ht);                        
        }

        private void CreateList()
        {
            var obj = new Helper(SiteUrl, WebUrl, ListName);
            obj.CreateList();
        }
    }
}
