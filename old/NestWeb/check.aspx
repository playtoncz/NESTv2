<%@ Page Language="VB" MasterPageFile="~/BasePage.master" AutoEventWireup="false" CodeFile="check.aspx.vb" Inherits="check" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<div class="panel" id="Div1" runat="server">
								<div class="panelNadpis"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, kontrolaBZ %>"></asp:literal>:</div>
								<div class="panelObsah"><asp:literal ID="Literal2" runat="server" Text="<%$ Resources:AppResource, vyberteBaziZnalosti %>"></asp:literal><br>
									<INPUT id="efile" type="file" size="30" runat="server"><br>
									<asp:button id="cmdNahraj" runat="server" CssClass="button" Text="<%$ Resources:AppResource, Zkontroluj %>"></asp:button>
								    <br />
								    <br />
								
    <asp:Label ID="lblMessage" runat="server" Text="Label"></asp:Label></div></div>
</asp:Content>

