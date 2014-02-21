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
    /// メッセージデータ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMMessage
    {
        #region プロパティ
        /// <summary>
        /// メッセージコード
        /// </summary>
        public string MessageCd { get; set; }

        /// <summary>
        /// データテーブル名、行番号、フィールドデータ
        /// </summary>
        public CMRowField RowField { get; set; }

        /// <summary>
        /// パラメータ
        /// </summary>
        public object[] Params { get; set; }
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// メッセージデータを生成する。
        /// </summary>
        /// <param name="argMsgCode">メッセージコード</param>
        /// <param name="argParams">パラメータ</param>
        //************************************************************************
        public CMMessage(string argMsgCode, params object[] argParams)
        {
            MessageCd = argMsgCode;
            Params = argParams;
        }

        //************************************************************************
        /// <summary>
        /// コンストラクタ（行番号指定）
        /// メッセージデータを生成する。
        /// </summary>
        /// <param name="argMsgCode">メッセージコード</param>
        /// <param name="argRowField">行番号</param>
        /// <param name="argParams">パラメータ</param>
        //************************************************************************
        public CMMessage(string argMsgCode, CMRowField argRowField,
            params object[] argParams) : this(argMsgCode, argParams)
        {
            RowField = argRowField;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// メッセージ文字列を返す。
        /// </summary>
        //************************************************************************
        public override string ToString()
        {
            return CMMessageManager.GetMessage(MessageCd, Params);
        }
    }
}
