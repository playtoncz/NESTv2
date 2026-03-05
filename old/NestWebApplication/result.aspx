<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="NestWebApplication.Migrated_result" Codebehind="result.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<script language="javascript" src="functions.js"></script>
       
       
     
     
     
		
		
		<div class="panel">
					<div class="panelNadpis"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, Vysledky %>"></asp:literal></div>
					<div class="panelObsah">
						<div id="vysledky" runat="server"></div>
					</div>
				</div>
				<div align="right"><a href="dotaznik.aspx"><asp:literal ID="Literal2" runat="server" Text="<%$ Resources:AppResource, UpravitOdpovedi %>"></asp:literal></a></div>
				<div class="panel" id="panelIntegritniOmezeni" runat="server">
					<div class="panelNadpis"><asp:literal ID="Literal3" runat="server" Text="<%$ Resources:AppResource, PorusenaIntOm %>"></asp:literal></div>
					<div class="panelObsah">
						<div id="IntegritniOmezeniObsah" runat="server"></div>
					</div>
				</div>
				<div class="panel">
					<div class="panelNadpis"><a id="prepinacvsechnyvyroky" style="text-decoration:none; color:white;" href="javascript:Zmiz('vsechnyvyroky');">+</a>
						<asp:literal ID="Literal4" runat="server" Text="<%$ Resources:AppResource, VsechnyVyroky %>"></asp:literal>
                        </div>
					<div class="panelObsah" style="visibility:hidden;position:absolute;" id="dvsechnyvyroky">
					<asp:literal ID="Literal5" runat="server" Text="<%$ Resources:AppResource, SetriditDle %>"></asp:literal>: <asp:LinkButton ID="cmdSortJmeno" runat="server" Text="<%$ Resources:AppResource, SetriditDleJmena %>"></asp:LinkButton> <asp:LinkButton ID="cmdSortMinVaha" runat="server" Text="<%$ Resources:AppResource, SetriditDleMinimalniVahy %>"></asp:LinkButton> <asp:LinkButton ID="cmdSortMaxVahy" runat="server" text="<%$ Resources:AppResource, SetriditDleMaximalniVahy %>"></asp:LinkButton>
						<div id="vsechnyVyroky" runat="server"></div>
					</div>
				</div>
				
				<div class="panel" id="panelPripady" runat="server">
					<div class="panelNadpis"><a id="prepinacpripady" style="text-decoration:none; color:white;" href="javascript:Zmiz('pripady');">+</a>
						<asp:literal ID="Literal6" runat="server" Text="<%$ Resources:AppResource, Pripady %>"></asp:literal>
                        </div>
					<div class="panelObsah" style="visibility:hidden;position:absolute;" id="dpripady">					
						<asp:Label ID="pripadyVahy" runat="server"></asp:Label>
					</div>
				</div>
				
				<div id="VolatSrcipty" runat="server"></div>
</asp:Content>

