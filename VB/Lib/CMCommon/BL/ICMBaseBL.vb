Imports NEXS.ERP.CM.Common

Namespace BL
    ''' <summary>
    ''' ファサードインターフェース
    ''' </summary>
    Public Interface ICMBaseBL
        ''' <summary>
        ''' 検索する。
        ''' </summary>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argSelectType">検索種別</param>
        ''' <param name="argOperationTime">操作時刻</param>
        ''' <param name="argMessage">結果メッセージ</param>
        ''' <returns>検索結果</returns>
        Function [Select](argParam As List(Of CMSelectParam), argSelectType As CMSelectType, ByRef argOperationTime As DateTime, ByRef argMessage As CMMessage) As DataSet

        ''' <summary>
        ''' データを登録する。
        ''' </summary>
        ''' <param name="argUpdateData">更新データ</param>
        ''' <param name="argOperationTime">操作時刻</param>
        ''' <returns>登録したレコード数</returns>
        Function Update(argUpdateData As DataSet, ByRef argOperationTime As DateTime) As Integer
    End Interface
End Namespace
