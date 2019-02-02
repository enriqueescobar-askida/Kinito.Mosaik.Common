using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using AlphaMosaik.Logger.Administration.Extensions;
using AlphaMosaik.Logger.Storage;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.WebControls;
using AlphaMosaik.Logger.Configuration;

namespace AlphaMosaik.Logger.Administration.ApplicationPages
{
    public class ConfigurePage : OperationsPage
    {
        protected WebApplicationSelector WebAppSelector;
        protected Label LabelErrorMessage;
        protected SPGridView GridViewProviders;
        protected InputFormCheckBox ChkBoxLoggerEnabled;

        protected StorageManager currentStorageManager;
        protected List<int> providersToUpdate;

        private bool providerChangesFlag = false;
        private bool loggerChangesFlag = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (providersToUpdate == null)
                providersToUpdate = new List<int>();

            if(ViewState["providerChangesFlag"] != null)
            {
                providerChangesFlag = (bool) ViewState["providerChangesFlag"];
            }
            if(ViewState["loggerChangesFlag"] != null)
            {
                loggerChangesFlag = (bool) ViewState["loggerChangesFlag"];
            }
            if(ViewState["providersToUpdate"] != null)
            {
                providersToUpdate = (List<int>) ViewState["providersToUpdate"];
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();


            if (!IsPostBack)
            {
                if (WebAppSelector.CurrentItem == null)
                {
                    LabelErrorMessage.Text = "Please select a web application."; // TODO : resx
                }
                else
                {
                    InitGridView();
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            ViewState.Add("providerChangesFlag", providerChangesFlag);
            ViewState.Add("loggerChangesFlag", loggerChangesFlag);
            ViewState.Add("providersToUpdate", providersToUpdate);
            base.OnPreRender(e);
        }

        protected void InitGridView()
        {
            try
            {
                currentStorageManager = StorageManager.Lookup(WebAppSelector.CurrentItem); // Pas la peine de stocker le storageManager dans le ViewState, il y a déjà une gestion de la mise en cache du côté StorageManager.
                ChkBoxLoggerEnabled.Checked = true; // Si le lookup ne génère pas d'exception c'est que le logger est activé pour la web application

                if (currentStorageManager.StorageProviders.Count > 0)
                {
                    GridViewProviders.DataSource = currentStorageManager.StorageProviders;
                    GridViewProviders.DataBind();
                }
                else
                {
                    LabelErrorMessage.Text = (string)GetGlobalResourceObject("AlphaMosaikLogger", "ConfigurePage_NoProviderAvailable");
                }
            }
            catch (LoggerConfigurationException ex)
            {
                LabelErrorMessage.Text = ex.Message;
            }
            catch (LoggerNotEnabledException)
            {
                LabelErrorMessage.Text = "The logger is not enabled on this web application."; // TODO : resx
                ChkBoxLoggerEnabled.Checked = false;
            }
        }

        protected void BtSubmitClick(object sender, EventArgs args)
        {
            if(loggerChangesFlag)
            {
                if (ChkBoxLoggerEnabled.Checked)
                {
                    ConfManager.EnableLogger(WebAppSelector.CurrentItem);
                }
                else
                {
                    ConfManager.EnableLogger(WebAppSelector.CurrentItem);
                }
            }
            if (providerChangesFlag)
            {
                if (providersToUpdate != null && providersToUpdate.Count > 0)
                {
                    if(currentStorageManager == null)
                    {
                        currentStorageManager = StorageManager.Lookup(WebAppSelector.CurrentItem);
                    }
                    foreach (int providerIdx in providersToUpdate)
                    {
                        currentStorageManager.StorageProviders[providerIdx].SaveSettings(WebAppSelector.CurrentItem);
                    }
                }
            }
        }

        protected void WebAppSelectorOnContextChange(object sender, EventArgs args)
        {
            if (WebAppSelector != null && WebAppSelector.CurrentItem != null)
            {
                InitGridView();
            }
        }

        protected void StorageProviderEnabledChanged(object sender, EventArgs args)
        {
            providerChangesFlag = true;

            if (providersToUpdate == null)
            {
                providersToUpdate = new List<int>();
            }
            if (currentStorageManager == null)
            {
                currentStorageManager = StorageManager.Lookup(WebAppSelector.CurrentItem);
            }

            CheckBox currentChkBox = (CheckBox)sender;
            GridViewRow currentRow = (GridViewRow)currentChkBox.NamingContainer;
            int idx = currentRow.DataItemIndex;

            IStorageProvider currentProvider = currentStorageManager.StorageProviders[idx];
            currentProvider.Enabled = currentChkBox.Checked;
            providersToUpdate.Add(idx);

        }

        protected void ChkBoxLoggerEnabledCheckChanged(object sender, EventArgs args)
        {
            loggerChangesFlag = true;
        }
    }
}
