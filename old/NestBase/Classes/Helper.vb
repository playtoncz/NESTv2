Public Class Helper

#Region "Nacitani z XML"
    Public Shared Function NactiXMLString(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As String, Optional ByVal defaultValue As String = "") As Boolean
        Try
            Value = xmlElement.SelectSingleNode(NameOfNode).InnerXml
            Return True
        Catch ex As Exception
            Value = defaultValue
            Return False
        End Try
    End Function
    Public Shared Function NactiXMLDatum(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As Date) As Boolean
        Try
            Value = CDate(xmlElement.SelectSingleNode(NameOfNode).InnerXml)
            Return True
        Catch ex As Exception
            Value = Now
            Return False
        End Try
    End Function
    Public Shared Function NactiXMLSingle(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As Single, Optional ByVal defaultValue As Single = 0) As Boolean
        Try
            Value = CSng(upravDesetinneTeckyCarky(xmlElement.SelectSingleNode(NameOfNode).InnerXml))
            Return True
        Catch ex As Exception
            Value = defaultValue
            Return False
        End Try
    End Function
    Public Shared Function NactiXMLDouble(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As Double, Optional ByVal defaultValue As Double = 0) As Boolean
        Try
            Value = CDbl(upravDesetinneTeckyCarky(upravDesetinneTeckyCarky(xmlElement.SelectSingleNode(NameOfNode).InnerXml)))
            Return True
        Catch ex As Exception
            Value = defaultValue
            Return False
        End Try
    End Function
    Public Shared Function NactiXMLLong(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As Long, Optional ByVal defaultValue As Long = 0) As Boolean
        Try
            Value = CLng(upravDesetinneTeckyCarky(xmlElement.SelectSingleNode(NameOfNode).InnerXml))
            Return True
        Catch ex As Exception
            Value = defaultValue
            Return False
        End Try
    End Function
    Public Shared Function NactiXMLBoolean(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As Boolean, Optional ByVal defaultValue As Boolean = False) As Boolean
        Try
            Value = CBool(CDbl(xmlElement.SelectSingleNode(NameOfNode).InnerXml))
            Return True
        Catch ex As Exception
            Value = defaultValue
            Return False
        End Try
    End Function

    Public Shared Function StrToDbl(ByVal hodnotaStr As String, ByRef value As Double) As Boolean
        Try
            value = CDbl(hodnotaStr)
            Return True
        Catch ex As Exception

        End Try
        Try
            If hodnotaStr.IndexOf(".") > 0 Then
                hodnotaStr = hodnotaStr.Replace(".", ",")
            ElseIf hodnotaStr.IndexOf(",") > 0 Then
                hodnotaStr = hodnotaStr.Replace(",", ".")
            End If
            value = CDbl(hodnotaStr)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function NactiVahu(ByVal hodnotaStr As String, ByRef Value As Interval, ByVal rozsahVah As Long, ByVal defaultValue As Interval) As Boolean
        Try
            If hodnotaStr = "" Then
                'Value = New Interval(defaultValue)
                Return False
            End If

            If LCase(hodnotaStr) = "unknown" Then
                Value = New Interval(-1, 1)
                Return True
            End If
            If LCase(hodnotaStr) = "irrelevant" Then
                Value = New Interval(0, 0)
                Return True
            End If
            If LCase(hodnotaStr) = "default" Then
                Value = New Interval(defaultValue)
                Return True
            End If

            If hodnotaStr.IndexOf(";") > 0 Then 'dve ruzne hodnoty intervalu
                Dim hodnoty() As String
                hodnoty = Split(hodnotaStr, ";")
                Dim hodnotaStr1 As String
                Dim hodnotaStr2 As String
                hodnotaStr1 = hodnoty(0)
                hodnotaStr2 = hodnoty(1)
                Dim hodnota1 As Double
                Dim hodnota2 As Double
                If StrToDbl(hodnotaStr1, hodnota1) Then
                    If StrToDbl(hodnotaStr2, hodnota2) Then
                        Value.SetValue(hodnota1 / rozsahVah, hodnota2 / rozsahVah)
                        Return True
                    End If
                End If
            Else 'jedna hodnota intervalu
                Dim hodnota As Double
                If StrToDbl(hodnotaStr, hodnota) Then
                    Value.SetValue(hodnota / rozsahVah, hodnota / rozsahVah)
                    Return True
                End If
            End If
            Value = New Interval(defaultValue)
            Return False
        Catch ex As Exception
            Value = New Interval(defaultValue)
            Return False
        End Try
    End Function

    Public Shared Function NactiXMLVahu(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As Interval, ByVal rozsahVah As Long, ByVal defaultValue As Interval) As Boolean
        Try
            Dim hodnotaStr As String
            If Not NactiXMLString(xmlElement, NameOfNode, hodnotaStr, 0) Then
                Value = New Interval(defaultValue)
                Return False
            End If
            hodnotaStr = upravDesetinneTeckyCarky(hodnotaStr)
            Return NactiVahu(hodnotaStr, Value, rozsahVah, defaultValue)

            'If hodnotaStr = "" Then
            '    Value = New Interval(defaultValue)
            '    Return False
            'End If

            'If LCase(hodnotaStr) = "unknown" Then
            '    Value = New Interval(-1, 1)
            '    Return True
            'End If
            'If LCase(hodnotaStr) = "irrelevant" Then
            '    Value = New Interval(0, 0)
            '    Return True
            'End If

            'If hodnotaStr.IndexOf(";") > 0 Then 'dve ruzne hodnoty intervalu
            '    Dim hodnoty() As String
            '    hodnoty = Split(hodnotaStr, ";")
            '    Dim hodnotaStr1 As String
            '    Dim hodnotaStr2 As String
            '    hodnotaStr1 = hodnoty(0)
            '    hodnotaStr2 = hodnoty(1)
            '    Dim hodnota1 As Double
            '    Dim hodnota2 As Double
            '    If StrToDbl(hodnotaStr1, hodnota1) Then
            '        If StrToDbl(hodnotaStr2, hodnota2) Then
            '            Value.SetValue(hodnota1 / rozsahVah, hodnota2 / rozsahVah)
            '            Return True
            '        End If
            '    End If
            'Else 'jedna hodnota intervalu
            '    Dim hodnota As Double
            '    If StrToDbl(hodnotaStr, hodnota) Then
            '        Value.SetValue(hodnota / rozsahVah, hodnota / rozsahVah)
            '        Return True
            '    End If
            'End If
            'Value = New Interval(defaultValue)
            'Return False
        Catch ex As Exception
            Value = New Interval(defaultValue)
            Return False
        End Try
    End Function


    Public Shared Function NactiHodnotuNumeric(ByVal Hodnota As String, ByRef Value As Interval, ByRef IsNumber As Boolean, ByVal defaultValue As Interval) As Boolean
        Try
            If Hodnota = "" Then
                Value = New Interval(defaultValue)
                Return False
            End If

            If LCase(Hodnota) = "unknown" Then
                Value = New Interval(-1, 1)
                IsNumber = False
                Return True
            End If
            If LCase(Hodnota) = "irrelevant" Then
                Value = New Interval(0, 0)
                IsNumber = False
                Return True
            End If

            IsNumber = True
            If Hodnota.IndexOf(";") > 0 Then 'dve ruzne hodnoty intervalu
                Dim hodnoty() As String
                hodnoty = Split(Hodnota, ";")
                Dim hodnotaStr1 As String
                Dim hodnotaStr2 As String
                hodnotaStr1 = hodnoty(0)
                hodnotaStr2 = hodnoty(1)
                Dim hodnota1 As Double
                Dim hodnota2 As Double
                If StrToDbl(hodnotaStr1, hodnota1) Then
                    If StrToDbl(hodnotaStr2, hodnota2) Then
                        Value.SetValue(hodnota1, hodnota2)
                        Return True
                    End If
                End If
            Else 'jedna hodnota intervalu
                Dim hodnota2 As Double
                If StrToDbl(Hodnota, Hodnota2) Then
                    Value.SetValue(Hodnota2, Hodnota2)
                    Return True
                End If
            End If
            Value = New Interval(defaultValue)
            Return False
        Catch ex As Exception
            Value = New Interval(defaultValue)
            Return False
        End Try
    End Function



    Public Shared Function NactiXMLHodnotuNumeric(ByVal xmlElement As Xml.XmlElement, ByVal NameOfNode As String, ByRef Value As Interval, ByRef IsNumber As Boolean, ByVal defaultValue As Interval) As Boolean
        Try
            IsNumber = True
            Dim hodnotaStr As String
            If Not NactiXMLString(xmlElement, NameOfNode, hodnotaStr, 0) Then
                Value = New Interval(defaultValue)
                Return False
            End If
            hodnotaStr = upravDesetinneTeckyCarky(hodnotaStr)

            Return NactiHodnotuNumeric(hodnotaStr, Value, IsNumber, defaultValue)

            'If hodnotaStr = "" Then
            '    Value = New Interval(defaultValue)
            '    Return False
            'End If

            'If LCase(hodnotaStr) = "unknown" Then
            '    Value = New Interval(-1, 1)
            '    IsNumber = False
            '    Return True
            'End If
            'If LCase(hodnotaStr) = "irrelevant" Then
            '    Value = New Interval(0, 0)
            '    IsNumber = False
            '    Return True
            'End If

            'If hodnotaStr.IndexOf(";") > 0 Then 'dve ruzne hodnoty intervalu
            '    Dim hodnoty() As String
            '    hodnoty = Split(hodnotaStr, ";")
            '    Dim hodnotaStr1 As String
            '    Dim hodnotaStr2 As String
            '    hodnotaStr1 = hodnoty(0)
            '    hodnotaStr2 = hodnoty(1)
            '    Dim hodnota1 As Double
            '    Dim hodnota2 As Double
            '    If StrToDbl(hodnotaStr1, hodnota1) Then
            '        If StrToDbl(hodnotaStr2, hodnota2) Then
            '            Value.SetValue(hodnota1, hodnota2)
            '            Return True
            '        End If
            '    End If
            'Else 'jedna hodnota intervalu
            '    Dim hodnota As Double
            '    If StrToDbl(hodnotaStr, hodnota) Then
            '        Value.SetValue(hodnota, hodnota)
            '        Return True
            '    End If
            'End If
            'Value = New Interval(defaultValue)
            'Return False
        Catch ex As Exception
            Value = New Interval(defaultValue)
            Return False
        End Try
    End Function
#End Region

    Public Shared Function FormatDatum(ByVal datum As Date) As String
        Return Format(datum, "d.M.yyyy")
    End Function
    Public Shared Function FormatInterval(ByVal interval As Interval, ByVal rozsahVah As Long) As String
        Return "[" & CStr(interval.MinHodnota * rozsahVah) & "; " & CStr(interval.MaxHodnota * rozsahVah) & "]"
    End Function

    Public Shared Sub VypisAddLine(ByRef text As String, ByVal line As String, ByVal typVypisu As Enums.enmTypVypisu, Optional ByVal typRadky As Enums.enmTypRadkyVypisu = Enums.enmTypRadkyVypisu.enmNormal)
        Select Case typVypisu
            Case Enums.enmTypVypisu.enmText
                text += line & vbCrLf
            Case Enums.enmTypVypisu.enmHTML
                Select Case typRadky
                    Case Enums.enmTypRadkyVypisu.enmNormal
                        text += "<div class=""vypis_radka"">" & line & "</div>"
                    Case Enums.enmTypRadkyVypisu.enmNadpis1
                        text += "<div class=""vypis_nadpis1"">" & line & "</div>"
                    Case Enums.enmTypRadkyVypisu.enmNadpis2
                        text += "<div class=""vypis_nadpis2"">" & line & "</div>"
                    Case Enums.enmTypRadkyVypisu.enmNadpis3
                        text += "<div class=""vypis_nadpis3"">" & line & "</div>"
                    Case Enums.enmTypRadkyVypisu.enmCara
                        text += "<hr>"
                    Case Enums.enmTypRadkyVypisu.enmOdsazeniStart
                        text += "<div class=""vypis_odsazeni"">"
                    Case Enums.enmTypRadkyVypisu.enmOdsazeniEnd
                        text += "</div>"
                    Case Enums.enmTypRadkyVypisu.enmAName
                        text += "<a name=""" & line & """></a>"
                End Select

        End Select
    End Sub

    Public Shared Function ValueIsInCollection(ByVal col As Collection, ByVal value As String) As Boolean
        Try
            Dim obj As Object
            obj = col(value)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function FloatToStr(ByVal value As Double) As String
        Return Format(value, "###########0.000")
    End Function

    Public Shared Function upravDesetinneTeckyCarky(ByVal text As String) As String
        'Return text.Replace(",", ".")

        'Dim znacka1 As String
        'Dim znacka2 As String
        Try
            Dim x As Double
            x = CDbl("1,1")
            If x < 2 Then Return text.Replace(".", ",")
            Return text.Replace(",", ".")
        Catch ex As Exception
            Return text.Replace(",", ".")
        End Try

    End Function
End Class
