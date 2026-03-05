
Partial Class BZToServer
    Inherits cBasePage

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here
    End Sub

    Private Sub cmdNahraj_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdNahraj.Click
        Dim fileLen As Integer
        Dim str As New System.Text.StringBuilder

        fileLen = efile.PostedFile.ContentLength
        Dim Input(fileLen) As Byte
        efile.PostedFile.InputStream.Read(Input, 0, fileLen)


        str.Append(System.Text.Encoding.GetEncoding("windows-1250").GetString(Input, 0, fileLen))

        Dim bzStr As String
        bzStr = str.ToString

        ' Create an instance of StreamWriter to write text to a file.
        Dim sw As IO.StreamWriter = New IO.StreamWriter(Server.MapPath("~\BZ\" & efile.PostedFile.FileName))
        ' Add some text to the file.
        sw.Write(bzStr)

        sw.Close()


    End Sub
End Class

