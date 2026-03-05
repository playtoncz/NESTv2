<%@ Page Language="VB" MasterPageFile="~/BasePage.master" AutoEventWireup="false" CodeFile="statistiky.aspx.vb" Inherits="statistiky" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<div class="panel">
					<div class="panelNadpis"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, menu_statistiky %>"></asp:literal></div>
					<div class="panelObsah">
<div class="statistiky" id="statistiky_div" runat="server"></div>
</div> 
</div> 
</asp:Content>

