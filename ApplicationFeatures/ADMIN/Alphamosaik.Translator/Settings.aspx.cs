// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.aspx.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SettingsPage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using TranslatorManagement;

namespace Alphamosaik.Translator.ApplicationFeatures.Admin
{
    public partial class SettingsPage : LayoutsPageBase
    {
        private SPWebApplication _selectedWebApp;

        public bool ItemFilteringEnabled
        {
            get
            {
                return ItemFilteringEnabledRad.Checked;
            }

            set
            {
                ItemFilteringEnabledRad.Checked = value ? true : false;
                ItemFilteringDisabledRad.Checked = value == false ? true : false;
            }
        }

        public bool CompletingModeEnabled
        {
            get
            {
                return CompletingModeEnabledRad.Checked;
            }

            set
            {
                CompletingModeEnabledRad.Checked = value ? true : false;
                CompletingModeDisabledRad.Checked = value == false ? true : false;
            }
        }

        public string AutoTranslatedMessage
        {
            get
            {
                return AutoTranslatedMessageTxBox.Text;
            }

            set
            {
                AutoTranslatedMessageTxBox.Text = value;
            }
        }

        public bool ReplaceLinkedPagesUrlEnabled
        {
            get
            {
                return ReplaceLinkedPagesUrlEnabledRad.Checked;
            }

            set
            {
                ReplaceLinkedPagesUrlEnabledRad.Checked = value ? true : false;
                ReplaceLinkedPagesUrlDisabledRad.Checked = value == false ? true : false;
            }
        }

        public bool TranslationExtractorToolEnabled
        {
            get
            {
                return TranslationExtractorToolEnabledRad.Checked;
            }

            set
            {
                TranslationExtractorToolEnabledRad.Checked = value ? true : false;
                TranslationExtractorToolDisabledRad.Checked = value == false ? true : false;
            }
        }

        public bool CssBannerEnabled
        {
            get
            {
                return CssBannerEnabledRad.Checked;
            }

            set
            {
                CssBannerEnabledRad.Checked = value ? true : false;
                CssBannerDisabledRad.Checked = value == false ? true : false;
            }
        }

        public bool PipeSymbolsEnabled
        {
            get
            {
                return PipeSymbolsEnabledRad.Checked;
            }

            set
            {
                PipeSymbolsEnabledRad.Checked = value ? true : false;
                PipeSymbolsDisabledRad.Checked = value == false ? true : false;
            }
        }

        public bool ItemDashboardEnabled
        {
            get
            {
                return ItemDashboardEnabledRad.Checked;
            }

            set
            {
                ItemDashboardEnabledRad.Checked = value ? true : false;
                ItemDashboardDisabledRad.Checked = value == false ? true : false;
            }
        }

        public bool FilteringButtonEnabled
        {
            get
            {
                return FilteringButtonEnabledRad.Checked;
            }

            set
            {
                FilteringButtonEnabledRad.Checked = value ? true : false;
                FilteringButtonDisabledRad.Checked = value == false ? true : false;
            }
        }

        public bool QuickLaunchFilteringEnabled
        {
            get
            {
                return QuickLaunchFilteringEnabledRad.Checked;
            }

            set
            {
                QuickLaunchFilteringEnabledRad.Checked = value ? true : false;
                QuickLaunchFilteringDisabledRad.Checked = value == false ? true : false;
            }
        }

        public void TranslationServiceSectionEvent(object sender, EventArgs e)
        {
            if (TranslationServiceTypeDdl.SelectedItem != null)
            {
                string authType = "anonymous";
                if (TranslationServiceAuthLoginRad.Checked)
                {
                    authType = "account";
                }

                if (TranslationServiceTypeDdl.SelectedItem.Value == "none")
                {
                    InitTranslationServiceSection("none", authType);
                }
                else if (TranslationServiceTypeDdl.SelectedItem.Value == "bing")
                {
                    InitTranslationServiceSection("bing", authType);
                }
                else if (TranslationServiceTypeDdl.SelectedItem.Value == "systran")
                {
                    InitTranslationServiceSection("systran", authType);
                }
            }
        }

        public void BtnSettingsClick(object sender, EventArgs e)
        {
            _selectedWebApp = WebAppSelector.CurrentItem;

            SaveSettings();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            BtnSettings.Click += BtnSettingsClick;
            BtnSettingsTop.Click += BtnSettingsClick;
            TranslationServiceTypeDdl.SelectedIndexChanged += TranslationServiceSectionEvent;
            TranslationServiceAuthAnonymousRad.CheckedChanged += TranslationServiceSectionEvent;
            TranslationServiceAuthLoginRad.CheckedChanged += TranslationServiceSectionEvent;

            if (!IsPostBack)
            {
                if (WebAppSelector.CurrentItem != null)
                {
                    _selectedWebApp = WebAppSelector.CurrentItem;
                    CreateSettingsControls();
                }
            }
        }

        private void CreateSettingsControls()
        {
            string strItemFiltering = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.ItemFiltering);
            ItemFilteringEnabled = bool.Parse(strItemFiltering);

            string strCompletingMode = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.CompletingMode);
            CompletingModeEnabled = bool.Parse(strCompletingMode);

            AutoTranslatedMessage = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.AutomaticTranslationMessage);

            string strReplacedLinked = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.LinkedPages);
            ReplaceLinkedPagesUrlEnabled = bool.Parse(strReplacedLinked);

            string strExtractorTool = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.ExtractorTool);
            TranslationExtractorToolEnabled = bool.Parse(strExtractorTool);

            string strPipe = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.ShowPipe);
            PipeSymbolsEnabled = bool.Parse(strPipe);

            string strCssBanner = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.CssBanner);
            CssBannerEnabled = bool.Parse(strCssBanner);

            string strItemDashboard = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.ItemDashboard);
            ItemDashboardEnabled = bool.Parse(strItemDashboard);

            string strFilteringButton = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.FilteringButton);
            FilteringButtonEnabled = bool.Parse(strFilteringButton);

            string strQuickLaunchFiltering = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.QuicklaunchFiltering);
            QuickLaunchFilteringEnabled = bool.Parse(strQuickLaunchFiltering);

            // TRANSLATION SERVICE SETTINGS
            string strTranslationServiceType = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceType).ToLower();
            string strTranslationServiceAuth = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceAuthentication);

            InitTranslationServiceSection(strTranslationServiceType, strTranslationServiceAuth); // Enable or disable controls

            if (strTranslationServiceType != "none")
            {
                string strTranslationServiceName = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceName);
                string strTranslationServiceUrl = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceUrl);
                
                string strTranslationServiceUser = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceUsername);
                string strTranslationServicePass = ConfigurationManager.GetSetting(_selectedWebApp, ConfigurationManager.TranslationServicePassword);

                TranslationServiceNameTxBox.Text = strTranslationServiceName;
                TranslationServiceUrlTxBox.Text = strTranslationServiceUrl;
                TranslationServiceUsernameTxBox.Text = strTranslationServiceUser;
                TranslationServicePasswordTxBox.Text = strTranslationServicePass;
            }
        }

        private void SaveSettings()
        {
            if (_selectedWebApp != null)
            {
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.AutomaticTranslationMessage, AutoTranslatedMessage);
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.CompletingMode, CompletingModeEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.CssBanner, CssBannerEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.ExtractorTool, TranslationExtractorToolEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.FilteringButton, FilteringButtonEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.ItemDashboard, ItemDashboardEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.ItemFiltering, ItemFilteringEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.LinkedPages, ReplaceLinkedPagesUrlEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.QuicklaunchFiltering, QuickLaunchFilteringEnabled.ToString());
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.ShowPipe, PipeSymbolsEnabled.ToString());

                // UNDONE : save translation service settings
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceType, TranslationServiceTypeDdl.SelectedValue ?? "none");
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceName, TranslationServiceNameTxBox.Text);
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceUrl, TranslationServiceUrlTxBox.Text);
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceBingApplicationid, TranslationServiceBingApplicationIdTextBox.Text);

                if (TranslationServiceAuthAnonymousRad.Checked)
                {
                    ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceAuthentication, "anonymous");
                }
                else if (TranslationServiceAuthLoginRad.Checked)
                {
                    ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceAuthentication, "account");
                }

                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServiceUsername, TranslationServiceUsernameTxBox.Text);
                ConfigurationManager.SetSetting(_selectedWebApp, ConfigurationManager.TranslationServicePassword, TranslationServicePasswordTxBox.Text);

                ConfigurationManager.SaveChanges(_selectedWebApp);
            }
        }

        private void InitTranslationServiceSection(string serviceType, string authType)
        {
            // Enable / disable controls depending of service type
            if (serviceType == "none")
            {
                TranslationServiceNameTxBox.Enabled = false;
                TranslationServiceUrlTxBox.Enabled = false;
                TranslationServiceBingApplicationIdTextBox.Enabled = false;
                TranslationServiceBingApplicationIdTextBox.Visible = false;
                TranslationServiceAuthAnonymousRad.Enabled = false;
                TranslationServiceAuthLoginRad.Enabled = false;
                TranslationServiceUsernameTxBox.Enabled = false;
                TranslationServicePasswordTxBox.Enabled = false;
            }
            else if (serviceType == "bing" || serviceType == "systran")
            {
                TranslationServiceNameTxBox.Enabled = true;
                TranslationServiceUrlTxBox.Enabled = true;
                TranslationServiceAuthAnonymousRad.Enabled = true;
                TranslationServiceAuthLoginRad.Enabled = true;
                TranslationServiceUsernameTxBox.Enabled = true;
                TranslationServicePasswordTxBox.Enabled = true;
                TranslationServiceBingApplicationIdTextBox.Enabled = false;
            }
            
            if (serviceType == "bing")
            {
                TranslationServiceBingApplicationIdTextBox.Enabled = true;
                TranslationServiceBingApplicationIdTextBox.Enabled = true;
            }

            // Enable login controls depending of authentication type
            if (authType == "anonymous")
            {
                TranslationServiceAuthAnonymousRad.Checked = true;
                TranslationServiceAuthLoginRad.Checked = false;
                TranslationServiceUsernameTxBox.Enabled = false;
                TranslationServicePasswordTxBox.Enabled = false;
            }
            else if (authType == "account")
            {
                TranslationServiceAuthAnonymousRad.Checked = false;
                TranslationServiceAuthLoginRad.Checked = true;
                TranslationServiceUsernameTxBox.Enabled = true;
                TranslationServicePasswordTxBox.Enabled = true;
            }
        }
    }
}
