Public Class Kontext
    Inherits GeneralObject

    Private mID As String
    Private mKomentar As String
    Private mStatus As Enums.enmStav
    Private mVaha As Interval
    Private mPredpoklad As Predpoklad

    Private mSeznamPravidel As Collection 'seznam pravidel ve kterych je tento kontext
#Region "Property"
    Public ReadOnly Property Id() As String
        Get
            Return mID
        End Get
    End Property
    Public ReadOnly Property SeznamPravidel() As Collection
        Get
            Return mSeznamPravidel
        End Get
    End Property
    Public ReadOnly Property Vaha() As Interval
        Get
            Return mVaha
        End Get
    End Property
    Public ReadOnly Property Status() As Enums.enmStav
        Get
            Return mStatus
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mVaha = New Interval(defaultVaha)
        mPredpoklad = Nothing
        mStatus = Enums.enmStav.enmUntouched
        mSeznamPravidel = New Collection
    End Sub
    Public Sub New(ByVal id As String, ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyClass.New(iEnvironment, defaultVaha, iLanguage)
        mID = id
    End Sub

    Public Function Clear(ByVal defaultVaha As Interval) As Boolean
        mVaha.SetValue(defaultVaha)
        mStatus = Enums.enmStav.enmUntouched
        
        Return True
    End Function

#Region "Xml"
    Public Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval, ByVal Atributy As Collection, ByVal rozsahVah As Long) As Boolean
        Try
            If Not Helper.NactiXMLString(xmlElement, "id", mID) Then
                Return Me.SetError(GetText("Kontext_Chyba_pri_nacitani_id_kontextu_z_xml"))
            End If
            Helper.NactiXMLString(xmlElement, "comment", mKomentar)

            'nacteni predpokladu
            Dim predpokladElement As Xml.XmlElement
            predpokladElement = xmlElement.SelectSingleNode("condition")
            mPredpoklad = New Predpoklad(Environment, defaultVaha, Language)
            If Not mPredpoklad.LoadFromXML(predpokladElement, defaultVaha, Atributy, rozsahVah) Then
                Return Me.AppendLastError(mPredpoklad.LastError, GetText("Kontext_kontext") & Me.mID)
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Kontext_Chyba_pri_nacitani_kontextu_z_XML"))
        End Try
    End Function

    Public Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try

            writer.WriteStartElement("context")

            writer.WriteElementString("id", mID)

            If Not Me.mPredpoklad.SaveToXML(writer) Then
                Return Me.SetError(mPredpoklad.LastError)
            End If


            writer.WriteEndElement()
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Kontext_Chyba_pri_SaveToXML_v_kontextu") & Me.mID)
        End Try
    End Function
#End Region

    Public Function PropojeniBaze() As Boolean
        Try

            If Not mPredpoklad.PropojeniKontextu(Me) Then
                Return Me.SetError(mPredpoklad.LastError)
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Kontext_Chyba_pri_propojovani_kontextu") & Me.mID)
        End Try
    End Function

    Public Overridable Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Try
            Helper.VypisAddLine(text, "  Kontext: ", typVypisu, Enums.enmTypRadkyVypisu.enmNadpis2)
            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniStart)
            Helper.VypisAddLine(text, "     id: " & mID, typVypisu)
            Helper.VypisAddLine(text, "     komentáø: " & mKomentar, typVypisu)

            If Not Me.mPredpoklad.GetVypis(text, typVypisu, rozsahVah) Then
                Return Me.SetError(mPredpoklad.LastError)
            End If
            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniEnd)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Kontext_Chyba_pri_tvorbe_vypisu_kontextu") & mID)
        End Try
    End Function

    Public Function VyhodnotBackward(ByRef uspesneVyhodnoceno As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
        Try
            If mStatus = Enums.enmStav.enmFinal Or mStatus = Enums.enmStav.enmProvisional Then
                uspesneVyhodnoceno = True
                Return True
            End If

            Dim vyhodnocenyPredpoklad As Boolean
            If Not Me.mPredpoklad.VyhodnotBackward(vyhodnocenyPredpoklad, neurcitost, aktSeznamDotazu, atributy, bezdotazu) Then
                Return Me.SetError(mPredpoklad.LastError)
            End If

            'neuspesne vyhodnoceny predpoklad
            If Not vyhodnocenyPredpoklad Then
                uspesneVyhodnoceno = False
                Return True
            End If

            'uspesne vyhodnoceny predpoklad
            mStatus = mPredpoklad.Status
            Me.mVaha.SetValue(mPredpoklad.Vaha)

            uspesneVyhodnoceno = True
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Kontext_Chyba_pri_vyhodnocovani_kontextu_backward"))
        End Try
    End Function

    Public Function PridejKontextZeStringu(ByVal KontextStr As String, ByVal DefaultVaha As Interval, ByVal atributy As Collection) As Boolean
        KontextStr = LCase(KontextStr)
        mPredpoklad = New Predpoklad(Environment, DefaultVaha, Language)

        If Not mPredpoklad.PridejPredpokladZeStringu(KontextStr, DefaultVaha, atributy) Then
            Return Me.SetError(mPredpoklad.LastError)
        End If
        Return True
    End Function
End Class
