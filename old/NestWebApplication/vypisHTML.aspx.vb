
Partial Class vypisHTML
    Inherits cBasePage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If konzultace.nahranaBaze Then
            Dim str As String = ""
            If konzultace.BZ.GetVypis(str, NestBase.Enums.enmTypVypisu.enmHTML) Then
                vypis.InnerHtml = str
            Else
                CType(Master, BasePage).ErrorMessage = konzultace.BZ.LastError.UserMessage
            End If

        End If
    End Sub
End Class

