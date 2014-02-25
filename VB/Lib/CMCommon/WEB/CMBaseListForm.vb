Imports System.Web.UI
Imports System.Web.UI.WebControls

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL

Namespace WEB
    ''' <summary>
    ''' 一覧画面の基底クラス
    ''' </summary>
    Public Class CMBaseListForm
        Inherits CMBaseForm
#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub
#End Region

#Region "protectedメソッド"
        ''' <summary>
        ''' 検索を実行する。
        ''' </summary>
        ''' <param name="argFacade">使用ファサード</param>
        ''' <param name="argParam">検索条件パラメータ</param>
        ''' <param name="argGrid">一覧表示用グリッド</param>
        ''' <param name="argPage">ページ</param>
        ''' <returns>True:エラーあり, False:エラーなし</returns>
        Protected Function DoSelect(argFacade As ICMBaseBL, argParam As List(Of CMSelectParam), argGrid As GridView, Optional argPage As Integer = 0) As Boolean
            Try
                ' ファサードの呼び出し
                Dim operationTime As DateTime
                Dim message As CMMessage = Nothing
                Dim result As DataSet = argFacade.[Select](argParam, CMSelectType.List, operationTime, message)

                ' 返却メッセージの表示
                If message IsNot Nothing Then
                    ShowMessage(message)
                End If

                ' DataSource設定
                argGrid.DataSource = result.Tables(0)
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

        ''' <summary>
        ''' CSV出力を実行する。
        ''' </summary>
        ''' <param name="argFacade">使用ファサード</param>
        ''' <param name="argParam">検索条件パラメータ</param>
        ''' <param name="argUrl">参照URL</param>
        ''' <returns>True:エラーあり, False:エラーなし</returns>
        Protected Function DoCsvOut(argFacade As ICMBaseBL, argParam As List(Of CMSelectParam), ByRef argUrl As String) As Boolean
            argUrl = Nothing

            Try
                ' ファサードの呼び出し
                Dim operationTime As DateTime
                Dim message As CMMessage = Nothing
                Dim result As DataSet = argFacade.[Select](argParam, CMSelectType.Csv, operationTime, message)

                ' 返却メッセージの表示
                If message IsNot Nothing Then
                    ShowMessage(message)
                End If

                Dim table As DataTable = result.Tables(0)

                Dim found As Boolean = table.Rows.Count > 0
                ' 検索結果ありの場合
                If found Then
                    ' CSVファイル名作成
                    'string fname = string.Format("{0}_{1}_{2}.csv",
                    Dim fname As String = String.Format("{0}_{1}_{2}.xlsx", table.TableName, DateTime.Now.ToString("yyyyMMddHHmmss"), CMInformationManager.UserInfo.Id)
                    ' CSVファイル出力
                    'ExportCsv(table, System.IO.Path.Combine(Request.PhysicalApplicationPath, "Csv", fname))
                    ExportExcel(result, System.IO.Path.Combine(Request.PhysicalApplicationPath, "Csv", fname))
                    ' 画面表示
                    argUrl = Request.ApplicationPath + "/Csv/" & Uri.EscapeUriString(fname)
                End If
            Catch ex As Exception
                ShowError(ex)
                Return True
            End Try

            Return False
        End Function

        ''' <summary>
        ''' 機能ボタンにスクリプトを登録する。
        ''' </summary>
        ''' <param name="argButtonSelect">検索ボタン</param>
        ''' <param name="argButtonCsvOut">CSV出力ボタン</param>
        Protected Sub AddFuncOnclick(argButtonSelect As Button, argButtonCsvOut As Button)
            argButtonSelect.Attributes.Add("onclick", "return CheckInputList()")
            argButtonCsvOut.Attributes.Add("onclick", "ShowWaitMessage(); return CheckInputList()")
        End Sub

        ''' <summary>
        ''' 機能ボタンにスクリプトを登録する。
        ''' </summary>
        ''' <param name="argButtonSelect">検索ボタン</param>
        ''' <param name="argButtonCsvOut">CSV出力ボタン</param>
        ''' <param name="argButtonInsert">新規ボタン</param>
        ''' <param name="argButtonUpdate">修正ボタン</param>
        ''' <param name="argButtonDelete">削除ボタン</param>
        Protected Sub AddFuncOnclick(argButtonSelect As Button, argButtonCsvOut As Button, argButtonInsert As Button, argButtonUpdate As Button, argButtonDelete As Button)
            AddFuncOnclick(argButtonSelect, argButtonCsvOut)
            argButtonInsert.Attributes.Add("onclick", "return OpenEntryForm('Insert')")
            argButtonUpdate.Attributes.Add("onclick", "return OpenEntryForm('Update')")
            argButtonDelete.Attributes.Add("onclick", "return OpenEntryForm('Delete')")
        End Sub

        ''' <summary>
        ''' 検索パラメータを作成する。
        ''' </summary>
        ''' <param name="argPanel">検索条件パネル</param>
        ''' <returns>検索パラメータ</returns>
        Protected Function CreateSelectParam(argPanel As Panel) As List(Of CMSelectParam)
            Dim param As New List(Of CMSelectParam)()

            For Each c As Control In argPanel.Controls
                Dim wc As WebControl = TryCast(c, WebControl)

                ' テキストとドロップダウンが対象
                If Not (TypeOf wc Is DropDownList) AndAlso Not (TypeOf wc Is TextBox) Then
                    Continue For
                End If

                ' Toは無視
                If wc.ID.EndsWith("To") Then
                    Continue For
                End If

                ' Fromの場合
                If wc.ID.EndsWith("From") Then
                    ' Fromなし名称取得
                    Dim colName As String = wc.ID.Substring(0, wc.ID.IndexOf("From"))

                    Dim toCnt As WebControl = DirectCast(argPanel.FindControl(colName & "To"), WebControl)
                    Dim isSetFrom As Boolean = IsSetValue(wc)
                    Dim isSetTo As Boolean = IsSetValue(toCnt)

                    ' FromTo
                    If isSetFrom AndAlso isSetTo Then
                        param.Add(New CMSelectParam(colName, String.Format("BETWEEN @{0} AND @{1}", wc.ID, toCnt.ID), GetValue(wc), GetValue(toCnt)))
                        ' From or To
                    ElseIf isSetFrom OrElse isSetTo Then
                        Dim op As String = If(isSetFrom, ">= @", "<= @")
                        Dim condCnt As WebControl = If(isSetFrom, wc, toCnt)

                        param.Add(New CMSelectParam(colName, op + condCnt.ID, GetValue(condCnt)))
                    End If
                Else
                    ' 単一項目の場合
                    ' 設定ありの場合
                    If IsSetValue(wc) Then
                        Dim op As String = "= @"
                        Dim value As Object = GetValue(wc)

                        ' LIKE検索の場合
                        If TypeOf wc Is TextBox AndAlso wc.ID.EndsWith("名") Then
                            op = "LIKE @"
                            value = "%" & Convert.ToString(value) & "%"
                        End If

                        param.Add(New CMSelectParam(wc.ID, op + wc.ID, value))
                    End If
                End If
            Next

            Return param
        End Function
#End Region
    End Class
End Namespace
