// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VaryByCultureHandler.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the VaryByCultureHandler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web;
using Microsoft.SharePoint.ApplicationRuntime;
using Translator.Common.Library;

namespace TranslatorHttpHandler
{
    public class VaryByCultureHandler : IVaryByCustomHandler
    {
        internal const int LowDetail = 1;

        private const string VaryByCultureString = "VaryByCulture";
        
        public string GetVaryByCustomString(HttpApplication app, HttpContext context, string custom)
        {
            if (string.IsNullOrEmpty(custom))
            {
                return string.Empty;
            }

            // This code handles custom vary parameters specified in the cache profile 
            string[] strings = custom.Split(';');

            string langCookie = Utilities.GetLanguageCode(context);
            
            if (string.IsNullOrEmpty(langCookie))
            {
                // Cas de la première utilisation du module multilingue par l'utilisateur : le cookie n'existe pas où n'a pas encore d'info de langue
                // Permet la compatibilité du output cache de IIS 7
                langCookie = "XX";
            }

            // verify if our parameter is configured 
            foreach (string str in strings)
            {
                // if yes, lets return a custom variation string which contains the CurrentUICulture LCID
                if (str == VaryByCultureString)
                {
                    return "VaryByCulture=" + langCookie;
                }
            }

            // if our parameter has not been provided just return an empty string. 
            return string.Empty;
        }
    } 
}
