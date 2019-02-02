// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SoftissimoTranslation.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SoftissimoTranslation type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.AutomaticTranslation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using System.Collections;
    using System.Web.Caching;
    using System.Linq;

    using Alphamosaik.Oceanik.AutomaticTranslation.com.reverso;
    using Alphamosaik.Oceanik.Sdk;

    using Microsoft.SharePoint;
    using Translator.Common.Library;
    using System.Text;
    using System.Diagnostics;
    
    public class SoftissimoTranslation : IAutomaticTranslation
    {
        //private readonly string _webservicePath;

        //private readonly string _userName;

        //private readonly string _password;

        //public SoftissimoTranslation(string webservicePath, string userName, string password)
        //{
        //    _webservicePath = webservicePath;
        //    _userName = userName;
        //    _password = password;
        //}

        #region Implementation of IAutomaticTranslation

        public string TranslateText(string toTranslate, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, bool isHtml)
        {
            string toTranslateOriginal = toTranslate;

            try
            {                
                sourceLang = ConvertLanguageCode2LettersTo3Letters(sourceLang);
                destinationLang = ConvertLanguageCode2LettersTo3Letters(destinationLang);

                if (string.IsNullOrEmpty(profileId))
                    profileId = "General";

                using (Translator webService = GetWebService(translationUserAccount.Login, translationUserAccount.Password, translationUserAccount.Url))
                {
                    DirectionInfo directionInfo = GetDirectionInfo(webService, sourceLang, destinationLang);

                    bool truncated;
                    long wordsLeft;

                    if (isHtml)
                    {

                        byte[] encbuff = Encoding.UTF8.GetBytes(toTranslate);
                        toTranslate = Convert.ToBase64String(encbuff);

                        string result = webService.TranslateHTML(toTranslate, directionInfo, GetTemplateInfo(profileId, directionInfo), string.Empty, false, null, "utf-8", out truncated, out wordsLeft);

                        byte[] decodedBytes = Convert.FromBase64String(result);
                        result = Encoding.UTF8.GetString(decodedBytes);

                        return result;
                    }
                    else
                    {
                        return webService.TranslateText(toTranslate, directionInfo, GetTemplateInfo(profileId, directionInfo), null, out truncated, out wordsLeft);
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("TranslateText", e, EventLogEntryType.Warning);
                return toTranslateOriginal;
            }
        }
        
        public byte[] TranslateFile(byte[] buffer, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, string fileName)
        {
            byte[] bufferOriginal = buffer;

            try
            {
                using (Translator webService = GetWebService(translationUserAccount.Login, translationUserAccount.Password, translationUserAccount.Url))
                {
                    sourceLang = ConvertLanguageCode2LettersTo3Letters(sourceLang);
                    destinationLang = ConvertLanguageCode2LettersTo3Letters(destinationLang);

                    DirectionInfo directionInfo = GetDirectionInfo(webService, sourceLang, destinationLang);

                    string outputFileExtension;
                    int translationsLeft;
                    string inputFileExtension = GetFileExtension(fileName);

                    byte[] result = webService.TranslateStream(buffer, inputFileExtension, directionInfo, GetTemplateInfo(profileId, directionInfo), null, out translationsLeft, out outputFileExtension);
                    
                    return result;                
                }
            }
            catch (Exception e)
            {
                Utilities.LogException("TranslateFile", e, EventLogEntryType.Warning);
                return bufferOriginal;
            }
        }

        public bool SupportFileTranslation()
        {
            return true;
        }

        public TranslationUserAccount LoadUserAccount(SPWeb spWeb, string sourceLanguage)
        {            
            string login = string.Empty;
            string password = string.Empty;
            string url = string.Empty;
            List<string> userProfileList = new List<string>();
            TranslationUserAccount translationUserAccount = new TranslationUserAccount();
            SPUser currentUser = spWeb.CurrentUser;

            string lang = string.Empty;
            if (string.IsNullOrEmpty(sourceLanguage))
                if (HttpContext.Current != null)
                    lang = Utilities.GetLanguageCode(HttpContext.Current);
                else
                    lang = sourceLanguage;

            if (HttpRuntime.Cache.Get("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + lang) == null)
            {
                try
                {
                    SPUser rootSiteCurrentUser = spWeb.SiteUsers.GetByID(currentUser.ID);
                    string systranAccount = string.Empty;

                    //if (HttpRuntime.Cache.Get("OceanikTranslationAccount " + currentUser.LoginName) == null)
                    //{
                    if (spWeb.Lists["Translation Accounts"] != null)
                    {
                        SPList accountsSystranList = spWeb.Lists["Translation Accounts"];

                        if (HttpRuntime.Cache.Get("OceanikTranslationAccountId " + spWeb.ID.ToString() + currentUser.LoginName) == null)
                        {
                            foreach (SPItem account in accountsSystranList.Items)
                            {
                                if (account["Groups"] != null)
                                {
                                    var o = account["Groups"];

                                    SPFieldUserValueCollection groupsForThisAccount = (SPFieldUserValueCollection)account["Groups"];

                                    foreach (SPFieldUserValue groupForThisAccount in groupsForThisAccount)
                                    {
                                        try
                                        {
                                            foreach (SPGroup groupForTheRootSiteCurrentUser in rootSiteCurrentUser.Groups)
                                            {
                                                if (groupForTheRootSiteCurrentUser.Name == groupForThisAccount.LookupValue)
                                                {
                                                    if (accountsSystranList.Fields.ContainsField("Account"))
                                                    {
                                                        systranAccount = account["Account"].ToString();
                                                        HttpRuntime.Cache.Remove("OceanikTranslationAccountId " + spWeb.ID.ToString() + currentUser.LoginName);
                                                        HttpRuntime.Cache.Add("OceanikTranslationAccountId " + spWeb.ID.ToString() + currentUser.LoginName, account.ID, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, null);



                                                        //if (accountsSystranList.Fields.ContainsField("Profiles"))
                                                        //{
                                                        //    string profilesFieldValue = account["Profiles"].ToString();
                                                        //    if (profilesFieldValue != string.Empty)
                                                        //    {
                                                        //        Regex lookupRegex = new Regex("[0-9]+;#(?<value>([^;]+))");
                                                        //        foreach (Match profileName in lookupRegex.Matches(profilesFieldValue))
                                                        //        {
                                                        //            userProfileArrayList.Add(profileName.Groups["value"].Value);
                                                        //        }
                                                        //    }
                                                        //}
                                                    }

                                                    break;
                                                }
                                            }

                                            if (userProfileList.Count > 0)
                                                break;
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    if (userProfileList.Count > 0)
                                        break;
                                }
                            }
                        }

                        if (accountsSystranList.Fields.ContainsField("Account") && accountsSystranList.Fields.ContainsField("Password") && accountsSystranList.Fields.ContainsField("Url"))
                        {
                            string accountId = Convert.ToString(HttpRuntime.Cache.Get("OceanikTranslationAccountId " + spWeb.ID.ToString() + currentUser.LoginName));
                            SPListItem accountItem = accountsSystranList.Items.Cast<SPListItem>().First(sPListItemTmp => sPListItemTmp.ID.ToString() == accountId);

                            if (accountItem["Account"] != null && accountItem["Password"] != null && accountItem["Url"] != null)
                            {
                                login = accountItem["Account"].ToString();
                                password = EncryptionUtility.Encrypt(accountItem["Password"].ToString());
                                url = accountItem["Url"].ToString();

                                string languageItemSource = string.Empty;

                                if (string.IsNullOrEmpty(sourceLanguage))
                                    if (HttpContext.Current != null)
                                        languageItemSource = Utilities.GetLanguageCode(HttpContext.Current);
                                    else
                                        languageItemSource = sourceLanguage;

                                //foreach (LanguageItem languageItemSource in Dictionaries.Instance.VisibleLanguages)
                                {
                                    foreach (LanguageItem languageItemDestination in Dictionaries.Instance.VisibleLanguages)
                                    {
                                        if (languageItemSource != languageItemDestination.LanguageDestination)
                                        {
                                            List<string> userProfileListTmp = GetTemplates(ConvertLanguageCode2LettersTo3Letters(languageItemSource),
                                                ConvertLanguageCode2LettersTo3Letters(languageItemDestination.LanguageDestination), url, login, password);

                                            foreach (string languageItemName in userProfileListTmp)
                                            {
                                                if (!userProfileList.Contains(languageItemName))
                                                {
                                                    userProfileList.Add(languageItemName);
                                                }
                                            }
                                        }
                                    }
                                }

                                //userProfileList = GetTemplates("eng", "fra", url, login, password);                            
                            }
                        }
                    }
                    //}                
                }
                catch
                { }

                translationUserAccount.Login = login;
                translationUserAccount.Password = password;
                translationUserAccount.Url = url;
                translationUserAccount.Profiles = userProfileList;

                HttpRuntime.Cache.Remove("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + lang);
                HttpRuntime.Cache.Add("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + lang, translationUserAccount, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, null);
            }
            else
            {
                translationUserAccount = (TranslationUserAccount)(HttpRuntime.Cache.Get("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + lang));
            }
            
            return translationUserAccount;            
        }

        #endregion

        public List<string> GetTemplates(string source3LettersLang, string destination3LettersLang, string url, string login, string password)
        {
            var templates = new List<string>();

            using (Translator webService = GetWebService(login, password, url))//(this._userName, this._password, this._webservicePath))
            {
                DirectionInfo directionInfo = GetDirectionInfo(webService, source3LettersLang, destination3LettersLang);

                if (directionInfo != null)
                {
                    foreach (TemplateInfo templateInfo in directionInfo.extraInfo.templates)
                    {
                        templates.Add(templateInfo.template);
                    }
                }
            }

            return templates;
        }

        private static TemplateInfo GetTemplateInfo(string templateName, DirectionInfo directionInfo)
        {
            foreach (TemplateInfo templateInfo in directionInfo.extraInfo.templates)
            {
                if (templateName == templateInfo.template)
                    return templateInfo;                
            }

            return null;
        }

        private static DirectionInfo GetDirectionInfo(Translator webService, string source3LettersLang, string destination3LettersLang)
        {
            DirectionInfo[] allTranslationDirections = webService.GetAllTranslationDirections();

            if (allTranslationDirections != null && allTranslationDirections.Length > 0)
            {
                foreach (DirectionInfo currentDirectionInfo in allTranslationDirections)
                {
                    if (currentDirectionInfo.srcLanguageCode.Equals(source3LettersLang, StringComparison.OrdinalIgnoreCase) &&
                        currentDirectionInfo.destLanguageCode.Equals(destination3LettersLang, StringComparison.OrdinalIgnoreCase))
                    {
                        return currentDirectionInfo;
                    }
                }
            }

            return null;
        }

        private static Translator GetWebService(string userName, string password, string webservicePath)
        {
            try
            {
                DateTime date = DateTime.Now.ToUniversalTime();

                var baseHeader = new BaseHeader { Created = date.ToString(CultureInfo.InvariantCulture), Username = userName };

                baseHeader.Signature = EncryptionUtility.ComputeHash(
                    string.Concat(userName, baseHeader.Created),
                    EncryptionUtility.Decrypt(password), System.Text.Encoding.ASCII);

                return new Translator { Url = webservicePath, Timeout = 360000, BaseHeaderValue = baseHeader };
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex.Message);
            }

            return null;
        }

        private static string ConvertLanguageCode2LettersTo3Letters(string languageCode2Letters)
        {
            CultureInfo ci = CultureInfo.GetCultureInfo(languageCode2Letters); // CultureInfo.CreateSpecificCulture(languageCode2Letters);
            return ci.ThreeLetterISOLanguageName;
        }

        private static string GetFileExtension(string fileName)
        {
            string fileExtension = string.Empty;
            string[] fileNameParts = fileName.Split('.');
            if (fileNameParts.Length > 0)
            {
                fileExtension = "." + fileNameParts[fileNameParts.Length - 1];
            }

            return fileExtension;
        }
    }
}