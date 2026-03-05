<%@ Reference Control="~/wucOdpovedi.ascx" %>
<%@ Register TagPrefix="uc1" TagName="wucOdpovedi" Src="wucOdpovedi.ascx" %>
<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="dotazSingle" CodeFile="dotazSingle.aspx.vb" CodeFileBaseClass="cBasePage" %>
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
						<div class="panelOdpovedi"><uc1:wucOdpovedi id="WucOdpovedi1" runat="server"></uc1:wucOdpovedi></div>
						<div class="komentar"><asp:Label id="lblKomentar" runat="server">Label</asp:Label></div>
						<div><asp:DropDownList id="eVyber" runat="server"></asp:DropDownList>
							<asp:TextBox id="eHodnota" runat="server"></asp:TextBox>
							<asp:Button id="cmdOdeslat" runat="server" Text="<%$ Resources:AppResource, Odeslat %>" CssClass="button"></asp:Button></div>
						<div class="rozsah"><asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, RozsahVah %>"></asp:literal>:
							<asp:Label id="lblRozsah" runat="server">Label</asp:Label></div>
						<div id="vyrovnatVysku" runat="server"></div>
					</div>
				</div>
</asp:Panel>				
<script language="javascript">focus();</script>
</asp:Content>

