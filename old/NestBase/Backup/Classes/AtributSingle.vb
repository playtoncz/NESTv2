Public Class AtributSingle
    Inherits Atribut

    Private mHodnota As String
    Private mSeznamHodnot As Collection

#Region "Property"
    Public Property Hodnota() As String
        Get
            Return mHodnota
        End Get
        Set(ByVal Value As String)
            mHodnota = Value
        End Set
    End Property
    Public ReadOnly Property SeznamHodnot() As Collection
        Get
            Return mSeznamHodnot
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmSingle
        mSeznamHodnot = New Collection
    End Sub

    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(id, iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmMultiple       
        mSeznamHodnot = New Collection
    End Sub

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
                    If vyrok.Id = Me.mHodnota Then
                        vyrok.Vaha.SetValue(mvaha)
                    Else
                        vyrok.Vaha.SetValue(Environment.vahaFalse)
                    End If
                    vyrok.Status = Enums.enmStav.enmFinal
                Next
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributSingle_Chyba_pri_pocitani_vahy_u_atributu") & Me.mId)
        End Try
    End Function

#Region "Xml"
    Public Overrides Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal prahPlatnostiKontextu As Double, ByVal atributy As Collection, ByVal rozsahVah As Long, ByVal defaultVaha As Interval, ByVal zdrojeCBR As Collection, Optional ByVal NscXML As String = "") As Boolean
        If Not MyBase.LoadFromXML(xmlElement, prahPlatnostiKontextu, atributy, rozsahVah, defaultVaha, zdrojeCBR) Then
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
            Return Me.SetError(ex, GetText("AtributSingle_Chyba_pri_nacitani_single_atributu_z_xml"))
        End Try
        If Not Me.LoadSourcesFromXML(xmlElement, atributy, rozsahVah, defaultVaha, zdrojeCBR, NscXML) Then
            Return False
        End If

        Return True
    End Function

    Public Overrides Function LoadAnswerFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval) As Boolean
        Try
            Dim odpovedElement As Xml.XmlElement
            odpovedElement = xmlElement.SelectSingleNode("answer")

            If Not Helper.NactiXMLVahu(odpovedElement, "weight", mVaha, 1, defaultVaha) Then
                mStatus = Enums.enmStav.enmUntouched
            Else
                If Not Helper.NactiXMLString(odpovedElement, "value", mHodnota, "") Then
                    Return Me.SetError(GetText("AtributSingle_Nepodarilo_se_nacist_hodnotu_odpovedi_pro_atribut") & Me.mId)
                End If
                If Not SeznamHodnotObsahuje(mHodnota) Then
                    Return Me.SetError(GetText("AtributSingle_Hodnota_odpovedi_neexistuje_pro_atribut") & Me.mId)
                End If

                mStatus = Enums.enmStav.enmFinal
            End If


            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributSingle_Chyba_pri_nacitani_odpovedi_z_xml_u_atributu") & Me.mId)
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByVal xmlDoc As Xml.XmlDocument, ByVal parentNode As Xml.XmlElement, ByRef vytvorenyElement As Xml.XmlElement) As Boolean
        Try
            If Not MyBase.SaveToXML(xmlDoc, parentNode, vytvorenyElement) Then
                Return False
            End If

            If Me.mStatus = Enums.enmStav.enmFinal Then
                Dim answer As Xml.XmlElement
                answer = xmlDoc.CreateElement("answer")
                Dim value As Xml.XmlElement
                value = xmlDoc.CreateElement("value")
                value.InnerText = Me.Hodnota
                answer.AppendChild(value)
                Dim element As Xml.XmlElement
                element = xmlDoc.CreateElement("weight")
                element.InnerText = Me.mVaha.ToStr
                answer.AppendChild(element)
                vytvorenyElement.AppendChild(answer)
            Else
                If mVyroky.Count = 1 Then
                    If CType(mVyroky(1), Vyrok).Status = Enums.enmStav.enmFinal Then
                        Dim answer As Xml.XmlElement
                        answer = xmlDoc.CreateElement("answer")
                        Dim value As Xml.XmlElement
                        value = xmlDoc.CreateElement("value")
                        value.InnerText = CType(mVyroky(1), Vyrok).Id
                        answer.AppendChild(value)
                        Dim element As Xml.XmlElement
                        element = xmlDoc.CreateElement("weight")
                        element.InnerText = CType(mVyroky(1), Vyrok).Vaha.ToStr
                        answer.AppendChild(element)
                        vytvorenyElement.AppendChild(answer)
                    End If
                End If
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributSingle_Chyba_pri_SaveToXML_v_atributu_single") & Me.mId)
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
            Return Me.SetError(ex, GetText("AtributSingle_Chyba_pri_SaveToXML_v_atributu_single") & Me.mId)
        End Try
    End Function
#End Region

    Public Function VlozHodotu(ByVal Hodnota As String, ByVal Vaha As String, ByVal defaultVaha As Interval, ByVal rozsahVah As Long) As enmVlozeniHodnoty
        Try
            If Not SeznamHodnotObsahuje(Hodnota) Then
                Return Atribut.enmVlozeniHodnoty.enmHodnotaNeexistuj
            End If
            Me.Hodnota = Hodnota

            If Not Helper.NactiVahu(Vaha, mVaha, rozsahVah, defaultVaha) Then
                mStatus = Enums.enmStav.enmUntouched
                Return Atribut.enmVlozeniHodnoty.enmChyba
            Else
                mStatus = Enums.enmStav.enmFinal
            End If
            If Not mvaha.JeVNorme Then
                Return Atribut.enmVlozeniHodnoty.enmVahaMimoRozsah
            End If

            If Not Me.SpoctiVahu Then
                Return Atribut.enmVlozeniHodnoty.enmChyba
            End If

            Return Atribut.enmVlozeniHodnoty.enmOK
        Catch ex As Exception
            Me.SetError(ex, GetText("AtributSingle_Chyba_pri_Vkladani_vahy_u_atributu") & Me.mId)
            Return Atribut.enmVlozeniHodnoty.enmChyba
        End Try
    End Function

    Private Function SeznamHodnotObsahuje(ByVal value As String) As Boolean
        Dim str As String
        For Each str In mSeznamHodnot
            If str = value Then Return True
        Next
        Return False
    End Function
End Class
