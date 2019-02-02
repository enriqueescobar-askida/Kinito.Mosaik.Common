<%@ Page Language="C#" MasterPageFile="~/_layouts/application.master"  Inherits="TranslatingServicesTest.SystranConfig, TranslatingServicesTest, Version=1.0.0.0, Culture=neutral, PublickeyToken=16f3e5949a3db94e" %>

<%@ Register Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" 

    Namespace="Microsoft.SharePoint.WebControls" TagPrefix="cc1"%>
    
<asp:Content ID="TitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
<div class="ms-pagetitle">Translation services configuration</div>
</asp:Content>
<asp:Content ID="PageDescription" ContentPlaceHolderID="PlaceHolderPageDescription" runat="server">
<div>Use this page to configure your online translation service connections</div>
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
<table width="100%">
    <tr>
    <td>
        <!-- Service Selector /-->
        <table width="100%" cellspacing="0">
            <tr>
                <td class="ms-formlabel">Translation provider</td>
                <td style="width:350px" class="ms-formbody">
                    <asp:DropDownList 
                            ID="lstTranslationProviders" runat="server" Width="100px" 
                        AutoPostBack="True">
                            <asp:ListItem Selected="True" Value="SYSTRAN">SYSTRAN</asp:ListItem>
                            <asp:ListItem Value="BING">BING</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
        </table> 
        <!-- Service Properties /-->
         <asp:MultiView ID="mvwProviders" runat="server" ActiveViewIndex="0">
            <asp:View ID="vwSystran" runat="server">
                <table width="100%" align="center" cellspacing="0">
                    <tr>
                        <td class="ms-formlabel">Type</td>
                        <td style="width:350px" class="ms-formbody">
                            <asp:DropDownList ID="lstServiceTypes" runat="server" Width="240px">
                                <asp:ListItem Selected="True" Value="SYSTRAN">Serveur d&#39;entreprise SYSTRAN</asp:ListItem>
                                <asp:ListItem Enabled="False" Value="BING">Service de traduction BING</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td class="ms-formlabel">Name</td>
                        <td class="ms-formbody">
                            <asp:TextBox ID="txtSystranSvcName" runat="server" Width="235px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="ms-formlabel">Url</td>
                        <td class="ms-formbody">
                            <asp:TextBox ID="txtServiceURL" runat="server" Width="235px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="ms-formlabel">User Type</td>
                        <td class="ms-formbody">
                            <asp:RadioButtonList ID="rblUserType" runat="server" AutoPostBack="true"
                                RepeatDirection="Horizontal" Width="240px" CssClass="ms-authoringcontrols">
                                <asp:ListItem Selected="True" Value="ANONYMOUS">Anonymous</asp:ListItem>
                                <asp:ListItem Value="AUTHENTICATED">Authenticated</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                </table>
                <%-- ********************* --%>
                <asp:Panel ID="pnlUserAuthentication" runat="server">
                    <table width="100%" align="center" cellspacing="0">
                        <tr>
                            <td class="ms-formlabel">UserName</td>
                            <td class="ms-formbody" style="width:350px">
                                <asp:TextBox ID="txtAuthUserName" runat="server" Width="150px" TextMode="SingleLine"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="ms-formlabel">Password</td>
                            <td class="ms-formbody">
                                <asp:TextBox ID="txtAuthPassword" runat="server" Width="150px" TextMode="Password"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="ms-formlabel">Confirm Password</td>
                            <td class="ms-formbody">
                                <asp:TextBox ID="txtAuthPassConfirm" runat="server" Width="150px" TextMode="Password"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
                <%-- ********************* --%>
                <table width="100%" align="center" cellspacing="0">
                    <tr>
                        <td class="ms-formlabel">Connection test</td>
                        <td style="width:350px" class="ms-formbody">
                        <asp:Button ID="btnConnectionTest" runat="server" Text="Validate" />
                        <asp:Label ID="lblStatus" runat="server" />
                        </td>
                    </tr>
                    
                    <tr>
                        <td>&nbsp;</td>
                        <td style="width:350px" style="text-align: right">
                            
                        </td>
                    </tr>
                </table>
            </asp:View>
            <asp:View ID="vwBing" runat="server">
                <table width="100%" cellspacing="0">
                    <tr>
                        <td class="ms-formlabel">URL</td>
                        <td style="width:350px" class="ms-formbody">
                            <asp:Textbox ID="txtBingUrl" runat="server" Width="290px">
                            </asp:Textbox>
                        </td>
                    </tr>
                </table> 
            </asp:View>
        </asp:MultiView>
        <!-- OK/Cancel buttons /-->
        <table width="100%" cellspacing="0">
            <tr>
                <td colspan="2" class="ms-formline" style="height:2px">&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td align="right" style="width:350px" >
                    <asp:Button ID="btnConfirm" runat="server" Text="OK" Width="75"/>&nbsp;&nbsp;&nbsp;
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" Width="75" />
                </td>
            </tr>
        </table> 
    </td>
    </tr>
    </table>
</asp:Content>

