<%@ Control Language="C#" Inherits="AlphaMosaik.Logger.Administration.WebControls.ProvidersSection, AlphaMosaik.Logger.Administration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e765a843b29600b9"
    CompilationMode="Always" EnableViewState="true" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/InputFormControl.ascx" %>

<wssuc:InputFormSection title="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_Title%>"
    description="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_Desc%>"
    runat="server" collapsible="true">
    <template_inputformcontrols>
		<wssuc:InputFormControl runat="server">
			<Template_Control>
				<SharePoint:InputFormCheckBox ID="ChkBoxAlphaLogProviderEnabled"
										  LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_EnabledLabel%>"
										  runat="server"/>
			</Template_Control>
		</wssuc:InputFormControl>
		<tr> <td class="ms-sectionline" height=1 colspan=2 class="ms-widthControl"><img src="/_layouts/images/blank.gif" width="1" height="1" alt=""></td> </tr>
		<TR><TD><IMG SRC="/_layouts/images/blank.gif" width=1 height=5 alt=""></TD></TR>
		<SharePoint:InputFormRadioButton runat="server" id="RadDefaultFilePath" Checked="true"
			LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_DefaultFilePath%>" GroupName="AlphaLogProviderSection_FilePath" />
		<SharePoint:InputFormRadioButton runat="server" id="RadCustomFilePath"
			LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_CustomFilePath%>" GroupName="AlphaLogProviderSection_FilePath" Checked="false" >
			<table border="0" width="100%" cellspacing="0" cellpadding="0" class="ms-authoringcontrols">
				<wssuc:InputFormControl runat="server"
					LabelText="" >
					<Template_control>
						<SharePoint:InputFormTextBox Title="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_CustomFilePath%>" class="ms-input" ID="TxBoxCustomFilePath" Columns="35" Runat="server" MaxLength=255 size="25" Enabled="false" />
						<SharePoint:InputFormRequiredFieldValidator
							ID="ReqValCustomFilePath"
							ControlToValidate="TxBoxCustomFilePath"
							ErrorMessage="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_CustomFilePath_Missing%>"
							Runat="server" />
					</Template_control>
				</wssuc:InputFormControl>
			</table>
		</SharePoint:InputFormRadioButton>
	</template_inputformcontrols>
</wssuc:InputFormSection>