Imports System.Data

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.BL

''' <summary>
''' 組織マスタメンテ
''' </summary>
Public Partial Class CM_CMSM010F01
    Inherits CMBaseListForm
    #Region "BLインジェクション用フィールド"
    Protected m_facade As CMSM010BL
    #End Region

    #Region "イベントハンドラ"
    ''' <summary>
    ''' ページロード
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' 組織ＣＤ、組織階層区分を設定
        KaishaCd.Value = CMInformationManager.UserInfo.SoshikiCd
        SoshikiLayer.Value = CMInformationManager.UserInfo.SoshikiKaisoKbn

        ' 機能ボタン スクリプト登録
        AddFuncOnclick(BtnSelect, BtnCsvOut, BtnInsert, BtnUpdate, BtnDelete)
        ' 画面ヘッダ初期化
        Master.Title = "組織マスタメンテ"

        ' 初期表示以外は処理しない
        If IsPostBack Then
            Return
        End If

        ' 更新許可を取得
        Dim canUpdate As Boolean = m_commonBL.GetRangeCanUpdate(System.IO.Path.GetFileNameWithoutExtension(Me.AppRelativeVirtualPath), False)

        Try
            ' 区分値の検索
            Dim kbnTable As DataTable = CommonBL.SelectKbn("M001")

            ' 区分値の設定
            Dim dv As New DataView(kbnTable, "分類CD = 'M001'", Nothing, DataViewRowState.CurrentRows)

            ' 組織階層区分のアイテム設定
            Dim table As DataTable = dv.ToTable()
            table.Rows.InsertAt(table.NewRow(), 0)
            組織階層区分.DataSource = table
            組織階層区分.DataBind()
        Catch ex As Exception
            ShowError(ex)
        End Try

        ' 操作履歴を出力
        WriteOperationLog()
    End Sub

    ''' <summary>
    ''' 検索、CSV出力ボタン押下
    ''' </summary>
    Protected Sub Select_Command(sender As Object, e As CommandEventArgs)
        ' 検索パラメータ取得
        Dim param As List(Of CMSelectParam) = CreateSelectParam(PanelCondition)

        Dim hasError As Boolean = False

        ' 検索
        If e.CommandName = "Select" Then
            hasError = DoSelect(m_facade, param, GridView1)

            ' 正常終了の場合
            If Not hasError Then
                ' 検索実行を記憶
                Selected.Value = "true"
                ' 検索条件を記憶
                Session("SelectCondition") = param
            End If
        ' CSV出力
        ElseIf e.CommandName = "CsvOut" Then
            Dim url As String = Nothing
            hasError = DoCsvOut(m_facade, param, url)
            ' 結果表示
            If Not hasError Then
                OpenExcel(url)
            End If
        End If
    End Sub

    ''' <summary>
    ''' 新規、修正、削除ボタン押下
    ''' </summary>
    Protected Sub OpenEntryForm_Command(sender As Object, e As CommandEventArgs)
        ' 検索が実行されている場合、再検索
        If Selected.Value.Length > 0 Then
            ' 検索条件取得
            Dim param As List(Of CMSelectParam) = DirectCast(Session("SelectCondition"), List(Of CMSelectParam))
            ' 検索実行
            DoSelect(m_facade, param, GridView1, GridView1.PageIndex)
        End If

        ' メッセージを設定
        If e.CommandName = "Delete" Then
            Master.ShowMessage("I", CMMessageManager.GetMessage("IV004"))
        Else
            Master.Body.Attributes.Remove("onload")
        End If
    End Sub

    ''' <summary>
    ''' データバインド
    ''' </summary>
    Protected Sub GridView1_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowIndex >= 0 Then
            Dim row As DataRowView = DirectCast(e.Row.DataItem, DataRowView)
        End If
    End Sub

    ''' <summary>
    ''' ページ切り替え
    ''' </summary>
    Protected Sub GridView1_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)
        ' 検索条件取得
        Dim param As List(Of CMSelectParam) = DirectCast(Session("SelectCondition"), List(Of CMSelectParam))
        ' 検索実行
        DoSelect(m_facade, param, GridView1, e.NewPageIndex)
    End Sub
    #End Region
End Class
