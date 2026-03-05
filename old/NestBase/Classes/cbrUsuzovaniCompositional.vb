Public Class cbrUsuzovaniCompositional
    Inherits cbrUsuzovani

    Public Overrides Function SpoctiVahyZaveru(ByVal novyPripad As Pripad, ByVal seznamPripadu As Collection, ByVal vahyAtributu As Collection, ByRef PripadVzdalenost As Collection) As Boolean
        Try


            For Each atribut As Atribut In novyPripad.Cile
                For Each vyrok As Vyrok In atribut.Vyroky
                    vyrok.Vaha.SetValue(0, 0)
                    vyrok.Status = Enums.enmStav.enmUntouched
                Next
            Next
            Dim pocetKladnychPripadu As Long = 0

            PripadVzdalenost = New Collection

            For Each staryPripad As Pripad In seznamPripadu
                Dim vzdalenost As Interval = Nothing
                If Not Me.VzdalenostPripadu(novyPripad, staryPripad, vahyAtributu, vzdalenost) Then
                    Return False
                End If
                Dim pv As New PripadVzdalenost
                pv.pripad = staryPripad
                pv.vzdalenost = vzdalenost
                PripadVzdalenost.Add(pv)
                If vzdalenost.MaxHodnota > 0 Then
                    pocetKladnychPripadu += 1
                    For i As Integer = 1 To novyPripad.Cile.Count
                        Dim atribut1 As Atribut = novyPripad.Cile(i)
                        Dim atribut2 As Atribut = staryPripad.Cile(i)
                        For u As Integer = 1 To atribut1.Vyroky.Count
                            Dim vyrok1 As Vyrok = atribut1.Vyroky(u)
                            Dim vyrok2 As Vyrok = atribut2.Vyroky(u)
                            vyrok1.Vaha.MinHodnota += vyrok2.Vaha.MinHodnota * vzdalenost.MinHodnota
                            vyrok1.Vaha.MaxHodnota += vyrok2.Vaha.MaxHodnota * vzdalenost.MaxHodnota
                        Next
                    Next
                End If
            Next
            For Each atribut As Atribut In novyPripad.Cile
                For Each vyrok As Vyrok In atribut.Vyroky
                    vyrok.Status = Enums.enmStav.enmFinal
                    If pocetKladnychPripadu > 0 Then
                        vyrok.Vaha.MinHodnota = vyrok.Vaha.MinHodnota / pocetKladnychPripadu
                        vyrok.Vaha.MaxHodnota = vyrok.Vaha.MaxHodnota / pocetKladnychPripadu
                    End If
                Next
            Next
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("CbrUsuzovaniCompositional_Chyba_pri_pocitani_vahy_zaveru_u_cbr_compositional"))
        End Try
    End Function

    Protected Overrides Function VzdalenostPripadu(ByVal pripad1 As Pripad, ByVal pripad2 As Pripad, ByVal vahyAtributu As Collection, ByRef vzdalenost As Interval) As Boolean
        Try
            Dim soucetVahAtributu As New Interval(0, 0)
            Dim soucetVah As New Interval(0, 0)
            For i As Integer = 1 To pripad1.Atributy.Count
                Dim atribut1 As Atribut = pripad1.Atributy(i)
                Dim atribut2 As Atribut = pripad2.Atributy(i)
                If Not atribut1.Vaha.JeRovno(Environment.vahaIrrelevant) Then
                    Dim vahaAtributu As Interval = vahyAtributu(atribut1.Id)
                    soucetVahAtributu.MinHodnota += vahaAtributu.MinHodnota
                    soucetVahAtributu.MaxHodnota += vahaAtributu.MaxHodnota
                    For u As Integer = 1 To atribut1.Vyroky.Count
                        Dim vyrok1 As Vyrok = atribut1.Vyroky(u)
                        Dim vyrok2 As Vyrok = atribut2.Vyroky(u)
                        Dim vzVyroku As New Interval
                        If Not Me.VzdalenostVyroku(vyrok1, vyrok2, vzVyroku) Then
                            Return False
                        End If
                        soucetVah.MinHodnota += (vzVyroku.MinHodnota * vahaAtributu.MinHodnota) / atribut1.Vyroky.Count
                        soucetVah.MaxHodnota += (vzVyroku.MaxHodnota * vahaAtributu.MaxHodnota) / atribut1.Vyroky.Count
                    Next
                End If
            Next
            vzdalenost = New Interval(0, 0)
            If soucetVahAtributu.MinHodnota <> 0 And soucetVahAtributu.MaxHodnota <> 0 Then
                vzdalenost.MinHodnota = soucetVah.MinHodnota / soucetVahAtributu.MinHodnota
                vzdalenost.MaxHodnota = soucetVah.MaxHodnota / soucetVahAtributu.MaxHodnota
            End If
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("CbrUsuzovaniCompositional_Chyba_pri_vypoctu_vzdalenosti_pripadu"))
        End Try
    End Function

    Protected Overrides Function VzdalenostVyroku(ByVal vyrok1 As Vyrok, ByVal vyrok2 As Vyrok, ByRef vzdalenost As Interval) As Boolean
        Try
            Dim a As Double
            Dim b As Double
            If (vyrok1.Vaha.MinHodnota > vyrok2.Vaha.MaxHodnota) Or (vyrok2.Vaha.MinHodnota > vyrok1.Vaha.MaxHodnota) Then
                'intervaly se neprekryvaji
                a = 1 - Math.Abs(vyrok1.Vaha.MinHodnota - vyrok2.Vaha.MaxHodnota)
                b = 1 - Math.Abs(vyrok1.Vaha.MaxHodnota - vyrok2.Vaha.MinHodnota)
            Else
                'intervaly se prekryvaji
                Dim x1 As Double
                Dim x2 As Double
                Dim x3 As Double
                Dim x4 As Double
                x1 = Math.Abs(vyrok1.Vaha.MinHodnota - vyrok2.Vaha.MinHodnota)
                x2 = Math.Abs(vyrok1.Vaha.MinHodnota - vyrok2.Vaha.MaxHodnota)
                x3 = Math.Abs(vyrok1.Vaha.MaxHodnota - vyrok2.Vaha.MinHodnota)
                x4 = Math.Abs(vyrok1.Vaha.MaxHodnota - vyrok2.Vaha.MaxHodnota)
                a = 1 - Math.Max(x1, Math.Max(x2, Math.Max(x3, x4)))
                b = 1
            End If
            vzdalenost = New Interval(a, b)
            Return True
        Catch ex As Exception
            Return Me.SetError(ex, GetText("CbrUsuzovaniCompositional_Chyba_pri_vypoctu_vzdalenosti_dvou_vyroku"))
        End Try
    End Function

    Public Sub New(ByVal iEnvironment As Environment, ByVal iLanguage As Enums.enmLanguage)
        MyBase.New(iEnvironment, ilanguage)
    End Sub

    Public Class PripadVzdalenost
        Public pripad As Pripad
        Public vzdalenost As Interval
    End Class
End Class
