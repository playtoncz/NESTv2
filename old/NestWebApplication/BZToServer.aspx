<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="NestWebApplication.BZToServer" Codebehind="BZToServer.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<div id="Div1" runat="server" class="panel">
					<div class="panelNadpis">Báze znalostí:</div>
					<div class="panelObsah">
						Vyberte bázi znalostí:<br>
						<INPUT type="file" id="efile" runat="server" size="30" NAME="efile"><br>
						<asp:Button id="cmdNahraj" runat="server" Text="Nahraj" CssClass="button"></asp:Button>
					</div>
				</div>
</asp:Content>