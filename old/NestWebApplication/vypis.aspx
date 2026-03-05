<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="NestWebApplication.vypis" Codebehind="vypis.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<h2><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, VypisZdrojovehoKodu %>"></asp:literal></h2>
				<asp:TextBox id="eVypis" runat="server" TextMode="MultiLine" Rows="20" Columns="80"></asp:TextBox>
</asp:Content>