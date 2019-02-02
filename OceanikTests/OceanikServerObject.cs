// -----------------------------------------------------------------------
// <copyright file="OceanikServerObject.cs" company="AlphaMosaik">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using
using System.Net;
using System.Web;
using Microsoft.SharePoint;

namespace OceanikTests
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OceanikServerObject : IDisposable
    {
        #region PrivateAttributes
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Flag for disposed resources
        /// </summary>
        private bool _IsDisposed = false;
        #endregion

        public string Url { get; internal set; }
        public SPSite SPSite { get; internal set; }
        public SPWeb SPWeb { get; internal set; }
        public SPList TranslationList { get; internal set; }
        //public SPList CustomPhrasesList { get; internal set; }
        public int DefaultLang { get; internal set; }
        public int NewLang { get; internal set; }
        public bool IsMultiLang { get; internal set; }
        public List<string> LangList { get; internal set; }
        //public HttpRequest CurrentRequest { get; internal set; }
        //public HttpResponse CurrentResponse { get; internal set; }
        //public HttpContext CurrentContext { get; internal set; }
        //public HttpCookieCollection CurrentCookies { get; internal set; }

        #region Constructor
        /// <summary>
        /// Oceaniks the server object.
        /// </summary>
        /// <param name="url">The URL.</param>
        public OceanikServerObject(string url)
        {
            if (url != null)
            {
                this.Url = url;
                SPSecurity.RunWithElevatedPrivileges(
                    delegate
                    {
                        using (SPSite spSite = new SPSite(url))
                        using (SPWeb spWeb = spSite.OpenWeb())
                        {
                            this.SPSite = spSite;
                            this.SPWeb = spWeb;
                            this.TranslationList = spWeb.GetList("/Lists/TranslationContents");
                            this.DefaultLang = this.SPWeb.UICulture.LCID;
                            this.IsMultiLang = this.SPWeb.IsMultilingual;
                            this.LangList = spWeb.SupportedUICultures.Select(
                                cultureInfo => cultureInfo.LCID.ToString()).ToList();
                            //HttpRequest httpRequest = new HttpRequest("", SPWeb.Url, "");
                            //this.CurrentRequest = httpRequest;
                            //HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(TextWriter.Null));
                            //this.CurrentContext = HttpContext.Current;
                            //this.CurrentCookies = httpRequest.Cookies;
                            ////SetCookie(this.CurrentContext);
                            //Console.WriteLine(this.CurrentContext.Request.Cookies.AllKeys);
                            //Console.WriteLine(httpRequest.Cookies.Count);
                            //HttpResponse httpResponse = new HttpResponse(TextWriter.Null);
                            //this.CurrentResponse = httpResponse;
                            //Console.WriteLine(httpRequest.Cookies.AllKeys);
                            ////GetCookie();
                            ////Parse();
                            ///*HttpCookie httpCookie = HttpContext.Current.Response.Cookies["SharePointTranslator"];
                            //if (httpCookie != null)
                            //{
                            //    string value = httpCookie.Value;
                            //}*/
                        }
                    }
                );
            }
        }
        #endregion

        #region Destructor
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="OceanikServerObject"/> is reclaimed by garbage collection.
        /// </summary>
        ~OceanikServerObject()
        {
            Dispose(false);
        }
        #endregion

        #region PublicMethods

        /// <summary>
        /// Determines whether [is lang present] [the specified lang].
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns>
        ///   <c>true</c> if [is lang present] [the specified lang]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsLangPresent(string lang)
        {
            return LangList.Contains(lang);
        }

        /// <summary>
        /// Matches the word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public bool MatchWord(string word)
        {
            bool boo = false;
            if (this.TranslationList.Items.Count > 0) //this.TranslationList.Fields.ContainsField(word); only 4 columns
            {
                foreach (SPListItem spListItem in this.TranslationList.Items)
                    boo = spListItem["EN"].ToString().Equals(word);
            }
            return boo;
        }

        /// <summary>
        /// Inserts the word list.
        /// </summary>
        /// <param name="myLangs">My langs.</param>
        /// <param name="myWords">My words.</param>
        /// <returns></returns>
        public bool InsertWordList(List<string> myLangs, List<string> myWords)
        {
            int i = 0;
            bool boo = true;
            SPListItem spListItem = this.TranslationList.Items.Add();
            foreach (string word in myWords)
            {
                boo = boo && MatchWord(word);
                Console.WriteLine("(" + word + ") " + boo);
                spListItem[myLangs.ElementAt(i)] = word;
                Console.WriteLine(myLangs.ElementAt(i) + " Adding " + word);
                i++;
            }
            spListItem.Update();
            spListItem.EnsureWorkflowInformation();
            spListItem.SystemUpdate();
            this.TranslationList.Update();
            //Console.WriteLine(MatchWord(myWords.ElementAt(0)));
            return boo;
        }

        public bool InsertWord(string word, string lang)
        {
            if (!MatchWord(word))
            {
                Console.WriteLine(this.TranslationList.Items.NumberOfFields);
            }
            return true;
        }

        public bool DeleteWord(string word, string lang)
        {
            return true;
        }

        /// <summary>
        /// Determines whether this instance is connected.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConnected()
        {
            return this.SPWeb.Exists;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region PrivateMethods
        private void Parse()
        {
            StreamWriter reqStreamWriter;
            Stream strReceiveStream = null;
            StreamReader sr = null;
            CookieContainer cookies;
            cookies = GetCookie();
            HttpWebRequest req = WebRequest.Create(this.Url) as HttpWebRequest;
            req.CookieContainer = cookies;
            HttpWebResponse result = req.GetResponse() as HttpWebResponse;
            if (result != null && result.StatusCode == HttpStatusCode.OK)
            {
                strReceiveStream = result.GetResponseStream();
                if (strReceiveStream != null) sr = new StreamReader(strReceiveStream);
                if (sr != null)
                {
                    string strResult = sr.ReadToEnd();
                    Console.WriteLine(strResult);
                }
            }
            else
            {
                Console.WriteLine("ERROR");
            }
            if (strReceiveStream != null) strReceiveStream.Close();
            req.Abort();
            if (sr != null) sr.Close();
        }
        private CookieContainer GetCookie()
        {
            //HttpWebResponse result;
            CookieContainer cookies = new CookieContainer();
            HttpWebRequest req = WebRequest.Create(this.Url) as HttpWebRequest;
            if (req != null)
            {
                req.CookieContainer = cookies;
                HttpWebResponse result = req.GetResponse() as HttpWebResponse;
                req.Abort();
            }
            Console.WriteLine(cookies.ToString());
            return cookies;
        }
        private void SetCookie(HttpContext context)
        {
            try
            {
                string language =
                    GetLanguageCodeFromQueryString();

                if (!string.IsNullOrEmpty(language))
                {
                    var cookie = new HttpCookie("SharePointTranslator", language);
                    cookie.Expires = DateTime.MaxValue;
                    context.Response.AppendCookie(cookie);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Translator2009", "Error in Translator2009.TranslatorModule.SetCookie: " + ex.Message, EventLogEntryType.Warning);
            }
        }
        public string GetLanguageCodeFromQueryString()
        {
            string result;

            if (HttpContext.Current.Request.QueryString["SPSLanguage"] != null)
                result = HttpContext.Current.Request.QueryString["SPSLanguage"];
            else
                return null;

            if (IsLangPresent(result))//IsSupportedLanguage(result))
                return result;

            int intIdx = HttpContext.Current.Request.Url.ToString().LastIndexOf("SPSLanguage") + "SPSLanguage".Length + 1;
            result = HttpContext.Current.Request.Url.ToString().Substring(intIdx, 2);

            if (IsLangPresent(result))
                return result;

            if (HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"] != null)
                return HttpContext.Current.Cache["SPS_TRANSLATION_DEFAULT_LANGUAGE"].ToString();
            else
                return null;
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="isDiposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool isDiposing)
        {
            //Check if Dispose has been called
            if (!this._IsDisposed)
            {//dispose managed and unmanaged resources
                if (isDiposing)
                {//managed resources clean
                    //string
                    this.Url = null;
                    this.SPSite.Dispose();
                    this.SPWeb.Dispose();
                    this.TranslationList.Update();
                    this.LangList.Clear();
                    this.LangList = null;
                }
                //unmanaged resources clean
                this.DefaultLang = -1;
                this.NewLang = -1;
                this.IsMultiLang = false;
                //confirm cleaning
                this._IsDisposed = true;
            }
        }
        #endregion
    }
}
