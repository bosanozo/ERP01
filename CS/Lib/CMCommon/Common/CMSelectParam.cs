/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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

        //************************************************************************
        /// <summary>
        /// 検索パラメータを作成する。
        /// </summary>
        /// <param name="argQuery">QueryString</param>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>検索パラメータ</returns>
        //************************************************************************
        public static List<CMSelectParam> CreateSelectParam(NameValueCollection argQuery, string argName = null)
        {
            // データセットを取得
            CM項目DataSet ds = argName != null ? ds = CM項目DataSet.ReadFormXml(argName) : null;

            List<CMSelectParam> param = new List<CMSelectParam>();

            foreach (string key in argQuery)
            {
                // 以下は無視
                if (key.StartsWith("_") || key.StartsWith("ctl00$") || key.EndsWith("To") ||
                    System.Text.RegularExpressions.Regex.IsMatch(key, "(nd|rows|page|sidx|sord|oper)")) continue;

                // Fromの場合
                if (key.EndsWith("From"))
                {
                    // Fromなし名称取得
                    string colName = key.Substring(0, key.IndexOf("From"));
                    string toName = colName + "To";

                    bool isSetFrom = !string.IsNullOrEmpty(argQuery[key]);
                    bool isSetTo = !string.IsNullOrEmpty(argQuery[toName]);

                    // FromTo
                    if (isSetFrom && isSetTo)
                    {
                        param.Add(new CMSelectParam(colName,
                            string.Format("BETWEEN @{0} AND @{1}", key, toName),
                            argQuery[key], argQuery[toName]));
                    }
                    // From or To
                    else if (isSetFrom || isSetTo)
                    {
                        string op = isSetFrom ? ">= @" + key : "<= @" + toName;

                        param.Add(new CMSelectParam(colName, op, isSetFrom ? argQuery[key] : argQuery[toName]));
                    }
                }
                // 単一項目の場合
                else
                {
                    // 設定ありの場合
                    if (string.IsNullOrEmpty(argQuery[key])) continue;

                    string op = "= @";
                    string value = argQuery[key];

                    if (ds != null)
                    {
                        // LIKE検索の場合
                        var irows = ds.項目.Where(item => item.項目名 == key);
                        if (irows.Count() > 0 && !string.IsNullOrEmpty(irows.First().一致条件))
                        {
                            if (irows.First().一致条件 != "指定なし") op = "LIKE @";

                            switch (irows.First().一致条件)
                            {
                                case "前方":
                                    value = value + "%";
                                    break;
                                case "中間":
                                    value = "%" + value + "%";
                                    break;
                                case "後方":
                                    value = "%" + value;
                                    break;
                            }
                        }
                    }

                    param.Add(new CMSelectParam(key, op + key, value));
                }
            }

            return param;
        }

        //************************************************************************
        /// <summary>
        /// 検索パラメータ作成
        /// </summary>
        /// <returns>検索パラメータ</returns>
        //************************************************************************
        public static List<CMSelectParam> CreateSelectParam(
            string Name, string Code, string Params, string DbCodeCol, string DbNameCol, string CodeId)
        {
            // 画面の条件を取得
            var formParam = new List<CMSelectParam>();

            if (!string.IsNullOrEmpty(Name))
                formParam.Add(new CMSelectParam("Name", "LIKE @Name", "%" + Name + "%"));

            if (!string.IsNullOrEmpty(Code))
                formParam.Add(new CMSelectParam("Code", "= @Code", Code));

            // 検索コード名
            var codeCol = Regex.Replace(CodeId, "(From|To)", "");
            var nameCol = Regex.Replace(codeCol, "(CD|ID)", "名");

            // 項目名の置き換え
            foreach (var p in formParam)
            {
                if (p.name == "Code") p.name = string.IsNullOrEmpty(DbCodeCol) ?
                    codeCol : DbCodeCol;
                else if (p.name == "Name")
                {
                    p.name = string.IsNullOrEmpty(DbNameCol) ?
                       nameCol : DbNameCol;
                    p.condtion = "LIKE @" + p.name;
                    p.paramFrom = "%" + p.paramFrom + "%";
                }
            }

            // 検索パラメータ作成
            var param = new List<CMSelectParam>();

            // 追加パラメータがある場合、追加する
            if (!string.IsNullOrEmpty(Params))
            {
                foreach (string p in Params.Split())
                {
                    object value;

                    // "#"から始まる場合はUserInfoから設定
                    if (p[0] == '#')
                    {
                        PropertyInfo pi = CMInformationManager.UserInfo.GetType().GetProperty(p.Substring(1));
                        value = pi.GetValue(CMInformationManager.UserInfo, null);
                    }
                    // セルの値を取得
                    else value = p;

                    // パラメータ追加
                    param.Add(new CMSelectParam(null, null, value));
                }
            }

            // 画面の条件を追加
            param.AddRange(formParam);

            return param;
        }
    }
}
