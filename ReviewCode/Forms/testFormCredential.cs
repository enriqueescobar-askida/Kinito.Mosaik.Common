using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ReviewCode;
using System.Security;


namespace ReviewCode
{
    public partial class testFormCredential : System.Windows.Forms.Form
    {
        public testFormCredential()
        {
            InitializeComponent();

        }

        private void buttonCredential_Click(object sender, EventArgs e)
        {

            if ((!string.IsNullOrEmpty(textBoxCredentialPassword.Text)) && (!string.IsNullOrEmpty(textBoxUsername.Text)))
            {

                SecureString password = new SecureString();
                char[] passChar = textBoxCredentialPassword.Text.ToCharArray();
                foreach (char item in passChar)
                {
                    password.AppendChar(item);
                }
                Buisness.ReviewCode.Password = password;
                Buisness.ReviewCode.Username = textBoxUsername.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();

            }
            else
            {
                labelPasswordRequire.Text = "Username and Password Require";

            }            
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
