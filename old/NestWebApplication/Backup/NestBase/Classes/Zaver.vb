Public Class Zaver
    Inherits GeneralObject

    Private mLiteral As Literal
    Private mCtr As Interval

#Region "Property"
    Public ReadOnly Property Literal() As Literal
        Get
            Return mLiteral
        End Get
    End Property
    Public ReadOnly Property Ctr() As Interval
        Get
            Return mCtr
        End Get
    End Property
    Public ReadOnly Property VahaZaveru() As Interval
        Get
            Return mLiteral.Vaha
        End Get
    End Property
    Public ReadOnly Property NegaceZaveru() As Boolean
        Get
            Return mLiteral.Negace
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mLiteral = New Literal(Environment, defaultVaha, iLanguage)
        mCtr = New Interval(defaultVaha)
    End Sub


#Region "Xml"
    Public Overridable Function LoadFromXML(ByVal xmlElement As Xml.XmlElement, ByVal defaultVaha As Interval, ByVal Atributy As Collection, ByVal rozsahVah As Long) As Boolean
        If Not mLiteral.LoadFromXML(xmlElement, Atributy) Then
            Return Me.SetError(mLiteral.LastError)
        End If
        Try
            Helper.NactiXMLVahu(xmlElement, "weight", mCtr, rozsahVah, defaultVaha)

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Zaver_Chyba_pri_nacitani_zaveru_pravidla_z_XML"))
        End Try
    End Function

    Public Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal WriteWeight As Boolean, ByVal rozsahVah As Long) As Boolean
        Try
            writer.WriteStartElement("conclusion")
            writer.WriteElementString("id_attribute", Me.mLiteral.Vyrok.RodicovskyAtribut.Id)
            If Me.mLiteral.Vyrok.RodicovskyAtribut.Typ <> Enums.enmTypAtributu.enmBinary Then
                writer.WriteElementString("id_proposition", Me.mLiteral.Vyrok.Id)
            End If
            writer.WriteElementString("negation", IIf(mLiteral.Negace, "1", "0"))

            If WriteWeight Then writer.WriteElementString("weight", Me.mCtr.ToStr(rozsahVah))

            writer.WriteEndElement()
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Zaver_Chyba_pri_SaveToXML_v_zaveru"))
        End Try
    End Function
#End Region


    Public Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long, Optional ByVal vypisovatVahu As Boolean = True) As Boolean
        Try
            mLiteral.GetVypis(text, typVypisu, rozsahVah)
            If vypisovatVahu Then text += "(" & mCtr.ToStr(rozsahVah) & ")"
            Return True

            'Helper.VypisAddLine(text, "     Závìr", typVypisu)
            'If Not mLiteral.GetVypis(text, typVypisu, rozsahVah) Then
            '    Return Me.SetError(mLiteral.LastError)
            'End If
            'Helper.VypisAddLine(text, "     Ctr: " & Helper.FormatInterval(mCtr, rozsahVah), typVypisu)

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("Zaver_Chyba_pri_tvorbe_vypisu_zaveru"))
        End Try
    End Function

    Public Function PridejZaverZeStringu(ByVal zaverStr As String, ByVal defaultvaha As Interval, ByVal atributy As Collection) As Boolean
        Dim r As New System.Text.RegularExpressions.Regex("^(\s*)(not)?(\s*)(\(?)(\s*)([^\s\(\)]*)(\s*)(\(?)(\s*)([^\s\(\)]*)(\s*)(\)?)(\s*)(\)?)(\s*)(\[)(-?)(\d*)([\.,]?)(\d*)(\])(\s*)$")
        Dim m As System.Text.RegularExpressions.Match
        m = r.Match(zaverStr)
        If Not m.Success Then

            zaverStr += "[1]"

            m = r.Match(zaverStr)
            If Not m.Success Then
                Return Me.SetError("zaver - " & zaverStr & " - ma spatnou syntax")
            End If
        End If

        Dim literalStr As String
        literalStr = r.Replace(zaverStr, "$2$3$4$5$6$7$8$9$10$11$12$13$14")
        If Not Me.Literal.PridejLiteralZeStringu(literalStr, defaultvaha, atributy) Then
            Return Me.SetError(Literal.LastError)
        End If

        Dim vahaStr As String
        vahaStr = r.Replace(zaverStr, "$17$18$19$20")
        Dim vahaFl As Double
        If Not Helper.StrToDbl(vahaStr, vahaFl) Then
            Return Me.SetError("Vaha v zaveru - " & zaverStr & " neni cislo")
        End If
        Me.mCtr = New Interval(vahaFl, vahaFl)
        Return True
    End Function
End Class
