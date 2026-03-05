Imports System.Xml
Partial Class dotaznik
    Inherits cBasePage

    Private javascript As String = ""
    Private javascript2 As String = ""

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        cmdOdeslat.Attributes.Add("onclick", "JavaScript:Create_hodnota_text();")
        CType(Master, BasePage).BodyScripts = "onLoad='Napln()'"
        If Not Page.IsPostBack Then
            RenderDotazy()
            GenerujOdpovedi()

        Else
            javascript2 = ViewState("javascript2")
            javascript = ViewState("javascript")
        End If
    End Sub

    Private Sub RenderDotazy()
        Dim result As String = ""

        Dim odpovedi As String
        odpovedi = konzultace.GetOdpovedi
        Dim xmlDoc As New XmlDocument
        Try
            xmlDoc.LoadXml(odpovedi)
        Catch ex As Exception
            '++++
        End Try

        Dim pocetVyroku As Long = 0
        For Each xmlElement As XmlElement In xmlDoc.SelectNodes("//answers/attribute")
            pocetVyroku += 1
            Dim id_element As String
            Helper.NactiXMLString(xmlElement, "id", id_element)

            Dim atribut As NestBase.Atribut
            atribut = konzultace.BZ.Atributy(id_element)

            Select Case atribut.Typ
                Case NestBase.Enums.enmTypAtributu.enmBinary
                    result += "<div class=""panel"">"
                    result += "<div class=""panelNadpis"">" & atribut.Jmeno & "</div>"
                    result += "<div class=""panelObsah"">"
                    result += "<div class=""komentar"">" & atribut.Komentar & "</div>"
                    result += "<div><input id=""eHodnota" & atribut.Id & """></input></div>"

                    result += "<div class=""rozsah"">" & GetText("RozsahVah") & ": -" & konzultace.BZ.RozsahVah.ToString & " : " & konzultace.BZ.RozsahVah.ToString & "</div>"

                    result += "</div>"
                    result += "</div>"

                    javascript += "result = result + '" & atribut.Id & "°' + document.getElementById('eHodnota" & atribut.Id & "').value + ""~"";" & vbCrLf

                    javascript2 += "var hod = hodnoty[" & pocetVyroku - 1 & "].split(""°"");"
                    javascript2 += "document.getElementById('eHodnota" & atribut.Id & "').value = hod[1];"

                Case NestBase.Enums.enmTypAtributu.enmSingle
                    result += "<div class=""panel"">"
                    result += "<div class=""panelNadpis"">" & atribut.Jmeno & "</div>"
                    result += "<div class=""panelObsah"">"
                    result += "<div class=""komentar"">" & atribut.Komentar & "</div>"
                    result += "<div><select id=""eHodnotaVyber" & atribut.Id & """>"
                    'For Each str2 As String In CType(atribut, NestBase.AtributSingle).SeznamHodnot
                    For Each vyrok As NestBase.Vyrok In CType(atribut, NestBase.AtributSingle).Vyroky
                        result += "<option value=""" & vyrok.Id & """>" & vyrok.Jmeno & "</option>"
                    Next
                    result += "</select>"
                    result += "<input id=""eHodnota" & atribut.Id & """></input></div>"

                    result += "<div class=""rozsah"">" & GetText("RozsahVah") & ": -" & konzultace.BZ.RozsahVah.ToString & " : " & konzultace.BZ.RozsahVah.ToString & "</div>"

                    result += "</div>"
                    result += "</div>"

                    javascript += "result = result + '" & atribut.Id & "°' + document.getElementById('eHodnotaVyber" & atribut.Id & "').options[document.getElementById('eHodnotaVyber" & atribut.Id & "').selectedIndex].value + ""|"" + document.getElementById('eHodnota" & atribut.Id & "').value + ""~"";" & vbCrLf

                    javascript2 += "var hod = hodnoty[" & pocetVyroku - 1 & "].split(""°"");"

                    javascript2 += "var hod2 = hod[1].split(""|"");"
                    Dim pocet As Long = 0
                    'javascript2 += "var si;"

                    For Each str As String In CType(atribut, NestBase.AtributSingle).SeznamHodnot
                        pocet += 1
                        javascript2 += "if (hod2[0] == '" & str & "'){si = " & pocet - 1 & ";};"


                    Next

                    javascript2 += "document.getElementById('eHodnotaVyber" & atribut.Id & "').selectedIndex = si;"
                    javascript2 += "document.getElementById('eHodnota" & atribut.Id & "').value = hod2[1];"


                Case NestBase.Enums.enmTypAtributu.enmMultiple
                    result += "<div class=""panel"">"
                    result += "<div class=""panelNadpis"">" & atribut.Jmeno & "</div>"
                    result += "<div class=""panelObsah"">"
                    result += "<div class=""komentar"">" & atribut.Komentar & "</div>"
                    result += "<DIV>"
                    result += "<table cellpadding=""0"" cellspacing=""0"">"
                    Dim pocet As Long = 0
                    For Each vyrok As NestBase.Vyrok In CType(atribut, NestBase.AtributMultiple).Vyroky
                        'For Each str2 As String In CType(atribut, NestBase.AtributMultiple).SeznamHodnot
                        pocet += 1
                        result += "<tr><td>" & vyrok.Jmeno & "</td><td>&nbsp;<input type=""text"" id=""" & atribut.Id & "text" & CStr(pocet) & """></td></tr>"
                    Next
                    result += "</table>"
                    result += "</DIV>"
                    result += "<div class=""rozsah"">" & GetText("RozsahVah") & ": -" & konzultace.BZ.RozsahVah.ToString & " : " & konzultace.BZ.RozsahVah.ToString & "</div>"

                    result += "</div>"
                    result += "</div>"

                    javascript += "result = result + '" & atribut.Id & "°';" & vbCrLf
                    pocet = 0
                    For Each str2 As String In CType(atribut, NestBase.AtributMultiple).SeznamHodnot
                        pocet += 1
                        javascript += "result = result + document.getElementById('" & atribut.Id & "text" & CStr(pocet) & "').value + ""|"";" & vbCrLf

                    Next
                    javascript += "result = result + '~';"

                    javascript2 += "var hod = hodnoty[" & pocetVyroku - 1 & "].split(""°"");"

                    javascript2 += "var hod2 = hod[1].split(""|"");"
                    pocet = 0
                    For Each str As String In CType(atribut, NestBase.AtributMultiple).SeznamHodnot
                        pocet += 1
                        javascript2 += "document.getElementById('" & atribut.Id & "text" & CStr(pocet) & "').value = hod2[" & CStr(pocet - 1) & "];"

                    Next


                Case NestBase.Enums.enmTypAtributu.enmNumeric
                    result += "<div class=""panel"">"
                    result += "<div class=""panelNadpis"">" & atribut.Jmeno & "</div>"
                    result += "<div class=""panelObsah"">"
                    result += "<div class=""komentar"">" & atribut.Komentar & "</div>"
                    result += "<div><input id=""eHodnota" & atribut.Id & """></input></div>"

                    result += "<div class=""rozsah"">" & GetText("DolniMez") & ": " & CType(atribut, NestBase.AtributNumeric).DolniMez & " " & GetText("HorniMez") & ": " & CType(atribut, NestBase.AtributNumeric).HorniMez & "</div>"

                    result += "</div>"
                    result += "</div>"

                    javascript += "result = result + '" & atribut.Id & "°' + document.getElementById('eHodnota" & atribut.Id & "').value + ""~"";" & vbCrLf

                    javascript2 += "var hod = hodnoty[" & pocetVyroku - 1 & "].split(""°"");"
                    javascript2 += "document.getElementById('eHodnota" & atribut.Id & "').value = hod[1];"
            End Select
        Next


        dotazy_div.InnerHtml = result
        ViewState("javascript2") = javascript2
        ViewState("javascript") = javascript
    End Sub


    Public Function GetJavaScript() As String
        Return javascript
    End Function
    Public Function GetJavaScript2() As String
        Return javascript2
    End Function

    Protected Sub cmdOdeslat_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdOdeslat.Click
        Dim htext As String = hodnota_text.Value
        Dim pokracovat As Boolean = True
        Dim atribut_text() As String
        atribut_text = Split(htext, "~")
        For Each str As String In atribut_text
            If str <> "" Then


                Dim s() As String
                s = Split(str, "°")
                Dim atribut As NestBase.Atribut
                atribut = konzultace.BZ.Atributy(s(0))
                Select Case atribut.Typ
                    Case NestBase.Enums.enmTypAtributu.enmBinary
                        If s(1) = "" Then
                            s(1) = konzultace.BZ.DefaultVaha.ToStr(konzultace.BZ.RozsahVah, True)
                        End If
                        Select Case CType(atribut, NestBase.AtributBinary).VlozVahu(s(1), konzultace.BZ.DefaultVaha, konzultace.BZ.RozsahVah)
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmVahaMimoRozsah
                                CType(Me.Master, BasePage).ErrorMessage = GetText("VahaJeMimoRozsah")
                                pokracovat = False
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmChyba
                                CType(Me.Master, BasePage).ErrorMessage = GetText("VahuSeNepodariloNacist")
                                pokracovat = False
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmOK

                        End Select
                    Case NestBase.Enums.enmTypAtributu.enmNumeric
                        If s(1) = "" Then
                            s(1) = konzultace.BZ.DefaultVaha.ToStr(konzultace.BZ.RozsahVah, True)
                        End If
                        Select Case CType(atribut, NestBase.AtributNumeric).VlozHodnotu(s(1), konzultace.BZ.DefaultVaha)
                            Case NestBase.AtributNumeric.enmVlozeniHodnoty.enmHodnotaMimoRozsah
                                CType(Me.Master, BasePage).ErrorMessage = GetText("HodnotaJeMimoRozsah")
                                pokracovat = False
                            Case NestBase.AtributNumeric.enmVlozeniHodnoty.enmChyba
                                CType(Me.Master, BasePage).ErrorMessage = GetText("HodnotuSeNepodariloNacist")
                                pokracovat = False
                            Case NestBase.AtributNumeric.enmVlozeniHodnoty.enmOK

                        End Select
                    Case NestBase.Enums.enmTypAtributu.enmMultiple
                        If s(1).StartsWith("|") Then s(1) = "default" + s(1)
                        While s(1).IndexOf("||") > -1
                            s(1) = s(1).Replace("||", "|default|")
                        End While


                        Select Case CType(atribut, NestBase.AtributMultiple).VlozVahy(s(1), konzultace.BZ.RozsahVah, konzultace.BZ.DefaultVaha)
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmVahaMimoRozsah
                                CType(Me.Master, BasePage).ErrorMessage = GetText("VahaJeMimoRozsah")
                                pokracovat = False
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmChyba
                                CType(Me.Master, BasePage).ErrorMessage = GetText("VahuSeNepodariloNacist")
                                pokracovat = False
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmOK

                        End Select
                    Case NestBase.Enums.enmTypAtributu.enmSingle

                        Dim ss() As String
                        ss = Split(s(1), "|")
                        If ss(1) = "" Then
                            ss(1) = konzultace.BZ.DefaultVaha.ToStr(konzultace.BZ.RozsahVah, True)
                        End If
                        Select Case CType(atribut, NestBase.AtributSingle).VlozHodotu(ss(0), ss(1), konzultace.BZ.DefaultVaha, konzultace.BZ.RozsahVah)
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmHodnotaNeexistuj
                                CType(Me.Master, BasePage).ErrorMessage = GetText("HodnotaNeexistuje")
                                pokracovat = False
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmVahaMimoRozsah
                                CType(Me.Master, BasePage).ErrorMessage = GetText("VahaJeMimoRozsah")
                                pokracovat = False
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmChyba
                                CType(Me.Master, BasePage).ErrorMessage = GetText("VahuSeNepodariloNacist")
                                pokracovat = False
                            Case NestBase.Atribut.enmVlozeniHodnoty.enmOK

                        End Select
                End Select
            End If
        Next

        If pokracovat Then


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
        End If
    End Sub


    Private Sub GenerujOdpovedi()
        Dim result As String = ""

        For Each atribut As NestBase.Atribut In konzultace.BZ.Atributy
            If atribut.Pozice = NestBase.Enums.enmPozice.enmQuestion Then
                Select Case atribut.Typ
                    Case NestBase.Enums.enmTypAtributu.enmBinary
                        result += atribut.Id & "°" & atribut.Vaha.ToStr(konzultace.BZ.RozsahVah, True) & "~"
                    Case NestBase.Enums.enmTypAtributu.enmSingle
                        result += atribut.Id & "°" & CType(atribut, NestBase.AtributSingle).Hodnota & "|" & atribut.Vaha.ToStr(konzultace.BZ.RozsahVah, True) & "~"
                    Case NestBase.Enums.enmTypAtributu.enmMultiple
                        result += atribut.Id & "°"
                        For i As Integer = 1 To CType(atribut, NestBase.AtributMultiple).SeznamHodnot.Count
                            result += CType(CType(atribut, NestBase.AtributMultiple).Vyroky(i), NestBase.Vyrok).Vaha.ToStr(konzultace.BZ.RozsahVah, True) & "|"
                        Next
                        result += "~"
                    Case NestBase.Enums.enmTypAtributu.enmNumeric
                        result += atribut.Id & "°" & CType(atribut, NestBase.AtributNumeric).HodnotaStr & "~"
                End Select
            End If

        Next
        hodnota_text.Value = result
    End Sub



End Class



