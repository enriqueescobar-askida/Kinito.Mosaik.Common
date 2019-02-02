// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAutomaticTranslation.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the IAutomaticTranslation type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using Microsoft.SharePoint;
namespace Alphamosaik.Oceanik.Sdk
{
    public interface IAutomaticTranslation
    {
        string TranslateText(string toTranslate, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, bool isHtml);

        byte[] TranslateFile(byte[] buffer, string sourceLang, string destinationLang, string profileId, TranslationUserAccount translationUserAccount, string fileName);

        bool SupportFileTranslation();

        TranslationUserAccount LoadUserAccount(SPWeb spWeb, string sourceLanguage);
    }
}