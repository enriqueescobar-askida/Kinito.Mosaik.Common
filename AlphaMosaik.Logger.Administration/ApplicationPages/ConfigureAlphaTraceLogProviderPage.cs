using System;
using System.Linq;
using System.Web.UI.WebControls;
using AlphaMosaik.Logger.Administration.Extensions;
using AlphaMosaik.Logger.Storage;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Administration;
using AlphaMosaik.Logger.Administration.WebControls;

namespace AlphaMosaik.Logger.Administration.ApplicationPages
{
    public class ConfigureAlphaTraceLogProviderPage : OperationsPage
    {
        protected AlphaLogProviderSection AlphaTraceLogProviderSection;
        protected Label LabelMessage;
        protected Label LabelErrorMessage;

        private SPWebApplication SelectedWebApplication;
        private StorageManager storageManager;
        private AlphaLogStorageProvider provider;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Request.QueryString.Get("WebApplicationId") != null)
            {
                Guid webAppId = new Guid(Request.QueryString.Get("WebApplicationId"));
                SelectedWebApplication = SPFarm.Local.GetObject(webAppId) as SPWebApplication;

                if (SelectedWebApplication != null)
                {
                    storageManager = StorageManager.Lookup(SelectedWebApplication);
                    provider = (from p in storageManager.StorageProviders
                                where p.Name == "AlphaLogStorageProvider"
                                select p).First() as AlphaLogStorageProvider;    // TODO : trouver un meilleur moyen d'accès au provider (par le type?)
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (!IsPostBack)
            {
                EnsureChildControls();
                AlphaTraceLogProviderSection.IsProviderEnabled = provider.Enabled;
                AlphaTraceLogProviderSection.IsCustomFilePathEnabled = true;
                AlphaTraceLogProviderSection.CustomFilePath = provider.LogFilePath;
            }
        }

        protected void BtSubmitClick(object sender, EventArgs e)
        {
            provider.Enabled = this.AlphaTraceLogProviderSection.IsProviderEnabled;
            provider.LogFilePath = this.AlphaTraceLogProviderSection.CustomFilePath;

            provider.SaveSettings(SelectedWebApplication);

            Response.Redirect("~/_admin/AlphaMosaik.Logger/Configure.aspx");
        }
    }
}
