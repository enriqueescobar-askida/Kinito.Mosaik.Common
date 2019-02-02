<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Licensing.aspx.cs" Inherits="Alphamosaik.Translator.ApplicationFeatures.Layouts.Alphamosaik.Translator.Pages.Licensing" DynamicMasterPageFile="~masterurl/default.master" %>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.WebControls" TagPrefix="asp" %>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
   
<!--
    <h2>My Farm</h2>
        <asp:TreeView ID="farmHierarchyViewer" runat="server" ShowLines="true" EnableViewState="true">
    </asp:TreeView>
    
    <asp:ListView ID="ListView1" runat="server">
    </asp:ListView>
-->

<!--
Checkbox | Nom du serveur | Licence.key | Licence.dat (none) | serveur.zip

Select All
Select None


-->

 <asp:ListView runat="server" 
          ID="CategoriesListView"
          OnItemDeleting="CategoriesListView_OnItemDeleting"
          DataSourceID="CategoriesDataSource" 
          DataKeyNames="ProductCategoryID">
          <LayoutTemplate>
            <table runat="server" id="tblCategories" 
                   cellspacing="0" cellpadding="1" width="440px" border="1">
              <tr id="itemPlaceholder" runat="server"></tr>
            </table>
          </LayoutTemplate>
          <ItemTemplate>
            <tr id="Tr1" runat="server">
              <td>
                <asp:Label runat="server" ID="NameLabel" Text='<%#Eval("Name") %>' />
              </td>
              <td style="width:40px">
                <asp:LinkButton runat="server" ID="SelectCategoryButton" 
                  Text="Select" CommandName="Select" />
              </td>
            </tr>
          </ItemTemplate>
          <SelectedItemTemplate>
            <tr id="Tr2" runat="server" style="background-color:#90EE90">
              <td>
                <asp:Label runat="server" ID="NameLabel" Text='<%#Eval("Name") %>' />
              </td>
              <td style="width:40px">
                <asp:LinkButton runat="server" ID="SelectCategoryButton" 
                  Text="Delete" CommandName="Delete" />
              </td>
            </tr>
          </SelectedItemTemplate>
        </asp:ListView>

       <asp:GridView runat="server" ID="SubCategoriesGridView" Width="300px"
             DataSourceID="SubCategoriesDataSource" DataKeyNames="ProductSubcategoryID" 
             AutoGenerateColumns="True" />
  
        <!-- This example uses Microsoft SQL Server and connects      -->
        <!-- to the AdventureWorks sample database. Use an ASP.NET    -->
        <!-- expression to retrieve the connection string value       -->
        <!-- from the Web.config file.                                -->
        <asp:SqlDataSource ID="CategoriesDataSource" runat="server" 
          ConnectionString="<%$ ConnectionStrings:AdventureWorks_DataConnectionString %>"
          SelectCommand="SELECT [ProductCategoryID], [Name]
                         FROM Production.ProductCategory">
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="SubCategoriesDataSource" runat="server" 
          ConnectionString="<%$ ConnectionStrings:AdventureWorks_DataConnectionString %>"
          SelectCommand="SELECT [ProductSubcategoryID], [Name]
                         FROM Production.ProductSubcategory
                         WHERE ProductCategoryID = @ProductCategoryID
                         ORDER BY [Name]">
            <SelectParameters>
              <asp:ControlParameter Name="ProductCategoryID" DefaultValue="0"
                   ControlID="CategoriesListView" PropertyName="SelectedValue"  />
            </SelectParameters>
        </asp:SqlDataSource>






</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Oceanik Licensing
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Oceanik Licensing Page
</asp:Content>
