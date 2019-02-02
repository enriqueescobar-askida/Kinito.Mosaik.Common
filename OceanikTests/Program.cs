// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Net;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Administration;
using System.IO;
using System.Text;
using System.Web;
using System.ComponentModel;
using System.Runtime.InteropServices;


using OceanikTests.Resources;
using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using System.Linq;
using Alphamosaik.Common.Library;
using Alphamosaik.Common.SharePoint.Library;

using Translator.Common.Library;
using Exception = System.Exception;

namespace OceanikTests
{

    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        private static string webApplicationUrl = String.Empty;
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            bool isSyntaxOk = true;
            bool isSiteOK = false;
            bool isWordOK = false;
            bool isWordIN = false;
            List<string> myLangs = new List<string>();
            List<string> myWords = new List<string>(new string[] {Welcoming.EN, Welcoming.FR});
            List<string> myLcids = new List<string>();

            //setting usage
            string usage =
            @" error : please verify the language codes in parameters.\nSyntax :\nOceanikTests.exe <SiteCollectionRootAbsoluteUrl> <your phrase> <DefaultLanguageCode> <LanguageCode1> [<LanguageCode2>]";
            usage += "Example:\n";
            usage += "OceanikTests.exe http://win-1icsimtja6l:22222";
            usage += " \"Getting Started\"";
            usage += " EN FR SP";

            //parsing args
            webApplicationUrl = args[0];
            //checking langs starting on third argument
            for (int i = 2; i < args.Length; i++)
            {
                Console.WriteLine("\n--->Validation of " + args[i] + ":" + LangCode(LangIndex(args[i])) +
                    " is suported? " + IsSupportedLanguage(LangCode(LangIndex(args[i]))));
                if (!IsValidLang(args[i]) && !IsSupportedLanguage(LangCode(LangIndex(args[i]))))
                {
                    isSyntaxOk = false;
                    break;
                }
                else
                {
                    myLangs.Add(args[i]);
                    myLcids.Add(LangCode(LangIndex(args[i])));
                }
            }

            //match count
            if (myLangs.Count != myWords.Count)
            {
                usage = " count do not match " + usage;
                isSyntaxOk = false;
            }

            //process it
            if (isSyntaxOk)
            {
                foreach (string myLang in myLangs)
                    Console.WriteLine(myLang);

                foreach (string myWord in myWords)
                    Console.WriteLine(myWord);

                //find first element in EN=default
                string webSearchPhrase = myWords.ElementAt(0);//args[1];
                string webDefaultLang = args[2];
                int webDefaultInd_ = LangIndex(webDefaultLang);
                string webDefaultCode = LangCode(webDefaultInd_);
                
                //fetch first element in EN=default
                Console.WriteLine("\nOceanikTest searching <" + webSearchPhrase + "> in " +
                webDefaultLang + " (" + webDefaultInd_ + "," + webDefaultCode + ") at " +
                webApplicationUrl + "\n");

                //check page is cached
                Console.WriteLine("Is Page Cached? " + TestPageCached(webApplicationUrl) + "\n");
                //TestOceanikClient(webApplicationUrl.Replace("/SitePages/Home.aspx", ""));

                //check connexion is stablished
                isSiteOK = TestOceanikServerConnection(webApplicationUrl.Replace("/SitePages/Home.aspx", ""));
                Console.WriteLine("Is connexion stablished? " + isSiteOK + "\n");

                //check is multilingual
                Console.WriteLine("Is multilingual? " +
                    TestOceanikServerObjectMultiLang(webApplicationUrl.Replace("/SitePages/Home.aspx", "")) + "\n");

                //check default language
                Console.WriteLine("Has a default language? " + isSiteOK + " and its lcid is " +
                    TestOceanikServerDefaultLang(webApplicationUrl.Replace("/SitePages/Home.aspx", "")) + "\n");

                //check word/phrase in website in default language
                isWordOK = TestOceanikClientWord(webApplicationUrl.Replace("/SitePages/Home.aspx", ""),
                            webSearchPhrase);
                Console.WriteLine("Is word/phrase <" + webSearchPhrase + "> in web site <" + webDefaultLang + ">? " +
                    isWordOK + "\n");

                //if not in EN_website exit
                //if (!isWordOK) return;

                //check if word/phrase is in translation list
                isWordIN = TestOceanikServerWordIn(webApplicationUrl.Replace("/SitePages/Home.aspx", ""),
                            webSearchPhrase);
                Console.WriteLine("Is word/phrase <" + webSearchPhrase + "> in dictionary? " + isWordIN + "\n");

                //if EN_word not in translation add list
                if (!isWordIN)
                {
                    Console.WriteLine("Adding Entire List to Translation list");
                    TestOceanikServerEnterWordsIn(webApplicationUrl.Replace("/SitePages/Home.aspx", ""),
                                                  myLangs, myWords);

                    //TestOceanikServerInsertWordIn(webApplicationUrl.Replace("/SitePages/Home.aspx", ""), "EN");
                    Console.WriteLine("Thibault");
                    return;
                }
                return;
            }
            else
                Console.WriteLine("SYNTAX " + usage);
        }

        /// <summary>
        /// Tests the oceanik server enter words in.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="myLangs">My langs.</param>
        /// <param name="myWords">My words.</param>
        private static void TestOceanikServerEnterWordsIn(string url, List<string> myLangs, List<string> myWords)
        {
            //bool boo = true;
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(url))
            {
                oceanikServerObject.InsertWordList(myLangs, myWords);
                /*int i = 0;
                SPListItem spListItem = oceanikServerObject.TranslationList.Items.Add();
                foreach (string word in myWords)
                {
                    boo = boo && oceanikServerObject.MatchWord(word);
                    Console.WriteLine("TestOceanikServerEnterWordsIn(" + word + ") " + boo);
                    spListItem[myLangs.ElementAt(i)] = word;
                    Console.WriteLine(myLangs.ElementAt(i) + " Adding " + word);
                    i++;
                }
                spListItem.Update();*/
            }
        }

        /// <summary>
        /// Determines whether [is supported language] [the specified language code].
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>
        ///   <c>true</c> if [is supported language] [the specified language code]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSupportedLanguage(string languageCode)
        {
            bool boo = false;
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(
                webApplicationUrl.Replace("/SitePages/Home.aspx", "")))
            {
                boo = oceanikServerObject.IsLangPresent(languageCode);
            }
            return boo;
        }

        /// <summary>
        /// Tests the page cached.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static bool TestPageCached(string url)
        {
            Console.WriteLine("TestPageCached(" + url + ")");
            bool boo = true;
            // Create a 'WebRequest' object with the specified url. 
            StringBuilder htmlResponse = GetHtmlResponse(url);

            if (htmlResponse.IndexOf("Oceanik retrieved Page from second level cache - ") == -1)
                boo = false;
                //throw new Exception(TestExceptions.PageNotInCache);
            return boo;
        }

        /// <summary>
        /// Tests the oceanik client.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static void TestOceanikClient(string url)
        {
            using (OceanikClientObject oceanikClientObject = new OceanikClientObject(url))
            {
                Console.WriteLine("TestOceanikClient(" + url + ")");
                oceanikClientObject.ConnectToClient();
                oceanikClientObject.LoadToClient();
                string[] strArr = {Welcoming.EN, Welcoming.FR};
                foreach (string str in strArr)
                    Console.WriteLine(str + ":" + !oceanikClientObject.IsAbscent(str));
            }
        }

        /// <summary>
        /// Tests the oceanik server connection.
        /// </summary>
        /// <param name="url">The url.</param>
        private static bool TestOceanikServerConnection(string url)
        {
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(url))
            {
                Console.WriteLine("TestOceanikServerConnection(" + url + ")");
                return oceanikServerObject.IsConnected();
            }
        }

        /// <summary>
        /// Determines whether [is supported lang] [the specified URL].
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="lang">The lang.</param>
        /// <returns>
        ///   <c>true</c> if [is supported lang] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSupportedLang(string url, string lang)
        {
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(url))
            {
                Console.WriteLine("IsSupportedLang(" + url + "," + lang + ")");
                Console.WriteLine("language " + lang + "is supported " +
                    oceanikServerObject.LangList.Contains(LangCode(LangIndex(lang))));
                return oceanikServerObject.LangList.Contains(LangCode(LangIndex(lang)));
            }
        }

        /// <summary>
        /// Tests the oceanik server object multi lang.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static bool TestOceanikServerObjectMultiLang(string url)
        {
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(url))
            {
                Console.WriteLine("TestOceanikServerObjectMultiLang(" + url + ")");
                return oceanikServerObject.IsMultiLang;
            }
        }

        /// <summary>
        /// Tests the oceanik server default lang.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static int TestOceanikServerDefaultLang(string url)
        {
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(url))
            {
                Console.WriteLine("TestOceanikServerDefaultLang(" + url + ")");
                return oceanikServerObject.DefaultLang;
            }
        }

        /// <summary>
        /// Tests the oceanik client word.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        private static bool TestOceanikClientWord(string url, string word)
        {
            using (OceanikClientObject oceanikClientObject = new OceanikClientObject(url))
            {
                Console.WriteLine("TestOceanikClientWord(" + url + "," + word + ")");
                oceanikClientObject.ConnectToClient();
                oceanikClientObject.LoadToClient();
                return !oceanikClientObject.IsAbscent(word);
            }
        }

        /// <summary>
        /// Tests the oceanik server word in.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        private static bool TestOceanikServerWordIn(string url, string word)
        {
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(url))
            {
                Console.WriteLine("TestOceanikServerWordIn(" + url + "," + word + ")");
                return oceanikServerObject.MatchWord(word);
            }
        }

        private static void TestOceanikServerInsertWordIn(string url, string word)
        {
            using (OceanikServerObject oceanikServerObject = new OceanikServerObject(url))
            {
                Console.WriteLine("TestOceanikServerInsertWordIn(" + url + "," + word + ")");
                SPList spList = oceanikServerObject.TranslationList;
                Console.WriteLine("TATA1");
                /*int i = 0;
                foreach (SPListItem spListItem in spList.Items)
                {
                    if ((bool)(spListItem["isCustomize"]))
                    {
                        Console.WriteLine(i + " @@ " + spListItem["EN"]);
                    }
                    //Console.WriteLine(i + " @ " + spListItem["EN"]);
                    i++;
                }*/
                SPListItem v = spList.Items.Add();
                /*Console.WriteLine("TATA2");
                v["EN"] = "Hi";*/
                Console.WriteLine("TATAHI");
                v["FR"] = "Bonjour";
                Console.WriteLine("TATAFR");
                v.Update();
                Console.WriteLine("TATA");
                //v.Delete();
            }
        }

        /// <summary>
        /// Determines whether [is valid lang] [the specified lang].
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns>
        ///   <c>true</c> if [is valid lang] [the specified lang]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidLang(string lang)
        {
            return Languages.Instance.AllLanguages.Contains(lang);
        }

        /// <summary>
        /// Langs the code.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private static string LangCode(int index)
        {
            return Languages.Instance.Lcid.ElementAt(index);
        }

        /// <summary>
        /// Langs the index.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns></returns>
        private static int LangIndex(string lang)
        {
            int i = 0;
            int result = -1;
            string[] lnid = Languages.Instance.AllLanguages;
            IEnumerator iEnumerator = lnid.GetEnumerator();
            while(iEnumerator.MoveNext())
            {
                if (iEnumerator.Current != null && iEnumerator.Current.ToString() == lang)
                    result = i;
                i++;
            }
            return result;
        }

        /// <summary>
        /// Gets the HTML response.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static StringBuilder GetHtmlResponse(string url)
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
    }
}
