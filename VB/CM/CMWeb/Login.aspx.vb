Imports Seasar.Quill

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL

''' <summary>
''' ログイン画面
''' </summary>
''' <remarks>
''' ログイン、ログアウト処理を実施
''' </remarks>
''' <author></author>
''' <date>2006/03/31</date>
''' <version>新規作成</version>
Public Partial Class Login
    Inherits System.Web.UI.Page
    #Region "インジェクション用フィールド"
    Protected m_authenticationBL As ICMAuthenticationBL
    #End Region

    #Region "コンストラクタ"
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    Public Sub New()
        ' インジェクション実行
        Dim injector As QuillInjector = QuillInjector.GetInstance()
        injector.Inject(Me)
    End Sub
    #End Region

    #Region "イベントハンドラ"
    ''' <summary>
    ''' Page Init(画面初期化)
    ''' </summary>
    Protected Sub Page_Init(sender As Object, e As EventArgs)
        ' このページはユーザのログイン状態が確定していないため、ユーザの操作によっては
        ' 同一ページの表示中にユーザのログイン状態が変化する可能性がある。
        ' ユーザのログイン状態が変化すると、ViewStateUserKeyの状態が不正となるため、
        ' このページでは明示的に Nothing を設定し、ユーザ状態による検証を無効にしている。
        ViewStateUserKey = Nothing
    End Sub

    ''' <summary>
    ''' Page Load(画面表示）
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' 画面タイトル設定
        Master.Title = "ログイン"
    End Sub

    ''' <summary>
    ''' 「ログイン」ボタン　Click
    ''' </summary>
    Protected Sub ButtonLogin_Click(sender As Object, e As EventArgs)
        ' ユーザＩＤ、パスワードによる認証確認
        Dim userId As String = TextBoxUserId.Text.ToUpper().Trim()
        Dim password As String = TextBoxPassword.Text.Trim()
        Dim authenticated As Boolean = m_authenticationBL.Authenticate(userId, password)
        If authenticated Then
            ' フォーム認証チケットの発行
            FormsAuthentication.SetAuthCookie(userId, False)

            ' メニュー画面へ遷移
            'Response.Redirect("~/CM/CMSM010F01.aspx")
            Response.Redirect("Menu.aspx")
        Else
            ' 認証失敗時エラーメッセージの表示
            Master.ShowMessage("E", "IDまたはパスワードが不正です。")
        End If
    End Sub
    #End Region
End Class
