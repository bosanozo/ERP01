Imports System
Imports System.IO
Imports System.Drawing
Imports System.Text
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls

Imports log4net
Imports Seasar.Quill

Imports DocumentFormat.OpenXml
Imports SpreadsheetLight

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL
Imports NEXS.ERP.CM.DA

Namespace WEB
    ''' <summary>
    ''' 画面の基底クラス
    ''' </summary>
    Public Class CMBaseForm
        Inherits Page
#Region "ロガーフィールド"
        Private m_logger As ILog
#End Region

#Region "インジェクション用フィールド"
        Protected m_commonBL As ICMCommonBL
#End Region

#Region "プロパティ"
        ''' <summary>
        ''' ロガー
        ''' </summary>
        Protected ReadOnly Property Log() As ILog
            Get
                Return m_logger
            End Get
        End Property

        ''' <summary>
        ''' 共通検索BL層
        ''' </summary>
        Protected ReadOnly Property CommonBL() As ICMCommonBL
            Get
                Return m_commonBL
            End Get
        End Property
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            ' ロガーを取得
            m_logger = LogManager.GetLogger(Me.[GetType]())

            ' インジェクション実行
            Dim injector As QuillInjector = QuillInjector.GetInstance()
            injector.Inject(Me)
        End Sub
#End Region

#Region "protectedメソッド"
#Region "メッセージ関連"
        ''' <summary>
        ''' 発生した例外をメッセージ表示する。
        ''' </summary>
        ''' <param name="argException">発生した例外</param>
        Protected Sub ShowError(argException As Exception)
            ' CMExceptionの場合
            If TypeOf argException Is CMException Then
                Dim ex As CMException = DirectCast(argException, CMException)

                ' ログの出力
                If ex.CMMessage IsNot Nothing AndAlso ex.CMMessage.MessageCd IsNot Nothing AndAlso ex.CMMessage.MessageCd.Length > 0 Then
                    If ex.CMMessage.MessageCd(0) = "E"c Then
                        Log.[Error](ex.CMMessage.ToString(), argException)
                    End If
                End If

                ' メッセージ表示
                ShowMessage(ex.CMMessage)
            Else
                ' その他の場合
                Dim msgCd As String = "EV001"

                If TypeOf argException Is FileNotFoundException Then
                    msgCd = "W"
                ElseIf TypeOf argException Is IOException Then
                    msgCd = "EV003"
                End If

                ' ログの出力
                If msgCd(0) = "E"c Then
                    Log.[Error](argException.Message, argException)
                End If

                ' メッセージ表示
                ShowMessage(msgCd, argException.Message)
            End If
        End Sub

        ''' <summary>
        ''' 指定されたメッセージコードのメッセージを表示する。
        ''' </summary>
        ''' <param name="argMessage">メッセージ</param>
        Protected Sub ShowMessage(argMessage As CMMessage)
            ShowMessage(argMessage.MessageCd, argMessage.Params)
        End Sub

        ''' <summary>
        ''' 指定されたメッセージコードのメッセージを表示する。
        ''' </summary>
        ''' <param name="argCode">メッセージコード</param>
        ''' <param name="argParams">パラメータ</param>
        Protected Sub ShowMessage(argCode As String, ParamArray argParams As Object())
            If DirectCast(Master, Object) IsNot Nothing Then
                DirectCast(Master, Object).ShowMessage(argCode, CMMessageManager.GetMessage(argCode, argParams))
            End If
        End Sub
#End Region

#Region "UI操作関連"
        ''' <summary>
        ''' 画面を閉じる
        ''' </summary>
        ''' <param name="argStatus">成功:True, 失敗:False</param>
        Protected Sub Close(argStatus As Boolean)
            Dim script As String = "<script language=JavaScript>" & "window.onLoad = window.returnValue = {0};" & "window.close()<" & "/" & "script>"

            ' スクリプト登録
            ClientScript.RegisterClientScriptBlock([GetType](), "Close", String.Format(script, argStatus.ToString().ToLower()))
        End Sub

        ''' <summary>
        ''' パネルを読み取り専用にする。
        ''' </summary>
        ''' <param name="argPanel">読み取り専用にするパネル</param>
        Protected Sub ProtectPanel(argPanel As Panel)
            For Each c As Control In argPanel.Controls
                If TypeOf c Is TextBox Then
                    Dim t As TextBox = DirectCast(c, TextBox)
                    ProtectTextBox(t)
                ElseIf TypeOf c Is DropDownList Then
                    Dim d As DropDownList = DirectCast(c, DropDownList)
                    d.Enabled = False
                    'if (d.Visible) d.Visible = false
                    d.BackColor = Color.FromName("#CCCCFF")
                ElseIf TypeOf c Is HtmlInputButton Then
                    Dim b As HtmlInputButton = DirectCast(c, HtmlInputButton)
                    b.Disabled = True
                End If
            Next
        End Sub

        ''' <summary>
        ''' テキストボックスを読み取り専用にする。
        ''' </summary>
        ''' <param name="argTextBox">読み取り専用にするテキストボックス</param>
        Protected Sub ProtectTextBox(argTextBox As TextBox)
            If argTextBox.[ReadOnly] Then
                Return
            End If

            'argTextBox.BorderStyle = BorderStyle.None
            argTextBox.BackColor = Color.FromName("#CCCCFF")
            'Color.Transparent
            argTextBox.[ReadOnly] = True
            argTextBox.TabIndex = -1
        End Sub

        ''' <summary>
        ''' コントロールに値が設定されているか返す。
        ''' </summary>
        ''' <param name="arg">コントロール</param>
        ''' <returns>True:設定あり, False:設定なし</returns>
        Protected Function IsSetValue(arg As WebControl) As Boolean
            Return TypeOf arg Is TextBox AndAlso DirectCast(arg, TextBox).Text.Trim().Length > 0 OrElse TypeOf arg Is DropDownList AndAlso DirectCast(arg, DropDownList).SelectedValue.Trim().Length > 0
        End Function

        ''' <summary>
        ''' コントロールに入力された文字列を返す。
        ''' </summary>
        ''' <param name="argControl">コントロール</param>
        ''' <returns>入力された文字列</returns>
        Protected Overridable Function GetValue(argControl As WebControl) As Object
            If TypeOf argControl Is TextBox Then
                If argControl.CssClass = "DateInput" Then
                    Return Convert.ToDateTime(DirectCast(argControl, TextBox).Text)
                Else
                    Return DirectCast(argControl, TextBox).Text
                End If
            End If

            If TypeOf argControl Is DropDownList Then
                Return DirectCast(argControl, DropDownList).SelectedValue
            End If

            Return ""
        End Function
#End Region

#Region "共通ファサード呼び出し"
        ''' <summary>
        ''' 操作履歴を出力する。
        ''' </summary>
        ''' <returns>True:エラーあり、False:エラーなし</returns>
        Protected Function WriteOperationLog() As Boolean
            Try
                ' 操作ログ記録
                CommonBL.WriteOperationLog(DirectCast(Master, Object).Title)
            Catch ex As Exception
                ShowError(ex)
                Return True
            End Try

            Return False
        End Function

        ''' <summary>
        ''' コード値から名称を取得する。
        ''' </summary>
        ''' <param name="argSelectId">検索種別</param>
        ''' <param name="argNotFound">True:名称取得失敗, False:名称取得成功</param>
        ''' <param name="argTextBox">名称表示テキストボックス</param>
        ''' <param name="argParams">パラメータ</param>
        ''' <returns>名称</returns>
        Protected Function GetCodeName(argSelectId As String, ByRef argNotFound As Boolean, argTextBox As TextBox, ParamArray argParams As Object()) As String
            Dim name As String = Nothing
            argNotFound = True

            Try
                ' 検索
                Dim result As DataTable = CommonBL.[Select](argSelectId, argParams)

                argNotFound = result Is Nothing OrElse result.Rows.Count = 0

                name = If(argNotFound, "コードエラー", result.Rows(0)(0).ToString())
                Dim cssClass As String = If(argNotFound, "transp warning", "1 transp")

                ' ラベルの設定
                argTextBox.Text = name
                argTextBox.CssClass = cssClass
            Catch ex As Exception
                ShowError(ex)
            End Try

            Return name
        End Function

        ''' <summary>
        ''' ドロップダウンリストにアイテムを設定する。
        ''' </summary>
        ''' <param name="argSelectId">検索種別</param>
        ''' <param name="argDDList">ドロップダウンリスト</param>
        ''' <param name="argParams">パラメータ</param>
        Protected Sub SetDropDownItems(argSelectId As String, argDDList As DropDownList, ParamArray argParams As Object())
            Try
                ' 検索
                Dim result As DataTable = CommonBL.[Select](argSelectId, argParams)

                ' 検索結果を設定
                argDDList.DataSource = result
            Catch ex As Exception
                ShowError(ex)
            End Try
        End Sub
#End Region

#Region "ドロップダウンリスト関連"
        ''' <summary>
        ''' ドロップダウンリストにアイテムを設定する。
        ''' ドロップダウンリストの最初に指定なしを挿入する。
        ''' </summary>
        ''' <param name="argSelectId">検索種別</param>
        ''' <param name="argDDList">ドロップダウンリスト</param>
        ''' <param name="argParams">パラメータ</param>
        Protected Sub SetDropDownItemsList(argSelectId As String, argDDList As DropDownList, ParamArray argParams As Object())
            SetDropDownItems(argSelectId, argDDList, argParams)
            InsertTopItem(argDDList, "指定なし")
        End Sub

        ''' <summary>
        ''' ドロップダウンリストにアイテムを設定する。
        ''' ドロップダウンリストの最初に指定なしを挿入する。
        ''' </summary>
        ''' <param name="argSelectId">検索種別</param>
        ''' <param name="argDDList">ドロップダウンリスト</param>
        ''' <param name="argParams">パラメータ</param>
        Protected Sub SetDropDownItemsEntry(argSelectId As String, argDDList As DropDownList, ParamArray argParams As Object())
            SetDropDownItems(argSelectId, argDDList, argParams)
            InsertTopItem(argDDList, "")
        End Sub

        ''' <summary>
        ''' ドロップダウンリストの最初にアイテムを挿入する。
        ''' </summary>
        ''' <param name="argDDList">ドロップダウンリスト</param>
        ''' <param name="argTopText">アイテム表示名</param>
        Protected Sub InsertTopItem(argDDList As DropDownList, argTopText As String)
            Dim table As DataTable = DirectCast(argDDList.DataSource, DataTable)
            Dim row As DataRow = table.NewRow()
            row(argDDList.DataTextField) = argTopText
            table.Rows.InsertAt(row, 0)
        End Sub

        ''' <summary>
        ''' ドロップダウンリストに時間アイテムを設定する。
        ''' </summary>
        ''' <param name="argDDList">ドロップダウンリスト</param>
        Protected Sub SetHourItems(argDDList As DropDownList)
            argDDList.Items.Add("")
            For i As Integer = 0 To 23
                argDDList.Items.Add(i.ToString("00"))
            Next
        End Sub

        ''' <summary>
        ''' ドロップダウンリストに分アイテムを設定する。
        ''' </summary>
        ''' <param name="argDDList">ドロップダウンリスト</param>
        Protected Sub SetMinuteItems(argDDList As DropDownList)
            argDDList.Items.Add("")
            For i As Integer = 0 To 11
                argDDList.Items.Add((i * 5).ToString("00"))
            Next
        End Sub

        ''' <summary>
        ''' ドロップダウンリストのアイテムの名称を取得する。
        ''' </summary>
        ''' <param name="argDDList">ドロップダウンリスト</param>
        ''' <returns>名称</returns>
        Protected Function GetItemName(argDDList As DropDownList) As String
            Dim text As String = argDDList.SelectedItem.Text
            Dim idx As Integer = text.IndexOf(" "c)
            Return If(idx >= 0, text.Substring(idx), "")
        End Function
#End Region

#Region "EXCEL入力"
        ''' <summary>
        ''' 指定のXmlファイルからデータテーブルを作成する。
        ''' </summary>
        ''' <param name="argName">Xmlファイル名</param>
        ''' <returns>データテーブル</returns>
        Protected Function CreateDataTableFromXml(argName As String) As DataTable
            ' データセットにファイルを読み込み
            Dim ds As New CMEntityDataSet()
            ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argName & ".xml"))

            ' データテーブル作成
            Dim table As New DataTable(ds.エンティティ(0).テーブル名)

            ' DataColumn追加
            For Each row As CMEntityDataSet.項目Row In ds.項目
                Dim col As String = If(String.IsNullOrEmpty(row.SourceColumn), row.項目名, row.SourceColumn)

                ' DataColumn作成
                Dim dcol As New DataColumn(col)
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
        ''' 指定StreamからDataSetにデータを取り込む。
        ''' </summary>
        ''' <param name="argInputStream">入力Stream</param>
        ''' <returns>データを取り込んだDataSet</returns>
        ''' <remarks>データを取り込むDataTableのスキーマはエンティティ定義XMLファイルより生成する。
        ''' シート名がXMLファイル名になる。</remarks>
        Protected Function ImportExcel(argInputStream As Stream) As DataSet
            ' EXCEL文書を作成
            Dim xslDoc As New SLDocument(argInputStream)

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

            Return ds
        End Function
#End Region

#Region "EXCEL出力"
        ''' <summary>
        ''' 指定されたデータセットの内容をEXCELファイルに出力する。
        ''' </summary>
        ''' <param name="argDataSet">データセット</param>
        ''' <param name="argPath">ファイル出力フルパス</param>
        ''' <returns>true:出力した, false:キャンセルした</returns>
        ''' <remarks>テンプレートファイルがある場合は、テンプレートファイルの書式に従って
        ''' データを出力する。出力開始位置はセルに"開始"と記述することで指定可能(無くても可)。
        ''' データを出力するシートはデータテーブル名とシート名が一致するものを使用する。
        ''' テンプレートファイルが無い場合は、デフォルトの形式でデータを出力する。</remarks>
        Protected Function ExportExcel(argDataSet As DataSet, argPath As String) As Boolean
            Dim xslDoc As SLDocument

            ' テンプレートファイル名作成
            Dim template As String = Path.Combine(Request.PhysicalApplicationPath, "Template", argDataSet.Tables(0).TableName + ".xlsx")

            If File.Exists(template) Then
                ' テンプレートを読み込み
                xslDoc = New SLDocument(template)

                For Each sheet As String In xslDoc.GetSheetNames()
                    Dim table As DataTable = argDataSet.Tables(sheet)
                    If table Is Nothing Then
                        Continue For
                    End If

                    ' シートを選択
                    xslDoc.SelectWorksheet(sheet)

                    Dim sheetStat = xslDoc.GetWorksheetStatistics()
                    Dim startRow As Integer = sheetStat.StartRowIndex + 1
                    Dim startCol As Integer = sheetStat.StartColumnIndex

                    ' 開始位置を検索
                    Dim cells = xslDoc.GetCells().Where(Function(c) c.Value.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString)
                    For Each cell As KeyValuePair(Of SLCellPoint, SLCell) In cells
                        If xslDoc.GetCellValueAsRstType(cell.Key.RowIndex, cell.Key.ColumnIndex).GetText() = "開始" Then
                            startRow = cell.Key.RowIndex
                            startCol = cell.Key.ColumnIndex
                            Exit For
                        End If
                    Next

                    ' スタイルを設定
                    For i As Integer = 0 To table.Columns.Count - 1
                        xslDoc.SetCellStyle(startRow + 1, i + startCol, table.Rows.Count + startRow - 1, i + startCol, xslDoc.GetCellStyle(startRow, i + startCol))
                    Next

                    ' データの出力
                    xslDoc.ImportDataTable(startRow, startCol, table, False)
                Next
            Else
                ' Bookを作成
                xslDoc = New SLDocument()

                For Each table As DataTable In argDataSet.Tables
                    ' シート名を追加
                    If xslDoc.GetCurrentWorksheetName() = SLDocument.DefaultFirstSheetName Then
                        xslDoc.RenameWorksheet(SLDocument.DefaultFirstSheetName, table.TableName)
                    Else
                        xslDoc.AddWorksheet(table.TableName)
                    End If

                    ' スタイルを設定
                    For i As Integer = 0 To table.Columns.Count - 1
                        ' DateTimeの場合
                        If table.Columns(i).DataType = GetType(DateTime) Then
                            xslDoc.SetColumnWidth(i + 1, 11)
                            Dim style = xslDoc.CreateStyle()
                            style.FormatCode = "yyyy/m/d"

                            xslDoc.SetCellStyle(2, i + 1, table.Rows.Count + 2, i + 1, style)
                        End If
                    Next

                    ' データの出力
                    xslDoc.ImportDataTable(1, 1, table, True)
                Next
            End If

            ' ブックを保存
            xslDoc.SaveAs(argPath)

            Return True
        End Function
#End Region

#Region "CSV出力"
        ''' <summary>
        ''' DataTableに保持されているデータをCSVファイルに出力する。
        ''' </summary>
        ''' <param name="argTable">出力するデータが保持されているDataTable</param>
        ''' <param name="argPath">ファイル出力フルパス</param>
        ''' <param name="argAppend">true:追加書き込み, false:新規作成</param>
        ''' <param name="argOutputHeader">ヘッダ出力フラグ</param>
        ''' <param name="argDuplicateNull">重複データNULLフラグ：trueの場合はNULL値を前の行のデータで復元する。</param>
        Protected Sub ExportCsv(argTable As DataTable, argPath As String, Optional argAppend As Boolean = False, Optional argOutputHeader As Boolean = True, Optional argDuplicateNull As Boolean = False)
            ' データなしは処理しない
            If argTable.Rows.Count = 0 Then
                Return
            End If

            Dim builder As New StringBuilder()
            Dim colmuns As DataColumnCollection = argTable.Columns

            ' ファイル出力
            Using writer As New StreamWriter(argPath, argAppend, Encoding.[Default])
                ' 新規作成かつヘッダ出力ありの場合、ヘッダを出力する
                If Not argAppend AndAlso argOutputHeader Then
                    ' 1列目
                    builder.Append(colmuns(0).Caption)
                    ' 列毎のループ
                    For i As Integer = 1 To colmuns.Count - 1
                        builder.Append(","c).Append(colmuns(i).Caption)
                    Next

                    ' 一行分出力
                    writer.WriteLine(builder)
                    ' クリア
                    builder.Length = 0
                End If

                ' 重複データ復元の指定ありの場合
                If argDuplicateNull Then
                    ' 重複データ記憶用
                    Dim preRow As DataRow = argTable.NewRow()

                    ' 行毎のループ
                    For Each row As DataRow In argTable.Rows
                        ' 1列目
                        If Not IsDBNull(row(0)) Then
                            preRow(0) = row(0)
                        End If
                        builder.Append(preRow(0).ToString())

                        ' 列毎のループ
                        For i As Integer = 1 To colmuns.Count - 1
                            ' 重複していなければ値を記憶
                            If Not IsDBNull(row(i)) Then
                                preRow(i) = row(i)
                            End If
                            builder.Append(","c).Append(preRow(i).ToString())
                        Next

                        ' 一行分出力
                        writer.WriteLine(builder)
                        ' クリア
                        builder.Length = 0
                    Next
                Else
                    ' 重複データ復元の指定なしの場合
                    ' 行毎のループ
                    For Each row As DataRow In argTable.Rows
                        ' 1列目
                        builder.Append(row(0).ToString())
                        ' 列毎のループ
                        For i As Integer = 1 To colmuns.Count - 1
                            builder.Append(","c).Append(row(i).ToString())
                        Next

                        ' 一行分出力
                        writer.WriteLine(builder)
                        ' クリア
                        builder.Length = 0
                    Next
                End If
            End Using
        End Sub

        ''' <summary>
        ''' Excelファイルを別のWindowsに開くメソッド
        ''' </summary>
        ''' <param name="argUrl">ExcelファイルのURL</param>
        Protected Sub OpenExcel(argUrl As String)
            ' 返すjavascript文作成
            Dim sb As New StringBuilder()
            sb.Append("<script language='javascript'>")
            sb.Append("function openExcel(){")
            ' 窓口のサイズを取得
            sb.Append("var xMax = screen.Width, yMax = screen.Height;")
            sb.Append("var xOffset = (xMax - 800)/2, yOffset = (yMax - 600)/4;")
            sb.Append("window.open('").Append(argUrl)
            ' 出力窓口の状態を設定
            sb.Append("',null,'menubar=yes,toolbar=no,location=no,width=800,height=600,screenX='+xOffset+',screenY='+yOffset+',top='+yOffset+',left='+xOffset+',resizable=yes,status=yes,scrollbars=yes,center=yes');}")
            sb.Append("window.onLoad=openExcel();")
            sb.Append("</script>")

            If Not ClientScript.IsClientScriptBlockRegistered("openExcelScript") Then
                ClientScript.RegisterClientScriptBlock([GetType](), "openExcelScript", sb.ToString())
            End If
        End Sub
#End Region
#End Region
    End Class
End Namespace
