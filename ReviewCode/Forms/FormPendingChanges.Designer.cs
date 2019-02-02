namespace ReviewCode
{
    partial class FormPendingChanges
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkedListBoxPendingChanges = new System.Windows.Forms.CheckedListBox();
            this.buttonSubmit = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonUnselectAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDescriptionCodeReview = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkedListBoxPendingChanges
            // 
            this.checkedListBoxPendingChanges.CheckOnClick = true;
            this.checkedListBoxPendingChanges.FormattingEnabled = true;
            this.checkedListBoxPendingChanges.HorizontalScrollbar = true;
            this.checkedListBoxPendingChanges.Location = new System.Drawing.Point(12, 22);
            this.checkedListBoxPendingChanges.Name = "checkedListBoxPendingChanges";
            this.checkedListBoxPendingChanges.Size = new System.Drawing.Size(789, 364);
            this.checkedListBoxPendingChanges.TabIndex = 0;
            // 
            // buttonSubmit
            // 
            this.buttonSubmit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSubmit.Location = new System.Drawing.Point(675, 473);
            this.buttonSubmit.Name = "buttonSubmit";
            this.buttonSubmit.Size = new System.Drawing.Size(126, 32);
            this.buttonSubmit.TabIndex = 1;
            this.buttonSubmit.Text = "Envoyer";
            this.buttonSubmit.UseVisualStyleBackColor = true;
            this.buttonSubmit.Click += new System.EventHandler(this.buttonSubmit_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(675, 535);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(126, 32);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Location = new System.Drawing.Point(127, 401);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(119, 26);
            this.buttonSelectAll.TabIndex = 3;
            this.buttonSelectAll.Text = "Sélectionner Tout";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonUnselectAll
            // 
            this.buttonUnselectAll.Location = new System.Drawing.Point(585, 401);
            this.buttonUnselectAll.Name = "buttonUnselectAll";
            this.buttonUnselectAll.Size = new System.Drawing.Size(119, 26);
            this.buttonUnselectAll.TabIndex = 4;
            this.buttonUnselectAll.Text = "Désélectionner Tout";
            this.buttonUnselectAll.UseVisualStyleBackColor = true;
            this.buttonUnselectAll.Click += new System.EventHandler(this.buttonUnselectAll_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Sélectionner les fichiers à envoyer en Code Review :";
            // 
            // textBoxDescriptionCodeReview
            // 
            this.textBoxDescriptionCodeReview.Location = new System.Drawing.Point(12, 462);
            this.textBoxDescriptionCodeReview.Multiline = true;
            this.textBoxDescriptionCodeReview.Name = "textBoxDescriptionCodeReview";
            this.textBoxDescriptionCodeReview.Size = new System.Drawing.Size(636, 105);
            this.textBoxDescriptionCodeReview.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 446);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Raison du Code Review :";
            // 
            // FormPendingChanges
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(813, 579);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxDescriptionCodeReview);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonUnselectAll);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSubmit);
            this.Controls.Add(this.checkedListBoxPendingChanges);
            this.Name = "FormPendingChanges";
            this.Text = "FormPendingChanges";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBoxPendingChanges;
        private System.Windows.Forms.Button buttonSubmit;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonUnselectAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDescriptionCodeReview;
        private System.Windows.Forms.Label label2;
    }
}