Public Class PravidloApriori
    Inherits Pravidlo


    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        Me.mTyp = Enums.enmTypPravidla.enmApriori
    End Sub


    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyClass.New(iEnvironment, iLanguage)
        mId = id
    End Sub

#Region "Xml"
    Public Overrides Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval, ByVal prahPlatnostiKontextu As Double, ByVal prahPlatnostiPredpokladu As Double, ByVal Atributy As Collection, ByVal Kontexty As Collection, ByVal rozsahVah As Long) As Boolean
        If Not MyBase.LoadFromXML(xmlElement, defaultVaha, prahPlatnostiKontextu, prahplatnostipredpokladu, Atributy, Kontexty, rozsahVah) Then
            Return False
        End If
        Try
            ''kontext
            'Dim idKontext As String
            'If Helper.NactiXMLString(xmlElement, "id_context", idKontext, "") Then
            '    Dim kontext As Kontext
            '    kontext = Kontexty(idKontext)
            '    Me.mKontext = kontext
            'End If


            'Helper.NactiXMLDouble(xmlElement, "context_threshold", mKontextThreshold, prahPlatnostiKontextu)

            ''nacteni predpokladu
            'Dim predpokladElement As Xml.XmlElement
            'predpokladElement = xmlElement.SelectSingleNode("condition")
            'mPredpoklad = New Predpoklad(Environment, defaultVaha)
            'If Not mPredpoklad.LoadFromXML(predpokladElement, defaultVaha, Atributy, rozsahVah) Then
            '    Return Me.AppendLastError(mPredpoklad.LastError, " : pravidlo " & Me.mId)
            'End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("PravidloApriori_Chyba_pri_nacitani_apriori_pravidla_z_XML"))
        End Try


    End Function

    Public Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try
            writer.WriteStartElement("apriori_rule")

            writer.WriteElementString("id", Mid)
            writer.WriteElementString("comment", Me.mKomentar)
            '++++ priorita

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
            Return Me.SetError(ex, GetText("PravidloApriori_Chyba_pøi_SaveToXML_v_apriori_pravidlu") & Me.mId)
        End Try
    End Function
#End Region

    Public Overrides Function PropojeniBaze() As Boolean
        Try
            If Not MyBase.PropojeniBaze Then
                Return False
            End If

            ''propojeni predpokladu
            'If Not mPredpoklad.PropojeniPredpokladu(Me) Then
            '    Return Me.SetError(mPredpoklad.LastError)
            'End If

            ''propojeni kontextu
            'If Not mKontext Is Nothing Then
            '    mKontext.SeznamPravidel.Add(Me, Me.mId)
            'End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("PravidloApriori_Chyba_pri_propojovani_apriori_pravidla") & Me.mId)
        End Try
    End Function

    Public Overrides Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Helper.VypisAddLine(text, Me.GetText("Apriori"), typVypisu, Enums.enmTypRadkyVypisu.enmNadpis2)
        Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniStart)
        If Not MyBase.GetVypis(text, typVypisu, rozsahVah) Then
            Return False
        End If
        Try
            Dim str As String = ""

            
            'zaver
            str += " THEN "

            Dim zaver As Zaver
            For i As Integer = 1 To mZavery.Count
                zaver = mZavery(i)
                If i > 1 Then
                    str += ", "
                End If
                If Not zaver.GetVypis(str, typVypisu, rozsahVah) Then
                    Return Me.SetError(zaver.LastError)
                End If
            Next

            Helper.VypisAddLine(text, str, typVypisu, Enums.enmTypRadkyVypisu.enmNormal)

            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniEnd)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("PravidloApriori_Chyba_pri_tvorbe_vypisu_apriori_pravidla") & Me.mId)
        End Try
    End Function

    Public Overrides Function AplikujBackward(ByRef uspesneAplikovano As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
        Try
            'pravidlo uz bylo vyhodnoceno drive
            If Me.mStatus = Enums.enmStav.enmFinal Or Me.mStatus = Enums.enmStav.enmProvisional Then
                uspesneAplikovano = True
                Return True
            End If

            ''ma-li pravidlo kontext
            'If Not Me.mKontext Is Nothing Then
            '    Dim vyhodnocenyKontext As Boolean
            '    If Not mKontext.VyhodnotBackward(vyhodnocenyKontext, neurcitost, aktSeznamDotazu) Then
            '        Return Me.SetError(mKontext.LastError)
            '    End If

            '    If Not vyhodnocenyKontext Then
            '        mstatus = Enums.enmStav.enmPartial
            '        uspesneAplikovano = False
            '        Return True
            '    End If

            '    'konetxt nepresah prah platnosti
            '    Dim zav As Zaver
            '    If (mKontext.Vaha.MaxHodnota < Me.mKontextThreshold) Or (mKontext.Vaha.MaxHodnota = 0 And Me.mKontextThreshold = 0) Then
            '        mstatus = Enums.enmStav.enmFinal

            '        For Each zav In Me.mZavery
            '            zav.Ctr.SetValue(Environment.vahaIrrelevant)
            '        Next
            '        uspesneAplikovano = True
            '        Return True
            '    End If

            '    'kontext presah prah platnosti
            '    For Each zav In Me.mZavery
            '        zav.VahaZaveru.SetValue(neurcitost.CTR(mKontext.Vaha, zav.VahaZaveru))
            '    Next

            '    mstatus = mKontext.Status
            'End If

            ''vyhodnoceni predpokladu
            'Dim vyhodnoceniPredpokladu As Boolean
            'If Not Me.mPredpoklad.VyhodnotBackward(vyhodnoceniPredpokladu, neurcitost, aktSeznamDotazu) Then
            '    Return Me.SetError(mPredpoklad.LastError)
            'End If

            ''predpoklad nebyl vyhodnocen
            'If Not vyhodnoceniPredpokladu Or mPredpoklad.Status = Enums.enmStav.enmPartial Or mPredpoklad.Status = Enums.enmStav.enmUntouched Then
            '    mstatus = Enums.enmStav.enmPartial
            '    uspesneAplikovano = True 'todle tady nebylo
            '    Return True
            'End If

            ''predpoklad byl vyhodnocen

            ''predpoklad neby splnen

            'If mPredpoklad.Vaha.MaxHodnota <= 0 Then
            '    mstatus = mPredpoklad.Status
            '    For Each zaver In mzavery
            '        zaver.Ctr.SetValue(Environment.vahaIrrelevant)
            '    Next
            '    uspesneAplikovano = True
            '    Return True
            'End If

            ''predpoklad byl splnen
            Dim zaver As Zaver
            For Each zaver In mZavery
                Dim pomocnyInterval As New Interval
                pomocnyInterval = neurcitost.CTR(Environment.vahaTrue, zaver.Ctr)
                If zaver.NegaceZaveru Then pomocnyInterval.SetValue(neurcitost.NEG(pomocnyInterval))
                zaver.Ctr.SetValue(neurcitost.NORM(pomocnyInterval))
            Next
            mStatus = Enums.enmStav.enmFinal
            'If mPredpoklad.Status = Enums.enmStav.enmProvisional Then
            '    mstatus = Enums.enmStav.enmProvisional
            'Else
            '    If mKontext Is Nothing Then
            '        mstatus = Enums.enmStav.enmFinal
            '    End If
            'End If

            'provedeni akci
            '+

            uspesneAplikovano = True
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("PravidloApriori_Chyba_pri_aplikovani_apriori_pravidla_backward") & Me.mId)
        End Try
    End Function

    Public Function PridejPravidloZeStringu(ByVal pravidloStr As String, ByVal DefaultVaha As Interval, ByVal atributy As Collection) As Boolean
        pravidloStr = LCase(pravidloStr)

        Dim r As New System.Text.RegularExpressions.Regex("^(if |.* if )(.*)( then )(.*)$")
        Dim m As System.Text.RegularExpressions.Match
        m = r.Match(pravidloStr)
        If Not m.Success Then
            Return Me.SetError("pravidlo nemá požadovanou syntaxi")
        End If


        Dim kontextStr As String
        Dim pravidloBezKontextuStr As String
        kontextStr = r.Replace(pravidloStr, "$1")
        kontextStr = kontextStr.Substring(0, kontextStr.Length - 3)
        pravidloBezKontextuStr = r.Replace(pravidloStr, "$2$3$4")


        Dim predpokladStr As String
        Dim zaveryStr As String
        Dim str() As String
        str = Split(pravidloBezKontextuStr, "then")

        predpokladStr = str(0)
        zaveryStr = str(1)


        'zpracovani zaveru
        Dim zaverStr() As String
        zaverStr = Split(zaveryStr, ",")
        For Each jedenZaver As String In zaverStr
            Dim zaver As New Zaver(Environment, DefaultVaha, Language)
            If Not zaver.PridejZaverZeStringu(jedenZaver, DefaultVaha, atributy) Then
                Return Me.SetError(zaver.LastError)
            End If
            Me.mZavery.Add(zaver)
        Next

        Return True
    End Function
End Class
