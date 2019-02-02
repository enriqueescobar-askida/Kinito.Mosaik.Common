using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AlphaMosaik.Logger.Service;
using AlphaMosaik.Logger.Configuration;

namespace AlphaMosaik.Logger.Debugging.ServiceEmulator
{
    public partial class Form1 : Form
    {
        private LoggerService service;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            service = new LoggerService()
        }

        private void btAddEntry_Click(object sender, EventArgs e)
        {
            AlphaMosaik.Logger.Service.LogEntry entryToLog = new AlphaMosaik.Logger.Service.LogEntry();
            entryToLog.Message = txBoxMessage.Text;

            service.LogEntry(entryToLog);
        }

        private void btEnableLogger_Click(object sender, EventArgs e)
        {
            service.EnableLogger();
        }

        private void btDisableLogger_Click(object sender, EventArgs e)
        {
            service.DisableLogger();
        }
    }
}
