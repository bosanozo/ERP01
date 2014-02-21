Namespace Common
    ''' <summary>
    ''' 共通検索呼び出し用引数データ
    ''' </summary>
    Public Class CMCommonSelectArgs
#Region "プロパティ"
        ''' <summary>検索ID</summary>
        Public Property SelectId As String

        ''' <summary>パラメータ</summary>
        Public Property Params As Object()
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argParams">パラメータ</param>
        Public Sub New(argSelectId As String, ParamArray argParams As Object())
            SelectId = argSelectId
            Params = argParams
        End Sub
#End Region
    End Class
End Namespace
