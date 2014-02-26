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
    public class CMSM010BL : CMBaseBL, ICMBaseBL
    {
        #region インジェクション用フィールド
        protected CMSM010DA m_dataAccess;
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMSM010BL()
        {
        }
        #endregion

        #region ファサードメソッド
        //************************************************************************
        /// <summary>
        /// 組織マスタを検索する。
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

            // 検索実行
            bool isOver;
            m_dataAccess.Connection = Connection;
            DataSet result = m_dataAccess.SelectFromXml(argParam, argSelectType,
                maxRow, out isOver, "CMSM組織");

            argMessage = null;
            // 検索結果なし
            if (result.Tables[0].Rows.Count == 0) argMessage = new CMMessage("IV001");
            // 最大検索件数オーバー
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }

        //************************************************************************
        /// <summary>
        /// 組織マスタにデータを登録する。
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

            //throw new Exception("aaa");

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// 組織マスタにデータをアップロードする。
        /// </summary>
        /// <param name="argUpdateData">更新データ</param>
        /// <param name="argOperationTime">操作時刻</param>
        /// <returns>登録したレコード数</returns>
        //************************************************************************
        [CMTransactionAttribute(Timeout = 100)]
        public virtual int Upload(DataSet argUpdateData, out DateTime argOperationTime)
        {
            // 操作時刻を設定
            CommonDA.Connection = Connection;
            argOperationTime = CommonDA.GetSysdate();

            // 入力値チェック
            CommonDA.ExistCheckFomXml(argUpdateData.Tables[0]);

            // データアクセス層にコネクション設定
            m_dataAccess.Connection = Connection;

            // 登録実行
            int cnt = m_dataAccess.Upload(argUpdateData, argOperationTime);

            return cnt;
        }
        #endregion
    }
}
