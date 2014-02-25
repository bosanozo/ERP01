Imports System.Text

Imports Seasar.Quill.Attrs

Imports NEXS.ERP.CM.Common

Namespace DA
    ''' <summary>
    ''' データアクセス層
    ''' </summary>
    <Implementation()> _
    Public Class CMSM010DA
        Inherits CMBaseDA
#Region "SQL文"
        ''' <summary>
        ''' 検索、登録対象のテーブル名
        ''' </summary>
        Private Const TABLE_NAME As String = "CMSM組織"

        ''' <summary>
        ''' 検索列
        ''' </summary>
        Private Const SELECT_COLS As String =
            "A.組織CD," &
            "A.組織名," &
            "A.組織階層区分," &
            "H1.基準値名 組織階層区分名," &
            "A.上位組織CD," &
            "S2.組織名 上位組織名,"

        ''' <summary>
        ''' 名称取得用外部結合
        ''' </summary>
        Private Const LEFT_JOIN As String =
            "LEFT JOIN CMSM組織 S2 ON S2.組織CD = A.上位組織CD " &
            "LEFT JOIN CMSM汎用基準値 H1 ON H1.分類CD = 'M001' AND H1.基準値CD = A.組織階層区分 "

        ''' <summary>
        ''' 検索時の並び順
        ''' </summary>
        Private Const ORDER As String = "組織CD"
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
        ''' 組織マスタを検索する。
        ''' </summary>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argSelectType">検索種別</param>
        ''' <param name="argMaxRow">最大検索件数</param>
        ''' <param name="argIsOver">最大検索件数オーバーフラグ</param>
        ''' <returns>検索結果</returns>
        Public Function [Select](argParam As List(Of CMSelectParam), argSelectType As CMSelectType, argMaxRow As Integer, ByRef argIsOver As Boolean) As DataSet
            ' WHERE句作成
            Dim where As New StringBuilder()
            AddWhere(where, argParam)

            ' SELECT文の設定
            Dim cmd As IDbCommand = CreateCommand(CreateSelectSql(SELECT_COLS, TABLE_NAME, where.ToString(), LEFT_JOIN, ORDER, argSelectType))
            Adapter.SelectCommand = cmd

            ' パラメータの設定
            SetParameter(cmd, argParam)
            ' 一覧検索の場合
            If argSelectType = CMSelectType.List Then
                cmd.Parameters.Add(CreateCmdParam("最大検索件数", argMaxRow))
            End If

            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Dim cnt As Integer = Adapter.Fill(ds)
            ' テーブル名を設定
            ds.Tables(0).TableName = TABLE_NAME

            ' 一覧検索で最大検索件数オーバーの場合、最終行を削除
            If argSelectType = CMSelectType.List AndAlso cnt >= argMaxRow Then
                argIsOver = True
                ds.Tables(0).Rows.RemoveAt(cnt - 1)
            Else
                argIsOver = False
            End If

            ' 検索結果の返却
            Return ds
        End Function
#End Region
    End Class
End Namespace
