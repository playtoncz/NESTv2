<%@ Reference Control="~/wucOdpovedi.ascx" %>
<%@ Register TagPrefix="uc1" TagName="wucOdpovedi" Src="wucOdpovedi.ascx" %>
<%@ Page Language="vb"  MasterPageFile="~/BasePage.master" AutoEventWireup="false" Inherits="dotazBinary" CodeFile="dotazBinary.aspx.vb" CodeFileBaseClass="cBasePage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
<script language="javascript">
 <!--
 function focus(){
    document.getElementById("<%=GetIDeHodnota() %>").focus();
 }
 //-->
</script>
<asp:Panel ID="Panel1" runat="server" DefaultButton="cmdOdeslat">
<div class="panel">
					<div class="panelNadpis" id="nadpis" runat="server"></div>
					<div class="panelObsah">
						<div class="panelOdpovedi"><uc1:wucodpovedi id="WucOdpovedi1" runat="server"></uc1:wucodpovedi></div>
						<div class="komentar"><asp:label id="lblKomentar" runat="server">Label</asp:label></div>
						<span id="insertWeight" runat="server">
							<div><asp:textbox id="eHodnota" runat="server"></asp:textbox><asp:button id="cmdOdeslat" runat="server" Text="<%$ Resources:AppResource, Odeslat %>" CssClass="button"></asp:button></div>
							<div class="rozsah"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, RozsahVah %>"></asp:literal>:
								<asp:label id="lblRozsah" runat="server">Label</asp:label></div>
						</span>
						<div id="vyrovnatVysku" runat="server"></div>
					</div>
				</div>
</asp:Panel>				
<script language="javascript">focus();</script>				
</asp:Content>