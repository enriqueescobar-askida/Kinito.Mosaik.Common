// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationManager.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ConfigurationManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Microsoft.SharePoint.Administration;

namespace TranslatorManagement
{
    public static class ConfigurationManager
    {
        public const string TranslatorSettingPrefix = "Alphamosaik.Translator";
        public const string TranslatorSettingOwner = "Alphamosaik.Translator";
        public const string ItemFiltering = "ItemFiltering";
        public const string CompletingMode = "CompletingMode";
        public const string AutomaticTranslationMessage = "AutomaticTranslationMessage";
        public const string LinkedPages = "LinkedPages";
        public const string ExtractorTool = "TranslationsExtractorTool";
        public const string CssBanner = "CssBanner";
        public const string ShowPipe = "ShowPipe";
        public const string ItemDashboard = "ItemDashboard";
        public const string FilteringButton = "FilteringButton";
        public const string QuicklaunchFiltering = "QuickLaunchFiltering";
        public const string TranslationServiceType = "TranslationServiceType";
        public const string TranslationServiceName = "TranslationServiceName";
        public const string TranslationServiceUrl = "TranslationServiceUrl";
        public const string TranslationServiceAuthentication = "TranslationServiceAuthentication";
        public const string TranslationServiceUsername = "TranslationServiceUsername";
        public const string TranslationServicePassword = "TranslationServicePassword";
        public const string TranslationServiceBingApplicationid = "TranslationServiceBingApplicationId";

        private static readonly Dictionary<string, string> DefaultSettings = new Dictionary<string, string>
        {
            { AutomaticTranslationMessage, string.Empty },
            { CompletingMode, "true" },
            { CssBanner, "false" },
            { ExtractorTool, "false" },
            { ItemFiltering, "true" },
            { LinkedPages, "false" },
            { ShowPipe, "true" },
            { ItemDashboard, "true" },
            { FilteringButton, "true" },
            { QuicklaunchFiltering, "false" },
            { TranslationServiceAuthentication, "anonymous" },
            { TranslationServiceName, "systran" },                   // TODO : check the default value for this setting. What is the purpose of this setting?
            { TranslationServicePassword, string.Empty },
            { TranslationServiceType, "systran" },
            { TranslationServiceUrl, string.Empty },                 // TODO : check the default value for this setting. Is there a default URL for the systran web service?
            { TranslationServiceUsername, string.Empty },
            { TranslationServiceBingApplicationid, "674781504275C8FC33D2AE3FF4345CBAA4979EF7" }
        };

        /// <summary>
        /// Given a Web application, returns the value of a setting in the web.config file of the default URL zone.
        /// </summary>
        /// <param name="webApp">Given a Web application</param>
        /// <param name="key">Given a key</param>
        /// <returns>return the value</returns>
        public static string GetSetting(SPWebApplication webApp, string key)
        {
            string rootPath = webApp.IisSettings[SPUrlZone.Default].Path.ToString(); // The modifications are propagated to all zones. So we can get settings from the default zone in all cases.

            var webConfigXmlDocument = new XmlDocument();
            webConfigXmlDocument.Load(rootPath.TrimEnd('/') + "/web.config");

            XmlNode settingNode = webConfigXmlDocument.SelectSingleNode(
                "configuration/appSettings/add[@key=\"" +
                TranslatorSettingPrefix + '.' + key +
                "\"]");

            if (settingNode == null)
            {
                if (DefaultSettings.ContainsKey(key))
                {
                    // Get the default value
                    return DefaultSettings[key];
                }

                // No default value
                throw new Exception(string.Format(
                    "No value is defined for the setting '{0}'.",
                    key));
            }

            // A value is defined
            if (settingNode.Attributes != null)
            {
                return settingNode.Attributes["value"].Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Edit the value of a setting in the web.config or add the key if it does not exist.
        /// Changes are applied only after calling the SaveChanges method.
        /// </summary>
        /// <param name="webApp">The Web Application for which the setting will be set.</param>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public static void SetSetting(SPWebApplication webApp, string key, string value)
        {
            // On vérifie si la valeur a changé
            try
            {
                if (value.ToLower() == GetSetting(webApp, key).ToLower())
                {
                    return;
                }
            }
            catch (Exception e)
            {
                // No default value
                Trace.WriteLine(e.Message);
            }

            string fullKey = TranslatorSettingPrefix + '.' + key;
            string pathEdit = "configuration/appSettings/add[@key=\"" + fullKey + "\"]";
            const string PathAdd = "configuration/appSettings";
            bool flag = false;

            // On supprime d'éventuelles modifications identiques
            for (int i = webApp.WebConfigModifications.Count - 1; i >= 0; i--)
            {
                SPWebConfigModification configModification = webApp.WebConfigModifications[i];

                // On vérifie s'il y a déjà eu édition
                if (configModification.Owner == TranslatorSettingOwner &&
                    configModification.Name == "value" &&
                    configModification.Path == pathEdit &&
                    configModification.Type == SPWebConfigModification.SPWebConfigModificationType.EnsureAttribute)
                {
                    webApp.WebConfigModifications.RemoveAt(i);
                    flag = true;
                }

                // On vérifie s'il y a déjà eu ajout
                if (configModification.Owner == TranslatorSettingOwner &&
                    configModification.Name == "add[@key=\"" + fullKey + "\"]" &&
                    configModification.Path == PathAdd &&
                    configModification.Type == SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode)
                {
                    flag = true;
                }
            }

            // On modifie le web.config
            var webConfigModification = new SPWebConfigModification();

            if (flag)
            {
                SaveChanges(webApp);

                webConfigModification.Owner = TranslatorSettingOwner;
                webConfigModification.Name = "value";
                webConfigModification.Value = value;
                webConfigModification.Path = pathEdit;
                webConfigModification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureAttribute;
            }
            else
            {
                webConfigModification.Owner = TranslatorSettingOwner;
                webConfigModification.Name = "add[@key=\"" + fullKey + "\"]";
                webConfigModification.Value = "<add key=\"" + fullKey + "\" value=\"" + value + "\"/>";
                webConfigModification.Path = PathAdd;
                webConfigModification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            }

            webApp.WebConfigModifications.Add(webConfigModification);
        }

        /// <summary>
        /// Applies changes to the web.config
        /// </summary>
        /// <param name="webApp">Given a Web application</param>
        public static void SaveChanges(SPWebApplication webApp)
        {
            webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
        }
    }
}
