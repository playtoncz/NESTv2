

Partial Class _default
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

    Private NahratBZ As String = ""

    'rivate nahravatNSC As Boolean = False

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here

        If Me.Theme <> "ThemeBase" Then
            tableNahravaniBZ.Visible = False
        End If

        If Not Page.IsPostBack Then
            If Theme <> "ThemeBase" Then
                Select Case Me.userSetting.Language
                    Case NestBase.Enums.enmLanguage.anglictina
                        NahratBZ = "../App_Themes/" & Theme & "/BZ_en.xml"
                        Dim fi As New IO.FileInfo(Server.MapPath("App_Themes/" & Theme & "/BZ_en.xml"))
                        If Not fi.Exists Then
                            NahratBZ = "../App_Themes/" & Theme & "/BZ.xml"
                        End If
                    Case NestBase.Enums.enmLanguage.cestina
                        NahratBZ = "../App_Themes/" & Theme & "/BZ.xml"
                End Select

                NahrajBZ()
            End If
            If Request.QueryString("clear") = "true" Then
                konzultace = New konzultace(Me.Environment, Me.userSetting.Language)
                Session("konzultace") = konzultace
                Response.Redirect("default.aspx")
            Else
                NahratBZ = Request.QueryString("bz")
                If NahratBZ <> "" Then
                    NahrajBZ()
                End If
            End If
        End If





        InfoOBazi()


    End Sub

    Private Function NahrajTlacitkaZBZ(ByVal BZXML As String, ByRef TextyTlacitek As Collection, ByRef HodnotyTlacitek As Collection, ByRef typyAtributu As Collection) As Boolean
        Dim xmldoc As New System.Xml.XmlDocument
        Try
            xmldoc.LoadXml(BZXML.Replace("<!DOCTYPE base SYSTEM ""base.dtd"">", ""))

            Dim element As System.Xml.XmlElement
            If xmldoc.SelectNodes("//buttons").Count = 0 Then Return False
            element = xmldoc.SelectSingleNode("//buttons")
            For Each btn As System.Xml.XmlElement In element.SelectNodes("button")
                Dim hodnota As String
                Dim text As String
                Dim typ As String
                Helper.NactiXMLString(btn, "text", text)
                Helper.NactiXMLString(btn, "value", hodnota)
                Helper.NactiXMLString(btn, "typeofattributes", typ)
                TextyTlacitek.Add(text)
                HodnotyTlacitek.Add(hodnota)
                typyAtributu.Add(typ)
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function NahrajInsertWeights(ByVal BZXML As String, ByRef WeightString As String) As Boolean
        Dim xmldoc As New System.Xml.XmlDocument
        Try
            xmldoc.LoadXml(BZXML.Replace("<!DOCTYPE base SYSTEM ""base.dtd"">", ""))

            Dim element As System.Xml.XmlElement
            If xmldoc.SelectNodes("//insert_weights").Count = 0 Then Return False
            element = xmldoc.SelectSingleNode("//insert_weights")
            WeightString = element.InnerText
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub PripravaTlacitekOdpovedi(ByVal BZXML As String)
        'pripraveni tlacitek odpovedi
        Dim hodnotyTlacitek As New Collection
        Dim textyTlacitek As New Collection
        Dim typyAtributu As New Collection


        If Not NahrajTlacitkaZBZ(BZXML, textyTlacitek, hodnotyTlacitek, typyAtributu) Then
            'standardni
            textyTlacitek.Add(GetText("JisteAno"))
            hodnotyTlacitek.Add(CStr(konzultace.BZ.RozsahVah))
            typyAtributu.Add("bs")
            textyTlacitek.Add(GetText("Jistene"))
            hodnotyTlacitek.Add("-" & CStr(konzultace.BZ.RozsahVah))
            typyAtributu.Add("b")
            textyTlacitek.Add(GetText("Irelevantni"))
            hodnotyTlacitek.Add("irrelevant")
            typyAtributu.Add("bsmn")
            textyTlacitek.Add(GetText("Nevim"))
            hodnotyTlacitek.Add("unknown")
            typyAtributu.Add("bsmn")
        End If
        Session("hodnotyTlacitek") = hodnotyTlacitek
        Session("textyTlacitek") = textyTlacitek
        Session("typyAtributu") = typyAtributu

        'priprava Insert Weights
        Dim weightString As String
        If Not Me.NahrajInsertWeights(BZXML, weightString) Then
            weightString = "bsmn"
        End If
        Session("insertWeight") = weightString

    End Sub

    Private Sub cmdNahraj_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdNahraj.Click

        'Dim fileLen As Integer
        Dim str As String
        Dim err As String = ""
        str = Helper.NactiPostedFile(efile, Server, err)
        If err <> "" Then
            CType(Me.Master, BasePage).ErrorMessage = err
            Exit Sub
        End If


        'nahrani nsc
        Dim strNsc As String = ""

        If nscfile.PostedFile.FileName <> "" Then
            strNsc = Helper.NactiPostedFile(nscfile, Server, err)
        End If
        If err <> "" Then
            CType(Me.Master, BasePage).ErrorMessage = err
            Exit Sub
        End If



        If ePridat.Checked And konzultace.nahranaBaze Then
            konzultace.BazeZnalostiXML = str

            If konzultace.NahrajBZ(True) Then

                Session("konzultace") = konzultace

            Else
                CType(Me.Master, BasePage).ErrorMessage = konzultace.BZ.LastError.UserMessage
            End If
        Else
            konzultace = New konzultace(Environment, Me.userSetting.Language)
            konzultace.BazeZnalostiXML = str.ToString
            If Not String.IsNullOrEmpty(strNsc) Then
                konzultace.NcsXML = strNsc
            End If


            If konzultace.NahrajBZ() Then

                Session("konzultace") = konzultace

            Else
                CType(Me.Master, BasePage).ErrorMessage = konzultace.BZ.LastError.UserMessage
            End If
            End If


            InfoOBazi()

            CType(Master, BasePage).konzultace = konzultace
            CType(Master, BasePage).UkazTlacitka()


            PripravaTlacitekOdpovedi(konzultace.BazeZnalostiXML)

        Select Case konzultace.BZ.InferencniMechanismus
            Case NestBase.Enums.enmInferencniMechanismus.Standardni
                eInferencniMechanismus.SelectedValue = "Standard"
            Case NestBase.Enums.enmInferencniMechanismus.Logicky
                eInferencniMechanismus.SelectedValue = "Logical"
        End Select
    End Sub

    Private Sub InfoOBazi()
        If konzultace.nahranaBaze Then
            expert.InnerHtml = konzultace.BZ.Expert
            inzenyr.InnerHtml = konzultace.BZ.Inzenyr
            datum.InnerHtml = Format(konzultace.BZ.DatumVytvoreni, "d.M.yyyy")

            Dim r As New System.Text.RegularExpressions.Regex("&amp;lt;([^&gt;]+)&amp;gt;")

            Dim popisstr As String
            popisstr = r.Replace(konzultace.BZ.Popis, "<$1>")

            r = New System.Text.RegularExpressions.Regex("(\s|^)([vszk]) (.)")

            popisstr = r.Replace(popisstr, "$1$2&nbsp;$3")

            popis.InnerHtml = popisstr

            'Dim pom As String
            'If konzultace.BZ.SaveBaseToXML(pom) Then
            '    tb.Text = pom
            'Else
            '    tb.Text = konzultace.BZ.LastError.UserMessage
            'End If
            If Not String.IsNullOrEmpty(konzultace.NcsXML) Then
                lblSkladPripaduLoaded.Visible = True
            Else
                lblSkladPripaduLoaded.Visible = False
            End If

            informaceOBazi.Visible = True

            If Not Page.IsPostBack Then
                Select Case konzultace.BZ.InferencniMechanismus
                    Case NestBase.Enums.enmInferencniMechanismus.Standardni
                        eInferencniMechanismus.SelectedValue = "Standard"
                    Case NestBase.Enums.enmInferencniMechanismus.Logicky
                        eInferencniMechanismus.SelectedValue = "Logical"
                End Select
            End If
            
        Else
            informaceOBazi.Visible = False
        End If
    End Sub

    Private Sub cmdKonzultuj_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdKonzultuj.Click
        If odpovediFile.PostedFile.FileName <> "" Then
            Dim err As String = ""
            konzultace.odpovediXML = Helper.NactiPostedFile(odpovediFile, Server, err)
           

        Else
            konzultace.odpovediXML = ""
        End If

        If eInferencniMechanismus.SelectedValue = "Standard" Then
            konzultace.InferencniMechanismus = NestBase.Enums.enmInferencniMechanismus.Standardni
        ElseIf eInferencniMechanismus.SelectedValue = "Logical" Then
            konzultace.InferencniMechanismus = NestBase.Enums.enmInferencniMechanismus.Logicky
        End If

        If eRezimKonzultace.SelectedValue = "Dotaznik" Then
            konzultace.RezimKonzultace = NestWebApplication.konzultace.enmRezimKonzultace.dotaznik
        ElseIf eRezimKonzultace.SelectedValue = "DotaznikSDialogem" Then
            konzultace.RezimKonzultace = NestWebApplication.konzultace.enmRezimKonzultace.dotaznikSDialogem
        Else
            konzultace.RezimKonzultace = NestWebApplication.konzultace.enmRezimKonzultace.dialog
        End If

        If konzultace.RezimKonzultace = NestWebApplication.konzultace.enmRezimKonzultace.dotaznik Or konzultace.RezimKonzultace = NestWebApplication.konzultace.enmRezimKonzultace.dotaznikSDialogem Then
            konzultace.BZ = New NestBase.BazeZnalosti(Environment, Me.userSetting.Language)
            If konzultace.NahrajBZ Then
                If konzultace.odpovediXML > "" Then
                    Dim str As String
                    Dim str2 As String
                    Dim str3 As String
                    If Not konzultace.Konzultuj(str, str2, str3, True) Then
                        'If Not konzultace.BZ.LoadAnswersFromXML(konzultace.odpovediXML) Then
                        CType(Me.Master, BasePage).ErrorMessage = konzultace.LastError.UserMessage
                    End If
                End If

                'Session("konzultace") = konzultace
                Response.Redirect("dotaznik.aspx")
            Else
                CType(Me.Master, BasePage).ErrorMessage = konzultace.BZ.LastError.UserMessage
            End If

        End If

        Dim redirectString As String
        Dim id_dotazu As String
        Dim result As String

        If konzultace.Konzultuj(redirectString, result, id_dotazu, True) Then
            Session("konzultace") = konzultace
            Session("id_dotazu") = id_dotazu
            Session("result") = result
            Response.Redirect(redirectString)
        Else
            CType(Me.Master, BasePage).ErrorMessage = konzultace.LastError.UserMessage
        End If
    End Sub

    Private Sub NahrajBZ()
        konzultace = New konzultace(Environment, Me.userSetting.Language)
        If NahratBZ <> "" Then
            Dim adresarBZ As String
            Select Case Me.userSetting.Language
                Case NestBase.Enums.enmLanguage.anglictina
                    adresarBZ = "BZ_en"
                Case Else
                    adresarBZ = "BZ"
            End Select

            Dim fr As New IO.StreamReader(Server.MapPath("~/" & adresarBZ & "/" & NahratBZ))
            Dim str As String = fr.ReadToEnd
            fr.Close()

            konzultace.BazeZnalostiXML = str

            If konzultace.NahrajBZ() Then

                Session("konzultace") = konzultace
                PripravaTlacitekOdpovedi(konzultace.BazeZnalostiXML)
            Else
                CType(Me.Master, BasePage).ErrorMessage = konzultace.BZ.LastError.UserMessage & " : " & konzultace.BZ.LastError.Exception.Message
            End If
        End If

    End Sub

    Public Function GetBZOnServer() As String
        Dim result As String = ""
        Dim adresarBZ As String
        Select Case Me.userSetting.Language
            Case NestBase.Enums.enmLanguage.anglictina
                adresarBZ = "BZ_en"
            Case Else
                adresarBZ = "BZ"
        End Select
        Dim di As New IO.DirectoryInfo(Server.MapPath("~\" & adresarBZ))
        For Each fi As IO.FileInfo In di.GetFiles
            result += "<li><a href=""default.aspx?bz=" & fi.Name & """>" & fi.Name & "</a></li>"
        Next
        Return result
    End Function

   
End Class


