

Public Class konzultace
    Inherits GeneralObject

    Public Enum enmRezimKonzultace
        dialog = 1
        dotaznik = 2
        dotaznikSDialogem = 3
    End Enum


    Public BZ As NestBase.BazeZnalosti
    Public BazeZnalostiXML As String
    Public NcsXML As String
    Public nahranaBaze As Boolean
    Public rozbehnutaKonzultace As Boolean
    Private mVolani As Boolean = False
    Public odpovediXML As String
    Public Language As NestBase.Enums.enmLanguage
    Public RezimKonzultace As enmRezimKonzultace

    Public Sub New(ByVal ienvironment As NestBase.Environment, ByVal iLanguage As NestBase.Enums.enmLanguage)
        MyBase.New(ienvironment)
        Language = iLanguage
        BZ = New NestBase.BazeZnalosti(ienvironment, Language)
        nahranaBaze = False
        rozbehnutaKonzultace = False
    End Sub

    Public Function IsQuestion(ByVal resultFromBase As String, ByRef dotazovanyAtribut As NestBase.Atribut, ByRef jeDotaz As Boolean) As Boolean
        Dim xmlDoc As New System.Xml.XmlDocument
        Try
            xmlDoc.LoadXml(resultFromBase)
        Catch ex As Exception
            Return Me.SetError(ex, , "Nepodaøilo se naèíst xml dokument")
        End Try

        If xmlDoc.SelectNodes("//results/questions/attribute").Count = 0 Then
            jeDotaz = False
            Return True
        End If
        Dim element As System.Xml.XmlElement
        element = xmlDoc.SelectSingleNode("//results/questions/attribute/id")
        dotazovanyAtribut = BZ.Atributy(element.InnerText)
        jeDotaz = True
        Return True
    End Function

    Public Function Konzultuj(ByRef RedirectString As String, ByRef result As String, ByRef id_dotazu As String, Optional ByVal PrvniVolani As Boolean = False, Optional ByRef pripadyVahy As String = "") As Boolean

        rozbehnutaKonzultace = True

        Dim odpovedi As String
        If PrvniVolani Then
            BZ = New NestBase.BazeZnalosti(Environment, Language)
            If Not BZ.GetQuestionFromBaze(BazeZnalostiXML, odpovedi, NcsXML) Then
                Return Me.SetError(BZ.LastError.UserMessage)
            End If
            If odpovediXML <> "" Then
                If Not BZ.LoadAnswersFromXML(odpovediXML) Then
                    Return Me.SetError(BZ.LastError.UserMessage)
                End If
                odpovedi = odpovediXML
            End If
            mVolani = True
        Else
            If Not BZ.GetQuestionFromBaze("", odpovedi, NcsXML) Then
                Return Me.SetError(BZ.LastError.UserMessage)
            End If
        End If


        ''Konzultace pres webservis
        'Dim ws As New nestws.NestWS
        'Dim consultationresult As nestws.ConsultationResult
        'consultationresult = ws.RunConsultation(BazeZnalostiXML, odpovedi)

        'BZ = New NestBase.BazeZnalosti(Me.Environment, BazeZnalostiXML, odpovedi)

        'If consultationresult.isOK Then
        '    'End If
        '    'If BZ.RunConsultation(BazeZnalostiXML, odpovedi, result) Then
        '    result = consultationresult.ResultXML
        '    Dim dotaz As NestBase.Atribut
        '    Dim jeDotaz As Boolean
        '    If Not IsQuestion(result, dotaz, jeDotaz) Then
        '        Return False
        '    End If
        '    If jeDotaz Then

        '        'presmerovani na dotaz

        '        id_dotazu = dotaz.Id
        '        Select Case dotaz.Typ
        '            Case NestBase.Enums.enmTypAtributu.enmBinary
        '                RedirectString = "dotazBinary.aspx"
        '            Case NestBase.Enums.enmTypAtributu.enmSingle
        '                RedirectString = "dotazSingle.aspx"
        '            Case NestBase.Enums.enmTypAtributu.enmMultiple
        '                RedirectString = "dotazMultiple.aspx"
        '            Case NestBase.Enums.enmTypAtributu.enmNumeric
        '                RedirectString = "dotazNumeric.aspx"
        '        End Select
        '        Return True
        '    Else
        '        'presmerovani na vysledek
        '        RedirectString = "result.aspx"
        '        Return True

        '    End If


        'konzultace pres base
        Dim BZ2 As New NestBase.BazeZnalosti(Environment, Me.Language)
        If BZ2.RunConsultation(BazeZnalostiXML, odpovedi, result, NcsXML, pripadyVahy) Then
            Dim dotaz As NestBase.Atribut
            Dim jeDotaz As Boolean
            If Not IsQuestion(result, dotaz, jeDotaz) Then
                Return False
            End If
            If jeDotaz Then

                'presmerovani na dotaz

                id_dotazu = dotaz.Id
                Select Case dotaz.Typ
                    Case NestBase.Enums.enmTypAtributu.enmBinary
                        RedirectString = "dotazBinary.aspx"
                    Case NestBase.Enums.enmTypAtributu.enmSingle
                        RedirectString = "dotazSingle.aspx"
                    Case NestBase.Enums.enmTypAtributu.enmMultiple
                        RedirectString = "dotazMultiple.aspx"
                    Case NestBase.Enums.enmTypAtributu.enmNumeric
                        RedirectString = "dotazNumeric.aspx"
                End Select
                Return True
            Else
                'presmerovani na vysledek
                RedirectString = "result.aspx"
                Return True

            End If
        Else
            Return Me.SetError(BZ2.LastError.UserMessage)
        End If


    End Function

    Public Function NahrajBZ(Optional ByVal pridat As Boolean = False) As Boolean
        nahranaBaze = BZ.LoadBaseFromXML(Me.BazeZnalostiXML, pridat, NcsXML)
        If Not BZ.SaveBaseToXML(Me.BazeZnalostiXML) Then
            Return False
        End If
        Return nahranaBaze
    End Function

    Public Function GetOdpovedi() As String


        Dim odpovedi As String = ""
        If mVolani Then
            If Not BZ.GetQuestionFromBaze("", odpovedi, NcsXML) Then
                Return Me.SetError(BZ.LastError.UserMessage)
            End If
        Else
            BZ = New NestBase.BazeZnalosti(Environment, Language)
            If Not BZ.GetQuestionFromBaze(Me.BazeZnalostiXML, odpovedi, NcsXML) Then
                Return Me.SetError(BZ.LastError.UserMessage)
            End If
            mVolani = True
        End If

        Return odpovedi
    End Function

End Class



