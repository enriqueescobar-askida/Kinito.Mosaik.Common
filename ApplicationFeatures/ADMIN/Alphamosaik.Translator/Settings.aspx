<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="Alphamosaik.Translator.ApplicationFeatures.Admin.SettingsPage" DynamicMasterPageFile="~masterurl/default.master" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" src="~/_controltemplates/ButtonSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/InputFormControl.ascx" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table border="0" width="100%" cellspacing="0" cellpadding="0">
        <wssuc:ButtonSection runat="server" TopButtons="true" BottomSpacing="5" ShowSectionLine="false">
		    <Template_Buttons>
			    <asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" Text="OK" id="BtnSettingsTop"/>
		    </Template_Buttons>
	    </wssuc:ButtonSection>

        <!-- WEB APP SELECTOR -->
        <wssuc:InputFormSection Title="<%$Resources:spadmin, multipages_webapplication_title%>" runat="server" id="idWebApplicationSelectorSection">
		    <Template_Description>
		       <SharePoint:EncodedLiteral ID="EncodedLiteral1" runat="server" text="<%$Resources:spadmin, multipages_webapplication_desc%>" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting'/>
		       <br /><br />
		       <SharePoint:EncodedLiteral ID="EncodedLiteral2" runat="server" text="<%$Resources:spadmin, createsite_webapp_desc1%>" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting'/>
			    <a id="LinkCreateWebApplication" href="#" onclick="javascript:GoToPageRelative('extendvs.aspx');return false;"><SharePoint:EncodedLiteral ID="EncodedLiteral3" runat="server" text="<%$Resources:spadmin, createsite_webapp_desc2%>" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting'/></a>
		      <SharePoint:EncodedLiteral ID="EncodedLiteral4" runat="server" text="<%$Resources:spadmin, createsite_webapp_desc3%>" EncodeMethod='HtmlEncodeAllowSimpleTextFormatting'/>
	        </Template_Description>
		    <Template_InputFormControls>
			    <tr><td>
			    <SharePoint:WebApplicationSelector id="WebAppSelector" runat="server" AllowAdministrationWebApplication="false" UseDefaultSelection="true" />
			    </td></tr>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- ITEM FILTERING -->
        <wssuc:InputFormSection
		    Title="Item filtering"
		    runat="server"
		    id="ItemFilteringSection"
            >
		    <Template_Description>
			    <p>This setting controls item filtering. If you choose to deactivate item filtering all items will be displayed on every page.</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Enable item filtering:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="ItemFiltering" ID="ItemFilteringEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="ItemFiltering" ID="ItemFilteringDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- DASHBOARD -->
        <wssuc:InputFormSection
		    Title="Item dashboard"
		    runat="server"
		    id="ItemDashboardSection"
            >
		    <Template_Description>
			    <p>Description.</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Enable item dashboard:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="ItemDashboard" ID="ItemDashboardEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="ItemDashboard" ID="ItemDashboardDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- FILTERING BUTTON -->
        <wssuc:InputFormSection
		    Title="Filtering button"
		    runat="server"
		    id="FilteringButtonSection"
            >
		    <Template_Description>
			    <p>Description.</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Enable filtering button:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="FilteringButton" ID="FilteringButtonEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="FilteringButton" ID="FilteringButtonDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- COMPLETING MODE -->
        <wssuc:InputFormSection
		    Title="Completing mode"
		    runat="server"
		    id="CompletingModeSection"
            >
		    <Template_Description>
			    <p>The dictionary auto completion allows you to retrieve all no translated expressions on a Sharepoint page and to add them in the dictionary thanks to a quick way.</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Enable completing mode:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="CompletingMode" ID="CompletingModeEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="CompletingMode" ID="CompletingModeDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- AUTOTRANSLATION TEXT MESSAGE -->
        <wssuc:InputFormSection
		    Title="Automatic translation message"
		    runat="server"
		    id="AutotranslationMessageSection"
            >
		    <Template_Description>
			    <p>Choose a message to display after your announcements when auto-translation is activated.</p>
                <p>This message will be displayed only for auto-translated items.</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Message:" smallindent="true">
			        <Template_control>
                        <asp:TextBox ID="AutoTranslatedMessageTxBox" runat="server" TextMode="MultiLine" Width="100%" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- REPLACED LINKS -->
        <wssuc:InputFormSection
		    Title="Manage hyperlinks to the linked pages"
		    runat="server"
		    id="ReplaceLinkedPagesUrlSection"
            >
		    <Template_Description>
			    <p>Description</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Enable:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="ReplaceLinkedPagesUrl" ID="ReplaceLinkedPagesUrlEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="ReplaceLinkedPagesUrl" ID="ReplaceLinkedPagesUrlDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- QUICK LAUNCH FILTER -->
        <wssuc:InputFormSection
		    Title="Quick launch filtering"
		    runat="server"
		    id="QuickLaunchFilteringSection"
            >
		    <Template_Description>
			    <p>Description</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Enable quick launch filtering:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="QuickLaunchFiltering" ID="QuickLaunchFilteringEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="QuickLaunchFiltering" ID="QuickLaunchFilteringDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- EXTRACTOR -->
        <wssuc:InputFormSection
		    Title="Translations Extractor Tool"
		    runat="server"
		    id="TranslationsExtractorToolSection"
            >
		    <Template_Description>
			    <p>Enable the Translations Extractor Tool to retrieve all missing translations in your Web Application. You must both install the "Translations Extractor Tool" and authorize its execution in order to use this functionality.</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Authorize translations extractor tool:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="TranslationExtractorTool" ID="TranslationExtractorToolEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="TranslationExtractorTool" ID="TranslationExtractorToolDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- CSS -->
        <wssuc:InputFormSection
		    Title="CSS Section"
		    runat="server"
		    id="CssSection"
            >
		    <Template_Description>
			    <p>Description</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Css Banner:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="CssBanner" ID="CssBannerEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="CssBanner" ID="CssBannerDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Show pipe symbols in the language banner:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="PipeSymbols" ID="PipeSymbolsEnabledRad" Text="Yes" runat="server" />
                        <br />
                        <asp:RadioButton GroupName="PipeSymbols" ID="PipeSymbolsDisabledRad" Text="No" runat="server" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <!-- TRANSLATION SERVICE -->
        <wssuc:InputFormSection
		    Title="Translation Service"
		    runat="server"
		    id="TranslationService"
            >
		    <Template_Description>
			    <p>Description</p>
		    </Template_Description>
		    <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Service type:" smallindent="true">
			        <Template_control>
                        <asp:DropDownList ID="TranslationServiceTypeDdl" runat="server" AutoPostBack="true">
                            <asp:ListItem value="none">None</asp:ListItem>
                            <asp:ListItem value="bing">Microsoft Bing Translator</asp:ListItem>
                            <asp:ListItem value="systran">SYSTRAN Enterprise Server</asp:ListItem>
                        </asp:DropDownList>
			        </Template_control>
                </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Service name:" smallindent="true">
			        <Template_control>
                        <asp:TextBox ID="TranslationServiceNameTxBox" runat="server" Width="100%" />
			        </Template_control>
                </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Service Url:" smallindent="true">
			        <Template_control>
                        <asp:TextBox ID="TranslationServiceUrlTxBox" runat="server" Width="100%" />
			        </Template_control>
                </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Bing Application Id:" smallindent="true">
			        <Template_control>
                        <asp:TextBox ID="TranslationServiceBingApplicationIdTextBox" runat="server" Width="100%" />
			        </Template_control>
                </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Authentication:" smallindent="true">
			        <Template_control>
                        <asp:RadioButton GroupName="TranslationServiceAuthType" ID="TranslationServiceAuthAnonymousRad" Text="Anonymous" runat="server" AutoPostBack="true" />
                        <br />
                        <asp:RadioButton GroupName="TranslationServiceAuthType" ID="TranslationServiceAuthLoginRad" Text="Account" runat="server" AutoPostBack="true" />
			        </Template_control>
                </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Login:" smallindent="true">
			        <Template_control>
                        Username :<asp:TextBox ID="TranslationServiceUsernameTxBox" runat="server" Width="100%" />
                        <br/>
                        Password :<asp:TextBox ID="TranslationServicePasswordTxBox" runat="server" Width="100%" TextMode="Password" />
			        </Template_control>
                </wssuc:InputFormControl>
		    </Template_InputFormControls>
	    </wssuc:InputFormSection>

        <wssuc:ButtonSection runat="server">
		    <Template_Buttons>
			    <asp:Button UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" Text="OK" id="BtnSettings" />
		    </Template_Buttons>
	    </wssuc:ButtonSection>
    </table>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Alphamosaik Translator Settings
</asp:Content>
