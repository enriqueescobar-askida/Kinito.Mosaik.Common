using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Administration;
using AlphaMosaik.Logger.Administration.Extensions;

namespace AlphaMosaik.Logger.Administration.WebControls
{
    public class SqlProviderSection : UserControl
    {
        public bool IsProviderEnabled
        {
            get
            {
                EnsureChildControls();
                return ChkBoxSqlProviderEnabled.Checked;
            }
            set
            {
                EnsureChildControls();
                ChkBoxSqlProviderEnabled.Checked = value;
            }
        }
        public string DatabaseName
        {
            get
            {
                EnsureChildControls();
                return TxBoxDatabaseName.Text;
            }
            set
            {
                EnsureChildControls();
                TxBoxDatabaseName.Text = value;
            }
        }
        public string DatabaseServer
        {
            get
            {
                EnsureChildControls();
                return TxBoxDatabaseServer.Text;
            }
            set
            {
                EnsureChildControls();
                TxBoxDatabaseServer.Text = value;
            }
        }
        public AuthenticationMethod Authentication
        {
            get
            {
                EnsureChildControls();
                if(RadSqlAuth.Checked)
                {
                    return AuthenticationMethod.Sql;
                }
                else
                {
                    return AuthenticationMethod.Windows;
                }
            }
            set
            {
                EnsureChildControls();
                if(value == AuthenticationMethod.Sql)
                {
                    RadSqlAuth.Checked = true;
                    RadWindowsAuth.Checked = false;
                    TxBoxDatabaseAccount.Enabled = true;
                    TxBoxDatabasePassword.Enabled = true;
                }
                else if(value == AuthenticationMethod.Windows)
                {
                    RadWindowsAuth.Checked = true;
                    RadSqlAuth.Checked = false;
                    TxBoxDatabaseAccount.Enabled = false;
                    TxBoxDatabasePassword.Enabled = false;
                }
            }
        }
        public string Username
        {
            get
            {
                EnsureChildControls();
                return TxBoxDatabaseAccount.Text;
            }
            set
            {
                EnsureChildControls();
                TxBoxDatabaseAccount.Text = value;
            }
        }
        public string Password
        {
            get
            {
                EnsureChildControls();
                return TxBoxDatabasePassword.Text;
            }
            set
            {
                EnsureChildControls();
                TxBoxDatabasePassword.Text = value;
            }
        }

        #region Controls

        protected CheckBox ChkBoxSqlProviderEnabled;
        protected TextBox TxBoxDatabaseServer;
        protected RequiredFieldValidator ReqValDatabaseServer;
        protected TextBox TxBoxDatabaseName;
        protected RequiredFieldValidator ReqValDatabaseName;
        protected CustomValidator DatabaseValidator;
        protected RadioButton RadWindowsAuth;
        protected RadioButton RadSqlAuth;
        protected TextBox TxBoxDatabaseAccount;
        protected RequiredFieldValidator ReqValDatabaseAccount;
        protected TextBox TxBoxDatabasePassword;
        protected RequiredFieldValidator ReqValDatabasePassword;

        #endregion

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            ChkBoxSqlProviderEnabled.Attributes["onclick"] = "javascript:InitSqlProviderSectionControls();";
            RadWindowsAuth.Attributes["onclick"] = "javascript:InitSqlProviderSectionControls();";
            RadSqlAuth.Attributes["onclick"] = "javascript:InitSqlProviderSectionControls();";

            if(IsPostBack)
            {
                if (!ChkBoxSqlProviderEnabled.Checked)
                {
                    ReqValDatabaseServer.Enabled = false;
                    ReqValDatabaseName.Enabled = false;

                    if(!RadSqlAuth.Checked)
                    {
                        ReqValDatabaseAccount.Enabled = false;
                        ReqValDatabasePassword.Enabled = false;
                    }
                } 
            }
        }

        protected void ValidateDatabase(object sender, ServerValidateEventArgs e)
        {
            return;
        }

        public enum AuthenticationMethod
        {
            Windows,
            Sql
        }
    }
}
