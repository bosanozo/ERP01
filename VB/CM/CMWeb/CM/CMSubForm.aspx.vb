Imports System.Data
Imports System.Reflection
Imports System.Web.UI.WebControls

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.BL

''' <summary>
''' 選択子画面
''' </summary>
Public Partial Class CMSubForm
    Inherits CMBaseListForm
    #Region "private変数"
    Private m_codeName As String
    #End Region

    #Region "イベントハンドラ"
    ''' <summary>
    ''' ページロード
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' 機能ボタン スクリプト登録
        BtnSelect.Attributes.Add("onclick", "return CheckInputList()")

        ' 以下は初期表示以外は処理しない
        If IsPostBack Then
            Return
        End If

        ' コード値幅調整
        Code.MaxLength = Convert.ToInt32(Request.Params("CodeLen"))
        Code.Width = Code.MaxLength * 8 + 2

        ' 検索コード名
        m_codeName = Regex.Replace(Request.Params("Code"), "(From|To)", "")

        ' グリッドの列名設定
        GridView1.Columns(1).HeaderText = GetCodeLabel()
        GridView1.Columns(2).HeaderText = GetNameLabel()

        ' データバインド実行
        DataBind()
    End Sub

    ''' <summary>
    ''' 検索ボタン押下
    ''' </summary>
    Protected Sub Select_Command(sender As Object, e As CommandEventArgs)
        ' 画面の条件を取得
        Dim formParam As List(Of CMSelectParam) = CreateSelectParam(PanelCondition)

        ' 項目名の置き換え
        For Each p As CMSelectParam In formParam
            If p.name = "Code" Then
                p.name = If(String.IsNullOrEmpty(Request.Params("DbCodeCol")), GridView1.Columns(1).HeaderText.Replace("コード", "CD"), Request.Params("DbCodeCol"))
            ElseIf p.name = "Name" Then
                p.name = If(String.IsNullOrEmpty(Request.Params("DbNameCol")), GridView1.Columns(2).HeaderText, Request.Params("DbNameCol"))
                p.condtion = "LIKE @" & Convert.ToString(p.name)
                p.paramFrom = "%" & Convert.ToString(p.paramFrom) & "%"
            End If
        Next

        ' 検索パラメータ作成
        Dim param As New List(Of CMSelectParam)()

        ' 追加パラメータがある場合、追加する
        If Not String.IsNullOrEmpty(Request.Params("Params")) Then
            For Each p As String In Request.Params("Params").Split()
                Dim value As Object

                ' "#"から始まる場合はUserInfoから設定
                If p(0) = "#"C Then
                    Dim pi As PropertyInfo = CMInformationManager.UserInfo.[GetType]().GetProperty(p.Substring(1))
                    value = pi.GetValue(CMInformationManager.UserInfo, Nothing)
                Else
                    ' セルの値を取得
                    value = p
                End If

                ' パラメータ追加
                param.Add(New CMSelectParam(Nothing, Nothing, value))
            Next
        End If

        ' 画面の条件を追加
        param.AddRange(formParam)

        Dim hasError As Boolean = DoSelect(param, GridView1)

        ' 正常終了の場合
        If Not hasError Then
            ' 検索条件を記憶
            Session("SelectCondition") = param
        End If
    End Sub

    ''' <summary>
    ''' ページ切り替え
    ''' </summary>
    Protected Sub GridView1_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)
        ' 検索条件取得
        Dim param As List(Of CMSelectParam) = DirectCast(Session("SelectCondition"), List(Of CMSelectParam))
        ' 検索実行
        DoSelect(param, GridView1, e.NewPageIndex)
    End Sub
    #End Region

    #Region "protectedメソッド"
    ''' <summary>
    ''' コードのラベル文字列を返す。
    ''' </summary>
    ''' <returns>コードのラベル文字列</returns>
    Protected Function GetCodeLabel() As String
        Return m_codeName.Replace("CD", "コード")
    End Function

    ''' <summary>
    ''' 名称のラベル文字列を返す。
    ''' </summary>
    ''' <returns>名称のラベル文字列</returns>
    Protected Function GetNameLabel() As String
        Return Regex.Replace(m_codeName, "(CD|ID)", "名")
    End Function
    #End Region

    #Region "privateメソッド"
    ''' <summary>
    ''' 検索を実行する。
    ''' </summary>
    ''' <param name="argParam">検索条件パラメータ</param>
    ''' <param name="argGrid">一覧表示用グリッド</param>
    ''' <param name="argPage">ページ</param>
    ''' <returns>True:エラーあり, False:エラーなし</returns>
    Private Overloads Function DoSelect(argParam As List(Of CMSelectParam), argGrid As GridView, Optional argPage As Integer = 0) As Boolean
        Try
            ' ファサードの呼び出し
            Dim message As CMMessage = Nothing
            Dim result As DataTable = CommonBL.SelectSub(Request.Params("SelectId"), argParam, message)

            ' 返却メッセージの表示
            If message IsNot Nothing Then
                ShowMessage(message)
            End If

            Dim idx As Integer = 0
            For Each col As DataControlField In argGrid.Columns
                ' 列ヘッダ設定
                If TypeOf col Is BoundField Then
                    Dim bf As BoundField = TryCast(col, BoundField)
                    If idx > 1 Then
                        bf.HeaderText = result.Columns(idx).ColumnName
                    End If
                    bf.DataField = result.Columns(idx).ColumnName
                    idx += 1
                End If
            Next

            ' DataSource設定
            argGrid.DataSource = result
            ' ページセット
            argGrid.PageIndex = argPage
            ' バインド
            argGrid.DataBind()
        Catch ex As Exception
            ' DataSourceクリア
            argGrid.DataSource = Nothing
            argGrid.DataBind()

            ShowError(ex)
            Return True
        End Try

        Return False
    End Function
    #End Region
End Class
