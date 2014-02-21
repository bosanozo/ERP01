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
using System.Text;
using System.Text.RegularExpressions;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ユーティリティクラス
    /// </summary>
    //************************************************************************
    public static class CMUtil
    {
        //************************************************************************
        /// <summary>
        /// DataRowから行番号を取得する。
        /// </summary>
        /// <param name="argDataRow">DataRow</param>
        /// <returns>行番号(行番号なしの場合は0を返す。)</returns>
        //************************************************************************
        public static int GetRowNumber(DataRow argDataRow)
        {
            int rowNumber = 0;
            // 行番号を取得
            if (argDataRow.Table.Columns.Contains("ROWNUMBER"))
            {
                DataRowVersion version =
                    argDataRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
                rowNumber = Convert.ToInt32(argDataRow["ROWNUMBER", version]);
            }

            return rowNumber;
        }

        //************************************************************************
        /// <summary>
        /// 数値・文字・記号を二種以上使用しているかチェックする。
        /// </summary>
        /// <param name="argPassword">パスワード</param>
        /// <returns>True:二種より少ない</returns>
        //************************************************************************
        public static bool CheckPassword(string argPassword)
        {
            int cnt = 0;
            if (Regex.IsMatch(argPassword, "[0-9]")) cnt++;
            if (Regex.IsMatch(argPassword, "[a-zA-Z]")) cnt++;
            if (Regex.IsMatch(argPassword, "[^a-zA-Z0-9]")) cnt++;

            return cnt < 2;
        }
    }
}
