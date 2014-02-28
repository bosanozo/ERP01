/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

using System.Data.SqlClient;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// DB項目種別
    /// </summary>
    //************************************************************************
    public enum CMDbType
    {
        コード, コード_可変, 区分, 文字列, 金額, 整数, 小数, フラグ, 日付, 日時
    }

    //************************************************************************
    /// <summary>
    /// Commandパラメータ
    /// </summary>
    //************************************************************************
    public class CMCmdParam
    {
        /// <summary>パラメータ名</summary>
        [Category("共通部品")]
        [Description("パラメータ名")]
        public string Name { get; set; }

        /// <summary>DataTableの列名(パラメータ名と異なる場合に指定)</summary>
        [Category("共通部品")]
        [Description("DataTableの列名(パラメータ名と異なる場合に指定)")]
        [DefaultValue(null)]
        public string SourceColumn { get; set; }

        /// <summary>DB項目の型</summary>
        [Category("共通部品")]
        [Description("DB項目の型")]
        [DefaultValue(CMDbType.コード)]
        public CMDbType DbType { get; set; }

        /// <summary>キー項目フラグ</summary>
        [Category("共通部品")]
        [Description("キー項目フラグ")]
        [DefaultValue(false)]
        public bool IsKey { get; set; }

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMCmdParam()
        {
            DbType = CMDbType.コード;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// SqlDbTypeを返す。
        /// </summary>
        /// <returns>SqlDbType</returns>
        //************************************************************************
        public SqlDbType GetDbType()
        {
            switch (DbType)
            {
                case CMDbType.コード_可変:
                    return SqlDbType.VarChar;
                case CMDbType.文字列:
                    return SqlDbType.NVarChar;
                case CMDbType.金額:
                    return SqlDbType.Money;
                case CMDbType.整数:
                    return SqlDbType.Int;
                case CMDbType.小数:
                    return SqlDbType.Decimal;
                case CMDbType.フラグ:
                    return SqlDbType.Bit;
                case CMDbType.日付:
                    return SqlDbType.Date;
                case CMDbType.日時:
                    return SqlDbType.DateTime;
                default:
                    return SqlDbType.Char;
            }
        }
    }

    //************************************************************************
    /// <summary>
    /// SqlCommand設定
    /// </summary>
    //************************************************************************
    public class CMCmdSetting
    {
        /// <summary>テーブル名</summary>
        [Category("共通部品")]
        [Description("テーブル名")]
        public string Name { get; set; }

        /// <summary>SqlCommandパラメータ配列</summary>
        [Category("共通部品")]
        [Description("SqlCommandパラメータ配列")]
        public CMCmdParam[] ColumnParams { get; set; }

        //************************************************************************
        /// <summary>
        /// 主キーの検索条件を返す。
        /// </summary>
        /// <returns>主キーの検索条件</returns>
        //************************************************************************
        public string GetKeyCondition()
        {
            StringBuilder builder = new StringBuilder();
            // テーブル項目列でループ
            foreach (var row in ColumnParams)
            {
                if (row.IsKey)
                {
                    if (builder.Length > 0) builder.Append(" AND ");
                    builder.AppendFormat("{0} = @{0}", row.Name);
                }
            }

            return builder.ToString();
        }    
    }
}
