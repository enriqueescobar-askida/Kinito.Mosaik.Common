using System;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using AlphaMosaik.Logger.DemoWebPart.LoggerWebService;
using AlphaMosaik.Logger.Storage;
using AlphaMosaik.Logger.DemoWebPart.Extensions;
using Microsoft.SharePoint;
using LoggerWebService = AlphaMosaik.Logger.DemoWebPart.LoggerWebService;
using LoggerCore = AlphaMosaik.Logger;


namespace AlphaMosaik.Logger.DemoWebPart
{

    [Guid("52f86e0a-4878-42c0-9313-7b8332428a29")]
    public class DemoWebPart : Microsoft.SharePoint.WebPartPages.WebPart
    {
        private Control demoWebPartControl;
        private TextBox txBoxMessage;
        private DropDownList ddlLevel;
        private Button btSubmitNewEntry;
        //private DropDownList ddlProviders;
        //private CheckBox chkBoxProviderEnabled;
        //private Button btSubmitEditProvider;
        private CheckBox chkBoxWebService;

        private bool error = false;

        public DemoWebPart()
        {
            this.ExportMode = WebPartExportMode.All;
        }

        /// <summary>
        /// Create all your controls here for rendering.
        /// Try to avoid using the RenderWebPart() method.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (!error)
            {
                try
                {
                    base.CreateChildControls();

                    Controls.Clear();
                    demoWebPartControl = Page.LoadControl(@"/_controltemplates/AlphaMosaik.Logger.DemoWebPartControl.ascx");
                    Controls.Add(demoWebPartControl);

                    txBoxMessage = (TextBox) demoWebPartControl.FindControlRecursive("txBoxMessage");
                    ddlLevel = (DropDownList) demoWebPartControl.FindControlRecursive("ddlLevel");

                    ddlLevel.Items.Add(new ListItem(LogLevel.Assert.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.CriticalEvent.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.Exception.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.High.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.InformationEvent.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.Medium.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.Monitorable.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.Unassigned.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.Unexpected.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.Verbose.ToString()));
                    ddlLevel.Items.Add(new ListItem(LogLevel.WarningEvent.ToString()));

                    //chkBoxWebService = (CheckBox) demoWebPartControl.FindControlRecursive("chkBoxWebService");

                    btSubmitNewEntry = (Button) demoWebPartControl.FindControlRecursive("btSubmitNewEntry");
                    btSubmitNewEntry.Click += new EventHandler(BtSubmitNewEntryClick);

                    //ddlProviders = (DropDownList) demoWebPartControl.FindControlRecursive("ddlProviders");

                    //foreach (IStorageProvider storageProvider in Logger.StorageManager.StorageProviders)
                    //{
                    //    ddlProviders.Items.Add(new ListItem(storageProvider.ToString()));
                    //}
                    //ddlProviders.SelectedIndexChanged += new EventHandler(DdlProvidersSelectedIndexChanged);

                    //chkBoxProviderEnabled = (CheckBox) demoWebPartControl.FindControlRecursive("chkBoxProviderEnabled");
                    
                    //if (!this.Page.IsPostBack)
                    //{
                    //    if (Logger.StorageManager.StorageProviders.Count > 0)
                    //    {
                    //        chkBoxProviderEnabled.Checked = Logger.StorageManager.StorageProviders[0].Enabled;
                    //    }
                    //}

                    //btSubmitEditProvider = (Button)demoWebPartControl.FindControlRecursive("btSubmitEditProvider");
                    //btSubmitEditProvider.Click += new EventHandler(BtSubmitEditProviderEntryClick);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }
        /// <summary>
        /// Ensures that the CreateChildControls() is called before events.
        /// Use CreateChildControls() to create your controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            if (!error)
            {
                try
                {
                    base.OnLoad(e);
                    this.EnsureChildControls();

                    // Your code here...
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        /// <summary>
        /// Clear all child controls and add an error message for display.
        /// </summary>
        /// <param name="ex"></param>
        private void HandleException(Exception ex)
        {
            this.error = true;
            this.Controls.Clear();
            this.Controls.Add(new LiteralControl(ex.Message));
        }

        void BtSubmitEditProviderEntryClick(object sender, EventArgs e)
        {
            //if (ddlProviders.SelectedIndex < Logger.StorageManager.StorageProviders.Count)
            //{
            //    Logger.StorageManager.StorageProviders[ddlProviders.SelectedIndex].Enabled = chkBoxProviderEnabled.Checked;
            //}
        }

        void DdlProvidersSelectedIndexChanged(object sender, EventArgs e)
        {
            //if (ddlProviders.SelectedIndex < Logger.StorageManager.StorageProviders.Count)
            //{
            //    chkBoxProviderEnabled.Checked =
            //        Logger.StorageManager.StorageProviders[ddlProviders.SelectedIndex].Enabled;
            //}
        }

        void BtSubmitNewEntryClick(object sender, EventArgs e)
        {
            try
            {
                //if(chkBoxWebService.Checked)
                if(false)
                {
                    LoggerWebService.LogLevel level = (LoggerWebService.LogLevel)Enum.Parse(typeof(LoggerWebService.LogLevel), ddlLevel.SelectedItem.Text, true);
                    
                    LoggerCore.LogEntry coreEntry = new LogEntry(txBoxMessage.Text);
                    LoggerWebService.LogEntry entry = new LoggerWebService.LogEntry();
                    entry.Level = level;
                    entry.ProductName = coreEntry.ProductName;
                    entry.StrictDateTimeEnabled = coreEntry.StrictDateTimeEnabled;
                    entry.TimeStamp = coreEntry.TimeStamp;
                    entry.Message = coreEntry.Message;

                    LoggerWebService.LoggerWebService webService = new LoggerWebService.LoggerWebService();
                    webService.Url = SPContext.Current.Web.Url.TrimEnd('/') + "/_vti_bin/AlphaMosaikLogger.asmx";
                    webService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    webService.AddEntry(entry);
                }
                else
                {
                    LoggerCore.LogLevel level = (LoggerCore.LogLevel)Enum.Parse(typeof(LoggerCore.LogLevel), ddlLevel.SelectedItem.Text, true);
                    LoggerCore.LogEntry entry = new LoggerCore.LogEntry(txBoxMessage.Text, level);
                    //LoggerCore.Logger.AddEntry(entry);
                }
            }
            catch (ArgumentException ex)
            {
                // TODO : message d'erreur
            }
        }
    }
}
