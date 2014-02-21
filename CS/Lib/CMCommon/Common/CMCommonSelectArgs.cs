/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// 共通検索呼び出し用引数データ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMCommonSelectArgs
    {
        #region プロパティ
        /// <summary>検索ID</summary>
        public string SelectId { get; set; }

        /// <summary>パラメータ</summary>
        public object[] Params { get; set; }
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argSelectId">検索ID</param>
        /// <param name="argParams">パラメータ</param>
        //************************************************************************
        public CMCommonSelectArgs(string argSelectId, params object[] argParams)
        {
            SelectId = argSelectId;
            Params = argParams;
        }
        #endregion
    }
}
