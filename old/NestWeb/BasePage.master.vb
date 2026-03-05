
Partial Class BasePage
    Inherits System.Web.UI.MasterPage


    Public konzultace As konzultace
    Public BodyScripts As String = ""
    Public strankaKonzultace As Boolean = False
    Private ResourceManagerCZ As System.Resources.ResourceManager
    Private ResourceManagerEN As System.Resources.ResourceManager
    Public userSetting As UserSetting

    Public Property ErrorMessage() As String
        Get
            Return lblError.Text
        End Get
        Set(ByVal Value As String)
            lblError.Text = Value
        End Set
    End Property

    'Protected Overridable Sub InitializeCulture()
    '    System.Threading.Thread.CurrentThread.CurrentUICulture = New System.Globalization.CultureInfo("en-US")
    '    System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US")

    'End Sub

    
    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        lblError.Text = ""

        'ResourceManagerCZ = New System.Resources.ResourceManager("NestWeb.Texty_cz", GetType(BasePage).Assembly)
        'ResourceManagerEN = New System.Resources.ResourceManager("NestWeb.Texty_en", GetType(BasePage).Assembly)
        '        Page.Culture = "en-us"
        '       Page.UICulture = "en"
        '        System.Threading.Thread.CurrentThread.CurrentUICulture = New System.Globalization.CultureInfo("en-US")
        '       System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US")

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        userSetting = Session("userSetting")
        konzultace = Session("konzultace")

        If Request.QueryString("pokracovatvkonzultaci") = "1" Then
            Dim redirectString As String
            Dim id_dotazu As String
            Dim result As String
            If konzultace.Konzultuj(redirectString, result, id_dotazu) Then
                Session("konzultace") = konzultace
                Session("id_dotazu") = id_dotazu
                Session("result") = result
                Response.Redirect(redirectString)
            Else
                CType(Me.Master, BasePage).ErrorMessage = konzultace.LastError.UserMessage
            End If
        End If
        UkazTlacitka()
    End Sub

    Public Sub UkazTlacitka(Optional ByVal NastavitTrue As Boolean = False)

        Dim menuVypis As Boolean
        If NastavitTrue Then
            menuVypis = True
        Else
            If konzultace Is Nothing Then
                menuVypis = False
            Else
                menuVypis = konzultace.nahranaBaze
            End If
        End If
        menu_vypis.Visible = menuVypis
        menu_vypis_submenu.Visible = menuVypis
        menu_vypis_init.Visible = menuVypis

        menu_odstranit.Visible = menuVypis
        menu_statistiky.Visible = menuVypis
        If strankaKonzultace Or konzultace Is Nothing Then
            'lbPokracovatVKonzultaci.Visible = False
            pokracovatVKonzultaci_item.Visible = False
        Else
            'lbPokracovatVKonzultaci.Visible = konzultace.rozbehnutaKonzultace
            pokracovatVKonzultaci_item.Visible = konzultace.rozbehnutaKonzultace
        End If



    End Sub

    Public Function GetBodyScript() As String
        Return BodyScripts
    End Function

    'Protected Sub lbPokracovatVKonzultaci_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lbPokracovatVKonzultaci.Click
    '    Dim redirectString As String
    '    Dim id_dotazu As String
    '    Dim result As String
    '    If konzultace.Konzultuj(redirectString, result, id_dotazu) Then
    '        Session("konzultace") = konzultace
    '        Session("id_dotazu") = id_dotazu
    '        Session("result") = result
    '        Response.Redirect(redirectString)
    '    Else
    '        CType(Me.Master, BasePage).ErrorMessage = konzultace.LastError.UserMessage
    '    End If
    'End Sub
    Public Function GetText(ByVal value As String) As String
        Dim result As String = ""
        Select Case userSetting.Language
            Case NestBase.Enums.enmLanguage.cestina
                result = ResourceManagerCZ.GetString(value)
                If result = "" Then result = "Neznámý text"
            Case NestBase.Enums.enmLanguage.anglictina
                result = ResourceManagerEN.GetString(value)
                If result = "" Then result = "Unknown text"
        End Select
        Return result
    End Function



    Protected Sub cmdJazyk_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdJazyk.Click
        If Me.userSetting.Language = NestBase.Enums.enmLanguage.cestina Then
            Me.userSetting.Language = NestBase.Enums.enmLanguage.anglictina
        Else
            Me.userSetting.Language = NestBase.Enums.enmLanguage.cestina
        End If
        Session("userSetting") = userSetting
        Response.Redirect("default.aspx?clear=true")
    End Sub
End Class

