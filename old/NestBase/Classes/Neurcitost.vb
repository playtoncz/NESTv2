Public MustInherit Class Neurcitost
    Inherits GeneralObject

    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
    End Sub

    Public Function NEG(ByVal interval As Interval) As Interval 'Negace intervalu
        Dim result As New Interval
        result.MinHodnota = -interval.MaxHodnota
        result.MaxHodnota = -interval.MinHodnota
        Return result
    End Function
    Public Function CONJ(ByVal interval1 As Interval, ByVal interval2 As Interval) As Interval 'Konjunkce
        Dim result As New Interval
        result.MinHodnota = Math.Min(interval1.MinHodnota, interval2.MinHodnota)
        result.MaxHodnota = Math.Min(interval1.MaxHodnota, interval2.MaxHodnota)
        Return result
    End Function
    Public Function DISJ(ByVal interval1 As Interval, ByVal interval2 As Interval) As Interval 'Disjunkce
        Dim result As New Interval
        result.MinHodnota = Math.Max(interval1.MinHodnota, interval2.MinHodnota)
        result.MaxHodnota = Math.Max(interval1.MaxHodnota, interval2.MaxHodnota)
        Return result
    End Function
    Public MustOverride Function CTR(ByVal a As Interval, ByVal interval1 As Interval) As Interval 'pro výpoèet pøíspìvku pravidla a vyhodnocování kontextu u pravidla
    Public MustOverride Function GLOB(ByVal Intervaly As Collection) As Interval 'Pro skládání pøíspìvkù více pravidel
    'function IMP(aI1: TInterval; aI2: TInterval): TInterval; virtual;  //Pro vyhodnocování integritních omezení
    Public Function NORM(ByVal interval As Interval) As Interval 'normuje interval na rozsah -0.99 ; 0.99
        Dim rozsah As Double
        Dim str As String
        str = "0" + Environment.DecimalSeparator
        Dim i As Integer
        For i = 0 To Environment.PocetDesetinnychMist - 1
            str += "9"
        Next
        rozsah = CDbl(Helper.upravDesetinneTeckyCarky(str))

        Dim result As New Interval
        result.MinHodnota = NormujCislo(interval.MinHodnota, rozsah)
        result.MaxHodnota = NormujCislo(interval.MaxHodnota, rozsah)
        Return result
    End Function

    Public Function IMP(ByVal interval1 As Interval, ByVal interval2 As Interval) As Interval
        Dim int As New Interval
        int.MinHodnota = IMP1(interval1.MinHodnota, interval2.MinHodnota)
        int.MaxHodnota = IMP1(interval1.MaxHodnota, interval2.MaxHodnota)
        Return int
    End Function
    Private Function IMP1(ByVal w1 As Double, ByVal w2 As Double) As Double
        If w1 <= 0 Then
            Return 0
        Else
            Return Math.Max(0, Math.Min(1, w1 - w2))
        End If
    End Function

    Private Function NormujCislo(ByVal cislo As Double, ByVal rozsah As Double) As Double
        Return Math.Min(Math.Max(cislo, -rozsah), rozsah)
    End Function

End Class
