Namespace Common
    ''' <summary>
    ''' メッセージデータ
    ''' </summary>
    <Serializable()>
    Public Class CMMessage
#Region "プロパティ"
        ''' <summary>
        ''' メッセージコード
        ''' </summary>
        Public Property MessageCd As String

        ''' <summary>
        ''' データテーブル名、行番号、フィールドデータ
        ''' </summary>
        Public Property RowField As CMRowField

        ''' <summary>
        ''' パラメータ
        ''' </summary>
        Public Property Params As Object()
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' メッセージデータを生成する。
        ''' </summary>
        ''' <param name="argMsgCode">メッセージコード</param>
        ''' <param name="argParams">パラメータ</param>
        Public Sub New(argMsgCode As String, ParamArray argParams As Object())
            MessageCd = argMsgCode
            Params = argParams
        End Sub

        ''' <summary>
        ''' コンストラクタ（行番号指定）
        ''' メッセージデータを生成する。
        ''' </summary>
        ''' <param name="argMsgCode">メッセージコード</param>
        ''' <param name="argRowField">行番号</param>
        ''' <param name="argParams">パラメータ</param>
        Public Sub New(argMsgCode As String, argRowField As CMRowField, ParamArray argParams As Object())
            Me.New(argMsgCode, argParams)
            RowField = argRowField
        End Sub
#End Region

        ''' <summary>
        ''' メッセージ文字列を返す。
        ''' </summary>
        Public Overrides Function ToString() As String
            Return CMMessageManager.GetMessage(MessageCd, Params)
        End Function
    End Class
End Namespace
