Imports System.Data

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.BL

''' <summary>
''' XXマスタメンテ
''' </summary>
Public Partial Class CM_CMSM010F02
    Inherits CMBaseEntryForm
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

        ' 共通設定
        Master.Body.Attributes.Remove("onload")

        ' 操作モードを設定
        Dim subName As String = SetOpeMode(PanelKeyItems, PanelSubItems, PanelUpdateInfo, PanelFunction, Nothing, BtnCommit, _
            BtnCancel)

        ' 画面ヘッダ初期化
        Master.Title = "組織マスタメンテ　" & subName

        ' 以下は初期表示以外は処理しない
        If IsPostBack Then
            Return
        End If

        Try
            ' 区分値の検索
            Dim kbnTable As DataTable = CommonBL.SelectKbn("M001")

            ' 区分値の設定
            Dim dv As New DataView(kbnTable, "分類CD = 'M001'", Nothing, DataViewRowState.CurrentRows)

            ' 組織階層区分のアイテム設定
            Dim table As DataTable = dv.ToTable()
            組織階層区分.DataSource = table
        Catch ex As Exception
            ShowError(ex)
        End Try

        ' 標準の画面表示処理実行
        OnPageOnLoad(Master.Body, m_facade)
    End Sub

    ''' <summary>
    ''' 登録ボタン押下
    ''' </summary>
    Protected Sub BtnCommit_Click(sender As Object, e As EventArgs)
        ' 標準の登録ボタン押下時処理実行
        OnCommitClick(Master.Body, m_facade)
    End Sub

    ''' <summary>
    ''' キャンセルボタン押下
    ''' </summary>
    Protected Sub BtnCancel_Click(sender As Object, e As EventArgs)
        ' 標準のキャンセルボタン押下時処理実行
        OnCancelClick(Master.Body)
    End Sub
    #End Region

    #Region "overrideメソッド"
    ''' <summary>
    ''' キーデータ文字列から検索パラメータを作成する。
    ''' </summary>
    ''' <param name="argKey">キーデータ文字列</param>
    ''' <returns>検索パラメータ</returns>
    Protected Overrides Function CreateSelectParam(argKey As String) As List(Of CMSelectParam)
        Dim param As New List(Of CMSelectParam)()
        param.Add(New CMSelectParam("組織CD", "= @組織CD", argKey))
        Return param
    End Function

    ''' <summary>
    ''' データが変更されているかチェックする。
    ''' </summary>
    ''' <returns>True:変更あり, False:変更なし</returns>
    Protected Overrides Function IsModified() As Boolean
        ' 新規の場合、キー項目チェック
        If OpeMode = "Insert" AndAlso IsPanelModified(PanelKeyItems) Then
            Return True
        End If

        ' 従属項目チェック
        If IsPanelModified(PanelSubItems) Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' 新規行にデフォルト値を設定する。
    ''' </summary>
    ''' <param name="argRow">デフォルト値を設定するDataRow</param>
    Protected Overrides Sub SetDefaultValue(argRow As DataRow)
        argRow("組織階層区分") = CMInformationManager.UserInfo.SoshikiKaisoKbn
    End Sub

    ''' <summary>
    ''' DataRowに入力データを設定する。
    ''' </summary>
    ''' <returns>True:エラーあり, False:エラーなし</returns>
    Protected Overrides Function SetInputRow() As Boolean
        Dim hasError As Boolean = False

        ' 新規の場合
        If OpeMode = "Insert" Then
            SetPanelInputRow(PanelKeyItems)
        End If

        ' 従属項目値を設定
        SetPanelInputRow(PanelSubItems)

        ' エラー有無を返却
        Return hasError
    End Function
    #End Region
End Class
