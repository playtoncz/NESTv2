
Partial Class wucOdpovedi
    Inherits System.Web.UI.UserControl

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

    Private HodnotyTlacitek As Collection
    Private TextyTlacitek As Collection
    Private typyAtributu As Collection
    Public TypAtributu As String

    Public VybranaHodnota As String
    Public Event OdpovedClick()

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here
        HodnotyTlacitek = Session("hodnotyTlacitek")
        TextyTlacitek = Session("textyTlacitek")
        typyAtributu = Session("typyAtributu")
        If Not Page.IsPostBack Then
            Dim pocet As Long = 0
            For Each str As String In TextyTlacitek
                pocet += 1
                Dim ta As String = typyAtributu(pocet)
                Select Case pocet
                    Case 1
                        cmd1.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd1.Visible = False
                    Case 2
                        cmd2.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd2.Visible = False
                    Case 3
                        cmd3.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd3.Visible = False
                    Case 4
                        cmd4.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd4.Visible = False
                    Case 5
                        cmd5.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd5.Visible = False
                    Case 6
                        cmd6.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd6.Visible = False
                    Case 7
                        cmd7.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd7.Visible = False
                    Case 8
                        cmd8.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd8.Visible = False
                    Case 9
                        cmd9.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd9.Visible = False
                    Case 10
                        cmd10.Text = str
                        If ta.IndexOf(TypAtributu) = -1 Then cmd10.Visible = False
                End Select
            Next
            For i As Integer = TextyTlacitek.Count + 1 To 10
                Select Case i
                    Case 1
                        cmd1.Visible = False
                    Case 2
                        cmd2.Visible = False
                    Case 3
                        cmd3.Visible = False
                    Case 4
                        cmd4.Visible = False
                    Case 5
                        cmd5.Visible = False
                    Case 6
                        cmd6.Visible = False
                    Case 7
                        cmd7.Visible = False
                    Case 8
                        cmd8.Visible = False
                    Case 9
                        cmd9.Visible = False
                    Case 10
                        cmd10.Visible = False
                End Select
            Next
        End If
    End Sub


    Private Sub cmd1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd1.Click
        VybranaHodnota = HodnotyTlacitek(1)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd2.Click
        VybranaHodnota = HodnotyTlacitek(2)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd3.Click
        VybranaHodnota = HodnotyTlacitek(3)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd4.Click
        VybranaHodnota = HodnotyTlacitek(4)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd5.Click
        VybranaHodnota = HodnotyTlacitek(5)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd6.Click
        VybranaHodnota = HodnotyTlacitek(6)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd7.Click
        VybranaHodnota = HodnotyTlacitek(7)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd8.Click
        VybranaHodnota = HodnotyTlacitek(8)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd9.Click
        VybranaHodnota = HodnotyTlacitek(9)
        RaiseEvent OdpovedClick()
    End Sub

    Private Sub cmd10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd10.Click
        VybranaHodnota = HodnotyTlacitek(10)
        RaiseEvent OdpovedClick()
    End Sub
End Class

