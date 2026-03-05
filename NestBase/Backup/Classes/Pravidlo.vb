Public MustInherit Class Pravidlo
    Inherits GeneralObject

    Protected mId As String
    Protected mTyp As Enums.enmTypPravidla
    Protected mStatus As Enums.enmStav
    Protected mKomentar As String
    Protected mPriorita As Single

    Protected mZavery As Collection
    'Protected mAkce as Collection 

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
    Public ReadOnly Property Typ() As Enums.enmTypPravidla
        Get
            Return mTyp
        End Get
    End Property
    Public ReadOnly Property Zavery() As Collection
        Get
            Return mZavery
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mPriorita = -1
        mZavery = New Collection
        mStatus = Enums.enmStav.enmUntouched
    End Sub

    Public Overridable Function Clear(ByVal defaultVaha As Interval) As Boolean
        mStatus = Enums.enmStav.enmUntouched
        Return True
    End Function

#Region "Xml"
    Public Overridable Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval, ByVal prahPlatnostiKontextu As Double, ByVal prahPlatnostiPredpokladu As Double, ByVal Atributy As Collection, ByVal Kontexty As Collection, ByVal rozsahVah As Long) As Boolean
        Try
            If Not Helper.NactiXMLString(xmlElement, "id", mId) Then
                Return Me.SetError(GetText("Pravidlo_Chyba_pri_nacitani_id_pravidla_z_xml"))
            End If
            Helper.NactiXMLSingle(xmlElement, "priority", mPriorita, -1)

            'nacteni zaverù
            Dim zaverElement As Xml.XmlElement
            For Each zaverElement In xmlElement.SelectNodes("conclusions/conclusion")
                Dim zaver As Zaver
                zaver = New Zaver(Environment, defaultVaha, Language)
                If Not zaver.LoadFromXML(zaverElement, defaultVaha, Atributy, rozsahVah) Then
                    Return Me.AppendLastError(zaver.LastError, GetText("Pravidlo_pravidlo") & mId)
                End If
                mZavery.Add(zaver)
            Next

            If Me.mZavery.Count = 0 Then
                Return Me.SetError(GetText("Pravidlo_Prazdny_zaver_pravidla") & mId)
            End If

            'nacteni akci
            '++++
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Pravidlo_Chyba_pri_nacitani_pravidla_z_XML"))
        End Try


    End Function

    Public MustOverride Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean

#End Region

    Public Overridable Function PropojeniBaze() As Boolean
        Try
            Dim zaver As Zaver
            For Each zaver In mZavery
                zaver.Literal.Vyrok.SeznamZaveru.Add(Me)
            Next
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Pravidlo_Chyba_pri_propojovani_pravidla") & Me.mId)
        End Try
    End Function

    Public Overridable Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Try
            'Helper.VypisAddLine(text, "  Pravidlo: ", typVypisu, Enums.enmTypRadkyVypisu.enmNadpis2)
            Helper.VypisAddLine(text, "     " & Me.GetText("id") & ": " & mId, typVypisu)
            'Helper.VypisAddLine(text, "     Typ: " & Enums.TxtEnmTypPravidla(mTyp), typVypisu)
            If mKomentar <> "" Then Helper.VypisAddLine(text, "     " & Me.GetText("Komentar") & ": " & mKomentar, typVypisu)
            Helper.VypisAddLine(text, "     " & Me.GetText("Priorita") & ": " & CStr(mPriorita), typVypisu)

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Pravidlo_Chyba_pri_tvorbe_vypisu_pravidla") & mId)
        End Try
    End Function

    Public MustOverride Function AplikujBackward(ByRef uspesneAplikovano As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean


    Public Function GetVahuZaveru(ByVal idVyroku As String, ByRef vaha As Interval) As Boolean
        Try
            Dim zaver As Zaver
            For Each zaver In Me.mZavery
                If zaver.Literal.Vyrok.Id = idVyroku Then
                    vaha = zaver.Ctr
                    Return True
                End If
            Next
            Return Me.SetError(GetText("Pravidlo_Chyba_pri_hledani_zaveru_pro_vyrok") & idVyroku & GetText("Pravidlo_u_pravidla") & Me.mId)
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Pravidlo_Chyba_pri_hledani_zaveru_pro_vyrok") & idVyroku & GetText("Pravidlo_u_pravidla") & Me.mId)
        End Try
    End Function
End Class
