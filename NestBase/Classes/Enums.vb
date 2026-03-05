Public Class Enums
    Enum enmLanguage
        cestina = 1
        anglictina = 2
    End Enum

    Enum enmInferencniMechanismus
        Standardni = 1
        Logicky = 2
        Neuronovy = 3
        Hybridni = 4
    End Enum

    Public Shared Function TxtEnmInferencniMechanismus(ByVal inferencniMechanismus As enmInferencniMechanismus, ByVal resourceManager As Resources.ResourceManager) As String
        Select Case inferencniMechanismus
            Case enmInferencniMechanismus.Standardni
                Return Gettext("Enums_standardni", resourceManager)
            Case enmInferencniMechanismus.Logicky
                Return Gettext("Enums_logicky", resourceManager)
            Case enmInferencniMechanismus.Neuronovy
                Return Gettext("Enums_neuronovy", resourceManager)
            Case enmInferencniMechanismus.Hybridni
                Return Gettext("Enums_hybridni", resourceManager)
        End Select
        Return ""
    End Function

    Enum enmPriorita
        First = 0
        Last = 1
        MinLength = 2
        MaxLength = 3
        User = 4
    End Enum

    Public Shared Function TxtEnmPriorita(ByVal priorita As enmPriorita, ByVal resourceManager As Resources.ResourceManager) As String
        Select Case priorita
            Case enmPriorita.First
                Return Gettext("Enums_od_zacatku", resourceManager)
            Case enmPriorita.Last
                Return Gettext("Enums_od_konce", resourceManager)
            Case enmPriorita.MinLength
                Return Gettext("Enums_minimalni_delka", resourceManager)
            Case enmPriorita.MaxLength
                Return Gettext("Enums_maximalni_delka", resourceManager)
            Case enmPriorita.User
                Return Gettext("Enums_uzivatelska", resourceManager)

        End Select
        Return ""
    End Function

    Enum enmTypAtributu
        enmBinary = 1
        enmSingle = 2
        enmMultiple = 3
        enmNumeric = 4
    End Enum
    Public Shared Function TxtEnmTypAtributu(ByVal typAtributu As enmTypAtributu, ByVal resourceManager As Resources.ResourceManager) As String
        Select Case typAtributu
            Case enmTypAtributu.enmBinary
                Return Gettext("Enums_binary", resourceManager)
            Case enmTypAtributu.enmSingle
                Return Gettext("Enums_single", resourceManager)
            Case enmTypAtributu.enmMultiple
                Return Gettext("Enums_multiple", resourceManager)
            Case enmTypAtributu.enmNumeric
                Return Gettext("Enums_numeric", resourceManager)
        End Select
        Return ""
    End Function
    Public Shared Function TxtEnmTypAtributuBasic(ByVal typAtributu As enmTypAtributu) As String
        Select Case typAtributu
            Case enmTypAtributu.enmBinary
                Return "binary"
            Case enmTypAtributu.enmSingle
                Return "single"
            Case enmTypAtributu.enmMultiple
                Return "multiple"
            Case enmTypAtributu.enmNumeric
                Return "numeric"
        End Select
        Return ""
    End Function

    Enum enmTypVyroku
        enmBinary = 1
        enmSingle = 2
        enmMultiple = 3
        enmNumeric = 4
    End Enum

    Enum enmTypPravidla
        enmKompozicionalni = 1
        enmLogicke = 2
        enmApriori = 3
    End Enum
    Public Shared Function TxtEnmTypPravidla(ByVal typPravidla As enmTypPravidla, ByVal resourceManager As Resources.ResourceManager) As String
        Select Case typPravidla
            Case enmTypPravidla.enmKompozicionalni
                Return Gettext("Enums_kompozicionalni", resourceManager)
            Case enmTypPravidla.enmLogicke
                Return Gettext("Enums_logicke", resourceManager)
            Case enmTypPravidla.enmApriori
                Return Gettext("Enums_apriori", resourceManager)
        End Select
        Return ""
    End Function

    Enum enmTypVypisu
        enmText = 1
        enmHTML = 2
    End Enum

    Enum enmTypRadkyVypisu
        enmNormal = 0
        enmNadpis1 = 1
        enmNadpis2 = 2
        enmCara = 3
        enmOdsazeniStart = 4
        enmOdsazeniEnd = 5
        enmNadpis3 = 6
        enmAName = 7
    End Enum

    Enum enmStav
        enmUntouched = 0    '(nebyl pokus o vyhonoceni)	na pocatku
        enmPartial = 3      '(castecne vyhodnoceno)		neco z podrizenych je partial nebo untouched
        enmProvisional = 2  '(odlozeno)			vse podrizene jen final nebo provisional
        enmFinal = 1        '(plne vyhodnoceno)		vse podrizene final
        enmLogical = 4      'vyrok je vyhodnocen pomoci logical pravidla s vahou false
        enmError = 5        'integritni omezeni
    End Enum
    Public Shared Function TxtEnmStav(ByVal stav As enmStav, ByVal resourceManager As Resources.ResourceManager) As String
        Select Case stav
            Case enmStav.enmUntouched
                Return Gettext("Enums_untouched", resourceManager)
            Case enmStav.enmPartial
                Return Gettext("Enums_partial", resourceManager)
            Case enmStav.enmProvisional
                Return Gettext("Enums_provisional", resourceManager)
            Case enmStav.enmFinal
                Return Gettext("Enums_final", resourceManager)
            Case enmStav.enmLogical
                Return Gettext("Enums_logical", resourceManager)
        End Select
        Return ""
    End Function

    Enum enmPozice
        enmNothing = 0
        enmQuestion = 1
        enmIntermediate = 2
        enmGoal = 3
        enmAlone = 4
    End Enum

    Enum enmTypZdroje
        enmOdvozovani = 1
        enmDotaz = 2
        enmVypocet = 3
        enmImplicitniVaha = 4
        enmCBR = 5
    End Enum

    Private Shared Function Gettext(ByVal value As String, ByVal resourceManager As Resources.ResourceManager) As String
        Return resourceManager.GetString(value)
    End Function
End Class
