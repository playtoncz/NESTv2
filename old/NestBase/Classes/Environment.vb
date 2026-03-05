Public Class Environment
    Public vahaUnknown As Interval
    Public vahaIrrelevant As Interval
    Public vahaTrue As Interval
    Public vahaFalse As Interval
    Public DecimalSeparator As String
    Public PocetDesetinnychMist As Long

    Public ResourceManagerCZ As System.Resources.ResourceManager
    Public ResourceManagerEN As System.Resources.ResourceManager

    
    Public Sub New()
        vahaUnknown = New Interval(-1, 1)
        vahaIrrelevant = New Interval(0, 0)
        vahaTrue = New Interval(1, 1)
        vahaFalse = New Interval(-1, -1)
        UrciDecimalSeparator()
        PocetDesetinnychMist = 3
        ResourceManagerCZ = New System.Resources.ResourceManager("NestBase.Texty_cz", GetType(Environment).Assembly)
        ResourceManagerEN = New System.Resources.ResourceManager("NestBase.Texty_en", GetType(Environment).Assembly)
        ResourceManagerCZ.IgnoreCase = True
        ResourceManagerEN.IgnoreCase = True
    End Sub

    Private Sub UrciDecimalSeparator()
        Try
            Dim dbl As Double
            dbl = CDbl("1.1")
            DecimalSeparator = "."
        Catch ex As Exception
            DecimalSeparator = ","
        End Try
    End Sub
End Class
