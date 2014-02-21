Namespace Common
    ''' <summary>
    ''' 検索条件クラス
    ''' </summary>
    Public Class CMSelectParam
        Public name As String
        Public condtion As String
        Public paramFrom As Object
        Public paramTo As Object
        Public leftCol As String

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
