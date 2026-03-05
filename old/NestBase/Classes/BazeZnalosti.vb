Imports System.Xml
Public Class BazeZnalosti
    Inherits GeneralObject

    Private mPopis As String
    Private mExpert As String
    Private mInzenyr As String
    Private mDatumVytvoreni As Date
    Private mInferencniMechanismus As Enums.enmInferencniMechanismus
    Private mSlope As Single
    Private mRozsahVah As Long
    Private mGlobalPriority As Enums.enmPriorita
    Private mPrahPlatnostiKontextu As Single
    Private mPrahPlatnostiPredpokladu As Single
    Private mDefaultVaha As Interval

    Private mPravidla As Collection
    Private mAtributy As Collection
    Private mKontexty As Collection
    Private mIntegritniOmezeni As Collection

    Private mCile As Collection
    Private mAktualniDotazy As Collection

    Private mSystemInfoXML As String



#Region "Property"
    'Public Property Popis() As String
    '    Get
    '        Return mPopis
    '    End Get
    '    Set(ByVal Value As String)
    '        mPopis = Value
    '    End Set
    'End Property

    Public ReadOnly Property Atributy() As Collection
        Get
            Return mAtributy
        End Get        
    End Property
    
    Public ReadOnly Property DefaultVaha() As Interval
        Get
            Return mDefaultVaha
        End Get
    End Property

    Public ReadOnly Property RozsahVah() As Long
        Get
            Return mRozsahVah
        End Get
    End Property

    Public ReadOnly Property Cile() As Collection
        Get
            Return mCile
        End Get
    End Property

    Public ReadOnly Property Expert() As String
        Get
            Return Me.mExpert
        End Get
    End Property

    Public ReadOnly Property Inzenyr() As String
        Get
            Return mInzenyr
        End Get
    End Property

    Public Property DatumVytvoreni() As Date
        Get
            Return mDatumVytvoreni
        End Get
        Set(ByVal value As Date)
            mDatumVytvoreni = value
        End Set

    End Property

    Public ReadOnly Property Popis() As String
        Get
            Return mPopis
        End Get
    End Property

    Public Property InferencniMechanismus() As Enums.enmInferencniMechanismus
        Get
            Return Me.mInferencniMechanismus
        End Get
        Set(ByVal value As Enums.enmInferencniMechanismus)
            mInferencniMechanismus = value
        End Set
    End Property

    Public Property PrahPlatnostiPredpokladu() As Single
        Get
            Return mPrahPlatnostiPredpokladu
        End Get
        Set(ByVal value As Single)
            mPrahPlatnostiPredpokladu = value
        End Set
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)

        mRozsahVah = 1
        mDefaultVaha = New Interval
        mPravidla = New Collection
        mAtributy = New Collection
        mKontexty = New Collection
        mIntegritniOmezeni = New Collection

        mCile = New Collection
        mAktualniDotazy = New Collection
    End Sub

    Public Sub New(ByVal iEnvironment As Environment, ByVal BazeZnalostiXML As String, ByVal AnswerXML As String, ByVal iLanguage As Enums.enmLanguage)
        Me.New(iEnvironment, iLanguage)
        If BazeZnalostiXML <> "" Then
            If Me.LoadBaseFromXML(BazeZnalostiXML) Then
                ' If Me.PropojeniBaze() Then
                If AnswerXML <> "" Then Me.LoadAnswersFromXML(AnswerXML)
                'End If
            End If
        End If
    End Sub

    'Funkce vyèistí bázi znalostí od všechn údajù pøedchozí konzultace
    Public Function Clear() As Boolean
        'vycisteni atributu a vyroku
        Dim atribut As Atribut
        For Each atribut In Me.mAtributy
            If Not atribut.Clear(Me.mDefaultVaha) Then
                Return Me.SetError(atribut.LastError)
            End If
        Next

        'vycisteni kontextu
        Dim kontext As Kontext
        For Each kontext In Me.mKontexty
            If Not kontext.Clear(Me.mDefaultVaha) Then
                Return Me.SetError(kontext.LastError)
            End If
        Next
        'vycisteni pravidel
        Dim pravidlo As Pravidlo
        For Each pravidlo In Me.mPravidla
            If Not pravidlo.Clear(Me.mDefaultVaha) Then
                Return Me.SetError(pravidlo.LastError)
            End If
        Next

        'vycisteni integritnich omezeni
        '++++

        'vycisteni aktualnich dotazu
        mAktualniDotazy = New Collection

        Return True
    End Function

#Region "Xml"
    Public Function LoadBaseFromXML(ByVal BazeZnalostiXML As String, Optional ByVal AddBZ As Boolean = False, Optional ByVal NcsXML As String = "") As Boolean
        Dim xmlDoc As New XmlDocument

        Dim ZdrojeCBR As New Collection

        BazeZnalostiXML = BazeZnalostiXML.Replace("<!DOCTYPE base SYSTEM ""base.dtd"">", "")
        BazeZnalostiXML = BazeZnalostiXML.Replace("xmlns=""http://www.vse.cz/NEST""", "")

        'nacteni xmldokumentu
        Try
            xmlDoc.LoadXml(BazeZnalostiXML)
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Nepodarilo_se_nacist_xml_dokument"))
        End Try

        Dim xmlElement As XmlElement

        'nacteni zakladnich udaju
        Try
            xmlElement = xmlDoc.SelectSingleNode("//global")
            Helper.NactiXMLString(xmlElement, "description", mPopis)
            Helper.NactiXMLString(xmlElement, "expert", mExpert)
            Helper.NactiXMLString(xmlElement, "knowledge_engineer", mInzenyr)
            Helper.NactiXMLDatum(xmlElement, "date", mDatumVytvoreni)

            Select Case xmlElement.SelectSingleNode("inference_mechanism").InnerXml
                Case "logical"
                    mInferencniMechanismus = Enums.enmInferencniMechanismus.Logicky
                Case "neural"
                    mInferencniMechanismus = Enums.enmInferencniMechanismus.Neuronovy
                Case "hybrid"
                    mInferencniMechanismus = Enums.enmInferencniMechanismus.Hybridni
                Case Else
                    mInferencniMechanismus = Enums.enmInferencniMechanismus.Standardni
            End Select
            Helper.NactiXMLSingle(xmlElement, "slope", mSlope, 1)
            Helper.NactiXMLLong(xmlElement, "weight_range", mRozsahVah, 1)

            Select Case xmlElement.SelectSingleNode("default_weight").InnerXml
                Case "unknown"
                    mDefaultVaha.SetValue(Me.Environment.vahaUnknown)
                Case "irrelevant"
                    mDefaultVaha.SetValue(Me.Environment.vahaIrrelevant)
            End Select
            Select Case xmlElement.SelectSingleNode("global_priority").InnerXml
                Case "last"
                    mGlobalPriority = Enums.enmPriorita.Last
                Case "minlength"
                    mGlobalPriority = Enums.enmPriorita.MinLength
                Case "maxlength"
                    mGlobalPriority = Enums.enmPriorita.MaxLength
                Case "user"
                    mGlobalPriority = Enums.enmPriorita.User
                Case Else
                    mGlobalPriority = Enums.enmPriorita.First
            End Select
            Helper.NactiXMLSingle(xmlElement, "context_global_threshold", mPrahPlatnostiKontextu)
            Helper.NactiXMLSingle(xmlElement, "condition_global_threshold", mPrahPlatnostiPredpokladu)


        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_nacitani_globalnich_parametru"))
        End Try


        'nacteni atributu
        Try
            For Each xmlElement In xmlDoc.SelectNodes("//attributes/attribute") 'pres vsechny atributy
                Dim atribut As Atribut
                atribut = Nothing
                'vytvorim atribut
                Select Case xmlElement.SelectSingleNode("type").InnerXml
                    Case "binary"
                        atribut = New AtributBinary(Environment, mDefaultVaha, language)
                    Case "single"
                        atribut = New AtributSingle(Environment, mDefaultVaha, Language)
                    Case "multiple"
                        atribut = New AtributMultiple(Environment, mDefaultVaha, Language)
                    Case "numeric"
                        atribut = New AtributNumeric(Environment, mDefaultVaha, Language)
                End Select

                'nactu atribut
                If Not atribut.LoadFromXML(xmlElement, Me.mPrahPlatnostiKontextu, Me.mAtributy, Me.mRozsahVah, Me.mDefaultVaha, ZdrojeCBR, NcsXML) Then
                    Return Me.SetError(atribut.LastError)
                End If

                If Helper.ValueIsInCollection(mAtributy, atribut.Id) Then
                    If AddBZ Then
                        mAtributy.Remove(atribut.Id)
                    Else
                        Return Me.SetError(GetText("BazeZnalosti_Pokus_o_nacteni_druheho_atributu") & atribut.Id)
                    End If
                End If
                'pridam atribut do baze
                Me.mAtributy.Add(atribut, atribut.Id)
            Next

        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_nacitani_atributu"))
        End Try

        'nahrani cbr
        For Each cbr As ZdrojCBR In ZdrojeCBR
            If Not cbr.LoadFromXML(Atributy, mRozsahVah, mDefaultVaha) Then
                Return Me.SetError(cbr.LastError)
            End If
        Next
        

        'nacteni kontextu
        For Each xmlElement In xmlDoc.SelectNodes("//contexts/context") 'pres vsechny kontexty
            Dim kontext As New Kontext(Environment, mDefaultVaha, Language)
            If Not kontext.LoadFromXML(xmlElement, mDefaultVaha, mAtributy, Me.mRozsahVah) Then
                Return Me.SetError(kontext.LastError)
            End If
            If Helper.ValueIsInCollection(mKontexty, kontext.Id) Then
                If AddBZ Then
                    mKontexty.Remove(kontext.Id)
                Else
                    Return Me.SetError(GetText("BazeZnalosti_Pokus_o_nacteni_druheho_kontextu") & kontext.Id)
                End If
            End If
            Me.mKontexty.Add(kontext, kontext.Id)
        Next

        'nacteni pravidel

        'nacteni apriori pravidel        
        For Each xmlElement In xmlDoc.SelectNodes("//rules/apriori_rules/apriori_rule") 'pres vsechny apriorni pravidla
            Dim pravidloApriori As New PravidloApriori(Environment, Language)
            If Not pravidloApriori.LoadFromXML(xmlElement, mDefaultVaha, Me.mPrahPlatnostiKontextu, Me.mPrahPlatnostiPredpokladu, mAtributy, mKontexty, Me.mRozsahVah) Then
                Return Me.SetError(pravidloApriori.LastError)
            End If
            If Helper.ValueIsInCollection(mPravidla, pravidloApriori.Id) Then
                If AddBZ Then
                    mPravidla.Remove(pravidloApriori.Id)
                Else
                    Return Me.SetError(GetText("BazeZnalosti_Pokus_o_nacteni_druheho_pravidla") & pravidloApriori.Id)
                End If
            End If
            Me.mPravidla.Add(pravidloApriori, pravidloApriori.Id)
        Next
        'nacteni logical pravidel
        For Each xmlElement In xmlDoc.SelectNodes("//rules/logical_rules/logical_rule") 'pres vsechny logicke pravidla
            Dim pravidloLog As New PravidloLogicke(Environment, Language)
            If Not pravidloLog.LoadFromXML(xmlElement, mDefaultVaha, Me.mPrahPlatnostiKontextu, Me.mPrahPlatnostiPredpokladu, mAtributy, mKontexty, Me.mRozsahVah) Then
                Return Me.SetError(pravidloLog.LastError)
            End If
            If Helper.ValueIsInCollection(mPravidla, pravidloLog.Id) Then
                If AddBZ Then
                    mPravidla.Remove(pravidloLog.Id)
                Else
                    Return Me.SetError(GetText("BazeZnalosti_Pokus_o_nacteni_druheho_pravidla") & pravidloLog.Id)
                End If
            End If
            Me.mPravidla.Add(pravidloLog, pravidloLog.Id)
        Next
        'nacteni kompozicionalnich pravidel
        For Each xmlElement In xmlDoc.SelectNodes("//rules/compositional_rules/compositional_rule") 'pres vsechny kompozicionalni pravidla
            Dim pravidloKomp As New PravidloKompozicionalni(Environment, Language)
            If Not pravidloKomp.LoadFromXML(xmlElement, mDefaultVaha, Me.mPrahPlatnostiKontextu, Me.mPrahPlatnostiPredpokladu, mAtributy, mKontexty, Me.mRozsahVah) Then
                Return Me.SetError(pravidloKomp.LastError)
            End If
            If Helper.ValueIsInCollection(mPravidla, pravidloKomp.Id) Then
                If AddBZ Then
                    mPravidla.Remove(pravidloKomp.Id)
                Else
                    Return Me.SetError(GetText("BazeZnalosti_Pokus_o_nacteni_druheho_pravidla") & pravidloKomp.Id)
                End If
            End If
            Me.mPravidla.Add(pravidloKomp, pravidloKomp.Id)
        Next

        'nacteni integritnich omezeni
        For Each xmlElement In xmlDoc.SelectNodes("//integrity_constraints/integrity_constraint") 'pres vsechny integritni omezeni
            Dim integritniOmezeni As New IntegritniOmezeni(Environment, Language)
            If Not integritniOmezeni.LoadFromXML(xmlElement, mDefaultVaha, Me.mPrahPlatnostiKontextu, mAtributy, mKontexty, Me.mRozsahVah) Then
                Return Me.SetError(integritniOmezeni.LastError)
            End If
            If Helper.ValueIsInCollection(mKontexty, integritniOmezeni.Id) Then
                If AddBZ Then
                    mIntegritniOmezeni.Remove(integritniOmezeni.Id)
                Else
                    Return Me.SetError(GetText("BazeZnalosti_Pokus_o_nacteni_druheho_integritniho_omezeni") & integritniOmezeni.Id)
                End If
            End If
            Me.mIntegritniOmezeni.Add(integritniOmezeni, integritniOmezeni.Id)
        Next

        'nacteni system info

        Helper.NactiXMLString(xmlDoc.DocumentElement, "system_info", mSystemInfoXML)

        If Not Me.PropojeniBaze Then
            Return False
        End If

        Return True
    End Function

    Public Function SaveBaseToXML(ByRef BazeZnalostiXML As String) As Boolean

        Dim SW As New System.IO.StringWriter
        Dim Writer As New XmlTextWriter(SW)


        'Writer.WriteStartDocument()
        Writer.WriteRaw("<?xml version=""1.0"" encoding=""windows-1250""?>" & vbCrLf)
        Writer.WriteStartElement("base")
        Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Global properties ********************-->" & vbCrLf & vbCrLf)
        'global
        Try
            Writer.WriteStartElement("global")
            Writer.WriteRaw(vbCrLf)

            Writer.WriteElementString("description", mPopis)
            Writer.WriteRaw(vbCrLf)
            Writer.WriteElementString("expert", mExpert)
            Writer.WriteRaw(vbCrLf)
            Writer.WriteElementString("knowledge_engineer", mInzenyr)
            Writer.WriteRaw(vbCrLf)
            Writer.WriteElementString("date", Format(mDatumVytvoreni, "d.M.yyyy"))
            Writer.WriteRaw(vbCrLf)
            Select Case mInferencniMechanismus
                Case Enums.enmInferencniMechanismus.Standardni
                    Writer.WriteElementString("inference_mechanism", "standard")
                Case Enums.enmInferencniMechanismus.Logicky
                    Writer.WriteElementString("inference_mechanism", "logical")
                Case Enums.enmInferencniMechanismus.Neuronovy
                    Writer.WriteElementString("inference_mechanism", "neural")
                Case Enums.enmInferencniMechanismus.Hybridni
                    Writer.WriteElementString("inference_mechanism", "hybrid")
            End Select
            Writer.WriteRaw(vbCrLf)
            Writer.WriteElementString("slope", Helper.FloatToStr(mSlope))
            Writer.WriteRaw(vbCrLf)
            Writer.WriteElementString("weight_range", CStr(mRozsahVah))
            Writer.WriteRaw(vbCrLf)
            If mDefaultVaha.JeRovno(Environment.vahaUnknown) Then
                Writer.WriteElementString("default_weight", "unknown")
            ElseIf mDefaultVaha.JeRovno(Environment.vahaIrrelevant) Then
                Writer.WriteElementString("default_weight", "irrelevant")
            End If
            Writer.WriteRaw(vbCrLf)
            Select Case mGlobalPriority
                Case Enums.enmPriorita.First
                    Writer.WriteElementString("global_priority", "first")
                Case Enums.enmPriorita.Last
                    Writer.WriteElementString("global_priority", "last")
                Case Enums.enmPriorita.MinLength
                    Writer.WriteElementString("global_priority", "minlength")
                Case Enums.enmPriorita.MaxLength
                    Writer.WriteElementString("global_priority", "maxlength")
                Case Enums.enmPriorita.User
                    Writer.WriteElementString("global_priority", "user")
            End Select
            Writer.WriteRaw(vbCrLf)
            Writer.WriteElementString("context_global_threshold", Helper.FloatToStr(mPrahPlatnostiKontextu))
            Writer.WriteRaw(vbCrLf)
            Writer.WriteElementString("condition_global_threshold", Helper.FloatToStr(mPrahPlatnostiPredpokladu))
            Writer.WriteRaw(vbCrLf)
            Writer.WriteEndElement() 'global

        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_uklani_globalnich_parametru"))
        End Try


        'ulozeni atributu
        Try
            Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Attributes and propositions ********************-->" & vbCrLf & vbCrLf)


            Writer.WriteStartElement("attributes")
            Writer.WriteRaw(vbCrLf)
            For Each atribut As Atribut In mAtributy
                If Not atribut.SaveToXML(Writer, Me.mRozsahVah) Then
                    Return Me.SetError(atribut.LastError)
                End If

            Next
            Writer.WriteEndElement()
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_ukladani_atributu"))
        End Try

        'ulozeni kontextu
        Try
            Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Contexts ********************-->" & vbCrLf & vbCrLf)

            Writer.WriteStartElement("contexts")
            For Each kontext As Kontext In mKontexty
                If Not kontext.SaveToXML(Writer) Then
                    Return Me.SetError(kontext.LastError)
                End If

            Next
            Writer.WriteEndElement()
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_ukladani_kontextu"))
        End Try


        Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Rules ********************-->" & vbCrLf & vbCrLf)
        'ulozeni pravidel
        Writer.WriteStartElement("rules")

        'apriori
        Try

            Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Apriori rules ********************-->" & vbCrLf & vbCrLf)
            Writer.WriteStartElement("apriori_rules")

            For Each pravidlo As Pravidlo In Me.mPravidla
                If pravidlo.Typ = Enums.enmTypPravidla.enmApriori Then
                    If Not pravidlo.SaveToXML(Writer, Me.mRozsahVah) Then
                        Me.SetError(pravidlo.LastError)
                    End If
                End If
            Next

            Writer.WriteEndElement()
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_ukladani_apriori_pravidel"))
        End Try


        'logicke
        Try
            Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Logical rules ********************-->" & vbCrLf & vbCrLf)

            Writer.WriteStartElement("logical_rules")

            For Each pravidlo As Pravidlo In Me.mPravidla
                If pravidlo.Typ = Enums.enmTypPravidla.enmLogicke Then
                    If Not pravidlo.SaveToXML(Writer, Me.mRozsahVah) Then
                        Me.SetError(pravidlo.LastError)
                    End If
                End If
            Next

            Writer.WriteEndElement()
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_ukladani_logickeho_pravidel"))
        End Try

        'kompozicionalni
        Try
            Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Compositional rules ********************-->" & vbCrLf & vbCrLf)

            Writer.WriteStartElement("compositional_rules")

            For Each pravidlo As Pravidlo In Me.mPravidla
                If pravidlo.Typ = Enums.enmTypPravidla.enmKompozicionalni Then
                    If Not pravidlo.SaveToXML(Writer, Me.mRozsahVah) Then
                        Me.SetError(pravidlo.LastError)
                    End If
                End If
            Next

            Writer.WriteEndElement()
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_ukladani_kompozicionalnich_pravidel"))
        End Try

        Writer.WriteEndElement()

        'ulozeni integritnich omezeni
        Try
            Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** Integrity constraints ********************-->" & vbCrLf & vbCrLf)

            Writer.WriteStartElement("integrity_constraints")
            For Each integritniOmezeni As IntegritniOmezeni In mIntegritniOmezeni
                If Not integritniOmezeni.SaveToXML(Writer, Me.mRozsahVah) Then
                    Return Me.SetError(integritniOmezeni.LastError)
                End If

            Next
            Writer.WriteEndElement()
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_ukladani_integritnich_omezeni"))
        End Try

        'ulozeni informaci o BZ
        Writer.WriteRaw(vbCrLf & vbCrLf & "<!--******************** System info ********************-->" & vbCrLf & vbCrLf)
        Writer.WriteStartElement("system_info")
        Writer.WriteRaw(Me.mSystemInfoXML)
        Writer.WriteEndElement()

        Writer.WriteEndElement()
        Writer.Flush()
        BazeZnalostiXML = SW.ToString
        Return True






    End Function

    Public Function LoadAnswersFromXML(ByVal AnswersXML As String) As Boolean
        Dim xmlDoc As New XmlDocument

        'nacteni xmldokumentu
        Try
            AnswersXML = AnswersXML.Replace("<!DOCTYPE answers SYSTEM ""answers.dtd"">", "")
            xmlDoc.LoadXml(AnswersXML)
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Nepodarilo_se_nacist_xml_dokument_odpovedi"))
        End Try

        Try
            Dim xmlElement As XmlElement
            For Each xmlElement In xmlDoc.SelectNodes("//attribute")
                Dim idAtributu As String
                If Not Helper.NactiXMLString(xmlElement, "id", idAtributu) Then
                    Return Me.SetError("Odpoved bez id")
                End If
                If Not Helper.ValueIsInCollection(Atributy, idAtributu) Then
                    Return Me.SetError("Chyba pøi naèítání odpovìdí z xml - neexistuje atribut s Id: " & idAtributu)
                End If
                Dim atribut As Atribut
                atribut = Me.mAtributy(idAtributu)
                Dim spravnyTyp As String
                Helper.NactiXMLString(xmlElement, "type", spravnyTyp)
                Select Case spravnyTyp
                    Case "binary"
                        If Not atribut.Typ = Enums.enmTypAtributu.enmBinary Then Return Me.SetError("Špatný typ atributu u odpovìdi: " & atribut.Id)
                    Case "single"
                        If Not atribut.Typ = Enums.enmTypAtributu.enmSingle Then Return Me.SetError("Špatný typ atributu u odpovìdi: " & atribut.Id)
                    Case "multiple"
                        If Not atribut.Typ = Enums.enmTypAtributu.enmMultiple Then Return Me.SetError("Špatný typ atributu u odpovìdi: " & atribut.Id)
                    Case "numeric"
                        If Not atribut.Typ = Enums.enmTypAtributu.enmNumeric Then Return Me.SetError("Špatný typ atributu u odpovìdi: " & atribut.Id)
                End Select
                If Not atribut.LoadAnswerFromXML(xmlElement, mDefaultVaha) Then
                    Return Me.SetError(atribut.LastError)
                End If
                If Not atribut.SpoctiVahu() Then
                    Return Me.SetError(atribut.LastError)
                End If
            Next
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_nacitani_odpovedi_z_xml"))
        End Try

    End Function

    Private Function SaveGoalsToXML(ByRef resultXML As String) As Boolean
        Dim xmlDoc As New XmlDocument

        Dim xmldecl As XmlDeclaration
        xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", Nothing)

        Dim results As Xml.XmlElement
        results = xmlDoc.CreateElement("results")
        xmlDoc.AppendChild(results)

        'pridani cilu do vysledku

        Dim goals As Xml.XmlElement
        goals = xmlDoc.CreateElement("goals")
        results.AppendChild(goals)

        Dim cil As Vyrok
        For Each cil In Me.mCile
            Dim pomocnyElement As Xml.XmlElement
            If Not cil.RodicovskyAtribut.SaveToXML(xmlDoc, goals, pomocnyElement) Then
                Return Me.SetError(cil.RodicovskyAtribut.LastError)
            End If
        Next

        'pridani dotazu
        If Me.mAktualniDotazy.Count > 0 Then
            Dim questions As Xml.XmlElement
            questions = xmlDoc.CreateElement("questions")
            results.AppendChild(questions)
            Dim dotaz As Atribut
            For Each dotaz In Me.mAktualniDotazy
                Dim pomocnyElement As Xml.XmlElement
                If Not dotaz.SaveToXML(xmlDoc, questions, pomocnyElement) Then
                    Return Me.SetError(dotaz.LastError)
                End If
            Next
        End If

        'pridani porusenych integritnich omezeni
        Dim jsouIO As Boolean = False
        Dim IntOm As Xml.XmlElement
        IntOm = xmlDoc.CreateElement("integrity_constraints")
        For Each integritniOmezeni As IntegritniOmezeni In Me.mIntegritniOmezeni
            If integritniOmezeni.Status = Enums.enmStav.enmError Then
                jsouIO = True
                Dim pomocnyElement As Xml.XmlElement
                If Not integritniOmezeni.SaveToXML(xmlDoc, IntOm, pomocnyElement, Me.mRozsahVah) Then
                    Return Me.SetError(integritniOmezeni.LastError)
                End If
            End If
        Next
        If jsouIO Then results.AppendChild(IntOm)

        Dim sw As New IO.StringWriter
        Dim w As New XmlTextWriter(sw)
        xmldecl.WriteTo(w)
        resultXML = sw.ToString

        resultXML += xmlDoc.OuterXml



        Return True
    End Function

    Private Function SaveAnswersToXml(ByRef resultXML As String) As Boolean
        Dim xmlDoc As New XmlDocument

        Dim xmldecl As XmlDeclaration
        xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", Nothing)

        Dim answers As Xml.XmlElement
        answers = xmlDoc.CreateElement("answers")
        xmlDoc.AppendChild(answers)

        Dim atribut As Atribut
        For Each atribut In Me.mAtributy
            If atribut.Pozice = Enums.enmPozice.enmQuestion Then
                Dim pomocnyElement As Xml.XmlElement
                If Not atribut.SaveToXML(xmlDoc, answers, pomocnyElement) Then
                    Return Me.SetError(atribut.LastError)
                End If
            End If
        Next

        Dim sw As New IO.StringWriter
        Dim w As New XmlTextWriter(sw)
        xmldecl.WriteTo(w)
        resultXML = sw.ToString

        resultXML += xmlDoc.OuterXml

        Return True
    End Function
#End Region

    Private Function PropojeniBaze() As Boolean
        Try
            Dim pravidlo As Pravidlo
            For Each pravidlo In mPravidla
                If Not pravidlo.PropojeniBaze Then
                    Return Me.SetError(pravidlo.LastError)
                End If
            Next

            'For Each integritniOmezeni As IntegritniOmezeni In mIntegritniOmezeni
            '    If Not integritniOmezeni.PropojeniBaze Then
            '        Return Me.SetError(integritniOmezeni.LastError)
            '    End If
            'Next

            Dim kontext As Kontext
            For Each kontext In mKontexty
                If Not kontext.PropojeniBaze Then
                    Return Me.SetError(kontext.LastError)
                End If
            Next

            'urceni pozice atributu a vyroku
            Dim atribut As Atribut
            For Each atribut In Me.mAtributy
                If Not atribut.UrciPozici(mAtributy) Then
                    Return Me.SetError(atribut.LastError)
                End If
            Next

            'nastaveni cilu
            mCile = New Collection
            For Each atribut In Me.mAtributy
                If atribut.Pozice = Enums.enmPozice.enmGoal Then
                    Dim vyrok As Vyrok
                    For Each vyrok In atribut.Vyroky
                        If vyrok.Pozice = Enums.enmPozice.enmGoal Then
                            Me.mCile.Add(vyrok, vyrok.Id)
                        End If
                    Next
                End If
            Next

            'kontrola cyklu
            If Not KontrolaCyklu() Then
                Return False
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_propojovani_baze"))
        End Try
    End Function

    'provede samotnou konzultaci - vse jiz musí být naèteno
    Private Function ProvedKonzultaci(ByRef ResultXML As String, ByRef pripadyVzdalenost As String) As Boolean
        Dim cil As Vyrok
        Dim neurcitost As Neurcitost
        '++++
        Select Case Me.mInferencniMechanismus
            Case Enums.enmInferencniMechanismus.Logicky
                'Return Me.SetError("Logicka neurcitost neni udelana")
                neurcitost = New NeurcitostLogical(Environment, Language)
            Case Enums.enmInferencniMechanismus.Neuronovy
                Return Me.SetError("Neuronova neurcitost neni udelana")
            Case Enums.enmInferencniMechanismus.Hybridni
                Return Me.SetError("Hybridni neurcitost neni udelana")
            Case Else
                neurcitost = New NeurcitostStandard(Environment, Language)
        End Select

        Dim pripadVzdalenost As Collection = Nothing
        For Each cil In Me.mCile
            Dim vyhodnocenyCil As Boolean

            If Not cil.VyhodnotVyrokBackward(vyhodnocenyCil, neurcitost, Me.mAktualniDotazy, Me.mAtributy, False, pripadVzdalenost) Then
                Return Me.SetError(cil.LastError)
            End If
            If Not vyhodnocenyCil Then
                Return Me.SetError("Nìkde je chyba, když cíl " & cil.Id & " není vyhodnocen")
            End If
        Next

        If Not Me.VyhodnotIntegritniOmezeni(neurcitost) Then
            Return False
        End If

        If Not Me.SaveGoalsToXML(ResultXML) Then
            Return False
        End If

        If pripadVzdalenost IsNot Nothing Then
            pripadyVzdalenost = pripadVzdalenost.Count.ToString
        End If

        Return True
    End Function

    'provede konzultaci na zaklade zaslane BazeZnalosti a Odpovedi
    Public Function RunConsultation(ByVal BazeZnalostiXML As String, ByVal AnswersXML As String, ByRef ResultXML As String, ByVal useInferenceMechanisme As Nullable(Of Enums.enmInferencniMechanismus), Optional ByVal NscXml As String = "", Optional ByRef PripadyVzdalenost As String = "") As Boolean
        Try
            'nacte bazi znalosti
            If Not LoadBaseFromXML(BazeZnalostiXML, , NscXml) Then
                Return False
            End If

            ''propojeni prvku v bazi
            'If Not Me.PropojeniBaze Then
            '    Return False
            'End If

            'nacte odpovedi do baze
            If Not LoadAnswersFromXML(AnswersXML) Then
                Return False
            End If

            If useInferenceMechanisme IsNot Nothing Then
                InferencniMechanismus = useInferenceMechanisme
            End If

            'provedeni samotne konzultace
            If Not ProvedKonzultaci(ResultXML, PripadyVzdalenost) Then
                Return False
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Neocekavana_chyba"))
        End Try
    End Function


    Public Function GetQuestionFromBaze(ByVal BazeZnalostiXML As String, ByRef AnswersXML As String, Optional ByVal NcsXml As String = "") As Boolean
        If BazeZnalostiXML <> "" Then
            'nacte bazi znalosti
            If Not LoadBaseFromXML(BazeZnalostiXML, , NcsXml) Then
                Return False
            End If

            ''propojeni prvku v bazi
            'If Not Me.PropojeniBaze Then
            '    Return False
            'End If
        End If


        If Not Me.SaveAnswersToXml(AnswersXML) Then
            Return False
        End If

        Return True
    End Function

    Public Function GetVypisDirect() As String
        Dim text As String
        GetVypis(text, Enums.enmTypVypisu.enmText)
        Return text
    End Function

    Public Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu) As Boolean
        Try
            'vypis globalnich promennych
            Helper.VypisAddLine(text, "globalni_parametry", typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, Me.GetText("GlobalniParametry"), typVypisu, Enums.enmTypRadkyVypisu.enmNadpis1)
            Helper.VypisAddLine(text, "  " & Me.GetText("Popis") & ": " + mPopis, typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("Expert") & ": " + mExpert, typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("Inzenyr") & ": " + mInzenyr, typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("Popis") & ": " + Helper.FormatDatum(mDatumVytvoreni), typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("InferencniMechanismus") & ": " + CStr(mInferencniMechanismus), typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("Slope") & ": " + CStr(mSlope), typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("RozsahVah") & ": " + CStr(mRozsahVah), typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("DefaultVaha") & ": " & Helper.FormatInterval(mDefaultVaha, mRozsahVah), typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("GlobalPriority") & ": " + CStr(mGlobalPriority), typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("PrahPlatnostiKontextu") & ": " + CStr(mPrahPlatnostiKontextu), typVypisu)
            Helper.VypisAddLine(text, "  " & Me.GetText("PrahPlatnostiPredpokladu") & ": " + CStr(mPrahPlatnostiPredpokladu), typVypisu)
            Helper.VypisAddLine(text, "", typVypisu)
            Helper.VypisAddLine(text, "--------------------------------------------", typVypisu, Enums.enmTypRadkyVypisu.enmCara)
            Helper.VypisAddLine(text, "", typVypisu)
            'vypis atributu a vyroku
            Helper.VypisAddLine(text, "atributy", typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, Me.GetText("Atributy"), typVypisu, Enums.enmTypRadkyVypisu.enmNadpis1)
            Dim atribut As Atribut
            For Each atribut In Me.mAtributy
                If Not atribut.GetVypis(text, typVypisu, mRozsahVah) Then
                    Return Me.SetError(atribut.LastError)
                End If
            Next
            Helper.VypisAddLine(text, "", typVypisu)
            Helper.VypisAddLine(text, "--------------------------------------------", typVypisu, Enums.enmTypRadkyVypisu.enmCara)
            Helper.VypisAddLine(text, "", typVypisu)


            'vypis kontexty
            Helper.VypisAddLine(text, "kontexty", typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, Me.GetText("Kontexty"), typVypisu, Enums.enmTypRadkyVypisu.enmNadpis1)
            Dim kontext As Kontext
            For Each kontext In Me.mKontexty
                If Not kontext.GetVypis(text, typVypisu, mRozsahVah) Then
                    Return Me.SetError(kontext.LastError)
                End If
            Next


            Helper.VypisAddLine(text, "", typVypisu)
            Helper.VypisAddLine(text, "--------------------------------------------", typVypisu, Enums.enmTypRadkyVypisu.enmCara)
            Helper.VypisAddLine(text, "", typVypisu)


            'vypis pravidel
            Helper.VypisAddLine(text, "pravidla", typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, Me.GetText("Pravidla"), typVypisu, Enums.enmTypRadkyVypisu.enmNadpis1)
            Dim pravidlo As Pravidlo
            For Each pravidlo In Me.mPravidla
                If Not pravidlo.GetVypis(text, typVypisu, mRozsahVah) Then
                    Return Me.SetError(pravidlo.LastError)
                End If
            Next

            Helper.VypisAddLine(text, "", typVypisu)
            Helper.VypisAddLine(text, "--------------------------------------------", typVypisu, Enums.enmTypRadkyVypisu.enmCara)
            Helper.VypisAddLine(text, "", typVypisu)


            'vypis integritni omezeni
            Helper.VypisAddLine(text, "integritni_omezeni", typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, Me.GetText("IntegritniOmezeni"), typVypisu, Enums.enmTypRadkyVypisu.enmNadpis1)
            '++++

            Return True
        Catch ex As Exception
            Me.SetError(ex, , "Chyba pøi sestavování výpisu báze")
        End Try
    End Function

    Public Function GetAtributForIdVyroku(ByVal idVyroku As String, ByRef atribut As Atribut) As Boolean
        For Each atribut2 As Atribut In mAtributy
            For Each vyrok As Vyrok In atribut2.Vyroky
                If vyrok.Id = idVyroku Then
                    atribut = atribut2
                    Return True
                End If
            Next
        Next
        Return False
    End Function

    Private Function VyhodnotIntegritniOmezeni(ByVal neurcitost As Neurcitost) As Boolean
        For Each integritniOmezeni As IntegritniOmezeni In mIntegritniOmezeni
            If Not integritniOmezeni.Vyhodnot(neurcitost, True) Then
                Return Me.SetError(integritniOmezeni.LastError)
            End If
        Next
        Return True
    End Function

    Public Function CheckBase(ByVal BazeZnalostiXML As String) As Boolean
        'nacte bazi znalosti
        If Not LoadBaseFromXML(BazeZnalostiXML) Then
            Return False
        End If

        ''propojeni prvku v bazi
        'If Not Me.PropojeniBaze Then
        '    Return False
        'End If

        '++++

        Return True
    End Function

    Private Function KontrolaCyklu() As Boolean
        For Each atribut As Atribut In mAtributy
            For Each vyrok As Vyrok In atribut.Vyroky
                Dim pouziteVyroky As New Collection
                Dim pouzitapravidla As New Collection
                If Not ZkontrolujCyklusVyroku(vyrok, pouziteVyroky, pouzitapravidla) Then
                    Return False
                End If
            Next
        Next
        Return True
    End Function
    Private Function ZkontrolujCyklusVyroku(ByVal vyrok As Vyrok, ByVal pouziteVyroky As Collection, ByVal pouzitaPravidla As Collection) As Boolean
        If Helper.ValueIsInCollection(pouziteVyroky, vyrok.RodicovskyAtribut.Id & "(" & vyrok.Id & ")") Then
            Dim prav As String = ""
            For Each pr As Pravidlo In pouzitaPravidla
                prav += pr.Id & ", "
            Next
            Return Me.SetError(GetText("BazeZnaloti_Baze_znalosti_obsahuje_cyklus") & vyrok.RodicovskyAtribut.Id & "(" & vyrok.Id & ") : pravidla - " & prav)
        End If
        For Each pravidlo As Pravidlo In vyrok.SeznamPredpokladu
            Dim newPouzitaPravidla As New Collection
            For Each pr As Pravidlo In pouzitaPravidla
                newPouzitaPravidla.Add(pr, pr.Id)
            Next
            For Each zaver As Zaver In pravidlo.Zavery
                Dim newPouziteVyroky As New Collection
                For Each v As Vyrok In pouziteVyroky
                    newPouziteVyroky.Add(v, v.RodicovskyAtribut.Id & "(" & v.Id & ")")
                Next
                newPouziteVyroky.Add(vyrok, vyrok.Id)
                If Not ZkontrolujCyklusVyroku(zaver.Literal.Vyrok, newPouziteVyroky, newPouzitaPravidla) Then
                    Return False
                End If
            Next
        Next

        For Each kontex As Kontext In vyrok.SeznamKontextu

            For Each pravidlo As Pravidlo In kontex.SeznamPravidel
                Dim newPouzitaPravidla As New Collection
                For Each pr As Pravidlo In pouzitaPravidla
                    newPouzitaPravidla.Add(pr, pr.Id)
                Next
                For Each zaver As Zaver In pravidlo.Zavery
                    Dim newPouziteVyroky As New Collection
                    For Each v As Vyrok In pouziteVyroky
                        newPouziteVyroky.Add(v, v.RodicovskyAtribut.Id & "(" & v.Id & ")")
                    Next
                    newPouziteVyroky.Add(vyrok, vyrok.Id)
                    If Not ZkontrolujCyklusVyroku(zaver.Literal.Vyrok, newPouziteVyroky, newPouzitaPravidla) Then
                        Return False
                    End If
                Next
            Next
        Next
        Return True
    End Function


    Private Function PridejRadekStatistiky(ByVal nadpis As String, ByVal hodnota As String) As String
        Dim result As String = ""
        result += "<tr><td class=""nadpis_polozky"">" & nadpis & "</td><td class=""data"">" & hodnota & "</td></tr>"
        Return result
    End Function
    Public Function GetStatistiky(ByRef result As String) As Boolean
        Try
            result = ""

            result += "<table class=""table_statistiky""><tr>"
            result += "<td class=""levy_sloupec"">"

            result += "<table class=""table_globalni"">"
            result += "<tr><td colspan=""2"" class=""nadpis_sekce"">" & GetText("GlobalniParametry") & "</td></tr>"
            result += PridejRadekStatistiky(GetText("Popis"), Me.mPopis)
            result += PridejRadekStatistiky(GetText("Expert"), Me.mExpert)
            result += PridejRadekStatistiky(GetText("Inzenyr"), Me.mInzenyr)
            result += PridejRadekStatistiky(GetText("Datum"), Format(Me.mDatumVytvoreni, "d.M.yyyy"))
            result += PridejRadekStatistiky(GetText("RozsahVah"), Me.mRozsahVah)
            result += PridejRadekStatistiky(GetText("PrahPlatnostiPredpokladu"), Me.mPrahPlatnostiKontextu)
            result += PridejRadekStatistiky(GetText("PrahPlatnostiKontextu"), Me.mPrahPlatnostiPredpokladu)
            result += PridejRadekStatistiky(GetText("InferencniMechanismus"), Enums.TxtEnmInferencniMechanismus(Me.mInferencniMechanismus, Me.GetResourceManager))
            result += PridejRadekStatistiky(GetText("DefaultVaha"), Me.mDefaultVaha.ToStr(, True))
            result += PridejRadekStatistiky(GetText("Priorita"), Enums.TxtEnmPriorita(Me.mGlobalPriority, Me.GetResourceManager))
            result += "</table>"

            result += "</td><td class=""pravy_sloupec"">"
            result += "<table class=""table_objekty""><tr>"
            result += "<td class=""horni_bunka"">"
            result += "<table class=""table_atributy"">"
            result += "<tr><td colspan=""2"" class=""nadpis_sekce"">" & GetText("Atributy") & "</td></tr>"
            result += PridejRadekStatistiky(GetText("Celkem"), GetPocetAtributu())
            result += PridejRadekStatistiky(GetText("Enums_binary"), GetPocetAtributu(Enums.enmTypAtributu.enmBinary))
            result += PridejRadekStatistiky(GetText("Enums_single"), GetPocetAtributu(Enums.enmTypAtributu.enmSingle))
            result += PridejRadekStatistiky(GetText("Enums_multiple"), GetPocetAtributu(Enums.enmTypAtributu.enmMultiple))
            result += PridejRadekStatistiky(GetText("Enums_numeric"), GetPocetAtributu(Enums.enmTypAtributu.enmNumeric))
            result += PridejRadekStatistiky(GetText("Dotaz"), GetPocetAtributu(, Enums.enmPozice.enmQuestion))
            result += PridejRadekStatistiky(GetText("Mezilehly"), GetPocetAtributu(, Enums.enmPozice.enmIntermediate))
            result += PridejRadekStatistiky(GetText("Cil"), GetPocetAtributu(, Enums.enmPozice.enmGoal))
            result += PridejRadekStatistiky(GetText("Osamoceny"), GetPocetAtributu(, Enums.enmPozice.enmAlone))
            result += PridejRadekStatistiky(GetText("Zdroje"), GetPocetZdrojuAtributu)
            'result += PridejRadekStatistiky("Akce", "-")

            result += "</table>"
            result += "</td><td class=""horni_bunka"">"
            result += "<table class=""table_vyroky"">"
            result += "<tr><td colspan=""2"" class=""nadpis_sekce"">" & GetText("Vyroky") & "</td></tr>"
            result += PridejRadekStatistiky(GetText("Celkem"), GetPocetVyroku())
            result += PridejRadekStatistiky(GetText("Enums_binary"), GetPocetVyroku(Enums.enmTypAtributu.enmBinary))
            result += PridejRadekStatistiky(GetText("Enums_single"), GetPocetVyroku(Enums.enmTypAtributu.enmSingle))
            result += PridejRadekStatistiky(GetText("Enums_multiple"), GetPocetVyroku(Enums.enmTypAtributu.enmMultiple))
            result += PridejRadekStatistiky(GetText("Enums_numeric"), GetPocetVyroku(Enums.enmTypAtributu.enmNumeric))
            result += PridejRadekStatistiky(GetText("Dotaz"), GetPocetVyroku(, Enums.enmPozice.enmQuestion))
            result += PridejRadekStatistiky(GetText("Mezilehly"), GetPocetVyroku(, Enums.enmPozice.enmIntermediate))
            result += PridejRadekStatistiky(GetText("Cil"), GetPocetVyroku(, Enums.enmPozice.enmGoal))
            result += PridejRadekStatistiky(GetText("Osamoceny"), GetPocetVyroku(, Enums.enmPozice.enmAlone))



            result += "</table>"
            result += "</td>"
            result += "</tr><tr>"
            result += "<td>"

            result += "<table class=""table_pravidla"">"
            result += "<tr><td colspan=""2"" class=""nadpis_sekce"">" & GetText("Pravidla") & "</td></tr>"
            result += PridejRadekStatistiky(GetText("Celkem"), GetPocetPravidel())
            result += PridejRadekStatistiky(GetText("Enums_apriori"), GetPocetPravidel(Enums.enmTypPravidla.enmApriori))
            result += PridejRadekStatistiky(GetText("Enums_logical"), GetPocetPravidel(Enums.enmTypPravidla.enmLogicke))
            result += PridejRadekStatistiky(GetText("Enums_kompozicionalni"), GetPocetPravidel(Enums.enmTypPravidla.enmKompozicionalni))
            'result += PridejRadekStatistiky("Akce", "-")
            
            result += "</table>"
            result += "</td><td>"
            result += "<table class=""table_dalsi"">"
            result += "<tr><td colspan=""2"" class=""nadpis_sekce"">" & GetText("Dalsi") & "</td></tr>"
            result += PridejRadekStatistiky(GetText("Kontexty"), mKontexty.Count)
            result += PridejRadekStatistiky(GetText("IntegritniOmezeni"), mIntegritniOmezeni.Count)
            


            result += "</table>"

            result += "</td>"
            result += "</tr></table>"
            result += "</td>"
            result += "</tr></table>"

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("BazeZnalosti_Chyba_pri_generovani_statistik"))
        End Try
    End Function

    Private Function GetPocetAtributu(Optional ByVal typAtributu As Enums.enmTypAtributu = 0, Optional ByVal pozice As Enums.enmPozice = 0) As Long
        Dim pocet As Long = 0
        For Each atribut As Atribut In mAtributy
            If typAtributu = 0 Or atribut.Typ = typAtributu Then
                If pozice = 0 Or atribut.Pozice = pozice Then
                    pocet += 1
                End If
            End If
        Next
        Return pocet
    End Function

    Private Function GetPocetVyroku(Optional ByVal typAtributu As Enums.enmTypAtributu = 0, Optional ByVal pozice As Enums.enmPozice = 0) As Long
        Dim pocet As Long = 0
        For Each atribut As Atribut In mAtributy
            If typAtributu = 0 Or atribut.Typ = typAtributu Then
                If pozice = 0 Or atribut.Pozice = pozice Then
                    pocet += atribut.Vyroky.Count
                End If
            End If
        Next
        Return pocet
    End Function

    Private Function GetPocetPravidel(Optional ByVal typPravidla As Enums.enmTypPravidla = 0) As Long
        Dim pocet As Long = 0
        For Each pravidlo As Pravidlo In mPravidla
            If typPravidla = 0 Or pravidlo.Typ = typPravidla Then
                    pocet += 1
            End If
        Next
        Return pocet
    End Function

    Private Function GetPocetZdrojuAtributu() As Long
        Dim pocet As Long = 0
        For Each atribut As Atribut In mAtributy

            pocet += atribut.GetPocetZdroju

        Next
        Return pocet
    End Function

    Public Function PridejAtributBinary(ByVal id As String, Optional ByVal jmeno As String = "", Optional ByVal komentar As String = "") As Boolean
        Try
            Dim atr As New AtributBinary(id, Environment, DefaultVaha, Language)
            atr.Jmeno = jmeno
            atr.Komentar = komentar

            Dim vyr As New Vyrok(Environment, DefaultVaha, atr, id, Language)
            vyr.Jmeno = jmeno
            vyr.Komentar = komentar

            atr.Vyroky.Add(vyr, vyr.Id)

            Atributy.Add(atr, atr.Id)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, "Chyba pri pridavani atributu binary")
        End Try
    End Function

    Public Function PridejAtributMultiple(ByVal id As String, ByVal hodnoty() As String, ByVal jmenaVyroku() As String, ByVal komentareVyroku() As String, Optional ByVal jmeno As String = "", Optional ByVal komentar As String = "") As Boolean
        Try
            Dim atr As New AtributMultiple(id, Environment, DefaultVaha, Language)
            atr.Jmeno = jmeno
            atr.Komentar = komentar

            For i As Integer = 0 To hodnoty.Length - 1

                Dim vyr As New Vyrok(Environment, DefaultVaha, atr, hodnoty(i), Language)
                vyr.Jmeno = jmenaVyroku(i)
                vyr.Komentar = komentareVyroku(i)
                atr.Vyroky.Add(vyr, vyr.Id)
            Next
            Atributy.Add(atr, atr.Id)
            Return True

        Catch ex As Exception
            Return Me.SetError(ex, "Chyba pri pridavani atributu multiple")
        End Try
    End Function

    Public Function PridejAtributNumeric(ByVal id As String, ByVal dolniMez As Double, ByVal horniMez As Double, ByVal idVyroku() As String, ByVal jmenaVyroku() As String, ByVal komentareVyroku() As String, ByVal fuzzyLower() As Double, ByVal crispLower() As Double, ByVal crispUpper() As Double, ByVal fuzzyUpper() As Double, Optional ByVal jmeno As String = "", Optional ByVal komentar As String = "") As Boolean
        Try
            Dim atr As New AtributNumeric(id, Environment, DefaultVaha, Language, dolniMez, horniMez)
            atr.Jmeno = jmeno
            atr.Komentar = komentar

            For i As Integer = 0 To idVyroku.Length - 1
                Dim vyr As New VyrokNumeric(Environment, DefaultVaha, atr, idVyroku(i), Language, fuzzyLower(i), crispLower(i), crispUpper(i), fuzzyUpper(i))
                vyr.Jmeno = jmenaVyroku(i)
                vyr.Komentar = komentareVyroku(i)
                atr.Vyroky.Add(vyr, vyr.Id)
            Next
            Atributy.Add(atr, atr.Id)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, "Chyba pri pridavani atributu numeric")
        End Try
    End Function

    Public Function PridejKontextZeStringu(ByVal kontextStr As String, ByVal id As String) As Boolean
        Try
            Dim kontext As New Kontext(id, Environment, DefaultVaha, Language)
            If Not kontext.PridejKontextZeStringu(kontextStr, DefaultVaha, Me.mAtributy) Then
                Return Me.SetError(kontext.LastError)
            End If
            Me.mKontexty.Add(kontext, kontext.Id)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, "Kontext se nepodarilo pridat")
        End Try
    End Function

    Public Function PridejPravidloKompozicionalZeStringu(ByVal pravidloStr As String, ByVal id As String) As Boolean
        Try
            Dim pravidlo As PravidloKompozicionalni
            pravidlo = New PravidloKompozicionalni(id, Environment, Language)


            If Not pravidlo.PridejPravidloZeStringu(pravidloStr, DefaultVaha, Me.mAtributy, Me.mKontexty) Then
                pravidlo = Nothing
                Return Me.SetError(pravidlo.LastError)
            End If

            Me.mPravidla.Add(pravidlo, pravidlo.Id)




            Return True
        Catch ex As Exception
            Return Me.SetError(ex, "Pravidlo se nepodarilo pridat")
        End Try
    End Function

    Public Function PridejPravidloLogicalZeStringu(ByVal pravidloStr As String, ByVal id As String, Optional ByVal ConditionThreshold As Double = -1) As Boolean
        Try
            Dim pravidlo As PravidloLogicke
            pravidlo = New PravidloLogicke(id, Environment, Language)


            If Not pravidlo.PridejPravidloZeStringu(pravidloStr, DefaultVaha, Me.mAtributy, Me.mKontexty, ConditionThreshold) Then
                pravidlo = Nothing
                Return Me.SetError(pravidlo.LastError)
            End If

            Me.mPravidla.Add(pravidlo, pravidlo.Id)




            Return True
        Catch ex As Exception
            Return Me.SetError(ex, "Pravidlo se nepodarilo pridat")
        End Try
    End Function

    Public Function PridejPravidloAprioriZeStringu(ByVal pravidloStr As String, ByVal id As String) As Boolean
        Try
            Dim pravidlo As PravidloApriori
            pravidlo = New PravidloApriori(id, Environment, Language)


            If Not pravidlo.PridejPravidloZeStringu(pravidloStr, DefaultVaha, Me.mAtributy) Then
                pravidlo = Nothing
                Return Me.SetError(pravidlo.LastError)
            End If

            Me.mPravidla.Add(pravidlo, pravidlo.Id)




            Return True
        Catch ex As Exception
            Return Me.SetError(ex, "Pravidlo se nepodarilo pridat")
        End Try
    End Function
End Class
