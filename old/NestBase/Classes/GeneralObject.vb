Public Class GeneralObject
    Private mLastError As GeneralError
    Private mEnvironment As Environment
    Private mLanguage As Enums.enmLanguage

#Region "Property"
    Public Property Environment() As Environment
        Get
            Return mEnvironment
        End Get
        Set(ByVal Value As Environment)
            mEnvironment = Value
        End Set
    End Property
    Public ReadOnly Property LastError() As GeneralError
        Get
            Return mLastError
        End Get
    End Property
    Public ReadOnly Property Language() As Enums.enmLanguage
        Get
            Return mLanguage
        End Get
    End Property


#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        mEnvironment = iEnvironment
        mLanguage = iLanguage
    End Sub

    Public Function GetText(ByVal value As String) As String
        Dim result As String = ""
        Select Case mLanguage
            Case Enums.enmLanguage.cestina
                result = mEnvironment.ResourceManagerCZ.GetString(value)
                If result = "" Then result = "Neznámý text"
            Case Enums.enmLanguage.anglictina
                result = mEnvironment.ResourceManagerEN.GetString(value)
                If result = "" Then result = "Unknown text"
        End Select
        Return result
    End Function

    Public Function GetResourceManager() As Resources.ResourceManager
        Select Case mLanguage
            Case Enums.enmLanguage.cestina
                Return mEnvironment.ResourceManagerCZ
            Case Enums.enmLanguage.anglictina
                Return mEnvironment.ResourceManagerEN
        End Select
        Return Nothing
    End Function

    Public Function SetError(ByVal UserMessage As String, Optional ByVal InnerMessage As String = "") As Boolean
        Dim err As New GeneralError
        err.UserMessage = UserMessage
        err.InnerMessage = InnerMessage
        err.Save()
        mLastError = err
        Return False
    End Function

    Public Function SetError(ByVal Number As Long, Optional ByVal UserMessage As String = "", Optional ByVal InnerMessage As String = "") As Boolean
        Dim err As New GeneralError
        err.Number = Number
        err.UserMessage = UserMessage
        err.InnerMessage = InnerMessage
        err.Save()
        mLastError = err
        Return False
    End Function

    'Public Function SetError(ByVal ex As Exception, Optional ByVal Number As Long = 0, Optional ByVal UserMessage As String = "", Optional ByVal InnerMessage As String = "") As Boolean
    '    Dim err As New GeneralError
    '    err.Exception = ex
    '    err.Number = Number
    '    err.UserMessage = UserMessage
    '    err.InnerMessage = InnerMessage
    '    err.Save()
    '    mLastError = err
    '    Return False
    'End Function
    Public Function SetError(ByVal ex As Exception, Optional ByVal UserMessage As String = "", Optional ByVal InnerMessage As String = "") As Boolean
        Dim err As New GeneralError
        err.Exception = ex
        err.UserMessage = UserMessage
        err.InnerMessage = InnerMessage
        err.Save()
        mLastError = err
        Return False
    End Function
    Public Function SetError(ByVal generalError As GeneralError) As Boolean
        mLastError = generalError
        Return False
    End Function

    Public Function AppendLastError(ByVal UserMessage As String, Optional ByVal InnerMessage As String = "") As Boolean
        If Not mLastError Is Nothing Then
            mLastError.UserMessage += UserMessage
            mLastError.InnerMessage += InnerMessage
        End If
        Return False
    End Function
    Public Function AppendLastError(ByVal generalError As GeneralError, ByVal UserMessage As String, Optional ByVal InnerMessage As String = "") As Boolean
        mLastError = generalError
        mLastError.UserMessage += UserMessage
        mLastError.InnerMessage += InnerMessage

        Return False
    End Function
End Class
