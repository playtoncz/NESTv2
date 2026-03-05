Imports Microsoft.VisualBasic

Public Class UserSetting
    Public Language As NestBase.Enums.enmLanguage
    Public app As String

    Public Sub New()
        Language = NestBase.Enums.enmLanguage.cestina
    End Sub

    Public Sub Load(ByVal qs As System.Collections.Specialized.NameValueCollection)
        Select Case qs("lang")
            Case "cz"
                Language = NestBase.Enums.enmLanguage.cestina
            Case "en"
                Language = NestBase.Enums.enmLanguage.anglictina
        End Select
        If qs("app") <> "" Then
            app = qs("app")
        End If
        If app = "no" Then app = "ThemeBase"
        If app = "" Then app = "ThemeBase"
    End Sub
End Class
