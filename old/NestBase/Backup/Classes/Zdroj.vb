Public MustInherit Class Zdroj
    Inherits GeneralObject
    Private mTyp As Enums.enmTypZdroje

    Public ReadOnly Property Typ() As Enums.enmTypZdroje
        Get
            Return mTyp
        End Get
    End Property
    Public Sub New(ByVal iEnvironment As Environment, ByVal iTyp As Enums.enmTypZdroje, ByVal atributy As Collection, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
        mTyp = iTyp
    End Sub

    Public MustOverride Function VyhodnotZdroj(ByRef uspesneVyhodnocen As Boolean, ByVal vyrok As Vyrok, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean, ByRef dalsiZdroj As Boolean, Optional ByVal Atribut As Atribut = Nothing) As Boolean
    
    Public Overridable Overloads Function SaveToXML(ByRef writer As Xml.XmlTextWriter, ByVal rozsahVah As Long) As Boolean
        Return True
    End Function

End Class
