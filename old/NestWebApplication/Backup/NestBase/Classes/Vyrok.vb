Public Class Vyrok
    Inherits GeneralObject

    Private mId As String
    Protected mTyp As Enums.enmTypVyroku
    Private mJmeno As String
    Private mKomentar As String
    Private mRodicovskyAtribut As Atribut
    Private mStatus As Enums.enmStav
    Private mVaha As Interval
    'Private mPosVaha as
    'Private mNegVaha as
    Private mPozice As Enums.enmPozice
    Private mZdroje As Collection

    Private mSeznamPredpokladu As Collection 'seznam pravidel, v jejiz predpokladu je tento vyrok
    Private mSeznamZaveru As Collection ' seznam pravidel, v jejiz zaveru je tento vyrok
    Private mSeznamKontextu As Collection 'seznam kontextu, ve kterych je tento vyrok

#Region "Property"
    Public ReadOnly Property Id() As String
        Get
            Return mId
        End Get
    End Property
    Public Property Jmeno() As String
        Get
            If mJmeno = "" Then Return mId
            Return mJmeno
        End Get
        Set(ByVal Value As String)
            mJmeno = Value
        End Set
    End Property
    Public Property Komentar() As String
        Get
            Return mKomentar
        End Get
        Set(ByVal Value As String)
            mKomentar = Value
        End Set
    End Property
    Public ReadOnly Property RodicovskyAtribut() As Atribut
        Get
            Return mRodicovskyAtribut
        End Get
    End Property
    Public ReadOnly Property Vaha() As Interval
        Get
            Return mVaha
        End Get
    End Property
    Public Property Status() As Enums.enmStav
        Get
            Return mStatus
        End Get
        Set(ByVal Value As Enums.enmStav)
            mStatus = Value
        End Set
    End Property
    Public ReadOnly Property SeznamPredpokladu() As Collection
        Get
            Return mSeznamPredpokladu
        End Get
    End Property
    Public ReadOnly Property SeznamZaveru() As Collection
        Get
            Return mSeznamZaveru
        End Get
    End Property
    Public ReadOnly Property SeznamKontextu() As Collection
        Get
            Return mSeznamKontextu
        End Get
    End Property
    Public ReadOnly Property Pozice() As Enums.enmPozice
        Get
            Return mPozice
        End Get
    End Property
    Public ReadOnly Property Zdroje() As Collection
        Get
            Return mZdroje
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iRodicovskyAtribut As Atribut, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mVaha = New Interval(defaultVaha)
        mRodicovskyAtribut = iRodicovskyAtribut
        mStatus = Enums.enmStav.enmUntouched
        mSeznamPredpokladu = New Collection
        mSeznamZaveru = New Collection
        mSeznamKontextu = New Collection
        mPozice = Enums.enmPozice.enmAlone
        mZdroje = New Collection
        Me.VytvorDefaultniZdroje()
    End Sub
    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iRodicovskyAtribut As Atribut, ByVal iId As String, ByVal iLanguage As Enums.enmLanguage)
        Me.New(iEnvironment, defaultVaha, iRodicovskyAtribut, iLanguage)
        mId = iId
    End Sub

    Public Function Clear(ByVal defaultVaha As Interval) As Boolean
        mStatus = Enums.enmStav.enmUntouched
        mVaha.SetValue(defaultVaha)
        Return True
    End Function

#Region "Xml"
    Public Overridable Function LoadFromXML(ByVal xmlElement As Xml.XmlElement) As Boolean
        Try
            If Not Helper.NactiXMLString(xmlElement, "id", mId) Then
                Return Me.SetError(GetText("Vyrok_Chyba_pri_nacitani_id_vyroku_z_xml"))
            End If
            Helper.NactiXMLString(xmlElement, "name", mJmeno, mId)

            '++++ nacteni akci

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Vyrok_Chyba_pri_nacitani_vyroku_z_XML"))
        End Try
    End Function

    Public Overridable Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try
            writer.WriteStartElement("proposition")

            writer.WriteElementString("id", mId)
            If mJmeno <> "No name" Then writer.WriteElementString("name", Me.mJmeno)
            writer.WriteElementString("comment", Me.mKomentar)

            writer.WriteEndElement()
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Vyrok_Chyba_pri_SaveToXML_ve_vyroku") & Me.mId)
        End Try
    End Function
#End Region

    Public Function UrciPozici() As Boolean
        Try
            If Me.mSeznamZaveru.Count > 0 Then
                If Me.mSeznamPredpokladu.Count > 0 Or Me.mSeznamKontextu.Count > 0 Then
                    Me.mPozice = Enums.enmPozice.enmIntermediate
                Else
                    Me.mPozice = Enums.enmPozice.enmGoal
                End If
            Else
                If Me.mSeznamPredpokladu.Count > 0 Or Me.mSeznamKontextu.Count > 0 Then
                    Me.mPozice = Enums.enmPozice.enmQuestion
                Else
                    Me.mPozice = Enums.enmPozice.enmAlone
                End If
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Vyrok_Chyba_pri_urcovani_pozice_vyroku") & Me.mId)
        End Try
    End Function

    Private Function VytvorDefaultniZdroje() As Boolean
        Try
            Dim zdroj As Zdroj
            zdroj = New ZdrojOdvozovani(Environment, Nothing, Nothing, Language)
            Me.mZdroje.Add(zdroj)
            zdroj = New ZdrojDotaz(Environment, Nothing, Nothing, Language)
            Me.mZdroje.Add(zdroj)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Vyrok_Chyba_pri_vytvareni_defaultnich_zdroju_vyroku") & Me.mId)
        End Try
    End Function

    Public Function VyhodnotVyrokBackward(ByRef uspesneVyhodnocen As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
        Try
            Dim zdroj As Zdroj
            For Each zdroj In Me.mZdroje
                Dim vyhodnoceniZdroje As Boolean
                Dim dalsiZdroj As Boolean = True
                If Not zdroj.VyhodnotZdroj(vyhodnoceniZdroje, Me, neurcitost, aktSeznamDotazu, atributy, bezdotazu, dalsiZdroj) Then
                    Return Me.SetError(zdroj.LastError)
                End If
                If vyhodnoceniZdroje Then
                    uspesneVyhodnocen = True
                    Return True
                ElseIf Not dalsiZdroj Then
                    uspesneVyhodnocen = False
                    Return True
                End If
            Next
            Return Me.SetError(GetText("Vyrok_Zadny_zdroj_nebyl_uspesne_vyhodnocen"))
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Vyrok_Chyba_pri_vyhodnocovani_vyroku_backward") & Me.mId)
        End Try
    End Function
#Region "vypis"
    Public Overridable Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Try
            Helper.VypisAddLine(text, mId, typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, "     " & Me.GetText("Vyrok") & "", typVypisu, Enums.enmTypRadkyVypisu.enmNadpis3)
            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniStart)
            Helper.VypisAddLine(text, "     " & Me.GetText("id") & ": " & mId, typVypisu)
            Helper.VypisAddLine(text, "     " & Me.GetText("Jmeno") & ": " & mJmeno, typVypisu)
            'Helper.VypisAddLine(text, "     Vaha: " & Helper.FormatInterval(Me.mVaha, rozsahVah), typVypisu)
            'Helper.VypisAddLine(text, "     Status: " & Enums.TxtEnmStav(Me.Status), typVypisu)
            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniEnd)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Vyrok_Chyba_pri_tvorbe_vypisu_vyroku") & mId)
        End Try
    End Function
#End Region

    Public Function OdvodVyrokBackward(ByRef uspesneOdvozen As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
        Try
            uspesneOdvozen = False

            If mStatus = Enums.enmStav.enmFinal Or mStatus = Enums.enmStav.enmProvisional Then
                uspesneOdvozen = True
                Return True
            End If

            'je-li vyrok dotaz ci osamoceny, nemuze byt odvozen
            If mPozice = Enums.enmPozice.enmQuestion Or mPozice = Enums.enmPozice.enmAlone Then
                Return True
            End If

            Dim pravidlo As Pravidlo
            Dim vahyZaveruPravidelProGlob As New Collection
            Dim pocetPravidelVeStavuFinal As Long = 0
            Dim pocetPravidelVeStavuProvisional As Long = 0
            Dim normovat As Boolean = True 'pri logickych pravidlech se do teto promene dale nastavy, aby se nenormovalo
            For Each pravidlo In Me.mSeznamZaveru
                If Not (Me.mStatus = Enums.enmStav.enmLogical And Not pravidlo.Typ = Enums.enmTypPravidla.enmLogicke) Then 'bylo-li jiz nalezeno logicke pravidlo drive a nedalo hodnotu true, zajimaji nas jen dalsi logicka pravidla
                    Dim pravidloAplikovano As Boolean
                    If Not pravidlo.AplikujBackward(pravidloAplikovano, neurcitost, aktSeznamDotazu, atributy, bezdotazu) Then
                        Return Me.SetError(pravidlo.LastError)
                    End If

                    'pravidlo vyhodnoceno
                    If pravidloAplikovano Then
                        If mStatus <> Enums.enmStav.enmLogical Then mStatus = Enums.enmStav.enmPartial

                        If pravidlo.Typ = Enums.enmTypPravidla.enmLogicke Then
                            Dim vahazaveru As New Interval
                            If Not pravidlo.GetVahuZaveru(Me.mId, vahazaveru) Then
                                Return Me.SetError(pravidlo.LastError)
                            End If
                            If vahazaveru.JeRovno(Environment.vahaTrue) Then
                                Me.mVaha.SetValue(Environment.vahaTrue)
                                Me.mStatus = Enums.enmStav.enmFinal
                                '++++ spusteni akci
                                uspesneOdvozen = True
                                Return True
                            End If
                            If vahazaveru.JeRovno(Environment.vahaFalse) Then
                                Me.mVaha.SetValue(Environment.vahaFalse)
                                Me.mStatus = Enums.enmStav.enmLogical
                            Else
                                vahyZaveruPravidelProGlob.Add(vahazaveru)
                                normovat = False
                                If pravidlo.Status = Enums.enmStav.enmProvisional Then
                                    pocetPravidelVeStavuProvisional += 1
                                End If
                                If pravidlo.Status = Enums.enmStav.enmFinal Then
                                    pocetPravidelVeStavuFinal += 1
                                End If
                            End If
                        Else
                            Dim vahazaveru As New Interval
                            If Not pravidlo.GetVahuZaveru(Me.mId, vahazaveru) Then
                                Return Me.SetError(pravidlo.LastError)
                            End If
                            vahyZaveruPravidelProGlob.Add(vahazaveru)
                            If pravidlo.Status = Enums.enmStav.enmFinal Then
                                pocetPravidelVeStavuFinal += 1
                            Else
                                pocetPravidelVeStavuProvisional += 1
                            End If
                        End If

                    End If



                End If
            Next

            If mStatus = Enums.enmStav.enmLogical Then 'tohle bylo puvodne napsano jinak
                Me.mStatus = Enums.enmStav.enmFinal
                uspesneOdvozen = True
                '++++ akce
                Return True
            End If

            mVaha.SetValue(neurcitost.GLOB(vahyZaveruPravidelProGlob))

            If normovat Then mVaha.SetValue(neurcitost.NORM(mVaha))

            If pocetPravidelVeStavuFinal = Me.SeznamZaveru.Count Then
                mStatus = Enums.enmStav.enmFinal
            ElseIf pocetPravidelVeStavuProvisional + pocetPravidelVeStavuFinal = Me.SeznamZaveru.Count Then
                mStatus = Enums.enmStav.enmProvisional
            Else
                mStatus = Enums.enmStav.enmPartial
                uspesneOdvozen = False
                Return True
            End If

            If mStatus = Enums.enmStav.enmFinal Then
                '++++ provedeni akci
            End If

            uspesneOdvozen = True

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Vyrok_Chyba_pri_odvozovani_vyroku_backward") & Me.mId)
        End Try
    End Function

End Class
