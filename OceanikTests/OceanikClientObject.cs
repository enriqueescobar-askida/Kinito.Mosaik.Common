using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
//using
using Microsoft.SharePoint.Client;

namespace OceanikTests
{
    /// <summary>
    /// 
    /// </summary>
    public class OceanikClientObject : IDisposable
    {
        #region PrivateAttributes
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Flag for disposed resources
        /// </summary>
        private bool _IsDisposed = false;
        #endregion

        #region PublicAttributes
        public string Url { get; internal set; }
        public StringBuilder HtmlResponse { get; internal set; }
        public ClientContext ClientContext { get; internal set; }
        public Site Site { get; internal set; }
        public Web Web { get; internal set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OceanikClientObject"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        public OceanikClientObject(string url)
        {
            if (url != null) this.Url = url;
        }
        #endregion

        #region Destructor
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="OceanikClientObject"/> is reclaimed by garbage collection.
        /// </summary>
        ~OceanikClientObject()
        {
            Dispose(false);
        }
        #endregion

        #region PublicMethods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Connects to client.
        /// </summary>
        public void ConnectToClient()
        {
            this.ClientContext = new ClientContext(this.Url);
            this.Site = this.ClientContext.Site;
            this.Web = this.ClientContext.Web;
        }

        /// <summary>
        /// Loads to client.
        /// </summary>
        public void LoadToClient()
        {
            this.ClientContext.Load(this.Web.Webs);
            this.ClientContext.ExecuteQuery();
            foreach (Web webSite in this.Web.Webs)
            {
                Console.WriteLine("Connected to website:\t{0}",webSite.Title);
            }
        }

        /// <summary>
        /// Determines whether the specified word is abscent.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>
        ///   <c>true</c> if the specified word is abscent; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAbscent(string word)
        {
            return (GetHtmlResponse(this.Url).IndexOf(word) == -1);
        }
        #endregion

        #region PrivateMethods
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
                    //Process
                    this.HtmlResponse = null;
                    this.ClientContext.Dispose();
                    this.Site = null;
                    this.Web.DeleteObject();
                }
                //unmanaged resources clean

                //confirm cleaning
                this._IsDisposed = true;
            }
        }

        /// <summary>
        /// Gets the HTML response.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private StringBuilder GetHtmlResponse(string url)
        {
            StringBuilder contents = new StringBuilder(string.Empty);
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;

            // Send the 'WebRequest' and wait for response.
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            // Use "ResponseUri" property to get the actual Uri from where the response was attained.
            if (webResponse != null)
            {
                // Gets the stream associated with the response.
                Stream receiveStream = webResponse.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                if (receiveStream != null)
                {
                    StreamReader readStream = new StreamReader(receiveStream, encode);

                    while (!readStream.EndOfStream)
                        contents.Append(readStream.ReadLine());

                    // Releases the resources of the response.
                    webResponse.Close();

                    // Releases the resources of the Stream.
                    readStream.Close();
                }
            }
            return contents;
        }
        #endregion
    }
}
