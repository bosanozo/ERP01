Namespace Common
    ''' <summary>
    ''' 検索条件クラス
    ''' </summary>
    Public Class CMSelectParam
        ''' <summary>項目名</summary>
        Public name As String
        ''' <summary>検索条件SQL</summary>
        Public condtion As String
        ''' <summary>プレースフォルダに設定するFrom値</summary>
        Public paramFrom As Object
        ''' <summary>プレースフォルダに設定するTo値</summary>
        Public paramTo As Object
        ''' <summary>左辺項目名(leftcol = @name)</summary>
        Public leftCol As String
        ''' <summary>検索条件を追加するテーブル名</summary>
        ''' <remarks>未指定の場合は全テーブルの検索に条件を追加する。</remarks>
        Public tableName As String

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argName">項目名</param>
        ''' <param name="argCondtion">検索条件SQL</param>
        ''' <param name="argValue">プレースフォルダに設定する値</param>
        Public Sub New(argName As String, argCondtion As String, argValue As Object)
            Me.New(argName, argCondtion, argValue, Nothing)
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argName">項目名</param>
        ''' <param name="argCondtion">検索条件SQL</param>
        ''' <param name="argFrom">プレースフォルダに設定するFrom値</param>
        ''' <param name="argTo">プレースフォルダに設定するTo値</param>
        Public Sub New(argName As String, argCondtion As String, argFrom As Object, argTo As Object)
            Me.New(Nothing, argName, argCondtion, argFrom, argTo)
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argLeftCol">左辺項目名</param>
        ''' <param name="argRightCol">右変項目名</param>
        ''' <param name="argCondtion">検索条件SQL</param>
        ''' <param name="argFrom">プレースフォルダに設定するFrom値</param>
        ''' <param name="argTo">プレースフォルダに設定するTo値</param>
        Public Sub New(argLeftCol As String, argRightCol As String, argCondtion As String, argFrom As Object, argTo As Object)
            leftCol = argLeftCol
            name = argRightCol
            condtion = argCondtion
            paramFrom = argFrom
            paramTo = argTo
        End Sub
    End Class
End Namespace
