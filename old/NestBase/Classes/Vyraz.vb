Public MustInherit Class Vyraz
    Inherits GeneralObject

    Protected mVaha As Interval
    Protected mStatus As Enums.enmStav
    ' Fweight_puvod: TInterval;
    '  Fstatus: integer;

#Region "Property"
    Public ReadOnly Property Status() As Enums.enmStav
        Get
            Return mStatus
        End Get
    End Property
    Public ReadOnly Property Vaha() As Interval
        Get
            Return mVaha
        End Get
    End Property
#End Region

    Public Sub New(ByVal iEnvironment As Environment, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mVaha = New Interval(defaultVaha)
        mStatus = Enums.enmStav.enmUntouched
    End Sub

    Public MustOverride Function PropojeniPredpokladu(ByVal pravidlo As Pravidlo) As Boolean
    Public MustOverride Function PropojeniKontextu(ByVal kontext As Kontext) As Boolean

    Public MustOverride Function GetVypis(ByRef text As String, ByVal typVypisu As Enums.enmTypVypisu, ByVal rozsahVah As Long) As Boolean
    Public MustOverride Function VyhodnotBackward(ByRef uspesneVyhodnoceno As Boolean, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean) As Boolean
    
    Public MustOverride Function SaveToXML(ByRef writer As Xml.XmlTextWriter) As Boolean
End Class
