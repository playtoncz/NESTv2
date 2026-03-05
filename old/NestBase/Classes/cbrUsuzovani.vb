Public MustInherit Class cbrUsuzovani
    Inherits GeneralObject

    Protected MustOverride Function VzdalenostVyroku(ByVal vyrok1 As Vyrok, ByVal vyrok2 As Vyrok, ByRef vzdalenost As Interval) As Boolean
    Protected MustOverride Function VzdalenostPripadu(ByVal pripad1 As Pripad, ByVal pripad2 As Pripad, ByVal vahyAtributu As Collection, ByRef vzdalenost As Interval) As Boolean


    Public MustOverride Function SpoctiVahyZaveru(ByVal novyPripad As Pripad, ByVal seznamPripadu As Collection, ByVal vahyAtributu As Collection, ByRef PripadVzdalenost As Collection) As Boolean


    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, iLanguage)
    End Sub
End Class
