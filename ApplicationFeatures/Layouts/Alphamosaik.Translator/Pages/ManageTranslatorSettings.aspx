<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageTranslatorSettings.aspx.cs" Inherits="Alphamosaik.Translator.ApplicationFeatures.Layouts.Alphamosaik.Translator.Pages.ManageTranslatorSettings" DynamicMasterPageFile="~masterurl/default.master" Debug="true" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
<script type="text/javascript">
    var tEnabledChanged = false; // Variable globale qui indique si des changements ont été effectués sur le formulaire
    
    // Indique que des changements ont été effectués
    function setEnabledChanged() {
        tEnabledChanged = true;
    }

    // Ferme la ModalDialog et soumet les résultats des modifications ou les traitements à effectuer
    function submitChanges() {
        if(tEnabledChanged == true) {
            var elE = document.getElementById(radioButtonEnabledId);

            if(elE.checked==true) {
                SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.OK, 'EnableItemTradFromList');
            }
            else {
                SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.OK, 'DisableItemTradFromList');
            }
        }
        else {
            SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.OK, null);
        }
    }

    // Ferme la ModalDialog en indiquant que l'opération a été annulée
    function cancelChanges() {
        SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.cancel, null);
    }

    // Affiche le message de status
    function setStatus() {
        SP.UI.Status.removeAllStatus(true);

        if (itemTradFromListEnabled == true) {
            var sId = SP.UI.Status.addStatus('Oceanik state : ', 'Oceanik features are enabled on this list', '', true);
            SP.UI.Status.setStatusPriColor(sId, 'green');
        }
        else {
            var sId = SP.UI.Status.addStatus('Oceanik state : ', 'Oceanik features are disabled on this list', '', true);
            SP.UI.Status.setStatusPriColor(sId, 'yellow');
        }
    }

    function launcher() {
        setStatus();
    }

    ExecuteOrDelayUntilScriptLoaded(launcher, 'SP.js');
</script>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <div>
        <asp:Label ID="LabelMessage" runat="server" />
    </div>

    <div>
        <table cellpadding="0" cellspacing="0" style="width:100%">
            <tr>
                <td colspan="2" class="ms-sectionline" style="height: 1px">
                    <img alt="" src="/_layouts/images/blank.gif" width="1" height="1"/>
                </td>
            </tr>
            <tr>
                <td style="width: 60%" class="ms-sectionheader">
                    <h3 class="ms-standardheader ms-inputformheader">Activate Oceanik features</h3>
                </td>
                <td rowspan="2" style="width: 40%" class="ms-authoringcontrols ms-inputformcontrols">
                    <input type="radio" name="TranslatorEnabled" id="RadioButtonTranslatorEnabled" onchange="javascript:setEnabledChanged();" runat="server" title="Enabled"/>Enabled<br />
                    <input type="radio" name="TranslatorEnabled" id="RadioButtonTranslatorDisabled" onchange="javascript:setEnabledChanged();" runat="server" title="Disabled"/>Disabled
                </td>
            </tr>
            <tr>
                <td style="width: 60%" class="ms-descriptiontext ms-inputformdescription">
                    You have to enable Oceanik features on this list if you want to filter elements depending of their native language.
                </td>
            </tr>
        </table>
    </div>

    <div>
        <table style="width:100%">
            <tr>
                <td class="ms-dialogButtonSection">
                    <table style="width:100%">
                        <tr>
                            <td style="width: 100%">&nbsp;</td>
                            <td>
                                <asp:Button ID="ButtonSubmit" runat="server" Text="OK" OnClientClick="javascript: submitChanges();" CssClass="ms-ButtonHeightWidth" />
                            </td>
                            <td>
                                <asp:Button ID="ButtonCancel" runat="server" Text="Cancel" OnClientClick="javascript: cancelChanges();" CssClass="ms-ButtonHeightWidth"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Manage Oceanik Settings
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Oceanik settings
</asp:Content>
