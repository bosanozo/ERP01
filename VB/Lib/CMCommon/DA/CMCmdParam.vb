Imports System.ComponentModel
Imports System.Data.SqlClient
Imports System.Text

Imports NEXS.ERP.CM.Common

Namespace DA
    ''' <summary>
    ''' DB項目種別
    ''' </summary>
    Public Enum CMDbType
        コード
        コード_可変
        区分
        文字列
        金額
        整数
        小数
        フラグ
        日付
        日時
    End Enum

    ''' <summary>
    ''' Commandパラメータ
    ''' </summary>
    Public Class CMCmdParam
        ''' <summary>パラメータ名</summary>
        <Category("共通部品")> _
        <Description("パラメータ名")> _
        Public Property Name As String

        '<DefaultValue(Nothing)> _
        ''' <summary>DataTableの列名(パラメータ名と異なる場合に指定)</summary>
        <Category("共通部品")> _
        <Description("DataTableの列名(パラメータ名と異なる場合に指定)")> _
        Public Property SourceColumn As String

        ''' <summary>DB項目の型</summary>
        <Category("共通部品")> _
        <Description("DB項目の型")> _
        <DefaultValue(CMDbType.コード)> _
        Public Property DbType As CMDbType

        ''' <summary>キー項目フラグ</summary>
        <Category("共通部品")> _
        <Description("キー項目フラグ")> _
        <DefaultValue(False)> _
        Public Property IsKey As Boolean

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            DbType = CMDbType.コード
        End Sub
#End Region

        ''' <summary>
        ''' SqlDbTypeを返す。
        ''' </summary>
        ''' <returns>SqlDbType</returns>
        Public Function GetDbType() As SqlDbType
            Select Case DbType
                Case CMDbType.コード_可変
                    Return SqlDbType.VarChar
                Case CMDbType.文字列
                    Return SqlDbType.NVarChar
                Case CMDbType.金額
                    Return SqlDbType.Money
                Case CMDbType.整数
                    Return SqlDbType.Int
                Case CMDbType.小数
                    Return SqlDbType.Decimal
                Case CMDbType.フラグ
                    Return SqlDbType.Bit
                Case CMDbType.日付
                    Return SqlDbType.Date
                Case CMDbType.日時
                    Return SqlDbType.DateTime
                Case Else
                    Return SqlDbType.Char
            End Select
        End Function
    End Class

    ''' <summary>
    ''' SqlCommand設定
    ''' </summary>
    Public Class CMCmdSetting
        ''' <summary>テーブル名</summary>
        <Category("共通部品")> _
        <Description("テーブル名")> _
        Public Property Name As String

        ''' <summary>SqlCommandパラメータ配列</summary>
        <Category("共通部品")> _
        <Description("SqlCommandパラメータ配列")> _
        Public Property ColumnParams As CMCmdParam()

        ''' <summary>
        ''' 主キーの検索条件を返す。
        ''' </summary>
        ''' <returns>主キーの検索条件</returns>
        Public Function GetKeyCondition() As String
            Dim builder As New StringBuilder()
            ' テーブル項目列でループ
            For Each row As CMCmdParam In ColumnParams
                If row.IsKey Then
                    If builder.Length > 0 Then
                        builder.Append(" AND ")
                    End If
                    builder.AppendFormat("{0} = @{0}", row.Name)
                End If
            Next

            Return builder.ToString()
        End Function
    End Class
End Namespace
