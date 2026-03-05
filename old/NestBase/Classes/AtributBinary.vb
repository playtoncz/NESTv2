Public Class AtributBinary
    Inherits Atribut

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmBinary
    End Sub

    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(id, iEnvironment, defaultVaha, iLanguage)
        Me.mTyp = Enums.enmTypAtributu.enmBinary
    End Sub

    Public Overrides Function SpoctiVahu() As Boolean
        Try
            If Me.mStatus = Enums.enmStav.enmUntouched Then
                Return True
            End If

            Dim vyrok As Vyrok
            vyrok = CType(Me.mVyroky(1), Vyrok)
            vyrok.Vaha.SetValue(Me.mVaha)
            vyrok.Status = Enums.enmStav.enmFinal
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributBinary_Chyba_pri_pocitani_vahy_u_atributu") & Me.mId)
        End Try
    End Function

    Public Function VlozVahu(ByVal Value As String, ByVal defaultVaha As Interval, ByVal rozsahVah As Long) As enmVlozeniHodnoty
        Try
            If Not Helper.NactiVahu(Value, mVaha, rozsahVah, defaultVaha) Then
                mStatus = Enums.enmStav.enmUntouched
                Return Atribut.enmVlozeniHodnoty.enmChyba
            Else
                If mvaha.JeVNorme Then
                    mStatus = Enums.enmStav.enmFinal
                Else
                    mStatus = Enums.enmStav.enmUntouched
                    Return Atribut.enmVlozeniHodnoty.enmVahaMimoRozsah
                End If
            End If

            If Not Me.SpoctiVahu Then
                Return Atribut.enmVlozeniHodnoty.enmChyba
            End If

            Return Atribut.enmVlozeniHodnoty.enmOK
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributBinary_Chyba_pri_Vkladani_vahy_u_atributu") & Me.mId)
        End Try
    End Function

    Public Function VlozVahu(ByVal Value As Interval) As enmVlozeniHodnoty
        Try
            Vaha.SetValue(Value)
            If Not Me.SpoctiVahu Then
                Return Atribut.enmVlozeniHodnoty.enmChyba
            End If

            Return Atribut.enmVlozeniHodnoty.enmOK
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributBinary_Chyba_pri_Vkladani_vahy_u_atributu") & Me.mId)
        End Try
    End Function


#Region "Xml"
    Public Overrides Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal prahPlatnostiKontextu As Double, ByVal atributy As Collection, ByVal rozsahVah As Long, ByVal defaultVaha As Interval, ByVal zdrojeCBR As Collection, Optional ByVal NscXML As String = "") As Boolean
        If Not MyBase.LoadFromXML(xmlElement, prahPlatnostiKontextu, atributy, rozsahVah, defaultVaha, zdrojeCBR) Then
            Return False
        End If
        Try
            'vytvoreni vyroku
            Dim vyrok As Vyrok
            vyrok = New Vyrok(Environment, defaultVaha, Me, Me.mId, Language)
            vyrok.Jmeno = Me.mJmeno
            vyrok.Komentar = Me.mKomentar

            'pridani vyroku k atributu
            Me.mVyroky.Add(vyrok, vyrok.Id)




        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributBinary_Chyba_pri_nacitani_binarniho_atributu_z_xml"))
        End Try

        If Not Me.LoadSourcesFromXML(xmlElement, atributy, rozsahVah, defaultVaha, zdrojeCBR, NscXML) Then
            Return False
        End If

        Return True
    End Function

    Public Overrides Function LoadAnswerFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval) As Boolean
        Try
            If Not Helper.NactiXMLVahu(xmlElement.SelectSingleNode("answer"), "weight", mVaha, 1, defaultVaha) Then
                'Return Me.SetError("Nepodaøilo se naèíst váhu odpovìdi pro atribut: " & Me.mId)
                mStatus = Enums.enmStav.enmUntouched
            Else
                mStatus = Enums.enmStav.enmFinal
            End If
            'mIsSet = True
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributBinary_Chyba_pri_nacitani_odpovedi_z_xml_u_atributu") & Me.mId)
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByVal xmlDoc As Xml.XmlDocument, ByVal parentNode As Xml.XmlElement, ByRef vytvorenyElement As Xml.XmlElement) As Boolean
        Try
            If Not MyBase.SaveToXML(xmlDoc, parentNode, vytvorenyElement) Then
                Return False
            End If

            Dim answer As Xml.XmlElement
            answer = xmlDoc.CreateElement("answer")
            Dim element As Xml.XmlElement
            element = xmlDoc.CreateElement("weight")
            'If Me.mStatus = Enums.enmStav.enmFinal Then
            If CType(Me.mVyroky(1), Vyrok).Status = Enums.enmStav.enmFinal Then
                element.InnerText = CType(Me.mVyroky(1), Vyrok).Vaha.ToStr
            End If

            answer.AppendChild(element)

            vytvorenyElement.AppendChild(answer)

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributBinary_Chyba_pri_SaveToXML_v_atributu_binary") & Me.mId)
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try

            writer.WriteStartElement("attribute")
            If Not MyBase.SaveToXML(writer, rozsahVah) Then
                Return False
            End If



            writer.WriteEndElement()

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("AtributBinary_Chyba_pri_SaveToXML_v_atributu_binary") & Me.mId)
        End Try
    End Function

#End Region

End Class
