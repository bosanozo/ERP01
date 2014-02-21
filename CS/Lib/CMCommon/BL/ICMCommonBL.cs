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

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// 共通処理ファサード層のインタフェース
    /// </summary>
    //************************************************************************
    [Implementation(typeof(CMCommonBL))]
    public interface ICMCommonBL
    {
        //************************************************************************
        /// <summary>
        /// 現在時刻を取得する。
        /// </summary>
        /// <returns>現在時刻</returns>
        //************************************************************************
        DateTime GetSysdate();

        //************************************************************************
        /// <summary>
        /// 指定された検索IDの検索を指定された条件で実行する。
        /// </summary>
        /// <param name="argSelectId">検索ID</param>
        /// <param name="argParams">パラメータ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        DataTable Select(string argSelectId, params object[] argParams);

        //************************************************************************
        /// <summary>
        /// 共通検索呼び出し用引数に指定された検索IDの検索を実行する。
        /// 共通検索呼び出し用引数は複数指定可能とし、検索結果をDataSetに格納する。
        /// </summary>
        /// <param name="args">共通検索呼び出し用引数</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        DataSet Select(params CMCommonSelectArgs[] args);

        //************************************************************************
        /// <summary>
        /// 操作ログを記録する。
        /// </summary>
        /// <param name="argFormName">画面名</param>
        /// <returns>現在時刻</returns>
        //************************************************************************
        DateTime WriteOperationLog(string argFormName);

        //************************************************************************
        /// <summary>
        /// 汎用基準値から区分値名称を取得する。
        /// </summary>
        /// <param name="argKbnList">基準値分類CDのリスト</param>
        /// <returns>区分値名称のDataTable</returns>
        //************************************************************************
        DataTable SelectKbn(params string[] argKbnList);

        //************************************************************************
        /// <summary>
        /// 参照範囲, 更新許可を検索する。
        /// </summary>
        /// <param name="argFormId">画面ＩＤ</param>
        /// <param name="argIsRange">True:参照範囲, False:更新許可</param>
        /// <returns>True:会社、更新可, False:拠点、更新不可</returns>
        //************************************************************************
        bool GetRangeCanUpdate(string argFormId, bool argIsRange);

        //************************************************************************
        /// <summary>
        /// 指定された検索IDの検索を指定された条件で実行する。
        /// </summary>
        /// <param name="argSelectId">検索ID</param>
        /// <param name="argParam">検索条件</param>
        /// <param name="argMessage">結果メッセージ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        DataTable SelectSub(string argSelectId, List<CMSelectParam> argParam,
            out CMMessage argMessage);

        //************************************************************************
        /// <summary>
        /// 更新者を指定された条件で検索する。
        /// </summary>
        /// <param name="argParam">検索条件</param>
        /// <param name="argTables">テーブル名の配列</param>
        /// <param name="argMessage">結果メッセージ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        DataTable SelectUpdSub(List<CMSelectParam> argParam, string[] argTables,
            out CMMessage argMessage);
    }
}
