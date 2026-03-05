Partial Class check
    Inherits cBasePage

    Protected Sub cmdNahraj_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdNahraj.Click
        Dim fileLen As Integer
        Dim str As New System.Text.StringBuilder

        fileLen = efile.PostedFile.ContentLength
        Dim Input(fileLen) As Byte
        efile.PostedFile.InputStream.Read(Input, 0, fileLen)


        str.Append(System.Text.Encoding.GetEncoding("windows-1250").GetString(Input, 0, fileLen))

        Dim BZ As New NestBase.BazeZnalosti(Environment, Me.userSetting.Language)

        If BZ.CheckBase(str.ToString) Then
            lblMessage.Text = "OK"
        Else
            lblMessage.Text = BZ.LastError.UserMessage

        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        lblMessage.Text = ""
    End Sub
End Class
