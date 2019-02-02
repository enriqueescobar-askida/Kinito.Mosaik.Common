<%@ Assembly Name="AlphaMosaik.Logger.Administration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e765a843b29600b9" %>
<%@ Assembly Name="AlphaMosaik.Logger, Version=1.0.0.0, Culture=neutral, PublicKeyToken=87090ce2894e759d" %>

<%@ Page Language="C#" Inherits="AlphaMosaik.Logger.Administration.ApplicationPages.ConfigureAlphaTraceLogProviderPage" MasterPageFile="~/_admin/admin.master" EnableViewState="true" %>
<%@ Import Namespace="System.ComponentModel"%>
<%@ Import Namespace="AlphaMosaik.Logger.Storage"%>

<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="AlphaMosaik" TagName="AlphaLogProviderSection" Src="~/_admin/AlphaMosaik.Logger/AlphaLogProviderSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" Src="~/_controltemplates/ButtonSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/InputFormControl.ascx" %>


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
             
       <AlphaMosaik:AlphaLogProviderSection runat="server" ID="AlphaTraceLogProviderSection" />
    </table>
</asp:Content>
