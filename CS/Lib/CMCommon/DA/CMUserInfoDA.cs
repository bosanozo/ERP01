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
using System.Linq;
using System.Text;

using Seasar.Quill.Attrs;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// ユーザー情報検索データアクセス層
    /// </summary>
    //************************************************************************
    [Implementation]
    public class CMUserInfoDA : CMBaseDA
    {
        #region SQL文
        /// <summary>
        /// SELECT文
        /// </summary>
        private const string SELECT_SQL =
            "SELECT " +
            "U.ユーザID ID," +
            "U.ユーザ名 NAME," +
            "U.パスワード PASSWD," +
            "'' ROLE," +
            "U.組織CD," +
            "S1.組織名," +
            "S1.組織階層区分 " +
            "FROM CMSMユーザ U " +
            "JOIN CMSM組織 S1 ON S1.組織CD = U.組織CD " +
            "WHERE U.ユーザID = @ID";

        /// <summary>
        /// ロールSELECT文
        /// </summary>
        private const string SELECT_ROLE_SQL =
            "SELECT " +
            "ロールID ROLE " +
            "FROM CMSMユーザロール " +
            "WHERE ユーザID = @ID " +
            "ORDER BY ロールID";
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMUserInfoDA()
        {
        }
        #endregion

        #region データアクセスメソッド
        //************************************************************************
        /// <summary>
        /// ユーザー情報を取得する。
        /// </summary>
        /// <param name="argUserId">ユーザーID</param>
        /// <returns>ユーザー情報DataRow</returns>
        //************************************************************************
        public DataRow FindById(string argUserId)
        {
            // SelectCommandの設定
            Adapter.SelectCommand = CreateCommand(SELECT_SQL);
            // パラメータの設定
            Adapter.SelectCommand.Parameters.Add(CreateCmdParam("ID", argUserId));

            // データセットの作成
            DataSet ds = new DataSet();
            // データの取得
            int cnt = Adapter.Fill(ds);

            // 検索結果なし
            if (cnt == 0) return null;

            // ロールを検索
            Adapter.SelectCommand.CommandText = SELECT_ROLE_SQL;
            // データテーブルの作成
            // データセットの作成
            DataSet roleDs = new DataSet();
            // データの取得
            Adapter.Fill(roleDs);

            // ロールを,区切りで結合
            StringBuilder sb = new StringBuilder();
            foreach (DataRow row in roleDs.Tables[0].Rows)
            {
                if (sb.Length > 0) sb.Append(',');
                sb.Append(row["ROLE"].ToString());
            }
            ds.Tables[0].Rows[0]["ROLE"] = sb.ToString();

            // 検索結果の返却
            return ds.Tables[0].Rows[0];
        }
        #endregion
    }
}
