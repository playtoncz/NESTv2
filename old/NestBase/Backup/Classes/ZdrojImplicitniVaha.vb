Public Class ZdrojImplicitniVaha
    Inherits Zdroj

    Private mVaha As Interval    

    Public Sub New(ByVal iEnvironment As Environment, ByVal atributy As Collection, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, Enums.enmTypZdroje.enmImplicitniVaha, atributy, defaultVaha, iLanguage)
        mVaha = New Interval(defaultVaha)
    End Sub

#Region "Xml"
    Public Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal rozsahVah As Long, ByVal defaultVaha As Interval) As Boolean
        Try

            Helper.NactiXMLVahu(xmlElement, "default_weight", mVaha, rozsahVah, defaultVaha)


            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojImplicitniVaha_Chyba_pri_nacitani_implicitni_vahy_z_XML"))
        End Try
    End Function

    Public Overloads Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Try
            writer.WriteStartElement("source")
            writer.WriteElementString("source_type", "default_weight")
            writer.WriteElementString("default_weight", Me.mVaha.ToStr(rozsahVah, True))
            writer.WriteEndElement()
            Return True
            
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojImplicitniVaha_Chyba_pri_SaveToXML_ve_zdroji_implicitni_vaha"))
        End Try
    End Function
#End Region

    Public Overrides Function VyhodnotZdroj(ByRef uspesneVyhodnocen As Boolean, ByVal vyrok As Vyrok, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Microsoft.VisualBasic.Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean, ByRef dalsiZdroj As Boolean, Optional ByVal Atribut As Atribut = Nothing) As Boolean
        Try
            vyrok.Vaha.SetValue(mVaha)
            vyrok.Status = Enums.enmStav.enmFinal
            uspesneVyhodnocen = True
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojImplicitniVaha_Chyba_pri_vyhodnocovani_zdroje_implicitni_vaha"))
        End Try

    End Function
End Class
