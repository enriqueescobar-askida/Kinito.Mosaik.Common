// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Helper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;

namespace Translator.Common.Library
{
    public class Helper : HelperParent
    {
        #region Contructor
        public Helper(string siteUrl, string siteName, string listName)
        {
            SiteUrl = siteUrl;
            SiteName = siteName;
            ListName = listName;
        }

        public Helper()
        {
            SiteUrl = string.Empty; 
            SiteName = string.Empty;
            ListName = string.Empty;
        }

        public Helper(string siteUrl, string siteName)
        {
            SiteUrl = siteUrl;
            SiteName = siteName;
            ListName = string.Empty;
        }

        public Helper(string siteUrl)
        {
            SiteUrl = siteUrl;
            SiteName = string.Empty;
            ListName = string.Empty;
        }

        #endregion

        #region Methods
        
        #region Item Actions
        /// <summary>
        /// add new items to sharepoint lists (don't forget to declare the constructor)
        /// </summary>
        /// <param name="fields"> hash table that contains the item values (key: the column name Value: the value inside the column)</param>
        public void AddItemToList(Hashtable fields)
        {
            if (!IsHashtableValid(fields))
                return;
            try
            {
                SPListItemCollection itemCollection = GetItemCollection();
                if (itemCollection == null)
                    return;

                SPListItem item = itemCollection.Add();
                ICollection keys = fields.Keys;
                FieldObject field;
                foreach (object key in keys)
                {
                    field = (FieldObject)fields[key];
                   
                    switch (field.ValueType)
                    {
                        case FieldObject.Types.Text:
                            item[key.ToString()] = Convert.ToString(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        case FieldObject.Types.Integer:
                            item[key.ToString()] = Convert.ToInt32(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        case FieldObject.Types.DateTime:
                            item[key.ToString()] = Convert.ToDateTime(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        
                        case FieldObject.Types.Number:
                            item[key.ToString()] = Convert.ToDecimal(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        case FieldObject.Types.Boolean:
                            item[key.ToString()] = Convert.ToBoolean(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "AddItemToList", "ClsHelper"));
                log.WriteToLog();
            }
        }

        /// <summary>
        /// search an item in sharepoint list (don't forget to declare the constructor)
        /// </summary>
        /// <param name="query">the spquery string</param>
        /// <returns>return the item found</returns>
        public SPListItem SearchItem(string query)
        {
            if (string.IsNullOrEmpty(query))
                return null;
            SPListItem item = null;
            try
            {
                if (GetItemCollection(query) != null)
                {
                    if (GetItemCollection(query).Count > 0)
                        item = GetItemCollection(query)[0];
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "SearchItem", "ClsHelper"));
                log.WriteToLog();
            }

            return item;
        }

        /// <summary>
        /// delete a sharepoint item
        /// </summary>
        /// <param name="item">the item to delete</param>
        public void DeleteItem(SPListItem item)
        {
            if (item == null)
                return;
            try
            {
                item.Delete();
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "DeleteItem", "ClsHelper"));
                log.WriteToLog();
            }
        }

        /// <summary>
        /// delete group of items in sharepoint
        /// </summary>
        /// <param name="items">the collection of items</param>
        public void DeleteItems(SPListItemCollection items)
        {
            if (items == null)
                return;

            try
            {
                int length = items.Count;

                for (int i = 0; i < length; i++)
                {
                    if (i == length - 1)
                        i = 0;

                    DeleteItem(items[i]);
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "DeleteItems", "ClsHelper"));
                log.WriteToLog();
            }
        }

        /// <summary>
        /// delete all the items in a shrepoint list (don't forget to declare the constructor)
        /// </summary>
        public void DeleteAllItemsInList()
        {
            try
            {
                SPListItemCollection itemCollection = GetItemCollection();
                if (itemCollection == null)
                    return;
                DeleteItems(itemCollection);
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "DeleteAllItemsInList", "ClsHelper"));
                log.WriteToLog();
            }
        }

        /// <summary>
        /// changes an item fields value
        /// </summary>
        /// <param name="fields">contains the collection of fields Names and values</param>
        /// <param name="item">the item that should be changed</param>
        public void ChangeItemInList(Hashtable fields, SPListItem item)
        {
            if (!IsHashtableValid(fields))
                return;
            if (item == null)
                return;
            FieldObject field;
            try
            {
                ICollection keys = fields.Keys;
                foreach (object key in keys)
                {
                    field = (FieldObject)fields[key];
                    switch (field.ValueType)
                    {
                        case FieldObject.Types.Text:
                            item[key.ToString()] = Convert.ToString(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        case FieldObject.Types.Integer:
                            item[key.ToString()] = Convert.ToInt32(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        case FieldObject.Types.DateTime:
                            item[key.ToString()] = Convert.ToDateTime(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        case FieldObject.Types.Number:
                            item[key.ToString()] = Convert.ToDecimal(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        case FieldObject.Types.Boolean:
                            item[key.ToString()] = Convert.ToBoolean(field.Value);
                            item.UpdateOverwriteVersion();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "ChangeItemInList", "ClsHelper"));
                log.WriteToLog();
            }
        }

        #endregion

        #region Document Library

        public void UploadFileToLibrary(string destinationLibraryUrl, string filePathOnDisk)
        {
            if (string.IsNullOrEmpty(destinationLibraryUrl))
                return;

            if (string.IsNullOrEmpty(filePathOnDisk))
                return;

            FileStream stream = null;
            SPWeb website = null;
            try
            {
                stream = File.OpenRead(filePathOnDisk);
                var contents = new byte[stream.Length];
                stream.Read(contents, 0, (int)stream.Length);
                website = GetWebSite();
                website.Files.Add(destinationLibraryUrl, contents);
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "UploadFileToLibrary", "ClsHelper"));
                log.WriteToLog();
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (website != null)
                    website.Dispose();
            }
        }

        public void CreateFolder(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
                return;

            SPWeb website = null;
            try
            {
                website = GetWebSite();
                var library = (SPDocumentLibrary)website.Lists[ListName];
                SPFolderCollection folders = website.Folders;
                folders.Add(Utilities.RemoveSlashAtTheEnd(SiteUrl) + "/" + Utilities.RemoveSlashAtTheEnd(SiteName) + "/" + Utilities.RemoveSlashAtTheEnd(ListName) + "/" + Utilities.RemoveSlashAtTheEnd(folderName) + "/");
                library.Update();
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "CreateFolder", "ClsHelper"));
                log.WriteToLog();
            }
            finally
            {
                if (website != null) website.Dispose();
            }
        }

        #endregion
        #region Site Actions
        public void CreateSite(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
                return;
            
            SPWeb website = null;
            try
            {
                website = GetWebSite();
                string template = website.WebTemplate;
                website.Webs.Add(siteName, siteName, string.Empty, 0, template, false, false);
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "CreateSite", "ClsHelper"));
                log.WriteToLog();
            }
            finally
            {
                if (website != null)
                    website.Dispose();
            }
        }
        #endregion
        #region List Actions
        // in this case we're not using the spweb in the base class in fact we're passing it as a parameter
        public void CreateList(SPWeb currentWebSite)
        {
            SPWeb website = null;
        
            try
            {
                website = currentWebSite ?? GetWebSite();

                website.AllowUnsafeUpdates = true;
                
                SPListCollection lists = website.Lists;
                Guid idList = lists.Add(ListName, string.Empty, SPListTemplateType.GenericList);

                SPList list = lists.GetList(idList, false);
                list.NoCrawl = true;
                list.Update();

                website.AllowUnsafeUpdates = false;
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "CreateList", "ClsHelper"));
                log.WriteToLog();
            }
            finally
            {
                if (website != null)
                    website.Dispose();
            }
        }

        public void CreateList()
        {
            CreateList(null);
        }

        public void DeleteList()
        {
            SPWeb website = null;

            try
            {
                website = GetWebSite();
                SPListCollection lists = website.Lists;
                SPList list = lists[ListName];
                Guid listGuid = list.ID;
                lists.Delete(listGuid);
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "DeleteList", "ClsHelper"));
                log.WriteToLog();
            }
            finally
            {
                if (website != null)
                    website.Dispose();
            }
        }

        public void CreateListView(string viewName, string query, StringCollection collViewFields, uint rowLimit,
                                   bool paged, bool makeViewDefault, SPViewCollection.SPViewType viewType, bool personalView)
        {
            SPWeb website = null;
            try
            {
                website = GetWebSite();
                SPListCollection lists = website.Lists;
                SPList list = lists[ListName];
                
                list.Views.Add(viewName, collViewFields, query, rowLimit, paged, makeViewDefault, viewType, personalView);
                
                SPView view = list.Views[viewName];
                
                view.Update();
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "CreateListView", "ClsHelper"));
                log.WriteToLog();
            }
            finally
            {
                if (website != null)
                    website.Dispose();
            }
        }

        public bool IsListExist()
        {
            SPWeb website = null;
            try
            {
                website = GetWebSite();

                if (website.Lists.Cast<SPList>().Any(list => list.Title == ListName))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "IsListExist", "ClsHelper"));
                log.WriteToLog();
            }
            finally
            {
                if (website != null)
                    website.Dispose();
            }

            return false;
        }

        public bool IsViewExist(string view)
        {
            try
            {
                SPList list = GetList();

                if (list.Views.Cast<SPView>().Any(listView => listView.ToString() == view))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "IsViewExist", "ClsHelper"));
                log.WriteToLog();
            }
            
            return false;
        }

        #endregion

        #region Field Actions
        // in this case we're not using the spweb in the base class in fact we're passing it as a parameter
        public void AddFields(Hashtable fields, SPWeb currentWebSite)
        {
            if (!IsHashtableValid(fields))
                return;
            FieldObject field;
            try
            {
                SPList list = GetList();
                SPFieldCollection fieldCollection = list.Fields;
                ICollection keys = fields.Keys;
                
                foreach (object key in keys)
                {
                    field = (FieldObject)fields[key];
                    switch (field.ValueType)
                    {
                        case FieldObject.Types.AllDayEvent:
                            fieldCollection.Add(key.ToString(), SPFieldType.AllDayEvent, field.IsRequired);
                            break;
                        case FieldObject.Types.Attachments:
                            fieldCollection.Add(key.ToString(), SPFieldType.Attachments, field.IsRequired);
                            break;
                        case FieldObject.Types.Boolean:
                            fieldCollection.Add(key.ToString(), SPFieldType.Boolean, field.IsRequired);
                            break;
                        case FieldObject.Types.Calculated:
                            fieldCollection.Add(key.ToString(), SPFieldType.Calculated, field.IsRequired);
                            break;
                        case FieldObject.Types.Choice:
                            fieldCollection.Add(key.ToString(), SPFieldType.Choice, field.IsRequired);
                            break;
                        case FieldObject.Types.Computed:
                            fieldCollection.Add(key.ToString(), SPFieldType.Computed, field.IsRequired);
                            break;
                        case FieldObject.Types.ContentTypeId:
                            fieldCollection.Add(key.ToString(), SPFieldType.ContentTypeId, field.IsRequired);
                            break;
                        case FieldObject.Types.Counter:
                            fieldCollection.Add(key.ToString(), SPFieldType.Counter, field.IsRequired);
                            break;
                        case FieldObject.Types.CrossProjectLink:
                            fieldCollection.Add(key.ToString(), SPFieldType.CrossProjectLink, field.IsRequired);
                            break;
                        case FieldObject.Types.Currency:
                            fieldCollection.Add(key.ToString(), SPFieldType.Currency, field.IsRequired);
                            break;
                        case FieldObject.Types.DateTime:
                            fieldCollection.Add(key.ToString(), SPFieldType.DateTime, field.IsRequired);
                            break;
                        case FieldObject.Types.Error:
                            fieldCollection.Add(key.ToString(), SPFieldType.Error, field.IsRequired);
                            break;
                        case FieldObject.Types.File:
                            fieldCollection.Add(key.ToString(), SPFieldType.File, field.IsRequired);
                            break;
                        case FieldObject.Types.GridChoice:
                            fieldCollection.Add(key.ToString(), SPFieldType.GridChoice, field.IsRequired);
                            break;
                        case FieldObject.Types.Guid:
                            fieldCollection.Add(key.ToString(), SPFieldType.Guid, field.IsRequired);
                            break;
                        case FieldObject.Types.Integer:
                            fieldCollection.Add(key.ToString(), SPFieldType.Integer, field.IsRequired);
                            break;
                        case FieldObject.Types.Invalid:
                            fieldCollection.Add(key.ToString(), SPFieldType.Invalid, field.IsRequired);
                            break;
                        case FieldObject.Types.Lookup:
                            fieldCollection.Add(key.ToString(), SPFieldType.Lookup, field.IsRequired);
                            break;
                        case FieldObject.Types.MaxItems:
                            fieldCollection.Add(key.ToString(), SPFieldType.MaxItems, field.IsRequired);
                            break;
                        case FieldObject.Types.ModStat:
                            fieldCollection.Add(key.ToString(), SPFieldType.ModStat, field.IsRequired);
                            break;
                        case FieldObject.Types.MultiChoice:
                            fieldCollection.Add(key.ToString(), SPFieldType.MultiChoice, field.IsRequired);
                            break;
                        case FieldObject.Types.Note:
                            fieldCollection.Add(key.ToString(), SPFieldType.Note, field.IsRequired);
                            break;
                        case FieldObject.Types.Number:
                            fieldCollection.Add(key.ToString(), SPFieldType.Number, field.IsRequired);
                            break;
                        case FieldObject.Types.PageSeparator:
                            fieldCollection.Add(key.ToString(), SPFieldType.PageSeparator, field.IsRequired);
                            break;
                        case FieldObject.Types.Recurrence:
                            fieldCollection.Add(key.ToString(), SPFieldType.Recurrence, field.IsRequired);
                            break;
                        case FieldObject.Types.Text:
                            fieldCollection.Add(key.ToString(), SPFieldType.Text, field.IsRequired);
                            break;
                        case FieldObject.Types.ThreadIndex:
                            fieldCollection.Add(key.ToString(), SPFieldType.ThreadIndex, field.IsRequired);
                            break;
                        case FieldObject.Types.Threading:
                            fieldCollection.Add(key.ToString(), SPFieldType.Threading, field.IsRequired);
                            break;
                        case FieldObject.Types.Url:
                            fieldCollection.Add(key.ToString(), SPFieldType.URL, field.IsRequired);
                            break;
                        case FieldObject.Types.User:
                            fieldCollection.Add(key.ToString(), SPFieldType.User, field.IsRequired);
                            break;
                        case FieldObject.Types.WorkflowEventType:
                            fieldCollection.Add(key.ToString(), SPFieldType.WorkflowEventType, field.IsRequired);
                            break;
                        case FieldObject.Types.WorkflowStatus:
                            fieldCollection.Add(key.ToString(), SPFieldType.WorkflowStatus, field.IsRequired);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "AddFields", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void AddFields(Hashtable fields)
        {
            AddFields(fields, null);
        }
        #endregion

        #region Farm User Methods
        public long GetFarmUsersCount()
        {
            long count = 0;

            try
            {
                UserProfileManager profileManager = GetProfileManager();
                if (profileManager != null)
                {
                    count = profileManager.Count;
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetUsersCount", "ClsHelper"));
                log.WriteToLog();
            }

            return count;
        }

        public void CreateFarmUser(string userAccount)
        {
            if (string.IsNullOrEmpty(userAccount))
                return;
            try
            {
                UserProfileManager profileManager = GetProfileManager();
                
                if (!profileManager.UserExists(userAccount))
                   profileManager.CreateUserProfile(userAccount);
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "CreateUser", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void CreateFarmUsers(params string[] usersAccount)
        {
            if (usersAccount == null)
                return;

            foreach (string userAccount in usersAccount)
                CreateFarmUser(userAccount);
        }

        public void DeleteFarmUser(string userAccount)
        {
            if (string.IsNullOrEmpty(userAccount))
                return;
            try
            {
                UserProfileManager profileManager = GetProfileManager();
                if (profileManager.UserExists(userAccount))
                    profileManager.RemoveUserProfile(userAccount);
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "DeleteUser", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void DeleteFarmUsers(params string[] usersAccount)
        {
            if (usersAccount == null)
                return;

            foreach (string userAccount in usersAccount)
                DeleteFarmUser(userAccount);
        }

        public UserProfile GetFarmUserProfile(string userAccount)
        {
            if (string.IsNullOrEmpty(userAccount))
                return null;
            UserProfile user = null;
            try
            {
                UserProfileManager profileManager = GetProfileManager();
                
                if (profileManager.UserExists(userAccount))
                {
                    user = profileManager.GetUserProfile(userAccount);
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetUserProfile", "ClsHelper"));
                log.WriteToLog();
            }

            return user;
        }

        public ArrayList GetFarmUsersProfile(params string[] usersAccount)
        {
            var usersProfile = new ArrayList();
            if (usersAccount == null)
                return null;
            foreach (string userAccount in usersAccount)
            {
                usersProfile.Add(GetFarmUserProfile(userAccount));
            }

            return usersProfile;
        }
        
        #endregion

        #region User Methods

        public SPUser GetUserObject(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return null;

            SPUser user = null;

            try
            {
                using (SPWeb website = GetWebSite())
                {
                    user = website.AllUsers[userName];
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetUserObject", "ClsHelper"));
                log.WriteToLog();
            }

            return user;
        }

        public SPUser InsertUser(UserObject user)
        {
            if (user == null)
                return null;
            if (string.IsNullOrEmpty(user.UserName))
                return null;

            SPUser sharepointUser = null;

            try
            {
                using (SPWeb varWebSite = GetWebSite())
                {
                    SPUserCollection users = varWebSite.SiteUsers;
                    users.Add(user.UserName, user.Email, user.Name, user.Notes);
                    AssignPermission(user.Role, string.Empty, user.UserName);
                    sharepointUser = varWebSite.AllUsers[user.UserName];
                    varWebSite.Update();
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "InsertUser", "ClsHelper"));
                log.WriteToLog();
            }

            return sharepointUser;
        }

        public void DeleteUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;
            try
            {
                SPUser user = GetUserObject(username);
                if (user == null)
                    return;
                using (SPWeb website = GetWebSite())
                {
                    SPUserCollection varUsers = website.SiteUsers;
                    varUsers.Remove(username);
                    website.Update();
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "DeleteUser", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void AddUserToGroup(UserObject user)
        {
            if (user == null)
                return;
            if (string.IsNullOrEmpty(user.UserName))
                return;
            if (string.IsNullOrEmpty(user.GroupName))
                return;

            try
            {
                SPUser sharepointUser = GetUserObject(user.UserName) ?? InsertUser(user);

                SPGroup sharepointGroup = GetGroupObject(user.GroupName);
                if (sharepointGroup == null)
                    return;

                sharepointGroup.AddUser(sharepointUser);
                sharepointGroup.Update();
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "AddUserToGroup", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void RemoveUserFromGroup(UserObject user)
        {
            if (user == null)
                return;
            if (string.IsNullOrEmpty(user.UserName))
                return;
            if (string.IsNullOrEmpty(user.GroupName))
                return;
            try
            {
                SPUser sharepointUser = GetUserObject(user.UserName);
                if (sharepointUser == null)
                    return;
                SPGroup sharepointGroup = GetGroupObject(user.GroupName);
                if (sharepointGroup == null)
                    return;

                sharepointGroup.RemoveUser(sharepointUser);
                sharepointGroup.Update();
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "RemoveUserFromGroup", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void UpdateUser(UserObject user)
        {
            if (user == null)
                return;
            if (string.IsNullOrEmpty(user.UserName))
                return;
            try
            {
                SPUser sharepointUser = GetUserObject(user.UserName);
                if (sharepointUser == null)
                    return;
                if (user.Email != string.Empty)
                    sharepointUser.Email = user.Email;
                if (user.Name != string.Empty)
                    sharepointUser.Name = user.Name;
                if (user.Notes != string.Empty)
                    sharepointUser.Notes = user.Notes;
                sharepointUser.Update();
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "UpdateUser", "ClsHelper"));
                log.WriteToLog();
            }
        }

        #endregion

        #region Group Methods
        public SPGroup GetGroupObject(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return null;

            SPGroup sharepointGroup = null;

            try
            {
                using (SPWeb website = GetWebSite())
                {
                    sharepointGroup = website.SiteGroups[groupName];
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "GetGroupObject", "ClsHelper"));
                log.WriteToLog();
            }

            return sharepointGroup;
        }

        public void AddGroup(string groupName, string owner, string defaultUser, string description,
                             UserObject.RoleDefinition role)
        {
            if (string.IsNullOrEmpty(groupName))
                return;
            if (string.IsNullOrEmpty(owner))
                return;
            if (string.IsNullOrEmpty(defaultUser))
                return;
            try
            {
                using (SPWeb website = GetWebSite())
                {
                    SPUser sharepointOwner = GetUserObject(owner);
                    if (sharepointOwner == null)
                        return;
                    SPUser sharepointDefaultUser = GetUserObject(defaultUser);
                    if (sharepointDefaultUser == null)
                        return;
                    SPGroupCollection sharepointGroups = website.SiteGroups;
                    sharepointGroups.Add(groupName, sharepointOwner, sharepointDefaultUser, description);
                    AssignPermission(role, groupName, string.Empty);
                    
                    website.Update();
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "AddGroup", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void DeleteGroup(string groupName)
        {
            try
            {
                using (SPWeb website = GetWebSite())
                {
                    SPGroupCollection sharepointGroups = website.SiteGroups;
                    sharepointGroups.Remove(groupName);
                    website.Update();
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "aGroupName", "ClsHelper"));
                log.WriteToLog();
            }
        }

        public void ChangeGroupName(string oldGroupName, string newGroupName)
        {
            if (string.IsNullOrEmpty(oldGroupName))
                return;
            if (string.IsNullOrEmpty(newGroupName))
                return;

            try
            {
                SPGroup sharepointGroup = GetGroupObject(oldGroupName);
                if (sharepointGroup == null)
                    return;
                sharepointGroup.Name = newGroupName;
                sharepointGroup.Update();
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "aGroupName", "ClsHelper"));
                log.WriteToLog();
            }
        }

        #endregion

        #region General Methods
        private static bool IsHashtableValid(Hashtable hashTable)
        {
            if (hashTable == null)
                return false;
            if (hashTable.Count == 0)
                return false;

            return true;
        }
       
        #endregion

        private void AssignPermission(UserObject.RoleDefinition role, string groupName, string userName)
        {
            try
            {
                using (SPWeb website = GetWebSite())
                {
                    SPRoleAssignment roleAssignment = null;

                    if (string.IsNullOrEmpty(groupName))
                        roleAssignment = new SPRoleAssignment(website.SiteUsers[userName]);
                    if (string.IsNullOrEmpty(userName))
                        roleAssignment = new SPRoleAssignment(website.SiteGroups[groupName]);

                    SPRoleDefinition roleDefinition = null;
                    switch (role)
                    {
                        case UserObject.RoleDefinition.FullControl:
                            roleDefinition = website.RoleDefinitions["Full Control"];
                            break;
                        case UserObject.RoleDefinition.Design:
                            roleDefinition = website.RoleDefinitions["Design"];
                            break;
                        case UserObject.RoleDefinition.ManageHierarchy:
                            roleDefinition = website.RoleDefinitions["Manage Hierarchy"];
                            break;
                        case UserObject.RoleDefinition.Approve:
                            roleDefinition = website.RoleDefinitions["Approve"];
                            break;
                        case UserObject.RoleDefinition.Contribute:
                            roleDefinition = website.RoleDefinitions["Contribute"];
                            break;
                        case UserObject.RoleDefinition.Read:
                            roleDefinition = website.RoleDefinitions["Read"];
                            break;
                        case UserObject.RoleDefinition.RestrictedRead:
                            roleDefinition = website.RoleDefinitions["Restricted Read"];
                            break;
                        case UserObject.RoleDefinition.LimitedAccess:
                            roleDefinition = website.RoleDefinitions["Limited Access"];
                            break;
                        case UserObject.RoleDefinition.ViewOnly:
                            roleDefinition = website.RoleDefinitions["View Only"];
                            break;
                        case UserObject.RoleDefinition.RecordsCenterSubmissionCompletion:
                            roleDefinition = website.RoleDefinitions["Records Center Submission Completion"];
                            break;
                        default:
                            break;
                    }

                    if (roleAssignment != null)
                    {
                        roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                        website.RoleAssignments.Add(roleAssignment);
                        website.Update();
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new AppEventLog(AppException.ExceptionMessage(ex, "AssignPermission", "ClsHelper"));
                log.WriteToLog();
            }
        }

        #endregion
    }
}