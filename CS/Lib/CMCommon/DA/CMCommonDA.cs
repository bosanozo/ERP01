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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using Seasar.Quill.Attrs;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// 共通処理データアクセス層
    /// </summary>
    //************************************************************************
    [Implementation]
    public class CMCommonDA : CMBaseDA
    {
        private static CMCommonSelectDataSet.SelectStatementDataTable s_selectStatementTable;

        #region SQL文
        /// <summary>
        /// 操作ログINSERT文
        /// </summary>
        private const string INSERT_OPLOG_SQL =
            "INSERT INTO CMSTシステム利用状況 " +
            "VALUES(" +
            "CURRENT_TIMESTAMP," +
            "@画面ID," +
            "@画面名," +
            "@ユーザID," +
            "@端末ID," +
            "@APサーバ)";

        /// <summary>
        /// 区分値名称SELECT文
        /// </summary>
        private const string SELECT_KBN_SQL =
            "SELECT '' 表示名, 分類CD, 分類名, 基準値CD, 基準値名 " +
            "FROM CMSM汎用基準値 WHERE 分類CD IN ({0}) " +
            "ORDER BY 分類CD, 基準値CD";

        /// <summary>
        /// 参照範囲, 更新許可ELECT文
        /// </summary>
        private const string SELECT_RANGE_CANUPDATE_SQL =
            // 上位組織再帰検索
            "WITH SL (組織CD, 上位組織CD, 組織階層区分) AS " +
            "(SELECT 組織CD, 上位組織CD, 組織階層区分 " +
              "FROM CMSM組織 " +
             "WHERE 組織CD = @組織CD " +
            "UNION ALL " +
            "SELECT A.組織CD, A.上位組織CD, A.組織階層区分 " +
              "FROM CMSM組織 A " +
              "JOIN SL ON SL.上位組織CD = A.組織CD AND SL.組織階層区分 != '1')" +
            // ユーザに付与された権限を画面IDが長く一致するもの、組織階層が近いものを優先して取得
            "SELECT DISTINCT ロールID, " +
                    "FIRST_VALUE(許否フラグ) OVER (PARTITION BY ロールID " +
                    "ORDER BY LEN(画面ID) DESC, 組織階層区分 DESC) 許否フラグ " +
              "FROM {0} A " +
              "JOIN SL ON SL.組織CD = A.組織CD " +
             "WHERE ロールID IN ({1}) " +
               "AND @画面ID LIKE 画面ID + '%' " +
             "ORDER BY ロールID";

        /// <summary>
        /// 更新者SELECT文
        /// </summary>
        private const string SELECT_UPD_SQL =
            "SELECT B.更新者ID, A.\"ユーザ名\" 更新者名 " +
            "FROM \"CMSMユーザ\" A " +
            "JOIN CMSM組織 S ON S.組織CD = A.組織CD " +
            "JOIN ({0}) B ON A.\"ユーザID\" = B.更新者ID " +
            "{1}" +
            "ORDER BY B.更新者ID";
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMCommonDA()
        {
            // 共通検索設定ファイル読み込み
            if (s_selectStatementTable == null)
            {
                s_selectStatementTable = new CMCommonSelectDataSet.SelectStatementDataTable();
                s_selectStatementTable.ReadXml(AppDomain.CurrentDomain.BaseDirectory + "/CommonSelect.xml");
            }
        }
        #endregion

        #region データアクセスメソッド
        //************************************************************************
        /// <summary>
        /// 現在時刻を取得する。
        /// </summary>
        /// <returns>現在時刻</returns>
        //************************************************************************
        public DateTime GetSysdate()
        {
            // SelectCommandの設定
            Adapter.SelectCommand = CreateCommand("SELECT CURRENT_TIMESTAMP");
            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            int cnt = Adapter.Fill(ds);

            return cnt > 0 ? (DateTime)ds.Tables[0].Rows[0][0] : DateTime.Now;
        }

        //************************************************************************
        /// <summary>
        /// 指定された検索IDの検索を指定された条件で実行する。
        /// </summary>
        /// <param name="argSelectId">検索ID</param>
        /// <param name="argParams">パラメータ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        public DataTable Select(string argSelectId, params object[] argParams)
        {
            // SELECT文の設定
            DataRow[] rows = s_selectStatementTable.Select("SelectId = '" + argSelectId + "'");
            if (rows.Length == 0) throw new Exception("CommonSelect.xmlに\"" + argSelectId + "\"が登録されていません。");
            string statement = rows[0]["Statement"].ToString();

            IDbCommand selectCommand = CreateCommand(statement);
            Adapter.SelectCommand = selectCommand;
            // パラメータの設定
            for (int i = 0; i < argParams.Length; i++)
            {
                /*
                // DateTime型の場合
                if (argParams[i] is DateTime)
                    selectCommand.Parameters.Add((i + 1).ToString(), SqlDbType.Date,
                        argParams[i], ParameterDirection.Input);
                // DateTime型以外の場合
                else*/
                selectCommand.Parameters.Add(CreateCmdParam((i + 1).ToString(), argParams[i]));
            }
            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            Adapter.Fill(ds);
            // 検索結果の返却
            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// 最大検索件数を返す。
        /// </summary>
        /// <param name="argId">画面ID</param>
        /// <returns>最大検索件数</returns>
        //************************************************************************
        public int GetMaxRow(string argId = null)
        {
            if (argId == null) argId = CMInformationManager.ClientInfo.FormId;

            DataTable result = Select("CMSM汎用基準値", "V001", argId);
            if (result.Rows.Count > 0 && result.Rows[0]["基準値１"] != DBNull.Value)
                return Convert.ToInt32(result.Rows[0]["基準値１"]);
            else return 1000;
        }

        //************************************************************************
        /// <summary>
        /// 操作ログを記録する。
        /// </summary>
        /// <param name="argFormName">画面名</param>
        //************************************************************************
        public void WriteOperationLog(string argFormName)
        {
            // コネクション自動オープン判定フラグ
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                // コネクションを開く
                if (isClosed) Connection.Open();
                // INSERT文の設定
                IDbCommand cmd = CreateCommand(INSERT_OPLOG_SQL);
                // パラメータの設定
                cmd.Parameters.Add(CreateCmdParam("画面ID", CMInformationManager.ClientInfo.FormId));
                cmd.Parameters.Add(CreateCmdParam("画面名", argFormName));
                cmd.Parameters.Add(CreateCmdParam("ユーザID", CMInformationManager.UserInfo.Id));
                cmd.Parameters.Add(CreateCmdParam("端末ID", CMInformationManager.ClientInfo.MachineName));
                cmd.Parameters.Add(CreateCmdParam("ＡＰサーバ", Environment.MachineName));
                // INSERT実行
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (isClosed)
                {
                    // コネクションを破棄する
                    Connection.Close();
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// 汎用基準値から区分値名称を取得する。
        /// </summary>
        /// <param name="argKbnList">基準値分類CDのリスト</param>
        /// <returns>区分値名称のDataTable</returns>
        //************************************************************************
        public DataTable SelectKbn(params string[] argKbnList)
        {
            // INの中の条件を作成
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= argKbnList.Length; i++)
            {
                if (i > 1) sb.Append(",");
                sb.AppendFormat("@{0}", i);
            }

            // IDbCommand作成
            IDbCommand cmd = CreateCommand(string.Format(SELECT_KBN_SQL, sb));
            Adapter.SelectCommand = cmd;

            // パラメータを設定
            foreach (string val in argKbnList) cmd.Parameters.Add(CreateCmdParam("1", val));

            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            int cnt = Adapter.Fill(ds);
            // 表示名の設定
            ds.Tables[0].Columns["表示名"].Expression = "[基準値CD] + ' ' + [基準値名]";

            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// 参照範囲, 更新許可を検索する。
        /// </summary>
        /// <param name="argFormId">画面ＩＤ</param>
        /// <param name="argIsRange">True:参照範囲, False:更新許可</param>
        /// <returns>True:会社、更新可, False:拠点、更新不可</returns>
        //************************************************************************
        public bool GetRangeCanUpdate(string argFormId, bool argIsRange)
        {
            // ロールの条件作成
            string[] roles = CMInformationManager.UserInfo.Roles;
            StringBuilder builder = new StringBuilder();

            if (roles != null && roles.Length > 0)
            {
                builder.Append("'" + roles[0] + "'");
                for (int i = 1; i < roles.Length; i++)
                {
                    builder.AppendFormat(",'{0}'", roles[i]);
                }
            }
            else return false;

            // SELECT文の設定
            IDbCommand cmd = CreateCommand(
                string.Format(SELECT_RANGE_CANUPDATE_SQL, argIsRange ? "CMSM参照範囲" : "CMSM更新許可",
                builder.ToString()));
            // パラメータの設定
            cmd.Parameters.Add(CreateCmdParam("組織CD", CMInformationManager.UserInfo.SoshikiCd));
            cmd.Parameters.Add(CreateCmdParam("画面ID", argFormId));

            // コマンド設定
            Adapter.SelectCommand = cmd;

            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            Adapter.Fill(ds);

            return ds.Tables[0].Select("許否フラグ = True").Count() > 0;
        }

        //************************************************************************
        /// <summary>
        /// 汎用基準値マスタを検索する。
        /// </summary>
        /// <param name="argSelectId">検索ID</param>
        /// <param name="argParam">検索条件</param>
        /// <param name="argIsOver">最大検索件数オーバーフラグ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        public DataTable SelectSub(string argSelectId, List<CMSelectParam> argParam, out bool argIsOver)
        {
            // SELECT文の取得
            DataRow[] rows = s_selectStatementTable.Select("SelectId = '" + argSelectId + "'");
            if (rows.Length == 0) throw new Exception("CommonSelect.xmlに\"" + argSelectId + "\"が登録されていません。");
            
            // SELECT文作成
            StringBuilder selectSql = new StringBuilder("SELECT TOP 1001 * FROM (");
            selectSql.Append(rows[0]["Statement"].ToString());

            // WHERE句追加
            AddWhere(selectSql, argParam);

            // 絞込み条件追加
            selectSql.Append(") A ORDER BY ROWNUMBER");

            // SELECT文の設定
            IDbCommand cmd = CreateCommand(selectSql.ToString());
            Adapter.SelectCommand = cmd;

            // パラメータの設定
            int pCnt = 1;
            foreach (var param in argParam)
            {
                if (param.paramFrom != null)
                {
                    // パラメータ名の取得
                    string name;
                    if (string.IsNullOrEmpty(param.condtion))
                    {
                        name = pCnt.ToString();
                        pCnt++;
                    }
                    else name = param.condtion.Substring(param.condtion.IndexOf("@") + 1);

                    /*
                    if (param.paramFrom is DateTime)
                        cmd.Parameters.Add(name, SqlDbType.Date, param.paramFrom, ParameterDirection.Input);
                    else*/
                    cmd.Parameters.Add(CreateCmdParam(name, param.paramFrom));
                }
            }

            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            int cnt = Adapter.Fill(ds);

            // 最大検索件数オーバーの場合、最終行を削除
            if (cnt > 1000)
            {
                argIsOver = true;
                ds.Tables[0].Rows.RemoveAt(cnt - 1);
            }
            else argIsOver = false;

            // 検索結果の返却
            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// 更新者を指定された条件で検索する。
        /// </summary>
        /// <param name="argParam">検索条件</param>
        /// <param name="argTables">テーブル名の配列</param>
        /// <param name="argIsOver">最大検索件数オーバーフラグ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        public DataTable SelectUpdSub(List<CMSelectParam> argParam, string[] argTables,
             out bool argIsOver)
        {
            // SQL文の作成
            StringBuilder union = new StringBuilder();
            // 副問い合わせの作成
            if (argTables != null && argTables.Length > 0)
            {
                union.Append("SELECT DISTINCT 更新者ID FROM ").Append(argTables[0]);

                for (int i = 1; i < argTables.Length; i++)
                    union.Append(" UNION SELECT 更新者ID FROM ").Append(argTables[i]);
            }

            // 組織階層が全社でなければ、会社の条件を追加
            CMUserInfo uinfo = CMInformationManager.UserInfo;
            if (uinfo.SoshikiKaisoKbn != CMSoshikiKaiso.ALL)
                argParam.Add(new CMSelectParam("S.組織CD", "= @組織CD", uinfo.SoshikiCd));

            // WHERE句作成
            StringBuilder where = new StringBuilder();
            AddWhere(where, argParam);

            // SELECT文の設定
            IDbCommand cmd = CreateCommand(string.Format(SELECT_UPD_SQL, union, where));
            Adapter.SelectCommand = cmd;

            // パラメータの設定
            SetParameter(cmd, argParam);

            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            int cnt = Adapter.Fill(ds);

            // 最大検索件数オーバーの場合、最終行を削除
            if (cnt > 1000)
            {
                argIsOver = true;
                ds.Tables[0].Rows.RemoveAt(cnt - 1);
            }
            else argIsOver = false;

            // 検索結果の返却
            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// 共通検索を使用してデータの存在チェックを行う。
        /// 存在しなかった場合は、CMExceptionをthrowする。
        /// </summary>
        /// <param name="argSelectId">検索ID</param>
        /// <param name="argTableName">存在チェック対象のテーブル名</param>
        /// <param name="argRow">存在チェック対象データを含むDataRow</param>
        /// <param name="argColumnName">存在チェック対象データのDataRow中の列名</param>
        /// <param name="argParams">共通検索部品に渡すパラメータ(指定なしの場合はargColumnNameで
        ///指定した列のデータをパラメータに使用する)</param>
        //************************************************************************
        public void ExistCheck(string argSelectId,
            string argTableName, DataRow argRow, string argColumnName, params object[] argParams)
        {
            int cnt;
            // argParamsの指定がなかった場合
            if (argParams.Length == 0)
                cnt = Select(argSelectId, argRow[argColumnName]).Rows.Count;
            // argParamsの指定があった
            else cnt = Select(argSelectId, argParams).Rows.Count;
            // 存在チェック
            if (cnt == 0)
            {
                // メッセージの作成
                CMMessage message = new CMMessage("WV107",
                    new CMRowField(CMUtil.GetRowNumber(argRow), argColumnName),
                    argTableName);
                // 例外を発生
                throw new CMException(message);
            }
        }

        //************************************************************************
        /// <summary>
        /// XMLファイルの設定からデータの存在チェックを行う。
        /// </summary>
        /// <param name="argTable">存在チェック対象のDataTable</param>
        /// <param name="argFname">読み込むXMLファイル名(拡張子なし)</param>
        //************************************************************************
        public void ExistCheckFomXml(DataTable argTable, string argFname = null)
        {
            // デフォルト設定
            if (argFname == null) argFname = argTable.TableName;

            // データセットにファイルを読み込み
            DataSet ds = new DataSet();
            ds.ReadXml(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Model", argFname + ".xml"));

            // 入力値チェックループ
            foreach (DataRow row in argTable.Rows)
            {
                // 削除データはチェックしない
                if (row.RowState == DataRowState.Deleted) continue;

                // 存在チェック項目ループ
                foreach (DataRow irow in ds.Tables["項目"].Select("Len(存在チェックテーブル名) > 0"))
                {
                    // キー項目は新規のみチェック
                    if (irow["Key"].ToString() == "True" && row.RowState != DataRowState.Added) continue;

                    List<object> checkParams = new List<object>();
                    string paramText;

                    // 共通検索パラメータ取得
                    if (irow.Table.Columns.Contains("共通検索パラメータ") &&
                        (paramText = irow["共通検索パラメータ"].ToString()).Length > 0)
                    {
                        foreach (string p0 in paramText.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string p = p0.TrimStart();
                            if (p.Length < 2) continue;

                            // 'から始まる場合はそのまま設定
                            if (p[0] == '\'') checkParams.Add(p.Substring(1));
                            // "#"から始まる場合はUserInfoから設定
                            else if (p[0] == '#')
                            {
                                PropertyInfo pi = CMInformationManager.UserInfo.GetType().GetProperty(p.Substring(1));
                                checkParams.Add(pi.GetValue(CMInformationManager.UserInfo, null));
                            }
                            // Rowの値を取得
                            else checkParams.Add(row[p]);
                        }
                    }

                    // 存在チェック
                    ExistCheck(irow["共通検索ID"].ToString(), irow["存在チェックテーブル名"].ToString(),
                        row, irow["項目名"].ToString(), checkParams.ToArray());
                }
            }
        }
        #endregion
    }
}
