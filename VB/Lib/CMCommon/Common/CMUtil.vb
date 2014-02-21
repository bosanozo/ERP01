Imports System.Text.RegularExpressions

Namespace Common
    ''' <summary>
    ''' ユーティリティクラス
    ''' </summary>
    Public NotInheritable Class CMUtil
        ''' <summary>
        ''' DataRowから行番号を取得する。
        ''' </summary>
        ''' <param name="argDataRow">DataRow</param>
        ''' <returns>行番号(行番号なしの場合は0を返す。)</returns>
        Public Shared Function GetRowNumber(argDataRow As DataRow) As Integer
            Dim rowNumber As Integer = 0
            ' 行番号を取得
            If argDataRow.Table.Columns.Contains("ROWNUMBER") Then
                Dim version As DataRowVersion = If(argDataRow.RowState = DataRowState.Deleted, DataRowVersion.Original, DataRowVersion.Current)
                rowNumber = Convert.ToInt32(argDataRow("ROWNUMBER", version))
            End If

            Return rowNumber
        End Function

        ''' <summary>
        ''' 数値・文字・記号を二種以上使用しているかチェックする。
        ''' </summary>
        ''' <param name="argPassword">パスワード</param>
        ''' <returns>True:二種より少ない</returns>
        Public Shared Function CheckPassword(argPassword As String) As Boolean
            Dim cnt As Integer = 0
            If Regex.IsMatch(argPassword, "[0-9]") Then
                cnt += 1
            End If
            If Regex.IsMatch(argPassword, "[a-zA-Z]") Then
                cnt += 1
            End If
            If Regex.IsMatch(argPassword, "[^a-zA-Z0-9]") Then
                cnt += 1
            End If

            Return cnt < 2
        End Function
    End Class
End Namespace
