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

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// ファサードインターフェース
    /// </summary>
    //************************************************************************
    public interface ICMBaseBL
    {
        //************************************************************************
        /// <summary>
        /// 検索する。
        /// </summary>
        /// <param name="argParam">検索条件</param>
        /// <param name="argSelectType">検索種別</param>
        /// <param name="argOperationTime">操作時刻</param>
        /// <param name="argMessage">結果メッセージ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        DataSet Select(List<CMSelectParam> argParam, CMSelectType argSelectType,
            out DateTime argOperationTime, out CMMessage argMessage);

        //************************************************************************
        /// <summary>
        /// データを登録する。
        /// </summary>
        /// <param name="argUpdateData">更新データ</param>
        /// <param name="argOperationTime">操作時刻</param>
        /// <returns>登録したレコード数</returns>
        //************************************************************************
        int Update(DataSet argUpdateData, out DateTime argOperationTime);
    }
}
