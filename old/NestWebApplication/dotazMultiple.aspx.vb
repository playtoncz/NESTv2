
Partial Class dotazMultiple
    Inherits cBasePage

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub
    'Protected WithEvents id As System.Web.UI.WebControls.TextBox


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here
        cmdOdeslat.Attributes.Add("onclick", "JavaScript:Create_hodnota_text();")
        CType(Master, BasePage).BodyScripts = "onLoad='Napln()'"
        wucOdpovedi1.TypAtributu = "m"
        vyrovnatVysku.InnerHtml = "<div style=""height:" & Me.PocetTlacitek * 15 & "px;"">&nbsp;</div>"
        CType(Master, BasePage).strankaKonzultace = True
        If Not Page.IsPostBack Then
            RenderData()
        End If
    End Sub

    Private Sub RenderData()
        Dim id_dotazu As String
        id_dotazu = Session("id_dotazu")
        Dim atribut As NestBase.AtributMultiple
        atribut = konzultace.BZ.Atributy(id_dotazu)
        nadpis.InnerHtml = atribut.Jmeno
        lblKomentar.Text = atribut.Komentar
        lblRozsah.Text = "-" & konzultace.BZ.RozsahVah.ToString & " : " & konzultace.BZ.RozsahVah.ToString
        Session("atribut") = atribut
        Dim result As String = ""
        Dim pocet As Integer = 0
        result += "<table cellpadding=""0"" cellspacing=""0"">"
        'For Each str As String In atribut.SeznamHodnot
        For Each vyrok As NestBase.Vyrok In atribut.Vyroky
            pocet += 1
            result += "<tr><td>" & vyrok.Jmeno & "</td><td>&nbsp;<input type=""text"" id=""text" & CStr(pocet) & """></td></tr>"
        Next
        result += "</table>"
        hodnoty.InnerHtml = result

    End Sub

    Public Function GetScript() As String
        Dim result As String = ""
        Dim id_dotazu As String
        id_dotazu = Session("id_dotazu")
        Dim atribut As NestBase.AtributMultiple
        atribut = konzultace.BZ.Atributy(id_dotazu)
        Dim pocet As Integer = 0
        For Each str As String In atribut.SeznamHodnot
            pocet += 1
            result += "result = result + document.getElementById('text" & CStr(pocet) & "').value + ""|"";" & vbCrLf

        Next
        Return result
    End Function

    Public Function GetScript2() As String
        Dim result As String = ""
        Dim id_dotazu As String
        id_dotazu = Session("id_dotazu")
        Dim atribut As NestBase.AtributMultiple
        atribut = konzultace.BZ.Atributy(id_dotazu)
        Dim pocet As Integer = 0
        For Each str As String In atribut.SeznamHodnot
            pocet += 1
            result += "document.getElementById('text" & CStr(pocet) & "').value = hodnoty[" & CStr(pocet - 1) & "];"

        Next
        Return result
    End Function


    Private Sub cmdOdeslat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOdeslat.Click
        Dim atribut As NestBase.AtributMultiple
        atribut = Session("atribut")

        Dim str As String = hodnota_text.Value
        If str.StartsWith("|") Then str = "default" + str
        While str.IndexOf("||") > -1
            str = str.Replace("||", "|default|")
        End While



        Select Case atribut.VlozVahy(str, konzultace.BZ.RozsahVah, konzultace.BZ.DefaultVaha)
            Case NestBase.Atribut.enmVlozeniHodnoty.enmVahaMimoRozsah
                CType(Me.Master, BasePage).ErrorMessage = GetText("VahaJeMimoRozsah")
            Case NestBase.Atribut.enmVlozeniHodnoty.enmChyba
                CType(Me.Master, BasePage).ErrorMessage = GetText("VahuSeNepodariloNacist")
            Case NestBase.Atribut.enmVlozeniHodnoty.enmOK
                Dim redirectString As String
                Dim id_dotazu As String
                Dim result As String

                If konzultace.Konzultuj(redirectString, result, id_dotazu) Then
                    Session("konzultace") = konzultace
                    Session("id_dotazu") = id_dotazu
                    Session("result") = result
                    Response.Redirect(redirectString)
                Else
                    CType(Me.Master, BasePage).ErrorMessage = konzultace.LastError.UserMessage
                End If
        End Select


    End Sub
    Private Sub cmdOdpovediClick() Handles wucOdpovedi1.OdpovedClick
        Dim id_dotazu As String
        id_dotazu = Session("id_dotazu")
        Dim atribut As NestBase.AtributMultiple
        atribut = konzultace.BZ.Atributy(id_dotazu)
        hodnota_text.Value = ""
        For i As Integer = 1 To atribut.SeznamHodnot.Count
            hodnota_text.Value += wucOdpovedi1.VybranaHodnota & "|"
        Next
        cmdOdeslat_Click(Nothing, Nothing)
    End Sub
End Class

