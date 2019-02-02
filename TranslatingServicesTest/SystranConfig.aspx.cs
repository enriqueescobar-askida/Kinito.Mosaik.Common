using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using System;
using System.Configuration;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Web.Configuration;
using System.IO;
using System.Xml.Serialization;

namespace TranslatingServicesTest
{
    [Serializable]
    public class SystranConfigSettings
    {
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public string ServiceURL { get; set; }
        public bool Authenticate { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public partial class SystranConfig: LayoutsPageBase
    {
        private SystranConfigSettings _SystranCfg;
        private string DefaultProvider;

        protected DropDownList lstServiceTypes;

        protected DropDownList lstTranslationProviders;
        protected TextBox txtSystranSvcName;
        protected TextBox txtServiceURL;
        protected RadioButtonList rblUserType;
        protected Panel pnlUserAuthentication;
        protected TextBox txtAuthUserName;
        protected TextBox txtAuthPassword;
        protected TextBox txtAuthPassConfirm;
        protected Button btnConfirm;
        protected Button btnCancel;
        protected Button btnConnectionTest;
        protected Label lblStatus;

        protected MultiView mvwProviders;
        protected View vwSystran;
        protected View vwBing;
      
        protected void Page_Init(object sender, EventArgs e)
        {
            lstTranslationProviders.SelectedIndexChanged += new EventHandler(lstTranslationProviders_SelectedIndexChanged);
            rblUserType.SelectedIndexChanged += new EventHandler(rblUserType_SelectedIndexChanged);
            btnConfirm.Click += new EventHandler(btnConfirm_Click);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnConnectionTest.Click += new EventHandler(btnConnectionTest_Click);
        }

        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                _SystranCfg = LoadServiceConfig();
                UpdateControls(_SystranCfg);
                ViewState["_SystranCfg"] = _SystranCfg;
            }
            else
            {
                try
                {
                    _SystranCfg = (SystranConfigSettings)ViewState["_SystranCfg"];
                    
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Exception: " + ex.Message;
                }
            }
        }

        void UpdateConfig(SystranConfigSettings config)
        {
            try
            {
                config.ServiceType = lstServiceTypes.SelectedIndex.ToString();
                config.ServiceName = txtSystranSvcName.Text;
                config.ServiceURL = txtServiceURL.Text;
                if (rblUserType.SelectedIndex==0)
                {
                    config.Authenticate = false;
                    config.UserName = string.Empty;
                    config.Password = string.Empty;
                }
                else
                {
                    config.Authenticate = true;
                    config.UserName = txtAuthUserName.Text;
                    config.Password = txtAuthPassword.Text;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = string.Format("Exception in UpdateConfig: {0}", ex.Message);
            }
            
        }

        void UpdateControls(SystranConfigSettings config)
        {
            try
            {
                lstTranslationProviders.SelectedIndex = lstTranslationProviders.Items.IndexOf(lstTranslationProviders.Items.FindByValue(DefaultProvider));
                DisplayView(DefaultProvider);
                txtSystranSvcName.Text = config.ServiceName; 
                txtServiceURL.Text = config.ServiceURL;
                if (config.Authenticate == false)
                {
                    rblUserType.SelectedIndex = 0; 
                    pnlUserAuthentication.Visible = false; 
                }
                else
                {
                    rblUserType.SelectedIndex = 1; 
                    pnlUserAuthentication.Visible = true;
                    txtAuthUserName.Text = config.UserName;
                    txtAuthPassword.Text = config.Password;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text+=string.Format("<br>Exception in UpdateControls: {0}",ex.Message);
            }
        }

        private void DisplayView(string Value)
        {
            if (Value == "SYSTRAN")
                mvwProviders.SetActiveView(vwSystran);
            else if (Value == "BING")
                mvwProviders.SetActiveView(vwBing);
        }

        SystranConfigSettings LoadServiceConfig()
        {
            SystranConfigSettings res = new SystranConfigSettings();
            try
            {
                using (SPWeb sWeb = SPContext.Current.Site.RootWeb)
                {
                    if (sWeb.AllProperties.Contains("Alpha.OnlineProvider"))
                    {
                        DefaultProvider = sWeb.AllProperties["Alpha.OnlineProvider"].ToString();
                        lstTranslationProviders.SelectedIndex = lstTranslationProviders.Items.IndexOf(lstTranslationProviders.Items.FindByValue(DefaultProvider));
                    }
                    if (sWeb.AllProperties.ContainsKey("Alpha.SystranConfig"))
                    {
                        string sConfig = (string)sWeb.AllProperties["Alpha.SystranConfig"];
                        byte[] bytes = Convert.FromBase64String(sConfig);
                        MemoryStream memStream = new MemoryStream(bytes);
                        XmlSerializer xmlSerializer = new XmlSerializer(res.GetType());
                        res = xmlSerializer.Deserialize(memStream) as SystranConfigSettings;
                    }
                    if (sWeb.AllProperties.ContainsKey("Alpha.BingConfig"))
                    {
                        //TODO:Add code for BING
                        string sBing = Convert.FromBase64String((string)sWeb.AllProperties["Alpha.BingConfig"]).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text += string.Format("Exception loading config file: {0} ", ex.Message);
            }
            
            return res;

        }

        bool StoreServiceConfig(SystranConfigSettings config)
        {
            bool res = false;
            try
            {
                using (SPWeb sWeb = SPContext.Current.Site.RootWeb)
                {
                    switch (lstTranslationProviders.SelectedValue)
                    {
                        case "SYSTRAN":
                            MemoryStream memStream = new MemoryStream();
                            XmlSerializer xmlSerializer = new XmlSerializer(config.GetType());
                            xmlSerializer.Serialize(memStream, config);
                            byte[] bytes = memStream.ToArray();
                            sWeb.AllProperties["Alpha.SystranConfig"] = Convert.ToBase64String(bytes);
                            break;
                        case "BING":
                            sWeb.AllProperties["Alpha.BingConfig"] = Convert.ToBase64String(
                                System.Text.Encoding.ASCII.GetBytes(
                                "http://api.microsofttranslator.com/V1/Soap.svc")
                                );
                            break;
                    default:break;
                    }
                    sWeb.AllProperties["Alpha.OnlineProvider"] = lstTranslationProviders.SelectedValue;

                    sWeb.AllowUnsafeUpdates = true;
                    sWeb.Update();
                    sWeb.AllowUnsafeUpdates = false;

                    res = true;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text += string.Format("Exception occured when tried to store the config file: {0} ", ex.Message);
            }
            return res;
        }

        void btnConnectionTest_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void btnConfirm_Click(object sender, EventArgs e)
        {
            if (rblUserType.SelectedIndex == 1)
            {
                if (txtAuthPassword.Text != txtAuthPassConfirm.Text) throw new Exception("The password and its confirmation must match");
            }
            UpdateConfig(_SystranCfg);

            if (StoreServiceConfig(_SystranCfg) == false)
            {
                //lblStatus.Text = string.Format("The configuration settings have not been stored");
            }
            else
            {
                lblStatus.Text = string.Format("The settings have been updated.");
            }
        }

        void rblUserType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblUserType.SelectedValue == "AUTHENTICATED")
            {
                pnlUserAuthentication.Visible=true;
            }
            else if (rblUserType.SelectedValue == "ANONYMOUS" )
            {
                pnlUserAuthentication.Visible=false;
            }
        }

        void lstTranslationProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            DefaultProvider = lstTranslationProviders.SelectedValue;
            DisplayView(DefaultProvider);
        }
    }
}
