using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using Microsoft.SharePoint.Administration;
using AlphaMosaik.Logger.Properties;
using System.Xml.Serialization;
using System.IO;
using Microsoft.SharePoint;
using System.Text.RegularExpressions;
using System.Xml;
using System.Configuration;
using System.Diagnostics;

namespace AlphaMosaik.Logger.Configuration
{
    public class ConfManager
    {
        public AppSettingsSection settingsSection;

        public ConfManager()
        {
            LoadSettings();
        }

        protected void LoadSettings()
        {
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            settingsSection = config.AppSettings;
            Trace.WriteLine("Lecture du fichier de configuration : " + config.FilePath);
        }

        public static void EnableLogger(SPWebApplication webApplication)
        {
            SetSetting(webApplication, Properties.Settings.Default.Settings_Logger_Enabled, "true");
            SaveSettings(webApplication);
        }

        public static void DisableLogger(SPWebApplication webApplication)
        {
            SetSetting(webApplication, Properties.Settings.Default.Settings_Logger_Enabled, "false");
            SaveSettings(webApplication);
        }

        public static bool IsLoggerEnabled(SPWebApplication webApplication)
        {
            string enabledAsString;

            if (TryGetSetting(webApplication, Properties.Settings.Default.Settings_Logger_Enabled, out enabledAsString))
            {
                bool enabled;
                if (bool.TryParse(enabledAsString, out enabled))
                {
                    return enabled;
                }
            }
            return false;
        }

        public static string GetSetting(string key)
        {
            return WebConfigurationManager.AppSettings.Get(Properties.Settings.Default.Settings_Prefix + '.' + key); // TODO : vérifier exception lorsque key inexistante
        }

        public static string GetSetting(SPWebApplication webApp, string key)
        {
            string rootPath = webApp.IisSettings[SPUrlZone.Default].Path.ToString(); // TODO : ne pas se baser sur SPUrlZone.Default

            XmlDocument webConfigXmlDocument = new XmlDocument();
            webConfigXmlDocument.Load(rootPath.TrimEnd('/') + "/web.config");

            XmlNode settingNode = webConfigXmlDocument.SelectSingleNode(
                "configuration/appSettings/add[@key=\"" +
                Properties.Settings.Default.Settings_Prefix + '.' + key +
                "\"]");

            if (settingNode == null)
            {
                throw new ArgumentException("The setting key does not exist.");
            }

            return settingNode.Attributes["value"].Value;
        }

        public static bool TryGetSetting(SPWebApplication webApp, string key, out string result)
        {
            string rootPath = webApp.IisSettings[SPUrlZone.Default].Path.ToString(); // TODO : ne pas se baser sur SPUrlZone.Default

            XmlDocument webConfigXmlDocument = new XmlDocument();
            webConfigXmlDocument.Load(rootPath.TrimEnd('/') + "/web.config");

            XmlNode settingNode = webConfigXmlDocument.SelectSingleNode(
                "configuration/appSettings/add[@key=\"" +
                Properties.Settings.Default.Settings_Prefix + '.' + key +
                "\"]");

            if (settingNode == null)
            {
                result = string.Empty;
                return false;
            }
            else
            {
                result = settingNode.Attributes["value"].Value;
                return true;
            }
        }

        public static string GetConnectionString(string name)
        {
            return WebConfigurationManager.ConnectionStrings[name].ConnectionString; // TODO : prendre en compte le web.config de l'application selectionnee
        }

        public static ConnectionStringSettings GetConnectionString(SPWebApplication webApp, string connectionName)
        {
            string rootPath = webApp.IisSettings[SPUrlZone.Default].Path.ToString(); // TODO : ne pas se baser sur SPUrlZone.Default

            XmlDocument webConfigXmlDocument = new XmlDocument();
            webConfigXmlDocument.Load(rootPath.TrimEnd('/') + "/web.config");

            XmlNode connectionNode = webConfigXmlDocument.SelectSingleNode(
                "configuration/connectionStrings/add[@name=\"" +
                 connectionName +
                "\"]");

            if (connectionNode == null)
            {
                throw new ArgumentException("The specified connection string does not exist.");
            }

            ConnectionStringSettings connection = new ConnectionStringSettings(
                connectionName,
                connectionNode.Attributes["connectionString"].Value,
                connectionNode.Attributes["providerName"].Value);

            return connection;
        }

        public static void SetConnectionString(SPWebApplication webApp, ConnectionStringSettings connectionStringSettings)
        {
            string name = connectionStringSettings.Name;
            string pathEdit = "configuration/connectionStrings/add[@name=\"" + name + "\"]";
            string pathAdd = "configuration/connectionStrings";
            bool flag = false;

            // On supprime d'éventuelles modifications identiques
            for (int i = webApp.WebConfigModifications.Count - 1; i >= 0; i--)
            {
                SPWebConfigModification configModification = webApp.WebConfigModifications[i];

                // On vérifie s'il y a déjà eu une édition
                if (configModification.Owner == Properties.Settings.Default.Settings_Owner &&
                    (configModification.Name == "providerName" || configModification.Name == "connectionString") &&
                    configModification.Path == pathEdit &&
                    configModification.Type == SPWebConfigModification.SPWebConfigModificationType.EnsureAttribute)
                {
                    webApp.WebConfigModifications.RemoveAt(i);
                    flag = true;
                }
                // On vérifie s'il y a eu un ajout
                if (configModification.Owner == Properties.Settings.Default.Settings_Owner &&
                    configModification.Name == "add[@name=\"" + connectionStringSettings.Name + "\"]" &&
                    configModification.Path == pathAdd &&
                    configModification.Type == SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode)
                {
                    flag = true;
                }
            }

            // On modifie le web.config
            if (flag) // on édite
            {
                SaveSettings(webApp);

                SPWebConfigModification modificationProviderName = new SPWebConfigModification();
                SPWebConfigModification modificationConnectionString = new SPWebConfigModification();

                modificationProviderName.Owner = modificationConnectionString.Owner = Properties.Settings.Default.Settings_Owner;
                modificationProviderName.Type = modificationConnectionString.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureAttribute;
                modificationProviderName.Path = modificationConnectionString.Path = pathEdit;
                modificationProviderName.Name = "providerName";
                modificationConnectionString.Name = "connectionString";
                modificationProviderName.Value = connectionStringSettings.ProviderName;
                modificationConnectionString.Value = connectionStringSettings.ConnectionString;

                webApp.WebConfigModifications.Add(modificationProviderName);
                webApp.WebConfigModifications.Add(modificationConnectionString);
            }
            else // on ajoute
            {
                SPWebConfigModification modification = new SPWebConfigModification();

                modification.Owner = Properties.Settings.Default.Settings_Owner;
                modification.Name = "add[@name=\"" + connectionStringSettings.Name + "\"]";
                modification.Value = "<add name=\"" + connectionStringSettings.Name + "\" providerName=\"" + connectionStringSettings.ProviderName + "\" connectionString=\"" + connectionStringSettings.ConnectionString + "\"/>";
                modification.Path = pathAdd;
                modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;

                webApp.WebConfigModifications.Add(modification);
            }
        }

        public static void SetSetting(string key, string value)
        {
        }

        /// <summary>
        /// Modifie la valeur d'un paramètre de configuration du Logger.
        /// Les modifications ne sont enregistrées qu'après l'appel de SaveSettings.
        /// </summary>
        /// <param name="webApp">Application Web pour laquelle sera modifié le paramètre</param>
        /// <param name="key">Clé unique permettant de retrouver le paramètre</param>
        /// <param name="value">Valeur du paramètre</param>
        public static void SetSetting(SPWebApplication webApp, string key, string value)
        {
            string fullKey = Properties.Settings.Default.Settings_Prefix + '.' + key;
            string pathEdit = "configuration/appSettings/add[@key=\"" + fullKey + "\"]";
            string pathAdd = "configuration/appSettings";
            bool flag = false;

            // On supprime d'éventuelles modifications identiques
            for (int i = webApp.WebConfigModifications.Count - 1; i >= 0; i--)
            {
                SPWebConfigModification configModification = webApp.WebConfigModifications[i];

                // On vérifie s'il y a déjà eu édition
                if (configModification.Owner == Properties.Settings.Default.Settings_Owner &&
                    configModification.Name == "value" &&
                    configModification.Path == pathEdit &&
                    configModification.Type == SPWebConfigModification.SPWebConfigModificationType.EnsureAttribute)
                {
                    webApp.WebConfigModifications.RemoveAt(i);
                    flag = true;
                }
                // On vérifie s'il y a déjà eu ajout
                if (configModification.Owner == Properties.Settings.Default.Settings_Owner &&
                    configModification.Name == "add[@key=\"" + fullKey + "\"]" &&
                    configModification.Path == pathAdd &&
                    configModification.Type == SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode)
                {
                    flag = true;
                }
            }

            // On modifie le web.config
            SPWebConfigModification webConfigModification = new SPWebConfigModification();

            if (flag)
            {
                SaveSettings(webApp);

                webConfigModification.Owner = Properties.Settings.Default.Settings_Owner;
                webConfigModification.Name = "value";
                webConfigModification.Value = value;
                webConfigModification.Path = pathEdit;
                webConfigModification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureAttribute;
            }
            else
            {
                webConfigModification.Owner = Properties.Settings.Default.Settings_Owner;
                webConfigModification.Name = "add[@key=\"" + fullKey + "\"]";
                webConfigModification.Value = "<add key=\"" + fullKey + "\" value=\"" + value + "\"/>";
                webConfigModification.Path = pathAdd;
                webConfigModification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            }

            webApp.WebConfigModifications.Add(webConfigModification);
        }

        /// <summary>
        /// Applique les changements de configuration effectués pour l'application Web passée en paramètre.
        /// </summary>
        /// <param name="webApp"></param>
        public static void SaveSettings(SPWebApplication webApp)
        {
            webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
        }

        public static StorageProviderDefinitions GetProviderDefinitions()
        {
            SPFarm localFarm = SPFarm.Local;

            if (localFarm.Properties.ContainsKey(Settings.Default.FarmPropertyBag_ProvidersDefinitions_Key))
            {
                string strDefinitions = (string)localFarm.Properties[Settings.Default.FarmPropertyBag_ProvidersDefinitions_Key];
                StringReader strReader = new StringReader(strDefinitions);

                XmlSerializer serializer = new XmlSerializer(typeof(StorageProviderDefinitions));
                StorageProviderDefinitions definitions = (StorageProviderDefinitions)serializer.Deserialize(strReader);

                return definitions;
            }
            else
            {
                return new StorageProviderDefinitions();
            }
        }
    }
}
