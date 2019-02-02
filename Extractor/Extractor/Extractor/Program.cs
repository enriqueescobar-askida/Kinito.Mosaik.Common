// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.SharePoint;
using Translator.Common.Library;

namespace TranslationsExtractor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string webApplicationUrl = args[0];
            bool isSyntaxOk = true;

            for (int i = 1; i < args.Length; i++)
            {
                if (!Languages.Instance.AllLanguages.Contains(args[i]))
                {
                    isSyntaxOk = false;
                    break;
                }
            }

            if (isSyntaxOk)
            {
                try
                {
                    using (var currentSite = new SPSite(webApplicationUrl))
                    using (SPWeb web = currentSite.OpenWeb())
                    {
                        SPList pagesToParseUrlList = web.GetList("/Lists/PagesToUpdateForTranslation");
                        SPList extractorTranslationsList = web.GetList("/Lists/ExtractorTranslations");

                        if (pagesToParseUrlList.Fields.ContainsField("Pages"))
                        {
                            if (pagesToParseUrlList.Items.Count > 0)
                            {
                                int initExtractorTranslationsCount = extractorTranslationsList.Items.Count;

                                Console.Write("Alphamosaik Translations Extractor process begins...");

                                string defaultLang = args[1];
                                var languageDisplayed = new List<string>();
                                for (int i = 2; i < args.Length; i++)
                                {
                                    languageDisplayed.Add(args[i]);
                                }

                                SPSecurity.RunWithElevatedPrivileges(delegate
                                                                         {
                                    foreach (SPListItem currentUrl in pagesToParseUrlList.Items)
                                    {
                                        foreach (string languageCode in languageDisplayed)
                                        {
                                            if (defaultLang != languageCode)
                                            {
                                                try
                                                {
                                                    int beforeThisAddCount = extractorTranslationsList.Items.Count;

                                                    Console.WriteLine();
                                                    Console.WriteLine(currentUrl["Pages"] + " is being parsed for language: \"" + languageCode + "\" ...");
                                                    WebRequest request = WebRequest.Create(currentUrl["Pages"] + "?SPS_Trans_Code=Translation_Extractor&SPS_Extractor_Status=" + currentUrl.ID + "&SPSLanguage=" + languageCode);
                                                    request.UseDefaultCredentials = true;
                                                    request.Timeout = 300000;

                                                    Console.WriteLine(currentUrl["Pages"] + "(language:\"" + languageCode + "\") is treated : " + (extractorTranslationsList.Items.Count - beforeThisAddCount) +
                                                        " phrases added.");
                                                }
                                                catch
                                                {
                                                    Console.WriteLine(currentUrl["Pages"] + " is not available.");
                                                }
                                            }
                                        }
                                    }
                                });

                                Console.WriteLine();
                                Console.WriteLine("Process completed : " + pagesToParseUrlList.Items.Count + " pages treated.");
                                Console.WriteLine((extractorTranslationsList.Items.Count - initExtractorTranslationsCount) + " phrases added to the \"ExtractorTranslations\" list.");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error : please verify the url or the language codes in parameters.");
                    Console.WriteLine("Syntax : TranslationsExtractor.exe <SiteCollectionRootAbsoluteUrl> <DefaultLanguageCode> <Language1Code> [<Language2Code>] [<Language3Code>]");
                }
            }
            else
            {
                Console.WriteLine("Syntax error : please verify the language codes in parameters.");
                Console.WriteLine("Syntax : TranslationsExtractor.exe <SiteCollectionRootAbsoluteUrl> <DefaultLanguageCode> <Language1Code> [<Language2Code>] [<Language3Code>]");
            }
        }
    }
}
