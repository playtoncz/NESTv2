Imports System.Xml
Public Class ZdrojCBR
    Inherits Zdroj

    Private mNscXml As String
    Private mPripady As Collection
    Private mVahyAtributu As Collection
    Private mCile As Collection
    Private mAktualniPripad As Pripad
    Private mDefaultVaha As Interval

    Public Sub New(ByVal iEnvironment As Environment, ByVal atributy As Collection, ByVal defaultVaha As Interval, ByVal NscXml As String, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, Enums.enmTypZdroje.enmCBR, atributy, defaultVaha, iLanguage)
        mNscXml = NscXml
        mPripady = New Collection
        mVahyAtributu = New Collection
        mCile = New Collection
        mDefaultVaha = defaultVaha
    End Sub

#Region "Xml"
    Public Function LoadFromXML(ByVal atributy As Collection, ByVal rozsahVah As Long, ByVal defaultVaha As Interval) As Boolean
        Dim xmlDoc As New XmlDocument

        'nacteni xmldokumentu
        Try
            xmlDoc.LoadXml(mNscXml.Replace("<!DOCTYPE list_of_answers SYSTEM ""list_of_answers.dtd"">", ""))
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojCBR_Nepodarilo_se_nacist_xml_dokument_pro_zdroj_CBR"))
        End Try

        Dim xmlElement As XmlElement

        'nacteni seznamu cilu
        Try
            For Each xmlElement In xmlDoc.SelectNodes("list_of_answers/goals/attribute_id")
                mCile.Add(xmlElement.InnerXml, xmlElement.InnerXml)
            Next
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojCBR_Chyba_pri_nacitani_cilu_u_zdroje_CBR"))
        End Try

        'nactu si vahy atributu, ktere zaroven slouzi pro urceni, ktere atributy maji byt v kazdem Pripadu
        Try
            For Each xmlElement In xmlDoc.SelectNodes("list_of_answers/weights_of_attributes/weight_of_attribute")
                Dim id_atributu As String = ""
                Helper.NactiXMLString(xmlElement, "id", id_atributu)
                Dim vaha As Interval
                Helper.NactiXMLVahu(xmlElement, "weight", vaha, rozsahVah, defaultVaha)
                mVahyAtributu.Add(vaha, id_atributu)
            Next
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojCBR_Chyba_pri_nacitani_vah_atributu_u_zdroje_CBR"))
        End Try

        Try
            'pripravim xmlstring atributu
            Dim SW As New System.IO.StringWriter
            Dim Writer As New XmlTextWriter(SW)
            Writer.WriteStartElement("attributes")


            For Each atribut As Atribut In atributy
                If Helper.ValueIsInCollection(mVahyAtributu, atribut.Id) Then
                    If Not atribut.SaveToXML(Writer, rozsahVah) Then
                        Return Me.SetError(atribut.LastError)
                    End If
                End If
            Next
            Writer.WriteEndElement()

            Dim stringAtributu As String
            Writer.Flush()
            stringAtributu = SW.ToString

            'nacteni vsech pripadu
            For Each xmlElement In xmlDoc.SelectNodes("list_of_answers/cases/answers")
                Dim pripad As New Pripad(Environment, Language)
                If Not pripad.LoadFromXML(xmlElement, stringAtributu, mVahyAtributu, mCile, rozsahVah, defaultVaha) Then
                    Return Me.SetError(pripad.LastError)
                End If
                mPripady.Add(pripad)
            Next

            'vytvoreni aktualniho pripadu
            mAktualniPripad = New Pripad(Environment, Language)
            mAktualniPripad.LoadFromXML(Nothing, stringAtributu, mVahyAtributu, mCile, rozsahVah, defaultVaha)

           
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZrojCBR_Chyba_pri_nacitani_CBR_z_XML"))
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try

            writer.WriteStartElement("source")
            writer.WriteElementString("source_type", "cbr")
            writer.WriteEndElement()
            Return True

        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojCBR_Chyba_pri_SaveToXML_ve_zdroji_CBR"))
        End Try
    End Function
#End Region

    Public Overrides Function VyhodnotZdroj(ByRef uspesneVyhodnocen As Boolean, ByVal vyrok As Vyrok, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Microsoft.VisualBasic.Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean, ByRef dalsiZdroj As Boolean, Optional ByVal Atribut As Atribut = Nothing, Optional ByRef PripadVzdalenost As Collection = Nothing) As Boolean
        Try
            Dim zadanyVsechnyHodnoty As Boolean = False
            If Not mAktualniPripad.NactiHodnotyPripaduZAtributu(atributy, mDefaultVaha, zadanyVsechnyHodnoty, aktSeznamDotazu) Then
                Return Me.SetError(mAktualniPripad.LastError)
            End If
            If Not zadanyVsechnyHodnoty Then
                uspesneVyhodnocen = True
                Return True
            End If

            Dim cbr As New cbrUsuzovaniCompositional(Environment, Language)
            If Not cbr.SpoctiVahyZaveru(mAktualniPripad, mPripady, Me.mVahyAtributu, PripadVzdalenost) Then
                Return Me.SetError(cbr.LastError)
            End If


            For Each atribut1 As Atribut In mAktualniPripad.Cile
                For Each vyr As Vyrok In atribut1.Vyroky
                    If vyr.Id = vyrok.Id Then
                        vyrok.Vaha.SetValue(vyr.Vaha)
                        vyrok.Status = vyr.Status
                    End If
                Next
            Next

            uspesneVyhodnocen = True
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojCBR_Chyba_pri_vyhodnocovani_zdroje_CBR"))
        End Try

    End Function

    'Public Function NactiAktualniPripad(ByVal atribut As Collection) As Boolean
    '    Try

    '        For Each atribut In mAktualniPripad.at

    '            Return True
    '    Catch ex As Exception
    '        Return Me.SetError(ex, "Chyba pri nacitani aktualniho pripadu pro CBR")
    '    End Try
    'End Function
End Class
