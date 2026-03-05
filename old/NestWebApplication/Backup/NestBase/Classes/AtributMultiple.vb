Public Class AtributMultiple
    Inherits Atribut

    Private mHodnoty As Collection 'seznam hodnot a jejich vah (v collection vaha, v key hodnota)
    Private mSeznamHodnot As Collection

#Region "Property"
    Public ReadOnly Property SeznamHodnot() As Collection
        Get
            Return mSeznamHodnot
        End Get
    End Property
    Public ReadOnly Property Hodnoty() As Collection
        Get
            Return mHodnoty
        End Get
    End Property

#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmMultiple
        mHodnoty = New Collection
        mSeznamHodnot = New Collection
    End Sub

    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(id, iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmMultiple
        mHodnoty = New Collection
        mSeznamHodnot = New Collection
    End Sub

    Public Overrides Function Clear(ByVal defaultVaha As Interval) As Boolean
        If Not MyBase.Clear(defaultVaha) Then
            Return False
        End If
        mHodnoty = New Collection
        Return True
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
                    Dim weight As Interval
                    weight = Me.mHodnoty(vyrok.Id) '++++ tohle nebude fungovat pro neexistujici
                    If Not weight Is Nothing Then
                        vyrok.Vaha.SetValue(weight)
                    End If
                    vyrok.Status = Enums.enmStav.enmFinal
                Next
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributMultiple_Chyba_pri_pocitani_vahy_u_atributu") & Me.mId)
        End Try
    End Function

#Region "Xml"
    Public Overrides Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal prahPlatnostiKontextu As Double, ByVal atributy As Collection, ByVal rozsahvah As Long, ByVal defaultVaha As Interval, ByVal zdrojeCBR As Collection, Optional ByVal NscXML As String = "") As Boolean
        If Not MyBase.LoadFromXML(xmlElement, prahPlatnostiKontextu, atributy, rozsahvah, defaultVaha, zdrojeCBR) Then
            Return False
        End If
        Try
            'vytvoreni vyroku
            Dim vyrokElement As Xml.XmlElement
            For Each vyrokElement In xmlElement.SelectNodes("propositions/proposition")
                Dim vyrok As New Vyrok(Environment, defaultVaha, Me, Language)
                If Not vyrok.LoadFromXML(vyrokElement) Then
                    Return Me.AppendLastError(vyrok.LastError, " : atribut " & Me.mId)
                End If
                Me.mVyroky.Add(vyrok, vyrok.Id)
                mSeznamHodnot.Add(vyrok.Id)

            Next


        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributMultiple_Chyba_pri_nacitani_multiple_atributu_z_xml"))
        End Try
        If Not Me.LoadSourcesFromXML(xmlElement, atributy, rozsahvah, defaultVaha, zdrojeCBR, NscXML) Then
            Return False
        End If

        Return True
    End Function

    Public Overrides Function LoadAnswerFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval) As Boolean
        Try
            Dim odpovedElement As Xml.XmlElement
            If xmlElement.SelectNodes("answer").Count > 0 Then
                mStatus = Enums.enmStav.enmFinal
            End If
            For Each odpovedElement In xmlElement.SelectNodes("answer")
                Dim vaha As New Interval
                If Not Helper.NactiXMLVahu(odpovedElement, "weight", vaha, 1, defaultVaha) Then
                    mstatus = Enums.enmStav.enmUntouched
                End If
                Dim hodnota As String
                If Not Helper.NactiXMLString(odpovedElement, "value", hodnota, "") Then
                    Return Me.SetError(GetText("AtributMultiple_Nepodarilo_se_nacist_hodnotu_odpovedi_pro_atribut") & Me.mId)
                End If
                If Not SeznamHodnotObsahuje(hodnota) Then
                    Return Me.SetError(GetText("AtributMultiple_Hodnota_odpovedi_neexistuje_pro_atribut") & Me.mId)
                End If
                mHodnoty.Add(vaha, hodnota)
            Next
            Me.mVaha.SetValue(Environment.vahaTrue)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributMultiple_Chyba_pri_nacitani_odpovedi_z_xml_u_atributu") & Me.mId)
        End Try

    End Function

    Public Overloads Overrides Function SaveToXML(ByVal xmlDoc As Xml.XmlDocument, ByVal parentNode As Xml.XmlElement, ByRef vytvorenyElement As Xml.XmlElement) As Boolean
        Try
            If Not MyBase.SaveToXML(xmlDoc, parentNode, vytvorenyElement) Then
                Return False
            End If

            If Me.mStatus = Enums.enmStav.enmFinal Then
                Dim i As Integer
                For i = 1 To Me.mHodnoty.Count
                    Dim answer As Xml.XmlElement
                    answer = xmlDoc.CreateElement("answer")
                    Dim value As Xml.XmlElement
                    value = xmlDoc.CreateElement("value")
                    value.InnerText = mSeznamHodnot(i)
                    answer.AppendChild(value)
                    Dim element As Xml.XmlElement
                    element = xmlDoc.CreateElement("weight")
                    element.InnerText = CType(mHodnoty(i), Interval).ToStr
                    answer.AppendChild(element)

                    vytvorenyElement.AppendChild(answer)
                Next
            Else
                Dim i As Integer
                For i = 1 To Me.mVyroky.Count
                    If CType(mVyroky(i), Vyrok).Status = Enums.enmStav.enmFinal Then
                        Dim answer As Xml.XmlElement
                        answer = xmlDoc.CreateElement("answer")
                        Dim value As Xml.XmlElement
                        value = xmlDoc.CreateElement("value")
                        value.InnerText = CType(mVyroky(i), Vyrok).Id
                        answer.AppendChild(value)
                        Dim element As Xml.XmlElement
                        element = xmlDoc.CreateElement("weight")
                        element.InnerText = CType(mVyroky(i), Vyrok).Vaha.ToStr
                        answer.AppendChild(element)
                        vytvorenyElement.AppendChild(answer)
                    End If
                Next
            End If




            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributMultiple_Chyba_pri_SaveToXML_v_atributu_multiple") & Me.mId)
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try

            writer.WriteStartElement("attribute")
            If Not MyBase.SaveToXML(writer, rozsahVah) Then
                Return False
            End If

            writer.WriteStartElement("propositions")
            For Each vyrok As Vyrok In Me.mVyroky
                If Not vyrok.SaveToXML(writer) Then
                    Return Me.SetError(vyrok.LastError)
                End If
            Next
            writer.WriteEndElement()

            writer.WriteEndElement()



            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributMultiple_Chyba_pri_SaveToXML_v_atributu_multiple") & Me.mId)
        End Try
    End Function
#End Region


    Private Function SeznamHodnotObsahuje(ByVal value As String) As Boolean
        Dim str As String
        For Each str In mSeznamHodnot
            If str = value Then Return True
        Next
        Return False
    End Function

    Public Function VlozVahy(ByVal Vahy As String, ByVal rozsahVah As Long, ByVal defaultValue As Interval) As enmVlozeniHodnoty
        Dim poleVah() As String
        poleVah = Split(Vahy, "|")
        If poleVah.Length - 1 <> Me.mSeznamHodnot.Count Then
            Return Me.SetError(GetText("AtributMultiple_Zadane_vahy_nemaji_spravny_pocet"))
        End If
        Dim pozice As Integer = 0
        mHodnoty = New Collection
        For pozice = 1 To Me.mSeznamHodnot.Count
            Dim vaha As New Interval
            Dim str As String
            str = poleVah(pozice - 1)
            If Not Helper.NactiVahu(str, vaha, rozsahVah, defaultValue) Then
                mstatus = Enums.enmStav.enmUntouched
                Return Me.SetError(GetText("AtributMultiple_Chyba_pri_vkladani_vahy") & str & GetText("AtributMultiple_k_atributu") & Me.mId)
            End If
            If Not vaha.JeVNorme Then
                mStatus = Enums.enmStav.enmUntouched
                Return Atribut.enmVlozeniHodnoty.enmVahaMimoRozsah
            End If
            mHodnoty.Add(vaha, mSeznamHodnot(pozice))
        Next
        mStatus = Enums.enmStav.enmFinal

        If Not Me.SpoctiVahu Then
            Return Atribut.enmVlozeniHodnoty.enmChyba
        End If
        Return Atribut.enmVlozeniHodnoty.enmOK
    End Function
    Public Function VlozVahy(ByVal newHodnoty As Collection) As enmVlozeniHodnoty
        Me.mHodnoty.Clear()
        For i As Integer = 1 To newHodnoty.Count
            mHodnoty.Add(newHodnoty(i), mSeznamHodnot(i))
        Next
        
        mStatus = Enums.enmStav.enmFinal

        If Not Me.SpoctiVahu Then
            Return Atribut.enmVlozeniHodnoty.enmChyba
        End If
        Return Atribut.enmVlozeniHodnoty.enmOK
    End Function
End Class
