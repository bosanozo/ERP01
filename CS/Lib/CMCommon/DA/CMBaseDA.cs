/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using log4net;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// データアクセス層の基底クラス
    /// </summary>
    //************************************************************************
    public class CMBaseDA
    {
        #region ロガーフィールド
        private ILog m_logger;
        #endregion

        #region プロパティ
        /// <summary>
        /// ロガー
        /// </summary>
        protected ILog Log
        {
            get { return m_logger; }
        }

        /// <summary>コネクション</summary>
        public IDbConnection Connection { get; set; }

        /// <summary>データアダプタ</summary>
        protected IDbDataAdapter Adapter { get; set; }
        #endregion

        /// <summary>行ロックタイムアウトエラーNO</summary>
        private const int LOCK_TIMEOUT_ERR = 1222;

        /// <summary>PKEY制約違反エラーNO</summary>
        private const int PKEY_ERR = 2627;

        #region SQL文
        /// <summary>
        /// INSERT文
        /// </summary>
        private const string INSERT_SQL =
            "INSERT INTO {0} (" +
            "{1}" +
            "作成日時," +
            "作成者ID," +
            "作成者IP," +
            "作成PG," +
            "更新日時," +
            "更新者ID," +
            "更新者IP," +
            "更新PG" +
            ")VALUES(" +
            "{2}" +
            "@更新日時," +
            "@更新者ID," +
            "@更新者IP," +
            "@更新PG," +
            "@更新日時," +
            "@更新者ID," +
            "@更新者IP," +
            "@更新PG)";

        /// <summary>
        /// UPDATE文
        /// </summary>
        private const string UPDATE_SQL =
            "UPDATE {0} SET " +
            "{1}" +
            "更新日時 = @更新日時," +
            "更新者ID = @更新者ID," +
            "更新者IP = @更新者IP," +
            "更新PG = @更新PG " +
            "WHERE ";

        /// <summary>
        /// DELETE文
        /// </summary>
        private const string DELETE_SQL =
            "DELETE FROM {0} WHERE ";

        /// <summary>
        /// 排他チェック用SELECT文
        /// </summary>
        private const string CONC_CHEK_SQL =
            "SET LOCK_TIMEOUT 10000 " +
            "SELECT 更新日時, 更新者ID, 更新者IP, 更新PG, 排他用バージョン " +
              "FROM {0} WITH(ROWLOCK, UPDLOCK) WHERE ";

        /// <summary>
        /// 存在チェック用SELECT文
        /// </summary>
        private const string EXIST_CHEK_SQL =
            "SELECT TOP 1 COUNT(*) FROM {0} WHERE ROWNUM <= 1";

        /// <summary>
        /// 監査証跡INSERT文
        /// </summary>
        private const string INSERT_AUDITLOG_SQL =
            "INSERT INTO CMST監査証跡 (" +
            "テーブル名," +
            "更新区分," +
            "キー," +
            "内容," +
            "作成日時," +
            "作成者ID," +
            "作成者IP," +
            "作成PG," +
            "更新日時," +
            "更新者ID," +
            "更新者IP," +
            "更新PG" +
            ")VALUES(" +
            "@テーブル名," +
            "@更新区分," +
            "@キー," +
            "@内容," +
            "@更新日時," +
            "@更新者ID," +
            "@更新者IP," +
            "@更新PG," +
            "@更新日時," +
            "@更新者ID," +
            "@更新者IP," +
            "@更新PG)";
        #endregion

        #region SELECT文作成用SQL
        /// <summary>
        /// 登録時に必要な共通項目
        /// </summary>
        private const string TOROKU_COLS =
            "A.作成日時," +
            "A.作成者ID," +
            "US1.ユーザ名 作成者名," +
            "A.作成者IP," +
            "A.作成PG," +
            "A.更新日時," +
            "A.更新者ID," +
            "US2.ユーザ名 更新者名," +
            "A.更新者IP," +
            "A.更新PG";

        /// <summary>
        /// 作成者名、更新者名取得用JOIN
        /// </summary>
        private const string TOROKU_JOIN =
            "LEFT JOIN CMSMユーザ US1 ON US1.ユーザID = A.作成者ID " +
            "LEFT JOIN CMSMユーザ US2 ON US2.ユーザID = A.更新者ID ";

        /// <summary>
        /// 最大検索件数の条件
        /// </summary>
        private const string ROWNUMBER_CONDITION =
            "WHERE ROWNUMBER <= @最大検索件数 ";

        /// <summary>
        /// SELECT文
        /// </summary>
        private const string SELECT_SQL =
            "SELECT " +
            "'0' 削除," +
            "{0}" +
            TOROKU_COLS + "," +
            "A.排他用バージョン," +
            "A.ROWNUMBER " +
            "FROM (SELECT A.*, ROW_NUMBER() OVER (ORDER BY {1}) - 1 ROWNUMBER " +
            "FROM {2} A{3}) A " +
            TOROKU_JOIN +
            "{4}" +
            ROWNUMBER_CONDITION +
            "ORDER BY ROWNUMBER";

        /// <summary>
        /// CSV出力用SELECT文
        /// </summary>
        private const string SELECT_CSV_SQL =
            "SELECT " +
            "{0}" +
            TOROKU_COLS +
            " FROM {2} A " +
            TOROKU_JOIN +
            "{4}{3}" +
            "ORDER BY {1}";

        /// <summary>
        /// 登録画面SELECT文
        /// </summary>
        private const string SELECT_EDIT_SQL =
            "SELECT " +
            "{0}" +
            TOROKU_COLS + "," +
            "A.排他用バージョン," +
            "0 ROWNUMBER " +
            "FROM {1} A " +
            TOROKU_JOIN +
            "{3}{2}";

        /// <summary>
        /// 適用期間チェックSELECT文
        /// </summary>
        private const string SELECT_SPAN_SQL =
            "SELECT NULL FROM {0} WHERE 適用終了日 >= TO_CHAR(@1, 'YYYYMMDD') AND 適用開始日 <= TO_CHAR(@1, 'YYYYMMDD')";
        #endregion
        
        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMBaseDA()
        {
            // ロガーを取得
            m_logger = LogManager.GetLogger(this.GetType());

            // データアダプタはfactoryから作成する
            DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            Adapter = factory.CreateDataAdapter();
        }
        #endregion

        #region publicメソッド
        //************************************************************************
        /// <summary>
        /// 指定されたXMLファイルからSELECT文を作成し、検索を実行する。
        /// </summary>
        /// <param name="argParam">検索条件</param>
        /// <param name="argSelectType">検索種別</param>
        /// <param name="argMaxRow">最大検索件数</param>
        /// <param name="argIsOver">最大検索件数オーバーフラグ</param>
        /// <param name="argFname">読み込むXMLファイル名(拡張子なし)</param>
        /// <returns>検索結果</returns>
        /// <remarks>XMLファイルを複数指定した場合、その順に検索結果のDataTableがDataSetに生成される。
        /// 最大検索件数チェックは検索種別が一覧検索でXMLファイルが最初の検索のみ行う。
        /// DataTableの列名はXMLファイル中の項目名となるが、SourceColumnを指定した場合は別名にすることが可能。</remarks>
        //************************************************************************
        public DataSet SelectFromXml(List<CMSelectParam> argParam, CMSelectType argSelectType,
            int argMaxRow, out bool argIsOver, params string[] argFname)
        {
            // データセットの作成
            DataSet result = new DataSet();

            foreach (string fname in argFname)
            {
                // データセットにファイルを読み込み
                CMEntityDataSet ds = new CMEntityDataSet();
                ds.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", fname + ".xml"));

                // テーブル名を取得
                string tableName = ds.エンティティ[0].テーブル名;

                StringBuilder sb = new StringBuilder();
                StringBuilder orderSb = new StringBuilder();
                foreach (var row in ds.項目)
                {
                    // 項目名に.が無いものは駆動表から取得
                    string col = row.項目名.Contains(".") ?
                        row.項目名 : "A." + row.項目名;

                    // 検索列を作成
                    // SourceColumnの指定がある場合は別名をつける
                    if (string.IsNullOrEmpty(row.SourceColumn))
                        sb.AppendFormat("{0},", col);
                    else
                        sb.AppendFormat("{0} {1},", col, row.SourceColumn);

                    // ソート条件を作成
                    if (row.Key)
                    {
                        if (orderSb.Length > 0) orderSb.Append(" ,");
                        orderSb.Append(col);
                    }
                }

                // ソート条件
                string order = ds.エンティティ[0].OrderBy;
                if (string.IsNullOrEmpty(order)) order = orderSb.ToString();

                // tableNameがテーブル名が一致するものとtableNameなしを抽出
                var p = from a in argParam
                        where a.tableName == tableName
                           || string.IsNullOrEmpty(a.tableName)
                        select a;

                // WHERE句作成
                StringBuilder where = new StringBuilder();                
                AddWhere(where, p.ToList());

                // 検索種別がEditで2つ目の検索以降はListにする
                CMSelectType selectType = argSelectType == CMSelectType.Edit &&
                    result.Tables.Count > 0 ? CMSelectType.List : argSelectType;

                // 結合テーブル
                StringBuilder joinSb = new StringBuilder();
                foreach (var row in ds.結合テーブル)
                {
                    if (!row.内部結合) joinSb.Append("LEFT ");
                    joinSb.AppendFormat("JOIN {0} {1} ON {2}", row.テーブル名, row.シノニム, row.結合条件);
                    joinSb.AppendLine();
                }

                // SELECT文の設定
                IDbCommand cmd = CreateCommand(
                    CreateSelectSql(sb.ToString(), tableName, where.ToString(),
                    joinSb.ToString(), order, selectType));
                Adapter.SelectCommand = cmd;

                // パラメータの設定
                SetParameter(cmd, p.ToList());
                // 一覧検索の場合 かつ 最初の検索の場合、最大検索件数で制限
                if (selectType == CMSelectType.List && result.Tables.Count == 0)
                    cmd.Parameters.Add(CreateCmdParam("最大検索件数", argMaxRow));
                else cmd.CommandText = cmd.CommandText.Replace(ROWNUMBER_CONDITION, "");

                // データの取得
                Adapter.Fill(result);
                // テーブル名を設定
                result.Tables["Table"].TableName = tableName;
            }

            // 最初のデータテーブルで検索件数オーバーを判定
            int cnt = result.Tables[0].Rows.Count;

            // 一覧検索で最大検索件数オーバーの場合、最終行を削除
            if (argSelectType == CMSelectType.List && cnt >= argMaxRow)
            {
                argIsOver = true;
                result.Tables[0].Rows.RemoveAt(cnt - 1);
            }
            else argIsOver = false;

            // 検索結果の返却
            return result;
        }

        //************************************************************************
        /// <summary>
        /// 指定されたテーブルに更新データを登録する。
        /// </summary>
        /// <param name="argUpdateData">更新データ</param>
        /// <param name="argOperationTime">操作時刻</param>
        /// <param name="argCmdSettings">Command設定</param>
        /// <returns>登録したレコード数</returns>
        /// <remarks>argCmdSettingsが未指定の場合、エンティティ定義XMLファイルからCMCmdSettingsを生成する。
        /// データテーブル名がXMLファイル名になる。
        /// argUpdateDataに複数のDataTableが存在する場合、
        /// DataTableの逆順に削除データを登録後、
        /// DataTableの正順に新規、修正データを登録する。</remarks>
        //************************************************************************
        public int Update(DataSet argUpdateData, DateTime argOperationTime, CMCmdSettings argCmdSettings = null)
        {
            // デフォルト設定
            if (argCmdSettings == null)
            {
                argCmdSettings = new CMCmdSettings();
                foreach (DataTable table in argUpdateData.Tables)
                    argCmdSettings.AddFomXml(table.TableName);
            }

            int cnt = 0;
            int tableCnt = argCmdSettings.CmdSettings.Count;

            // 1テーブルの場合
            if (tableCnt == 1)
            {
                // 登録実行
                cnt = UpdateTable(argCmdSettings[0], argUpdateData.Tables[argCmdSettings[0].Name], argOperationTime);
            }
            // 複数テーブルの場合
            else
            {
                // Command設定の逆順に削除データを登録
                for (int i = tableCnt - 1; i > 0; i--)
                {
                    DataTable table = argUpdateData.Tables[argCmdSettings[i].Name].GetChanges(DataRowState.Deleted);
                    if (table != null && table.Rows.Count > 0)
                        cnt += UpdateTable(argCmdSettings[i], table, argOperationTime);
                }

                // 最初のテーブルのデータを登録
                cnt += UpdateTable(argCmdSettings[0], argUpdateData.Tables[argCmdSettings[0].Name], argOperationTime);

                // Command設定の順に新規、修正データを登録
                for (int i = 1; i < tableCnt; i++)
                {
                    DataTable table = argUpdateData.Tables[argCmdSettings[i].Name].GetChanges(DataRowState.Added | DataRowState.Modified);
                    if (table != null && table.Rows.Count > 0)
                        cnt += UpdateTable(argCmdSettings[i], table, argOperationTime);
                }
            }

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// 指定されたテーブルに更新データをアップロードする。
        /// </summary>
        /// <param name="argUpdateData">更新データ</param>
        /// <param name="argOperationTime">操作時刻</param>
        /// <param name="argCmdSettings">Command設定</param>
        /// <returns>登録したレコード数</returns>
        /// <remarks>argCmdSettingsが未指定の場合、エンティティ定義XMLファイルからCMCmdSettingsを生成する。
        /// データテーブル名がXMLファイル名になる。
        /// argUpdateDataに複数のDataTableが存在する場合、
        /// DataTableの正順に新規、修正データを登録する。
        /// 削除データは扱わない。</remarks>
        //************************************************************************
        public int Upload(DataSet argUpdateData, DateTime argOperationTime, CMCmdSettings argCmdSettings = null)
        {
            // デフォルト設定
            if (argCmdSettings == null)
            {
                argCmdSettings = new CMCmdSettings();
                foreach (DataTable table in argUpdateData.Tables)
                    argCmdSettings.AddFomXml(table.TableName);
            }

            int cnt = 0;

            // Command設定の順に新規、修正データを登録
            for (int i = 0; i < argCmdSettings.CmdSettings.Count; i++)
            {
                DataTable table = argUpdateData.Tables[argCmdSettings[i].Name];
                if (table != null && table.Rows.Count > 0)
                    cnt += UploadTable(argCmdSettings[i], table, argOperationTime);
            }

            return cnt;
        }
        #endregion

        #region protectedメソッド
        #region SQL作成メソッド
        //************************************************************************
        /// <summary>
        /// 検索種別に応じたSELECT文を作成する。駆動表のシノニムはAとする。
        /// </summary>
        /// <param name="argCols">検索列</param>
        /// <param name="argTableName">検索テーブル名</param>
        /// <param name="argWhere">WHERE句</param>
        /// <param name="argJoin">JOIN句</param>
        /// <param name="argOrder">並び順</param>
        /// <param name="argSelectType">検索種別</param>
        /// <returns>SELECT文</returns>
        //************************************************************************
        protected string CreateSelectSql(string argCols, string argTableName,
            string argWhere, string argJoin, string argOrder, CMSelectType argSelectType)
        {
            string tableName = argTableName;

            // 登録画面の場合
            if (argSelectType == CMSelectType.Edit)
                return string.Format(SELECT_EDIT_SQL, argCols, tableName, argWhere, argJoin);
            // 一覧検索, CSV出力の場合
            else
            {
                return string.Format(argSelectType == CMSelectType.List ? SELECT_SQL : SELECT_CSV_SQL,
                    argCols, argOrder, tableName, argWhere, argJoin);
            }
        }

        //************************************************************************
        /// <summary>
        /// StringBuilderに検索条件を追加する。
        /// </summary>
        /// <param name="argWhereSb">検索条件を追加するStringBuilder</param>
        /// <param name="argParam">検索条件</param>
        /// <remarks>argWhereSbの長さが0の場合はWHEREから文字列を追加する。
        /// 0でない場合はANDから文字列を追加する。</remarks>
        //************************************************************************
        protected void AddWhere(StringBuilder argWhereSb, List<CMSelectParam> argParam)
        {
            // 空白で終わってなければ、空白追加
            if (argWhereSb.Length > 0 && argWhereSb[argWhereSb.Length - 1] != ' ')
                argWhereSb.Append(" ");

            // 追加の条件
            foreach (var param in argParam)
            {
                if (string.IsNullOrEmpty(param.condtion)) continue;
                argWhereSb.Append(argWhereSb.Length > 0 ? "AND " : " WHERE ");
                if (!string.IsNullOrEmpty(param.name))
                {
                    // テーブルの指定がない場合は、Aをつける
                    if (!param.name.Contains('.')) argWhereSb.Append("A.");
                    argWhereSb.AppendFormat("{0} ", param.name);
                }
                argWhereSb.Append(param.condtion).Append(" ");
            }
        }

        //************************************************************************
        /// <summary>
        /// テーブル項目列DataTableからINSERT文, UPDATE文を作成する。
        /// </summary>
        /// <param name="argCmdSetting">Command設定</param>
        /// <param name="argInsertSql">INSERT文</param>
        /// <param name="argUpdateSql">UPDATE文</param>
        //************************************************************************
        protected void CreateInsertUpdateSql(CMCmdSetting argCmdSetting,
            out string argInsertSql, out string argUpdateSql)
        {
            StringBuilder ins1 = new StringBuilder();
            StringBuilder ins2 = new StringBuilder();
            StringBuilder upd = new StringBuilder();

            // テーブル項目列でループ
            foreach (var row in argCmdSetting.ColumnParams)
            {
                string valueFmt;

                // キー項目にNULLは設定させない
                if (row.IsKey)
                {
                    if (row.DbType == CMDbType.金額 || row.DbType == CMDbType.整数 || row.DbType == CMDbType.小数)
                        valueFmt = "ISNULL(@{0}, 0),";
                    // 日付型は対応なし
                    else if (row.DbType == CMDbType.日時 || row.DbType == CMDbType.日付)
                        valueFmt = "@{0},";
                    else valueFmt = "ISNULL(@{0}, ' '),";
                }
                else valueFmt = "@{0},";

                string colName = row.Name;

                // INSERT文作成
                ins1.Append(colName).Append(",");
                ins2.AppendFormat(valueFmt, row.Name);

                // 従属項目の場合
                if (!row.IsKey)
                    // UPDATE文作成
                    upd.Append(colName).Append(" = ").AppendFormat(valueFmt, row.Name);
            }

            string tname = argCmdSetting.Name;
            argInsertSql = string.Format(INSERT_SQL, tname, ins1, ins2);
            argUpdateSql = string.Format(UPDATE_SQL, tname, upd);
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// 接続に関連付けられたCommandオブジェクトを作成する。
        /// </summary>
        /// <param name="argCommandText">Commandに設定するSQL文</param>
        /// <returns>接続に関連付けられたCommandオブジェクト</returns>
        //************************************************************************
        protected IDbCommand CreateCommand(string argCommandText)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = argCommandText;
            return cmd;
        }

        //************************************************************************
        /// <summary>
        /// 接続に関連付けられたCommandのパラメータオブジェクトを作成する。
        /// </summary>
        /// <param name="argParameterName">パラメータ名</param>
        /// <param name="argValue">値</param>
        /// <returns>接続に関連付けられたCommandのパラメータオブジェクト</returns>
        //************************************************************************
        protected IDbDataParameter CreateCmdParam(string argParameterName, object argValue)
        {
            return new SqlParameter(argParameterName, argValue);
        }

        //************************************************************************
        /// <summary>
        /// 接続に関連付けられたCommandのパラメータオブジェクトを作成する。
        /// </summary>
        /// <param name="argParameterName">パラメータ名</param>
        /// <param name="argDbType">SqlDbType</param>
        /// <returns>接続に関連付けられたCommandのパラメータオブジェクト</returns>
        //************************************************************************
        protected IDbDataParameter CreateCmdParam(string argParameterName, SqlDbType argDbType)
        {
            return new SqlParameter(argParameterName, argDbType);
        }

        //************************************************************************
        /// <summary>
        /// Commandに検索パラメータを設定する。
        /// </summary>
        /// <param name="argCmd">IDbCommand</param>
        /// <param name="argParam">検索条件</param>
        //************************************************************************
        protected void SetParameter(IDbCommand argCmd, List<CMSelectParam> argParam)
        {
            Regex regex = new Regex("@\\w+");

            foreach (var param in argParam)
            {
                // プレースフォルダ名を取得
                MatchCollection mc = regex.Matches(param.condtion);
                if (mc.Count == 0) continue;
 
                if (param.paramFrom != null && param.paramTo != null)
                {
                    argCmd.Parameters.Add(CreateCmdParam(mc[0].Value, param.paramFrom));
                    argCmd.Parameters.Add(CreateCmdParam(mc[1].Value, param.paramTo));
                }
                else
                {
                    if (param.paramFrom != null) argCmd.Parameters.Add(CreateCmdParam(mc[0].Value, param.paramFrom));
                    if (param.paramTo != null) argCmd.Parameters.Add(CreateCmdParam(mc[0].Value, param.paramTo));
                }
            }
        }

        #region データ登録メソッド
        //************************************************************************
        /// <summary>
        /// データベース更新パラメータを設定する
        /// </summary>
        /// <param name="argDataRow">パラメータ設定対象のDataRow</param>
        /// <param name="argUpdateTime">データベースに記録する更新時刻</param>
        //************************************************************************
        protected void SetUpdateParameter(DataRow argDataRow, DateTime argUpdateTime)
        {
            argDataRow["更新日時"] = argUpdateTime;
            argDataRow["更新者ID"] = CMInformationManager.UserInfo.Id;
            argDataRow["更新者IP"] = CMInformationManager.ClientInfo.MachineName;
            argDataRow["更新PG"] = CMInformationManager.ClientInfo.FormId;
        }

        //************************************************************************
        /// <summary>
        /// 指定されたテーブルに更新データを登録する。
        /// </summary>
        /// <param name="argCmdSetting">Command設定</param>
        /// <param name="argDataTable">更新データを格納したDataTable</param>
        /// <param name="argSysdate">データベースに記録する更新時刻</param>
        /// <param name="argInsertSql">INSERT文</param>
        /// <param name="argUpdateSql">UPDATE文</param>
        /// <returns>登録したレコード数</returns>
        /// <remarks>INSERT文, UPDATE文が未指定の場合、Command設定から生成する。
        /// 更新、削除データの場合、項目：排他用バージョンにより排他チェックを行う。
        /// 削除データ、更新データ、新規データの順に登録を行う。
        /// 登録が成功した場合、監査証跡の出力を行う。</remarks>
        //************************************************************************
        protected int UpdateTable(CMCmdSetting argCmdSetting,
            DataTable argDataTable, DateTime argSysdate,
            string argInsertSql = null, string argUpdateSql = null)
        {
            // テーブル名を取得
            //string tname = Escape(argCmdSetting.Name);
            string tname = argCmdSetting.Name;

            // 主キーの検索条件を取得
            string keyCond = argCmdSetting.GetKeyCondition();

            // INSERT文, UPDATE文の自動設定
            if (argInsertSql == null || argUpdateSql == null)
            {
                // INSERT文, UPDATE文を作成
                string insertSql;
                string updateSql;
                CreateInsertUpdateSql(argCmdSetting, out insertSql, out updateSql);
                // nullの場合は作成したものを設定
                if (argInsertSql == null) argInsertSql = insertSql;
                if (argUpdateSql == null) argUpdateSql = updateSql + keyCond;
            }

            // INSERT, UPDATE, DELETEコマンドの作成
            IDbCommand insertCommand = CreateCommand(argInsertSql);
            IDbCommand updateCommand = CreateCommand(argUpdateSql);
            IDbCommand deleteCommand =
                CreateCommand(string.Format(DELETE_SQL, tname) + keyCond);
            // 排他チェック用SELECTコマンドの作成
            IDbCommand concCheckCommand =
                CreateCommand(string.Format(CONC_CHEK_SQL, tname) + keyCond);

            // INSERT, UPDATE, DELETE文の設定
            Adapter.InsertCommand = insertCommand;
            Adapter.UpdateCommand = updateCommand;
            Adapter.DeleteCommand = deleteCommand;

            // INSERT, UPDATE, DELETE, 排他チェック用SELECTコマンドのパラメータを設定
            AddCommandParameter(insertCommand, updateCommand, deleteCommand,
                concCheckCommand, argCmdSetting);

            // コネクション自動オープン判定フラグ
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                if (isClosed) Connection.Open();

                // データの更新ループ
                foreach (DataRow row in argDataTable.Rows)
                {
                    // データベース更新パラメータの設定
                    if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                        SetUpdateParameter(row, argSysdate);

                    // 更新、削除データの場合、排他チェックを実施
                    if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Deleted)
                        DoConcCheck(concCheckCommand, row, argCmdSetting);
                }
            }
            finally
            {
                if (isClosed) Connection.Close();
            }

            // データの登録を実行
            int cnt = DoUpdate(argDataTable);

            // 監査証跡出力
            WriteAuditLog(argCmdSetting, argDataTable, argSysdate);

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// 指定されたテーブルに、指定されたデータをアップロードする。
        /// </summary>
        /// <param name="argCmdSetting">Command設定</param>
        /// <param name="argDataTable">更新データを格納したDataTable</param>
        /// <param name="argUpdateTime">データベースに記録する更新時刻</param>
        /// <param name="argInsertSql">INSERT文</param>
        /// <param name="argUpdateSql">UPDATE文</param>
        /// <returns>登録したレコード数</returns>
        /// <remarks>INSERT文, UPDATE文が未指定の場合、Command設定から生成する。
        /// 同一キーのデータが存在する場合UPDATE、存在しない場合INSERTを実行する。
        /// 監査証跡の出力は行わない。</remarks>
        //************************************************************************
        protected int UploadTable(CMCmdSetting argCmdSetting,
            DataTable argDataTable,  DateTime argUpdateTime,
            string argInsertSql = null, string argUpdateSql = null)
        {
            // 更新用の列をDataTableに追加
            argDataTable.Columns.Add("更新日時", typeof(DateTime));
            argDataTable.Columns.Add("更新者ID");
            argDataTable.Columns.Add("更新者IP");
            argDataTable.Columns.Add("更新PG");

            // テーブル名を取得
            //string tname = Escape(argCmdSetting.Name);
            string tname = argCmdSetting.Name;

            // 主キーの検索条件を取得
            string keyCond = argCmdSetting.GetKeyCondition();

            // INSERT文, UPDATE文の自動設定
            if (argInsertSql == null || argUpdateSql == null)
            {
                // INSERT文, UPDATE文を作成
                string insertSql;
                string updateSql;
                CreateInsertUpdateSql(argCmdSetting, out insertSql, out updateSql);
                // nullの場合は作成したものを設定
                if (argInsertSql == null) argInsertSql = insertSql;
                if (argUpdateSql == null) argUpdateSql = updateSql + keyCond;
            }
           
            // INSERT, UPDATEコマンドの作成
            IDbCommand insertCommand = CreateCommand(argInsertSql);
            IDbCommand updateCommand = CreateCommand(argUpdateSql);

            // 存在チェック用SELECT文の作成
            IDbCommand existCheckCommand = 
                CreateCommand(string.Format(CONC_CHEK_SQL, tname) + keyCond);

            // INSERT, UPDATE文の設定
            Adapter.InsertCommand = insertCommand;
            Adapter.UpdateCommand = updateCommand;

            // INSERT, UPDATE, 存在チェック用SELECTコマンドのパラメータを設定
            AddCommandParameter(insertCommand, updateCommand, null, existCheckCommand, argCmdSetting);

            // コネクション自動オープン判定フラグ
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                if (isClosed) Connection.Open();

                // データの更新ループ
                foreach (DataRow row in argDataTable.Rows)
                {
                    // データベース更新パラメータの設定
                    SetUpdateParameter(row, argUpdateTime);
                    // 存在するかチェック
                    DoUploadCheck(existCheckCommand, row, argCmdSetting);
                }
            }
            finally
            {
                if (isClosed) Connection.Close();
            }

            // データの登録を実行
            int cnt = 0;
            try
            {
                DataTable table = argDataTable.Copy();
                table.TableName = "Table";
                DataSet ds = new DataSet();
                ds.Tables.Add(table);
                cnt = Adapter.Update(ds);
            }
            catch (SqlException ex)
            {
                // 未登録行を取得
                DataRow[] rows = argDataTable.Select(null, null, DataViewRowState.ModifiedCurrent | DataViewRowState.Added);
                // 行番号を取得
                int rowNumber = argDataTable.Rows.IndexOf(rows[0]) + 1;
                // メッセージコードを設定
                string msgCode = ex.Number == PKEY_ERR ? "WV001" : "EV002";
                // メッセージ設定
                string message = ex.Number == PKEY_ERR ? CMMessageManager.GetMessage(msgCode) : ex.Message;
                // メッセージ作成
                CMMessage msgData = new CMMessage(msgCode,
                    new CMRowField(argDataTable.TableName, rowNumber), message);
                // 例外発生
                throw new CMException(msgData, ex);
            }
            return cnt;
        }
        #endregion
        #endregion

        #region privateメソッド
        //************************************************************************
        /// <summary>
        /// 排他チェックを実行する。
        /// </summary>
        /// <param name="argConcCheckCommand">排他チェック用コマンド</param>
        /// <param name="argRow">排他チェック対象のDataRow</param>
        /// <param name="argCmdSetting">Command設定</param>
        //************************************************************************
        private void DoConcCheck(IDbCommand argConcCheckCommand,
            DataRow argRow, CMCmdSetting argCmdSetting)
        {
            // 排他チェック用コマンドにパラメータ値を設定
            foreach (var row in argCmdSetting.ColumnParams)
            {
                if (row.IsKey)
                {
                    string name = !string.IsNullOrEmpty(row.SourceColumn) ? row.SourceColumn : row.Name;
                    ((IDbDataParameter)argConcCheckCommand.Parameters[row.Name]).Value = argRow[name, DataRowVersion.Original];
                }
            }

            // 検索実行
            try
            {
                using (IDataReader reader = argConcCheckCommand.ExecuteReader())
                {
                    // レコードありの場合
                    if (reader.Read())
                    {
                        long rowversion = BitConverter.ToInt64((byte[])reader.GetValue(4), 0);

                        // データ更新チェック
                        if (BitConverter.ToInt64((byte[])argRow["排他用バージョン", DataRowVersion.Original], 0) != rowversion)
                        {
                            DateTime updateTime = reader.GetDateTime(0);
                            string userId = reader.GetString(1);
                            string hostname = reader.GetString(2);
                            string progId = reader.GetString(3);

                            // データが更新されていた場合
                            // メッセージコードの設定
                            string msgCode = argRow.RowState == DataRowState.Modified ? "WV002" : "WV004";
                            CMMessage message;
                            int rowNumber = CMUtil.GetRowNumber(argRow);
                            // 行番号ありの場合
                            if (rowNumber >= 0) message = new CMMessage(msgCode,
                                    new CMRowField(argRow.Table.TableName, rowNumber),
                                    userId, updateTime, progId, hostname);
                            // 行番号なしの場合
                            else message = new CMMessage(msgCode, userId, updateTime, progId, hostname);
                            // 例外発生
                            throw new CMException(message);
                        }
                    }
                    // レコードなしの場合
                    else
                    {
                        // メッセージコードの設定
                        string msgCode = argRow.RowState == DataRowState.Modified ? "WV003" : "WV005";
                        CMMessage message;
                        int rowNumber = CMUtil.GetRowNumber(argRow);
                        // 行番号ありの場合
                        if (rowNumber >= 0) message = new CMMessage(msgCode,
                            new CMRowField(argRow.Table.TableName, rowNumber));
                        // 行番号なしの場合
                        else message = new CMMessage(msgCode);
                        // 例外発生
                        throw new CMException(message);
                    }
                }
            }
            catch (SqlException ex)
            {
                // リソースビジー以外はそのままthrow
                if (ex.Number != LOCK_TIMEOUT_ERR) throw ex;

                // メッセージコードの設定
                string msgCode = argRow.RowState == DataRowState.Modified ? "WV006" : "WV007";
                CMMessage message;
                int rowNumber = CMUtil.GetRowNumber(argRow);
                // 行番号ありの場合
                if (rowNumber >= 0) message = new CMMessage(msgCode,
                    new CMRowField(argRow.Table.TableName, rowNumber));
                // 行番号なしの場合
                else message = new CMMessage(msgCode);
                // 例外発生
                throw new CMException(message);
            }
        }

        //************************************************************************
        /// <summary>
        /// 存在チェックを実行する。
        /// </summary>
        /// <param name="argExistCheckCommand">存在チェック用コマンド</param>
        /// <param name="argRow">存在チェック対象のDataRow</param>
        /// <param name="argCmdSetting">Command設定</param>
        //************************************************************************
        private void DoUploadCheck(IDbCommand argExistCheckCommand,
            DataRow argRow, CMCmdSetting argCmdSetting)
        {
            // 存在チェック用コマンドにパラメータ値を設定
            foreach (var row in argCmdSetting.ColumnParams)
            {
                if (row.IsKey)
                {
                    string name = !string.IsNullOrEmpty(row.SourceColumn) ? row.SourceColumn : row.Name;
                    ((IDbDataParameter)argExistCheckCommand.Parameters[row.Name]).Value = argRow[name];
                }
            }

            // 検索実行
            using (IDataReader reader = argExistCheckCommand.ExecuteReader())
            {
                // レコードありの場合
                if (reader.Read())
                {
                    // 新規を通常に変更
                    argRow.AcceptChanges();
                    // 更新に変更
                    argRow.SetModified();
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// パラメータ変数を設定する。
        /// </summary>
        /// <param name="argInsertCommand">INSERTコマンド</param>
        /// <param name="argUpdateCommand">UPDATEコマンド</param>
        /// <param name="argDeleteCommand">DELETEコマンド</param>
        /// <param name="argConcCheckCommand">排他チェック用コマンド</param>
        /// <param name="argCmdSetting">項目設定</param>
        //************************************************************************
        private void AddCommandParameter(IDbCommand argInsertCommand, IDbCommand argUpdateCommand,
            IDbCommand argDeleteCommand, IDbCommand argConcCheckCommand, CMCmdSetting argCmdSetting)
        {
            string[] updateCols = { "更新日時", "更新者ID", "更新者IP", "更新PG" };
            SqlDbType[] updateTypes = { SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

            IDbCommand[] keyCmds = { argInsertCommand, argUpdateCommand, argDeleteCommand, argConcCheckCommand };
            IDbCommand[] apdCmds = { argInsertCommand, argUpdateCommand };

            foreach (var row in argCmdSetting.ColumnParams)
            {
                // sourceColumnが設定されていた場合は使用する
                string sc = !string.IsNullOrEmpty(row.SourceColumn) ? row.SourceColumn : row.Name;

                // キー項目の場合
                if (row.IsKey)
                {
                    foreach (var cmd in keyCmds)
                    {
                        if (cmd == null) continue;

                        // パラメータを追加
                        IDbDataParameter cmdParam = CreateCmdParam(row.Name, row.GetDbType());
                        cmdParam.SourceColumn = sc;
                        cmd.Parameters.Add(cmdParam);
                    }
                }
                // 従属項目の場合
                else
                {
                    foreach (var cmd in apdCmds)
                    {
                        // パラメータを追加
                        IDbDataParameter cmdParam = CreateCmdParam(row.Name, row.GetDbType());
                        cmdParam.SourceColumn = sc;
                        cmd.Parameters.Add(cmdParam);
                    }
                }
            }

            // 更新情報パラメータ
            for (int i = 0; i < updateCols.Length; i++)
            {
                foreach (var cmd in apdCmds)
                {
                    // パラメータを追加
                    IDbDataParameter cmdParam = CreateCmdParam(updateCols[i], updateTypes[i]);
                    cmdParam.SourceColumn = updateCols[i];
                    cmd.Parameters.Add(cmdParam);
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// データの登録を実行する。
        /// </summary>
        /// <param name="argDataTable">更新データを格納したDataTable</param>
        /// <returns>登録したレコード数</returns>
        //************************************************************************
        private int DoUpdate(DataTable argDataTable)
        {
            DataRowState[] stats = new DataRowState[]
            { 
                DataRowState.Deleted, DataRowState.Modified, DataRowState.Added
            };

            DataTable updateTable = null;
            int cnt = 0;
            try
            {
                // Delete, Update, Insert実行
                foreach (DataRowState sts in stats)
                {
                    updateTable = argDataTable.GetChanges(sts);
                    if (updateTable != null)
                    {
                        updateTable.TableName = "Table";
                        DataSet ds = new DataSet();
                        ds.Tables.Add(updateTable);
                        cnt += Adapter.Update(ds);
                    }
                }
            }
            catch (SqlException ex)
            {
                DataTable table = updateTable.GetChanges();
                // メッセージコードを設定
                string msgCode = ex.Number == PKEY_ERR ? "WV001" : "EV002";

                CMMessage message;
                int rowNumber = CMUtil.GetRowNumber(table.Rows[0]);
                // 行番号ありの場合
                if (rowNumber >= 0) message = new CMMessage(msgCode,
                    new CMRowField(argDataTable.TableName, rowNumber), ex.Message);
                // 行番号なしの場合
                else message = new CMMessage(msgCode, ex.Message);
                // 例外発生
                Log.Error(message.ToString(), ex);
                throw new CMException(message);
            }

            return cnt;
        }
        #endregion

        #region 監査証跡
        //************************************************************************
        /// <summary>
        /// 監査証跡を記録する。
        /// </summary>
        /// <param name="argCmdSetting">Command設定</param>
        /// <param name="argDataTable">更新データを格納したDataTable</param>
        /// <param name="argUpdateTime">データベースに記録する更新時刻</param>
        //************************************************************************
        protected void WriteAuditLog(CMCmdSetting argCmdSetting, 
            DataTable argDataTable, DateTime argUpdateTime)
        {
            // 出力OFFの場合は出力しない
            if (!Properties.Settings.Default.WriteAuditLog) return;

            // マスタ以外は対象
            if (!argCmdSetting.Name.StartsWith("CMSM")) return;

            // コネクション自動オープン判定フラグ
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                // コネクションを開く
                if (isClosed) Connection.Open();
                // INSERT文の設定
                IDbCommand cmd = CreateCommand(INSERT_AUDITLOG_SQL);
                // パラメータの設定
                cmd.Parameters.Add(CreateCmdParam("テーブル名", argCmdSetting.Name));
                cmd.Parameters.Add(CreateCmdParam("更新区分", SqlDbType.Char));
                cmd.Parameters.Add(CreateCmdParam("キー", SqlDbType.NVarChar));
                cmd.Parameters.Add(CreateCmdParam("内容", SqlDbType.NVarChar));
                cmd.Parameters.Add(CreateCmdParam("更新日時", argUpdateTime));
                cmd.Parameters.Add(CreateCmdParam("更新者ID", CMInformationManager.UserInfo.Id));
                cmd.Parameters.Add(CreateCmdParam("更新者IP", CMInformationManager.ClientInfo.MachineName));
                cmd.Parameters.Add(CreateCmdParam("更新PG", CMInformationManager.ClientInfo.FormId));

                StringBuilder key = new StringBuilder();
                StringBuilder content = new StringBuilder();

                // 登録ループ
                foreach (DataRow row in argDataTable.Rows)
                {
                    // 更新区分の設定
                    string updType;
                    switch (row.RowState)
                    {
                        case DataRowState.Added:
                            updType = "C";
                            break;
                        case DataRowState.Modified:
                            updType = "U";
                            break;
                        case DataRowState.Deleted:
                            updType = "D";
                            break;
                        default:
                            continue;
                    }
                    ((IDbDataParameter)cmd.Parameters["更新区分"]).Value = updType;

                    // DataRowVersionの判定
                    DataRowVersion ver = row.RowState == DataRowState.Deleted ?
                        DataRowVersion.Original : DataRowVersion.Default;

                    // 更新列のみ出力フラグ設定
                    bool onlyModCol =
                        row.RowState == DataRowState.Modified && row.HasVersion(DataRowVersion.Original);

                    key.Length = 0;
                    content.Length = 0;

                    // テーブル項目列でループ
                    foreach (var csRow in argCmdSetting.ColumnParams)
                    {
                        // 列名を取得
                        string srcCol = csRow.SourceColumn != null ? csRow.SourceColumn : csRow.Name;

                        // フォーマットを設定
                        string format = csRow.DbType == CMDbType.日付 ?
                            "{0}:{1:yyyy/MM/dd}" : "{0}:{1}";

                        // キー項目
                        if (csRow.IsKey)
                        {
                            if (key.Length > 0) key.Append(",");
                            key.AppendFormat(format, csRow.Name, row[srcCol, ver]);
                        }
                        // 従属項目
                        else
                        {
                            // 更新列出力フラグがTrueのときは、元と値が異なるときのみ出力
                            if (onlyModCol && row[srcCol].ToString() != row[srcCol, DataRowVersion.Original].ToString()
                                || !onlyModCol)
                            {
                                if (content.Length > 0) content.Append(",");
                                content.AppendFormat(format, csRow.Name, row[srcCol, ver]);
                            }
                        }
                    }

                    // キーの設定
                    ((IDbDataParameter)cmd.Parameters["キー"]).Value = key.ToString();
                    // 内容の設定
                    ((IDbDataParameter)cmd.Parameters["内容"]).Value = content.ToString();

                    // INSERT実行
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Error("監査証跡出力エラー", ex);
            }
            finally
            {
                // コネクションを閉じる
                if (isClosed) Connection.Close();
            }
        }
        #endregion
    }
}
