/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// 検索条件クラス
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMSelectParam
    {
        /// <summary>項目名</summary>
        public string name;
        /// <summary>検索条件SQL</summary>
        public string condtion;
        /// <summary>プレースフォルダに設定するFrom値</summary>
        public object paramFrom;
        /// <summary>プレースフォルダに設定するTo値</summary>
        public object paramTo;
        /// <summary>左辺項目名(leftcol = @name)</summary>
        public string leftCol;
        /// <summary>検索条件を追加するテーブル名</summary>
        /// <remarks>未指定の場合は全テーブルの検索に条件を追加する。</remarks>
        public string tableName;

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argName">項目名</param>
        /// <param name="argCondtion">検索条件SQL</param>
        /// <param name="argValue">プレースフォルダに設定する値</param>
        //************************************************************************
        public CMSelectParam(string argName, string argCondtion, object argValue)
            : this(argName, argCondtion, argValue, null) { }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argName">項目名</param>
        /// <param name="argCondtion">検索条件SQL</param>
        /// <param name="argFrom">プレースフォルダに設定するFrom値</param>
        /// <param name="argTo">プレースフォルダに設定するTo値</param>
        //************************************************************************
        public CMSelectParam(string argName, string argCondtion, object argFrom, object argTo)
            : this(null, argName, argCondtion, argFrom, argTo) { }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argLeftCol">左辺項目名</param>
        /// <param name="argRightCol">右変項目名</param>
        /// <param name="argCondtion">検索条件SQL</param>
        /// <param name="argFrom">プレースフォルダに設定するFrom値</param>
        /// <param name="argTo">プレースフォルダに設定するTo値</param>
        //************************************************************************
        public CMSelectParam(string argLeftCol, string argRightCol,
            string argCondtion, object argFrom, object argTo)
        {
            leftCol = argLeftCol;
            name = argRightCol;
            condtion = argCondtion;
            paramFrom = argFrom;
            paramTo = argTo;
        }
    }
}
