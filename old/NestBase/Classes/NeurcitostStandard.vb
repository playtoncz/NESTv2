Public Class NeurcitostStandard
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
            Return a * w
        Else
            Return 0
        End If
    End Function
        


    Public Overrides Function GLOB(ByVal Intervaly As Microsoft.VisualBasic.Collection) As Interval
        Dim result As New Interval
        Dim hodnota As Double = 0
        Dim interval As Interval
        For Each interval In Intervaly
            hodnota = GLOBjedna(hodnota, interval.MinHodnota)
        Next
        result.MinHodnota = hodnota
        hodnota = 0
        For Each interval In Intervaly
            hodnota = GLOBjedna(hodnota, interval.MaxHodnota)
        Next
        result.MaxHodnota = hodnota
        Return result
    End Function
    Private Function GLOBjedna(ByVal v As Double, ByVal w As Double) As Double
        If ((v = 1) And (w = -1)) Or ((v = -1) And (w = 1)) Then
            Return 0
        Else
            Return (w + v) / (1 + w * v)
        End If

    End Function

End Class
