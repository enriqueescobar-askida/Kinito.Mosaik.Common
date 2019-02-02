<%@ Control Language="C#" Inherits="AlphaMosaik.Logger.Administration.WebControls.SqlProviderSection, AlphaMosaik.Logger.Administration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e765a843b29600b9"
    CompilationMode="Always" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" Src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" Src="~/_controltemplates/InputFormControl.ascx" %>

<script type="text/javascript">
    _spBodyOnLoadFunctionNames.push("SqlProviderSection_OnLoad");

    function SqlProviderSection_OnLoad() {
        InitSqlProviderSectionControls(true);
    }

    function InitSqlProviderSectionControls(bSilent) {
        var bProviderEnabled = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(ChkBoxSqlProviderEnabled.ClientID),Response.Output);%>).checked;
        
        var txBoxDatabaseServer = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(TxBoxDatabaseServer.ClientID),Response.Output);%>);
        var txBoxDatabaseName = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(TxBoxDatabaseName.ClientID),Response.Output);%>);
        var txBoxDatabaseAccount = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(TxBoxDatabaseAccount.ClientID),Response.Output);%>);
        var txBoxDatabasePassword = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(TxBoxDatabasePassword.ClientID),Response.Output);%>);
        
        var radWindowsAuth = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(RadWindowsAuth.ClientID),Response.Output);%>);
        var radSqlAuth = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(RadSqlAuth.ClientID),Response.Output);%>);
        
        var reqValDatabaseServer = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(ReqValDatabaseServer.ClientID),Response.Output);%>);
        var reqValDatabaseName = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(ReqValDatabaseName.ClientID),Response.Output);%>);
        var reqValDatabaseAccount = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(ReqValDatabaseAccount.ClientID),Response.Output);%>);
        var reqValDatabasePassword = document.getElementById(<%SPHttpUtility.AddQuote(SPHttpUtility.NoEncode(ReqValDatabasePassword.ClientID),Response.Output);%>);
        
        
        txBoxDatabaseServer.disabled = !bProviderEnabled;
        txBoxDatabaseName.disabled = !bProviderEnabled;
        radSqlAuth.disabled = !bProviderEnabled;
        radWindowsAuth.disabled = !bProviderEnabled;
        
        if(!bProviderEnabled || !radSqlAuth.checked) {
            txBoxDatabaseAccount.disabled = true;
            txBoxDatabasePassword.disabled = true;
        }
        else {
            txBoxDatabaseAccount.disabled = false;
            txBoxDatabasePassword.disabled = false;
        }
        
        ValidatorEnable(reqValDatabaseServer, bProviderEnabled);
        ValidatorEnable(reqValDatabaseName, bProviderEnabled);
        
        ValidatorEnable(reqValDatabaseAccount, !txBoxDatabaseAccount.disabled);
        ValidatorEnable(reqValDatabasePassword, !txBoxDatabasePassword.disabled);
    }

</script>

<wssuc:InputFormSection title="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_Title%>"
    description="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_Desc%>"
    runat="server" collapsible="true">
    <template_inputformcontrols>
		<wssuc:InputFormControl runat="server">
			<Template_Control>
				<SharePoint:InputFormCheckBox ID="ChkBoxSqlProviderEnabled"
										  LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_AlphaLogProviderSection_EnabledLabel%>"
										  runat="server"/>
			</Template_Control>
		</wssuc:InputFormControl>
		<tr> <td class="ms-sectionline" height=1 colspan=2 class="ms-widthControl"><img src="/_layouts/images/blank.gif" width="1" height="1" alt=""></td> </tr>
		<TR><TD><IMG SRC="/_layouts/images/blank.gif" width=1 height=5 alt=""></TD></TR>
		<wssuc:InputFormControl runat="server"
			LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseServer%>" >
			<Template_control>
				<SharePoint:InputFormTextBox Title="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseServer%>" class="ms-input" ID="TxBoxDatabaseServer" Columns="35" Runat="server" MaxLength=255 size=25 />
				<SharePoint:InputFormRequiredFieldValidator
					ID="ReqValDatabaseServer"
					ControlToValidate="TxBoxDatabaseServer"
					ErrorMessage="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseServer_Missing%>"
					Runat="server" />
			</Template_control>
		</wssuc:InputFormControl>
		<wssuc:InputFormControl runat="server"
			LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseName%>" >
			<Template_control>
				<SharePoint:InputFormTextBox Title="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseName%>" class="ms-input" ID="TxBoxDatabaseName" Columns="35" Runat="server" MaxLength=64 size="25" />
				<SharePoint:InputFormRequiredFieldValidator
					ID="ReqValDatabaseName"
					ControlToValidate="TxBoxDatabaseName"
					ErrorMessage="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseName_Missing%>"
					Runat="server" />
				<SharePoint:InputFormCustomValidator ID="DatabaseValidator"
					ControlToValidate="TxBoxDatabaseName"
					OnServerValidate="ValidateDatabase"
					runat="server"/>
			</Template_control>
		</wssuc:InputFormControl>
		<wssuc:InputFormControl runat="server"
			LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseAuthentication%>"/>
		<SharePoint:InputFormRadioButton runat="server" id="RadWindowsAuth" Checked="true"
			LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseWindowsAuth%>" GroupName="SqlProviderSection_DatabaseAuthentication" />
		<SharePoint:InputFormRadioButton runat="server" id="RadSqlAuth"
			LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseSqlAuth%>" GroupName="SqlProviderSection_DatabaseAuthentication" >
			<table border="0" width="100%" cellspacing="0" cellpadding="0" class="ms-authoringcontrols">
				<wssuc:InputFormControl runat="server"
					LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseAccount%>" >
					<Template_control>
						<SharePoint:InputFormTextBox Title="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseAccount%>" class="ms-input" ID="TxBoxDatabaseAccount" Columns="35" Runat="server" MaxLength=255 size="25" />
						<SharePoint:InputFormRequiredFieldValidator
							ID="ReqValDatabaseAccount"
							ControlToValidate="TxBoxDatabaseAccount"
							ErrorMessage="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabaseAccount_Missing%>"
							Runat="server" />
					</Template_control>
				</wssuc:InputFormControl>
				<wssuc:InputFormControl runat="server"
					LabelText="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabasePassword%>" >
					<Template_control>
						<SharePoint:InputFormTextBox Title="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabasePassword%>" class="ms-input" ID="TxBoxDatabasePassword" Columns="35" Runat="server" MaxLength=64 TextMode="Password" size="25" />
						<SharePoint:InputFormRequiredFieldValidator
							ID="ReqValDatabasePassword"
							ControlToValidate="TxBoxDatabasePassword"
							ErrorMessage="<%$Resources:AlphaMosaikLogger, ConfigurePage_SqlProviderSection_DatabasePassword_Missing%>"
							Runat="server" />
					</Template_control>
				</wssuc:InputFormControl>
			</table>
		</SharePoint:InputFormRadioButton>
	</template_inputformcontrols>
</wssuc:InputFormSection>