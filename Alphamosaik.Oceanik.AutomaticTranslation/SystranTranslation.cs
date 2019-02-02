// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystranTranslation.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SystranTranslation type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using Alphamosaik.Oceanik.Sdk;
using Translator.Common.Library;
using Microsoft.SharePoint;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Linq;
using System.Collections.Generic;

namespace Alphamosaik.Oceanik.AutomaticTranslation
{
    public class SystranTranslation : IAutomaticTranslation
    {
        private static readonly SystranTranslationWebService Tws = new SystranTranslationWebService();

        #region Implementation of IAutomaticTranslation

        public string TranslateText(string toTranslate, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, bool isHtml)
        {
            string username = string.Empty;
            string password = string.Empty;

            sourceLang = sourceLang.Replace("CH", "zh");
            destinationLang = destinationLang.Replace("CH", "zh");
            sourceLang = sourceLang.Replace("JP", "ja");
            destinationLang = destinationLang.Replace("JP", "ja");
            sourceLang = sourceLang.Replace("DU", "nl");
            destinationLang = destinationLang.Replace("DU", "nl");

            if (translationUserAccount != null)
            {
                username = translationUserAccount.Login;
                password = translationUserAccount.Password;
                Tws.Url = translationUserAccount.Url;
            }

            //Tws.Url = "http://demov7.systran.fr/ws";

            // promt user for language pair and input text
            string lp = sourceLang.ToLower() + "_" + destinationLang.ToLower();
            string text = toTranslate;
            string gui = string.Empty;

            int optionCount = 7;

            if (!string.IsNullOrEmpty(profileId))
                optionCount++;

            // create request object and set options
            var request = new Request { option = new Option[optionCount] };
            request.option[0] = new Option { name = "lp", value = lp };
            request.option[1] = new Option { name = "input_text", value = text };
            request.option[2] = new Option { name = "translate_source", value = "string" };
            request.option[3] = new Option { name = "nfw_marker", value = string.Empty };
            request.option[4] = new Option { name = "gui", value = gui };
            request.option[5] = new Option { name = "login.username", value = username };
            request.option[6] = new Option { name = "login.password", value = password };

            if (!string.IsNullOrEmpty(profileId))
            {
                request.option[7] = new Option { name = "profile", value = profileId };
            }

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
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
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

        public byte[] TranslateFile(byte[] buffer, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, string fileName)
        {
            string username = string.Empty;
            string password = string.Empty;
            string gui = string.Empty;            
            string fileData;

            if (translationUserAccount != null)
            {
                username = translationUserAccount.Login;
                password = translationUserAccount.Password;
                Tws.Url = translationUserAccount.Url;
            }

            sourceLang = sourceLang.Replace("CH", "zh");
            destinationLang = destinationLang.Replace("CH", "zh");
            sourceLang = sourceLang.Replace("JP", "ja");
            destinationLang = destinationLang.Replace("JP", "ja");
            sourceLang = sourceLang.Replace("DU", "nl");
            destinationLang = destinationLang.Replace("DU", "nl");

            //Tws.Url = "http://demov7.systran.fr/ws";

            string lp = sourceLang.ToLower() + "_" + destinationLang.ToLower();

            // read the file and encode as a base64 string
            try
            {
                fileData = Convert.ToBase64String(buffer);
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in Translator2009.TranslatorAutoTranslation.CallFileTranslationSystranWebService: " + e.Message, EventLogEntryType.Warning);
                return buffer;
            }

            // create request object and set options
            var request = new Request();
            int optionCount = 5;
            int optExtra = optionCount;

            if (gui.Length != 0)
                optionCount++;
            if (!string.IsNullOrEmpty(profileId))
                optionCount++;

            request.option = new Option[optionCount];

            request.option[0] = new Option { name = "login.username", value = username };
            request.option[1] = new Option { name = "login.password", value = password };
            request.option[2] = new Option { name = "lp", value = lp };
            request.option[3] = new Option { name = "translate_source", value = "file_data" };
            request.option[4] = new Option { name = "input_file_data", value = fileData };

            if (gui.Length != 0)
            {
                request.option[optExtra] = new Option { name = "gui", value = gui };
                optExtra++;
            }

            if (!string.IsNullOrEmpty(profileId))
            {
                request.option[optExtra] = new Option { name = "profile", value = profileId };
            }

            // call the web service
            string status = null; // status indicates the percent completion of the request
            string requestid = null; // requestid uniquely identifies the request
            Response response = Tws.Translate(request);
            foreach (Option option in response.option)
            {
                if (option.name == "status")
                    status = option.value;
                else if (option.name == "requestid")
                    requestid = option.value;
            }

            // wait for the request to complete
            while (status == null || status != "100")
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
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
            if ((string)results["error"] == null)
            {
                // if the error option is set, an error occured            
                try
                {
                    buffer = Convert.FromBase64String((string)results["output_file_data"]);
                }
                catch (Exception e)
                {
                    Utilities.LogException("Error in Translator2009.TranslatorAutoTranslation.CallFileTranslationSystranWebService: " + e.Message, EventLogEntryType.Warning);
                    return buffer;
                }
            }

            return buffer;
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

            if (HttpRuntime.Cache.Get("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + sourceLanguage) == null)
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
                                    password = accountItem["Password"].ToString();
                                    url = accountItem["Url"].ToString();

                                    userProfileList = GetProfilesList(url, login, password);
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

                HttpRuntime.Cache.Remove("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + sourceLanguage);
                HttpRuntime.Cache.Add("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + sourceLanguage, translationUserAccount, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, null);
            }
            else
            {
                translationUserAccount = (TranslationUserAccount)(HttpRuntime.Cache.Get("OceanikTranslationAccount " + spWeb.ID.ToString() + currentUser.LoginName + "-" + sourceLanguage));
            }

            return translationUserAccount;
        }

        private List<string> GetProfilesList(string url, string username, string password)
        {            
            List<string> profilesList = new List<string>();
            string resultProfilesString = string.Empty;
            RequestInfo request = new RequestInfo();
            request.option = new Option[2];

            request.option[0] = new Option();
            request.option[0].name = "login.username";
            request.option[0].value = username;
            request.option[1] = new Option();
            request.option[1].name = "login.password";
            request.option[1].value = password;

            request.infotype = "server";
            try
            {
                Tws.Url = url;

                // call the web service
                Response response = Tws.GetInfo(request);
                                
                // display results
                foreach (Option option in response.option)
                {
                    if (option.name == "profiles")
                    {
                        resultProfilesString = option.value;
                        //profilesList.Add(option.value);
                        break;
                    }
                }
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                Utilities.LogException("Server Error - Translation web service: " + e.Message, EventLogEntryType.Warning);
            }
            catch (System.Net.WebException e)
            {
                Utilities.LogException("Network Error - Translation web service: " + e.Message, EventLogEntryType.Warning);
            }

            string[] profilesArray = resultProfilesString.Split('|');

            for (int i = 0; i < profilesArray.Count(); i++)
            {
                string[] profileIdAndName = profilesArray[i].Split(':');
                if (profilesArray.Count() > 1)
                {
                    profilesList.Add(profileIdAndName[0]);
                }
            }

            return profilesList;
        }

        #endregion
    }
}