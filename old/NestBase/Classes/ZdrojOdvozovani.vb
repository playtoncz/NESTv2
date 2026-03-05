Public Class ZdrojOdvozovani
    Inherits Zdroj

    Public Sub New(ByVal iEnvironment As Environment, ByVal atributy As Collection, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, Enums.enmTypZdroje.enmOdvozovani, atributy, defaultVaha, iLanguage)
    End Sub

    Public Overrides Function VyhodnotZdroj(ByRef uspesneVyhodnocen As Boolean, ByVal vyrok As Vyrok, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean, ByRef dalsiZdroj As Boolean, Optional ByVal Atribut As Atribut = Nothing, Optional ByRef PripadVzdalenost As Collection = Nothing) As Boolean
        Try
            If Not vyrok.OdvodVyrokBackward(uspesneVyhodnocen, neurcitost, aktSeznamDotazu, atributy, bezdotazu) Then
                Return Me.SetError(vyrok.LastError)
            End If

            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("ZdrojOdvozovani_Chyba_ve_zdroji_odvozovani_pro_vyrok") & vyrok.Id)
        End Try

    End Function
End Class
