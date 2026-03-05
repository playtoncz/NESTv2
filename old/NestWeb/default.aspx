<%@ Page Language="vb" MasterPageFile="~/BasePage.master" AutoEventWireup="false" Inherits="_default" CodeFile="default.aspx.vb" CodeFileBaseClass="cBasePage"%>

<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
    
    
    <script language="javascript" src="functions.js"></script>
				<table cellSpacing="0" cellPadding="0" width="100%" id="tableNahravaniBZ" runat="server">
					<tr>
						<td vAlign="top">
							<div class="panel" id="Div1" runat="server">
								<div class="panelNadpis"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, vlastniBazeZnalosti %>"></asp:literal></div>
								<div class="panelObsah"><asp:literal ID="Literal2" runat="server" Text="<%$ Resources:AppResource, vyberteBaziZnalosti %>"></asp:literal><br>
									<INPUT id="efile" type="file" size="30" runat="server"><br>
									<span id="cbrInput" runat="server"><asp:literal ID="Literal3" runat="server" Text="<%$ Resources:AppResource, vyberteSkladPripadu %>"></asp:literal><br>
									<INPUT id="nscfile" type="file" size="30" runat="server"><br></span>
									
									<asp:button id="cmdNahraj" runat="server" CssClass="button" Text="<%$ Resources:AppResource, Nahraj %>"></asp:button><asp:checkbox id="ePridat" runat="server" Text="<%$ Resources:AppResource, Pridej %>"></asp:checkbox></div>
							</div>
						</td>
						<td>&nbsp;</td>
						<td vAlign="top">
							<div class="panel" id="Div2" runat="server">
								<div class="panelNadpis"><asp:literal ID="Literal4" runat="server" Text="<%$ Resources:AppResource, bazeZnalostiDostupneNaServeru %>"></asp:literal></div>
								<div class="panelObsah">
									<ul>
										<%=GetBZOnServer()%>
									</ul>
								</div>
							</div>
						</td>
					</tr>
				</table>				
				<div class="panel" id="informaceOBazi" runat="server">
					<div class="panelNadpis"><asp:literal ID="Literal5" runat="server" Text="<%$ Resources:AppResource, informaceOBaziZnalosti %>"></asp:literal></div>
					<div class="panelObsah">
						<table cellSpacing="0" cellPadding="2">
							<tr>
								<td><asp:literal ID="Literal6" runat="server" Text="<%$ Resources:AppResource, expert %>"></asp:literal>
								</td>
								<td>
									<div id="expert" runat="server"></div>
								</td>
							</tr>
							<tr>
								<td><asp:literal ID="Literal7" runat="server" Text="<%$ Resources:AppResource, znalostniInzenyr %>"></asp:literal>
								</td>
								<td>
									<div id="inzenyr" runat="server"></div>
								</td>
							</tr>
							<tr>
								<td><asp:literal ID="Literal8" runat="server" Text="<%$ Resources:AppResource, datumVytvoreni %>"></asp:literal>
								</td>
								<td>
									<div id="datum" runat="server"></div>
								</td>
							</tr>
							<tr>
								<td><asp:literal ID="Literal9" runat="server" Text="<%$ Resources:AppResource, popis %>"></asp:literal>
								</td>
								<td>
									<div id="popis" runat="server"></div>
								</td>
							</tr>
						</table>
						<asp:button id="cmdKonzultuj" runat="server" CssClass="button" Text="<%$ Resources:AppResource, Konzultuj %>"></asp:button></div>
						<div class="panel">
					<div class="panelNadpis"><a id="prepinacdalsinastaveni" style="text-decoration:none; color:white;" href="javascript:Zmiz('dalsinastaveni');">+</a>
						<asp:literal ID="Literal10" runat="server" Text="<%$ Resources:AppResource, dalsiNastaveni %>"></asp:literal></div>
					<div class="panelObsah" style="visibility:hidden;position:absolute;" id="ddalsinastaveni">
                        <asp:literal ID="Literal11" runat="server" Text="<%$ Resources:AppResource, rezimKonzultace %>"></asp:literal>
                        <asp:RadioButtonList ID="eRezimKonzultace" runat="server">
                            <asp:ListItem Value="Dialog" Selected="True" Text="<%$ Resources:AppResource, dialog %>"></asp:ListItem>
                            <asp:ListItem Value="Dotaznik" Text="<%$ Resources:AppResource, dotaznik %>"></asp:ListItem>
                        </asp:RadioButtonList>
                        <span id="odpovediInput" runat="server"><asp:literal ID="Literal12" runat="server" Text="<%$ Resources:AppResource, NacistOdpovediZeSouboru %>"></asp:literal><br/>
									<INPUT id="odpovediFile" type="file" size="30" runat="server"><br></span>
					</div>
					
					
					
				</div>
				</div>
    
</asp:Content>

