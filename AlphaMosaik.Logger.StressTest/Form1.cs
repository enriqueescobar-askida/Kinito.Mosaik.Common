using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using AlphaMosaik.Logger.StressTest.Logger;

namespace AlphaMosaik.Logger.StressTest
{
    public partial class Form1 : Form
    {
        private LoggerClient client;

        public Form1()
        {
            InitializeComponent();
            client = new LoggerClient("TcpBasicEndPoint");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!StressWorker.IsBusy)
            {
                StressWorker.RunWorkerAsync();
                button1.Text = "Stop";
            }
            else
            {
                StressWorker.CancelAsync();
                button1.Text = "Stress";
            }
        }

        private void StressWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int max = int.Parse(TextBoxMax.Text);
            int speed = int.Parse(TextBoxSpeed.Text);

            for (int i = 0; i < max; i++)
            {
                if(StressWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                client.LogMessage("Stress test" + i, LogEntryLevel.Information);

                StressWorker.ReportProgress((int)((float)i / max * 100));
                Thread.Sleep(speed);
            }
        }

        private void StressWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void StressWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                MessageBox.Show("Operation cancelled");
            }
            else
            {
                MessageBox.Show("Done");
            }
            progressBar.Value = 0;
        }
    }
}
