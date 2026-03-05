Imports System.Xml
Public Class Pripad
    Inherits GeneralObject

    Private mCile As Collection
    Private mAtributy As Collection

#Region "Property"
    Public ReadOnly Property Cile() As Collection
        Get
            Return mCile
        End Get
    End Property
    Public ReadOnly Property Atributy() As Collection
        Get
            Return mAtributy
        End Get
    End Property
#End Region


    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mCile = New Collection
        mAtributy = New Collection
    End Sub

#Region "Xml"
    Public Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal atributesXML As String, ByVal vahy As Collection, ByVal acile As Collection, ByVal rozsahVah As Long, ByVal defaultVaha As Interval) As Boolean
        Try
            Dim xmlDoc As New XmlDocument

            'nacteni xmldokumentu
            Try
                xmlDoc.LoadXml(atributesXML)
            Catch ex As Exception
                Return Me.SetError(ex, GetText("Pripad_Nepodarilo_se_nacist_xml_dokument_pro_Pripad"))
            End Try



            For Each atrElement As XmlElement In xmlDoc.SelectNodes("//attribute")
                Dim atribut As Atribut
                atribut = Nothing
                'vytvorim atribut
                Select Case atrElement.SelectSingleNode("type").InnerXml
                    Case "binary"
                        atribut = New AtributBinary(Environment, defaultVaha, Language)
                    Case "single"
                        atribut = New AtributSingle(Environment, defaultVaha, Language)
                    Case "multiple"
                        atribut = New AtributMultiple(Environment, defaultVaha, Language)
                    Case "numeric"
                        atribut = New AtributNumeric(Environment, defaultVaha, Language)
                End Select

                'nactu atribut
                Dim pomZdrojeCBR As New Collection
                If Not atribut.LoadFromXML(atrElement, 0, Nothing, rozsahVah, defaultVaha, pomZdrojeCBR, "") Then
                    Return Me.SetError(atribut.LastError)
                End If

                atribut.Vaha.SetValue(vahy(atribut.Id))

                If Helper.ValueIsInCollection(acile, atribut.Id) Then
                    mCile.Add(atribut, atribut.Id)
                Else
                    mAtributy.Add(atribut, atribut.Id)
                End If

                If Not xmlElement Is Nothing Then
                    'nacteni odpovedi pro atribut
                    Dim odpovedXml As XmlElement
                    odpovedXml = xmlElement.SelectSingleNode("attribute[id='" & atribut.Id & "']")
                    If Not odpovedXml Is Nothing Then
                        If Not atribut.LoadAnswerFromXML(odpovedXml, defaultVaha) Then
                            Return Me.SetError(atribut.LastError)
                        End If
                    End If
                End If
                
                
                atribut.SpoctiVahu()
            Next




            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Pripad_Chyba_pri_nacitani_Pripadu_z_XML"))
        End Try


    End Function

    Public Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try
            Return Me.SetError("SaveToXML u pripadu neni udelane")

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Pripad_Chyba_pri_SaveToXML_v_Pripadu"))
        End Try
    End Function
#End Region

    Public Function NactiHodnotyPripaduZAtributu(ByVal atributy As Collection, ByVal defaultVaha As Interval, ByRef zadanyVsechnyHodnoty As Boolean, ByVal aktSeznamDotazu As Microsoft.VisualBasic.Collection) As Boolean
        Try
            zadanyVsechnyHodnoty = True
            For Each atribut As Atribut In Me.mAtributy
                Dim atributBZ As Atribut
                atributBZ = atributy(atribut.Id)
                If atributBZ.Status <> Enums.enmStav.enmFinal Then
                    zadanyVsechnyHodnoty = False
                    If Not Helper.ValueIsInCollection(aktSeznamDotazu, atributBZ.Id) Then
                        aktSeznamDotazu.Add(atributBZ, atributBZ.Id)
                    End If
                Else
                    Select Case atribut.Typ
                        Case Enums.enmTypAtributu.enmBinary
                            If CType(atribut, AtributBinary).VlozVahu(atributBZ.Vaha) = NestBase.Atribut.enmVlozeniHodnoty.enmChyba Then
                                Return Me.SetError(atribut.LastError)
                            End If
                        Case Enums.enmTypAtributu.enmSingle
                            If CType(atribut, AtributSingle).VlozHodotu(CType(atributBZ, AtributSingle).Hodnota, atributBZ.Vaha.ToStr, defaultVaha, 1) = NestBase.Atribut.enmVlozeniHodnoty.enmChyba Then
                                Return Me.SetError(atribut.LastError)
                            End If
                        Case Enums.enmTypAtributu.enmMultiple
                            If CType(atribut, AtributMultiple).VlozVahy(CType(atributBZ, AtributMultiple).Hodnoty) = NestBase.Atribut.enmVlozeniHodnoty.enmChyba Then
                                Return Me.SetError(atribut.LastError)
                            End If
                        Case Enums.enmTypAtributu.enmNumeric
                            If CType(atribut, AtributNumeric).VlozHodnotu(CType(atributBZ, AtributNumeric).HodnotaStr, defaultVaha) = NestBase.Atribut.enmVlozeniHodnoty.enmChyba Then
                                Return Me.SetError(atribut.LastError)
                            End If
                    End Select
                End If

            Next

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Pripad_Chyba_pri_nacitani_hodnot_pripadu_z_atributu"))
        End Try
    End Function

End Class
