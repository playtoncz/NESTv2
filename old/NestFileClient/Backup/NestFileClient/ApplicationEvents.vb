Namespace My

    ' The following events are availble for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Function GetCommandLineArgs() As String()
            ' Declare variables.
            Dim separators As String = " "
            Dim commands As String = Microsoft.VisualBasic.Interaction.Command()
            Dim args() As String = commands.Split(separators.ToCharArray)
            Return args
        End Function

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            Dim BZFileName As String = "BZ.xml"
            Dim answersFileName As String = "answers.xml"
            Dim resultFileName As String = "result.xml"
            Try


                Dim args() As String
                args = GetCommandLineArgs()
                If args(0) <> "" Then
                    BZFileName = args(0)
                End If
                If args.Length > 1 Then
                    answersFileName = args(1)
                End If
                If args.Length > 2 Then
                    resultFileName = args(2)
                End If


                'nacteni ze souboru

                If LCase(Right(BZFileName, 4)) = ".zip" Then

                    ' Open a file that is to be loaded into a byte array
                    Dim oFile As System.IO.FileInfo
                    oFile = New System.IO.FileInfo(BZFileName)

                    Dim oFileStream As System.IO.FileStream = oFile.OpenRead()
                    Dim lBytes As Long = oFileStream.Length

                    Dim fileData(lBytes) As Byte

                    ' Read the file into a byte array
                    oFileStream.Read(fileData, 0, lBytes)
                    oFileStream.Close()


                    
                    'Dim bazeZnalostiXML() As Byte
                    Dim answersXML As String

                    'Dim sr As New IO.StreamReader(BZFileName)
                    'bazeZnalostiXML = sr.Read(
                    'sr.Close()



                    'sr = New IO.StreamReader(answersFileName)
                    'answersXML = sr.ReadToEnd
                    'sr.Close()



                    'zavolani WebService
                    Dim ws As New nestws.NestWS
                    ws.Timeout = 1200000
                    Dim consultationResult As nestws.ConsultationResult

                    If args.Length > 1 Then
                        Dim sr As New IO.StreamReader(answersFileName)
                        answersXML = sr.ReadToEnd
                        sr.Close()
                        consultationResult = ws.RunConsultationZip2(fileData, answersXML)
                    Else
                        consultationResult = ws.RunConsultationZip(fileData)
                    End If



                    Dim vysl As String
                    If consultationResult.isOK Then
                        vysl = consultationResult.ResultXML
                    Else
                        vysl = "<error>" & consultationResult.errorMessage & "</error>"
                    End If

                    'ulozeni vysledku do souboru
                    Dim sw As New IO.StreamWriter(resultFileName)
                    sw.Write(vysl)
                    sw.Close()
                Else



                    Dim bazeZnalostiXML As String
                    Dim answersXML As String

                    Dim sr As New IO.StreamReader(BZFileName)
                    bazeZnalostiXML = sr.ReadToEnd
                    sr.Close()

                    sr = New IO.StreamReader(answersFileName)
                    answersXML = sr.ReadToEnd
                    sr.Close()



                    'zavolani WebService
                    Dim ws As New nestws.NestWS
                    ws.Timeout = 1200000
                    Dim consultationResult As nestws.ConsultationResult
                    consultationResult = ws.RunConsultation(bazeZnalostiXML, answersXML)

                    Dim vysl As String
                    If consultationResult.isOK Then
                        vysl = consultationResult.ResultXML
                    Else
                        vysl = "<error>" & consultationResult.errorMessage & "</error>"
                    End If

                    'ulozeni vysledku do souboru
                    Dim sw As New IO.StreamWriter(resultFileName)
                    sw.Write(vysl)
                    sw.Close()
                End If

            Catch ex As Exception
                Dim sw As New IO.StreamWriter(resultFileName)
                sw.Write("<error>" & ex.Message & "</error>")
                sw.Close()
            End Try
        End Sub
    End Class

End Namespace

