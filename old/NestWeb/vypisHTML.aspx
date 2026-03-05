<%@ Page Language="VB" MasterPageFile="~/BasePage.master" AutoEventWireup="false" CodeFile="vypisHTML.aspx.vb" Inherits="vypisHTML"%>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<div class="panel" style="position: fixed; width:150px; margin-left: 570px;">
					<div class="panelNadpis"><asp:literal ID="Literal4" runat="server" Text="<%$ Resources:AppResource, Navigace %>"></asp:literal></div>
					<div class="panelObsah">
						<a href="#globalni_parametry"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, GlobalniParametry %>"></asp:literal></a><br />
						<a href="#atributy"><asp:literal ID="Literal2" runat="server" Text="<%$ Resources:AppResource, Atributy %>"></asp:literal></a><br />
						<a href="#kontexty"><asp:literal ID="Literal3" runat="server" Text="<%$ Resources:AppResource, Kontexty %>"></asp:literal></a><br />
						<a href="#pravidla"><asp:literal ID="Literal5" runat="server" Text="<%$ Resources:AppResource, Pravidla %>"></asp:literal></a><br />
						<a href="#integritni_omezeni"><asp:literal ID="Literal6" runat="server" Text="<%$ Resources:AppResource, IntegritniOmezeni %>"></asp:literal></a><br />
					</div>
				</div>

<h2><asp:literal ID="Literal7" runat="server" Text="<%$ Resources:AppResource, VypisBazeZnalosti %>"></asp:literal></h2>

				
<div id="vypis" runat="server"></div>
</asp:Content>

