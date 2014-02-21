/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// データテーブル名、行番号、フィールドデータ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMRowField
    {
        #region プロパティ
        /// <summary>
        /// データテーブル名
        /// </summary>
        public string DataTableName { get; set; }

        /// <summary>
        /// 行番号
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// フィールド名
        /// </summary>
        public string FieldName { get; set; }
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argRowNumber">行番号</param>
        //************************************************************************
        public CMRowField(int argRowNumber) : this(argRowNumber, null) {}

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argRowNumber">行番号</param>
        /// <param name="argFieldName">フィールド名</param>
        //************************************************************************
        public CMRowField(int argRowNumber, string argFieldName)
            : this (null, argRowNumber, argFieldName) {}

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argDataTableName">データテーブル名</param>
        /// <param name="argRowNumber">行番号</param>
        //************************************************************************
        public CMRowField(string argDataTableName, int argRowNumber)
            : this (argDataTableName, argRowNumber, null) {}

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argDataTableName">データテーブル名</param>
        /// <param name="argRowNumber">行番号</param>
        /// <param name="argFieldName">フィールド名</param>
        //************************************************************************
        public CMRowField(string argDataTableName, int argRowNumber, string argFieldName)
        {            
            DataTableName = argDataTableName;
            RowNumber = argRowNumber;
            FieldName = argFieldName;
        }
        #endregion
    }
}
