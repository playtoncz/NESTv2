

Public Class GeneralError
    Private mNumber As Long
    Private mUserMessage As String
    Private mInnerMessage As String
    Private mException As Exception

#Region "Property"
    Public Property Number() As Long
        Get
            Return mNumber
        End Get
        Set(ByVal Value As Long)
            mNumber = Value
        End Set
    End Property

    Public Property UserMessage() As String
        Get
            Return mUserMessage
        End Get
        Set(ByVal Value As String)
            mUserMessage = Value
        End Set
    End Property

    Public Property InnerMessage() As String
        Get
            If mInnerMessage <> "" Then Return mInnerMessage
            If Not mException Is Nothing Then Return mException.Message
        End Get
        Set(ByVal Value As String)
            mInnerMessage = Value
        End Set
    End Property

    Public Property Exception() As Exception
        Get
            Return mException
        End Get
        Set(ByVal Value As Exception)
            mException = Value
        End Set
    End Property
#End Region


    Public Sub New()

    End Sub

    Public Function Save() As Boolean
        '++++
        Return True
    End Function
End Class


