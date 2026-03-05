Public Class IntegritniOmezeni
    Inherits GeneralObject

    Private mId As String
    Private mJmeno As String
    Private mPredpoklad As Predpoklad
    Private mKomentar As String
    Private mStatus As Enums.enmStav
    Private mKontext As Kontext
    Private mKontextThreshold As Double
    Private mZavery As Collection

#Region "Property"
    Public ReadOnly Property Id() As String
        Get
            Return mId
        End Get
    End Property
    Public ReadOnly Property Status() As Enums.enmStav
        Get
            Return mStatus
        End Get
    End Property
    Public ReadOnly Property Jmeno() As String
        Get
            Return mJmeno
        End Get
    End Property
    Public ReadOnly Property Komentar() As String
        Get
            Return mKomentar
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mZavery = New Collection
        mStatus = Enums.enmStav.enmUntouched
        mPredpoklad = Nothing
        mKontext = Nothing
    End Sub

#Region "Xml"
    Public Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval, ByVal prahPlatnostiKontextu As Double, ByVal Atributy As Collection, ByVal Kontexty As Collection, ByVal rozsahVah As Long) As Boolean
        Try
            If Not Helper.NactiXMLString(xmlElement, "id", mId) Then
                Return Me.SetError(GetText("IntegritniOmezeni_Chyba_pri_nacitani_id_integritniho_omezeni_z_xml"))
            End If
            Helper.NactiXMLString(xmlElement, "name", mJmeno, "No name")
            Helper.NactiXMLString(xmlElement, "comment", mKomentar)

            'nacteni zaverù
            Dim zaverElement As Xml.XmlElement
            For Each zaverElement In xmlElement.SelectNodes("conclusions/conclusion")
                Dim zaver As Zaver
                zaver = New Zaver(Environment, defaultVaha, Language)
                If Not zaver.LoadFromXML(zaverElement, defaultVaha, Atributy, rozsahVah) Then
                    Return Me.AppendLastError(zaver.LastError, GetText("IntegritniOmezeni_integritni_omezeni") & mId)
                End If
                mZavery.Add(zaver)
            Next

            'kontext
            Dim idKontext As String
            If Helper.NactiXMLString(xmlElement, "id_context", idKontext, "") Then
                Dim kontext As Kontext
                kontext = Kontexty(idKontext)
                Me.mKontext = kontext
            End If


            Helper.NactiXMLDouble(xmlElement, "context_threshold", mKontextThreshold, prahPlatnostiKontextu)

            'nacteni predpokladu
            Dim predpokladElement As Xml.XmlElement
            predpokladElement = xmlElement.SelectSingleNode("condition")
            mPredpoklad = New Predpoklad(Environment, defaultVaha, Language)
            If Not mPredpoklad.LoadFromXML(predpokladElement, defaultVaha, Atributy, rozsahVah) Then
                Return Me.AppendLastError(mPredpoklad.LastError, GetText("IntegritniOmezeni_pravidlo") & Me.mId)
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("IntegritniOmezeni_Chyba_pri_nacitani_integritniho_omezeni_z_XML"))
        End Try


    End Function

    Public Overloads Function SaveToXML(ByVal xmlDoc As Xml.XmlDocument, ByVal parentNode As Xml.XmlElement, ByRef vytvorenyElement As Xml.XmlElement, ByVal rozsahVah As Long) As Boolean
        Try
            vytvorenyElement = xmlDoc.CreateElement("integrity_constraint")
            Dim element As Xml.XmlElement
            element = xmlDoc.CreateElement("id")
            element.InnerText = Me.mId
            vytvorenyElement.AppendChild(element)

            element = xmlDoc.CreateElement("name")
            If mJmeno <> "No name" Then element.InnerText = mJmeno
            vytvorenyElement.AppendChild(element)
            element = xmlDoc.CreateElement("comment")
            element.InnerText = mKomentar
            vytvorenyElement.AppendChild(element)
            For Each zaver As Zaver In Me.mZavery
                element = xmlDoc.CreateElement("weight")
                element.InnerText = zaver.Ctr.ToStr(rozsahVah)
                vytvorenyElement.AppendChild(element)
            Next


            parentNode.AppendChild(vytvorenyElement)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("IntegritniOmezeni_Chyba_pri_SaveToXML_v_integritnim_omezeni") & Me.mId)
        End Try
    End Function

    Public Overloads Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try
            writer.WriteStartElement("integrity_constraint")

            writer.WriteElementString("id", mId)
            If mJmeno <> "No name" Then writer.WriteElementString("name", Me.mJmeno)
            writer.WriteElementString("comment", Me.mKomentar)
            If Not Me.mKontext Is Nothing Then
                writer.WriteElementString("id_context", mKontext.Id)
                writer.WriteElementString("context_threshold", Helper.FloatToStr(Me.mKontextThreshold))
            End If
            '++++ priorita
            '++++ condition threshold

            If Not Me.mPredpoklad.SaveToXML(writer) Then
                Return Me.SetError(mPredpoklad.LastError)
            End If

            writer.WriteStartElement("conclusions")
            For Each zaver As Zaver In Me.mZavery
                If Not zaver.SaveToXML(writer, True, rozsahVah) Then
                    Return Me.SetError(zaver.LastError)
                End If
            Next

            writer.WriteEndElement()

            writer.WriteEndElement()
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("IntegritniOmezeni_Chyba_pri_SaveToXML_v_integritnim_omezeni") & Me.mId)
        End Try
    End Function
#End Region

    'Public Overridable Function PropojeniBaze() As Boolean
    '    Try
    '        Dim zaver As Zaver
    '        For Each zaver In mZavery
    '            zaver.Literal.Vyrok.SeznamZaveru.Add(Me)
    '        Next
    '        Return True
    '    Catch ex As Exception
    '        Return Me.SetError(ex, , "Chyba pri propojovani pravidla " & Me.mId)
    '    End Try
    'End Function

    Public Function Vyhodnot(ByVal neurcitost As Neurcitost, ByVal bezdotazu As Boolean) As Boolean
        Try
            'int.om. uz bylo vyhodnoceno drive
            If Me.mStatus = Enums.enmStav.enmFinal Or Me.mStatus = Enums.enmStav.enmProvisional Then
                'uspesneVyhodnoceno = True
                Return True
            End If

            'ma-li int.om. kontext
            If Not Me.mKontext Is Nothing Then
                Dim vyhodnocenyKontext As Boolean
                If Not mKontext.VyhodnotBackward(vyhodnocenyKontext, neurcitost, Nothing, Nothing, bezdotazu) Then
                    Return Me.SetError(mKontext.LastError)
                End If

                If Not vyhodnocenyKontext Then
                    mStatus = Enums.enmStav.enmPartial
                    'uspesneVyhodnoceno = True
                    Return True
                End If

                'konetxt nepresah prah platnosti
                Dim zav As Zaver
                If (mKontext.Vaha.MaxHodnota < Me.mKontextThreshold) Or (mKontext.Vaha.MaxHodnota = 0 And Me.mKontextThreshold = 0) Then
                    mStatus = Enums.enmStav.enmFinal

                    For Each zav In Me.mZavery
                        zav.Ctr.SetValue(Environment.vahaIrrelevant)
                    Next
                    'uspesneVyhodnoceno = True
                    Return True
                End If

                'kontext presah prah platnosti
                For Each zav In Me.mZavery
                    zav.VahaZaveru.SetValue(neurcitost.CTR(mKontext.Vaha, zav.VahaZaveru))
                Next

                mStatus = mKontext.Status
            End If

            'vyhodnoceni predpokladu
            Dim vyhodnoceniPredpokladu As Boolean
            If Not Me.mPredpoklad.VyhodnotBackward(vyhodnoceniPredpokladu, neurcitost, Nothing, Nothing, bezdotazu) Then
                Return Me.SetError(mPredpoklad.LastError)
            End If

            'predpoklad nebyl vyhodnocen
            If Not vyhodnoceniPredpokladu Or mPredpoklad.Status = Enums.enmStav.enmPartial Or mPredpoklad.Status = Enums.enmStav.enmUntouched Then
                mStatus = Enums.enmStav.enmPartial
                'uspesneVyhodnoceno = True 'todle tady nebylo
                Return True
            End If

            'predpoklad byl vyhodnocen


            'predpoklad nebyl splnen
            Dim zaver As Zaver
            If mPredpoklad.Vaha.MaxHodnota <= 0 Then
                mStatus = mPredpoklad.Status
                For Each zaver In mZavery
                    zaver.Ctr.SetValue(Environment.vahaIrrelevant)
                Next
                'uspesneVyhodnoceno = True
                Return True
            End If

            If mPredpoklad.Status = Enums.enmStav.enmProvisional Then
                mStatus = Enums.enmStav.enmProvisional
            Else
                If mKontext Is Nothing Then
                    mStatus = Enums.enmStav.enmFinal
                End If
            End If
            'predpoklad byl splnen
            For Each zaver In mZavery
                Dim pomocnyInterval As New Interval

                If zaver.NegaceZaveru Then
                    pomocnyInterval = neurcitost.IMP(mPredpoklad.Vaha, neurcitost.NEG(zaver.VahaZaveru))
                Else
                    pomocnyInterval = neurcitost.IMP(mPredpoklad.Vaha, zaver.VahaZaveru)
                End If

                pomocnyInterval = neurcitost.CTR(pomocnyInterval, zaver.Ctr)



                'If zaver.NegaceZaveru Then pomocnyInterval.SetValue(neurcitost.NEG(pomocnyInterval))
                zaver.Ctr.SetValue(neurcitost.NORM(pomocnyInterval))
                If zaver.Ctr.MinHodnota > 0 Then
                    Me.mStatus = Enums.enmStav.enmError
                End If
            Next




            'uspesneVyhodnoceno = True
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("IntegritniOmezeni_Chyba_pri_vyhodnocovani_integritniho_omezeni") & Me.mId)
        End Try
    End Function
End Class
