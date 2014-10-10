using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.Common
{
    public partial class CM項目DataSet
    {
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
            foreach (var row in 項目.Where(i => i.主キー))
            {
                if (builder.Length > 0) builder.Append(" AND ");
                builder.AppendFormat("{0} = @{0}", row.項目名);
            }

            return builder.ToString();
        }

        //************************************************************************
        /// <summary>
        /// モデルのXMLファイルを読み込む。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>モデルデータセット</returns>
        //************************************************************************
        public static CM項目DataSet ReadModelXml(string argName)
        {
            var ds = new CM項目DataSet();
            ds.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argName + ".xml"));

            return ds;
        }

        //************************************************************************
        /// <summary>
        /// フォームのXMLファイルを読み込む。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>フォームデータセット</returns>
        //************************************************************************
        public static CM項目DataSet ReadFormXml(string argName)
        {
            // データセットにファイルを読み込み
            var ds = new CM項目DataSet();
            ds.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "View", argName + ".xml"));

            string entName = ds.項目一覧.First().エンティティID;
            if (!string.IsNullOrEmpty(entName))
            {
                var entDs = CM項目DataSet.ReadModelXml(entName);

                // マージする項目名
                string[] colNames =
                    {
                        "主キー", "項目型", "長さ", "小数桁", "基準値分類CD", "デフォルト", "共通検索ID", "共通検索パラメータ"
                    };

                // フォームにエンティティの情報をマージ
                foreach (var row in ds.項目)
                {
                    var entRows = entDs.項目.Where(i => i.項目名 == row.項目名);
                    if (entRows.Count() == 0) continue;

                    var entRow = entRows.First();

                    foreach (string col in colNames)
                        if (row[col] == DBNull.Value) row[col] = entRow[col];

                    if (row["入力制限"] == DBNull.Value)
                    {
                        if (entRow.必須 == true) row.入力制限 = "必須";
                        else if (!string.IsNullOrEmpty(entRow.入力制限)) row.入力制限 = entRow.入力制限;
                    }
                }
            }
                        
            return ds;
        }

        public partial class 項目Row
        {
            //************************************************************************
            /// <summary>
            /// SqlDbTypeを返す。
            /// </summary>
            /// <returns>SqlDbType</returns>
            //************************************************************************
            public SqlDbType GetDbType()
            {
                var dbType = (CMDbType)Enum.Parse(typeof(CMDbType), 項目型);

                switch (dbType)
                {
                    case CMDbType.コード_可変:
                        return SqlDbType.VarChar;
                    case CMDbType.文字列:
                        return SqlDbType.NVarChar;
                    case CMDbType.金額:
                        return SqlDbType.Money;
                    case CMDbType.数値:
                        return 小数桁 > 0 ? SqlDbType.Decimal : SqlDbType.Int;
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
    }
}
