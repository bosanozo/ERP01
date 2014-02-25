Imports Seasar.Quill.Attrs

Imports NEXS.ERP.CM.Common

Namespace BL
    ''' <summary>
    ''' 共通処理ファサード層のインタフェース
    ''' </summary>
    <Implementation(GetType(CMCommonBL))> _
    Public Interface ICMCommonBL
        ''' <summary>
        ''' 現在時刻を取得する。
        ''' </summary>
        ''' <returns>現在時刻</returns>
        Function GetSysdate() As DateTime

        ''' <summary>
        ''' 指定された検索IDの検索を指定された条件で実行する。
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argParams">パラメータ</param>
        ''' <returns>検索結果</returns>
        Function [Select](argSelectId As String, ParamArray argParams As Object()) As DataTable

        ''' <summary>
        ''' 共通検索呼び出し用引数に指定された検索IDの検索を実行する。
        ''' 共通検索呼び出し用引数は複数指定可能とし、検索結果をDataSetに格納する。
        ''' </summary>
        ''' <param name="args">共通検索呼び出し用引数</param>
        ''' <returns>検索結果</returns>
        Function [Select](ParamArray args As CMCommonSelectArgs()) As DataSet

        ''' <summary>
        ''' 操作ログを記録する。
        ''' </summary>
        ''' <param name="argFormName">画面名</param>
        ''' <returns>現在時刻</returns>
        Function WriteOperationLog(argFormName As String) As DateTime

        ''' <summary>
        ''' 汎用基準値から区分値名称を取得する。
        ''' </summary>
        ''' <param name="argKbnList">基準値分類CDのリスト</param>
        ''' <returns>区分値名称のDataTable</returns>
        Function SelectKbn(ParamArray argKbnList As String()) As DataTable

        ''' <summary>
        ''' 参照範囲, 更新許可を検索する。
        ''' </summary>
        ''' <param name="argFormId">画面ＩＤ</param>
        ''' <param name="argIsRange">True:参照範囲, False:更新許可</param>
        ''' <returns>True:会社、更新可, False:拠点、更新不可</returns>
        Function GetRangeCanUpdate(argFormId As String, argIsRange As Boolean) As Boolean

        ''' <summary>
        ''' 指定された検索IDの検索を指定された条件で実行する。
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argMessage">結果メッセージ</param>
        ''' <returns>検索結果</returns>
        Function SelectSub(argSelectId As String, argParam As List(Of CMSelectParam), ByRef argMessage As CMMessage) As DataTable
    End Interface
End Namespace
