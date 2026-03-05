Public Class AtributNumeric
    Inherits Atribut

    Private mOmezenZdola As Boolean
    Private mOmezenShora As Boolean
    Private mDolniMez As Double
    Private mHorniMez As Double
    Private mHodnota1 As Double
    Private mHodnota2 As Double
    Private mZadanaHodnota As Boolean = False
    Private mCalculationSources As Collection


#Region "Property"
    Public ReadOnly Property DolniMez() As String
        Get
            If Not mOmezenZdola Then Return "-"
            Return CStr(mDolniMez)
        End Get
    End Property
    Public ReadOnly Property HorniMez() As String
        Get
            If Not mOmezenShora Then Return "-"
            Return CStr(mHorniMez)
        End Get
    End Property
    Public ReadOnly Property Hodnota1() As Double
        Get
            Return mHodnota1
        End Get        
    End Property
    Public ReadOnly Property Hodnota2() As Double
        Get
            Return mHodnota2
        End Get        
    End Property
    Public ReadOnly Property HodnotaStr() As String
        Get
            If mHodnota1 = mHodnota2 Then Return mHodnota1
            Return mHodnota1 & ";" & mHodnota2
        End Get
    End Property
    Public ReadOnly Property ZadanaHodnota() As Boolean
        Get
            Return mZadanaHodnota
        End Get
    End Property
    Public ReadOnly Property CalculationSources() As Collection
        Get
            Return mCalculationSources
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmNumeric
        mCalculationSources = New Collection
    End Sub

    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(id, iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmNumeric
      
    End Sub

    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage, ByVal iDolniMez As Double, ByVal iHorniMez As Double)
        MyBase.New(id, iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmNumeric
        Me.mDolniMez = iDolniMez
        Me.mOmezenZdola = True
        Me.mHorniMez = iHorniMez
        Me.mOmezenShora = True
    End Sub


    Public Function VlozHodnotu(ByVal Value As String, ByVal defaultVaha As Interval) As enmVlozeniHodnoty
        Dim vaha As New Interval
        If Not Helper.NactiHodnotuNumeric(Value, vaha, mZadanaHodnota, defaultVaha) Then
            mStatus = Enums.enmStav.enmUntouched
            Return enmVlozeniHodnoty.enmChyba
        Else
            If mZadanaHodnota Then
                mHodnota1 = vaha.MinHodnota
                mHodnota2 = vaha.MaxHodnota
                If (mOmezenZdola And mHodnota1 < mDolniMez) Or (mOmezenShora And mHodnota2 > mHorniMez) Then
                    mStatus = Enums.enmStav.enmUntouched
                    vaha.SetValue(defaultVaha)
                    Return enmVlozeniHodnoty.enmHodnotaMimoRozsah
                End If


                mvaha.SetValue(Environment.vahaTrue)
            Else
                mvaha.SetValue(vaha)
            End If
            mStatus = Enums.enmStav.enmFinal
        End If

        If Not Me.SpoctiVahu Then
            Return enmVlozeniHodnoty.enmChyba
        End If
        Return enmVlozeniHodnoty.enmOK
    End Function

    Public Overrides Function SpoctiVahu() As Boolean
        Try
            If Me.mStatus = Enums.enmStav.enmUntouched Then
                Return True
            End If

            Dim vyrok As Vyrok
            If mvaha.JeRovno(Environment.vahaUnknown) Or mvaha.JeRovno(Environment.vahaIrrelevant) Then
                For Each vyrok In Me.mVyroky
                    vyrok.Vaha.SetValue(mvaha)
                    vyrok.Status = Enums.enmStav.enmFinal
                Next
            Else
                For Each vyrok In Me.mVyroky
                    vyrok.Vaha.SetValue(CType(vyrok, VyrokNumeric).SpoctiVahu(Me.mHodnota1), CType(vyrok, VyrokNumeric).SpoctiVahu(Me.mHodnota2))
                    vyrok.Status = Enums.enmStav.enmFinal
                Next
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributNumeric_Chyba_pri_pocitani_vahy_u_atributu") & Me.mId)
        End Try
    End Function

#Region "Xml"
    Public Overrides Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal prahPlatnostiKontextu As Double, ByVal atributy As Collection, ByVal rozsahVah As Long, ByVal defaultVaha As Interval, ByVal zdrojeCBR As Collection, Optional ByVal NscXML As String = "") As Boolean
        If Not MyBase.LoadFromXML(xmlElement, prahPlatnostiKontextu, atributy, rozsahVah, defaultVaha, zdrojeCBR) Then
            Return False
        End If
        Try
            'nacteni omezeni
            Dim legalValuesElement As Xml.XmlElement
            legalValuesElement = xmlElement.SelectSingleNode("legal_values")
            If Helper.NactiXMLDouble(legalValuesElement, "lower_bound", mDolniMez, 0) Then
                mOmezenZdola = True
            Else
                mOmezenZdola = False
            End If
            If Helper.NactiXMLDouble(legalValuesElement, "upper_bound", mHorniMez, 0) Then
                mOmezenShora = True
            Else
                mOmezenShora = False
            End If

            'vytvoreni vyroku
            Dim vyrokElement As Xml.XmlElement
            For Each vyrokElement In xmlElement.SelectNodes("propositions/proposition")
                Dim vyrok As New VyrokNumeric(Environment, defaultVaha, Me, Language)
                If Not vyrok.LoadFromXML(vyrokElement) Then
                    Return Me.AppendLastError(vyrok.LastError, " : atribut " & Me.mId)
                End If
                Me.mVyroky.Add(vyrok, vyrok.Id)
            Next

            'nacteni zdroju (vypocet)
            If xmlElement.SelectNodes("sources").Count > 0 Then
                For Each sourceElement As Xml.XmlElement In xmlElement.SelectNodes("sources/source")
                    Dim typZdroje As String
                    If Not Helper.NactiXMLString(sourceElement, "source_type", typZdroje) Then
                        Return Me.SetError(GetText("AtributNumeric_Chyba_pri_nacitani_zdroje"))
                    End If
                    Select Case typZdroje
                        Case "calculation"
                            Dim vypocet As New ZdrojVypocet(Environment, atributy, defaultVaha, Language)
                            If Not vypocet.LoadFromXML(sourceElement) Then
                                Return Me.SetError(vypocet.LastError)
                            End If
                            Me.Sources.Add(vypocet)
                            Me.CalculationSources.Add(vypocet)
                            For Each vyrok As Vyrok In mVyroky
                                vyrok.Zdroje.Add(vypocet, , 1)
                            Next
                            '++++
                    End Select
                Next

            End If
            

        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributNumeric_Chyba_pri_nacitani_numeric_atributu_z_xml"))
        End Try
        If Not Me.LoadSourcesFromXML(xmlElement, atributy, rozsahVah, defaultVaha, zdrojeCBR, NscXML) Then
            Return False
        End If

        Return True
    End Function

    Public Overrides Function LoadAnswerFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval) As Boolean
        Try
            Dim vaha As New Interval
            If Not Helper.NactiXMLHodnotuNumeric(xmlElement.SelectSingleNode("answer"), "value", vaha, mZadanaHodnota, defaultVaha) Then
                mStatus = Enums.enmStav.enmUntouched
            Else
                If mZadanaHodnota Then
                    mHodnota1 = vaha.MinHodnota
                    mHodnota2 = vaha.MaxHodnota
                    mvaha.SetValue(Environment.vahaTrue)
                Else
                    mvaha.SetValue(vaha)
                End If
                mStatus = Enums.enmStav.enmFinal
            End If





            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributNumeric_Chyba_pri_nacitani_odpovedi_z_xml_u_atributu") & Me.mId)
        End Try

    End Function

    Public Overloads Overrides Function SaveToXML(ByVal xmlDoc As Xml.XmlDocument, ByVal parentNode As Xml.XmlElement, ByRef vytvorenyElement As Xml.XmlElement) As Boolean
        Try
            If Not MyBase.SaveToXML(xmlDoc, parentNode, vytvorenyElement) Then
                Return False
            End If

            'vytvorenyElement = parentNode.FirstChild
            Dim answer As Xml.XmlElement
            answer = xmlDoc.CreateElement("answer")
            Dim element As Xml.XmlElement
            element = xmlDoc.CreateElement("value")
            If Me.mStatus = Enums.enmStav.enmFinal Then
                If mZadanaHodnota Then
                    Dim pomInterval As New Interval(mHodnota1, mHodnota2)
                    element.InnerText = pomInterval.ToStr
                Else
                    element.InnerText = mVaha.ToStr(, True)
                End If
            End If


            answer.AppendChild(element)

            vytvorenyElement.AppendChild(answer)

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributNumeric_Chyba_pri_SaveToXML_v_atributu_binary") & Me.mId)
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try

            writer.WriteStartElement("attribute")
            If Not MyBase.SaveToXML(writer, rozsahVah) Then
                Return False
            End If

            writer.WriteStartElement("legal_values")
            If Me.mOmezenZdola Then
                writer.WriteElementString("lower_bound", CStr(Me.mDolniMez))
            End If
            If Me.mOmezenShora Then
                writer.WriteElementString("upper_bound", CStr(Me.mHorniMez))
            End If
            writer.WriteEndElement()

            writer.WriteStartElement("propositions")
            For Each vyrok As VyrokNumeric In mVyroky
                If Not vyrok.SaveToXML(writer) Then
                    Return Me.SetError(vyrok.LastError)
                End If
            Next
            writer.WriteEndElement()

            writer.WriteEndElement()

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributNumeric_Chyba_pri_SaveToXML_v_atributu") & Me.mId)
        End Try
    End Function
#End Region

End Class
