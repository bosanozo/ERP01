Imports System.Data

Imports DocumentFormat.OpenXml
Imports SpreadsheetLight

Imports NEXS.ERP.CM
Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.DA
Imports NEXS.ERP.CM.BL

''' <summary>
''' 組織マスタEXCEL入力
''' </summary>
Public Partial Class CM_CMSM010F03
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

        ' 画面ヘッダ初期化
        Master.Title = "組織マスタEXCEL入力"

        ' 初期表示以外は処理しない
        If IsPostBack Then
            Return
        End If

        ' 操作履歴を出力
        WriteOperationLog()
    End Sub

    ''' <summary>
    ''' EXCEL入力ボタン押下
    ''' </summary>
    Protected Sub BtnExcelInput_Click(sender As Object, e As EventArgs)
        If Not FileUpload1.HasFile Then
            Return
        End If

        Try
            ' EXCEL文書を作成
            Dim xslDoc As New SLDocument(FileUpload1.PostedFile.InputStream)

            ' データセットにデータを取り込む
            Dim ds As New DataSet()

            ' シートでループ
            For Each sheet As String In xslDoc.GetSheetNames()
                ' シートを選択
                xslDoc.SelectWorksheet(sheet)

                ' データテーブル作成
                Dim table As DataTable = CreateDataTableFromXml(sheet)

                Dim sheetStat = xslDoc.GetWorksheetStatistics()

                ' １行ずつ読み込み、先頭行はタイトルとして読み飛ばす
                For rowIdx As Integer = sheetStat.StartRowIndex + 1 To sheetStat.EndRowIndex
                    Dim newRow As DataRow = table.NewRow()
                    For colIdx As Integer = 0 To table.Columns.Count - 1
                        Dim col As Integer = colIdx + sheetStat.StartColumnIndex

                        ' 型に応じて値を取得する
                        Select Case table.Columns(colIdx).DataType.Name
                            Case "bool"
                                newRow(colIdx) = xslDoc.GetCellValueAsBoolean(rowIdx, col)
                                Exit Select

                            Case "decimal"
                                newRow(colIdx) = xslDoc.GetCellValueAsDecimal(rowIdx, col)
                                Exit Select

                            Case "long"
                                newRow(colIdx) = xslDoc.GetCellValueAsInt64(rowIdx, col)
                                Exit Select

                            Case "DateTime"
                                newRow(colIdx) = xslDoc.GetCellValueAsDateTime(rowIdx, col)
                                Exit Select
                            Case Else

                                newRow(colIdx) = xslDoc.GetCellValueAsString(rowIdx, col)
                                Exit Select
                        End Select
                    Next
                    table.Rows.Add(newRow)
                Next

                ' データテーブルを追加
                ds.Tables.Add(table)
            Next

            ' データセットを記憶
            Session("ImportDataSet") = ds

            ' DataSource設定
            GridView1.DataSource = ds.Tables(0)
            ' ページセット
            GridView1.PageIndex = 0
            ' バインド
            GridView1.DataBind()
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    ''' <summary>
    ''' 指定のXmlファイルからデータテーブルを作成する。
    ''' </summary>
    ''' <param name="argName">Xmlファイル名</param>
    ''' <returns>データテーブル</returns>
    Private Function CreateDataTableFromXml(argName As String) As DataTable
        ' データセットにファイルを読み込み
        Dim ds As New CMEntityDataSet()
        ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argName & ".xml"))

        ' データテーブル作成
        Dim table As New DataTable(ds.エンティティ(0).テーブル名)

        ' DataColumn追加
        For Each row As CMEntityDataSet.項目Row In ds.項目
            ' DataColumn作成
            Dim dcol As New DataColumn(row.項目名)
            ' 型
            Dim dbType As CMDbType = DirectCast([Enum].Parse(GetType(CMDbType), row.項目型), CMDbType)
            Select Case dbType
                Case CMDbType.フラグ
                    dcol.DataType = GetType(Boolean)
                    Exit Select
                Case CMDbType.金額, CMDbType.小数
                    dcol.DataType = GetType(Decimal)
                    Exit Select
                Case CMDbType.整数
                    dcol.DataType = GetType(Long)
                    Exit Select
                Case CMDbType.日付, CMDbType.日時
                    dcol.DataType = GetType(DateTime)
                    Exit Select
            End Select
            ' 必須入力
            If row.必須 Then
                dcol.AllowDBNull = False
            End If

            table.Columns.Add(dcol)
        Next

        Return table
    End Function

    ''' <summary>
    ''' 登録ボタン押下
    ''' </summary>
    Protected Sub BtnUpdate_Click(sender As Object, e As EventArgs)
        If Session("ImportDataSet") Is Nothing Then
            Return
        End If

        ' データセットを取得
        Dim ds As DataSet = DirectCast(Session("ImportDataSet"), DataSet)

        Try
            ' ファサードの呼び出し
            Dim operationTime As DateTime
            m_facade.Upload(ds, operationTime)
        Catch ex As Exception
            ShowError(ex)
        End Try
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
        'DoSelect(m_facade, param, GridView1, e.NewPageIndex)
    End Sub
    #End Region
End Class
