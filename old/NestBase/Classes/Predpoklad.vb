Public Class Predpoklad
    Inherits Vyraz

    Private mVyrazy As Collection 'seznam konjunkci a literalu v disjunkci

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iLanguage)
        mVyrazy = New Collection
    End Sub

#Region "Xml"
    Public Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval, ByVal Atributy As Collection, ByVal rozsahVah As Long) As Boolean
        Try
            Dim konjunkceElement As Xml.XmlElement
            For Each konjunkceElement In xmlElement.SelectNodes("conjunction")
                If konjunkceElement.SelectNodes("literal").Count = 1 Then 'pouze jeden literal v konjunkci
                    Dim literalElement As Xml.XmlElement
                    literalElement = konjunkceElement.SelectSingleNode("literal")
                    Dim literal As New Literal(Environment, defaultVaha, Language)
                    If Not literal.LoadFromXML(literalElement, Atributy) Then
                        Return Me.SetError(literal.LastError)
                    End If
                    mVyrazy.Add(literal)
                Else 'vice literalu v predpokladu
                    Dim konjunkce As New Konjunkce(Environment, defaultVaha, Language)
                    If Not konjunkce.LoadFromXML(konjunkceElement, defaultVaha, Atributy) Then
                        Return Me.SetError(konjunkce.LastError)
                    End If
                    mVyrazy.Add(konjunkce)
                End If
            Next
            If Me.mVyrazy.Count = 0 Then
                Return Me.SetError(GetText("Predpoklad_Prazdny_predpoklad_pravidla"))
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Predpoklad_Chyba_pri_nacitani_predpokladu_pravidla_z_XML"))
        End Try
    End Function

    Public Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try
            writer.WriteStartElement("condition")

            For Each vyraz As Vyraz In Me.mVyrazy
                writer.WriteStartElement("conjunction")
                If Not vyraz.SaveToXML(writer) Then
                    Return Me.SetError(vyraz.LastError)
                End If
                writer.WriteEndElement()
            Next

            writer.WriteEndElement()
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Predpoklad_Chyba_pri_SaveToXML_v_predpokladu"))
        End Try
    End Function

#End Region



    Public Overrides Function PropojeniPredpokladu(ByVal pravidlo As Pravidlo) As Boolean
        Try
            Dim vyraz As Vyraz
            For Each vyraz In mVyrazy
                If Not vyraz.PropojeniPredpokladu(pravidlo) Then
                    Return Me.SetError(vyraz.LastError)
                End If
            Next
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Predpoklad_Chyba_pri_propojovani_predpokladu_u_pravidla") & pravidlo.Id)
        End Try
    End Function

    Public Overrides Function PropojeniKontextu(ByVal kontext As Kontext) As Boolean
        Try
            Dim vyraz As Vyraz
            For Each vyraz In mVyrazy
                If Not vyraz.PropojeniKontextu(kontext) Then
                    Return Me.SetError(vyraz.LastError)
                End If
            Next
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Predpoklad_Chyba_pri_propojovani_kontextu") & kontext.Id)
        End Try
    End Function

    Public Overrides Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Try
            'Helper.VypisAddLine(text, "     Pøedpoklad", typVypisu)

            If mVyrazy.Count = 1 Then
                Dim vyraz As Vyraz = mVyrazy(1)
                If Not Vyraz.GetVypis(text, typVypisu, rozsahVah) Then
                    Return Me.SetError(Vyraz.LastError)
                End If
            Else
                For i As Integer = 1 To mVyrazy.Count
                    Dim vyraz As Vyraz = Me.mVyrazy(i)
                    If i > 1 Then text += " OR "
                    text += "("
                    If Not vyraz.GetVypis(text, typVypisu, rozsahVah) Then
                        Return Me.SetError(vyraz.LastError)
                    End If
                    text += ")"

                Next
            End If


            

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Predpoklad_Chyba_pri_tvorbe_vypisu_predpokladu"))
        End Try
    End Function



    Public Overrides Function VyhodnotBackward(ByRef uspesneVyhodnoceno As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
        Try
            If mStatus = Enums.enmStav.enmFinal Or mStatus = Enums.enmStav.enmProvisional Then
                uspesneVyhodnoceno = True
                Return True
            End If

            Dim pocetVyrazuVeStavuFinal As Long = 0
            Dim pocetVyrazuVeStavuProvisional As Long = 0

            mVaha.SetValue(Environment.vahaFalse)

            'zkouska, jestli nejaky vyraz neni vyhodnoceny a kladny
            Dim pomocnyInterval As New Interval(Environment.vahaFalse)
            Dim jeStavFinal As Boolean = False
            For Each vyraz As Vyraz In Me.mVyrazy
                Dim vyhodnoceniVyrazu As Boolean
                If Not vyraz.VyhodnotBackward(vyhodnoceniVyrazu, neurcitost, aktSeznamDotazu, atributy, True) Then
                    Return Me.SetError(vyraz.LastError)
                End If

                If vyraz.Status = Enums.enmStav.enmFinal Then
                    jeStavFinal = True
                    pomocnyInterval.SetValue(neurcitost.DISJ(pomocnyInterval, vyraz.Vaha))
                End If
            Next
            If jeStavFinal Then
                If pomocnyInterval.JeRovno(Environment.vahaTrue) Then
                    Me.Vaha.SetValue(pomocnyInterval)
                    Me.mStatus = Enums.enmStav.enmFinal
                    uspesneVyhodnoceno = True
                    Return True
                End If
            End If


            For Each vyraz As Vyraz In Me.mVyrazy
                Dim vyhodnoceniVyrazu As Boolean
                If Not vyraz.VyhodnotBackward(vyhodnoceniVyrazu, neurcitost, aktSeznamDotazu, atributy, bezdotazu) Then
                    Return Me.SetError(vyraz.LastError)
                End If

                'vyraz nebyl vyhodnoce
                If Not vyhodnoceniVyrazu Then
                    uspesneVyhodnoceno = False
                    Return True
                End If

                'vyraz byl vyhodnocen
                If Not (vyraz.Status = Enums.enmStav.enmUntouched Or vyraz.Status = Enums.enmStav.enmPartial) Then
                    If vyraz.Vaha.JeRovno(Environment.vahaTrue) Then
                        mStatus = Enums.enmStav.enmFinal
                        mVaha.SetValue(Environment.vahaTrue)
                        uspesneVyhodnoceno = True
                        Return True
                    End If

                    mVaha.SetValue(neurcitost.DISJ(mVaha, vyraz.Vaha))
                    Select Case vyraz.Status
                        Case Enums.enmStav.enmFinal
                            pocetVyrazuVeStavuFinal += 1
                        Case Enums.enmStav.enmProvisional
                            pocetVyrazuVeStavuProvisional += 1
                    End Select
                End If


            Next

            If mVyrazy.Count = pocetVyrazuVeStavuFinal Then
                mStatus = Enums.enmStav.enmFinal
                uspesneVyhodnoceno = True
                Return True
            ElseIf mVyrazy.Count = pocetVyrazuVeStavuFinal + pocetVyrazuVeStavuProvisional Then
                mStatus = Enums.enmStav.enmProvisional
                uspesneVyhodnoceno = True
                Return True
            End If

            mStatus = Enums.enmStav.enmPartial
            uspesneVyhodnoceno = True 'todle tady nebylo
            Return True

        Catch ex As Exception
            Return Me.SetError(ex, GetText("Predpoklad_Chyba_pri_vyhodnocovani_predpokladu_backward"))
        End Try
    End Function

    Public Function PridejPredpokladZeStringu(ByVal predpokladStr As String, ByVal defaultvaha As Interval, ByVal atributy As Collection) As Boolean
        Dim disjunkceStr() As String
        disjunkceStr = Split(predpokladStr, ") or (")
        Dim cisloDisjunkce As Integer = 0
        For Each jedenClenDisjunkce As String In disjunkceStr
            cisloDisjunkce += 1
            If disjunkceStr.Length > 1 Then


                If cisloDisjunkce = 1 Then
                    'odstraneni zavorky na zacatku
                    Dim r As New System.Text.RegularExpressions.Regex("^(\s*)(\()(\s*)(.*)$")
                    jedenClenDisjunkce = r.Replace(jedenClenDisjunkce, "$4")
                End If
                If cisloDisjunkce = disjunkceStr.Length Then
                    'odstraneni zavorky na konci
                    Dim r As New System.Text.RegularExpressions.Regex("^(.*)(\s*)(\))(\s*)$")
                    jedenClenDisjunkce = r.Replace(jedenClenDisjunkce, "$1")
                End If
            End If

            Dim konjunkce As New Konjunkce(Environment, defaultvaha, Language)
            If Not konjunkce.PridejKonjunkciZeStringu(jedenClenDisjunkce, defaultvaha, atributy) Then
                Return Me.SetError(konjunkce.LastError)
            End If
            Me.mVyrazy.Add(konjunkce)
        Next
        Return True
    End Function
  
End Class
