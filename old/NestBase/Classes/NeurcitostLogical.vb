Public Class NeurcitostLogical
    Inherits Neurcitost

    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
    End Sub



    Public Overrides Function CTR(ByVal a As Interval, ByVal interval1 As Interval) As Interval
        Dim result As New Interval
        result.MinHodnota = CTRjedna(a.MinHodnota, interval1.MinHodnota)
        result.MaxHodnota = CTRjedna(a.MaxHodnota, interval1.MaxHodnota)
        Return result
    End Function
    Private Function CTRjedna(ByVal a As Double, ByVal w As Double) As Double
        If a > 0 Then
            Return Math.Sign(w) * Math.Max(0, a + Math.Abs(w) - 1)
        Else
            Return 0
        End If
    End Function



    Public Overrides Function GLOB(ByVal Intervaly As Microsoft.VisualBasic.Collection) As Interval
        Dim result As New Interval
        Dim kladne As Double = 0
        For Each interval As Interval In Intervaly
            If interval.MinHodnota > 0 Then kladne += interval.MinHodnota
        Next
        kladne = Math.Min(kladne, 1)
        Dim zaporne As Double = 0
        For Each interval As Interval In Intervaly
            If interval.MinHodnota < 0 Then zaporne += interval.MinHodnota
        Next
        zaporne = Math.Max(zaporne, -1)
        result.MinHodnota = kladne + zaporne

        kladne = 0
        For Each interval As Interval In Intervaly
            If interval.MaxHodnota > 0 Then kladne += interval.MaxHodnota
        Next
        kladne = Math.Min(kladne, 1)
        zaporne = 0
        For Each interval As Interval In Intervaly
            If interval.MaxHodnota < 0 Then zaporne += interval.MaxHodnota
        Next
        zaporne = Math.Max(zaporne, -1)
        result.MaxHodnota = kladne + zaporne

        Return result
    End Function
    
End Class
