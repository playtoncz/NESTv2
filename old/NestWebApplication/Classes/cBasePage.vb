
Imports System.Threading
Imports System.Globalization
Public Class cBasePage
    Inherits System.Web.UI.Page

    Public Environment As NestBase.Environment

    Public konzultace As konzultace
    Public userSetting As UserSetting
    Private ResourceManagerCZ As System.Resources.ResourceManager
    Private ResourceManagerEN As System.Resources.ResourceManager


    Protected Overrides Sub InitializeCulture()
        Dim us As UserSetting = Session("userSetting")
        If us Is Nothing Then us = New UserSetting
        us.Load(Request.QueryString)
        Dim lang As NestBase.Enums.enmLanguage
        lang = Me.NactiJazyk(Page, us)



        Dim selectedLanguage As String
        Select Case lang
            Case NestBase.Enums.enmLanguage.cestina
                selectedLanguage = "cs-CZ"
            Case NestBase.Enums.enmLanguage.anglictina
                selectedLanguage = "en-US"
        End Select
        UICulture = selectedLanguage
        Culture = selectedLanguage
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(selectedLanguage)
        Thread.CurrentThread.CurrentUICulture = New CultureInfo(selectedLanguage)

        MyBase.InitializeCulture()
    End Sub

    Protected Overrides Sub OnInit(ByVal e As System.EventArgs)
        Environment = Application("environment")

        userSetting = Session("userSetting")
        If userSetting Is Nothing Then
            userSetting = New UserSetting
        End If
        userSetting.Language = Me.NactiJazyk(Me.Page, userSetting)

        konzultace = Session("konzultace")
        If konzultace Is Nothing Then
            konzultace = New konzultace(Environment, Me.userSetting.Language)
        End If
        'ResourceManagerCZ = New System.Resources.ResourceManager("NestWeb.AppResource.cs", GetType(cBasePage).Assembly)
        'ResourceManagerEN = New System.Resources.ResourceManager("NestWeb.AppResource", GetType(cBasePage).Assembly)

        Session("userSetting") = userSetting
        MyBase.OnInit(e)
    End Sub

    Public Function GetTitle() As String
        Return "NEST Web"
    End Function

    Public Function GetMeta() As String
        Return ""
    End Function

    Public Function PocetTlacitek() As Long
        Try
            Dim col As Collection
            col = Session("textyTlacitek")
            Return col.Count
        Catch ex As Exception
            Return 0
        End Try

    End Function

    Public Function ZobrazitWeightEdit(ByVal typAtributu As NestBase.Enums.enmTypAtributu) As Boolean
        Dim insertWeight As String = Session("insertWeight")
        Select Case typAtributu
            Case NestBase.Enums.enmTypAtributu.enmBinary
                If insertWeight.IndexOf("b") > -1 Then Return True
            Case NestBase.Enums.enmTypAtributu.enmSingle
                If insertWeight.IndexOf("s") > -1 Then Return True
            Case NestBase.Enums.enmTypAtributu.enmMultiple
                If insertWeight.IndexOf("m") > -1 Then Return True
        End Select
        Return False
    End Function

    Public Function GetText(ByVal value As String) As String
        Return GetGlobalResourceObject("AppResource", value).ToString
        
    End Function

    Public Shared Function NactiJazyk(ByVal page As System.Web.UI.Page, ByVal userSetting As UserSetting) As NestBase.Enums.enmLanguage
        Dim deflan As String = ""
        Dim result As NestBase.Enums.enmLanguage
        Dim lang As String = ""
        lang = page.Request.QueryString("lang")
        If lang <> "" Then
            Select Case lang
                Case "cz"
                    result = NestBase.Enums.enmLanguage.cestina
                Case "en"
                    result = NestBase.Enums.enmLanguage.anglictina
                Case Else
                    result = result = NestBase.Enums.enmLanguage.cestina
            End Select
            page.Session("Language") = result
            Return result
        End If
        Try
            deflan = page.Request.UserLanguages(0)
        Catch ex As Exception
            deflan = ""
        End Try
        Try
            '--Nactu jazyk
            If Not page.Session("userSetting") Is Nothing Then
                result = userSetting.Language
            Else
                If deflan = "" Then
                    result = NestBase.Enums.enmLanguage.cestina
                ElseIf deflan.Substring(0, 2) = "en" Then
                    result = NestBase.Enums.enmLanguage.anglictina
                End If
                result = NestBase.Enums.enmLanguage.cestina

            End If

            Return result
        Catch ex As Exception
            Return NestBase.Enums.enmLanguage.cestina
        End Try
    End Function

    


    Private Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        Dim us As UserSetting = Session("userSetting")
        If us Is Nothing Then us = New UserSetting
        us.Load(Request.QueryString)
        Session("userSetting") = us

        Me.Page.Theme = us.app

    End Sub
End Class


