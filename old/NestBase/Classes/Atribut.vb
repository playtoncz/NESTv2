Public MustInherit Class Atribut
    Inherits GeneralObject

    Protected mId As String
    Protected mTyp As Enums.enmTypAtributu
    Protected mJmeno As String
    Private mScope As Boolean
    Protected mKomentar As String
    Protected mStatus As Enums.enmStav
    Private mIdKontextu As String
    Private mContextThreshold As Double
    Private mKontext As Kontext
    Protected mVaha As Interval
    Private mPozice As Enums.enmPozice
    'private mCisloZdroje as
    
    Protected mVyroky As Collection
    Private mAkce As Collection
    Private mSources As Collection
    
    Public Enum enmVlozeniHodnoty
        enmChyba = 0
        enmOK = 1
        enmHodnotaMimoRozsah = 2
        enmVahaMimoRozsah = 3
        enmHodnotaNeexistuj = 4
    End Enum

#Region "Property"
    Public ReadOnly Property Id() As String
        Get
            Return mId
        End Get
    End Property
    Public ReadOnly Property Vyroky() As Collection
        Get
            Return mVyroky
        End Get
    End Property
    Public ReadOnly Property Typ() As Enums.enmTypAtributu
        Get
            Return mTyp
        End Get
    End Property
    Public ReadOnly Property Vaha() As Interval
        Get
            Return mVaha
        End Get
    End Property

    Public ReadOnly Property Pozice() As Enums.enmPozice
        Get
            Return mPozice
        End Get
    End Property

    Public Property Jmeno() As String
        Get
            If mJmeno = "No name" Then Return mId
            If mJmeno = "" Then Return mId
            Return mJmeno
        End Get
        Set(ByVal value As String)
            mJmeno = value
        End Set
    End Property

    Public Property Komentar() As String
        Get
            Return mKomentar
        End Get
        Set(ByVal value As String)
            mKomentar = Komentar
        End Set
    End Property

    Public ReadOnly Property Status() As Enums.enmStav
        Get
            Return mStatus
        End Get
    End Property

    Public ReadOnly Property Sources() As Collection
        Get
            Return Me.mSources
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)

        mVyroky = New Collection
        mAkce = New Collection
        mSources = New Collection
        mVaha = New Interval(defaultVaha)
        mScope = True
        mStatus = Enums.enmStav.enmUntouched
    End Sub

    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyClass.New(iEnvironment, defaultVaha, iLanguage)
        mId = id
    End Sub

    Public Overridable Function Clear(ByVal defaultVaha As Interval) As Boolean
        If Me.mScope Then
            mVaha.SetValue(defaultVaha)
            'vycisteni vyroku
            Dim vyrok As Vyrok
            For Each vyrok In Me.mVyroky
                If Not vyrok.Clear(defaultVaha) Then
                    Return Me.SetError(vyrok.LastError)
                End If
            Next
            Me.mStatus = Enums.enmStav.enmUntouched
        End If

        Return True
    End Function

    'Funkce spocita vahu vyroku podle hodnot prirazenych atributu
    Public MustOverride Function SpoctiVahu() As Boolean


#Region "prace s XML"
    Public Overridable Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal prahPlatnostiKontextu As Double, ByVal atributy As Collection, ByVal rozsahVah As Long, ByVal defaultVaha As Interval, ByVal zdrojeCBR As Collection, Optional ByVal NscXML As String = "") As Boolean
        Try
            If Not Helper.NactiXMLString(xmlElement, "id", mId) Then
                Return Me.SetError(GetText("Atribut_Chyba_pri_nacitani_id_atributu_z_xml"))
            End If
            Helper.NactiXMLString(xmlElement, "name", mJmeno, mId)
            Helper.NactiXMLString(xmlElement, "comment", mKomentar)
            If Not xmlElement.SelectSingleNode("scope") Is Nothing Then
                Select Case xmlElement.SelectSingleNode("scope").InnerXml
                    Case "environment"
                        mScope = False
                    Case Else
                        mScope = True
                End Select
            Else
                mScope = True
            End If

            Helper.NactiXMLString(xmlElement, "id_context", mIdKontextu, )
            Helper.NactiXMLDouble(xmlElement, "context_threshold", mContextThreshold, prahPlatnostiKontextu)



            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Atribut_Chyba_pri_nacitani_atributu_z_XML"))
        End Try
    End Function

    Protected Function LoadSourcesFromXML(ByVal xmlElement As Xml.XmlElement, ByVal atributy As Collection, ByVal rozsahVah As Long, ByVal defaultVaha As Interval, ByVal zdrojeCBR As Collection, Optional ByVal NscXML As String = "") As Boolean
        Try

            If xmlElement.SelectNodes("sources").Count > 0 Then
                For Each sourceElement As Xml.XmlElement In xmlElement.SelectNodes("sources/source")
                    Dim typZdroje As String
                    If Not Helper.NactiXMLString(sourceElement, "source_type", typZdroje) Then
                        Return Me.SetError(GetText("Atribut_Chyba_pri_nacitani_zdroje"))
                    End If
                    Select Case typZdroje
                        Case "default_weight"
                            Dim implicitniVaha As New ZdrojImplicitniVaha(Environment, atributy, defaultVaha, Language)
                            If Not implicitniVaha.LoadFromXML(sourceElement, rozsahVah, defaultVaha) Then
                                Return Me.SetError(implicitniVaha.LastError)
                            End If
                            Me.Sources.Add(implicitniVaha)

                            For Each vyrok As Vyrok In mVyroky
                                vyrok.Zdroje.Add(implicitniVaha, , 1)
                            Next
                        Case "cbr"
                            Dim cbr As New ZdrojCBR(Environment, atributy, defaultVaha, NscXML, Language)

                            zdrojeCBR.Add(cbr) 'cbr musim loadovat az po nahrani vsech atributu

                            'If Not cbr.LoadFromXML(atributy, rozsahVah, defaultVaha) Then
                            '    Return Me.SetError(cbr.LastError)
                            'End If
                            Me.Sources.Add(cbr)
                            For Each vyrok As Vyrok In mVyroky
                                vyrok.Zdroje.Add(cbr, , 1)
                            Next

                            '++++
                    End Select
                Next

            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Atribut_Chyba_pri_nacitani_zdroju_atributu_z_XML"))
        End Try
    End Function

    Public MustOverride Function LoadAnswerFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval) As Boolean
    Public Overridable Overloads Function SaveToXML(ByVal xmlDoc As Xml.XmlDocument, ByVal parentNode As Xml.XmlElement, ByRef vytvorenyElement As Xml.XmlElement) As Boolean
        Try
            vytvorenyElement = xmlDoc.CreateElement("attribute")
            Dim element As Xml.XmlElement
            element = xmlDoc.CreateElement("id")
            element.InnerText = Me.mId
            vytvorenyElement.AppendChild(element)
            parentNode.AppendChild(vytvorenyElement)
            element = xmlDoc.CreateElement("type")
            element.InnerText = Enums.TxtEnmTypAtributuBasic(Me.mTyp)
            vytvorenyElement.AppendChild(element)


            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Atribut_Chyba_pri_SaveToXML_v_atributu") & Me.mId)
        End Try
    End Function

    Public Overridable Overloads Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try


            writer.WriteElementString("id", mId)
            If mJmeno <> "No name" Then writer.WriteElementString("name", Me.mJmeno)
            writer.WriteElementString("type", Enums.TxtEnmTypAtributuBasic(Me.mTyp))
            writer.WriteElementString("comment", Me.mKomentar)
            'zdroje
            'Dim zdrojeExtra As Boolean = False
            'If mVyroky.Count > 0 Then
            '    Dim vyrok As Vyrok = Me.mVyroky(1)
            '    For Each zdroj As Zdroj In vyrok.Zdroje
            '        If zdroj.Typ <> Enums.enmTypZdroje.enmOdvozovani And zdroj.Typ <> Enums.enmTypZdroje.enmDotaz Then
            '            zdrojeExtra = True
            '        End If
            '    Next



            '    If zdrojeExtra Then
            writer.WriteStartElement("sources")
            For Each zdroj As Zdroj In Sources
                If zdroj.Typ <> Enums.enmTypZdroje.enmOdvozovani And zdroj.Typ <> Enums.enmTypZdroje.enmDotaz Then
                    If Not zdroj.SaveToXML(writer, rozsahVah) Then
                        Return Me.SetError(zdroj.LastError)
                    End If
                End If
            Next
            writer.WriteEndElement()
            '    End If
            'End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Atribut_Chyba_pri_SaveToXML_v_atributu") & Me.mId)
        End Try
    End Function

#End Region

#Region "vypis"
    Public Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Try
            Helper.VypisAddLine(text, mId, typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, Me.GetText("Atribut"), typVypisu, Enums.enmTypRadkyVypisu.enmNadpis2)
            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniStart)
            Helper.VypisAddLine(text, "   " & Me.GetText("id") & ": " & mId, typVypisu)
            Helper.VypisAddLine(text, "   " & Me.GetText("Jmeno") & ": " & mJmeno, typVypisu)
            Helper.VypisAddLine(text, "   " & Me.GetText("Komentar") & ": " & mKomentar, typVypisu)
            Helper.VypisAddLine(text, "   " & Me.GetText("Typ") & ": " & Enums.TxtEnmTypAtributu(mTyp, Me.GetResourceManager), typVypisu)
            Helper.VypisAddLine(text, "   " & Me.GetText("Rozsah") & ": " & CStr(mScope), typVypisu)
            '++++

            If Me.Typ <> Enums.enmTypAtributu.enmBinary Then
                'vyroky
                Helper.VypisAddLine(text, "   " & Me.GetText("Vyroky") & ": ", typVypisu, Enums.enmTypRadkyVypisu.enmNadpis2)

                Dim vyrok As Vyrok
                For Each vyrok In Me.mVyroky
                    If Not vyrok.GetVypis(text, typVypisu, rozsahVah) Then
                        Return Me.SetError(vyrok.LastError)
                    End If
                Next
            End If
            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniEnd)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Atribut_Chyba_pri_tvorbe_vypisu_atributu") & mId)
        End Try
    End Function
#End Region

    Public Function UrciPozici(ByVal atributy As Collection) As Boolean
        Try
            Dim vyrok As Vyrok
            For Each vyrok In Me.mVyroky
                If Not vyrok.UrciPozici Then
                    Return Me.SetError(vyrok.LastError)
                End If
            Next

            Dim pomocnaPozice As Enums.enmPozice = Enums.enmPozice.enmNothing

            For Each vyrok In Me.mVyroky
                If vyrok.Pozice <> Enums.enmPozice.enmAlone Then
                    If Not pomocnaPozice = Enums.enmPozice.enmNothing Then
                        If vyrok.Pozice <> pomocnaPozice Then
                            Return Me.SetError(GetText("Atribut_Vyroky_maji_ruzne_pozice_pro_atribut") & Me.mId)
                        End If
                    End If
                    pomocnaPozice = vyrok.Pozice
                End If
            Next
            Me.mPozice = pomocnaPozice

            'uprava pozice podle zdroje
            'pozice dotazu u calculation
            If mPozice = Enums.enmPozice.enmAlone Or mPozice = Enums.enmPozice.enmQuestion Or mPozice = Enums.enmPozice.enmNothing Then
                If Me.Typ = Enums.enmTypAtributu.enmNumeric Then
                    For Each atribut As Atribut In atributy
                        If atribut.Typ = Enums.enmTypAtributu.enmNumeric Then
                            ' If atribut.Vyroky.Count > 0 Then
                            'Dim vyrokn As Vyrok = atribut.Vyroky(1)
                            For Each Zdroj As Zdroj In CType(atribut, AtributNumeric).CalculationSources ' vyrokn.Zdroje
                                'If Zdroj.Typ = Enums.enmTypZdroje.enmVypocet Then
                                Dim vypocet As ZdrojVypocet = Zdroj
                                For Each clen As Clen In vypocet.Cleny
                                    If clen.Vstup1 = "a" & mId Then
                                        mPozice = Enums.enmPozice.enmQuestion
                                        Return True
                                    End If
                                    If clen.Vstup2 = "a" & mId Then
                                        mPozice = Enums.enmPozice.enmQuestion
                                        Return True
                                    End If
                                Next
                                'End If
                            Next
                            'End If
                        End If
                    Next
                End If
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Atribut_Chyba_pri_urcovani_pozice_atributu") & Me.mId)
        End Try
    End Function

    Public Function GetPocetZdroju() As Long
        Dim pocet As Long = 0
        If mVyroky.Count > 0 Then
            Dim vyrok As Vyrok = mVyroky(1)
            For Each zdroj As Zdroj In vyrok.Zdroje
                If zdroj.Typ <> Enums.enmTypZdroje.enmDotaz And zdroj.Typ <> Enums.enmTypZdroje.enmOdvozovani Then
                    pocet += 1
                End If
            Next
        End If

        Return pocet
    End Function
End Class
