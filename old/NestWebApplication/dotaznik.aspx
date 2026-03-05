<%@ Page Language="VB" MasterPageFile="~/BasePage.master" AutoEventWireup="false" Inherits="NestWebApplication.dotaznik"  Codebehind="dotaznik.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="obsah" Runat="Server">
  <script>
		  function Create_hodnota_text()
		  {
		  
		    
    result = '';
    <%=GetJavascript() %>
    
    dok = document.getElementById('<%=hodnota_text.ClientID%>');
    
    dok.value = result;

  };
  
  function Napln()
  {
	dok = document.getElementById('<%=hodnota_text.ClientID%>');	
	if (dok.value != ""){
		var hodnoty = dok.value.split("~");
		<%=GetJavaScript2()%>
	};
  };
  
  		</script>
  
  <div id="dotazy_div" runat="server">
  
  </div>
  <asp:Button id="cmdOdeslat" runat="server" Text="<%$ Resources:AppResource, Odeslat %>" CssClass="button"></asp:Button>
  <input id="hodnota_text" runat="server" type="hidden" NAME="hodnota_text">
</asp:Content>

