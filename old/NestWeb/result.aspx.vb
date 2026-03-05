'===========================================================================
' This file was modified as part of an ASP.NET 2.0 Web project conversion.
' The class name was changed and the class modified to inherit from the abstract base class 
' in file 'App_Code\Migrated\Stub_result_aspx_vb.vb'.
' During runtime, this allows other classes in your web application to bind and access 
' the code-behind page using the abstract base class.
' The associated content page 'result.aspx' was also modified to refer to the new class name.
' For more information on this code pattern, please refer to http://go.microsoft.com/fwlink/?LinkId=46995 
'===========================================================================

'Partial Class result
Partial Class Migrated_result

    Inherits result

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
        CType(Master, BasePage).strankaKonzultace = True
        If Request.QueryString("upravit") <> "" Then
            Dim atribut As NestBase.Atribut
            If konzultace.BZ.GetAtributForIdVyroku(Request.QueryString("upravit"), atribut) Then
                atribut.Clear(konzultace.BZ.DefaultVaha)
                Dim redirectString As String
                Dim id_dotazu As String
                Dim result As String

                If konzultace.Konzultuj(redirectString, result, id_dotazu, False) Then
                    Session("konzultace") = konzultace
                    Session("id_dotazu") = id_dotazu
                    Session("result") = result
                    Response.Redirect(redirectString)
                Else
                    CType(Me.Master, BasePage).ErrorMessage = konzultace.LastError.UserMessage
                End If
            End If

        End If


        Dim xmlDoc As New System.Xml.XmlDocument
        xmlDoc.LoadXml(Session("result"))
        For Each xmlElement As System.Xml.XmlElement In xmlDoc.SelectNodes("results/goals/attribute")
            Dim iden As String
            Helper.NactiXMLString(xmlElement, "id", iden)
            Dim atr As NestBase.Atribut
            atr = konzultace.BZ.Atributy(iden)
            atr.LoadAnswerFromXML(xmlElement, konzultace.BZ.DefaultVaha)
            atr.SpoctiVahu()
        Next

        'pomocne spusteni pro urceni mezilehlych vyroku
        Dim odpovedi As String
        Dim pom As String
        konzultace.BZ.GetQuestionFromBaze("", odpovedi)
        konzultace.BZ = New NestBase.BazeZnalosti(Environment, Me.userSetting.Language)
        konzultace.BZ.RunConsultation(konzultace.BazeZnalostiXML, odpovedi, pom)


        vysledky.InnerHtml = "<table>"

        Dim ciloveVyroky As New Collection
        For Each vyrok As NestBase.Vyrok In konzultace.BZ.Cile
            Dim pozice As Long = 1
            While pozice <= ciloveVyroky.Count
                If vyrok.Vaha.JeVetsiNez(CType(ciloveVyroky(pozice), NestBase.Vyrok).Vaha) Then
                    Exit While
                End If
                pozice += 1
            End While
            ciloveVyroky.Add(vyrok, , pozice)
        Next

        For Each vyrok As NestBase.Vyrok In ciloveVyroky
            vysledky.InnerHtml += "<tr><td valign=""top"" class=""result_jmeno"">" & vyrok.Jmeno & "</td><td valign=""top"" class=""result_vaha"">"
            If vyrok.Vaha.MinHodnota > 0 Then
                vysledky.InnerHtml += "<font color=""green"">"
            End If
            If vyrok.Vaha.MaxHodnota < 0 Then
                vysledky.InnerHtml += "<font color=""red"">"
            End If
            vysledky.InnerHtml += vyrok.Vaha.ToStr(konzultace.BZ.RozsahVah)

            If vyrok.Vaha.MinHodnota > 0 Or vyrok.Vaha.MaxHodnota < 0 Then
                vysledky.InnerHtml += "</font>"
            End If
            vysledky.InnerHtml += "</td><td class=""result_komentar"">" & vyrok.Komentar & "</td></tr>"
        Next
        vysledky.InnerHtml += "</table>"

        RenderVsechnyVyroky(1)
        

        panelIntegritniOmezeni.Visible = False
        IntegritniOmezeniObsah.InnerHtml = "<table>"
        For Each xmlElement As System.Xml.XmlElement In xmlDoc.SelectNodes("results/integrity_constraints/integrity_constraint")
            IntegritniOmezeniObsah.InnerHtml += "<tr>"
            panelIntegritniOmezeni.Visible = True
            Dim iden As String
            Helper.NactiXMLString(xmlElement, "name", iden)
            If iden = "" Then
                Helper.NactiXMLString(xmlElement, "id", iden)
            End If
            IntegritniOmezeniObsah.InnerHtml += "<td>" & iden & "</td>"
            Helper.NactiXMLString(xmlElement, "weight", iden)
            IntegritniOmezeniObsah.InnerHtml += "<td>" & iden & "</td>"
            Helper.NactiXMLString(xmlElement, "comment", iden)
            IntegritniOmezeniObsah.InnerHtml += "<td>" & iden & "</td>"
            IntegritniOmezeniObsah.InnerHtml += "</tr>"
        Next
        IntegritniOmezeniObsah.InnerHtml += "</table>"
        '++++
    End Sub

    Private Sub RenderVsechnyVyroky(ByVal tridit As Integer)
        Dim setrideneVyroky As New Collection
        For Each atribut As NestBase.Atribut In konzultace.BZ.Atributy
            For Each vyrok As NestBase.Vyrok In atribut.Vyroky
                If setrideneVyroky.Count = 0 Then
                    setrideneVyroky.Add(vyrok, vyrok.Id)
                Else

                    Dim i As Integer = 1
                    Dim pokracovat As Boolean = True
                    While pokracovat
                        If i <= setrideneVyroky.Count Then
                            Select Case tridit
                                Case 2
                                    If vyrok.Vaha.MinHodnota <= CType(setrideneVyroky(i), NestBase.Vyrok).Vaha.MinHodnota Then
                                        i = i + 1
                                    Else
                                        pokracovat = False
                                    End If
                                Case 3
                                    If vyrok.Vaha.MaxHodnota <= CType(setrideneVyroky(i), NestBase.Vyrok).Vaha.MaxHodnota Then
                                        i = i + 1
                                    Else
                                        pokracovat = False
                                    End If
                                Case Else
                                    If String.Compare(vyrok.Jmeno, CType(setrideneVyroky(i), NestBase.Vyrok).Jmeno) = 1 Then
                                        i = i + 1
                                    Else
                                        pokracovat = False
                                    End If
                            End Select
                            
                        Else
                            pokracovat = False
                        End If
                    End While
                    setrideneVyroky.Add(vyrok, , i)
                End If
            Next
        Next



        vsechnyVyroky.InnerHtml = "<table>"
        'For Each atribut As NestBase.Atribut In konzultace.BZ.Atributy
        For Each vyrok As NestBase.Vyrok In setrideneVyroky
            vsechnyVyroky.InnerHtml += "<tr><td>" & vyrok.Jmeno & "</td><td>"
            If vyrok.Vaha.MinHodnota > 0 Then
                vsechnyVyroky.InnerHtml += "<font color=""green"">"
            End If
            If vyrok.Vaha.MaxHodnota < 0 Then
                vsechnyVyroky.InnerHtml += "<font color=""red"">"
            End If
            vsechnyVyroky.InnerHtml += vyrok.Vaha.ToStr(konzultace.BZ.RozsahVah)
            If vyrok.Vaha.MinHodnota > 0 Or vyrok.Vaha.MaxHodnota < 0 Then
                vsechnyVyroky.InnerHtml += "</font>"
            End If
            vsechnyVyroky.InnerHtml += "</td><td><a href=""result.aspx?upravit=" & vyrok.Id & """>" & GetText("upravit") & "</td></tr>"
        Next
        ' Next

        vsechnyVyroky.InnerHtml += "</table>"
    End Sub


    Protected Sub cmdSortJmeno_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSortJmeno.Click
        RenderVsechnyVyroky(1)
        VolatSrcipty.InnerHtml += "<script style=""text/javascript"">Ukaz('vsechnyvyroky');</script>"
    End Sub

    Protected Sub cmdSortMinVaha_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSortMinVaha.Click
        RenderVsechnyVyroky(2)
        VolatSrcipty.InnerHtml += "<script style=""text/javascript"">Ukaz('vsechnyvyroky');</script>"
    End Sub

    Protected Sub cmdSortMaxVahy_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSortMaxVahy.Click
        RenderVsechnyVyroky(3)
        VolatSrcipty.InnerHtml += "<script style=""text/javascript"">Ukaz('vsechnyvyroky');</script>"
    End Sub
End Class

