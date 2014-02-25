Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlControls

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL

Namespace WEB
    ''' <summary>
    ''' 登録画面の基底クラス
    ''' </summary>
    Public Class CMBaseEntryForm
        Inherits CMBaseForm
#Region "プロパティ"
        ''' <summary>
        ''' 操作モード
        ''' </summary>
        Public Property OpeMode As String

        ''' <summary>
        ''' 入力データを保持するDataRow
        ''' </summary>
        Public Property InputRow As DataRow
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub
#End Region

#Region "protectedメソッド"
        ''' <summary>
        ''' 操作モードを設定し、操作モードに応じ画面の状態を変更する。
        ''' </summary>
        ''' <param name="argPanelKeyItems">キー項目パネル</param>
        ''' <param name="argPanelSubItems">従属項目パネル</param>
        ''' <param name="argPanelUpdateInfo">更新情報パネル</param>
        ''' <param name="argPanelFunction">機能ボタンパネル</param>
        ''' <param name="argButtonClose">閉じるボタン</param>
        ''' <param name="argButtonConfirm">確認ボタン</param>
        ''' <param name="argButtonCancel">キャンセルボタン</param>
        ''' <returns>サブ画面名</returns>
        Protected Function SetOpeMode(argPanelKeyItems As Panel, argPanelSubItems As Panel, argPanelUpdateInfo As Panel, argPanelFunction As Panel, argButtonClose As HtmlInputButton, argButtonConfirm As Button, _
            argButtonCancel As Button) As String
            ' 操作モードを取得
            OpeMode = Request.Params("mode")

            Dim subName As String
            'argButtonClose.Visible = false

            ' 操作モードに応じた設定
            Select Case OpeMode
                Case "Insert"
                    subName = "新規"
                    argPanelUpdateInfo.Visible = False
                    argButtonConfirm.Attributes.Add("onclick", String.Format("return confirm('{0}') && CheckInputEntry('{1}')", CMMessageManager.GetMessage("QV001"), OpeMode))
                    Exit Select
                Case "Update"
                    subName = "修正"
                    ProtectPanel(argPanelKeyItems)
                    argButtonConfirm.Attributes.Add("onclick", String.Format("return confirm('{0}') && CheckInputEntry('{1}')", CMMessageManager.GetMessage("QV001"), OpeMode))
                    Exit Select
                Case "Delete"
                    subName = "削除確認"
                    ProtectPanel(argPanelKeyItems)
                    ProtectPanel(argPanelSubItems)
                    argButtonConfirm.Text = "削除実行"
                    argButtonCancel.Text = "キャンセル"
                    argButtonConfirm.Attributes.Add("onclick", String.Format("return confirm('{0}')", CMMessageManager.GetMessage("QV002")))
                    Exit Select
                Case Else
                    subName = "参照"
                    'argButtonClose.Visible = true
                    ProtectPanel(argPanelKeyItems)
                    ProtectPanel(argPanelSubItems)
                    argButtonConfirm.Enabled = False
                    Exit Select
            End Select

            Return subName
        End Function

        ''' <summary>
        ''' 画面表示処理
        ''' </summary>
        ''' <param name="argBody">bodyタグ</param>
        ''' <param name="argFacade">使用ファサード</param>
        Protected Sub OnPageOnLoad(argBody As HtmlGenericControl, argFacade As ICMBaseBL)
            ' キーを取得
            Dim paramKey As String = Request.Params("keys")

            ' 初期表示の場合
            If paramKey IsNot Nothing Then
                ' キャンセルボタンの戻り値を初期化
                Session("cancelRet") = False

                ' パラメータ作成
                Dim param As List(Of CMSelectParam) = CreateSelectParam(paramKey)

                Try
                    ' ファサードの呼び出し
                    Dim operationTime As DateTime
                    Dim message As CMMessage = Nothing
                    Dim result As DataSet = argFacade.[Select](param, CMSelectType.Edit, operationTime, message)

                    Dim table As DataTable = result.Tables(0)

                    Dim found As Boolean = table.Rows.Count > 0
                    ' 新規または検索結果ありの場合
                    If OpeMode = "Insert" OrElse found Then
                        ' 新規で検索結果なしの場合
                        If Not found Then
                            ' デフォルトの行を作成
                            Dim newRow As DataRow = table.NewRow()
                            ' 新規行にデフォルト値を設定する
                            SetDefaultValue(newRow)
                            ' 新規行を追加
                            table.Rows.Add(newRow)
                            ' 更新を確定
                            table.AcceptChanges()
                        End If

                        ' 検索結果を取得
                        InputRow = table.Rows(0)

                        ' データバインド実行
                        DataBind()

                        ' セッションに検索結果を保持
                        Session("inputRow") = InputRow

                        ' 操作履歴を出力
                        WriteOperationLog()
                    Else
                        ' 検索結果なしの場合
                        argBody.Attributes.Add("onload", "alert('" & CMMessageManager.GetMessage("IV001") & "'); window.returnValue = false; window.close()")
                    End If
                Catch ex As Exception
                    ShowError(ex)
                    Return
                End Try
            Else
                ' 確認画面、戻った画面の場合
                ' 編集結果を取得
                InputRow = DirectCast(Session("inputRow"), DataRow)

                ' 結果メッセージを表示
                Dim mes As String = DirectCast(Session("retMessage"), String)
                If mes IsNot Nothing AndAlso mes.Length > 0 Then
                    DirectCast(Master, Object).ShowMessage("I", mes)
                    Session.Remove("retMessage")
                End If

                ' データバインド実行
                DataBind()
            End If
        End Sub

        ''' <summary>
        ''' 登録ボタン押下時処理
        ''' </summary>
        ''' <param name="argBody">bodyタグ</param>
        ''' <param name="argFacade">使用ファサード</param>
        Protected Sub OnCommitClick(argBody As HtmlGenericControl, argFacade As ICMBaseBL)
            ' セッションからデータを取得
            InputRow = DirectCast(Session("inputRow"), DataRow)
            ' 登録DataTable
            Dim inputTable As DataTable = InputRow.Table

            ' 新規、修正の場合
            If OpeMode = "Insert" OrElse OpeMode = "Update" Then
                ' データが更新されていなければ、アラート表示
                If Not IsModified() Then
                    ShowMessage("WV106")
                    Return
                End If

                ' 入力データを設定
                Dim hasError As Boolean = SetInputRow()

                ' セッションに編集結果を保持
                Session("inputRow") = InputRow

                ' エラーがなければ登録実行
                If hasError Then
                    Return
                End If

                ' 新規確認の場合
                If OpeMode = "Insert" Then
                    Dim ds As DataSet = InputRow.Table.DataSet.Clone()
                    inputTable = ds.Tables(0)
                    Dim row As DataRow = inputTable.NewRow()
                    ' データコピー
                    For i As Integer = 0 To inputTable.Columns.Count - 1
                        row(i) = InputRow(i)
                    Next
                    ' 新規行追加
                    inputTable.Rows.Add(row)
                End If
            Else
                ' 削除確認の場合
                Dim ds As DataSet = InputRow.Table.DataSet.Copy()
                inputTable = ds.Tables(0)
                inputTable.Rows(0).Delete()
            End If

            Try
                ' ファサードの呼び出し
                Dim operationTime As DateTime
                argFacade.Update(inputTable.DataSet, operationTime)

                ' 新規、修正の場合
                If OpeMode = "Insert" OrElse OpeMode = "Update" Then
                    ' 変更を確定
                    InputRow.AcceptChanges()
                    ' セッションに編集結果を保持
                    Session("inputRow") = InputRow
                    Session("retMessage") = CMMessageManager.GetMessage("IV003")
                    ' 新規画面へリダイレクト
                    'Response.Redirect(Request.Path + "?mode=" + OpeMode)
                    Session("cancelRet") = True
                Else
                    ' 削除確認の場合、画面を閉じる
                    Close(True)
                End If
            Catch ex As Exception
                ShowError(ex)
            End Try
        End Sub

        ''' <summary>
        ''' キャンセルボタン押下時処理
        ''' </summary>
        ''' <param name="argBody">bodyタグ</param>
        Protected Sub OnCancelClick(argBody As HtmlGenericControl)
            ' セッションからデータを取得
            InputRow = DirectCast(Session("inputRow"), DataRow)
            Dim retVal As Boolean = CBool(Session("cancelRet"))

            ' 新規、修正の場合
            If OpeMode = "Insert" OrElse OpeMode = "Update" Then
                Dim msgcd As String = If(IsModified(), "QV005", "QV006")

                ' 確認画面を表示
                argBody.Attributes.Add("onload", String.Format("if (confirm('{0}')) {{window.returnValue = {1}; window.close()}}", CMMessageManager.GetMessage(msgcd).Replace(vbCr & vbLf, "\n"), retVal.ToString().ToLower()))
            Else
                Close(retVal)
            End If
        End Sub

        ''' <summary>
        ''' パネルのデータが変更されているかチェックする。
        ''' </summary>
        ''' <param name="argPanel">パネル</param>
        ''' <returns>True:変更あり, False:変更なし</returns>
        Protected Function IsPanelModified(argPanel As Panel) As Boolean
            For Each c As Control In argPanel.Controls
                Dim wc As WebControl = TryCast(c, WebControl)

                ' テキストとドロップダウンが対象
                If Not (TypeOf wc Is DropDownList) AndAlso Not (TypeOf wc Is TextBox) Then
                    Continue For
                End If

                ' 値を比較
                If InputRow(wc.ID, DataRowVersion.Original).ToString() <> GetValue(wc).ToString() Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' パネルに設定された値をInputRowに設定する。
        ''' </summary>
        ''' <param name="argPanel">パネル</param>
        Protected Sub SetPanelInputRow(argPanel As Panel)
            For Each c As Control In argPanel.Controls
                Dim wc As WebControl = TryCast(c, WebControl)

                ' テキストとドロップダウンが対象
                If Not (TypeOf wc Is DropDownList) AndAlso Not (TypeOf wc Is TextBox) Then
                    Continue For
                End If

                ' 値を設定
                InputRow(wc.ID) = GetValue(wc)
            Next
        End Sub

        ''' <summary>
        ''' InputRow中で指定列の値が変更されているかチェックし、変更されている場合、
        ''' 項目名ラベルの文字色をオレンジに変更する。
        ''' </summary>
        ''' <param name="argColname">列名</param>
        ''' <param name="argLabel">項目名ラベル</param>
        Protected Sub CheckSetModColor(argColname As String, argLabel As Label)
            If InputRow(argColname).ToString() <> InputRow(argColname, DataRowVersion.Original).ToString() Then
                argLabel.Attributes.Add("class", "transp head2")
            End If
        End Sub

        ''' <summary>
        ''' 登録日時の文字列を取得する。
        ''' </summary>
        ''' <returns>登録日時の文字列</returns>
        Protected Function GetAddInfo() As String
            If InputRow Is Nothing Then
                Return ""
            End If

            Return String.Format("{0}：{1:yyyy/MM/dd HH:mm:ss} </TD><TD>{2}", "作成日時", InputRow("作成日時"), InputRow("作成者名"))
        End Function

        ''' <summary>
        ''' 更新日時の文字列を取得する。
        ''' </summary>
        ''' <returns>更新日時の文字列</returns>
        Protected Function GetUpdateInfo() As String
            If InputRow Is Nothing Then
                Return ""
            End If

            Return String.Format("{0}：{1:yyyy/MM/dd HH:mm:ss} </TD><TD>{2}", "更新日時", InputRow("更新日時"), InputRow("更新者名"))
        End Function

        ''' <summary>
        ''' 指定列の時刻部分を取得する。
        ''' </summary>
        ''' <param name="argCol">列名</param>
        ''' <returns>時刻部分文字列</returns>
        Protected Function GetHour(argCol As String) As String
            Dim s As String = InputRow(argCol).ToString()
            Return If(s.Length < 2, "", s.Substring(0, 2))
        End Function

        ''' <summary>
        ''' 指定列の分部分を取得する。
        ''' </summary>
        ''' <param name="argCol">列名</param>
        ''' <returns>分部分文字列</returns>
        Protected Function GetMinute(argCol As String) As String
            Dim s As String = InputRow(argCol).ToString()
            Return If(s.Length < 4, "", s.Substring(2, 2))
        End Function

        ''' <summary>
        ''' 時刻文字列を取得する。
        ''' </summary>
        ''' <param name="argCol">列名</param>
        ''' <returns>分部分文字列</returns>
        Protected Function GetTimeStr(argCol As String) As String
            ' 新規、修正の場合、時刻、分はドロップダウンリストに表示する
            If OpeMode = "Insert" OrElse OpeMode = "Update" Then
                Return "："
            End If

            Dim s As String = InputRow(argCol).ToString()
            Return If(s.Length < 4, "", s.Substring(0, 2) & "：" & s.Substring(2, 2))
        End Function
#End Region

#Region "サブクラスで上書きするメソッド"
        ''' <summary>
        ''' キーデータ文字列から検索パラメータを作成する。
        ''' </summary>
        ''' <param name="argKey">キーデータ文字列</param>
        ''' <returns>検索パラメータ</returns>
        Protected Overridable Function CreateSelectParam(argKey As String) As List(Of CMSelectParam)
            Return New List(Of CMSelectParam)()
        End Function

        ''' <summary>
        ''' 新規行にデフォルト値を設定する。
        ''' </summary>
        ''' <param name="argRow">デフォルト値を設定するDataRow</param>
        Protected Overridable Sub SetDefaultValue(argRow As DataRow)
        End Sub

        ''' <summary>
        ''' データが変更されているかチェックする。
        ''' </summary>
        ''' <returns>True:変更あり, False:変更なし</returns>
        Protected Overridable Function IsModified() As Boolean
            Return False
        End Function

        ''' <summary>
        ''' InputRowに入力データを設定する。
        ''' </summary>
        ''' <returns>True:エラーあり, False:エラーなし</returns>
        Protected Overridable Function SetInputRow() As Boolean
            Return False
        End Function
#End Region
    End Class
End Namespace
