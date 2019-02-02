using System;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Administration;

namespace Alphamosaik.Translator.ApplicationFeatures.Layouts.Alphamosaik.Translator.Pages
{
    public partial class Licensing : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
/*
            string[] frontEndList =
                {
                    "Server001", 
                    "Server002"
                };

            CreateFrontEndListView(frontEndList);
*/
/*
            SPFarm thisFarm = SPFarm.Local;
            TreeNode node;
            farmHierarchyViewer.Nodes.Clear();
            foreach (SPService svc in thisFarm.Services)
            {
                node = new TreeNode();
                node.Text = "Farm Service (Type=" + svc.TypeName + "; Status="
                            + svc.Status + ")";
                farmHierarchyViewer.Nodes.Add(node);
                TreeNode svcNode = node;
                if (svc is SPWebService)
                {
                    SPWebService webSvc = (SPWebService) svc;
                    foreach (SPWebApplication webApp in webSvc.WebApplications)
                    {
                        node = new TreeNode();
                        node.Text = webApp.DisplayName;
                        svcNode.ChildNodes.Add(node);
                        TreeNode webAppNode = node;
                        if (!webApp.IsAdministrationWebApplication)
                        {
                            foreach (SPSite site in webApp.Sites)
                            {
                                site.CatchAccessDeniedException = false;
                                try
                                {
                                    node = new TreeNode();
                                    node.Text = site.Url;
                                    webAppNode.ChildNodes.Add(node);
                                    TreeNode siteNode = node;
                                    node = new TreeNode(site.RootWeb.Title, null, null,
                                                        site.RootWeb.Url +
                                                        "/_layouts/lab01/PropertyChanger.aspx?type=web&objectID="
                                                        + site.RootWeb.ID, "_self");
                                    siteNode.ChildNodes.Add(node);
                                    TreeNode parentNode = node;
                                    foreach (SPList list in site.RootWeb.Lists)
                                    {
                                        node = new TreeNode(list.Title, null, null,
                                                            site.RootWeb.Url +
                                                            "/_layouts/lab01/PropertyChanger.aspx?type=list&objectID="
                                                            + list.ID, "_self");
                                        parentNode.ChildNodes.Add(node);
                                    }
                                    foreach (SPWeb childWeb in site.RootWeb.Webs)
                                    {
                                        try
                                        {
                                            addWebs(childWeb, parentNode);
                                        }
                                        finally
                                        {
                                            childWeb.Dispose();
                                        }
                                    }
                                    site.CatchAccessDeniedException = false;
                                }
                                finally
                                {
                                    site.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            farmHierarchyViewer.ExpandAll();
        }

        void addWebs(SPWeb web, TreeNode parentNode)
        {
            TreeNode node;
            node = new TreeNode(web.Title, null, null, web.Url
            + "/_layouts/lab01/PropertyChanger.aspx?type=web&objectID="
            + web.ID, "_self");
            parentNode.ChildNodes.Add(node);
            parentNode = node;
            foreach (SPList list in web.Lists)
            {
                node = new TreeNode(list.Title, null, null, web.Url
                + "/_layouts/lab01/PropertyChanger.aspx?type=list&objectID="
                + list.ID, "_self");
                parentNode.ChildNodes.Add(node);
            }
            foreach (SPWeb childWeb in web.Webs)
            {
                try
                {
                    addWebs(childWeb, parentNode);
                }
                finally
                {
                    childWeb.Dispose();
                }
            }
        }
*/
/*
        private void CreateFrontEndListView(string[] actions)
        {
            ColumnHeader columnHeader1 = new ColumnHeader();
            ColumnHeader columnHeader2 = new ColumnHeader();

            columnHeader1.Text = "Server";
            columnHeader1.Width = 250;
            columnHeader2.Text = "IP Address";
            columnHeader2.Width = 100;

            ListView1.Columns.Add(columnHeader1);
            ListView1.Columns.Add(columnHeader2);

            foreach (string action in actions)
            {
                ListViewItem listViewItem1 = new ListViewItem();
                ListViewItem.ListViewSubItem listViewSubItem1 = new ListViewItem.ListViewSubItem();

                listViewItem1.Text = action;
                listViewSubItem1.Text = "";
                listViewItem1.SubItems.Add(listViewSubItem1);

                ListView1.Items.Add(listViewItem1);

                m_SkinList.InsertItem(i, cszItem);
                m_SkinList.SetItemText(i, 1, cszItem);
                m_SkinList.SetItemText(i, 2, "Matthew Good");
                m_SkinList.SetItemText(i, 3, "Rock");


            }
*/
        }
    }
}
