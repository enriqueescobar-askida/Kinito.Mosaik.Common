<%@ Control Language="C#" Inherits="AlphaMosaik.Logger.Administration.WebControls.SpLogProviderSection, AlphaMosaik.Logger.Administration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e765a843b29600b9"
    CompilationMode="Always" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/InputFormControl.ascx" %>

<wssuc:InputFormSection title="<%$Resources:AlphaMosaikLogger, ConfigurePage_SpLogProviderSection_Title%>"
    description="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_Desc%>"
    runat="server" collapsible="true">
    <template_inputformcontrols>
		<wssuc:InputFormControl runat="server">
			<Template_Control>
				<SharePoint:InputFormCheckBox ID="ChkBoxSpLogProviderEnabled"
										  LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SpLogProviderSection_EnabledLabel%>"
										  runat="server"/>
			</Template_Control>
		</wssuc:InputFormControl>
	</template_inputformcontrols>
</wssuc:InputFormSection>