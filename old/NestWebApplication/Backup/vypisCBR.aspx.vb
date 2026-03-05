Public Partial Class vypisCBR
    Inherits cBasePage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If konzultace.nahranaBaze AndAlso Not String.IsNullOrEmpty(konzultace.NcsXML) Then
            
            Dim sb As New System.Text.StringBuilder

            Dim xmlDoc As New System.Xml.XmlDocument
            xmlDoc.LoadXml(konzultace.NcsXML.Replace("<!DOCTYPE list_of_answers SYSTEM ""list_of_answers.dtd"">", ""))

            ''zahlavi tabulky
            'Dim el As System.Xml.XmlElement = xmlPripady.SelectSingleNode("//cases/case")
            'sb.Append("<tr><th>" + Resources.AppResource.Pripad + " </th><th>" + Resources.AppResource.Vzdalenost + "</th>")
            'For Each attribute As System.Xml.XmlElement In el.SelectNodes("answers/attribute")
            '    sb.Append("<th>")
            '    sb.Append(attribute.SelectSingleNode("id").InnerXml)
            '    sb.Append("</th>")
            'Next
            'sb.Append("</tr>")

            Dim counter As Long = 0
            For Each element As System.Xml.XmlElement In xmlDoc.SelectNodes("//cases/answers")
                counter += 1
                sb.Append("<div style=""margin-bottom:10px;"">")
                sb.Append("Případ: " + counter.ToString + "<br/>")
                'pripadySB.Append("<td>")
                'pripadySB.Append(element.SelectSingleNode("distance").InnerXml)
                'pripadySB.Append("</td>")
                sb.Append("<table>")
                For Each attribute As System.Xml.XmlElement In element.SelectNodes("attribute")
                    sb.Append("<tr><td>")
                    sb.Append(attribute.SelectSingleNode("id").InnerXml + "<br/>")
                    '    pripadySB.Append("<td>")
                    sb.Append("</td><td>")
                    Dim type As String = attribute.SelectSingleNode("type").InnerXml
                    Select Case type
                        Case "multiple"
                            For Each ans As System.Xml.XmlElement In attribute.SelectNodes("answer")
                                sb.Append(ans.SelectSingleNode("value").InnerXml + ": ")
                                sb.Append(ans.SelectSingleNode("weight").InnerXml + " ")
                            Next
                        Case "numeric"
                            sb.Append(attribute.SelectSingleNode("answer/value").InnerXml)
                        Case Else
                            sb.Append(attribute.SelectSingleNode("answer/weight").InnerXml)
                    End Select
                    sb.Append("</td></tr>")

                    '    pripadySB.Append("</td>")
                Next
                sb.Append("</table>")
                sb.Append("</div>")
                'pripadySB.Append("</tr>")
            Next

            content.InnerHtml = sb.ToString
        End If
    End Sub

End Class