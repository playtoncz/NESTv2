<%@ Reference Control="~/wucOdpovedi.ascx" %>
<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="dotazMultiple" CodeFile="dotazMultiple.aspx.vb" CodeFileBaseClass="cBasePage" %>
<%@ Register TagPrefix="uc1" TagName="wucOdpovedi" Src="wucOdpovedi.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<script>
		  function Create_hodnota_text()
		  {
		    
    result = '';
    <%=GetScript()%>
    
    
    dok = document.getElementById('<%=hodnota_text.ClientID%>');
    
    dok.value = result;

  };
  
  function Napln()
  {
	dok = document.getElementById('<%=hodnota_text.ClientID%>');
	if (dok.value != ""){
		var hodnoty = dok.value.split("|");
		<%=GetScript2()%>
	};
  };
		</script>
		
		<div class="panel">
					<div class="panelNadpis" id="nadpis" runat="server"></div>
					<div class="panelObsah">
						<div class="panelOdpovedi"><uc1:wucOdpovedi id="WucOdpovedi1" runat="server"></uc1:wucOdpovedi></div> 
						<div class="komentar"><asp:Label id="lblKomentar" runat="server">Label</asp:Label></div>
						<table cellpadding="0" cellspacing="0">
							<tr>
								<td><DIV id="hodnoty" runat="server"></DIV>
									<div class="rozsah"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, RozsahVah %>"></asp:literal>:
										<asp:Label id="lblRozsah" runat="server">Label</asp:Label></div>
								</td>
								<td valign="top">&nbsp;<asp:Button id="cmdOdeslat" runat="server" Text="<%$ Resources:AppResource, Odeslat %>" CssClass="button"></asp:Button></td>
							</tr>
						</table>
						<div id="vyrovnatVysku" runat="server"></div>
					</div>
				</div>
				<input id="hodnota_text" runat="server" type="hidden" NAME="hodnota_text">
</asp:Content>
