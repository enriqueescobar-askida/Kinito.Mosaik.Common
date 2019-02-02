using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;

namespace AlphaMosaik.SharePoint.ConfigurationStore.Features.Configuration_Store
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using AlphaMosaik.SharePoint.ConfigurationStore.ConfigListEventReceiver;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("5a9fa09b-fa10-4a2f-9ecc-977ddee5837e")]
    public class Configuration_StoreEventReceiver : SPFeatureReceiver
    {
        private const string m_SiteUrlModification = "SiteKeyModification";
        private const string m_WebNameModification = "WebNameKeyModification";
        private const string m_ListNameModification = "ListNameKeyModification";
        private const string m_ExpressionBuilderModification = "ExpressionBuilderModification";
        private const string m_CacheFileModification = "CacheFileModification";
        private const string m_AppSettingsXPath = "configuration/appSettings";

        private const string m_ConfigSiteAppSettingsKey = "ConfigSiteUrl";
        private const string m_ConfigWebAppSettingsKey = "ConfigWebName";
        private const string m_ConfigListAppSettingsKey = "ConfigListName";

        private const string m_ModificationOwner = "ConfigStoreFeatureReceiver";
        private const string m_DefaultListName = "Configuration Store";
        private const string m_ExpressionBuildersXPath = "configuration/system.web/compilation/expressionBuilders";
        private const string m_CacheFileAppSettingsKey = "ConfigStoreCacheDependencyFile";

        private const string m_CacheDependencyFile = @"\\myUncPath\folder\ConfigStoreCacheDependency.txt";
        private const string m_ExpressionBuilderPrefix = "SPConfigStore";

        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            return;

            bool bApplyChanges = ApplyWebConfigMods(properties);
            SPSite currentSite = properties.Feature.Parent as SPSite;

            if (bApplyChanges)
            {
                string sExpressionBuilderType =
                    string.Format("AlphaMosaik.SharePoint.ConfigurationStore.ConfigStoreExpressionBuilder, {0}",
                                  Assembly.GetExecutingAssembly().FullName);

                // add web.config entries..
                SPWebConfigModification siteEntry = addAppSetting(m_SiteUrlModification, m_AppSettingsXPath,
                                                                  m_ConfigSiteAppSettingsKey,
                                                                  currentSite.Url, 100);
                SPWebConfigModification webEntry = addAppSetting(m_WebNameModification, m_AppSettingsXPath,
                                                                 m_ConfigWebAppSettingsKey,
                                                                 string.Empty, 200);
                SPWebConfigModification listEntry = addAppSetting(m_ListNameModification, m_AppSettingsXPath,
                                                                  m_ConfigListAppSettingsKey,
                                                                  m_DefaultListName, 300);
                SPWebConfigModification cacheFileEntry = addAppSetting(m_CacheFileModification, m_AppSettingsXPath,
                                                                       m_CacheFileAppSettingsKey,
                                                                       m_CacheDependencyFile, 400);
                SPWebConfigModification expressionBuilderEntry = addExpressionBuilder(m_ExpressionBuilderModification,
                                                                                      m_ExpressionBuildersXPath,
                                                                                      m_ExpressionBuilderPrefix,
                                                                                      sExpressionBuilderType, 500);

                SPWebApplication currentWebApp = currentSite.WebApplication;
                currentWebApp.WebConfigModifications.Add(siteEntry);
                currentWebApp.WebConfigModifications.Add(listEntry);
                currentWebApp.WebConfigModifications.Add(webEntry);
                currentWebApp.WebConfigModifications.Add(cacheFileEntry);
                currentWebApp.WebConfigModifications.Add(expressionBuilderEntry);

                currentWebApp.WebService.ApplyWebConfigModifications();
                currentWebApp.Update();
            }

            // Déjà fait
            //// and also add list event receivers..
            //SPWeb rootWeb = currentSite.RootWeb;

            //SPList configStoreList = null;
            //try
            //{
            //    configStoreList = rootWeb.Lists[m_DefaultListName];
            //    addEventReceivers(configStoreList);
            //    configStoreList.Update();
            //}
            //catch
            //{
            //    // if we can't find the list we won't try and add event receivers..
            //}
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            return;

            SPSite currentSite = properties.Feature.Parent as SPSite;
            SPWebApplication currentWebApp = currentSite.WebApplication;

            Collection<SPWebConfigModification> colMods = currentWebApp.WebConfigModifications;
            Collection<SPWebConfigModification> colForRemoval = new Collection<SPWebConfigModification>();

            foreach (SPWebConfigModification mod in colMods)
            {
                if (mod.Owner == m_ModificationOwner)
                {
                    colForRemoval.Add(mod);
                }
            }

            foreach (SPWebConfigModification modForRemoval in colForRemoval)
            {
                colMods.Remove(modForRemoval);
            }

            if (colForRemoval.Count > 0)
            {
                currentWebApp.WebService.ApplyWebConfigModifications();
                currentWebApp.Update();
            }
        }

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}

        private static bool ApplyWebConfigMods(SPFeatureReceiverProperties properties)
        {
            bool bApplyChanges = true;
            SPFeaturePropertyCollection featureProps = properties.Feature.Properties;
            if (featureProps != null && featureProps.Count > 0)
            {
                SPFeatureProperty applyModsProp = null;
                try
                {
                    applyModsProp = featureProps["ApplyWebConfigModifications"];

                    if (applyModsProp != null)
                    {
                        bool.TryParse(applyModsProp.Value, out bApplyChanges);
                    }
                }
                catch (Exception)
                {
                    // we'll go ahead and make the web.config changes if the property is not present..
                }
            }
            return bApplyChanges;
        }

        // Pas nécessaire
        //private void addEventReceivers(SPList configList)
        //{
        //    string sAssemblyName = Assembly.GetExecutingAssembly().FullName;
        //    string sClassName = typeof(ConfigStoreListEventReceiver).FullName;

        //    configList.EventReceivers.Add(SPEventReceiverType.ItemUpdated, sAssemblyName, sClassName);
        //    configList.EventReceivers.Add(SPEventReceiverType.ItemAdded, sAssemblyName, sClassName);
        //    configList.EventReceivers.Add(SPEventReceiverType.ItemDeleted, sAssemblyName, sClassName);
        //}

        private static SPWebConfigModification addAppSetting(string sModificationName, string sXpath, string sKey, string sValue, uint iSequence)
        {
            var configMod = new SPWebConfigModification(sModificationName, sXpath)
                {
                    Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                    Name = string.Format("add[@key='{0}']", sKey),
                    Owner = m_ModificationOwner,
                    Value = string.Format("<add key=\"{0}\" value=\"{1}\" />", sKey, sValue),
                    Sequence = iSequence
                };

            return configMod;
        }

        private static SPWebConfigModification addExpressionBuilder(string sModificationName, string sXpath, string sPrefix, string sType, uint iSequence)
        {
            var configMod = new SPWebConfigModification(sModificationName, sXpath)
                {
                    Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                    Name = string.Format("add[@expressionPrefix='{0}']", sPrefix),
                    Owner = m_ModificationOwner,
                    Value = string.Format("<add expressionPrefix=\"{0}\" type=\"{1}\" />", sPrefix, sType),
                    Sequence = iSequence
                };

            return configMod;
        }
    }
}
