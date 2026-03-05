Public Class Konjunkce
    Inherits Vyraz

    Private mLiteraly As Collection

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iLanguage)
        mLiteraly = New Collection
    End Sub

#Region "Xml"
    Public Overridable Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval, ByVal Atributy As Collection) As Boolean
        Try
            Dim literalElement As Xml.XmlElement
            For Each literalElement In xmlElement.SelectNodes("literal")
                Dim literal As New Literal(Environment, defaultVaha, Language)
                If Not literal.LoadFromXML(literalElement, Atributy) Then
                    Return Me.SetError(literal.LastError)
                End If
                mLiteraly.Add(literal)
            Next
            If mLiteraly.Count = 0 Then
                Return Me.SetError(GetText("Konjunkce_Prazdna_konjunkce"))
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Konjunkce_Chyba_pri_nacitani_konjunkce_z_XML"))
        End Try
    End Function

    Public Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try
            
            For Each vyraz As Vyraz In Me.mLiteraly
                If Not vyraz.SaveToXML(writer) Then
                    Return Me.SetError(vyraz.LastError)
                End If
            Next


            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Konjunkce_Chyba_pri_SaveToXML_v_konjunkci"))
        End Try
    End Function
#End Region

    Public Overrides Function PropojeniPredpokladu(ByVal pravidlo As Pravidlo) As Boolean
        Try
            Dim literal As Literal
            For Each literal In mLiteraly
                If Not literal.PropojeniPredpokladu(pravidlo) Then
                    Return Me.SetError(literal.LastError)
                End If
            Next
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Konjunkce_Chyba_pri_propojovani_predpokladu_u_pravidla") & pravidlo.Id)
        End Try
    End Function

    Public Overrides Function PropojeniKontextu(ByVal kontext As Kontext) As Boolean
        Try
            Dim literal As Literal
            For Each literal In mLiteraly
                If Not literal.PropojeniKontextu(kontext) Then
                    Return Me.SetError(literal.LastError)
                End If
            Next
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Konjunkce_Chyba_pri_propojovani_kontextu") & kontext.Id)
        End Try
    End Function

    Public Overrides Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Try
    

            'Helper.VypisAddLine(text, "     Konjunkce", typVypisu)
            If mLiteraly.Count = 1 Then
                Dim literal As Literal = mLiteraly(1)
                If Not literal.GetVypis(text, typVypisu, rozsahVah) Then
                    Return Me.SetError(literal.LastError)
                End If
            Else
                For i As Integer = 1 To mLiteraly.Count
                    Dim literal As Literal = Me.mLiteraly(i)
                    If i > 1 Then text += " AND "
                    text += "("
                    If Not literal.GetVypis(text, typVypisu, rozsahVah) Then
                        Return Me.SetError(literal.LastError)
                    End If
                    text += ")"

                Next
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Konjunkce_Chyba_pri_tvorbe_vypisu_konjunkce"))
        End Try
    End Function

    Public Overrides Function VyhodnotBackward(ByRef uspesneVyhodnoceno As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
        Try
            If mStatus = Enums.enmStav.enmFinal Or mStatus = Enums.enmStav.enmProvisional Then
                uspesneVyhodnoceno = True
                Return True
            End If

            Dim pocetLiteraluVeStavuFinal As Long = 0
            Dim pocetLiteraluVeStavuProvisional As Long = 0

            mVaha.SetValue(Environment.vahaTrue)
            Dim literal As Literal

            'zkouska, jestli nejaky literal neni vyhodnoceny a zaporny
            Dim pomocnyInterval As New Interval(Environment.vahaTrue)
            Dim jeStavFinal As Boolean = False
            For Each literal In Me.mLiteraly
                Dim vyhodnoceniLiteralu As Boolean
                If Not literal.VyhodnotBackward(vyhodnoceniLiteralu, neurcitost, aktSeznamDotazu, atributy, True) Then
                    Return Me.SetError(literal.LastError)
                End If

                If literal.Status = Enums.enmStav.enmFinal Then
                    jeStavFinal = True
                    pomocnyInterval.SetValue(neurcitost.CONJ(pomocnyInterval, literal.Vaha))
                End If
            Next
            If jeStavFinal Then
                If pomocnyInterval.MaxHodnota <= 0 Then
                    Me.Vaha.SetValue(pomocnyInterval)
                    Me.mStatus = Enums.enmStav.enmFinal
                    uspesneVyhodnoceno = True
                    Return True
                End If
            End If




            For Each literal In Me.mLiteraly 'pres vsechny literaly
                Dim vyhodnoceniLiteralu As Boolean
                If Not literal.VyhodnotBackward(vyhodnoceniLiteralu, neurcitost, aktSeznamDotazu, atributy, bezdotazu) Then
                    Return Me.SetError(literal.LastError)
                End If

                'pokud literal neni vyhodnocen
                If Not vyhodnoceniLiteralu Then
                    uspesneVyhodnoceno = False
                    Return True
                End If

                'pokud by literal vyhodnocen

                If literal.Status = Enums.enmStav.enmUntouched Or literal.Status = Enums.enmStav.enmPartial Then
                    mStatus = Enums.enmStav.enmPartial
                    uspesneVyhodnoceno = True 'todle tady nebylo
                    Return True
                End If

                If literal.Vaha.MaxHodnota <= 0 Then
                    mStatus = Enums.enmStav.enmFinal
                    mVaha.SetValue(Environment.vahaIrrelevant)
                    uspesneVyhodnoceno = True
                    Return True
                End If

                mVaha.SetValue(neurcitost.CONJ(mVaha, literal.Vaha))
                Select Case literal.Status
                    Case Enums.enmStav.enmFinal
                        pocetLiteraluVeStavuFinal += 1
                    Case Enums.enmStav.enmProvisional
                        pocetLiteraluVeStavuProvisional += 1
                End Select
            Next

            If mLiteraly.Count = pocetLiteraluVeStavuFinal Then
                mStatus = Enums.enmStav.enmFinal
            Else
                mStatus = Enums.enmStav.enmProvisional
            End If

            uspesneVyhodnoceno = True
            Return True

        Catch ex As Exception
            Return Me.SetError(ex, GetText("Konjunkce_Chyba_pri_vyhodnocovani_konjunkce_backward"))
        End Try
    End Function

    Public Function PridejKonjunkciZeStringu(ByVal jedenClenDisjunkce As String, ByVal defaultvaha As Interval, ByVal atributy As Collection) As Boolean
        Dim konjunkceStr() As String
        konjunkceStr = Split(jedenClenDisjunkce, ") and (")
        
        For Each jedenClenKonjunkce As String In konjunkceStr
            'odstraneni zavorky na zacatku
            Dim r As New System.Text.RegularExpressions.Regex("^(\s*)(\(?)(\s*)(.*)$*")
            jedenClenKonjunkce = r.Replace(jedenClenKonjunkce, "$4")

            r = New System.Text.RegularExpressions.Regex("^(\s*)(not)?(\s*)(\(?)(\s*)([^\(\)\s]*)(\s*)(\(?)(\s*)([^\(\)\s]*)(\s*)(\)?)(\s*)(\)?)(\s*)$")
            Dim m As System.Text.RegularExpressions.Match
            m = r.Match(jedenClenKonjunkce)
            If Not m.Success Then
                Return Me.SetError("clen - " & jedenClenKonjunkce & " - ma spatnou syntax")
            End If

            Dim clenProZpracovani As String
            clenProZpracovani = r.Replace(jedenClenKonjunkce, "$2$3$4$5$6$8$10$12$14")

            Dim literal As New Literal(Environment, defaultvaha, Language)
            If Not literal.PridejLiteralZeStringu(clenProZpracovani, defaultvaha, atributy) Then
                Return Me.SetError(literal.LastError)
            End If
            Me.mLiteraly.Add(literal)
        Next
        Return True
    End Function

End Class
