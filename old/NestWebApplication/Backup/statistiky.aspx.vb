
Partial Class statistiky
    Inherits cBasePage


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If konzultace.nahranaBaze Then
            Dim str As String = ""
            If konzultace.BZ.GetStatistiky(str) Then
                statistiky_div.InnerHtml = str
            Else
                CType(Master, BasePage).ErrorMessage = konzultace.BZ.LastError.UserMessage
            End If
        End If
    End Sub

End Class

