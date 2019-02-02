namespace ReviewCode
{
    partial class FormWorkItem
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
            this.ListBoxProjectTFS = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.listBoxUserList = new System.Windows.Forms.ListBox();
            this.buttonTestUserList = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ListBoxProjectTFS
            // 
            this.ListBoxProjectTFS.FormattingEnabled = true;
            this.ListBoxProjectTFS.Location = new System.Drawing.Point(12, 90);
            this.ListBoxProjectTFS.Name = "ListBoxProjectTFS";
            this.ListBoxProjectTFS.Size = new System.Drawing.Size(331, 108);
            this.ListBoxProjectTFS.TabIndex = 0;
            this.ListBoxProjectTFS.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(349, 51);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(78, 44);
            this.button1.TabIndex = 1;
            this.button1.Text = "Selectionner";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.LoadListingWorkItemProject_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(349, 140);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(78, 44);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // listBoxUserList
            // 
            this.listBoxUserList.FormattingEnabled = true;
            this.listBoxUserList.Location = new System.Drawing.Point(12, 23);
            this.listBoxUserList.Name = "listBoxUserList";
            this.listBoxUserList.Size = new System.Drawing.Size(331, 212);
            this.listBoxUserList.TabIndex = 3;
            // 
            // buttonTestUserList
            // 
            this.buttonTestUserList.Location = new System.Drawing.Point(364, 230);
            this.buttonTestUserList.Name = "buttonTestUserList";
            this.buttonTestUserList.Size = new System.Drawing.Size(62, 22);
            this.buttonTestUserList.TabIndex = 4;
            this.buttonTestUserList.Text = "Temp";
            this.buttonTestUserList.UseVisualStyleBackColor = true;
            this.buttonTestUserList.Visible = false;
            this.buttonTestUserList.Click += new System.EventHandler(this.buttonTestUserList_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(258, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Veuillez choisir la personne à affecter au code review";
            // 
            // FormWorkItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 266);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonTestUserList);
            this.Controls.Add(this.listBoxUserList);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ListBoxProjectTFS);
            this.Name = "FormWorkItem";
            this.Text = "FormWorkItem";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox ListBoxProjectTFS;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListBox listBoxUserList;
        private System.Windows.Forms.Button buttonTestUserList;
        private System.Windows.Forms.Label label1;
    }
}