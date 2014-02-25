Imports System.Text
Imports Seasar.Quill.Attrs

Namespace DA
    ''' <summary>
    ''' ユーザー情報検索データアクセス層
    ''' </summary>
    <Implementation()> _
    Public Class CMUserInfoDA
        Inherits CMBaseDA
#Region "SQL文"
        ''' <summary>
        ''' SELECT文
        ''' </summary>
        Private Const SELECT_SQL As String =
            "SELECT " &
            "U.ユーザID ID," &
            "U.ユーザ名 NAME," &
            "U.パスワード PASSWD," &
            "'' ROLE," &
            "U.組織CD," &
            "S1.組織名," &
            "S1.組織階層区分 " &
            "FROM CMSMユーザ U " &
            "JOIN CMSM組織 S1 ON S1.組織CD = U.組織CD " &
            "WHERE U.ユーザID = @ID"

        ''' <summary>
        ''' ロールSELECT文
        ''' </summary>
        Private Const SELECT_ROLE_SQL As String =
            "SELECT " &
            "ロールID ROLE " &
            "FROM CMSMユーザロール " &
            "WHERE ユーザID = @ID " &
            "ORDER BY ロールID"
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub
#End Region

#Region "データアクセスメソッド"
        ''' <summary>
        ''' ユーザー情報を取得する。
        ''' </summary>
        ''' <param name="argUserId">ユーザーID</param>
        ''' <returns>ユーザー情報DataRow</returns>
        Public Function FindById(argUserId As String) As DataRow
            ' SelectCommandの設定
            Adapter.SelectCommand = CreateCommand(SELECT_SQL)
            ' パラメータの設定
            Adapter.SelectCommand.Parameters.Add(CreateCmdParam("ID", argUserId))

            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Dim cnt As Integer = Adapter.Fill(ds)

            ' 検索結果なし
            If cnt = 0 Then
                Return Nothing
            End If

            ' ロールを検索
            Adapter.SelectCommand.CommandText = SELECT_ROLE_SQL
            ' データテーブルの作成
            ' データセットの作成
            Dim roleDs As New DataSet()
            ' データの取得
            Adapter.Fill(roleDs)

            ' ロールを,区切りで結合
            Dim sb As New StringBuilder()
            For Each row As DataRow In roleDs.Tables(0).Rows
                If sb.Length > 0 Then
                    sb.Append(","c)
                End If
                sb.Append(row("ROLE").ToString())
            Next
            ds.Tables(0).Rows(0)("ROLE") = sb.ToString()

            ' 検索結果の返却
            Return ds.Tables(0).Rows(0)
        End Function
#End Region
    End Class
End Namespace
