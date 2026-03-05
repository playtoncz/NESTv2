<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="errorPage" CodeFile="errorPage.aspx.vb" CodeFileBaseClass="cBasePage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<p>Došlo k neoèekávané chybì.</p>
				<p>Podrobnosti o chybì:</p>
				<div id="lblerror" runat="server"></div>
</asp:Content>
