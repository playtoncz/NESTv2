<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="vypis" CodeFile="vypis.aspx.vb" CodeFileBaseClass="cBasePage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<h2><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, VypisZdrojovehoKodu %>"></asp:literal></h2>
				<asp:TextBox id="eVypis" runat="server" TextMode="MultiLine" Rows="20" Columns="80"></asp:TextBox>
</asp:Content>