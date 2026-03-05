Imports System.Web.Services


Namespace NestWS


<System.Web.Services.WebService(Namespace := "http://tempuri.org/NestWS/NestWS")> _
Public Class NestWS
    Inherits System.Web.Services.WebService

#Region " Web Services Designer Generated Code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Web Services Designer.
        InitializeComponent()

        'Add your own initialization code after the InitializeComponent() call

    End Sub

    'Required by the Web Services Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Web Services Designer
    'It can be modified using the Web Services Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        'CODEGEN: This procedure is required by the Web Services Designer
        'Do not modify it using the code editor.
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

    Public Class BaseResult
        Public isOK As Boolean
        Public errorMessage As String
    End Class

    Public Class ConsultationResult
        Inherits BaseResult
        Public ResultXML As String
    End Class

    'Public Class QuestionFromBazeResult
    '    Inherits BaseResult
    '    Public AnswersXML As String
    'End Class

    ' WEB SERVICE EXAMPLE
    ' The HelloWorld() example service returns the string Hello World.
    ' To build, uncomment the following lines then save and build the project.
    ' To test this web service, ensure that the .asmx file is the start page
    ' and press F5.
    '
    '<WebMethod()> _
    'Public Function HelloWorld() As String
    '   Return "Hello World"
    'End Function
    <WebMethod()> _
    Public Function RunConsultation(ByVal BazeZnalostiXML As String, ByVal AnswersXML As String) As ConsultationResult
        Dim env As NestBase.Environment
        env = Application("environment")
            Dim BZ As New NestBase.BazeZnalosti(env, NestBase.Enums.enmLanguage.cestina)
        Dim resultxml As String
        Dim result As New ConsultationResult
        If BZ.RunConsultation(BazeZnalostiXML, AnswersXML, resultxml) Then
            result.isOK = True
            result.ResultXML = resultxml
        Else
            result.isOK = False
            result.errorMessage = BZ.LastError.UserMessage & " : " & BZ.LastError.InnerMessage

        End If
        Return result
        End Function

        <WebMethod()> _
    Public Function RunConsultationZip(ByVal BazeZnalostiZip() As Byte) As ConsultationResult
            Dim BazeZnalostiXML As String
            Dim AnswersXML As String
            Dim result As New ConsultationResult

            Dim name As String = "BZ" & Format(Now(), "ddMMyyyyhhmmss") & ".zip"
            Dim oFile2 As System.IO.FileInfo
            oFile2 = New System.IO.FileInfo(Server.MapPath("~/temp/" & name))
            Dim oFileStream2 As IO.FileStream = oFile2.OpenWrite
            Dim lBytes2 As Long = BazeZnalostiZip.Length
            oFileStream2.Write(BazeZnalostiZip, 0, lBytes2)
            oFileStream2.Close()



            '--ctu ze zipu
            Dim xmlstring As String
            Try
                Dim ZF As New C1.C1Zip.C1ZipFile()
                ZF.Open(Server.MapPath("~/temp/" & name))
                Dim ZE As C1.C1Zip.C1ZipEntry
                For Each ZE In ZF.Entries
                    If LCase(ZE.FileName) = "bz.xml" Then
                        Dim sr As New IO.StreamReader(ZE.OpenReader)
                        BazeZnalostiXML = sr.ReadToEnd()
                        sr.Close()
                    End If
                    If LCase(ZE.FileName) = "answers.xml" Then
                        Dim sr As New IO.StreamReader(ZE.OpenReader)
                        AnswersXML = sr.ReadToEnd()
                        sr.Close()
                    End If

                Next
            Catch ex As Exception
                result.isOK = False
                result.errorMessage = ex.Message
                Return result
            End Try


            Dim env As NestBase.Environment
            env = Application("environment")
            Dim BZ As New NestBase.BazeZnalosti(env, NestBase.Enums.enmLanguage.cestina)
            Dim resultxml As String

            If BZ.RunConsultation(BazeZnalostiXML, AnswersXML, resultxml) Then
                result.isOK = True
                result.ResultXML = resultxml
            Else
                result.isOK = False
                result.errorMessage = BZ.LastError.UserMessage & " : " & BZ.LastError.InnerMessage

            End If
            Return result

        End Function

        <WebMethod()> _
    Public Function RunConsultationZip2(ByVal BazeZnalostiZip() As Byte, ByVal AnswersXML As String) As ConsultationResult
            Dim BazeZnalostiXML As String

            Dim result As New ConsultationResult

            Dim name As String = "BZ" & Format(Now(), "ddMMyyyyhhmmss") & ".zip"
            Dim oFile2 As System.IO.FileInfo
            oFile2 = New System.IO.FileInfo(Server.MapPath("~/temp/" & name))
            Dim oFileStream2 As IO.FileStream = oFile2.OpenWrite
            Dim lBytes2 As Long = BazeZnalostiZip.Length
            oFileStream2.Write(BazeZnalostiZip, 0, lBytes2)
            oFileStream2.Close()



            '--ctu ze zipu
            Dim xmlstring As String
            Try
                Dim ZF As New C1.C1Zip.C1ZipFile()
                ZF.Open(Server.MapPath("~/temp/" & name))
                Dim ZE As C1.C1Zip.C1ZipEntry
                For Each ZE In ZF.Entries
                    'If LCase(ZE.FileName) = "bz.xml" Then
                    Dim sr As New IO.StreamReader(ZE.OpenReader)
                    BazeZnalostiXML = sr.ReadToEnd()
                    sr.Close()
                    Exit For
                    'End If
                Next
            Catch ex As Exception
                result.isOK = False
                result.errorMessage = ex.Message
                Return result
            End Try


            Dim env As NestBase.Environment
            env = Application("environment")
            Dim BZ As New NestBase.BazeZnalosti(env, NestBase.Enums.enmLanguage.cestina)
            Dim resultxml As String

            If BZ.RunConsultation(BazeZnalostiXML, AnswersXML, resultxml) Then
                result.isOK = True
                result.ResultXML = resultxml
            Else
                result.isOK = False
                result.errorMessage = BZ.LastError.UserMessage & " : " & BZ.LastError.InnerMessage

            End If
            Return result

        End Function

    '<WebMethod()> _
    'Public Function GetQuestionFromBaze(ByVal BazeZnalostiXML As String, ByVal AnswersXML As String) As QuestionFromBazeResult
    '    Dim env As NestBase.Environment
    '    env = Application("environment")
    '    Dim BZ As New NestBase.BazeZnalosti(env)

    '    BZ.lo()

    '    Dim resultxml As String
    '    Dim result As New QuestionFromBazeResult
    '    If BZ.GetQuestionFromBaze(BazeZnalostiXML, resultxml) Then
    '        result.isOK = True
    '        result.AnswersXML = resultxml
    '    Else
    '        result.isOK = False
    '        result.errorMessage = BZ.LastError.UserMessage & " : " & BZ.LastError.InnerMessage
    '    End If
    '    Return result
    'End Function
End Class

End Namespace
