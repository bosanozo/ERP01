Imports NEXS.ERP.CM.Common

''' <summary>
''' 表示タイプ
''' </summary>
Public Enum DisplayType
    ''' <summary>
    ''' テキストボックス
    ''' </summary>
    TextBox
    ''' <summary>
    ''' ラベル
    ''' </summary>
    Label
End Enum

''' <summary>
''' マスタページ
''' </summary>
Public Partial Class CMMaster
    Inherits System.Web.UI.MasterPage
    Private m_operationTime As DateTime

    #Region "publicプロパティ"
    ''' <summary>
    ''' ページタイトル表示エリアへ、タイトルを表示します
    ''' </summary>
    Public Property Title() As String
        Get
            Return LabelTitle.Text
        End Get
        Set
            LabelTitle.Text = value
        End Set
    End Property

    ''' <summary>操作時刻</summary>
    Public Property OperationTime() As DateTime
        Get
            Return m_operationTime
        End Get
        Set
            m_operationTime = value
            LabelDateTime.Text = value.ToString("yyyy/MM/dd HH:mm")
        End Set
    End Property

    ''' <summary>画面ＩＤ</summary>
    Public Property FormId() As String
        Get
            Return LabelFormId.Text
        End Get
        Set
            LabelFormId.Text = value
        End Set
    End Property

    Public ReadOnly Property Body() As HtmlGenericControl
        Get
            Return Body1
        End Get
    End Property
    #End Region

    #Region "publicメソッド"
    ''' <summary>
    ''' メッセージ表示エリアへ、メッセージを表示します。
    ''' </summary>
    ''' <remarks>
    ''' メッセージ種別がエラーの場合は赤文字で表示、ガイドの場合は青文字で表示します
    ''' 表示文字列はHTMLエンコードします
    ''' また、表示文字列中に改行があった場合は、ｂｒタグで置換します。
    ''' </remarks>
    ''' <param name="argType">メッセージ種別</param>
    ''' <param name="argMessage">表示するメッセージ文字列</param>
    Public Sub ShowMessage(argType As String, argMessage As String)
        Dim method As String = Nothing

        Select Case argType(0)
            Case "E"C
                method = "MsgError"
                Exit Select
            Case "W"C
                method = "alert"
                Exit Select
            Case "I"C
                method = "MsgInfo"
                Exit Select
        End Select

        ' ダイアログ表示
        'method,
        Body.Attributes.Add("onload", String.Format("{0}('{1}')", "alert", argMessage.Replace(vbCr & vbLf, "\n")))

        #If HtmlMessage Then
        Dim message As String = Server.HtmlEncode(argMessage)
        message = message.Replace(vbCr & vbLf, "<br/>")
        message = message.Replace(vbCr, "<br/>")
        message = message.Replace(vbLf, "<br/>")

        Select Case argType(0)
            Case "E"C
                LabelMessage.ForeColor = Color.Red
                Exit Select
            Case "W"C
                LabelMessage.ForeColor = Color.Yellow
                Exit Select
            Case "I"C
                LabelMessage.ForeColor = Color.Black
                Exit Select
        End Select

        LabelMessage.Text = message
        #End If
    End Sub
    #End Region

    #Region "イベントハンドラ"
    ''' <summary>
    ''' ページの ViewStateUserKey を設定します。
    ''' </summary>
    Protected Sub Page_Init(sender As Object, e As EventArgs)
        Dim user As CMUserInfo = CMInformationManager.UserInfo
        If user IsNot Nothing Then
            ' ユーザIDとセッションIDを、ページの ViewStateUserKey に設定
            ' （ユーザとセッションを検証するため）
            Page.ViewStateUserKey = user.Id + ":" & Session.SessionID
        End If
    End Sub

    ''' <summary>
    ''' ログインユーザのID、名称を表示します
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' ユーザIDの設定
        Dim user As CMUserInfo = CMInformationManager.UserInfo

        ' ユーザ名称の設定
        LabelUserName.Text = If(user IsNot Nothing, Server.HtmlEncode(user.Name), "")

        ' 操作時刻の設定
        OperationTime = DateTime.Now

        ' 画面ＩＤの設定
        FormId = "【" & System.IO.Path.GetFileNameWithoutExtension(Page.AppRelativeVirtualPath) & "】"

        ' メッセージクリア
        LabelMessage.Text = ""
    End Sub
    #End Region
End Class
