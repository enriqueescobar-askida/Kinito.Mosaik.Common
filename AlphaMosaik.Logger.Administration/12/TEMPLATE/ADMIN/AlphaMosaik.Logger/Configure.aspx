<%@ Assembly Name="AlphaMosaik.Logger.Administration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e765a843b29600b9" %>
<%@ Assembly Name="AlphaMosaik.Logger, Version=1.0.0.0, Culture=neutral, PublicKeyToken=87090ce2894e759d" %>

<%@ Page Language="C#" Inherits="AlphaMosaik.Logger.Administration.ApplicationPages.ConfigurePage"
    MasterPageFile="~/_admin/admin.master" EnableViewState="true" EnableViewStateMac="true" %>
<%@ Import Namespace="System.ComponentModel"%>
<%@ Import Namespace="AlphaMosaik.Logger.Storage"%>

<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="AlphaMosaik" TagName="SqlProviderSection" Src="~/_admin/AlphaMosaik.Logger/SqlProviderSection.ascx" %>
<%@ Register TagPrefix="AlphaMosaik" TagName="SpLogProviderSection" Src="~/_admin/AlphaMosaik.Logger/SpLogProviderSection.ascx" %>
<%@ Register TagPrefix="AlphaMosaik" TagName="AlphaLogProviderSection" Src="~/_admin/AlphaMosaik.Logger/AlphaLogProviderSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="~/_controltemplates/ButtonSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/InputFormControl.ascx" %>

<script runat="server">
    string DisplayDefaultIcon(object dataItem)
    {
        IStorageProvider storageProvider = (IStorageProvider) dataItem;
        
        if(currentStorageManager != null && currentStorageManager.DefaultStorageProvider != null && storageProvider != null)
        {
            if (currentStorageManager.DefaultStorageProvider.Name == storageProvider.Name)
            {
                return "<img src=\"~/_layouts/images/chkmrk.gif\" alt=\"Default provider\"/>";
            }
        }

        return string.Empty;
    }
</script>


<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    <sharepoint:encodedliteral runat="server" text="<%$Resources:AlphaMosaikLogger, ConfigurePage_Title%>"
        encodemethod="HtmlEncode" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea"
    runat="server">
    <sharepoint:encodedliteral id="EncodedLiteral1" runat="server" text="<%$Resources:AlphaMosaikLogger, ConfigurePage_Title%>"
        encodemethod="HtmlEncode" />
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PlaceHolderPageDescription" runat="server">
    <sharepoint:encodedliteral id="EncodedLiteral2" runat="server" text="<%$Resources:AlphaMosaikLogger, ConfigurePage_Description%>"
        encodemethod="HtmlEncodeAllowSimpleTextFormatting" />
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <table width="100%" class="propertysheet" cellspacing="0" cellpadding="0" border="0">
        <tr>
            <td class="ms-descriptionText">
                <asp:Label ID="LabelMessage" runat="server" EnableViewState="False" class="ms-descriptionText" />
            </td>
        </tr>
        <tr>
            <td class="ms-error">
                <asp:Label ID="LabelErrorMessage" runat="server" EnableViewState="False" />
            </td>
        </tr>
        <tr>
            <td class="ms-descriptionText">
                <asp:ValidationSummary ID="ValSummary" HeaderText="<%$SPHtmlEncodedResources:spadmin, ValidationSummaryHeaderText%>"
                    DisplayMode="BulletList" ShowSummary="True" runat="server"></asp:ValidationSummary>
            </td>
        </tr>
        <tr>
            <td>
                <img src="/_layouts/images/blank.gif" width="10" height="1" alt="" />
            </td>
        </tr>
    </table>
    <table border="0" cellspacing="0" cellpadding="0" width="100%">
        <wssuc:ButtonSection runat="server" TopButtons="true" BottomSpacing="5" ShowSectionLine="false">
            <template_buttons>
			    <asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" OnClick="BtSubmitClick" Text="<%$Resources:wss,multipages_okbutton_text%>" id="BtSaveSettingsTop" accesskey="<%$Resources:wss,okbutton_accesskey%>"/>
		    </template_buttons>
        </wssuc:ButtonSection>
        <wssuc:InputFormSection runat="server" title="<%$Resources:spadmin, multipages_webapplication_title%>"
            description="<%$Resources:spadmin, multipages_webapplication_desc%>">
            <template_inputformcontrols>
                <tr>
                    <td>
			            <SharePoint:WebApplicationSelector ID="WebAppSelector" runat="server" AllowAdministrationWebApplication="false" OnContextChange="WebAppSelectorOnContextChange" />
                    </td>
                </tr>
		    </template_inputformcontrols>
        </wssuc:InputFormSection>
        <wssuc:InputFormSection title="<%$Resources:AlphaMosaikLogger, ConfigurePage_EnableSection_Title%>"
            description="<%$Resources:AlphaMosaikLogger, ConfigurePage_EnableSection_Desc%>"
            runat="server">
            <template_inputformcontrols>
				<wssuc:InputFormControl runat="server">
					<Template_Control>
						<SharePoint:InputFormCheckBox ID="ChkBoxLoggerEnabled"
												  LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_EnableSection_ChkBoxEnabled%>"
												  runat="server"
												  OnCheckChanged="ChkBoxLoggerEnabledCheckChanged" />
					</Template_Control>
				</wssuc:InputFormControl>
			</template_inputformcontrols>
        </wssuc:InputFormSection>      
    </table>
    
    <table width="100%">
        <tr>
            <td class="ms-linksectionheader" style="padding:4px">Log Storage Providers</td>
        </tr>
    </table>
    
    <table width="100%">
        <tr>
            <td colspan="2">
                <sharepoint:spgridview runat="server" id="GridViewProviders" autogeneratecolumns="false">
                    <Columns>
                        <asp:TemplateField HeaderText="Name">
                            <ItemTemplate>
                                <asp:Label ID="StorageProviderDisplayName" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Definition.DisplayName").ToString() %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Default">
                            <ItemTemplate>
                                <%# DisplayDefaultIcon(Container.DataItem) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Enabled">
                            <ItemTemplate>
                                <asp:CheckBox runat="server" ID="StorageProviderEnabled" OnCheckedChanged="StorageProviderEnabledChanged" AutoPostBack="true" Checked='<%# DataBinder.Eval(Container.DataItem, "Enabled") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Advanced configuration">
                            <ItemTemplate>
                                <asp:Literal runat="server" Text='<%# (Eval("Definition.ConfigurationUrl") == null ? "" : "<a href=\"" + DataBinder.Eval(Container.DataItem, "Definition.ConfigurationUrl") + "?WebApplicationId=" + WebAppSelector.CurrentId + "\">Configure</a>") %>' />
                                <asp:LinkButton runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </sharepoint:spgridview>
            </td>
        </tr>
    </table>
    <table border="0" cellspacing="0" cellpadding="0" width="100%">
        <wssuc:ButtonSection runat="server">
            <template_buttons>
			<asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" OnClick="BtSubmitClick" Text="<%$Resources:wss,multipages_okbutton_text%>" id="BtnCreateSite" accesskey="<%$Resources:wss,okbutton_accesskey%>"/>
		</template_buttons>
        </wssuc:ButtonSection>
    </table>
</asp:Content>
