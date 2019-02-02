// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BingTranslation.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the BingTranslation type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Web;
using Alphamosaik.Oceanik.Sdk;
using Translator.Common.Library;
using System.Collections;
using Microsoft.SharePoint;

namespace Alphamosaik.Oceanik.AutomaticTranslation
{
    public class BingTranslation : IAutomaticTranslation
    {
        #region Implementation of IAutomaticTranslation

        public string TranslateText(string toTranslate, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, bool isHtml)
        {
            if (string.IsNullOrEmpty(sourceLang) || string.IsNullOrEmpty(destinationLang))
            {
                return toTranslate;
            }

            try
            {
                sourceLang = sourceLang.Replace("CH", "zh-CHS");
                destinationLang = destinationLang.Replace("CH", "zh-CHS");
                sourceLang = sourceLang.Replace("JP", "ja");
                destinationLang = destinationLang.Replace("JP", "ja");
                sourceLang = sourceLang.Replace("DU", "nl");
                destinationLang = destinationLang.Replace("DU", "nl");

                string translatedText;

                using (var client = new LanguageService.LanguageServiceClient())
                {
                    translatedText = client.Translate(HttpRuntime.Cache["AlphamosaikBingApplicationId"].ToString(), toTranslate, sourceLang.ToLower(), destinationLang.ToLower());
                }

                return translatedText;
            }
            catch (Exception e)
            {
                Utilities.LogException("Error in BingTranslation.TranslateText: " + e.Message, EventLogEntryType.Warning);
                return toTranslate;
            } 
        }

        public byte[] TranslateFile(byte[] buffer, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, string fileName)
        {
            throw new NotImplementedException();
        }

        public bool SupportFileTranslation()
        {
            return false;
        }

        public TranslationUserAccount LoadUserAccount(SPWeb spWeb, string sourceLanguage)
        {
            return new TranslationUserAccount();
        }

        #endregion
    }
}