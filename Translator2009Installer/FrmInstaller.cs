// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrmInstaller.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the FrmInstaller type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using InstallHelper;
using Microsoft.SharePoint;
using Translator2009.Installer.Properties;

namespace Translator2009.Installer
{
    using System.ComponentModel;
    using System.ServiceProcess;

    using Microsoft.SharePoint.Administration;

    public partial class FrmInstaller : Form
    {
        private Thread _subThread;

        public enum ApplicationState
        {
            AppStateIdle,
            AppStateActivationInProgress,
            AppStateDeactivationInProgress
        };

        private ApplicationState _currentAppState;
        
        // Install Action
        const string ActionInstallCreateSpList = "Create the Sharepoint List";
        const string ActionInstallCreateSpView = "Create the Sharepoint Views";
        const string ActionInstallLinkEventToList = "Link events to lists";
        const string ActionInstallActivateManagementFeature = "Activate the management feature";

        // Uninstall Action
        const string ActionUninstallDelSpList = "Delete the Sharepoint lists";
        const string ActionUninstallDeactivateManagementFeature = "Deactivate the management feature";

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool _shouldStop;

        public FrmInstaller()
        {
            InitializeComponent();
            txtAppFolder.Text = GetCurrentDirectory();

            _lastUiUpdateTime = DateTime.Now;
            _currentAppState = ApplicationState.AppStateIdle;
        }

        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        public delegate void SetStatusCallback(string text);

        // This delegate enables asynchronous calls
        public delegate void SetActionCallback(string actionName);
        public delegate void SetActionDoneCallback(string actionName);
        public delegate void ResetActionListCallback();
        public delegate void ShowWaitCursorCallback(bool show);
        public delegate void SetApplicationStateCallback(ApplicationState state);
                
        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        public delegate void SetEnableControlCallback(bool isEnabled);

        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox contpublic public rol.
        public delegate void MoveProgressBarCallback(String progressText);

        private DateTime _lastUiUpdateTime;

        /// <summary>
        /// Gets a value indicating whether it detects sharepoint in the system registry
        /// </summary>
        public static bool IsSharePointInstalled
        {
            get
            {
                string value = SharePointRegistry.GetValue("SharePoint");

                if ("Installed".Equals(value))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user running this thread has acces to install packages on this server
        /// </summary>
        public static bool HasInstallationPermissions
        {
            get
            {
                try
                {
                    if (SPFarm.Local.CurrentUserIsAdministrator())
                    {
                        return true;
                    }
                }
                catch (NullReferenceException)
                {
                    return false;
                }
                catch (Exception ee)
                {
                    throw new Exception(ee.Message, ee);
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether it detects tha "Microsoft Sharepoint Services Administration" service is running
        /// </summary>
        public static bool SpAdministrationRunning
        {
            get
            {
                try
                {
                    var adminService = new ServiceController(WindowsServices.Current.SPAdminName);
                    if (adminService.Status == ServiceControllerStatus.Running)
                    {
                        return true;
                    }
                }
                catch (NullReferenceException)
                {
                    return false;
                }
                catch (Win32Exception ee)
                {
                    throw new Exception(ee.Message, ee);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether it detects tha "Microsoft Sharepoint Timer Administration" service is running
        /// </summary>
        public static bool SpTimerRunning
        {
            get
            {
                try
                {
                    var span = new TimeSpan(0, 0, 60);
                    var timerService = new ServiceController(WindowsServices.Current.SPTimerName);
                    if (timerService.Status == ServiceControllerStatus.Running)
                    {
                        timerService.Stop();
                        timerService.WaitForStatus(ServiceControllerStatus.Stopped, span);
                    }

                    timerService.Start();
                    timerService.WaitForStatus(ServiceControllerStatus.Running, span);
                    return true;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
                catch (TimeoutException e)
                {
                    throw new Exception(e.Message, e);
                }
                catch (Win32Exception ee)
                {
                    throw new Exception(ee.Message, ee);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        public static void AjustResourceThrottling(string url)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (var currentSite = new SPSite(url))
                {
                    try
                    {
                        currentSite.WebApplication.AllowOMCodeOverrideThrottleSettings = true;
                        currentSite.WebApplication.MaxItemsPerThrottledOperation = 50000;
                        currentSite.WebApplication.MaxItemsPerThrottledOperationOverride = 50000;

                        currentSite.WebApplication.Update();

                        if (currentSite.WebApplication.MaxItemsPerThrottledOperation < 50000)
                        {
                            throw new Exception("Resource Throttling of the Web Application must be ajusted according to the Oceanik Documentation!");
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("Resource Throttling of the Web Application must be ajusted according to the Oceanik Documentation!");
                    }
                }
            });
        }

        /// <summary>
        /// Does a preflight check to make sure the basic services are running
        /// </summary>
        /// <param name="url">
        /// The url of the site.
        /// </param>
        /// <param name="install">
        /// The install state.
        /// </param>
        public static void GetPrerequisitesErrors(string url, bool install)
        {
            if (!IsSharePointInstalled)
                throw new Exception("SharePoint is not installed on " + Environment.MachineName);

            if (!HasInstallationPermissions)
                throw new Exception("User " + Environment.UserName + " does not have installation permissions on " + Environment.MachineName);

            if (!SpAdministrationRunning)
                throw new Exception("Microsoft SharePoint Services Administration is not running on " + Environment.MachineName);

            if (!SpTimerRunning)
                throw new Exception("Microsoft SharePoint Services Timer is not running on " + Environment.MachineName);

            if (install)
                AjustResourceThrottling(url);
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        private static string GetCurrentDirectory()
        {
            return Application.StartupPath;
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            // Request that the worker thread stop itself:
            RequestStop();
            Application.Exit();
        }

        // This method is passed in to the SetTextCallBack delegate
        // to set the Text property of textBox1.
        private void SetStatus(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lblStatus.InvokeRequired)
            {
                var d = new SetStatusCallback(SetStatus);
                Invoke(d, new object[] { text });
            }
            else
            {
                lblStatus.Text = text;
                lblStatus.Update();
            }
        }

        // This method is passed in to the SetTextCallBack delegate
        // to set the Text property of textBox1.
        private void SetAction(string actionName)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lblAction.InvokeRequired)
            {
                var d = new SetActionCallback(SetAction);
                Invoke(d, new object[] { actionName });
            }
            else
            {
                ListViewItem item = listViewActions.FindItemWithText(actionName);
                if (item != null)
                {
                    item.SubItems[1].Text = "In Progress...";
                }
                MoveProgressBar("Action in progress: " + actionName);
            }
        }

        private void SetActionDone(string actionName)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lblAction.InvokeRequired)
            {
                var d = new SetActionDoneCallback(SetActionDone);
                Invoke(d, new object[] { actionName });
            }
            else
            {
                ListViewItem item = listViewActions.FindItemWithText(actionName);
                if (item != null)
                {
                    item.SubItems[1].Text = "Done !";
                }
            }
        }

        private void ResetActionList()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lblAction.InvokeRequired)
            {
                var d = new ResetActionListCallback(ResetActionList);
                Invoke(d);
            }
            else
            {
                listViewActions.Clear();
                progressBar1.Value = 0;
                progressBar1.Refresh();
            }
        }

        private void AllActionCompleted()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lblAction.InvokeRequired)
            {
                var d = new ResetActionListCallback(ResetActionList);
                Invoke(d);
            }
            else
            {
                progressBar1.Value = 100;
                progressBar1.Refresh();
            }
        }
     
        // This method is passed in to the SetTextCallBack delegate
        // to set the Text property of textBox1.
        private void MoveProgressBar(String progressText)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (progressBar1.InvokeRequired)
            {
                var d = new MoveProgressBarCallback(MoveProgressBar);
                Invoke(d, new object[] { progressText });
            }
            else
            {
                DateTime actualTime = DateTime.Now;
                
                TimeSpan timeSinceLastUpdate = actualTime - _lastUiUpdateTime;

                if (timeSinceLastUpdate.Milliseconds > 250)
                {
                    _lastUiUpdateTime = DateTime.Now;

                    progressBar1.Value += 10;
                    if (progressBar1.Value >= 100)
                    {
                        progressBar1.Value = 0;
                    }

                    progressBar1.Refresh();
                    
                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", (float) 8.25, System.Drawing.FontStyle.Regular);
                    System.Drawing.PointF drawPoint = new System.Drawing.PointF(10, progressBar1.Height/2 - 5);

                    // On limite la longueur de la string
                    if (progressText.Length > 90)
                    {
                        progressText = progressText.Substring(0, 87);
                        progressText += "...";
                    }

                    progressBar1.CreateGraphics().DrawString(progressText, drawFont, System.Drawing.Brushes.Black,
                                                             drawPoint);
                }
            }
        }

        private void ShowWaitCursor(bool show)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lblAction.InvokeRequired)
            {
                var d = new ShowWaitCursorCallback(ShowWaitCursor);
                Invoke(d, new object[] { show });
            }
            else
            {
                if(show)
                {
                    Cursor = Cursors.WaitCursor;                    
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void SetApplicationState(ApplicationState state)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (lblAction.InvokeRequired)
            {
                var d = new SetApplicationStateCallback(SetApplicationState);
                Invoke(d, new object[] { state });
            }
            else
            {
                _currentAppState = state;
            }
        }

        private void EnableControls(bool isEnabled)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (txtAppFolder.InvokeRequired)
            {
                var d = new SetEnableControlCallback(EnableControls);
                Invoke(d, new object[] { isEnabled });
            }
            else
            {
                txtAppFolder.Enabled = isEnabled;
                txtAppFolder.Update();
                txtUrl.Enabled = isEnabled;
                txtUrl.Update();
                cmbDefaultLang.Enabled = isEnabled;
                cmbDefaultLang.Update();

                btnBrowseUrl.Enabled = isEnabled;
                btnCancel.Enabled = isEnabled;
                btnInstall.Enabled = isEnabled;
                btnUninstall.Enabled = isEnabled;
            }
        }

        private bool IsTextBoxEmpty(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                errorProvider1.SetError(textBox, "you should not leave empty spaces, please fill up the fields");
                return true;
            }

            errorProvider1.SetError(textBox, string.Empty);
            return false;
        }

        private bool IsLanguageSelected(ComboBox comboBox)
        {
            const int noItemSelected = -1;

            if (comboBox.SelectedIndex == noItemSelected)
            {
                errorProvider1.SetError(comboBox, "Please select default language.");
                return false;
            }

            errorProvider1.SetError(comboBox, string.Empty);
            return true;
        }

        private void CreateActionsListView(string[] actions)
        {
            ColumnHeader columnHeader1 = new ColumnHeader();
            ColumnHeader columnHeader2 = new ColumnHeader();

            columnHeader1.Text = "Description";
            columnHeader1.Width = 250;
            columnHeader2.Text = "Status";
            columnHeader2.Width = 100;

            listViewActions.Columns.Add(columnHeader1);
            listViewActions.Columns.Add(columnHeader2);

            foreach (string action in actions)
            {
                ListViewItem listViewItem1 = new ListViewItem();
                ListViewItem.ListViewSubItem listViewSubItem1 = new ListViewItem.ListViewSubItem();

                listViewItem1.Text = action;
                listViewSubItem1.Text = "";
                listViewItem1.SubItems.Add(listViewSubItem1);

                listViewActions.Items.Add(listViewItem1);
            }
        }

        private void BtnInstallClick(object sender, EventArgs e)
        {
            try
            {
                if (!IsTextBoxEmpty(txtAppFolder) && !IsTextBoxEmpty(txtUrl) && IsLanguageSelected(cmbDefaultLang))
                {
                    if (txtUrl.Text.EndsWith("/"))
                    {
                        txtUrl.Text = txtUrl.Text.Substring(0, txtUrl.Text.Length - 1);
                    }

                    GetPrerequisitesErrors(txtUrl.Text, true);

                    progressBar1.Maximum = 100;
                    progressBar1.Minimum = 0;
                    progressBar1.Value = 0;
                    progressBar1.Step = 10;

                    string[] actions = { 
                        ActionInstallCreateSpList,
                        ActionInstallCreateSpView,
                        ActionInstallLinkEventToList,
                        ActionInstallActivateManagementFeature };

                    CreateActionsListView(actions);

                    var installationParams = new InstallationParameters(this, txtUrl.Text,
                        txtAppFolder.Text, cmbDefaultLang.Text, cmbDefaultLang.SelectedIndex);

                    _subThread = new Thread(installationParams.SubThreadInstall) { IsBackground = true };
                    _subThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK);
            }
        }

        private void BtnBrowseUrlClick(object sender, EventArgs e)
        {
            folderBrowserDialogUrl.ShowDialog();
            if (!string.IsNullOrEmpty(folderBrowserDialogUrl.SelectedPath))
            {
                txtAppFolder.Text = folderBrowserDialogUrl.SelectedPath;
            }
        }

        private void BtnUninstallClick(object sender, EventArgs e)
        {
            try
            {
                if (!IsTextBoxEmpty(txtAppFolder) && !IsTextBoxEmpty(txtUrl) )
                {
                    if (txtUrl.Text.EndsWith("/"))
                    {
                        txtUrl.Text = txtUrl.Text.Substring(0, txtUrl.Text.Length - 1);
                    }

                    GetPrerequisitesErrors(txtUrl.Text, false);

                    progressBar1.Maximum = 100;
                    progressBar1.Minimum = 0;
                    progressBar1.Value = 0;
                    progressBar1.Step = 10;

                    string[] actions = { 
                        ActionUninstallDelSpList,
                        ActionUninstallDeactivateManagementFeature };

                    CreateActionsListView(actions);

                    var installationParams = new InstallationParameters(this, txtUrl.Text,
                        txtAppFolder.Text, cmbDefaultLang.Text, cmbDefaultLang.SelectedIndex);

                    _subThread = new Thread(installationParams.SubThreadUninstall) { IsBackground = true };
                    _subThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK);
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            // Request that the worker thread stop itself:
            RequestStop();
            Process.GetCurrentProcess().Kill();
        }

        public class InstallationParameters
        {
            private readonly FrmInstaller _frmInstaller;

            private readonly string _txtUrl;
            private readonly string _txtAppFolder;
            private readonly string _cmbDefaultLang;

            private readonly int _cmbDefaultLangIndexSelected;

            public InstallationParameters(FrmInstaller frmInstaller, string txtUrl, string txtAppFolder, string cmbDefaultLang, int cmbDefaultLangIndexSelected)
            {
                _cmbDefaultLangIndexSelected = cmbDefaultLangIndexSelected;
                _cmbDefaultLang = cmbDefaultLang;
                _txtAppFolder = txtAppFolder;
                _frmInstaller = frmInstaller;
                _txtUrl = txtUrl;
            }

            public int CmbDefaultLangIndexSelected
            {
                get { return _cmbDefaultLangIndexSelected; }
            }

            public string CmbDefaultLang
            {
                get { return _cmbDefaultLang; }
            }

            public string TxtAppFolder
            {
                get { return _txtAppFolder; }
            }

            public FrmInstaller FrmInstaller1
            {
                get { return _frmInstaller; }
            }

            public string TxtUrl
            {
                get { return _txtUrl; }
                set { throw new NotImplementedException(); }
            }

            public void MoveProgressBar(string ProgressText)
            {
                FrmInstaller1.MoveProgressBar(ProgressText);
            }


            public void SubThreadInstall()
            {
                var result = MessageBox.Show(Resources.Are_you_sure_you_want_to_activate_the_Translator + TxtUrl + Resources._Web_Application_, Resources._Attention, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (TxtUrl.EndsWith("/"))
                    {
                        TxtUrl = TxtUrl.Substring(0, TxtUrl.Length - 1);
                    }

                    FrmInstaller1.EnableControls(false);
                    FrmInstaller1.SetAction(ActionInstallCreateSpList);
                    FrmInstaller1.SetApplicationState(ApplicationState.AppStateActivationInProgress);
                    FrmInstaller1.ShowWaitCursor(true);
                    
                    try
                    {
                        SPSecurity.RunWithElevatedPrivileges(delegate
                                                        {
                                                            var obj = new InstallerHelper(MoveProgressBar);

                                                        if (!obj.CheckPermissions(TxtUrl))
                                                        {
                                                            MessageBox.Show(Resources.Unable_to_access_ + TxtUrl +
                                                                            Resources.Please_verify_the_url_and_permissions_you_need_to_use_a_Farm_Admin_Account);
                                                        }
                                                        else
                                                        {
                                                            CreateLists(obj);
                                                            FrmInstaller1.SetActionDone(ActionInstallCreateSpList);

                                                            FrmInstaller1.SetAction(ActionInstallCreateSpView);
                                                            CreateViews(obj);
                                                            FrmInstaller1.SetActionDone(ActionInstallCreateSpView);

                                                            FrmInstaller1.SetAction(ActionInstallLinkEventToList);
                                                            obj.LinkCheckDefaultLangEvent(TxtUrl);
                                                            obj.LinkCreateGuidEvent(TxtUrl);
                                                            obj.LinkReloadCacheEvent(TxtUrl);
                                                            FrmInstaller1.SetActionDone(ActionInstallLinkEventToList);

                                                            FrmInstaller1.SetAction(ActionInstallActivateManagementFeature);
                                                            obj.ActivateManagementFeature(TxtAppFolder, TxtUrl);

                                                            Thread.Sleep(8000);
                                                            FrmInstaller1.SetActionDone(ActionInstallActivateManagementFeature);

                                                            FrmInstaller1.MoveProgressBar("Done !");
                                                            FrmInstaller1.AllActionCompleted();

                                                            MessageBox.Show(Resources.Activation_Completed_Successfully);
                                                            FrmInstaller1.ResetActionList();
                                                        }
                                                        });
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(Resources.Activation_Completed_Unsuccessfully + e.Message);
                    }
                    finally
                    {
                        FrmInstaller1.EnableControls(true);
                        FrmInstaller1.RequestStop();
                    }
                }

                FrmInstaller1.MoveProgressBar("");
                FrmInstaller1.ResetActionList();

                FrmInstaller1.ShowWaitCursor(false);
                FrmInstaller1.SetApplicationState(ApplicationState.AppStateIdle);
            }

            public void SubThreadUninstall()
            {
                DialogResult result = MessageBox.Show(Resources.Are_you_sure_you_want_to_deactivate_the_Translator + TxtUrl + Resources._Web_Application_, Resources._Attention, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    FrmInstaller1.EnableControls(false);
                    FrmInstaller1.SetApplicationState(ApplicationState.AppStateDeactivationInProgress);
                    FrmInstaller1.ShowWaitCursor(true);

                    try
                    {
                        SPSecurity.RunWithElevatedPrivileges(delegate
                                                         {
                                                            var obj = new InstallerHelper(MoveProgressBar);

                                                            if (!obj.CheckPermissions(TxtUrl))
                                                            {
                                                                MessageBox.Show(Resources.Unable_to_access_ + TxtUrl +
                                                                                Resources.Please_verify_the_url_and_permissions_you_need_to_use_a_Farm_Admin_Account);
                                                            }
                                                            else
                                                            {
                                                                DialogResult resultRemoveList = MessageBox.Show(@"Do you wish to delete the translator lists on the " + TxtUrl + @" Web Application ?", @"Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                                                if (resultRemoveList == DialogResult.Yes)
                                                                {
                                                                    FrmInstaller1.SetAction(ActionUninstallDelSpList);
                                                                    obj.RemoveLists(TxtUrl);
                                                                    FrmInstaller1.SetActionDone(ActionUninstallDelSpList);
                                                                }

                                                                FrmInstaller1.SetAction(ActionUninstallDeactivateManagementFeature);
                                                                obj.DeActivateManagementFeature(TxtUrl, TxtAppFolder);

                                                                Thread.Sleep(8000);
                                                                FrmInstaller1.SetActionDone(ActionUninstallDeactivateManagementFeature);

                                                                FrmInstaller1.MoveProgressBar("Done !");
                                                                FrmInstaller1.AllActionCompleted();

                                                                MessageBox.Show(Resources.Deactivation_Completed_Successfully);
                                                            }
                                                         });
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(Resources.Deactivation_Completed_Unsuccessfully + e.Message);
                    }
                    finally
                    {
                        FrmInstaller1.EnableControls(true);
                    }
                }

                FrmInstaller1.MoveProgressBar("");
                FrmInstaller1.ResetActionList();
                FrmInstaller1.ShowWaitCursor(false);
                FrmInstaller1.SetApplicationState(ApplicationState.AppStateIdle);
            }

            private void CreateLists(InstallerHelper installerHelper)
            {
                installerHelper.CreateLoadBalancingServersList(TxtUrl);
                installerHelper.CreateConfigurationStoreServersList(TxtUrl);
                installerHelper.CreateTroubleshootingStoreServersList(TxtUrl);

                installerHelper.CreatePageTransList(TxtAppFolder + "\\pagesNotToTranslate.txt", TxtAppFolder + "\\adminPagesToTranslate.txt", TxtUrl);

                // check if the first item is choosed in the drop down list then  the default lang is english else choose the selected text value
                if (CmbDefaultLang == "Choose..." || CmbDefaultLangIndexSelected == 0)
                {
                    installerHelper.CreateLangVisibilityList(TxtUrl, "English");
                }
                else
                {
                    installerHelper.CreateLangVisibilityList(TxtUrl, CmbDefaultLang);
                }

                installerHelper.CreateTranslationList(TxtAppFolder + "\\translations.txt", TxtUrl);
            }

            private void CreateViews(InstallerHelper installerHelper)
            {
                installerHelper.CreateTransListNormView(TxtUrl);
                installerHelper.CreateTransListDataSheetView(TxtUrl);
                installerHelper.CreateTransListNonCustomizeNormalView(TxtUrl);
                installerHelper.CreateTransListNonCustomizeGridView(TxtUrl);
                installerHelper.CreateTransListCustomizeNormalView(TxtUrl);
                installerHelper.CreateTransListCustomizeGridView(TxtUrl);
                installerHelper.CreatePagesTransListNormalView(TxtUrl);
                installerHelper.CreatePagesTransListGridView(TxtUrl);
                installerHelper.CreateLanguagesVisibilityNormalView(TxtUrl);
                installerHelper.CreateLanguagesVisibilityGridView(TxtUrl);
                installerHelper.CreateConfigurationStoreView(TxtUrl);
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            string warningMessage = "";

            switch (_currentAppState)
            {
                case ApplicationState.AppStateIdle:
                    break;

                case ApplicationState.AppStateActivationInProgress:
                    warningMessage = Resources.Cannot_quit_application;
                    break;

                case ApplicationState.AppStateDeactivationInProgress:
                    warningMessage = Resources.Cannot_quit_application;
                    break;

                default:
                    break;
            }

            if (_currentAppState != ApplicationState.AppStateIdle)
            {
                DialogResult result =
                    MessageBox.Show(
                        warningMessage,
                        Resources._Attention,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                // On ne laisse pas le choix lors de l'activation/desactivation
                e.Cancel = true;
            }
        }
    }
}