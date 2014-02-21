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

using Seasar.Quill.Attrs;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// データアクセス層
    /// </summary>
    //************************************************************************
    [Implementation]
    public class CMSM010DA : CMBaseDA
    {
        #region SQL文
        /// <summary>
        /// 検索、登録対象のテーブル名
        /// </summary>
        private const string TABLE_NAME = "CMSM組織";

        /// <summary>
        /// 検索列
        /// </summary>
        private const string SELECT_COLS =
            "A.組織CD," +
            "A.組織名," +
            "A.組織階層区分," +
            "H1.基準値名 組織階層区分名," +
            "A.上位組織CD," +
            "S2.組織名 上位組織名,";

        /// <summary>
        /// 名称取得用外部結合
        /// </summary>
        private const string LEFT_JOIN =
            "LEFT JOIN CMSM組織 S2 ON S2.組織CD = A.上位組織CD " +
            "LEFT JOIN CMSM汎用基準値 H1 ON H1.分類CD = 'M001' AND H1.基準値CD = A.組織階層区分 ";

        /// <summary>
        /// 検索時の並び順
        /// </summary>
        private const string ORDER = "組織CD";
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMSM010DA()
        {
        }
        #endregion

        #region データアクセスメソッド
        //************************************************************************
        /// <summary>
        /// 組織マスタを検索する。
        /// </summary>
        /// <param name="argParam">検索条件</param>
        /// <param name="argSelectType">検索種別</param>
        /// <param name="argMaxRow">最大検索件数</param>
        /// <param name="argIsOver">最大検索件数オーバーフラグ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        public DataSet Select(List<CMSelectParam> argParam, CMSelectType argSelectType,
            int argMaxRow, out bool argIsOver)
        {
            /*
            // 組織階層が全社でなければ、会社の条件を追加
            CMUserInfo uinfo = CMInformationManager.UserInfo;
            if (uinfo.SoshikiKaisoKbn != CMSoshikiKaiso.ALL)
                argParam.Add(new CMSelectParam("会社CD", "= :会社CD", uinfo.KaishaCd));*/

            // WHERE句作成
            StringBuilder where = new StringBuilder();
            AddWhere(where, argParam);

            // SELECT文の設定
            IDbCommand cmd = CreateCommand(
                CreateSelectSql(SELECT_COLS, TABLE_NAME, where.ToString(), LEFT_JOIN, ORDER, argSelectType));
            Adapter.SelectCommand = cmd;

            // パラメータの設定
            SetParameter(cmd, argParam);
            // 一覧検索の場合
            if (argSelectType == CMSelectType.List)
                cmd.Parameters.Add(CreateCmdParam("最大検索件数", argMaxRow));

            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            int cnt = Adapter.Fill(ds);
            // テーブル名を設定
            ds.Tables[0].TableName = TABLE_NAME;

            // 一覧検索で最大検索件数オーバーの場合、最終行を削除
            if (argSelectType == CMSelectType.List && cnt >= argMaxRow)
            {
                argIsOver = true;
                ds.Tables[0].Rows.RemoveAt(cnt - 1);
            }
            else argIsOver = false;

            // 検索結果の返却
            return ds;
        }
        #endregion
    }
}
