<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="/_controltemplates/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="/_controltemplates/ButtonSection.ascx" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<style type="text/css">
    .style1
    {
        width: 160px;
    }
</style>
<table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="100%">
    <colgroup>
        <col style="width: 60%"></col>
        <col style="width: 40%"></col>
    </colgroup>
    <tr>
        <td>
            <!-- Ajout d'une nouvelle entrée -->
            <wssuc:InputFormSection ID="formSectionNewEntry" runat="server" Title="Add new entry"
                Description="Use this form to add a new entry into the log system. All enabled providers will catch and store this entry."
                Collapsible="false">
                <template_inputformcontrols runat="server">
                    <wssuc:InputFormControl runat="server" LabelText="Message:" AssociatedControlId="txBoxMessage">
                        <Template_Control runat="server">
                            <asp:TextBox ID="txBoxMessage" runat="server" />                  
                        </Template_Control>
                    </wssuc:InputFormControl>
                    <wssuc:InputFormControl runat="server" LabelText="Trace severity:" AssociatedControlId="ddlLevel">
                        <Template_Control runat="server">
                            <asp:DropDownList ID="ddlLevel" runat="server" />                  
                        </Template_Control>
                    </wssuc:InputFormControl>
                    <wssuc:InputFormControl runat="server" LabelText="Use web service:" AssociatedControlId="ddlLevel">
                        <Template_Control runat="server">
                            <asp:CheckBox ID="chkBoxWebService" runat="server" Text="logger web service will be used instead class library" />                  
                        </Template_Control>
                    </wssuc:InputFormControl>
                </template_inputformcontrols>
            </wssuc:InputFormSection>
            <!-- Boutons ajout d'une nouvelle entrée -->
            <wssuc:ButtonSection runat="server" ShowStandardCancelButton="false">
                <template_buttons>
                   <asp:PlaceHolder ID="PlaceHolder1" runat="server">               
                       <asp:Button ID="btSubmitNewEntry" UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" 
                                   Text="OK" />
                   </asp:PlaceHolder>
                </template_buttons>
            </wssuc:ButtonSection>
            <!-- Configuration des providers -->
            <wssuc:InputFormSection ID="formSectionEditProviders" runat="server" Title="Configure providers"
                Description="Use this form to configure available logging providers." Collapsible="false">
                <template_inputformcontrols runat="server">
                <wssuc:InputFormControl runat="server" LabelText="Provider:" AssociatedControlId="ddlProvider">
                    <Template_Control runat="server">
                        <asp:DropDownList ID="ddlProviders" runat="server" AutoPostBack="true" Enabled="false" /><br />
                        <asp:CheckBox ID="chkBoxProviderEnabled" runat="server" Text="Enabled" Enabled="false" />       
                    </Template_Control>
                </wssuc:InputFormControl>
            </template_inputformcontrols>
            </wssuc:InputFormSection>
            <!-- Boutons configuration d'un provider -->
            <wssuc:ButtonSection runat="server" ShowStandardCancelButton="false">
                <template_buttons>
                   <asp:PlaceHolder ID="PlaceHolder2" runat="server">               
                       <asp:Button ID="btSubmitEditProvider" UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" 
                                   Text="OK" Enabled="false" />
                   </asp:PlaceHolder>
                </template_buttons>
            </wssuc:ButtonSection>
        </td>
    </tr>
</table>
