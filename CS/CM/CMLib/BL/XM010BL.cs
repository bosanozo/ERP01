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
            "AND エンティティ名 = A.エンティティ名{1})";

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

                /*
                // パラメータ
                argParam[0].tableName = fnames[0];
                argParam[1].tableName = fnames[0];

                for (int i = 1; i <= 2; i++)
                {
                    CMSelectParam param1 = new CMSelectParam("エンティティ名",
                        "= @エンティティ名", argParam[0].paramFrom);
                    param1.tableName = fnames[i];

                    CMSelectParam param2 = new CMSelectParam("VER",
                        string.Format(VER_COND, fnames[i], i == 1 ? "" : " AND 項目NO = A.項目NO"),
                        argParam[1].paramFrom);
                    param2.tableName = fnames[i];

                    argParam.Add(param1);
                    argParam.Add(param2);
                }*/
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
        #endregion
    }
}
