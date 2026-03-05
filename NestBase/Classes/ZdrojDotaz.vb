Public Class ZdrojDotaz
    Inherits Zdroj

    Public Sub New(ByVal iEnvironment As Environment, ByVal atributy As Collection, ByVal defaultVaha As Interval, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, Enums.enmTypZdroje.enmDotaz, atributy, defaultVaha, iLanguage)
    End Sub

    Public Overrides Function VyhodnotZdroj(ByRef uspesneVyhodnocen As Boolean, ByVal vyrok As Vyrok, ByVal neurcitost As Neurcitost, ByVal aktSeznamDotazu As Collection, ByVal atributy As Collection, ByVal bezdotazu As Boolean, ByRef dalsiZdroj As Boolean, Optional ByVal Atribut As Atribut = Nothing, Optional ByRef PripadVzdalenost As Collection = Nothing) As Boolean
        If bezdotazu Then
            dalsiZdroj = False
            uspesneVyhodnocen = False
            Return True
        End If

        If aktSeznamDotazu Is Nothing Then
            uspesneVyhodnocen = True
            Return True
        End If

        If Not vyrok.Pozice = Enums.enmPozice.enmQuestion Then
            uspesneVyhodnocen = True
            Return True
        End If
        If Not Helper.ValueIsInCollection(aktSeznamDotazu, vyrok.RodicovskyAtribut.Id) Then
            aktSeznamDotazu.Add(vyrok.RodicovskyAtribut, vyrok.RodicovskyAtribut.Id)
        End If


        uspesneVyhodnocen = True
        Return True

        'Return Me.SetError("Vyhodnoceni zdroje Dotaz neni udelane")
    End Function
End Class
