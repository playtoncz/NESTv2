Public Class Literal
    Inherits Vyraz

    Private mVyrok As Vyrok
    Private mNegace As Boolean

#Region "Property"
    Public ReadOnly Property Vyrok() As Vyrok
        Get
            Return mVyrok
        End Get
    End Property
    Public ReadOnly Property Negace() As Boolean
        Get
            Return mNegace
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iLanguage)
    End Sub

    Public Sub New(ByVal iEnvironment As Environment, ByVal Vaha As Interval, ByVal iVyrok As Vyrok, ByVal iNegace As Boolean, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, Vaha, iLanguage)
        mVyrok = iVyrok
        mNegace = iNegace
    End Sub

#Region "Xml"
    Public Overridable Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal Atributy As Collection) As Boolean
        Try
            Dim atributId As String
            Dim vyrokId As String

            'nacteni hodnot z xml
            If Not Helper.NactiXMLString(xmlElement, "id_attribute", atributId) Then
                Return Me.SetError("Chyba pøi naèítání literalu")
            End If
            If Not Helper.NactiXMLString(xmlElement, "id_proposition", vyrokId) Then
                vyrokId = atributId
            End If
            Helper.NactiXMLBoolean(xmlElement, "negation", mNegace, False)

            'nalezeni atributu a vyroku
            Dim atribut As Atribut
            atribut = Atributy(atributId)
            mVyrok = atribut.Vyroky(vyrokId)


            Return True
        Catch ex As Exception
            Return Me.SetError(ex, , "Chyba pøi naèítání literalu z XML")
        End Try


    End Function

    Public Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try
            writer.WriteStartElement("literal")
            writer.WriteElementString("id_attribute", Me.mVyrok.RodicovskyAtribut.Id)
            If Me.mVyrok.RodicovskyAtribut.Typ <> Enums.enmTypAtributu.enmBinary Then
                writer.WriteElementString("id_proposition", Me.mVyrok.Id)
            End If

            writer.WriteElementString("negation", IIf(mNegace, "1", "0"))



            writer.WriteEndElement()
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, , "Chyba pøi SaveToXML v literalu")
        End Try
    End Function
#End Region

    Public Overrides Function PropojeniPredpokladu(ByVal pravidlo As Pravidlo) As Boolean
        Try
            mVyrok.SeznamPredpokladu.Add(pravidlo)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Literal_Chyba_pri_propojovani_predpokladu_u_pravidla") & pravidlo.Id)
        End Try
    End Function
    Public Overrides Function PropojeniKontextu(ByVal kontext As Kontext) As Boolean
        Try
            mVyrok.SeznamKontextu.Add(kontext)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Literal_Chyba_pri_propojovani_kontextu") & kontext.Id)
        End Try
    End Function


    Public Overrides Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahvah As Long) As Boolean
        Try
            'Helper.VypisAddLine(text, "     Literál", typVypisu)
            If mNegace Then
                text += "NOT(<a href=""#" & mVyrok.Id & """>" & mVyrok.Id & "</a>)"
            Else
                text += "<a href=""#" & mVyrok.Id & """>" & mVyrok.Id & "</a>"
            End If



            'If Not Me.mVyrok.GetVypis(text, typVypisu, rozsahvah) Then
            '    Return Me.SetError(mVyrok.LastError)
            'End If
            'Helper.VypisAddLine(text, "     negace: " & CStr(mNegace), typVypisu)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Literal_Chyba_pri_tvorbe_vypisu_literalu"))
        End Try
    End Function

    Public Overrides Function VyhodnotBackward(ByRef uspesneVyhodnoceno As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
        Try
            If mStatus = Enums.enmStav.enmFinal Or mStatus = Enums.enmStav.enmProvisional Then
                uspesneVyhodnoceno = True
                Return True
            End If

            'vyhodnotim vyrok
            Dim vyhodnoceniVyroku As Boolean
            If Not Me.Vyrok.VyhodnotVyrokBackward(vyhodnoceniVyroku, neurcitost, aktSeznamDotazu, atributy, bezdotazu) Then
                Return Me.SetError(Vyrok.LastError)
            End If

            'jestlize vyrok nebyl vyhodnocen
            If Not vyhodnoceniVyroku Then
                uspesneVyhodnoceno = False
                Return True
            End If

            If Vyrok.Status = Enums.enmStav.enmUntouched Or Vyrok.Status = Enums.enmStav.enmPartial Then
                mStatus = Enums.enmStav.enmPartial
                uspesneVyhodnoceno = True 'tohle ve starem nebylo - tady je to kvuli dotazum
                Return True
            End If

            If Me.mNegace Then
                mVaha.SetValue(neurcitost.NEG(mVyrok.Vaha))
            Else
                mVaha.SetValue(mVyrok.Vaha)
            End If

            mStatus = Vyrok.Status

            uspesneVyhodnoceno = True
            Return True

        Catch ex As Exception
            Return Me.SetError(ex, GetText("Literal_Chyba_pri_vyhodnocovani_literalu_backward"))
        End Try
    End Function

    Public Function PridejLiteralZeStringu(ByVal jedenClenKonjunkce As String, ByVal defaultvaha As Interval, ByVal atributy As Collection) As Boolean
        Dim r As New System.Text.RegularExpressions.Regex("^(\s*)(not)?(\s*)(\(?)(\s*)([^\(\)\s]*)(\s*)(\(?)(\s*)([^\(\)\s]*)(\s*)(\)?)(\s*)(\)?)(\s*)$")
        Dim m As System.Text.RegularExpressions.Match
        m = r.Match(jedenClenKonjunkce)
        If Not m.Success Then
            Return Me.SetError("literal - " & jedenClenKonjunkce & " - ma spatnou syntax")
        End If

        Dim negaceStr As String
        negaceStr = r.Replace(jedenClenKonjunkce, "$2")
        If negaceStr = "not" Then Me.mNegace = True
        Dim AtributVyrokStr As String
        Dim atributStr As String
        Dim vyrokStr As String
        AtributVyrokStr = r.Replace(jedenClenKonjunkce, "$5$6$8$10$12")


        r = New System.Text.RegularExpressions.Regex("^([^\s\(\)]+)(\s*)(\()(\s*)([^\s\(\)]+)(\s*)(\))(\s*)$")
        m = r.Match(AtributVyrokStr)
        If m.Success Then
            atributStr = r.Replace(AtributVyrokStr, "$1")
            vyrokStr = r.Replace(AtributVyrokStr, "$5")
        Else
            atributStr = AtributVyrokStr
            vyrokStr = AtributVyrokStr
        End If

        'nalezeni prislusneho vyroku
        Dim vyrok As Vyrok = Nothing
        Dim atribut As Atribut
        atribut = atributy(atributStr)
        If atribut Is Nothing Then Return Me.SetError("Atribut s identifikatorem - " & atributStr & " - nebyl nalezen")
        vyrok = atribut.Vyroky(vyrokStr)
        'For Each atribut As Atribut In atributy
        '    For Each vyr As Vyrok In atribut.Vyroky
        '        If LCase(vyr.Id) = vyrokStr Then
        '            vyrok = vyr
        '            Exit For
        '        End If
        '    Next
        '    If Not vyrok Is Nothing Then Exit For
        'Next
        If vyrok Is Nothing Then Return Me.SetError("Vyrok s identifikatorem - " & vyrokStr & " - nebyl nalezen")

        Me.mVyrok = vyrok

        Return True
    End Function
 
End Class
