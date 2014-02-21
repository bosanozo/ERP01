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
using System.Web;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.DA;

using Seasar.Quill.Attrs;
using NEXS.ERP.CM.WEB;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// 共通処理ファサード層
    /// </summary>
    //************************************************************************
    public class CMCommonBL : CMBaseBL, ICMCommonBL
    {
        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMCommonBL() 
        {
        }
        #endregion

        #region ファサードメソッド
        //************************************************************************
        /// <summary>
        /// 現在時刻を取得する。
        /// </summary>
        /// <returns>現在時刻</returns>
        //************************************************************************
        public DateTime GetSysdate()
        {
            CommonDA.Connection = Connection;
            return CommonDA.GetSysdate();
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
            // 検索実行
            CommonDA.Connection = Connection;
            DataTable result = CommonDA.Select(argSelectId, argParams);

            return result;
        }

        //************************************************************************
        /// <summary>
        /// 共通検索呼び出し用引数に指定された検索IDの検索を実行する。
        /// 共通検索呼び出し用引数は複数指定可能とし、検索結果をDataSetに格納する。
        /// </summary>
        /// <param name="args">共通検索呼び出し用引数</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        public DataSet Select(params CMCommonSelectArgs[] args)
        {
            CommonDA.Connection = Connection;

            DataSet dataSet = new DataSet();
            // 検索ループ
            foreach (var arg in args)
            {
                // 検索実行
                DataTable table = CommonDA.Select(arg.SelectId, arg.Params);
                // 検索IDを設定
                string tableName = arg.SelectId;
                int idx = 1;
                while (dataSet.Tables.IndexOf(tableName) >= 0) tableName = arg.SelectId + idx++;
                table.TableName = tableName;
                // DataSetに追加
                dataSet.Tables.Add(table);
            }

            return dataSet;
        }

        //************************************************************************
        /// <summary>
        /// 操作ログを記録する。
        /// </summary>
        /// <param name="argFormName">画面名</param>
        /// <returns>現在時刻</returns>
        //************************************************************************
        public DateTime WriteOperationLog(string argFormName)
        {
            CommonDA.Connection = Connection;

            // 操作ログ記録
            CommonDA.WriteOperationLog(argFormName);

            // 現在時刻返却
            return DateTime.Now;
        }

        //************************************************************************
        /// <summary>
        /// 汎用基準値から区分値名称を取得する。
        /// </summary>
        /// <param name="argKbnList">基準値分類CDのリスト</param>
        /// <returns>区分値名称のDataTable</returns>
        //************************************************************************
        //[Aspect(typeof(CMWebInterceptor))]
        public virtual DataTable SelectKbn(params string[] argKbnList)
        {
            CommonDA.Connection = Connection;
            return CommonDA.SelectKbn(argKbnList);
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
            CommonDA.Connection = Connection;
            return CommonDA.GetRangeCanUpdate(argFormId, argIsRange);
        }

        //************************************************************************
        /// <summary>
        /// 指定された検索IDの検索を指定された条件で実行する。
        /// </summary>
        /// <param name="argSelectId">検索ID</param>
        /// <param name="argParam">検索条件</param>
        /// <param name="argMessage">結果メッセージ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        public DataTable SelectSub(string argSelectId, List<CMSelectParam> argParam,
            out CMMessage argMessage)
        {
            // 検索実行
            bool isOver;
            CommonDA.Connection = Connection;
            DataTable result = CommonDA.SelectSub(argSelectId, argParam, out isOver);

            argMessage = null;
            // 検索結果なし
            if (result.Rows.Count == 0) argMessage = new CMMessage("IV001");
            // 最大検索件数オーバー
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }

        //************************************************************************
        /// <summary>
        /// 更新者を指定された条件で検索する。
        /// </summary>
        /// <param name="argParam">検索条件</param>
        /// <param name="argTables">テーブル名の配列</param>
        /// <param name="argMessage">結果メッセージ</param>
        /// <returns>検索結果</returns>
        //************************************************************************
        public DataTable SelectUpdSub(List<CMSelectParam> argParam, string[] argTables,
            out CMMessage argMessage)
        {
            // 検索実行
            bool isOver;
            CommonDA.Connection = Connection;
            DataTable result = CommonDA.SelectUpdSub(argParam, argTables, out isOver);

            argMessage = null;
            // 検索結果なし
            if (result.Rows.Count == 0) argMessage = new CMMessage("IV001");
            // 最大検索件数オーバー
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }
        #endregion
    }
}
