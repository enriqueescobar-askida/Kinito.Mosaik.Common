// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainForm.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   The state of the services after calling start or stop.
//   If a new state is added remember to add the corresonding message to the GetStateMessage method.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.ComponentModel;
using System.Windows.Forms;
using SP2010ServiceManager.Manager;

namespace SP2010ServiceManager
{
    /// <summary>
    /// The state of the services after calling start or stop.
    /// If a new state is added remember to add the corresonding message to the GetStateMessage method.
    /// </summary>
    public enum ServiceState
    {
        Started,
        StartedWithErrors,
        Stopped,
        StoppedWithErrors
    }

    public partial class MainForm : Form
    {
        private readonly ServiceManager _serviceManager;

        public MainForm()
        {
            InitializeComponent();
            _serviceManager = new ServiceManager(backgroundWorker);
            serviceStatusRefreshTimer.Start();
            Text += @" " + GetSimplifiedVersion();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetManualStartupButtonState();            
        }

        private void serviceStatusRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var services = _serviceManager.GetSharePointServices();
                foreach (var service in services)
                {
                    var items = lvwServices.Items.Find(service.Name, true);
                    if(items.Length > 0)
                    {
                        items[0].ImageIndex = (int) service.Status;
                    }
                    else
                    {
                        lvwServices.Items.Add(service.Name, service.Name, (int)service.Status);
                    }
                }
            }
            catch
            {
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ServiceState state;
            try
            {
                var start = (bool)e.Argument;
                
                if (start)
                {
                    state = _serviceManager.StartServices() ? ServiceState.Started : ServiceState.StartedWithErrors;
                }
                else
                {
                    state = _serviceManager.StopServices() ? ServiceState.Stopped : ServiceState.StoppedWithErrors;
                }
                e.Result = state;
            }
            catch
            {
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            SetRunningState();
            backgroundWorker.RunWorkerAsync(true);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            SetRunningState();
            backgroundWorker.RunWorkerAsync(false);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ShowStatus((string)e.UserState);
        }

        private void ShowStatus(string message)
        {
            txtStatus.Text += message;
            txtStatus.SelectionStart = txtStatus.Text.Length;
            txtStatus.ScrollToCaret();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var message = GetStateMessage((ServiceState) e.Result);
            ShowStatus(message);
            SetIdleState();
            MessageBox.Show(this, message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string GetStateMessage(ServiceState state)
        {
            string message = string.Empty;
            switch (state)
            {
                case ServiceState.Started:
                    message = "SharePoint 2010 has been started.";
                    break;
                case ServiceState.StartedWithErrors:
                    message = "One or more services failed to start.\r\nPlease check the Windows Event Log for errors.";
                    break;
                case ServiceState.Stopped:
                    message = "SharePoint 2010 has been stopped.\r\nYour workstation has more RAM now!";
                    break;
                case ServiceState.StoppedWithErrors:
                    message = "One or more services failed to stop.\r\nPlease check the Windows Event Log for errors.";
                    break;

            }
            return message;
        }
        private void SetRunningState()
        {
            progressBar.Visible = true;
            btnSetManualStartup.Enabled = false;
            txtStatus.Text = string.Empty;
            btnStart.Enabled = false;
            btnStop.Enabled = false;
        }

        private void SetIdleState()
        {
            progressBar.Visible = false;
            btnSetManualStartup.Enabled = (bool)btnSetManualStartup.Tag;
            btnStart.Enabled = true;
            btnStop.Enabled = true;
        }

        private void SetManualStartupButtonState()
        {
            btnSetManualStartup.Tag = true;
            try
            {
                btnSetManualStartup.Visible = !_serviceManager.IsServicesSetToManualStartup();
                btnSetManualStartup.Tag = btnSetManualStartup.Visible;
            }
            catch (Exception)
            {
            }
        }

        private void btnSetManualStartup_Click(object sender, EventArgs e)
        {
            try
            {
                SetRunningState();
                ShowStatus("Changing SharePoint services to startup manually.\r\n");
                _serviceManager.SetToManualStartup();
                SetManualStartupButtonState();
                var message = "All SharePoint services have been set to startup manually.\r\n";
                SetIdleState();
                ShowStatus(message);
                MessageBox.Show(this, message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception)
            {
                SetIdleState();
            }
            
        }

        private static string GetSimplifiedVersion()
        {
            var version = Application.ProductVersion;
            var index = version.LastIndexOf("0.0");
            return version.Substring(0, version.Length - index);
        }
    }
}
