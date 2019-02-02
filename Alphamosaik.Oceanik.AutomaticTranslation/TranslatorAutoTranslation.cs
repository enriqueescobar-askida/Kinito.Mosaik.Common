// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorAutoTranslation.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the TranslatorAutoTranslation type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Alphamosaik.Common.SharePoint.Library;
using System.IO;
using Alphamosaik.Oceanik.Sdk;
using System.Threading;
using Microsoft.SharePoint;
using Translator.Common.Library;
using Exception = System.Exception;
using Utilities = Translator.Common.Library.Utilities;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;
using System.Web.UI.WebControls.WebParts;

namespace Alphamosaik.Oceanik.AutomaticTranslation
{
    public class TranslatorAutoTranslation
    {
        private static readonly SystranTranslationWebService Tws = new SystranTranslationWebService();

        const string _profileFieldName = "Translation Profile"; //"Translation_X0020_Profile";
        const string _profileListName = "Translation Profiles";
        const string _profileIdFieldName = "Profile ID"; //"Profile_X0020_ID";

        public static void CreateClonedMultilingualItem(IAutomaticTranslation automaticTranslationPlugin, SPWeb siteWeb, string listId, string url, string itemId, string lang,
            bool toTranslate, bool discussionBoardEdition)
        {
            try
            {
                using (var currentSite = new SPSite(url))
                using (SPWeb web = currentSite.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    SPList currentList = web.Lists[new Guid(listId)];

                    //SPItem currentItem;

                    //if (currentList.BaseTemplate == SPListTemplateType.DiscussionBoard)
                    //{
                    //    currentItem = currentList.Folders.
                    //}

                    SPListItem currentItem = currentList.GetItemById(Convert.ToInt32(itemId));

                    string currentItemLanguage = string.Empty;

                    if (currentList.Fields.ContainsField("SharePoint_Item_Language"))
                    {
                        currentItemLanguage = (string)currentItem["SharePoint_Item_Language"];
                        currentItemLanguage = currentItemLanguage.Replace("(SPS_LNG_ALL)", string.Empty).Replace("SPS_LNG_", string.Empty);
                    }

                    var displayedLang = new ArrayList();

                    if (lang == "ALL")
                        foreach (LanguageItem languageItem in BaseStaticOverride<Dictionaries>.Instance.VisibleLanguages)
                        {
                            if (currentItemLanguage != languageItem.LanguageDestination)
                                displayedLang.Add(languageItem.LanguageDestination);
                        }
                    else
                        displayedLang.Add(lang);

                    foreach (string langTmp in displayedLang)
                    {
                        lang = langTmp;

                        if (currentList.Fields.ContainsField(lang + " version"))
                        {
                            bool isLinkedItemExist = false;
                            SPListItem cloneItem = null;
                            if (currentItem[lang + " version"] != null)
                            {
                                string linkedItemId = currentItem[lang + " version"].ToString();
                                var query = new SPQuery
                                {
                                    Query = "<Where><Eq><FieldRef Name='ID'/>" +
                                            "<Value Type='Number'>" +
                                            linkedItemId.Remove(linkedItemId.IndexOf(";")) +
                                            "</Value></Eq></Where>",
                                    QueryThrottleMode = SPQueryThrottleOption.Override
                                };

                                if ((discussionBoardEdition) && (currentItem.Folder == null))
                                    query.ViewAttributes = "Scope='Recursive'";

                                SPListItemCollection collListItems = currentList.GetItems(query);
                                if (collListItems.Count > 0)
                                {
                                    foreach (SPListItem listItemTmp in collListItems)
                                    {
                                        string listItemTmpLanguage = listItemTmp["SharePoint_Item_Language"].ToString();
                                        if (listItemTmpLanguage == "SPS_LNG_" + lang)
                                        {
                                            isLinkedItemExist = true;
                                            cloneItem = listItemTmp;
                                        }
                                    }
                                }
                            }

                            bool toOverwrite = false;
                            if (currentItem["ItemsAutoCreation"] != null)
                                if (currentItem["ItemsAutoCreation"].ToString() == "Overwrite/Create items for all languages")
                                {
                                    toOverwrite = true;
                                }

                            if ((!isLinkedItemExist) || toOverwrite)
                            {
                                string cloneItemName;

                                if (currentList.BaseType == SPBaseType.DocumentLibrary)
                                    cloneItemName = currentItem.File.Name.Remove(currentItem.File.Name.LastIndexOf(".")) + "_" + lang + currentItem.Name.Substring(currentItem.Name.LastIndexOf("."));
                                else
                                    cloneItemName = currentItem.Name + "_" + lang;

                                if (!isLinkedItemExist)
                                {
                                    if (currentList.BaseType == SPBaseType.DocumentLibrary)
                                    {
                                        byte[] fileBytes = currentItem.File.OpenBinary();
                                        SPFile file = currentList.RootFolder.Files.Add(currentList.RootFolder.Url + "/" + cloneItemName, fileBytes);
                                        cloneItem = file.Item;
                                        cloneItem["Title"] = cloneItemName;
                                        cloneItem["ItemsAutoCreation"] = "None";
                                        cloneItem.SystemUpdate(false);
                                    }
                                    else
                                        if (currentList.BaseTemplate == SPListTemplateType.DiscussionBoard)
                                        {
                                            string discussionParentId = string.Empty;


                                            //if (currentUrl.IndexOf("&DiscussionParentID=") > -1)
                                            //{
                                            //    discussionParentId = currentUrl.Substring(currentUrl.IndexOf("=", currentUrl.IndexOf("&DiscussionParentID=")) + 1);
                                            //    if (discussionParentId.IndexOf("&") > -1)
                                            //        discussionParentId = discussionParentId.Remove(discussionParentId.IndexOf("&"));
                                            //}

                                            //if (discussionParentId == string.Empty)
                                            //if (web.GetFolder(currentItem.Url).ParentFolder == null)
                                            if (currentItem.Folder != null)
                                            {
                                                cloneItem = SPUtility.CreateNewDiscussion(currentList.Items, cloneItemName); //  currentList.Items.Add();
                                            }
                                            else
                                            {
                                                SPFolder parentFolder = web.GetFolder(currentItem.Url).ParentFolder;
                                                SPListItem parentDiscussion = web.GetFolder(currentItem.Url).ParentFolder.Item;

                                                //SPListItem parentDiscussion = currentList.GetItemById(Convert.ToInt32(discussionParentId));

                                                if ((parentDiscussion["SharePoint_Group_Language"] != null) && (Convert.ToInt32(parentDiscussion["SharePoint_Group_Language"]) != 0))
                                                {
                                                    var query = new SPQuery();
                                                    query.Query = "<Where><And><Eq><FieldRef Name=\"SharePoint_Item_Language\" /><Value Type=\"Text\">SPS_LNG_" + lang + "</Value></Eq>" +
                                                        "<Eq><FieldRef Name='SharePoint_Group_Language'/>" +
                                                        "<Value Type='Number'>" + parentDiscussion["SharePoint_Group_Language"].ToString() + "</Value></Eq></And></Where>";

                                                    SPListItemCollection clonedDiscussionsCollection = currentList.GetItems(query);

                                                    foreach (SPListItem clonedDiscussion in clonedDiscussionsCollection)
                                                    {
                                                        cloneItem = SPUtility.CreateNewDiscussionReply(clonedDiscussion);
                                                        break;
                                                    }
                                                }
                                            }

                                            cloneItem[SPBuiltInFieldId.Body] = cloneItemName;
                                            cloneItem[SPBuiltInFieldId.TrimmedBody] = currentItem[SPBuiltInFieldId.TrimmedBody];

                                            cloneItem["Title"] = cloneItemName;
                                            cloneItem["ItemsAutoCreation"] = "None";

                                            cloneItem.SystemUpdate(false);
                                        }
                                        else
                                        {
                                            cloneItem = currentList.Items.Add();

                                            cloneItem["Title"] = cloneItemName;
                                            cloneItem["ItemsAutoCreation"] = "None";

                                            cloneItem.SystemUpdate(false);
                                        }
                                }

                                var oQuery = new SPQuery
                                {
                                    QueryThrottleMode = SPQueryThrottleOption.Override
                                };

                                if (currentList.BaseType == SPBaseType.DocumentLibrary)
                                {
                                    oQuery.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/>" +
                                        "<Value Type='File'>" + cloneItemName + "</Value></Eq></Where>";
                                }
                                else
                                {
                                    oQuery.Query = "<Where><Eq><FieldRef Name='Title'/>" +
                                        "<Value Type='Text'>" + cloneItemName + "</Value></Eq></Where>";
                                }

                                if (currentList.Fields.ContainsField("MetadataToDuplicate"))
                                {
                                    if (currentItem["MetadataToDuplicate"] != null)
                                    {
                                        string[] metadataToUpdateArray = ((string)currentItem["MetadataToDuplicate"]).Split(';');
                                        foreach (string t in metadataToUpdateArray)
                                        {
                                            if (currentList.Fields.ContainsField(t))
                                            {
                                                cloneItem[t] = currentItem[t];
                                                cloneItem.SystemUpdate(false);
                                            }
                                        }
                                    }
                                }

                                bool isBodyExist = currentList.Fields.Cast<SPField>().Any(sPFieldTmp => sPFieldTmp.InternalName == "Body");

                                bool isTitleExist = currentList.Fields.Cast<SPField>().Any(sPFieldTmp => sPFieldTmp.InternalName == "Title");

                                bool isProfileExist = currentList.Fields.Cast<SPField>().Any(sPFieldTmp => sPFieldTmp.ToString() == _profileFieldName);

                                string profileId = string.Empty;

                                if (isProfileExist)
                                {
                                    profileId = GetProfileId(web, currentItem);
                                }

                                TranslationUserAccount translationUserAccount = null;

                                if (automaticTranslationPlugin != null)
                                {
                                    translationUserAccount = automaticTranslationPlugin.LoadUserAccount(web.Site.RootWeb, currentItemLanguage);
                                }

                                if (isBodyExist && (currentItem["Body"] != null))
                                {
                                    string bodyContentToTranslate = currentItem["Body"].ToString();
                                    string bodyContentTranslated = bodyContentToTranslate;

                                    if ((currentItem["AutoTranslation"] != null) && (currentItemLanguage != lang))
                                        if ((currentItem["AutoTranslation"].ToString() == "Yes") || toTranslate)
                                        {
                                            if (automaticTranslationPlugin != null)
                                            {
                                                bodyContentTranslated = automaticTranslationPlugin.TranslateText(bodyContentToTranslate, currentItemLanguage, lang, profileId, translationUserAccount, true);
                                                bodyContentTranslated += " " + HttpRuntime.Cache["AlphamosaikMessageForAutotranslateText"];
                                            }
                                            else
                                            {
                                                bodyContentTranslated = bodyContentToTranslate;
                                            }
                                            
                                            if ((currentList.BaseTemplate == SPListTemplateType.DiscussionBoard) && (cloneItem[SPBuiltInFieldId.TrimmedBody] != null))
                                            {
                                                if (automaticTranslationPlugin != null)
                                                {
                                                    cloneItem[SPBuiltInFieldId.TrimmedBody] = automaticTranslationPlugin.TranslateText(currentItem[SPBuiltInFieldId.TrimmedBody].ToString(), currentItemLanguage,
                                                    lang, profileId, translationUserAccount, true);

                                                    if (!string.IsNullOrEmpty(cloneItem[SPBuiltInFieldId.TrimmedBody].ToString()))
                                                    {
                                                        cloneItem[SPBuiltInFieldId.TrimmedBody] += " " + HttpRuntime.Cache["AlphamosaikMessageForAutotranslateText"];
                                                    }
                                                }
                                                else
                                                {
                                                    cloneItem[SPBuiltInFieldId.TrimmedBody] = currentItem[SPBuiltInFieldId.TrimmedBody];
                                                }                                                
                                            }
                                        }

                                    if (currentList.BaseTemplate == SPListTemplateType.DiscussionBoard)
                                    {
                                        CopyDiscussionAttachments(currentItem, cloneItem);
                                    }

                                    cloneItem["Body"] = bodyContentTranslated;
                                    cloneItem.SystemUpdate(false);
                                }

                                if (currentList.BaseType != SPBaseType.DocumentLibrary)
                                {
                                    if (isTitleExist && (currentItem["Title"] != null))
                                    {
                                        string titleContentToTranslate = currentItem["Title"].ToString();
                                        string titleContentTranslated = titleContentToTranslate;
                                        if (currentItem["AutoTranslation"] != null)
                                            if (automaticTranslationPlugin != null && ((currentItem["AutoTranslation"].ToString() == "Yes") || toTranslate))
                                            {
                                                titleContentTranslated = automaticTranslationPlugin.TranslateText(titleContentTranslated, currentItemLanguage, lang, profileId, translationUserAccount, false);
                                            }

                                        cloneItem["Title"] = titleContentTranslated;
                                        cloneItem.SystemUpdate(false);
                                    }
                                }
                                else
                                {
                                    if (currentItem["AutoTranslation"] != null)
                                        if ((currentItem["AutoTranslation"].ToString() == "Yes") || toTranslate)
                                        {
                                            if (currentList.BaseTemplate == (SPListTemplateType)(850))
                                            {
                                                string pageContentToTranslate = string.Empty;

                                                if (currentItem["PublishingPageContent"] != null)
                                                {
                                                    pageContentToTranslate = currentItem["PublishingPageContent"].ToString();
                                                }

                                                if (currentList.Fields.ContainsField("PublishingPageContent"))
                                                {
                                                    if (automaticTranslationPlugin != null)
                                                    {
                                                        cloneItem["PublishingPageContent"] = automaticTranslationPlugin.TranslateText(pageContentToTranslate, currentItemLanguage,
                                                        lang, profileId, translationUserAccount, true);
                                                    }
                                                    else
                                                    {
                                                        cloneItem["PublishingPageContent"] = pageContentToTranslate;
                                                    }

                                                    cloneItem.SystemUpdate(false);
                                                }
                                            }
                                            else if (automaticTranslationPlugin != null && automaticTranslationPlugin.SupportFileTranslation())
                                            {
                                                cloneItem.SystemUpdate(false);
                                                lock (cloneItem)
                                                {
                                                    byte[] fileBytes = currentItem.File.OpenBinary();
                                                    fileBytes = automaticTranslationPlugin.TranslateFile(fileBytes, currentItemLanguage, lang, profileId, translationUserAccount, currentItem.File.Name);
                                                    cloneItem.File.SaveBinary(fileBytes);
                                                }
                                            }
                                        }

                                    if (currentList.BaseTemplate == (SPListTemplateType)(850))
                                    {
                                        CreatePublishingPage(currentItem, cloneItem, web, toOverwrite);
                                    }
                                }


                                url = url.Replace("?SPS_Trans_Code=CreateClonedMultilingualItem&", "?").Replace("&SPS_Trans_Code=CreateClonedMultilingualItem", string.Empty);
                                Utilities.SetItemLanguage(siteWeb, listId, url, cloneItem.ID.ToString(), lang);
                                Utilities.LinkItemWith(siteWeb, listId, url, itemId, cloneItem.ID.ToString());
                            }
                            else
                            {
                                if (HttpContext.Current != null)
                                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=CreateClonedMultilingualItem&", "?")
                                                .Replace("&SPS_Trans_Code=CreateClonedMultilingualItem", string.Empty), false);
                            }
                        }
                        else
                        {
                            if (HttpContext.Current != null)
                                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=CreateClonedMultilingualItem&", "?")
                                            .Replace("&SPS_Trans_Code=CreateClonedMultilingualItem", string.Empty), false);
                        }
                    }
                }

                if (HttpContext.Current != null)
                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("?SPS_Trans_Code=CreateClonedMultilingualItem&", "?")
                                .Replace("&SPS_Trans_Code=CreateClonedMultilingualItem", string.Empty), false);
            }
            catch (Exception exc)
            {
                Utilities.LogException("Error in Translator2010.TranslatorAutoTranslation.CreateClonedMultilingualItem: " + exc.Message, EventLogEntryType.Warning);
            }
        }

        private static void CreatePublishingPage(SPListItem sourcePageItem, SPListItem destinationPageItem, SPWeb web, bool toOverwrite)
        {
            DeleteAllWebParts(destinationPageItem.File.ServerRelativeUrl, web, sourcePageItem.File.ServerRelativeUrl, web, toOverwrite);
            CopyAllWebParts(destinationPageItem.File.ServerRelativeUrl, web, sourcePageItem.File.ServerRelativeUrl, web, toOverwrite);
        }

        private static void DeleteAllWebParts(string destinationPageUrlServerRelative, SPWeb destinationPageWeb, string sourcePageUrlServerRelative, SPWeb sourcePageWeb, bool shouldOverwriteDestinationWebParts)
        {
            SPWeb web = null;
            SPWeb web2 = null;
            try
            {
                SPLimitedWebPartManager limitedWebPartManager = destinationPageWeb.GetLimitedWebPartManager(destinationPageUrlServerRelative, PersonalizationScope.Shared);
                SPLimitedWebPartManager manager2 = sourcePageWeb.GetLimitedWebPartManager(sourcePageUrlServerRelative, PersonalizationScope.Shared);
                web2 = limitedWebPartManager.Web;
                web = manager2.Web;
                SPLimitedWebPartCollection webParts = manager2.WebParts;
                SPLimitedWebPartCollection parts2 = limitedWebPartManager.WebParts;
                if (shouldOverwriteDestinationWebParts && (parts2 != null))
                {
                    while (parts2.Count > 0)
                    {
                        try
                        {
                            limitedWebPartManager.DeleteWebPart(parts2[0]);
                            continue;
                        }
                        catch (WebPartPageUserException)
                        {
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (HttpContext.Current != null)
                {
                    throw;
                }
            }
            finally
            {
                if ((web != sourcePageWeb) && (web != null))
                {
                    web.Close();
                }
                if ((web2 != destinationPageWeb) && (web2 != null))
                {
                    web2.Close();
                }
            }
        }

        private static void CopyAllWebParts(string destinationPageUrlServerRelative, SPWeb destinationPageWeb, string sourcePageUrlServerRelative, SPWeb sourcePageWeb, bool shouldOverwriteDestinationWebParts)
        {
            SPWeb web = null;
            SPWeb web2 = null;
            try
            {
                SPLimitedWebPartManager limitedWebPartManager = destinationPageWeb.GetLimitedWebPartManager(destinationPageUrlServerRelative, PersonalizationScope.Shared);
                SPLimitedWebPartManager manager2 = sourcePageWeb.GetLimitedWebPartManager(sourcePageUrlServerRelative, PersonalizationScope.Shared);
                web2 = limitedWebPartManager.Web;
                web = manager2.Web;
                SPLimitedWebPartCollection webParts = manager2.WebParts;
                SPLimitedWebPartCollection parts2 = limitedWebPartManager.WebParts;

                if (webParts.Count > 0)
                {
                    foreach (System.Web.UI.WebControls.WebParts.WebPart part in webParts)
                    {
                        if (!part.IsClosed)
                        {
                            System.Web.UI.WebControls.WebParts.WebPart webPart = parts2[part.ID];
                            if (webPart == null)
                            {
                                try
                                {
                                    string zoneID = manager2.GetZoneID(part);
                                    limitedWebPartManager.AddWebPart(part, zoneID, part.ZoneIndex);
                                }
                                catch (ArgumentException e)
                                {
                                }
                            }
                            else
                            {
                                if (webPart.IsClosed)
                                {
                                    limitedWebPartManager.OpenWebPart(webPart);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (HttpContext.Current != null)
                {
                    throw;
                }
            }
            finally
            {
                if ((web != sourcePageWeb) && (web != null))
                {
                    web.Close();
                }
                if ((web2 != destinationPageWeb) && (web2 != null))
                {
                    web2.Close();
                }
            }
        }

        private static string GetProfileId(SPWeb web, SPListItem currentItem)
        {
            //string profileName = string.Empty;
            string profileId = string.Empty;

            if (currentItem[_profileFieldName] != null)
                profileId = currentItem[_profileFieldName].ToString();

            //SPList profileList = web.Lists.TryGetList(_profileListName);
            //bool isTitleFieldInProfileListExist = false;
            //bool isProfileIdFieldInProfileListExist = false;

            //if (profileList != null)
            //{
            //    isTitleFieldInProfileListExist = profileList.Fields.Cast<SPField>().Any(sPFieldTmp => sPFieldTmp.InternalName == "Title");
            //    isProfileIdFieldInProfileListExist = profileList.Fields.Cast<SPField>().Any(sPFieldTmp => sPFieldTmp.ToString() == _profileIdFieldName);
            //}

            //if (isTitleFieldInProfileListExist && isProfileIdFieldInProfileListExist)
            //{
            //    SPListItem profileItem = profileList.Items.Cast<SPListItem>().First(sPListItemTmp => sPListItemTmp["Title"].ToString() == profileName);

            //    if ((profileItem != null) && (profileItem[_profileIdFieldName] != null))
            //    {
            //        profileId = profileItem[_profileIdFieldName].ToString();
            //    }
            //}

            return profileId;
        }

        private static void CopyDiscussionAttachments(SPListItem spliDiscussion, SPListItem targetItem)
        {
            try
            {
                SPFolder fldrDiscAttachments = spliDiscussion.Web.Folders["Lists"].SubFolders[spliDiscussion.ParentList.Title]
                    .SubFolders["Attachments"].SubFolders[spliDiscussion.ID.ToString()];

                //if (fldrDiscAttachments.Files.Count > 0)
                targetItem.SystemUpdate(false);

                SPFolderCollection targetSubFoldersCollection = targetItem.Web.Folders["Lists"].SubFolders[targetItem.ParentList.Title]
                    .SubFolders["Attachments"].SubFolders;

                bool isFolderExist = false;

                foreach (SPFolder folder in targetSubFoldersCollection)
                {
                    if (folder.Name == targetItem.ID.ToString())
                    {
                        isFolderExist = true;
                        break;
                    }
                }

                if (isFolderExist)
                {
                    SPFolder targetDiscAttachments = targetSubFoldersCollection[targetItem.ID.ToString()];

                    foreach (SPFile file in targetDiscAttachments.Files)
                    {
                        targetItem.Attachments.DeleteNow(file.Name);
                    }
                }

                foreach (SPFile file in fldrDiscAttachments.Files)
                {
                    byte[] binFile = file.OpenBinary();
                    targetItem.Attachments.AddNow(file.Name, binFile);
                }
            }
            catch { }
        }

        public string CallTranslationWebService(string toTranslate)
        {
            if (String.IsNullOrEmpty(toTranslate))
            {
                return toTranslate;
            }

            // Source language is English            
            const string SourceLang = "eng";

            // Target Language is French
            const string DestinationLang = "fra";

            toTranslate = toTranslate.Trim();
            string stringTranslated = toTranslate;

            // LWTRANSLI connects to  LW web service 
            var client = new LWTRANSLI(SourceLang, DestinationLang);

            // determin the username. (If you specify a username the job is related to this user.)
            const string User = "";
            const bool Unknown = true;
            const string Encoding = "";

            // This path is a path for saving data on server.
            if (!Directory.Exists("c:\\temp\\"))
                Directory.CreateDirectory("c:\\temp\\");

            const string Path = "c:\\temp\\";

            try
            {
                // The input is a text
                const string FilePath = Path + "\\temp.txt";
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                var file = new StreamWriter(FilePath);
                try
                {
                    file.Write(toTranslate);
                }
                finally
                {
                    file.Close();
                }

                // send a file contains the source text to translator.
                const string FileType = "text/plain";
                const string FileName = FilePath;
                int jobId = client.translate_File(FileName, FileType, Encoding, SourceLang, DestinationLang, User, Unknown);

                // receive translated text and save it in a file.
                const string ReceiveFilename = Path + "\\tempTransli.txt";
                client.receive(ReceiveFilename, jobId);

                using (var streamReader1 = new StreamReader(ReceiveFilename))
                {
                    stringTranslated = streamReader1.ReadToEnd();
                }

                // Remove the job from Queue.
                client.removeJobUser(User, jobId);
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in Translator2009.TranslatorAutoTranslation.CallTranslationWebService: " + e.Message, EventLogEntryType.Warning);
            }

            return stringTranslated;
        }

        public string CallStringTranslationSystranWebService(string toTranslate, string sourceLang, string destinationLang)
        {
            sourceLang = sourceLang.Replace("CH", "zh");
            destinationLang = destinationLang.Replace("CH", "zh");
            sourceLang = sourceLang.Replace("JP", "ja");
            destinationLang = destinationLang.Replace("JP", "ja");
            sourceLang = sourceLang.Replace("DU", "nl");
            destinationLang = destinationLang.Replace("DU", "nl");

            Tws.Url = "http://demov7.systran.fr/ws";

            // promt user for language pair and input text
            string lp = sourceLang.ToLower() + "_" + destinationLang.ToLower();
            string text = toTranslate;
            string gui = String.Empty;

            // create request object and set options
            var request = new Request { option = new Option[5] };
            request.option[0] = new Option { name = "lp", value = lp };
            request.option[1] = new Option { name = "input_text", value = text };
            request.option[2] = new Option { name = "translate_source", value = "string" };
            request.option[3] = new Option { name = "nfw_marker", value = String.Empty };
            request.option[4] = new Option { name = "gui", value = gui };

            // call the web service
            Response response = Tws.Translate(request);

            // wait for the request to complete
            string status = null; // status indicates the percent completion of the request
            string requestid = null; // requestid uniquely identifies the request
            foreach (Option option in response.option)
            {
                if (option.name == "status")
                    status = option.value;
                else if (option.name == "requestid")
                    requestid = option.value;
            }

            while (status == null || status != "100")
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                var infoRequest = new RequestInfo
                {
                    option = new Option[0],
                    infotype = "request",
                    requestid = requestid
                };
                response = Tws.GetInfo(infoRequest);
                foreach (Option option in response.option)
                    if (option.name == "status")
                        status = option.value;
            }

            // read the results
            var results = new Hashtable();
            foreach (Option option in response.option)
                results[option.name] = option.value;

            // display the results
            if ((string)results["error"] != null)
            {
                return toTranslate;
            }

            return Convert.ToString(results["output_text"]);
        }
    }
}
