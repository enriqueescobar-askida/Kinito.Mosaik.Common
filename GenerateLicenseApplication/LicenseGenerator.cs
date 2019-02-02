// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LicenseGenerator.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the LicenseGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Windows.Forms;
using Alphamosaik.Common.Library.Licensing;

namespace GenerateLicenseApplication
{
    using Alphamosaik.Common.Library;

    public partial class LicenseGenerator : Form
    {
        public LicenseGenerator()
        {
            InitializeComponent();
        }

        private static void WriteLicenseKey(string licenceKey, string fileName)
        {
            try
            {
                var f7 = new FileInfo(fileName);

                if (f7.Exists)
                {
                    f7.Delete();
                }

                // Create a file to write to.
                using (f7.CreateText())
                {
                }

                using (StreamWriter swriterAppend = f7.AppendText())
                {
                    swriterAppend.WriteLine(licenceKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void OnButtonImportFromFileClick(object sender, EventArgs e)
        {
            var fdlg = new OpenFileDialog
                           {
                               Title = @"License Open File Dialog",
                               InitialDirectory = @"c:\",
                               Filter = @"key files (*.key)|*.key|All files (*.*)|*.*",
                               FilterIndex = 1,
                               RestoreDirectory = true
                           };

            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                string licenseKey = File.ReadAllText(fdlg.FileName);
                licenseKey = licenseKey.Replace("\r\n", string.Empty);
                textBoxClientKey.Text = licenseKey;
            }
        }

        private void OnButtonGenerateLicenseClick(object sender, EventArgs e)
        {
            if (ValidateChildren())
            {
                var license = new License
                                  {
                                      Type = (License.LicenseType)Enum.Parse(typeof(License.LicenseType), comboBoxLicenseType.Text),
                                      TrialStart = dateTimePickerStartDate.Value,
                                      TrialEnd = dateTimePickerEndDate.Value
                                  };

                if ((license.Type == License.LicenseType.Prod || license.Type == License.LicenseType.Dev) && !checkBoxUseDateRange.Checked)
                {
                    license.TrialStart = DateTime.Now + TimeSpan.FromDays(-36500);
                    license.TrialEnd = DateTime.Now + TimeSpan.FromDays(36500);
                }

                string generateLicense = license.GenerateLicense(textBoxClientKey.Text);

                var fdlg = new FolderBrowserDialog();

                if (fdlg.ShowDialog() == DialogResult.OK)
                {
                    string fullPath = Path.Combine(fdlg.SelectedPath, "license.dat");
                    WriteLicenseKey(generateLicense, fullPath);
                }
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            // Setup the binding as follows: 
            // MyValues is the enum type 
            comboBoxLicenseType.DataSource = Enum.GetValues(typeof(License.LicenseType)); 
        }

        private void OnTextBoxClientKeyValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxClientKey.Text))
            {
                MessageBox.Show(@"Client key must have a value", @"ERROR");
                e.Cancel = true;
            }
        }

        private void ComboBoxLicenseTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            var licenseType = (License.LicenseType)Enum.Parse(typeof(License.LicenseType), comboBoxLicenseType.Text);

            checkBoxUseDateRange.Visible = licenseType == License.LicenseType.Prod || licenseType == License.LicenseType.Dev;
        }

        private void TextBoxClientKeyTextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxClientKey.Text))
            {
                string decodeFromBase64 = StringUtilities.DecodeFromBase64(this.textBoxClientKey.Text);
                string crypt = StringUtilities.Crypt(decodeFromBase64, License.PassPhrase, false);

                ClientKeyDecrypted.Text = crypt;
            }
        }

        private void LoadLicenceButtonClick(object sender, EventArgs e)
        {
            if (ValidateChildren())
            {
                var fdlg = new OpenFileDialog
                {
                    Title = @"License Open File Dialog",
                    InitialDirectory = @"c:\",
                    Filter = @"key files (*.dat)|*.dat|All files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (fdlg.ShowDialog() == DialogResult.OK)
                {
                    string licenseDat = File.ReadAllText(fdlg.FileName);
                    licenseDat = licenseDat.Replace("\r\n", string.Empty);

                    var license = new License(textBoxClientKey.Text, licenseDat);

                    comboBoxLicenseType.SelectedIndex = comboBoxLicenseType.FindString(license.Type.ToString());

                    dateTimePickerStartDate.Value = license.TrialStart;
                    dateTimePickerEndDate.Value = license.TrialEnd;
                }
            }
        }
    }
}
