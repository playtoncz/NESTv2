<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="NestWebApplication.errorPage" Codebehind="errorPage.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<p>Došlo k neoèekávané chybì.</p>
				<p>Podrobnosti o chybì:</p>
				<div id="lblerror" runat="server"></div>
</asp:Content>
