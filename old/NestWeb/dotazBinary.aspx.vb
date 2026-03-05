
Partial Class dotazBinary
    Inherits cBasePage

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here
        wucOdpovedi1.TypAtributu = "b"
        vyrovnatVysku.InnerHtml = "<div style=""height:" & Me.PocetTlacitek * 15 & "px;"">&nbsp;</div>"
        CType(Master, BasePage).strankaKonzultace = True
        If Not Page.IsPostBack Then
            RenderData()
        End If
    End Sub

    Private Sub RenderData()
        Dim id_dotazu As String
        id_dotazu = Session("id_dotazu")
        Dim atribut As NestBase.Atribut
        atribut = konzultace.BZ.Atributy(id_dotazu)
        nadpis.InnerHtml = atribut.Jmeno
        lblKomentar.Text = atribut.Komentar
        lblRozsah.Text = "-" & konzultace.BZ.RozsahVah.ToString & " : " & konzultace.BZ.RozsahVah.ToString
        Session("atribut") = atribut

        If Not Me.ZobrazitWeightEdit(NestBase.Enums.enmTypAtributu.enmBinary) Then
            insertWeight.Visible = False
        End If
    End Sub

    Private Sub cmdOdeslat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOdeslat.Click
        Dim atribut As NestBase.AtributBinary
        atribut = Session("atribut")
        Select Case atribut.VlozVahu(eHodnota.Text, konzultace.BZ.DefaultVaha, konzultace.BZ.RozsahVah)
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
        eHodnota.Text = wucOdpovedi1.VybranaHodnota
        cmdOdeslat_Click(Nothing, Nothing)
    End Sub

    Public Function GetIDeHodnota() As String
        Return eHodnota.ClientID
    End Function
End Class

