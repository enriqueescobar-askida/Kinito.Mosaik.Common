using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AlphaMosaik.Logger.Administration.Extensions;
using Microsoft.SharePoint.Administration;

namespace AlphaMosaik.Logger.Administration.WebControls
{
    public class SpLogProviderSection : UserControl
    {
        public SPWebApplication SelectedWebApplication { get; set; }
        public bool IsProviderEnabled
        {
            get
            {
                this.EnsureChildControls();
                return ChkBoxSpLogProviderEnabled.Checked;
            }
            set
            {
                this.EnsureChildControls();
                ChkBoxSpLogProviderEnabled.Checked = value;
            }
        }

        protected CheckBox ChkBoxSpLogProviderEnabled;

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }
    }
}
