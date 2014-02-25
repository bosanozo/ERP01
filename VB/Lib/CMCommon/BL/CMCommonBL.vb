Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.DA

Imports Seasar.Quill.Attrs
Imports NEXS.ERP.CM.WEB

Namespace BL
    ''' <summary>
    ''' 共通処理ファサード層
    ''' </summary>
    Public Class CMCommonBL
        Inherits CMBaseBL
        Implements ICMCommonBL
#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub
#End Region

#Region "ファサードメソッド"
        ''' <summary>
        ''' 現在時刻を取得する。
        ''' </summary>
        ''' <returns>現在時刻</returns>
        Public Function GetSysdate() As DateTime Implements ICMCommonBL.GetSysdate
            CommonDA.Connection = Connection
            Return CommonDA.GetSysdate()
        End Function

        ''' <summary>
        ''' 指定された検索IDの検索を指定された条件で実行する。
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argParams">パラメータ</param>
        ''' <returns>検索結果</returns>
        Public Function [Select](argSelectId As String, ParamArray argParams As Object()) As DataTable _
            Implements ICMCommonBL.Select
            ' 検索実行
            CommonDA.Connection = Connection
            Dim result As DataTable = CommonDA.[Select](argSelectId, argParams)

            Return result
        End Function

        ''' <summary>
        ''' 共通検索呼び出し用引数に指定された検索IDの検索を実行する。
        ''' 共通検索呼び出し用引数は複数指定可能とし、検索結果をDataSetに格納する。
        ''' </summary>
        ''' <param name="args">共通検索呼び出し用引数</param>
        ''' <returns>検索結果</returns>
        Public Function [Select](ParamArray args As CMCommonSelectArgs()) As DataSet _
            Implements ICMCommonBL.Select
            CommonDA.Connection = Connection

            Dim dataSet As New DataSet()
            ' 検索ループ
            For Each arg As CMCommonSelectArgs In args
                ' 検索実行
                Dim table As DataTable = CommonDA.Select(arg.SelectId, arg.Params)
                ' 検索IDを設定
                Dim tableName As String = arg.SelectId
                Dim idx As Integer = 1
                While dataSet.Tables.IndexOf(tableName) >= 0
                    tableName = arg.SelectId + System.Math.Max(System.Threading.Interlocked.Increment(idx), idx - 1)
                End While
                table.TableName = tableName
                ' DataSetに追加
                dataSet.Tables.Add(table)
            Next

            Return dataSet
        End Function

        ''' <summary>
        ''' 操作ログを記録する。
        ''' </summary>
        ''' <param name="argFormName">画面名</param>
        ''' <returns>現在時刻</returns>
        Public Function WriteOperationLog(argFormName As String) As DateTime _
            Implements ICMCommonBL.WriteOperationLog
            CommonDA.Connection = Connection

            ' 操作ログ記録
            CommonDA.WriteOperationLog(argFormName)

            ' 現在時刻返却
            Return DateTime.Now
        End Function

        ''' <summary>
        ''' 汎用基準値から区分値名称を取得する。
        ''' </summary>
        ''' <param name="argKbnList">基準値分類CDのリスト</param>
        ''' <returns>区分値名称のDataTable</returns>
        Public Overridable Function SelectKbn(ParamArray argKbnList As String()) As DataTable _
            Implements ICMCommonBL.SelectKbn
            CommonDA.Connection = Connection
            Return CommonDA.SelectKbn(argKbnList)
        End Function

        ''' <summary>
        ''' 参照範囲, 更新許可を検索する。
        ''' </summary>
        ''' <param name="argFormId">画面ＩＤ</param>
        ''' <param name="argIsRange">True:参照範囲, False:更新許可</param>
        ''' <returns>True:会社、更新可, False:拠点、更新不可</returns>
        Public Function GetRangeCanUpdate(argFormId As String, argIsRange As Boolean) As Boolean _
            Implements ICMCommonBL.GetRangeCanUpdate
            CommonDA.Connection = Connection
            Return CommonDA.GetRangeCanUpdate(argFormId, argIsRange)
        End Function

        ''' <summary>
        ''' 指定された検索IDの検索を指定された条件で実行する。
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argMessage">結果メッセージ</param>
        ''' <returns>検索結果</returns>
        Public Function SelectSub(argSelectId As String, argParam As List(Of CMSelectParam), ByRef argMessage As CMMessage) As DataTable _
            Implements ICMCommonBL.SelectSub
            ' 検索実行
            Dim isOver As Boolean
            CommonDA.Connection = Connection
            Dim result As DataTable = CommonDA.SelectSub(argSelectId, argParam, isOver)

            argMessage = Nothing
            ' 検索結果なし
            If result.Rows.Count = 0 Then
                argMessage = New CMMessage("IV001")
                ' 最大検索件数オーバー
            ElseIf isOver Then
                argMessage = New CMMessage("IV002")
            End If

            Return result
        End Function
#End Region
    End Class
End Namespace
