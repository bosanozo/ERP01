Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.ComponentModel

Imports Seasar.Quill.Attrs

Imports NEXS.ERP.CM.Common

Namespace DA
    ''' <summary>
    ''' 共通処理データアクセス層
    ''' </summary>
    <Implementation()> _
    Public Class CMCommonDA
        Inherits CMBaseDA
        Private Shared s_selectStatementTable As CMCommonSelectDataSet.SelectStatementDataTable

#Region "SQL文"
        ''' <summary>
        ''' 操作ログINSERT文
        ''' </summary>
        Private Const INSERT_OPLOG_SQL As String =
            "INSERT INTO CMSTシステム利用状況 " &
            "VALUES(" &
            "CURRENT_TIMESTAMP," &
            "@画面ID," &
            "@画面名," &
            "@ユーザID," &
            "@端末ID," &
            "@APサーバ)"

        ''' <summary>
        ''' 区分値名称SELECT文
        ''' </summary>
        Private Const SELECT_KBN_SQL As String =
            "SELECT '' 表示名, 分類CD, 分類名, 基準値CD, 基準値名 " &
            "FROM CMSM汎用基準値 WHERE 分類CD IN ({0}) " &
            "ORDER BY 分類CD, 基準値CD"

        ''' <summary>
        ''' 参照範囲, 更新許可ELECT文
        ''' 上位組織再帰検索
        ''' ユーザに付与された権限を画面IDが長く一致するもの、組織階層が近いものを優先して取得
        ''' </summary>
        Private Const SELECT_RANGE_CANUPDATE_SQL As String =
            "WITH SL (組織CD, 上位組織CD, 組織階層区分) AS " &
            "(SELECT 組織CD, 上位組織CD, 組織階層区分 " &
            "FROM CMSM組織 " &
            "WHERE 組織CD = @組織CD " &
            "UNION ALL " &
            "SELECT A.組織CD, A.上位組織CD, A.組織階層区分 " &
            "FROM CMSM組織 A " &
            "JOIN SL ON SL.上位組織CD = A.組織CD AND SL.組織階層区分 != '1')" &
            "SELECT DISTINCT ロールID, " &
            "FIRST_VALUE(許否フラグ) OVER (PARTITION BY ロールID " &
            "ORDER BY LEN(画面ID) DESC, 組織階層区分 DESC) 許否フラグ " &
            "FROM {0} A " &
            "JOIN SL ON SL.組織CD = A.組織CD " &
            "WHERE ロールID IN ({1}) " &
            "AND @画面ID LIKE 画面ID + '%' " &
            "ORDER BY ロールID"

        ''' <summary>
        ''' 更新者SELECT文
        ''' </summary>
        Private Const SELECT_UPD_SQL As String =
            "SELECT B.更新者ID, A.""ユーザ名"" 更新者名 " &
            "FROM ""CMSMユーザ"" A " &
            "JOIN CMSM組織 S ON S.組織CD = A.組織CD " &
            "JOIN ({0}) B ON A.""ユーザID"" = B.更新者ID " &
            "{1}" &
            "ORDER BY B.更新者ID"
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            ' 共通検索設定ファイル読み込み
            If s_selectStatementTable Is Nothing Then
                s_selectStatementTable = New CMCommonSelectDataSet.SelectStatementDataTable()
                s_selectStatementTable.ReadXml(AppDomain.CurrentDomain.BaseDirectory + "/CommonSelect.xml")
            End If
        End Sub
#End Region

#Region "データアクセスメソッド"
        ''' <summary>
        ''' 現在時刻を取得する。
        ''' </summary>
        ''' <returns>現在時刻</returns>
        Public Function GetSysdate() As DateTime
            ' SelectCommandの設定
            Adapter.SelectCommand = CreateCommand("SELECT CURRENT_TIMESTAMP")
            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Dim cnt As Integer = Adapter.Fill(ds)

            Return If(cnt > 0, DirectCast(ds.Tables(0).Rows(0)(0), DateTime), DateTime.Now)
        End Function

        ''' <summary>
        ''' 指定された検索IDの検索を指定された条件で実行する。
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argParams">パラメータ</param>
        ''' <returns>検索結果</returns>
        Public Function [Select](argSelectId As String, ParamArray argParams As Object()) As DataTable
            ' SELECT文の設定
            Dim rows As DataRow() = s_selectStatementTable.[Select]("SelectId = '" & argSelectId & "'")
            If rows.Length = 0 Then
                Throw New Exception("CommonSelect.xmlに""" & argSelectId & """が登録されていません。")
            End If
            Dim statement As String = rows(0)("Statement").ToString()

            Dim selectCommand As IDbCommand = CreateCommand(statement)
            Adapter.SelectCommand = selectCommand
            ' パラメータの設定
            For i As Integer = 0 To argParams.Length - 1
                selectCommand.Parameters.Add(CreateCmdParam((i + 1).ToString(), argParams(i)))
            Next
            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Adapter.Fill(ds)
            ' 検索結果の返却
            Return ds.Tables(0)
        End Function

        ''' <summary>
        ''' 最大検索件数を返す。
        ''' </summary>
        ''' <param name="argId">画面ID</param>
        ''' <returns>最大検索件数</returns>
        Public Function GetMaxRow(Optional argId As String = Nothing) As Integer
            If argId Is Nothing Then
                argId = CMInformationManager.ClientInfo.FormId
            End If

            Dim result As DataTable = [Select]("CMSM汎用基準値", "V001", argId)
            If result.Rows.Count > 0 AndAlso Not IsDBNull(result.Rows(0)("基準値１")) Then
                Return Convert.ToInt32(result.Rows(0)("基準値１"))
            Else
                Return 1000
            End If
        End Function

        ''' <summary>
        ''' 操作ログを記録する。
        ''' </summary>
        ''' <param name="argFormName">画面名</param>
        Public Sub WriteOperationLog(argFormName As String)
            ' コネクション自動オープン判定フラグ
            Dim isClosed As Boolean = Connection.State = ConnectionState.Closed

            Try
                ' コネクションを開く
                If isClosed Then
                    Connection.Open()
                End If
                ' INSERT文の設定
                Dim cmd As IDbCommand = CreateCommand(INSERT_OPLOG_SQL)
                ' パラメータの設定
                cmd.Parameters.Add(CreateCmdParam("画面ID", CMInformationManager.ClientInfo.FormId))
                cmd.Parameters.Add(CreateCmdParam("画面名", argFormName))
                cmd.Parameters.Add(CreateCmdParam("ユーザID", CMInformationManager.UserInfo.Id))
                cmd.Parameters.Add(CreateCmdParam("端末ID", CMInformationManager.ClientInfo.MachineName))
                cmd.Parameters.Add(CreateCmdParam("ＡＰサーバ", Environment.MachineName))
                ' INSERT実行
                cmd.ExecuteNonQuery()
            Finally
                If isClosed Then
                    ' コネクションを破棄する
                    Connection.Close()
                End If
            End Try
        End Sub

        ''' <summary>
        ''' 汎用基準値から区分値名称を取得する。
        ''' </summary>
        ''' <param name="argKbnList">基準値分類CDのリスト</param>
        ''' <returns>区分値名称のDataTable</returns>
        Public Function SelectKbn(ParamArray argKbnList As String()) As DataTable
            ' INの中の条件を作成
            Dim sb As New StringBuilder()
            For i As Integer = 1 To argKbnList.Length
                If i > 1 Then
                    sb.Append(",")
                End If
                sb.AppendFormat("@{0}", i)
            Next

            ' IDbCommand作成
            Dim cmd As IDbCommand = CreateCommand(String.Format(SELECT_KBN_SQL, sb))
            Adapter.SelectCommand = cmd

            ' パラメータを設定
            For Each val As String In argKbnList
                cmd.Parameters.Add(CreateCmdParam("1", val))
            Next

            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Dim cnt As Integer = Adapter.Fill(ds)
            ' 表示名の設定
            ds.Tables(0).Columns("表示名").Expression = "[基準値CD] + ' ' + [基準値名]"

            Return ds.Tables(0)
        End Function

        ''' <summary>
        ''' 参照範囲, 更新許可を検索する。
        ''' </summary>
        ''' <param name="argFormId">画面ＩＤ</param>
        ''' <param name="argIsRange">True:参照範囲, False:更新許可</param>
        ''' <returns>True:会社、更新可, False:拠点、更新不可</returns>
        Public Function GetRangeCanUpdate(argFormId As String, argIsRange As Boolean) As Boolean
            ' ロールの条件作成
            Dim roles As String() = CMInformationManager.UserInfo.Roles
            Dim builder As New StringBuilder()

            If roles IsNot Nothing AndAlso roles.Length > 0 Then
                builder.Append("'" & roles(0) & "'")
                For i As Integer = 1 To roles.Length - 1
                    builder.AppendFormat(",'{0}'", roles(i))
                Next
            Else
                Return False
            End If

            ' SELECT文の設定
            Dim cmd As IDbCommand = CreateCommand(String.Format(SELECT_RANGE_CANUPDATE_SQL, If(argIsRange, "CMSM参照範囲", "CMSM更新許可"), builder.ToString()))
            ' パラメータの設定
            cmd.Parameters.Add(CreateCmdParam("組織CD", CMInformationManager.UserInfo.SoshikiCd))
            cmd.Parameters.Add(CreateCmdParam("画面ID", argFormId))

            ' コマンド設定
            Adapter.SelectCommand = cmd

            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Adapter.Fill(ds)

            Return ds.Tables(0).[Select]("許否フラグ = True").Count() > 0
        End Function

        ''' <summary>
        ''' 汎用基準値マスタを検索する。
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argIsOver">最大検索件数オーバーフラグ</param>
        ''' <returns>検索結果</returns>
        Public Function SelectSub(argSelectId As String, argParam As List(Of CMSelectParam), ByRef argIsOver As Boolean) As DataTable
            ' SELECT文の取得
            Dim rows As DataRow() = s_selectStatementTable.[Select]("SelectId = '" & argSelectId & "'")
            If rows.Length = 0 Then
                Throw New Exception("CommonSelect.xmlに""" & argSelectId & """が登録されていません。")
            End If

            ' SELECT文作成
            Dim selectSql As New StringBuilder("SELECT TOP 1001 * FROM (")
            selectSql.Append(rows(0)("Statement").ToString())

            ' WHERE句追加
            AddWhere(selectSql, argParam)

            ' 絞込み条件追加
            selectSql.Append(") A ORDER BY ROWNUMBER")

            ' SELECT文の設定
            Dim cmd As IDbCommand = CreateCommand(selectSql.ToString())
            Adapter.SelectCommand = cmd

            ' パラメータの設定
            Dim pCnt As Integer = 1
            For Each param As CMSelectParam In argParam
                If param.paramFrom IsNot Nothing Then
                    ' パラメータ名の取得
                    Dim name As String
                    If String.IsNullOrEmpty(param.condtion) Then
                        name = pCnt.ToString()
                        pCnt += 1
                    Else
                        name = param.condtion.Substring(param.condtion.IndexOf("@") + 1)
                    End If

                    cmd.Parameters.Add(CreateCmdParam(name, param.paramFrom))
                End If
            Next

            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Dim cnt As Integer = Adapter.Fill(ds)

            ' 最大検索件数オーバーの場合、最終行を削除
            If cnt > 1000 Then
                argIsOver = True
                ds.Tables(0).Rows.RemoveAt(cnt - 1)
            Else
                argIsOver = False
            End If

            ' 検索結果の返却
            Return ds.Tables(0)
        End Function

        ''' <summary>
        ''' 更新者を指定された条件で検索する。
        ''' </summary>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argTables">テーブル名の配列</param>
        ''' <param name="argIsOver">最大検索件数オーバーフラグ</param>
        ''' <returns>検索結果</returns>
        Public Function SelectUpdSub(argParam As List(Of CMSelectParam), argTables As String(), ByRef argIsOver As Boolean) As DataTable
            ' SQL文の作成
            Dim union As New StringBuilder()
            ' 副問い合わせの作成
            If argTables IsNot Nothing AndAlso argTables.Length > 0 Then
                union.Append("SELECT DISTINCT 更新者ID FROM ").Append(argTables(0))

                For i As Integer = 1 To argTables.Length - 1
                    union.Append(" UNION SELECT 更新者ID FROM ").Append(argTables(i))
                Next
            End If

            ' 組織階層が全社でなければ、会社の条件を追加
            Dim uinfo As CMUserInfo = CMInformationManager.UserInfo
            If uinfo.SoshikiKaisoKbn <> CMSoshikiKaiso.ALL Then
                argParam.Add(New CMSelectParam("S.組織CD", "= @組織CD", uinfo.SoshikiCd))
            End If

            ' WHERE句作成
            Dim where As New StringBuilder()
            AddWhere(where, argParam)

            ' SELECT文の設定
            Dim cmd As IDbCommand = CreateCommand(String.Format(SELECT_UPD_SQL, union, where))
            Adapter.SelectCommand = cmd

            ' パラメータの設定
            SetParameter(cmd, argParam)

            ' データセットの作成
            Dim ds As New DataSet()
            ' データの取得
            Dim cnt As Integer = Adapter.Fill(ds)

            ' 最大検索件数オーバーの場合、最終行を削除
            If cnt > 1000 Then
                argIsOver = True
                ds.Tables(0).Rows.RemoveAt(cnt - 1)
            Else
                argIsOver = False
            End If

            ' 検索結果の返却
            Return ds.Tables(0)
        End Function

        ''' <summary>
        ''' 共通検索を使用してデータの存在チェックを行う。
        ''' 存在しなかった場合は、CMExceptionをthrowする。
        ''' </summary>
        ''' <param name="argSelectId">検索ID</param>
        ''' <param name="argTableName">存在チェック対象のテーブル名</param>
        ''' <param name="argRow">存在チェック対象データを含むDataRow</param>
        ''' <param name="argColumnName">存在チェック対象データのDataRow中の列名</param>
        ''' <param name="argParams">共通検索部品に渡すパラメータ(指定なしの場合はargColumnNameで
        '''指定した列のデータをパラメータに使用する)</param>
        Public Sub ExistCheck(argSelectId As String, argTableName As String, argRow As DataRow, argColumnName As String, ParamArray argParams As Object())
            Dim cnt As Integer
            ' argParamsの指定がなかった場合
            If argParams.Length = 0 Then
                cnt = [Select](argSelectId, argRow(argColumnName)).Rows.Count
            Else
                ' argParamsの指定があった
                cnt = [Select](argSelectId, argParams).Rows.Count
            End If
            ' 存在チェック
            If cnt = 0 Then
                ' メッセージの作成
                Dim message As New CMMessage("WV107", New CMRowField(CMUtil.GetRowNumber(argRow), argColumnName), argTableName)
                ' 例外を発生
                Throw New CMException(message)
            End If
        End Sub

        ''' <summary>
        ''' XMLファイルの設定からデータの存在チェックを行う。
        ''' </summary>
        ''' <param name="argTable">存在チェック対象のDataTable</param>
        ''' <param name="argFname">読み込むXMLファイル名(拡張子なし)</param>
        Public Sub ExistCheckFomXml(argTable As DataTable, Optional argFname As String = Nothing)
            ' デフォルト設定
            If argFname Is Nothing Then
                argFname = argTable.TableName
            End If

            ' データセットにファイルを読み込み
            Dim ds As New CMEntityDataSet()
            ds.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argFname & ".xml"))

            ' 入力値チェックループ
            For Each row As DataRow In argTable.Rows
                ' 削除データはチェックしない
                If row.RowState = DataRowState.Deleted Then
                    Continue For
                End If

                ' 存在チェック項目ループ
                For Each irow As CMEntityDataSet.項目Row In ds.項目.[Select]("Len(存在チェックテーブル名) > 0")
                    ' キー項目は新規のみチェック
                    If irow.Key AndAlso row.RowState <> DataRowState.Added Then
                        Continue For
                    End If

                    Dim checkParams As New List(Of Object)()
                    Dim paramText As String = ""

                    ' 共通検索パラメータ取得
                    If (InlineAssignHelper(paramText, irow.共通検索パラメータ)).Length > 0 Then
                        For Each p0 As String In paramText.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
                            Dim p As String = p0.TrimStart()
                            If p.Length < 2 Then
                                Continue For
                            End If

                            ' 'から始まる場合はそのまま設定
                            If p(0) = "'"c Then
                                checkParams.Add(p.Substring(1))
                                ' "#"から始まる場合はUserInfoから設定
                            ElseIf p(0) = "#"c Then
                                Dim pi As PropertyInfo = CMInformationManager.UserInfo.[GetType]().GetProperty(p.Substring(1))
                                checkParams.Add(pi.GetValue(CMInformationManager.UserInfo, Nothing))
                            Else
                                ' Rowの値を取得
                                checkParams.Add(row(p))
                            End If
                        Next
                    End If

                    ' 存在チェック
                    ExistCheck(irow.共通検索ID, irow.存在チェックテーブル名, row, irow.項目名, checkParams.ToArray())
                Next
            Next
        End Sub
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
#End Region
    End Class
End Namespace
