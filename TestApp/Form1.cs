using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ListsInstallation;

namespace TestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
           // CLists obj = new CLists("http://localhost:55555", @"C:\Inetpub\wwwroot\wss\VirtualDirectories\55555\translations.txt", "TranslationContents");
           // obj.CreateTranbslatorList();
            test();

        }

        private void test()
        {

            bool x = Convert.ToBoolean("1");

        }
    }
}