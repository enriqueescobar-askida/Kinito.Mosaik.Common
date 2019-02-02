using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AlphaMosaik.Logger.Administration.Extensions;

namespace AlphaMosaik.Logger.Administration.WebControls
{
    public class AlphaLogProviderSection : UserControl
    {
        public bool IsProviderEnabled
        {
            get
            {
                this.EnsureChildControls();
                return ChkBoxAlphaLogProviderEnabled.Checked;
            }
            set
            {
                this.EnsureChildControls();
                ChkBoxAlphaLogProviderEnabled.Checked = value;
            }
        }

        public bool IsCustomFilePathEnabled
        {
            get
            {
                EnsureChildControls();
                return RadCustomFilePath.Checked;
            }
            set
            {
                EnsureChildControls();
                RadCustomFilePath.Checked = value;
                TxBoxCustomFilePath.Enabled = value;
            }
        }

        public string CustomFilePath
        {
            get
            {
                EnsureChildControls();
                return TxBoxCustomFilePath.Text;
            }
            set
            {
                EnsureChildControls();
                TxBoxCustomFilePath.Text = value;
            }
        }

        protected CheckBox ChkBoxAlphaLogProviderEnabled;
        protected TextBox TxBoxCustomFilePath;
        protected RadioButton RadCustomFilePath;
        protected RadioButton RadDefaultFilePath;
        protected RequiredFieldValidator ReqValCustomFilePath;

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            ChkBoxAlphaLogProviderEnabled.Attributes["onclick"] = "javascript:ProviderEnabledChanged_" + this.ClientID + "();";
            RadCustomFilePath.Attributes["onclick"] = "javascript:LogFileModeChanged_" + this.ClientID + "();";
            RadDefaultFilePath.Attributes["onclick"] = "javascript:LogFileModeChanged_" + this.ClientID + "();";

            if(IsPostBack)
            {
                if(!ChkBoxAlphaLogProviderEnabled.Checked)
                {
                    ReqValCustomFilePath.Enabled = false;
                }
            }
        }
    }
}
