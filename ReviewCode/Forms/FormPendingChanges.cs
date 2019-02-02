using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReviewCode
{
    public partial class FormPendingChanges : System.Windows.Forms.Form
    {
        private delegate void performAction();
        public FormPendingChanges()
        {
            InitializeComponent();
            this.Text = "Envoyer un Code Review pour le projet : "+Buisness.ReviewCode.TfsExtCall.ActiveProjectContext.ProjectName;
           
            InitializeListPendingChanges();
        }

        private void InitializeListPendingChanges()
        {
            checkedListBoxPendingChanges.Items.AddRange(Buisness.ReviewCode.GetPendingChanges().ToArray());
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            int nbItems = checkedListBoxPendingChanges.Items.Count;
            for (int i = 0; i < nbItems; i++)
            {
                checkedListBoxPendingChanges.SetItemCheckState(i, CheckState.Checked);
            }
        }

        private void buttonUnselectAll_Click(object sender, EventArgs e)
        {
            int nbItems = checkedListBoxPendingChanges.Items.Count;
            for (int i = 0; i < nbItems; i++)
            {
                checkedListBoxPendingChanges.SetItemCheckState(i, CheckState.Unchecked);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if (checkedListBoxPendingChanges.CheckedItems.Count > 0 && textBoxDescriptionCodeReview.TextLength > 0)
            {
                List<string> selectedItems = new List<string>();
                foreach (string item in checkedListBoxPendingChanges.CheckedItems)
                {
                    selectedItems.Add(item);
                }
                Buisness.ReviewCode.ListChanges = selectedItems;
                Buisness.ReviewCode.WorkItemDescription = textBoxDescriptionCodeReview.Text.Trim();
                this.Close();
            }
            else
            {
                MessageBox.Show("Merci de bien vouloir sélectionner le ou les fichier(s) \n et décrire brièvement la raison du Code Review");

            }
        }
    }
}
