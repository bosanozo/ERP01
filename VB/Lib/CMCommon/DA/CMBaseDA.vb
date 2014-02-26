Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Imports log4net

Imports NEXS.ERP.CM.Common

Namespace DA
    ''' <summary>
    ''' データアクセス層の基底クラス
    ''' </summary>
    Public Class CMBaseDA
#Region "ロガーフィールド"
        Private m_logger As ILog
#End Region

#Region "プロパティ"
        ''' <summary>
        ''' ロガー
        ''' </summary>
        Protected ReadOnly Property Log As ILog
            Get
                Return m_logger
            End Get
        End Property

        ''' <summary>コネクション</summary>
        Public Property Connection As IDbConnection

        ''' <summary>データアダプタ</summary>
        Protected Property Adapter As IDbDataAdapter
#End Region

        ' 行ロックタイムアウトエラーNO
        Private Const LOCK_TIMEOUT_ERR As Integer = 1222

        ' PKEY制約違反エラーNO
        Private Const PKEY_ERR As Integer = 2627

#Region "SQL文"
        ''' <summary>
        ''' INSERT文
        ''' </summary>
        Private Const INSERT_SQL As String =
            "INSERT INTO {0} (" &
            "{1}" &
            "作成日時," &
            "作成者ID," &
            "作成者IP," &
            "作成PG," &
            "更新日時," &
            "更新者ID," &
            "更新者IP," &
            "更新PG" &
            ")VALUES(" &
            "{2}" &
            "@更新日時," &
            "@更新者ID," &
            "@更新者IP," &
            "@更新PG," &
            "@更新日時," &
            "@更新者ID," &
            "@更新者IP," &
            "@更新PG)"

        ''' <summary>
        ''' UPDATE文
        ''' </summary>
        Private Const UPDATE_SQL As String =
            "UPDATE {0} SET " &
            "{1}" &
            "更新日時 = @更新日時," &
            "更新者ID = @更新者ID," &
            "更新者IP = @更新者IP," &
            "更新PG = @更新PG " &
            "WHERE "

        ''' <summary>
        ''' DELETE文
        ''' </summary>
        Private Const DELETE_SQL As String = "DELETE FROM {0} WHERE "

        ''' <summary>
        ''' 排他チェック用SELECT文
        ''' </summary>
        Private Const CONC_CHEK_SQL As String =
            "SET LOCK_TIMEOUT 10000 " &
            "SELECT 更新日時, 更新者ID, 更新者IP, 更新PG, 排他用バージョン " &
            "FROM {0} WITH(ROWLOCK, UPDLOCK) WHERE "

        ''' <summary>
        ''' 存在チェック用SELECT文
        ''' </summary>
        Private Const EXIST_CHEK_SQL As String = "SELECT TOP 1 COUNT(*) FROM {0} WHERE ROWNUM <= 1"

        ''' <summary>
        ''' 監査証跡INSERT文
        ''' </summary>
        Private Const INSERT_AUDITLOG_SQL As String =
            "INSERT INTO CMST監査証跡 (" &
            "テーブル名," &
            "更新区分," &
            "キー," &
            "内容," &
            "作成日時," &
            "作成者ID," &
            "作成者IP," &
            "作成PG," &
            "更新日時," &
            "更新者ID," &
            "更新者IP," &
            "更新PG" &
            ")VALUES(" &
            "@テーブル名," &
            "@更新区分," &
            "@キー," &
            "@内容," &
            "@更新日時," &
            "@更新者ID," &
            "@更新者IP," &
            "@更新PG," &
            "@更新日時," &
            "@更新者ID," &
            "@更新者IP," &
            "@更新PG)"
#End Region

#Region "SELECT文作成用SQL"
        Private Const TOROKU_COLS As String =
            "A.作成日時," &
            "A.作成者ID," &
            "US1.ユーザ名 作成者名," &
            "A.作成者IP," &
            "A.作成PG," &
            "A.更新日時," &
            "A.更新者ID," &
            "US2.ユーザ名 更新者名," &
            "A.更新者IP," &
            "A.更新PG"

        Private Const TOROKU_JOIN As String =
            "LEFT JOIN CMSMユーザ US1 ON US1.ユーザID = A.作成者ID " &
            "LEFT JOIN CMSMユーザ US2 ON US2.ユーザID = A.更新者ID "

        Private Const ROWNUMBER_CONDITION As String =
            "WHERE ROWNUMBER <= @最大検索件数 "

        ''' <summary>
        ''' SELECT文
        ''' </summary>
        Private Const SELECT_SQL As String =
            "SELECT " &
            "'0' 削除," &
            "{0}" &
            TOROKU_COLS & "," &
            "A.排他用バージョン," &
            "A.ROWNUMBER " &
            "FROM (SELECT A.*, ROW_NUMBER() OVER (ORDER BY {1}) - 1 ROWNUMBER " &
            "FROM {2} A{3}) A " &
            TOROKU_JOIN &
            "{4}" &
            ROWNUMBER_CONDITION &
            "ORDER BY ROWNUMBER"

        ''' <summary>
        ''' CSV出力用SELECT文
        ''' </summary>
        Private Const SELECT_CSV_SQL As String =
            "SELECT " &
            "{0}" &
            TOROKU_COLS &
            " FROM {2} A " &
            TOROKU_JOIN &
            "{4}{3}" &
            "ORDER BY {1}"

        ''' <summary>
        ''' 登録画面SELECT文
        ''' </summary>
        Private Const SELECT_EDIT_SQL As String =
            "SELECT " &
            "{0}" &
            TOROKU_COLS & "," &
            "A.排他用バージョン," &
            "0 ROWNUMBER " &
            "FROM {1} A " &
            TOROKU_JOIN &
            "{3}{2}"
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            ' ロガーを取得
            m_logger = LogManager.GetLogger(Me.[GetType]())

            ' データアダプタはfactoryから作成する
            Dim factory As DbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient")
            Adapter = factory.CreateDataAdapter()
        End Sub
#End Region

#Region "publicメソッド"
        ''' <summary>
        ''' 指定されたXMLファイルからSELECT文を作成し、検索を実行する。
        ''' </summary>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argSelectType">検索種別</param>
        ''' <param name="argMaxRow">最大検索件数</param>
        ''' <param name="argIsOver">最大検索件数オーバーフラグ</param>
        ''' <param name="argFname">読み込むXMLファイル名(拡張子なし)</param>
        ''' <returns>検索結果</returns>
        Public Function SelectFromXml(argParam As List(Of CMSelectParam), argSelectType As CMSelectType, argMaxRow As Integer,
                                      ByRef argIsOver As Boolean, ParamArray argFname As String()) As DataSet
            ' データセットの作成
            Dim result As New DataSet()

            For Each fname As String In argFname
                ' データセットにファイルを読み込み
                Dim ds As New CMEntityDataSet()
                ds.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", fname & ".xml"))

                ' テーブル名を取得
                Dim tableName As String = ds.エンティティ(0).テーブル名

                Dim sb As New StringBuilder()
                Dim orderSb As New StringBuilder()
                For Each row As CMEntityDataSet.項目Row In ds.項目
                    ' 項目名に.が無いものは駆動表から取得
                    Dim col As String = If(row.項目名.Contains("."), row.項目名, "A." & Convert.ToString(row.項目名))

                    ' 検索列を作成
                    ' SourceColumnの指定がある場合は別名をつける
                    If String.IsNullOrEmpty(row.SourceColumn) Then
                        sb.AppendFormat("{0},", col)
                    Else
                        sb.AppendFormat("{0} {1},", col, row.SourceColumn)
                    End If

                    ' ソート条件を作成
                    If row.Key Then
                        If orderSb.Length > 0 Then
                            orderSb.Append(" ,")
                        End If
                        orderSb.Append(col)
                    End If
                Next

                ' ソート条件
                Dim order As String = ds.エンティティ(0).OrderBy
                If String.IsNullOrEmpty(order) Then
                    order = orderSb.ToString()
                End If

                ' WHERE句作成
                Dim where As New StringBuilder()
                AddWhere(where, argParam)

                ' SELECT文の設定
                Dim cmd As IDbCommand = CreateCommand(CreateSelectSql(sb.ToString(), tableName, where.ToString(), ds.エンティティ(0).Join + " ", order, argSelectType))
                Adapter.SelectCommand = cmd

                ' パラメータの設定
                SetParameter(cmd, argParam)
                ' 一覧検索の場合 かつ 最初の検索の場合、最大検索件数で制限
                If argSelectType = CMSelectType.List AndAlso result.Tables.Count = 0 Then
                    cmd.Parameters.Add(CreateCmdParam("最大検索件数", argMaxRow))
                Else
                    cmd.CommandText = cmd.CommandText.Replace(ROWNUMBER_CONDITION, "")
                End If

                ' データの取得
                Adapter.Fill(result)
                ' テーブル名を設定
                result.Tables("Table").TableName = tableName
            Next

            ' 最初のデータテーブルで検索件数オーバーを判定
            Dim cnt As Integer = result.Tables(0).Rows.Count

            ' 一覧検索で最大検索件数オーバーの場合、最終行を削除
            If argSelectType = CMSelectType.List AndAlso cnt >= argMaxRow Then
                argIsOver = True
                result.Tables(0).Rows.RemoveAt(cnt - 1)
            Else
                argIsOver = False
            End If

            ' 検索結果の返却
            Return result
        End Function

        ''' <summary>
        ''' 指定されたテーブルに更新データを登録する。
        ''' </summary>
        ''' <param name="argUpdateData">更新データ</param>
        ''' <param name="argOperationTime">操作時刻</param>
        ''' <param name="argCmdSettings">Command設定</param>
        ''' <returns>登録したレコード数</returns>
        Public Function Update(argUpdateData As DataSet, argOperationTime As DateTime, Optional argCmdSettings As CMCmdSettings = Nothing) As Integer
            ' デフォルト設定
            If argCmdSettings Is Nothing Then
                argCmdSettings = New CMCmdSettings()
                For Each table As DataTable In argUpdateData.Tables
                    argCmdSettings.AddFomXml(table.TableName)
                Next
            End If

            Dim cnt As Integer = 0
            Dim tableCnt As Integer = argCmdSettings.CmdSettings.Count

            ' 1テーブルの場合
            If tableCnt = 1 Then
                ' 登録実行
                cnt = UpdateTable(argCmdSettings(0), argUpdateData.Tables(argCmdSettings(0).Name), argOperationTime)
            Else
                ' 複数テーブルの場合
                ' Command設定の逆順に削除データを登録
                For i As Integer = tableCnt - 1 To 1 Step -1
                    Dim table As DataTable = argUpdateData.Tables(argCmdSettings(i).Name).GetChanges(DataRowState.Deleted)
                    If table IsNot Nothing AndAlso table.Rows.Count > 0 Then
                        cnt += UpdateTable(argCmdSettings(i), table, argOperationTime)
                    End If
                Next

                ' 最初のテーブルのデータを登録
                cnt += UpdateTable(argCmdSettings(0), argUpdateData.Tables(argCmdSettings(0).Name), argOperationTime)

                ' Command設定の順に新規、修正データを登録
                For i As Integer = 1 To tableCnt - 1
                    Dim table As DataTable = argUpdateData.Tables(argCmdSettings(i).Name).GetChanges(DataRowState.Added Or DataRowState.Modified)
                    If table IsNot Nothing AndAlso table.Rows.Count > 0 Then
                        cnt += UpdateTable(argCmdSettings(i), table, argOperationTime)
                    End If
                Next
            End If

            Return cnt
        End Function

        ''' <summary>
        ''' 指定されたテーブルに更新データをアップロードする。
        ''' </summary>
        ''' <param name="argUpdateData">更新データ</param>
        ''' <param name="argOperationTime">操作時刻</param>
        ''' <param name="argCmdSettings">Command設定</param>
        ''' <returns>登録したレコード数</returns>
        Public Function Upload(argUpdateData As DataSet, argOperationTime As DateTime, Optional argCmdSettings As CMCmdSettings = Nothing) As Integer
            ' デフォルト設定
            If argCmdSettings Is Nothing Then
                argCmdSettings = New CMCmdSettings()
                For Each table As DataTable In argUpdateData.Tables
                    argCmdSettings.AddFomXml(table.TableName)
                Next
            End If

            Dim cnt As Integer = 0

            ' Command設定の順に新規、修正データを登録
            For i As Integer = 0 To argCmdSettings.CmdSettings.Count - 1
                Dim table As DataTable = argUpdateData.Tables(argCmdSettings(i).Name)
                If table IsNot Nothing AndAlso table.Rows.Count > 0 Then
                    cnt += UploadTable(argCmdSettings(i), table, argOperationTime)
                End If
            Next

            Return cnt
        End Function
#End Region

#Region "protectedメソッド"
#Region "SQL作成メソッド"
        ''' <summary>
        ''' 検索種別に応じたSELECT文を作成する。駆動表のシノニムはAとする。
        ''' </summary>
        ''' <param name="argCols">検索列</param>
        ''' <param name="argTableName">検索テーブル名</param>
        ''' <param name="argWhere">WHERE句</param>
        ''' <param name="argJoin">JOIN句</param>
        ''' <param name="argOrder">並び順</param>
        ''' <param name="argSelectType">検索種別</param>
        ''' <returns>SELECT文</returns>
        Protected Function CreateSelectSql(argCols As String, argTableName As String, argWhere As String, argJoin As String, argOrder As String, argSelectType As CMSelectType) As String
            Dim tableName As String = argTableName

            ' 登録画面の場合
            If argSelectType = CMSelectType.Edit Then
                Return String.Format(SELECT_EDIT_SQL, argCols, tableName, argWhere, argJoin)
            Else
                ' 一覧検索, CSV出力の場合
                Return String.Format(If(argSelectType = CMSelectType.List, SELECT_SQL, SELECT_CSV_SQL), argCols, argOrder, tableName, argWhere, argJoin)
            End If
        End Function

        ''' <summary>
        ''' StringBuilderに検索条件を追加する。
        ''' </summary>
        ''' <param name="where">検索条件を追加するStringBuilder</param>
        ''' <param name="argParam">検索条件</param>
        Protected Sub AddWhere(where As StringBuilder, argParam As List(Of CMSelectParam))
            ' 空白で終わってなければ、空白追加
            If where.Length > 0 AndAlso where(where.Length - 1) <> " "c Then
                where.Append(" ")
            End If

            ' 追加の条件
            For Each param As CMSelectParam In argParam
                If String.IsNullOrEmpty(param.condtion) Then
                    Continue For
                End If
                where.Append(If(where.Length > 0, "AND ", " WHERE "))
                If Not String.IsNullOrEmpty(param.name) Then
                    ' テーブルの指定がない場合は、Aをつける
                    If Not param.name.Contains("."c) Then
                        where.Append("A.")
                    End If
                    where.AppendFormat("{0} ", param.name)
                End If
                where.Append(param.condtion).Append(" ")
            Next
        End Sub

        ''' <summary>
        ''' テーブル項目列DataTableからINSERT文, UPDATE文を作成する。
        ''' </summary>
        ''' <param name="argCmdSetting">Command設定</param>
        ''' <param name="argInsertSql">INSERT文</param>
        ''' <param name="argUpdateSql">UPDATE文</param>
        Protected Sub CreateInsertUpdateSql(argCmdSetting As CMCmdSetting, ByRef argInsertSql As String, ByRef argUpdateSql As String)
            Dim ins1 As New StringBuilder()
            Dim ins2 As New StringBuilder()
            Dim upd As New StringBuilder()

            ' テーブル項目列でループ
            For Each row As CMCmdParam In argCmdSetting.ColumnParams
                Dim valueFmt As String

                ' キー項目にNULLは設定させない
                If row.IsKey Then
                    If row.DbType = CMDbType.金額 OrElse row.DbType = CMDbType.整数 OrElse row.DbType = CMDbType.小数 Then
                        valueFmt = "ISNULL(@{0}, 0),"
                        ' 日付型は対応なし
                    ElseIf row.DbType = CMDbType.日時 OrElse row.DbType = CMDbType.日付 Then
                        valueFmt = "@{0},"
                    Else
                        valueFmt = "ISNULL(@{0}, ' '),"
                    End If
                Else
                    valueFmt = "@{0},"
                End If

                Dim colName As String = row.Name

                ' INSERT文作成
                ins1.Append(colName).Append(",")
                ins2.AppendFormat(valueFmt, row.Name)

                ' 従属項目の場合
                If Not row.IsKey Then
                    ' UPDATE文作成
                    upd.Append(colName).Append(" = ").AppendFormat(valueFmt, row.Name)
                End If
            Next

            Dim tname As String = argCmdSetting.Name
            argInsertSql = String.Format(INSERT_SQL, tname, ins1, ins2)
            argUpdateSql = String.Format(UPDATE_SQL, tname, upd)
        End Sub
#End Region

        ''' <summary>
        ''' 接続に関連付けられたCommandオブジェクトを作成する。
        ''' </summary>
        ''' <param name="argCommandText">Commandに設定するSQL文</param>
        ''' <returns>接続に関連付けられたCommandオブジェクト</returns>
        Protected Function CreateCommand(argCommandText As String) As IDbCommand
            Dim cmd As IDbCommand = Connection.CreateCommand()
            cmd.CommandText = argCommandText
            Return cmd
        End Function

        ''' <summary>
        ''' 接続に関連付けられたCommandのパラメータオブジェクトを作成する。
        ''' </summary>
        ''' <param name="argParameterName">パラメータ名</param>
        ''' <param name="argValue">値</param>
        ''' <returns>接続に関連付けられたCommandのパラメータオブジェクト</returns>
        Protected Function CreateCmdParam(argParameterName As String, argValue As Object) As IDbDataParameter
            Return New SqlParameter(argParameterName, argValue)
        End Function

        ''' <summary>
        ''' 接続に関連付けられたCommandのパラメータオブジェクトを作成する。
        ''' </summary>
        ''' <param name="argParameterName">パラメータ名</param>
        ''' <param name="argDbType">SqlDbType</param>
        ''' <returns>接続に関連付けられたCommandのパラメータオブジェクト</returns>
        Protected Function CreateCmdParam(argParameterName As String, argDbType As SqlDbType) As IDbDataParameter
            Return New SqlParameter(argParameterName, argDbType)
        End Function

        ''' <summary>
        ''' Commandに検索パラメータを設定する。
        ''' </summary>
        ''' <param name="argCmd">IDbCommand</param>
        ''' <param name="argParam">検索条件</param>
        Protected Sub SetParameter(argCmd As IDbCommand, argParam As List(Of CMSelectParam))
            Dim regex As New Regex("@\S+")

            For Each param As CMSelectParam In argParam
                ' プレースフォルダ名を取得
                Dim mc As MatchCollection = regex.Matches(param.condtion)
                If mc.Count = 0 Then
                    Continue For
                End If

                If param.paramFrom IsNot Nothing AndAlso param.paramTo IsNot Nothing Then
                    argCmd.Parameters.Add(CreateCmdParam(mc(0).Value, param.paramFrom))
                    argCmd.Parameters.Add(CreateCmdParam(mc(1).Value, param.paramTo))
                Else
                    If param.paramFrom IsNot Nothing Then
                        argCmd.Parameters.Add(CreateCmdParam(mc(0).Value, param.paramFrom))
                    End If
                    If param.paramTo IsNot Nothing Then
                        argCmd.Parameters.Add(CreateCmdParam(mc(0).Value, param.paramTo))
                    End If
                End If
            Next
        End Sub

#Region "データ登録メソッド"
        ''' <summary>
        ''' データベース更新パラメータを設定する
        ''' </summary>
        ''' <param name="argDataRow">パラメータ設定対象のDataRow</param>
        ''' <param name="argUpdateTime">データベースに記録する更新時刻</param>
        Protected Sub SetUpdateParameter(argDataRow As DataRow, argUpdateTime As DateTime)
            argDataRow("更新日時") = argUpdateTime
            argDataRow("更新者ID") = CMInformationManager.UserInfo.Id
            argDataRow("更新者IP") = CMInformationManager.ClientInfo.MachineName
            argDataRow("更新PG") = CMInformationManager.ClientInfo.FormId
        End Sub

        ''' <summary>
        ''' 指定されたテーブルに更新データを登録する。
        ''' </summary>
        ''' <param name="argCmdSetting">Command設定</param>
        ''' <param name="argDataTable">更新データを格納したDataTable</param>
        ''' <param name="argSysdate">データベースに記録する更新時刻</param>
        ''' <param name="argInsertSql">INSERT文</param>
        ''' <param name="argUpdateSql">UPDATE文</param>
        ''' <returns>登録したレコード数</returns>
        Protected Function UpdateTable(argCmdSetting As CMCmdSetting, argDataTable As DataTable, argSysdate As DateTime, Optional argInsertSql As String = Nothing, Optional argUpdateSql As String = Nothing) As Integer
            Dim tname As String = argCmdSetting.Name

            ' 主キーの検索条件を取得
            Dim keyCond As String = argCmdSetting.GetKeyCondition()

            ' INSERT文, UPDATE文の自動設定
            If argInsertSql Is Nothing OrElse argUpdateSql Is Nothing Then
                ' INSERT文, UPDATE文を作成
                Dim insertSql As String = ""
                Dim updateSql As String = ""
                CreateInsertUpdateSql(argCmdSetting, insertSql, updateSql)
                ' nullの場合は作成したものを設定
                If argInsertSql Is Nothing Then
                    argInsertSql = insertSql
                End If
                If argUpdateSql Is Nothing Then
                    argUpdateSql = updateSql & keyCond
                End If
            End If

            ' INSERT, UPDATE, DELETEコマンドの作成
            Dim insertCommand As IDbCommand = CreateCommand(argInsertSql)
            Dim updateCommand As IDbCommand = CreateCommand(argUpdateSql)
            Dim deleteCommand As IDbCommand = CreateCommand(String.Format(DELETE_SQL, tname) & keyCond)
            ' 排他チェック用SELECTコマンドの作成
            Dim concCheckCommand As IDbCommand = CreateCommand(String.Format(CONC_CHEK_SQL, tname) & keyCond)

            ' INSERT, UPDATE, DELETE文の設定
            Adapter.InsertCommand = insertCommand
            Adapter.UpdateCommand = updateCommand
            Adapter.DeleteCommand = deleteCommand

            ' INSERT, UPDATE, DELETE, 排他チェック用SELECTコマンドのパラメータを設定
            AddCommandParameter(insertCommand, updateCommand, deleteCommand, concCheckCommand, argCmdSetting)

            ' コネクション自動オープン判定フラグ
            Dim isClosed As Boolean = Connection.State = ConnectionState.Closed

            Try
                If isClosed Then
                    Connection.Open()
                End If

                ' データの更新ループ
                For Each row As DataRow In argDataTable.Rows
                    ' データベース更新パラメータの設定
                    If row.RowState = DataRowState.Added OrElse row.RowState = DataRowState.Modified Then
                        SetUpdateParameter(row, argSysdate)
                    End If

                    ' 更新、削除データの場合、排他チェックを実施
                    If row.RowState = DataRowState.Modified OrElse row.RowState = DataRowState.Deleted Then
                        DoConcCheck(concCheckCommand, row, argCmdSetting)
                    End If
                Next
            Finally
                If isClosed Then
                    Connection.Close()
                End If
            End Try

            ' データの登録を実行
            Dim cnt As Integer = DoUpdate(argDataTable)

            ' 監査証跡出力
            WriteAuditLog(argCmdSetting, argDataTable, argSysdate)

            Return cnt
        End Function

        ''' <summary>
        ''' 指定されたテーブルに、指定されたデータをアップロードする。
        ''' </summary>
        ''' <param name="argCmdSetting">Command設定</param>
        ''' <param name="argDataTable">更新データを格納したDataTable</param>
        ''' <param name="argUpdateTime">データベースに記録する更新時刻</param>
        ''' <param name="argInsertSql">INSERT文</param>
        ''' <param name="argUpdateSql">UPDATE文</param>
        ''' <returns>登録したレコード数</returns>
        Protected Function UploadTable(argCmdSetting As CMCmdSetting, argDataTable As DataTable, argUpdateTime As DateTime, Optional argInsertSql As String = Nothing, Optional argUpdateSql As String = Nothing) As Integer
            ' 更新用の列をDataTableに追加
            argDataTable.Columns.Add("更新日時", GetType(DateTime))
            argDataTable.Columns.Add("更新者ID")
            argDataTable.Columns.Add("更新者IP")
            argDataTable.Columns.Add("更新PG")

            ' テーブル名を取得
            Dim tname As String = argCmdSetting.Name

            ' 主キーの検索条件を取得
            Dim keyCond As String = argCmdSetting.GetKeyCondition()

            ' INSERT文, UPDATE文の自動設定
            If argInsertSql Is Nothing OrElse argUpdateSql Is Nothing Then
                ' INSERT文, UPDATE文を作成
                Dim insertSql As String = ""
                Dim updateSql As String = ""
                CreateInsertUpdateSql(argCmdSetting, insertSql, updateSql)
                ' nullの場合は作成したものを設定
                If argInsertSql Is Nothing Then
                    argInsertSql = insertSql
                End If
                If argUpdateSql Is Nothing Then
                    argUpdateSql = updateSql & keyCond
                End If
            End If

            ' INSERT, UPDATEコマンドの作成
            Dim insertCommand As IDbCommand = CreateCommand(argInsertSql)
            Dim updateCommand As IDbCommand = CreateCommand(argUpdateSql)

            ' 存在チェック用SELECT文の作成
            Dim existCheckCommand As IDbCommand = CreateCommand(String.Format(CONC_CHEK_SQL, tname) & keyCond)

            ' INSERT, UPDATE文の設定
            Adapter.InsertCommand = insertCommand
            Adapter.UpdateCommand = updateCommand

            ' INSERT, UPDATE, 存在チェック用SELECTコマンドのパラメータを設定
            AddCommandParameter(insertCommand, updateCommand, Nothing, existCheckCommand, argCmdSetting)

            ' コネクション自動オープン判定フラグ
            Dim isClosed As Boolean = Connection.State = ConnectionState.Closed

            Try
                If isClosed Then
                    Connection.Open()
                End If

                ' データの更新ループ
                For Each row As DataRow In argDataTable.Rows
                    ' データベース更新パラメータの設定
                    SetUpdateParameter(row, argUpdateTime)
                    ' 存在するかチェック
                    DoUploadCheck(existCheckCommand, row, argCmdSetting)
                Next
            Finally
                If isClosed Then
                    Connection.Close()
                End If
            End Try

            ' データの登録を実行
            Dim cnt As Integer = 0
            Try
                Dim table As DataTable = argDataTable.Copy()
                table.TableName = "Table"
                Dim ds As New DataSet()
                ds.Tables.Add(table)
                cnt = Adapter.Update(ds)
            Catch ex As SqlException
                ' 未登録行を取得
                Dim rows As DataRow() = argDataTable.[Select](Nothing, Nothing, DataViewRowState.ModifiedCurrent Or DataViewRowState.Added)
                ' 行番号を取得
                Dim rowNumber As Integer = argDataTable.Rows.IndexOf(rows(0)) + 1
                ' メッセージコードを設定
                Dim msgCode As String = If(ex.Number = PKEY_ERR, "WV001", "EV002")
                ' メッセージ設定
                Dim message As String = If(ex.Number = PKEY_ERR, CMMessageManager.GetMessage(msgCode), ex.Message)
                ' メッセージ作成
                Dim msgData As New CMMessage(msgCode, New CMRowField(rowNumber), message)
                ' 例外発生
                Throw New CMException(msgData, ex)
            End Try
            Return cnt
        End Function
#End Region
#End Region

#Region "privateメソッド"
        ''' <summary>
        ''' 排他チェックを実行する。
        ''' </summary>
        ''' <param name="argConcCheckCommand">排他チェック用コマンド</param>
        ''' <param name="argRow">排他チェック対象のDataRow</param>
        ''' <param name="argCmdSetting">Command設定</param>
        Private Sub DoConcCheck(argConcCheckCommand As IDbCommand, argRow As DataRow, argCmdSetting As CMCmdSetting)
            ' 排他チェック用コマンドにパラメータ値を設定
            For Each row As CMCmdParam In argCmdSetting.ColumnParams
                If row.IsKey Then
                    Dim name As String = If(Not String.IsNullOrEmpty(row.SourceColumn), row.SourceColumn, row.Name)
                    DirectCast(argConcCheckCommand.Parameters(row.Name), IDbDataParameter).Value = argRow(name, DataRowVersion.Original)
                End If
            Next

            ' 検索実行
            Try
                Using reader As IDataReader = argConcCheckCommand.ExecuteReader()
                    ' レコードありの場合
                    If reader.Read() Then
                        Dim rowversion As Long = BitConverter.ToInt64(DirectCast(reader.GetValue(4), Byte()), 0)

                        ' データ更新チェック
                        If BitConverter.ToInt64(DirectCast(argRow("排他用バージョン", DataRowVersion.Original), Byte()), 0) <> rowversion Then
                            Dim updateTime As DateTime = reader.GetDateTime(0)
                            Dim userId As String = reader.GetString(1)
                            Dim hostname As String = reader.GetString(2)
                            Dim progId As String = reader.GetString(3)

                            ' データが更新されていた場合
                            ' メッセージコードの設定
                            Dim msgCode As String = If(argRow.RowState = DataRowState.Modified, "WV002", "WV004")
                            Dim message As CMMessage
                            Dim rowNumber As Integer = CMUtil.GetRowNumber(argRow)
                            ' 行番号ありの場合
                            If rowNumber >= 0 Then
                                message = New CMMessage(msgCode, New CMRowField(rowNumber), userId, updateTime, progId, hostname)
                            Else
                                ' 行番号なしの場合
                                message = New CMMessage(msgCode, userId, updateTime, progId, hostname)
                            End If
                            ' 例外発生
                            Throw New CMException(message)
                        End If
                    Else
                        ' レコードなしの場合
                        ' メッセージコードの設定
                        Dim msgCode As String = If(argRow.RowState = DataRowState.Modified, "WV003", "WV005")
                        Dim message As CMMessage
                        Dim rowNumber As Integer = CMUtil.GetRowNumber(argRow)
                        ' 行番号ありの場合
                        If rowNumber >= 0 Then
                            message = New CMMessage(msgCode, New CMRowField(rowNumber))
                        Else
                            ' 行番号なしの場合
                            message = New CMMessage(msgCode)
                        End If
                        ' 例外発生
                        Throw New CMException(message)
                    End If
                End Using
            Catch ex As SqlException
                ' リソースビジー以外はそのままthrow
                If ex.Number <> LOCK_TIMEOUT_ERR Then
                    Throw ex
                End If

                ' メッセージコードの設定
                Dim msgCode As String = If(argRow.RowState = DataRowState.Modified, "WV006", "WV007")
                Dim message As CMMessage
                Dim rowNumber As Integer = CMUtil.GetRowNumber(argRow)
                ' 行番号ありの場合
                If rowNumber >= 0 Then
                    message = New CMMessage(msgCode, New CMRowField(rowNumber))
                Else
                    ' 行番号なしの場合
                    message = New CMMessage(msgCode)
                End If
                ' 例外発生
                Throw New CMException(message)
            End Try
        End Sub

        ''' <summary>
        ''' 存在チェックを実行する。
        ''' </summary>
        ''' <param name="argExistCheckCommand">存在チェック用コマンド</param>
        ''' <param name="argRow">存在チェック対象のDataRow</param>
        ''' <param name="argCmdSetting">Command設定</param>
        Private Sub DoUploadCheck(argExistCheckCommand As IDbCommand, argRow As DataRow, argCmdSetting As CMCmdSetting)
            ' 存在チェック用コマンドにパラメータ値を設定
            For Each row As CMCmdParam In argCmdSetting.ColumnParams
                If row.IsKey Then
                    Dim name As String = If(Not String.IsNullOrEmpty(row.SourceColumn), row.SourceColumn, row.Name)
                    DirectCast(argExistCheckCommand.Parameters(row.Name), IDbDataParameter).Value = argRow(name)
                End If
            Next

            ' 検索実行
            Using reader As IDataReader = argExistCheckCommand.ExecuteReader()
                ' レコードありの場合
                If reader.Read() Then
                    ' 新規を通常に変更
                    argRow.AcceptChanges()
                    ' 更新に変更
                    argRow.SetModified()
                End If
            End Using
        End Sub

        ''' <summary>
        ''' パラメータ変数を設定する。
        ''' </summary>
        ''' <param name="argInsertCommand">INSERTコマンド</param>
        ''' <param name="argUpdateCommand">UPDATEコマンド</param>
        ''' <param name="argDeleteCommand">DELETEコマンド</param>
        ''' <param name="argConcCheckCommand">排他チェック用コマンド</param>
        ''' <param name="argCmdSetting">項目設定</param>
        Private Sub AddCommandParameter(argInsertCommand As IDbCommand, argUpdateCommand As IDbCommand, argDeleteCommand As IDbCommand, argConcCheckCommand As IDbCommand, argCmdSetting As CMCmdSetting)
            Dim updateCols As String() = {"更新日時", "更新者ID", "更新者IP", "更新PG"}
            Dim updateTypes As SqlDbType() = {SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar}

            Dim keyCmds As IDbCommand() = {argInsertCommand, argUpdateCommand, argDeleteCommand, argConcCheckCommand}
            Dim apdCmds As IDbCommand() = {argInsertCommand, argUpdateCommand}

            For Each row As CMCmdParam In argCmdSetting.ColumnParams
                ' sourceColumnが設定されていた場合は使用する
                Dim sc As String = If(Not String.IsNullOrEmpty(row.SourceColumn), row.SourceColumn, row.Name)

                ' キー項目の場合
                If row.IsKey Then
                    For Each cmd As IDbCommand In keyCmds
                        If cmd Is Nothing Then
                            Continue For
                        End If

                        ' パラメータを追加
                        Dim cmdParam As IDbDataParameter = CreateCmdParam(row.Name, row.GetDbType())
                        cmdParam.SourceColumn = sc
                        cmd.Parameters.Add(cmdParam)
                    Next
                Else
                    ' 従属項目の場合
                    For Each cmd As IDbCommand In apdCmds
                        ' パラメータを追加
                        Dim cmdParam As IDbDataParameter = CreateCmdParam(row.Name, row.GetDbType())
                        cmdParam.SourceColumn = sc
                        cmd.Parameters.Add(cmdParam)
                    Next
                End If
            Next

            ' 更新情報パラメータ
            For i As Integer = 0 To updateCols.Length - 1
                For Each cmd As IDbCommand In apdCmds
                    ' パラメータを追加
                    Dim cmdParam As IDbDataParameter = CreateCmdParam(updateCols(i), updateTypes(i))
                    cmdParam.SourceColumn = updateCols(i)
                    cmd.Parameters.Add(cmdParam)
                Next
            Next
        End Sub

        ''' <summary>
        ''' データの登録を実行する。
        ''' </summary>
        ''' <param name="argDataTable">更新データを格納したDataTable</param>
        ''' <returns>登録したレコード数</returns>
        Private Function DoUpdate(argDataTable As DataTable) As Integer
            Dim stats As DataRowState() = New DataRowState() {DataRowState.Deleted, DataRowState.Modified, DataRowState.Added}

            Dim updateTable As DataTable = Nothing
            Dim cnt As Integer = 0
            Try
                ' Delete, Update, Insert実行
                For Each sts As DataRowState In stats
                    updateTable = argDataTable.GetChanges(sts)
                    If updateTable IsNot Nothing Then
                        updateTable.TableName = "Table"
                        Dim ds As New DataSet()
                        ds.Tables.Add(updateTable)
                        cnt += Adapter.Update(ds)
                    End If
                Next
            Catch ex As SqlException
                Dim table As DataTable = updateTable.GetChanges()
                ' メッセージコードを設定
                Dim msgCode As String = If(ex.Number = PKEY_ERR, "WV001", "EV002")

                Dim message As CMMessage
                Dim rowNumber As Integer = CMUtil.GetRowNumber(table.Rows(0))
                ' 行番号ありの場合
                If rowNumber >= 0 Then
                    message = New CMMessage(msgCode, New CMRowField(rowNumber), ex.Message)
                Else
                    ' 行番号なしの場合
                    message = New CMMessage(msgCode, ex.Message)
                End If
                ' 例外発生
                Log.[Error](message.ToString(), ex)
                Throw New CMException(message)
            End Try

            Return cnt
        End Function
#End Region

#Region "監査証跡"
        ''' <summary>
        ''' 監査証跡を記録する。
        ''' </summary>
        ''' <param name="argCmdSetting">Command設定</param>
        ''' <param name="argDataTable">更新データを格納したDataTable</param>
        ''' <param name="argUpdateTime">データベースに記録する更新時刻</param>
        Protected Sub WriteAuditLog(argCmdSetting As CMCmdSetting, argDataTable As DataTable, argUpdateTime As DateTime)
            ' 出力OFFの場合は出力しない
            If Not My.MySettings.Default.WriteAuditLog Then
                Return
            End If

            ' マスタ以外は対象
            If Not argCmdSetting.Name.StartsWith("CMSM") Then
                Return
            End If

            ' コネクション自動オープン判定フラグ
            Dim isClosed As Boolean = Connection.State = ConnectionState.Closed

            Try
                ' コネクションを開く
                If isClosed Then
                    Connection.Open()
                End If
                ' INSERT文の設定
                Dim cmd As IDbCommand = CreateCommand(INSERT_AUDITLOG_SQL)
                ' パラメータの設定
                cmd.Parameters.Add(CreateCmdParam("テーブル名", argCmdSetting.Name))
                cmd.Parameters.Add(CreateCmdParam("更新区分", SqlDbType.[Char]))
                cmd.Parameters.Add(CreateCmdParam("キー", SqlDbType.NVarChar))
                cmd.Parameters.Add(CreateCmdParam("内容", SqlDbType.NVarChar))
                cmd.Parameters.Add(CreateCmdParam("更新日時", argUpdateTime))
                cmd.Parameters.Add(CreateCmdParam("更新者ID", CMInformationManager.UserInfo.Id))
                cmd.Parameters.Add(CreateCmdParam("更新者IP", CMInformationManager.ClientInfo.MachineName))
                cmd.Parameters.Add(CreateCmdParam("更新PG", CMInformationManager.ClientInfo.FormId))

                Dim key As New StringBuilder()
                Dim content As New StringBuilder()

                ' 登録ループ
                For Each row As DataRow In argDataTable.Rows
                    ' 更新区分の設定
                    Dim updType As String
                    Select Case row.RowState
                        Case DataRowState.Added
                            updType = "C"
                            Exit Select
                        Case DataRowState.Modified
                            updType = "U"
                            Exit Select
                        Case DataRowState.Deleted
                            updType = "D"
                            Exit Select
                        Case Else
                            Continue For
                    End Select
                    DirectCast(cmd.Parameters("更新区分"), IDbDataParameter).Value = updType

                    ' DataRowVersionの判定
                    Dim ver As DataRowVersion = If(row.RowState = DataRowState.Deleted, DataRowVersion.Original, DataRowVersion.[Default])

                    ' 更新列のみ出力フラグ設定
                    Dim onlyModCol As Boolean = row.RowState = DataRowState.Modified AndAlso row.HasVersion(DataRowVersion.Original)

                    key.Length = 0
                    content.Length = 0

                    ' テーブル項目列でループ
                    For Each csRow As CMCmdParam In argCmdSetting.ColumnParams
                        ' 列名を取得
                        Dim srcCol As String = If(csRow.SourceColumn IsNot Nothing, csRow.SourceColumn, csRow.Name)

                        ' フォーマットを設定
                        Dim format As String = If(csRow.DbType = CMDbType.日付, "{0}:{1:yyyy/MM/dd}", "{0}:{1}")

                        ' キー項目
                        If csRow.IsKey Then
                            If key.Length > 0 Then
                                key.Append(",")
                            End If
                            key.AppendFormat(format, csRow.Name, row(srcCol, ver))
                        Else
                            ' 従属項目
                            ' 更新列出力フラグがTrueのときは、元と値が異なるときのみ出力
                            If onlyModCol AndAlso row(srcCol).ToString() <> row(srcCol, DataRowVersion.Original).ToString() OrElse Not onlyModCol Then
                                If content.Length > 0 Then
                                    content.Append(",")
                                End If
                                content.AppendFormat(format, csRow.Name, row(srcCol, ver))
                            End If
                        End If
                    Next

                    ' キーの設定
                    DirectCast(cmd.Parameters("キー"), IDbDataParameter).Value = key.ToString()
                    ' 内容の設定
                    DirectCast(cmd.Parameters("内容"), IDbDataParameter).Value = content.ToString()

                    ' INSERT実行
                    cmd.ExecuteNonQuery()
                Next
            Catch ex As Exception
                Log.[Error]("監査証跡出力エラー", ex)
            Finally
                ' コネクションを閉じる
                If isClosed Then
                    Connection.Close()
                End If
            End Try
        End Sub
#End Region
    End Class
End Namespace
