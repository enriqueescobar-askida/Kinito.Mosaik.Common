// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpApplicationEvents.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the HttpApplicationEvents type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Web;
using Alphamosaik.Common.Library.Licensing;
using Alphamosaik.Oceanik.AutomaticTranslation;
using Alphamosaik.Oceanik.Sdk;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Translator.Common.Library;
using TranslatorHttpHandler.Dictionary;
using TranslatorHttpHandler.ResponseFilters;
using TranslatorHttpHandler.TranslatorHelper;

namespace TranslatorHttpHandler.ApplicationEvents
{
    using System.Web.Caching;

    public class HttpApplicationEvents : IHttpApplicationEvents
    {
        private static readonly object LockThis = new object();
        private const string ContextKey = "AlphaMosaik_TranslatorModule_InplaceView";
        private const string OceanikITranslator = "Oceanik.ITranslator";
        private readonly License _license;
        private readonly IAutomaticTranslation _automaticTranslationPlugin;
        private readonly TranslatorHelper.TranslatorHelper _translatorHelper;
        private readonly DebugTranslatorHelper _debugTranslatorHelper;
        private TranslatorHelper.TranslatorHelper _currentTranslatorHelperToUse;

        public HttpApplicationEvents(TranslatorHelper.TranslatorHelper translatorHelper, License license, IAutomaticTranslation automaticTranslationPlugin)
        {
            _license = license;
            _automaticTranslationPlugin = automaticTranslationPlugin;
            _translatorHelper = translatorHelper;
            _debugTranslatorHelper = new DebugTranslatorHelper(_translatorHelper);
            _currentTranslatorHelperToUse = _translatorHelper;
        }

        public void ContextBeginRequest(object sender, EventArgs e)
        {
            try
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // Store the time of request beginning
                if (context == null)
                {
                    return;
                }

                HttpContext.Current.Items[OceanikITranslator] = _currentTranslatorHelperToUse;

                if (!HttpContext.Current.Request.Path.EndsWith("/") && HttpContext.Current.Request.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                {
                    string resultDebugHtml = HttpContext.Current.Request.QueryString["SPS_Debug_Html"];

                    if (!string.IsNullOrEmpty(resultDebugHtml))
                    {
                        _currentTranslatorHelperToUse = resultDebugHtml.Equals("true", StringComparison.OrdinalIgnoreCase) ? _debugTranslatorHelper : _translatorHelper;
                    }

                    if (HttpContext.Current.Request.Path.Contains("/_layouts/inplview.aspx") && !HttpContext.Current.Items.Contains(ContextKey))
                    {
                        string languageCode = Utilities.GetLanguageCode(context);
                        string currentSiteUrl = context.Request.Url.AbsoluteUri.Replace(context.Request.Url.PathAndQuery, string.Empty);

                        HttpResponse response = HttpContext.Current.Response;
                        response.Filter = new ResponseFilterInplaceViewStream(response.Filter, _currentTranslatorHelperToUse, languageCode, currentSiteUrl);
                        HttpContext.Current.Items.Add(ContextKey, new object());

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException("Error in Oceanik.HttpApplicationEvents.Context_BeginRequest: " + ex.Message, EventLogEntryType.Warning);
            }
        }

        public void ContextEndRequest(object sender, EventArgs e)
        {
            // Create HttpApplication, HttpContext and HttpRespoonse objects to access request and response properties.
            var app = (HttpApplication)sender;
            var context = app.Context;

            if (context == null)
                return;

            HttpContext.Current.Items.Remove(ContextKey);
            HttpContext.Current.Items.Remove(OceanikITranslator);
        }

        public void ContextReleaseRequestState(object sender, EventArgs e)
        {
            try
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // When we use an adminPagesToTranslate.txt file
                if (context.Request.Path.StartsWith("/_") && !context.Request.Url.AbsolutePath.EndsWith(".ashx", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Url.AbsolutePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Url.AbsolutePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Url.AbsolutePath.EndsWith(".css", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Url.AbsolutePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Url.AbsolutePath.EndsWith(".asmx", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Url.AbsolutePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Url.AbsolutePath.EndsWith(".axd", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Path.StartsWith("/_vti_bin/") &&
                    !context.Request.Path.EndsWith("/Postback.FormServer.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Path.StartsWith("/_layouts/mobile/") && (context.Response.ContentType == "text/html" || context.Response.ContentType == "application/json"))
                {
                    AddResponseFilter(context);
                }
                else if (context.Request.Url.AbsolutePath.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase) &&
                        !context.Request.Path.EndsWith("/Postback.FormServer.aspx", StringComparison.OrdinalIgnoreCase) &&
                         ((context.Response.ContentType == "text/html" || context.Response.ContentType == "text/plain" || context.Response.ContentType == "application/json") &&
                          (HttpContext.Current.Request.Form["__CALLBACKPARAM"] == null) &&
                          (HttpContext.Current.Request.Form["Bamboo_UpdatePage"] == null)))
                {
                    if (HttpContext.Current.Request.Form["&__EVENTVALIDATION"] != null)
                    {
                        if (!HttpContext.Current.Request.Form["&__EVENTVALIDATION"].Contains("SPS_STATIC_LNG_"))
                        {
                            AddResponseFilter(context);
                        }
                    }
                    else
                    {
                        AddResponseFilter(context);
                    }
                }
            }
            catch (Exception ex)
            {
                SPSecurity.RunWithElevatedPrivileges(
                    () =>
                    Utilities.LogException(
                        "Error in Oceanik.HttpApplicationEvents.Context_ReleaseRequestState: " + ex.Message,
                        EventLogEntryType.Warning));
            }
        }

        public void OnPreProcessRequest(object sender, EventArgs eventArgs)
        {
            // CalendarService.ashx
            if (HttpContext.Current.Request.Path.Contains("CalendarService.ashx"))
            {
                HttpContext context = ((HttpApplication)sender).Context;
                string languageCode = Utilities.GetLanguageCode(context);
                SPRegionalSettings regionalSettings = SPContext.Current.RegionalSettings;

                int lcid = Languages.Instance.GetLcid(languageCode);

                regionalSettings.LocaleId = (uint)lcid;
            }
        }

        private void SetDisableItemTrad(HttpContext context, SPWeb objWeb, string listForItemLanguageId)
        {
            if (string.IsNullOrEmpty(listForItemLanguageId))
            {
                _currentTranslatorHelperToUse.DisableItemTrad(objWeb, context.Request.QueryString["SPS_MenuWebPartID"]);
            }
            else
            {
                _currentTranslatorHelperToUse.DisableItemTradFromList(objWeb, listForItemLanguageId, context.Request.Url.AbsoluteUri);
            }
        }

        private void SetEnableItemTrad(HttpContext context, SPWeb objWeb, string listForItemLanguageId)
        {
            if (string.IsNullOrEmpty(listForItemLanguageId))
            {
                _currentTranslatorHelperToUse.EnableItemTrad(objWeb, context.Request.QueryString["SPS_MenuWebPartID"]);
            }
            else
            {
                _currentTranslatorHelperToUse.EnableItemTradFromList(listForItemLanguageId, context.Request.Url.AbsoluteUri);
            }
        }

        private bool IsTrialFinished()
        {
            if (_license != null && _license.IsValide)
            {
                if ((DateTime.Now.Date >= _license.TrialStart.Date) && (DateTime.Now.Date <= _license.TrialEnd.Date))
                {
                    return false;
                }

                Utilities.LogException("Oceanik 2010 License has expired");
                return true;
            }

            // Grace Period if no license was provided to client
            var trialBeginDate = new DateTime(2010, 11, 01);

            // Set To DateMax for non trial version
            var trialEndDate = new DateTime(2011, 12, 01);

            // Set To DateMin for non trial version
            if ((DateTime.Now.Date < trialBeginDate.Date) || (DateTime.Now.Date > trialEndDate.Date))
            {
                Utilities.LogException("Oceanik 2010 License has expired");
                return true;
            }

            return false;
        }

        private void AddResponseFilter(HttpContext context)
        {
            string rootSiteUrl = Alphamosaik.Common.SharePoint.Library.Utilities.GetAbsoluteUri(context.ApplicationInstance);
            rootSiteUrl = Alphamosaik.Common.SharePoint.Library.Utilities.FilterUrl(rootSiteUrl); // root server url
            
            bool pageToTranslate = !string.IsNullOrEmpty(rootSiteUrl)
                                       ? _currentTranslatorHelperToUse.IsPageToBeTranslated(rootSiteUrl)
                                       : false;
            bool callNormalHttpHandler = pageToTranslate ? IsTrialFinished() : true;

            if (callNormalHttpHandler)
            {
                return;
            }

            string currentSiteUrl = SPContext.Current.Web.Url;

            if (!currentSiteUrl.EndsWith("/"))
            {
                currentSiteUrl = currentSiteUrl + "/";
            }

            HttpResponse objResponse = context.Response;

            // Lock before loading cache to be sure that it load only once even 
            // if we receive simultaneous request by many users
            lock (LockThis)
            {
                bool reloadCustomCache;
                bool reloadGlobalCache;
                bool cacheMustBeReloaded = StandardTranslatorHelper.GetCacheMustBeReloaded(SPContext.Current, currentSiteUrl, out reloadCustomCache, out reloadGlobalCache);

                if (cacheMustBeReloaded)
                {
                    SPSecurity.RunWithElevatedPrivileges(() => _currentTranslatorHelperToUse.InitializeCache(SPContext.Current, currentSiteUrl, reloadCustomCache, reloadGlobalCache));
                }
            }

            if (HttpContext.Current.Cache["AlphamosaikExtractor"] != null)
            {
                if (HttpContext.Current.Cache["AlphamosaikExtractor"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    SPSecurity.RunWithElevatedPrivileges(() => _currentTranslatorHelperToUse.AddPageUrlForExtractor(rootSiteUrl));
                }
            }

            if (HttpContext.Current.Cache["SPS_FUNCT_WEBPART"] != null)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    _currentTranslatorHelperToUse.ReloadWebpartProperties();
                    HttpContext.Current.Cache.Remove("SPS_FUNCT_WEBPART");
                });
            }

            bool viewAllItemsInEveryLanguages = (bool)HttpContext.Current.Cache["AlphamosaikItemFiltering"] == false;
            string spsTransCodePers = context.Request.QueryString["SPS_Trans_Code_Pers"];

            if (!string.IsNullOrEmpty(spsTransCodePers) && spsTransCodePers.Equals("Unfiltering"))
            {
                // View All Items In Every Language
                viewAllItemsInEveryLanguages = true;
            }

            bool completingDictionaryMode = false;
            int extractorStatus = -1;
            int autocompletionStatus = -1;

            // Do translations actions like allow item langauge, assign a lang to a webPart
            string transCode = context.Request.QueryString["SPS_Trans_Code"];

            if (!string.IsNullOrEmpty(transCode))
            {
                SPWeb objWeb = SPControl.GetContextWeb(context);

                objWeb.AllowUnsafeUpdates = true;

                // Get list id (if not null, it means that we are activating/deactivating the feature from a list and not a WebPart)
                string listForItemLanguageId = context.Request.QueryString["listForItemLanguageId"];

                if (transCode.Equals("EnableItemTrad"))
                {
                    SetEnableItemTrad(context, objWeb, listForItemLanguageId);
                }
                else if (transCode.Equals("DisableItemTrad"))
                {
                    SetDisableItemTrad(context, objWeb, listForItemLanguageId);
                }
                else if (transCode.Equals("SwitchALL"))
                {
                    _currentTranslatorHelperToUse.SwitchToLanguage(objWeb, context.Request.QueryString["SPS_MenuWebPartID"], "ALL");
                }
                else if (transCode.Equals("DisableWebpartContentTrad"))
                {
                    _currentTranslatorHelperToUse.DisableWebpartContentTrad(objWeb, context.Request.QueryString["SPS_MenuWebPartID"]);
                }
                else if (transCode.Equals("EnableWebpartContentTrad"))
                {
                    _currentTranslatorHelperToUse.EnableWebpartContentTrad(objWeb, context.Request.QueryString["SPS_MenuWebPartID"]);
                }
                else if (transCode.Equals("LinkListItem"))
                {
                    Utilities.LinkItemWith(objWeb, context.Request.QueryString["SPS_ListID"],
                                                  context.Request.Url.AbsoluteUri,
                                                  context.Request.QueryString["SPS_ItemID"],
                                                  context.Request.QueryString["SPS_LinkWith"]);
                }
                else if (transCode.Equals("CreateClonedMultilingualItem"))
                {
                    TranslatorAutoTranslation.CreateClonedMultilingualItem(_automaticTranslationPlugin, objWeb,
                                                                           context.Request.QueryString["SPS_ListID"],
                                                                           context.Request.Url.AbsoluteUri,
                                                                           context.Request.QueryString["SPS_ItemID"],
                                                                           context.Request.QueryString["SPS_NewItemLang"],
                                                                           Convert.ToBoolean(context.Request.QueryString["SPS_AutoTranslation"]),
                                                                           false);
                }
                else if (transCode.Equals("SetLanguage"))
                {
                    Utilities.SetItemLanguage(objWeb, context.Request.QueryString["SPS_ListID"],
                                                     context.Request.Url.AbsoluteUri,
                                                     context.Request.QueryString["SPS_ItemID"],
                                                     context.Request.QueryString["SPS_SetLanguage"]);
                }
                else if (transCode.Equals("HideFieldsForLinks"))
                {
                    _currentTranslatorHelperToUse.HideFieldsForLinks(context.Request.QueryString["SPS_ListID"],
                                                        context.Request.Url.AbsoluteUri,
                                                        context.Request.QueryString["SPS_Hide_Action"]);
                }
                else if (transCode.Equals("Completing_Dictionary_Mode_ON"))
                {
                    // View All Items In Every Language
                    completingDictionaryMode = true;
                }
                else if (transCode.Equals("Completing_Dictionary_Mode_Process1"))
                {
                    autocompletionStatus = 1;
                    completingDictionaryMode = true;
                }
                else if (transCode.Equals("Completing_Dictionary_Mode_Process2"))
                {
                    autocompletionStatus = 2;
                    completingDictionaryMode = true;
                }
                else if (transCode.Equals("AddToDictionary"))
                {
                    string resultMessage = _currentTranslatorHelperToUse.AddToDictionary(rootSiteUrl, context.Request.QueryString["SPS_Default_Lang"],
                                                                            context.Request.QueryString["SPS_Phrase_To_Add"], _automaticTranslationPlugin);
                    objResponse.ClearContent();
                    objResponse.Write(resultMessage);
                    return;
                }
                else if (transCode.Equals("AddTerm"))
                {
                    using (var dictionary = new SpsDictionary(rootSiteUrl, _automaticTranslationPlugin))
                    {
                        var status =
                            dictionary.AddTerm(
                                HttpUtility.UrlDecode(context.Request.QueryString["SPS_Term"].Replace("+", "%2B")),
                                context.Request.QueryString["SPS_Default_Lang"]);
                        objResponse.ClearContent();
                        switch (status)
                        {
                            case SpsDictionary.ItemStatus.Deleted:
                                objResponse.Write("Deleted" + SpsDictionary.AlphaSeparator +
                                                  context.Request.QueryString["SPS_Term"]);
                                break;
                            case SpsDictionary.ItemStatus.Existing:
                                objResponse.Write("Existing" + SpsDictionary.AlphaSeparator +
                                                  context.Request.QueryString["SPS_Term"]);
                                break;
                            case SpsDictionary.ItemStatus.Inserted:
                                objResponse.Write("Added" + SpsDictionary.AlphaSeparator +
                                                  context.Request.QueryString["SPS_Term"]);
                                break;
                            case SpsDictionary.ItemStatus.None:
                                objResponse.Write("Error" + SpsDictionary.AlphaSeparator +
                                                  context.Request.QueryString["SPS_Term"]);
                                break;
                            case SpsDictionary.ItemStatus.Updated:
                                objResponse.Write("Updated" + SpsDictionary.AlphaSeparator +
                                                  context.Request.QueryString["SPS_Term"]);
                                break;
                            default:
                                objResponse.Write("Error" + SpsDictionary.AlphaSeparator +
                                                  context.Request.QueryString["SPS_Term"]);
                                break;
                        }
                    }

                    return;
                }
                else if (transCode.Equals("SPS_Reload_Dictionary"))
                {
                    string webId = "_" + Convert.ToString(SPContext.Current.Web.ID);

                    HttpRuntime.Cache.Remove("SPS_TRANSLATION_CACHE_IS_LOADED" + webId);
                    HttpRuntime.Cache.Add("SPS_TRANSLATION_CACHE_IS_LOADED" + webId, "2", null,
                                          Cache.NoAbsoluteExpiration,
                                          Cache.NoSlidingExpiration,
                                          CacheItemPriority.NotRemovable, null);
                    objResponse.ClearContent();
                    objResponse.Write(
                        "<html><body><div ><table style=\"border:solid 0px black; color:black; background:#F3F387;font-family:tahoma;font-size:13;\"><tr ><td><img src=\"/_layouts/images/alpha_logo_menu.png\" border=\"0\" ></td><td style=\"text-align:center;border-bottom:1px solid black;font-weight:bold;\">Operation result</td></tr><tr><td></td><td style=\"padding:5px;padding:20px;\"><div>The dictionary has been updated successfully</div></td></tr><tr height=\"40\" ><td></td><td></td></tr><tr><td></td><td style=\"padding:10px;\"><div><input type=\"button\" name=\"reload\" value=\"Update dictionary cache\" onclick=\"javascript:window.location.search =\'?SPS_Trans_Code=SPS_Reload_Dictionary\'\" style=\"background-color:#3cb371;\" style=\"color:white; font-family:tahoma;font-size:13;\"/></div></td></table></div></body></html>");
                    return;
                }
                else if (transCode.Equals("Translation_Extractor"))
                {
                    if (context.Request.QueryString["SPS_Extractor_Status"] == "-1")
                    {
                        _currentTranslatorHelperToUse.ProcessTranslationExtractor(rootSiteUrl, HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"].ToString());
                    }

                    if (context.Request.QueryString["SPS_Extractor_Status"] != "-1")
                    {
                        extractorStatus = Convert.ToInt32(context.Request.QueryString["SPS_Extractor_Status"]);
                    }
                }
                else if (transCode.IndexOf("Switch") != -1)
                {
                    foreach (string language in Languages.Instance.AllLanguages)
                    {
                        if (transCode.Equals("Switch" + language))
                        {
                            _currentTranslatorHelperToUse.SwitchToLanguage(objWeb, context.Request.QueryString["SPS_MenuWebPartID"],
                                                              language);
                        }
                    }
                }
            }

            bool mobilePage = context.Request.Path.StartsWith("/_layouts/mobile/", StringComparison.OrdinalIgnoreCase);

            string languageCode = Utilities.GetLanguageCode(context);

            if ((bool)HttpContext.Current.Cache["AlphamosaikDefaultLangDeactivation"] &&
                HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"].ToString() == languageCode)
            {
                return;
            }

            if (Utilities.IsCrawler(context) == 0)
            {
                objResponse.Filter = new ResponseFilterGenericStream(objResponse.Filter, _currentTranslatorHelperToUse, languageCode, true,
                                                                     viewAllItemsInEveryLanguages,
                                                                     completingDictionaryMode, extractorStatus, currentSiteUrl,
                                                                     autocompletionStatus, mobilePage, _license.Type);
            }
            else
                if (Utilities.IsCrawler(context) == 1)
                {
                    objResponse.Filter = new ResponseFilterRobotsStream(objResponse.Filter, _currentTranslatorHelperToUse, languageCode, currentSiteUrl);
                }
        }
    }
}
