Public Class ZdrojVypocet
    Inherits Zdroj
    Private mCleny As Collection
    Private mAtributy As Collection
    Private mDefaultVaha As Interval

    Public ReadOnly Property Cleny() As Collection
        Get
            Return mCleny
        End Get
    End Property

    Public Sub New(ByVal iEnvironment As Environment, ByVal atributy As Collection, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, Enums.enmTypZdroje.enmVypocet, atributy, defaultVaha, iLanguage)
        mCleny = New Collection
        mAtributy = atributy
        mDefaultVaha = defaultVaha
    End Sub

    Public Overrides Function VyhodnotZdroj(ByRef uspesneVyhodnocen As Boolean, ByVal vyrok As Vyrok, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Microsoft.VisualBasic.Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean, ByRef dalsiZdroj As Boolean, Optional ByVal Atribut As Atribut = Nothing) As Boolean
        Dim atributN As AtributNumeric
        If Not vyrok Is Nothing Then
            atributN = vyrok.RodicovskyAtribut
        Else
            atributN = Atribut
        End If


        Dim vysledkyClenu As New Collection
        For Each clen As Clen In mCleny
            Dim vys As New Interval
            Dim dotaz As Boolean
            If clen.Vypocti(vysledkyClenu, aktSeznamDotazu, vys, dotaz, bezdotazu) Then
                If dotaz Then Return True
                vysledkyClenu.Add(vys)
            Else
                'Return Me.SetError(clen.LastError)
                uspesneVyhodnocen = False
                Return True
            End If
        Next

        Dim vyslednyInterval As Interval = vysledkyClenu(vysledkyClenu.Count)

        atributN.VlozHodnotu(vyslednyInterval.ToStr, mDefaultVaha)
        uspesneVyhodnocen = True
        Return True
    End Function

#Region "Xml"
    Public Function LoadFromXML(ByVal xmlElement As Xml.XmlElement) As Boolean
        Try
            For Each clenXML As Xml.XmlElement In xmlElement.SelectNodes("calculation/expression")
                Dim v1 As String
                Dim v2 As String
                Dim op As Long
                If Not Helper.NactiXMLString(clenXML, "entry1", v1, "") Then
                    Return Me.SetError(GetText("ZdrojVypocet_Chyba_pri_nacitani_prvniho_vstupu_vypoctu"))
                End If
                If Not Helper.NactiXMLString(clenXML, "entry2", v2, "") Then
                    Return Me.SetError(GetText("ZdrojVypocet_Chyba_pri_nacitani_druheho_vstupu_vypoctu"))
                End If
                If Not Helper.NactiXMLLong(clenXML, "operand", op, ) Then
                    Return Me.SetError(GetText("ZdrojVypocet_Chyba_pri_nacitani_operandu"))
                End If
                Dim clen As New Clen(Environment, v1, v2, op, mAtributy, Language)
                mCleny.Add(clen)
            Next


            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojVypocet_Chyba_pri_nacitani_vypoctu_z_XML"))
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try
            writer.WriteStartElement("source")
            writer.WriteRaw(vbCrLf)
            writer.WriteElementString("source_type", "calculation")
            writer.WriteRaw(vbCrLf)

            writer.WriteStartElement("calculation")
            writer.WriteRaw(vbCrLf)
            For Each clen As Clen In Me.mCleny
                If Not clen.SaveToXML(writer) Then
                    Return Me.SetError(clen.LastError)
                End If
            Next

            writer.WriteEndElement()
            writer.WriteRaw(vbCrLf)
            writer.WriteEndElement()
            writer.WriteRaw(vbCrLf)
            Return True

        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojVypocet_Chyba_pri_SaveToXML_ve_zdroji_vypocet"))
        End Try
    End Function
#End Region


End Class

Public Class Clen
    Inherits GeneralObject
    Private mVstup1 As String
    Private mVstup2 As String
    Private mOperand As Long
    Private mAtributy As Collection

    Public ReadOnly Property Vstup1() As String
        Get
            Return mVstup1
        End Get
    End Property

    Public ReadOnly Property Vstup2() As String
        Get
            Return mVstup2
        End Get
    End Property

    Public Sub New(ByVal iEnvironment As Environment, ByVal vstup1 As String, ByVal vstup2 As String, ByVal operand As Long, ByVal atributy As Collection, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mVstup1 = vstup1
        mVstup2 = vstup2
        mOperand = operand
        mAtributy = atributy
    End Sub

    Public Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try
            writer.WriteStartElement("expression")
            writer.WriteRaw(vbCrLf)

            writer.WriteElementString("entry1", Me.mVstup1)
            writer.WriteRaw(vbCrLf)
            writer.WriteElementString("entry2", Me.mVstup2)
            writer.WriteRaw(vbCrLf)

            writer.WriteElementString("operand", CStr(Me.mOperand))
            writer.WriteRaw(vbCrLf)


            writer.WriteEndElement()
            writer.WriteRaw(vbCrLf)
            Return True

        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojVypocet_Chyba_pri_SaveToXML_v_clenu_vypoctu"))
        End Try
    End Function

    Public Function Vypocti(ByVal vysledkyClenu As Collection, ByVal aktSeznamDotazu As Microsoft.VisualBasic.Collection, ByRef result As Interval, ByRef vracenDotaz As Boolean, ByVal bezDotazu As Boolean) As Boolean
        Try
            vracenDotaz = False
            Dim hodnota1 As Interval
            'Dim hodnota1_vstup1 As Double
            'Dim hodnota2_vstup1 As Double
            If mVstup1.StartsWith("a") Then 'na vstupu je jiný atribut
                Dim atribut1 As Atribut
                atribut1 = mAtributy(mVstup1.Substring(1))
                If atribut1.Typ <> Enums.enmTypAtributu.enmNumeric Then
                    Return Me.SetError(GetText("ZdrojVypocet_Atribut_na_vstupu_clenu_vypoctu_neni_numericky"))
                End If
                Dim atribut1num As AtributNumeric
                atribut1num = atribut1
                If atribut1.Status = Enums.enmStav.enmFinal Or atribut1.Status = Enums.enmStav.enmPartial Then
                    If Not atribut1num.ZadanaHodnota Then
                        Return Me.SetError(GetText("ZdrojVypocet_zadana_vaha"))
                    End If
                    hodnota1 = New Interval(atribut1num.Hodnota1, atribut1num.Hodnota2)
                    'hodnota1_vstup1 = atribut1num.Hodnota1
                    'hodnota2_vstup1 = atribut1num.Hodnota2
                    '++++ rozliseni mezi final a partial
                Else

                    If atribut1num.CalculationSources.Count > 0 Then
                        Dim vyhodnoceniZdroje As Boolean
                        Dim dalsiZdroj As Boolean
                        If Not CType(atribut1num.CalculationSources(1), ZdrojVypocet).VyhodnotZdroj(vyhodnoceniZdroje, Nothing, Nothing, aktSeznamDotazu, Nothing, bezDotazu, dalsiZdroj, atribut1num) Then
                            Return Me.SetError(GetText("Chyba vyhodnot Zdroj výpoèet call 2 atribut 1"))
                        End If
                        If atribut1.Status = Enums.enmStav.enmFinal Or atribut1.Status = Enums.enmStav.enmPartial Then
                            If Not atribut1num.ZadanaHodnota Then
                                Return Me.SetError(GetText("ZdrojVypocet_zadana_vaha"))
                            End If
                            hodnota1 = New Interval(atribut1num.Hodnota1, atribut1num.Hodnota2)


                        End If
                    Else
                        'dotaz na atribut

                        If Not bezDotazu Then
                            If Not Helper.ValueIsInCollection(aktSeznamDotazu, atribut1num.Id) Then
                                aktSeznamDotazu.Add(atribut1num, atribut1num.Id)
                            End If
                        End If
                        vracenDotaz = True
                        Return True
                    End If
                End If
            ElseIf mVstup1.StartsWith("f") Then
                    Dim int As Interval
                    int = vysledkyClenu(CLng(mVstup1.Substring(1)))
                    hodnota1 = New Interval(int)
            Else
                    Dim dbl1 As Double
                    If Not Helper.StrToDbl(mVstup1, dbl1) Then
                        Return Me.SetError(GetText("ZdrojVypocet_vstup_1_neni_cislo"))
                    End If
                    hodnota1 = New Interval(dbl1, dbl1)
            End If


            Dim hodnota2 As Interval
            'Dim hodnota1_vstup1 As Double
            'Dim hodnota2_vstup1 As Double
            If mVstup2.StartsWith("a") Then 'na vstupu je jiný atribut
                Dim atribut2 As Atribut
                atribut2 = mAtributy(mVstup2.Substring(1))
                If atribut2.Typ <> Enums.enmTypAtributu.enmNumeric Then
                    Return Me.SetError(GetText("ZdrojVypocet_Atribut_na_vstupu_clenu_vypoctu_neni_numericky"))
                End If
                Dim atribut2num As AtributNumeric
                atribut2num = atribut2
                If atribut2.Status = Enums.enmStav.enmFinal Or atribut2.Status = Enums.enmStav.enmPartial Then
                    If Not atribut2num.ZadanaHodnota Then
                        Return Me.SetError(GetText("ZdrojVypocet_zadana_vaha"))
                    End If
                    hodnota2 = New Interval(atribut2num.Hodnota1, atribut2num.Hodnota2)
                    'hodnota1_vstup1 = atribut1num.Hodnota1
                    'hodnota2_vstup1 = atribut1num.Hodnota2
                    '++++ rozliseni mezi final a partial
                Else

                    If atribut2num.CalculationSources.Count > 0 Then
                        Dim vyhodnoceniZdroje As Boolean
                        Dim dalsiZdroj As Boolean
                        If Not CType(atribut2num.CalculationSources(1), ZdrojVypocet).VyhodnotZdroj(vyhodnoceniZdroje, Nothing, Nothing, aktSeznamDotazu, Nothing, bezDotazu, dalsiZdroj, atribut2num) Then
                            Return Me.SetError(GetText("Chyba vyhodnot Zdroj výpoèet call 2 atribut 1"))
                        End If
                        If atribut2.Status = Enums.enmStav.enmFinal Or atribut2.Status = Enums.enmStav.enmPartial Then
                            If Not atribut2num.ZadanaHodnota Then
                                Return Me.SetError(GetText("ZdrojVypocet_zadana_vaha"))
                            End If
                            hodnota1 = New Interval(atribut2num.Hodnota1, atribut2num.Hodnota2)


                        End If
                    Else
                        'dotaz na atribut

                        If Not bezDotazu Then
                            If Not Helper.ValueIsInCollection(aktSeznamDotazu, atribut2num.Id) Then
                                aktSeznamDotazu.Add(atribut2num, atribut2num.Id)
                            End If
                        End If
                        vracenDotaz = True
                        Return True
                    End If

                    
                End If
            ElseIf mVstup2.StartsWith("f") Then
                Dim int As Interval
                int = vysledkyClenu(CLng(mVstup2.Substring(1)))
                hodnota2 = New Interval(int)
            Else
                Dim dbl2 As Double
                If Not Helper.StrToDbl(mVstup2, dbl2) Then
                    Return Me.SetError(GetText("ZdrojVypocet_vstup_2_neni_cislo"))
                End If
                hodnota2 = New Interval(dbl2, dbl2)
            End If

            result = New Interval

            Select Case mOperand
                Case 1
                    result.MinHodnota = hodnota1.MinHodnota + hodnota2.MinHodnota
                    result.MaxHodnota = hodnota1.MaxHodnota + hodnota2.MaxHodnota
                Case 2
                    result.MinHodnota = hodnota1.MinHodnota - hodnota2.MinHodnota
                    result.MaxHodnota = hodnota1.MaxHodnota - hodnota2.MaxHodnota
                Case 3
                    result.MinHodnota = hodnota1.MinHodnota * hodnota2.MinHodnota
                    result.MaxHodnota = hodnota1.MaxHodnota * hodnota2.MaxHodnota
                Case 4
                    result.MinHodnota = hodnota1.MinHodnota / hodnota2.MinHodnota
                    result.MaxHodnota = hodnota1.MaxHodnota / hodnota2.MaxHodnota
            End Select

            Return True
        Catch ex As Exception
            Me.SetError(ex, GetText("ZdrojVypocet_Chyba_pri_vypoctu_clenu"))
        End Try

    End Function
End Class


