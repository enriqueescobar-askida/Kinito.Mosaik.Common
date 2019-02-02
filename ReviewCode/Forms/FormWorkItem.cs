using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using EnvDTE80;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Server;


namespace ReviewCode
{
    public partial class FormWorkItem : System.Windows.Forms.Form
    {
        private DTE2 _applicationObject;

        public FormWorkItem(DTE2 appObj)
        {
            InitializeComponent();
            _applicationObject = appObj;
            this.Text = "Selectionner un Code Reviewer";
            //ListingWorkItem();
            ListingUserList();
        }

        private void LoadListingWorkItemProject_Click(object sender, EventArgs e)
        {
            if ((listBoxUserList.SelectedItem != null) && (listBoxUserList.SelectedItems.Count == 1))
            {
                //Call A thread 
                Buisness.ReviewCode.UserDiplayName = listBoxUserList.SelectedItem.ToString();
                this.Close();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Merci de bien vouloir selectionner une personne à affecter au code review");
            }            

        }

        private void ListingWorkItem()
        {            
            ListBoxProjectTFS.Items.Clear();

            List<string> listWorkItem =  Buisness.ReviewCode.GetListWorkItemType();
            foreach (string item in listWorkItem)
            {
                ListBoxProjectTFS.Items.Add(item);
            }
            
            //System.Windows.Forms.MessageBox.Show(Buisness.ReviewCode.ListShelveset());
        }

        private void ListingUserList()
        {
            listBoxUserList.Items.Clear();
            List<string> userList = Buisness.ReviewCode.RetreiveUserList();
            listBoxUserList.Items.AddRange(userList.ToArray());

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonTestUserList_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(Buisness.ReviewCode.GetUserDomainName(listBoxUserList.SelectedItem.ToString())+"\n" + Buisness.ReviewCode.GetUserEmailAddress(listBoxUserList.SelectedItem.ToString()));
        }
    }
}
