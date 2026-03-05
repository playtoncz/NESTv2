Public Class VyrokNumeric
    Inherits Vyrok

    Private mFuzzyLower As Double
    Private mCrispLower As Double
    Private mCrispUpper As Double
    Private mFuzzyUpper As Double
    
    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iRodicovskyAtribut As Atribut, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iRodicovskyAtribut, iLanguage)
        Me.mTyp = Enums.enmTypVyroku.enmNumeric
    End Sub
    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iRodicovskyAtribut As Atribut, ByVal iId As String, ByVal ilanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, defaultVaha, iRodicovskyAtribut, iId, iLanguage)
        Me.mTyp = Enums.enmTypVyroku.enmNumeric
    End Sub
    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iRodicovskyAtribut As Atribut, ByVal iId As String, ByVal ilanguage As Enums.enmLanguage, ByVal iFuzzyLower As Double, ByVal iCrispLower As Double, ByVal iCrispUpper As Double, ByVal iFuzzyUpper As Double)
        MyBase.New(iEnvironment, defaultVaha, iRodicovskyAtribut, iId, ilanguage)
        Me.mTyp = Enums.enmTypVyroku.enmNumeric
        Me.mFuzzyLower = iFuzzyLower
        Me.mCrispLower = iCrispLower
        Me.mCrispUpper = iCrispUpper
        Me.mFuzzyUpper = iFuzzyUpper
    End Sub



    Public Overrides Function LoadFromXML(ByVal xmlElement As Xml.XmlElement) As Boolean
        If Not MyBase.LoadFromXML(xmlElement) Then
            Return False
        End If
        Try
            Dim weightFunctionElement As Xml.XmlElement
            weightFunctionElement = xmlElement.SelectSingleNode("weight_function")
            Helper.NactiXMLDouble(weightFunctionElement, "fuzzy_lower_bound", mFuzzyLower, 0)
            Helper.NactiXMLDouble(weightFunctionElement, "crisp_lower_bound", mCrispLower, 0)
            Helper.NactiXMLDouble(weightFunctionElement, "crisp_upper_bound", mCrispUpper, 0)
            Helper.NactiXMLDouble(weightFunctionElement, "fuzzy_upper_bound", mFuzzyUpper, 0)

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("VyrokNumeric_Chyba_pri_nacitani_numeric_vyroku_z_XML"))
        End Try
    End Function

#Region "vypis"
    Public Overrides Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
        Try
            Helper.VypisAddLine(text, Id, typVypisu, Enums.enmTypRadkyVypisu.enmAName)
            Helper.VypisAddLine(text, "     " & Me.GetText("Vyrok") & "", typVypisu, Enums.enmTypRadkyVypisu.enmNadpis3)
            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniStart)
            Helper.VypisAddLine(text, "     " & Me.GetText("id") & ": " & Id, typVypisu)
            Helper.VypisAddLine(text, "     " & Me.GetText("Jmeno") & ": " & Jmeno, typVypisu)
            Helper.VypisAddLine(text, "     " & Me.GetText("FuzzyLower") & ": " & Me.mFuzzyLower, typVypisu)
            Helper.VypisAddLine(text, "     " & Me.GetText("CrispLower") & ": " & Me.mCrispLower, typVypisu)
            Helper.VypisAddLine(text, "     " & Me.GetText("CrispUpper") & ": " & Me.mCrispUpper, typVypisu)
            Helper.VypisAddLine(text, "     " & Me.GetText("FuzzyUpper") & ": " & Me.mFuzzyUpper, typVypisu)

            Helper.VypisAddLine(text, "", typVypisu, Enums.enmTypRadkyVypisu.enmOdsazeniEnd)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("VyrokNumeric_Chyba_pri_tvorbe_vypisu_vyroku") & Id)
        End Try
    End Function
#End Region

    Public Overrides Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
        Try
            writer.WriteStartElement("proposition")

            writer.WriteElementString("id", Id)
            writer.WriteElementString("name", Me.Jmeno)
            writer.WriteElementString("comment", Me.Komentar)

            writer.WriteStartElement("weight_function")
            writer.WriteElementString("fuzzy_lower_bound", Helper.FloatToStr(Me.mFuzzyLower))
            writer.WriteElementString("crisp_lower_bound", Helper.FloatToStr(Me.mCrispLower))
            writer.WriteElementString("crisp_upper_bound", Helper.FloatToStr(Me.mCrispUpper))
            writer.WriteElementString("fuzzy_upper_bound", Helper.FloatToStr(Me.mFuzzyUpper))
            writer.WriteEndElement()

            writer.WriteEndElement()
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("VyrokNumeric_Chyba_pri_SaveToXML_ve_vyroku") & Me.Id)
        End Try
    End Function

    Public Function SpoctiVahu(ByVal hodnota As Double) As Single

        If (hodnota >= mCrispLower) And (hodnota <= mCrispUpper) Then 'je-li hodnota atributu uvnitr fuzzy intervalu
            Return 1
        ElseIf (hodnota >= mFuzzyLower) And (hodnota <= mCrispLower) Then             'je-li hodnota atributu v levem okraji fuzzy intervalu
            Return (2 * (hodnota - mFuzzyLower) / (mCrispLower - mFuzzyLower)) - 1 'takto se spocita, pokud je vysledna vaha v rozsahu -0.99 - 0.99
        ElseIf (hodnota >= mCrispUpper) And (hodnota <= mFuzzyUpper) Then 'je-li hodnota atributu v pravem okraji fuzzy intervalu
            Return (2 * (mFuzzyUpper - hodnota) / (mFuzzyUpper - mCrispUpper)) - 1 'takto se spocita, pokud je vysledna vaha v rozsahu -0.99 - 0.99
        End If
        Return -1  '-0.99
    End Function
End Class
