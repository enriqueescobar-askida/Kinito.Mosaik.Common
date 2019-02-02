<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="About.aspx.cs" Inherits="Formulaire_Premier.About" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Test Page
    </h2>
    <div class="accountInfo">
        <fieldset class="register">
            <legend>Test Fields</legend>
            <p>
                <asp:Label ID="PremierLabel" runat="server" AssociatedControlID="Premier">Premier</asp:Label>
                <asp:TextBox ID="Premier" runat="server" CssClass="textEntry"></asp:TextBox>
            </p>
            <p>
                <asp:Label ID="DeuxiemeLabel" runat="server" AssociatedControlID="Deuxieme">Deuxieme</asp:Label>
                <asp:TextBox ID="Deuxieme" runat="server" CssClass="textEntry"></asp:TextBox>
            </p>
            <p>
                <asp:Label ID="TroisiemeLabel" runat="server" AssociatedControlID="Troisieme">Troisieme</asp:Label>
                <asp:TextBox ID="Troisieme" runat="server" CssClass="textEntry"></asp:TextBox>
            </p>
        </fieldset>
        <p class="submitButton">
            <asp:Button ID="StartTestButton" runat="server" CommandName="StartTest" Text="Test" CausesValidation="true"/>
        </p>
    </div>
</asp:Content>
