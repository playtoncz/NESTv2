Public Class Interval
    
    Private mMinHodnota As Double
    Private mMaxHodnota As Double

#Region "Property"
    Public Property MinHodnota() As Double
        Get
            Return mMinHodnota
        End Get
        Set(ByVal Value As Double)
            mMinHodnota = Value
        End Set
    End Property

    Public Property MaxHodnota() As Double
        Get
            Return mMaxHodnota
        End Get
        Set(ByVal Value As Double)
            mMaxHodnota = Value
        End Set
    End Property
#End Region

    Public Sub New()
        Me.SetValue(0, 0)
    End Sub
    Public Sub New(ByVal interval As Interval)
        Me.SetValue(interval)        
    End Sub
    Public Sub New(ByVal iMinHodnota As Double, ByVal iMaxHodnota As Double)
        Me.SetValue(iMinHodnota, iMaxHodnota)        
    End Sub

    Public Sub SetValue(ByVal iMinHodnota As Double, ByVal iMaxHodnota As ValueType)
        mMinHodnota = iMinHodnota
        mMaxHodnota = iMaxHodnota
    End Sub
    Public Sub SetValue(ByVal interval As Interval)
        mMinHodnota = interval.MinHodnota
        mMaxHodnota = interval.MaxHodnota
    End Sub
    Public Function JeRovno(ByVal interval As Interval) As Boolean
        If mMinHodnota = interval.MinHodnota Then
            If mMaxHodnota = interval.MaxHodnota Then
                Return True
            End If
        End If
        Return False
    End Function
    Public Function ToStr(Optional ByVal rozsahVah As Long = 1, Optional ByVal prevadetNaText As Boolean = False) As String
        If prevadetNaText And mMinHodnota = 0 And mMaxHodnota = 0 Then Return "irrelevant"
        If prevadetNaText And mMinHodnota = -1 And mMaxHodnota = 1 Then Return "unknown"
        If mMinHodnota = mMaxHodnota Then
            Return Helper.FloatToStr(mMinHodnota * rozsahVah)
        Else
            Return Helper.FloatToStr(mMinHodnota * rozsahVah) & ";" & Helper.FloatToStr(mMaxHodnota * rozsahVah)
        End If
    End Function
    Public Function JeVNorme() As Boolean
        If mMinHodnota >= -1 And mMaxHodnota <= 1 Then Return True
        Return False
    End Function
    Public Function JeVetsiNez(ByVal Interval2 As Interval) As Boolean
        If Me.MinHodnota > Interval2.MinHodnota Then Return True
        If Me.MinHodnota < Interval2.MinHodnota Then Return False
        If Me.MaxHodnota > Interval2.MaxHodnota Then Return True
        Return False
    End Function
End Class
