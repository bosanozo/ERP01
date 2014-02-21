Namespace Common
    ''' <summary>
    ''' データテーブル名、行番号、フィールドデータ
    ''' </summary>
    Public Class CMRowField
#Region "プロパティ"
        ''' <summary>
        ''' データテーブル名
        ''' </summary>
        Public Property DataTableName As String

        ''' <summary>
        ''' 行番号
        ''' </summary>
        Public Property RowNumber As Integer

        ''' <summary>
        ''' フィールド名
        ''' </summary>
        Public Property FieldName As String
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argRowNumber">行番号</param>
        Public Sub New(argRowNumber As Integer)
            Me.New(argRowNumber, Nothing)
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argRowNumber">行番号</param>
        ''' <param name="argFieldName">フィールド名</param>
        Public Sub New(argRowNumber As Integer, argFieldName As String)
            Me.New(Nothing, argRowNumber, argFieldName)
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argDataTableName">データテーブル名</param>
        ''' <param name="argRowNumber">行番号</param>
        Public Sub New(argDataTableName As String, argRowNumber As Integer)
            Me.New(argDataTableName, argRowNumber, Nothing)
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argDataTableName">データテーブル名</param>
        ''' <param name="argRowNumber">行番号</param>
        ''' <param name="argFieldName">フィールド名</param>
        Public Sub New(argDataTableName As String, argRowNumber As Integer, argFieldName As String)
            DataTableName = argDataTableName
            RowNumber = argRowNumber
            FieldName = argFieldName
        End Sub
#End Region
    End Class
End Namespace
