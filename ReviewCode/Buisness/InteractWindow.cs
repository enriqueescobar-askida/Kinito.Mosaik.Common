using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;
using EnvDTE80;
using EnvDTE;
using System.Data;

delegate void InitiateDataGridView();

namespace ReviewCode.Buisness
{
    public static class InteractWindow
    {
        //Configuration XML of a CodeReview Item
        private static string paramID = "System.Id";
        private static DataGridView dataGridViewWorkItem;
        private static DataGridView dataGridViewWorkItemOriginal;
        private static int workItemId;
        private static Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemCollection workItemColl;
        private static List<DataGridViewColumn> colsDataGrid = null;
        private static DataSet dataSet;
        private static DataTable dataTable;
        public static int call;

        /// <summary>
        /// Permit to select the Work Item Id in the GridView Of Visual Studio
        /// </summary>
        /// <param name="dataGridView">DataGridView WorkItem</param>
        /// <param name="id">WorkItem ID</param>
        public static void ModifyGridViewWorkItem(DataGridView dataGridView, int id)
        {
            dataGridViewWorkItem = dataGridView;
            workItemId = id;
            dataGridViewWorkItem.Refresh();
            dataGridView.RowPostPaint += new DataGridViewRowPostPaintEventHandler(dataGridView_RowPostPaint);


        }

        static void dataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (dataGridViewWorkItem.RowCount > 0)
            {
                int rows = dataGridViewWorkItem.RowCount;
                int getColsID = dataGridViewWorkItem.Columns[paramID].Index;

                for (int i = 0; i < rows; i++)
                {
                    int idWorkItemGridView = int.Parse(dataGridViewWorkItem[paramID, i].Value.ToString());
                    if (idWorkItemGridView == workItemId)
                    {
                        dataGridViewWorkItem.Rows[i].Selected = true;
                        dataGridViewWorkItem.Rows[i].Cells[0].Value = true;
                        break;
                    }
                }
            }
        }

        static void InitiateDataGridViewMethod()
        {
            if (call == 0)
            {
                dataGridViewWorkItemOriginal = dataGridViewWorkItem;
                dataGridViewWorkItem.Dispose();
            }

        }

        public static void ModifyGridViewWorkItemTest(DataGridView dataGridView, int id, Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemCollection testcoll)
        {
            call++;
            workItemColl = testcoll;
            dataGridViewWorkItem = dataGridView;

            dataGridViewWorkItem.RowsAdded += new DataGridViewRowsAddedEventHandler(dataGridViewWorkItem_RowsAdded);

            InitiateDataGridView delegateInit = InitiateDataGridViewMethod;
            dataGridViewWorkItem.BeginInvoke(delegateInit);

            //Assign variable ;
            if (dataSet == null)
            {
                dataGridViewWorkItem = new DataGridView();
                dataTable = new DataTable();
                dataSet = new DataSet();
            }


            //Clean DataGridView and dataset

            dataGridViewWorkItem.DataSource = null;
            dataGridViewWorkItem.Rows.Clear();
            dataGridViewWorkItem.Columns.Clear();
            dataGridViewWorkItem.Refresh();
            dataSet.Clear();

            //dataGridViewWorkItem.DataBindings.Clear();
            //Lauchning Tread to initiate Data
            //        System.Threading.Thread threadDataSetConstruct = new System.Threading.Thread(new System.Threading.ThreadStart(ConstructDataSet));
            //        threadDataSetConstruct.Start();
            /*
            dataGridViewWorkItem.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            colsDataGrid = new List<DataGridViewColumn>();
            //Create Columns of datagrid
            foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.FieldDefinition field in workItemColl.DisplayFields)
            {
                DataGridViewColumn colDataGrid = new DataGridViewColumn();
                DataGridViewCell cell = new DataGridViewTextBoxCell();
                colDataGrid.CellTemplate = cell;
                colDataGrid.Visible = true;
                colDataGrid.ValueType = field.SystemType;
                colDataGrid.DataPropertyName = field.ReferenceName;
                colDataGrid.Name = field.ReferenceName;
                colDataGrid.HeaderText = field.Name;
                //colsDataGrid.Add(colDataGrid);
                dataGridViewWorkItem.Columns.Add(colDataGrid);
            }
            */
            dataGridViewWorkItem.Refresh();
           // if (call > 1)
          //  {
                //Populate DATASET 
                ConstructDataSet();
                dataGridViewWorkItem.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //           threadDataSetConstruct.Join();
                //dataGridViewWorkItem.Columns.AddRange(colsDataGrid.ToArray());
                dataGridViewWorkItem.DataSource = dataSet;
                dataGridViewWorkItem.DataMember = dataSet.Tables[0].TableName;
          //  }
        }

        static void dataGridViewWorkItem_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            MessageBox.Show("Data ");
        }


        public static void ConstructDataSet()
        {
            dataTable.Clear();
            if (dataTable.Columns.Count != workItemColl.DisplayFields.Count)
            {
                dataTable.Columns.Clear();
                foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.FieldDefinition titleField in workItemColl.DisplayFields)
                {
                    DataColumn column = new DataColumn();
                    column.ColumnName = titleField.Name;
                    column.DataType = titleField.SystemType;
                    dataTable.Columns.Add(column);
                }
            }
            foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem workItem in workItemColl)
            {
                DataRow row = dataTable.NewRow();
                foreach (DataColumn cols in dataTable.Columns)
                {
                    row[cols] = workItem[cols.ColumnName];
                    System.Diagnostics.Trace.WriteLine(" Colonne : " + cols + " value :" + workItem[cols.ColumnName]);
                }
                dataTable.Rows.Add(row);
            }
            dataSet.Tables.Clear();
            dataSet.Tables.Add(dataTable);

        }

        /// <summary>
        /// Open Pending Changes Window
        /// </summary>
        /// <param name="cmdBar">Take CommanBars of the main Window </param>
        public static void OpenPendingChangesWindow(CommandBars cmdBar)
        {
            CommandBar cmdMenuBar = (CommandBar)cmdBar[1];
            CommandBarPopup cmdBarPopupView = (CommandBarPopup)cmdMenuBar.Controls[3];
            CommandBar cmdBarView = cmdBarPopupView.CommandBar;
            CommandBarPopup cmdOtherWindowBarPopup = (CommandBarPopup)cmdBarView.Controls["Other Windows"];
            CommandBarButton buttonPendingChanges = (CommandBarButton)cmdOtherWindowBarPopup.Controls[17];
            buttonPendingChanges.Execute();

        }

        public static void OpenWorkItemView(ToolStrip toolWorkItemLeftBar)
        {
            toolWorkItemLeftBar.Items["buttonWorkItems"].PerformClick();
            // toolWorkItemTop.Items["ctlRefreshQuery"].PerformClick();
        }

        public static void RefreshWorkItemView(ToolStrip toolWorkItemTopBar)
        {
            toolWorkItemTopBar.Items["ctlRefreshQuery"].PerformClick();
            
        }

        public static void GetListBoxToolStripTop(ToolStrip toolWorkItemTopBar)
        {
            foreach(ToolStripItem item in toolWorkItemTopBar.Items)
            {
                System.Diagnostics.Trace.WriteLine(item.Name);
            }

            ToolStripComboBox ctrlList = (ToolStripComboBox) toolWorkItemTopBar.Items["ctlQuery"];
            ToolStripLabel test = (ToolStripLabel)toolWorkItemTopBar.Items["labelShowQuery"];
            test.Text = "coucou";
            test.Invalidate();
            ctrlList.PerformClick();
    //        ctrlLst.PerformClick();
            System.Diagnostics.Trace.WriteLine("Selected item : " +ctrlList.SelectedItem  +" Nombre item "+ctrlList.Items.Count);
            ctrlList.SelectedIndexChanged += new EventHandler(ctrlList_SelectedIndexChanged);
        }

        static void ctrlList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Index Changed");
        }

    }
}
