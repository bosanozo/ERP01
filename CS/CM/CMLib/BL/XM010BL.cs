/*******************************************************************************
 * 【ERPシステム】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.DA;

using Seasar.Quill.Attrs;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// ファサード層
    /// </summary>
    //************************************************************************
    [Implementation]
    public class XM010BL : CMBaseBL, ICMBaseBL
    {
        #region インジェクション用フィールド
        protected XM010DA m_dataAccess;
        #endregion

        // 最新バージョン検索条件
        private const string VER_COND =
            "= (SELECT MAX(VER) FROM {0} WHERE VER <= @VER " +
            "AND 項目一覧ID = A.項目一覧ID{1})";

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public XM010BL()
        {
        }
        #endregion

        #region ファサードメソッド
        //************************************************************************
        /// <summary>
        /// エンティティ定義を検索する。
        /// </summary>
        /// <param name="argParam">検索条件</param>
        /// <param name="argSelectType">検索種別</param>
        /// <param name="argOperationTime">操作時刻</param>
        /// <param name="argMessage">結果メッセージ</param>
        /// <returns>検索結果</returns>
        //************************************************************************      
        public DataSet Select(List<CMSelectParam> argParam, CMSelectType argSelectType,
            out DateTime argOperationTime, out CMMessage argMessage)
        {
            // 操作時刻を設定
            argOperationTime = DateTime.Now;

            // 最大検索件数取得
            CommonDA.Connection = Connection;
            int maxRow = CommonDA.GetMaxRow();

            string[] fnames = { "XMEM項目一覧" };

            // 登録画面の場合
            if (argSelectType == CMSelectType.Edit)
            {
                // 検索テーブル
                fnames = new string[] { "XMEM項目一覧", "XMEM結合テーブル", "XMEM項目" };

                // VERの条件を変更
                argParam[1].tableName = fnames[0];

                // 子テーブルのVERの条件を追加
                for (int i = 1; i <= 2; i++)
                {
                    CMSelectParam param2 = new CMSelectParam("VER",
                        string.Format(VER_COND,
                        i == 1 ? "XM結合テーブル" : "XM項目",
                        i == 1 ? " AND テーブル名 = A.テーブル名" : " AND 項目名 = A.項目名"),
                        argParam[1].paramFrom);
                    param2.tableName = fnames[i];
                    argParam.Add(param2);
                }
            }　
            else
            {
                // 最新VERのみ表示
                var p1 = argParam.Where(p => p.name == "最新版のみ");
                if (p1.Count() > 0)
                {
                    var param = p1.First();
                    if (param.paramFrom.ToString() == "true")
                    {
                        param.name = "VER";
                        param.condtion = "= (SELECT MAX(VER) FROM XM項目一覧 WHERE 項目一覧ID = A.項目一覧ID)";
                        param.paramFrom = null;
                    }
                }
            }

            // 検索実行
            bool isOver;
            m_dataAccess.Connection = Connection;
            DataSet result = m_dataAccess.SelectFromXml(argParam, argSelectType,
                maxRow, out isOver, fnames);

            argMessage = null;
            // 検索結果なし
            if (result.Tables[0].Rows.Count == 0) argMessage = new CMMessage("IV001");
            // 最大検索件数オーバー
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }

        //************************************************************************
        /// <summary>
        /// エンティティ定義にデータを登録する。
        /// </summary>
        /// <param name="argUpdateData">更新データ</param>
        /// <param name="argOperationTime">操作時刻</param>
        /// <returns>登録したレコード数</returns>
        //************************************************************************
        [CMTransactionAttribute(Timeout=100)]
        public virtual int Update(DataSet argUpdateData, out DateTime argOperationTime)
        {
            // 操作時刻を設定
            CommonDA.Connection = Connection;
            argOperationTime = CommonDA.GetSysdate();

            CMUserInfo uinfo = CMInformationManager.UserInfo;

            // 入力値チェック
            CommonDA.ExistCheckFomXml(argUpdateData.Tables[0]);

            // データアクセス層にコネクション設定
            m_dataAccess.Connection = Connection;

            // 登録実行
            int cnt = m_dataAccess.Update(argUpdateData, argOperationTime);

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// 子テーブルの削除データを追加する。
        /// </summary>
        /// <param name="argDataSet">削除データ追加対象DataSet</param>
        //************************************************************************
        public void AddChildDelRow(DataSet argDataSet)
        {
            foreach (DataRow prow in argDataSet.Tables[0].Rows)
            {
                if (prow["削除"].ToString() == "1")
                {
                    // 検索条件設定
                    List<CMSelectParam> paramList = new List<CMSelectParam>();
                    paramList.Add(new CMSelectParam("項目一覧ID", "= @項目一覧ID", prow["項目一覧ID"]));
                    paramList.Add(new CMSelectParam("VER", "= @VER", prow["VER"]));

                    m_dataAccess.Connection = Connection;

                    // 検索実行
                    bool isOver;
                    DataSet result = m_dataAccess.SelectFromXml(paramList, CMSelectType.Edit,
                        0, out isOver, "XMEM結合テーブル", "XMEM項目");

                    // 削除行を取り込み
                    foreach (DataTable dt in result.Tables)
                    {
                        if (!argDataSet.Tables.Contains(dt.TableName)) argDataSet.Tables.Add(dt.Clone());

                        foreach (DataRow row in dt.Rows)
                        {
                            row["削除"] = "1";
                            argDataSet.Tables[dt.TableName].ImportRow(row);
                        }
                    }
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// 削除行の全VERの削除データを追加する。
        /// </summary>
        /// <param name="argDataSet">削除データ追加対象DataSet</param>
        //************************************************************************
        public void AddDelRow(DataSet argDataSet)
        {
            m_dataAccess.Connection = Connection;

            foreach (DataTable dt in argDataSet.Tables)
            {
                foreach (var delRow in dt.Select("削除 = '1'"))
                {
                    // 検索条件設定
                    List<CMSelectParam> paramList = new List<CMSelectParam>();
                    paramList.Add(new CMSelectParam("項目一覧ID", "= @項目一覧ID", delRow["項目一覧ID"]));
                    paramList.Add(new CMSelectParam("VER", "!= @VER", delRow["VER"]));
                    if (dt.TableName == "XMEM結合テーブル" || dt.TableName == "XMEM項目")
                    {
                        string key2 = dt.TableName == "XMEM結合テーブル" ? "テーブル名" : "項目名";
                        paramList.Add(new CMSelectParam(key2, "= @" + key2, delRow[key2]));
                    }

                    // 検索実行
                    bool isOver;
                    DataSet result = m_dataAccess.SelectFromXml(paramList, CMSelectType.Edit,
                        0, out isOver, dt.TableName);

                    // 削除行を取り込み
                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        row["削除"] = "1";
                        dt.ImportRow(row);
                    }
                }
            }
        }
        #endregion
    }
}
