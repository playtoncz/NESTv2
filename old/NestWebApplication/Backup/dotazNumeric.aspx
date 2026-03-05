<%@ Reference Control="~/wucOdpovedi.ascx" %>
<%@ Page MasterPageFile="~/BasePage.master" Language="vb" AutoEventWireup="false" Inherits="NestWebApplication.dotazNumeric" Codebehind="dotazNumeric.aspx.vb" %>
<%@ Register TagPrefix="uc1" TagName="wucOdpovedi" Src="wucOdpovedi.ascx" %>
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
						<div><asp:textbox id="eHodnota" runat="server"></asp:textbox><asp:button id="cmdOdeslat" runat="server" CssClass="button" Text="<%$ Resources:AppResource, Odeslat %>"></asp:button></div>
						<div class="rozsah"><asp:literal ID="Literal9" runat="server" Text="<%$ Resources:AppResource, DolniMez %>"></asp:literal>: 
							<asp:label id="lblDolniMez" runat="server" CssClass="rozsah">Label</asp:label>
							<asp:literal ID="Literal1" runat="server" Text="<%$ Resources:AppResource, HorniMez %>"></asp:literal>: 
							<asp:label id="lblHorniMez" runat="server" CssClass="rozsah">Label</asp:label></div>
						<div id="vyrovnatVysku" runat="server"></div>
					</div>
				</div>
</asp:Panel>
<script language="javascript">focus();</script>				
</asp:Content>