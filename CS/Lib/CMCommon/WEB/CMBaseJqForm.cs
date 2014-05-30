using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// Jquery画面の基底クラス
    /// </summary>
    //************************************************************************
    public class CMBaseJqForm : CMBaseForm
    {
        private Dictionary<string, CM項目DataSet> m_formDsDic = new Dictionary<string, CM項目DataSet>();

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMBaseJqForm()
        {
        }
        #endregion

        #region protectedメソッド
        #region jqGrid
        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからjqGridの列名配列を作成する。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>jqGridの列名配列</returns>
        //************************************************************************
        protected string GetColNames(string argName)
        {
            // データセットを取得
            CM項目DataSet ds = GetFormDataSet(argName);

            // StringBuilder作成
            StringBuilder sb = new StringBuilder();

            // DataColumn追加
            foreach (var row in ds.項目)
                sb.AppendFormat("'{0}', ", string.IsNullOrEmpty(row.ラベル) ? row.項目名 : row.ラベル);

            string[] updateCols = 
                {
                    "作成日時", "作成者ID", "作成者名", "作成者IP", "作成PG",
                    "更新日時", "更新者ID", "更新者名", "更新者IP", "更新PG"
                };

            sb.AppendLine();
            foreach (var col in updateCols)
                sb.AppendFormat("'{0}', ", col);
            sb.Length -= 2;

            return sb.ToString();
        }

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからjqGridの列設定を作成する。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>jqGridの列設定</returns>
        //************************************************************************
        protected string GetColModel(string argName)
        {
            // データセットを取得
            CM項目DataSet ds = GetFormDataSet(argName);

            // StringBuilder作成
            StringBuilder sb = new StringBuilder();

            // DataColumn追加
            foreach (var row in ds.項目)
            {
                string cssClass;
                int maxLen;
                int width;
                GetColParams(row, out cssClass, out maxLen, out width);

                // 項目名
                sb.AppendFormat("{{ name: '{0}', width: {1}, ", row.項目名, width);

                // 型
                CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), row.項目型);

                // align
                if (dbType == CMDbType.金額 || dbType == CMDbType.数値)
                    sb.Append("align: 'right', ");
                else if (dbType == CMDbType.フラグ || dbType == CMDbType.日付 || dbType == CMDbType.日時)
                    sb.Append("align: 'center', ");

                // キー
                if (row.主キー) sb.Append("frozen: true, ");

                // 非表示列
                if (!string.IsNullOrEmpty(row.非表示) && row.非表示.Contains('G'))
                    sb.Append("hidden: true, ");

                // 編集
                if (row.入力制限 != "不可")
                {
                    sb.Append("editable: true, editrules: { ");

                    // 必須入力
                    if (row.入力制限 == "必須") sb.Append("required: true, ");
                    sb.Append("}, ");

                    switch (dbType)
                    {
                        case CMDbType.区分:
                            sb.Append("edittype: 'select', formatter:'select', editoptions: { value:'");
                            int sbLen = sb.Length;
                            foreach (DataRow kbnRow in CommonBL.SelectKbn(row.基準値分類CD).Rows)
                            {
                                if (sb.Length > sbLen) sb.Append(";");
                                sb.AppendFormat("{0}:{1}", kbnRow["基準値CD"], kbnRow["表示名"]);
                            }
                            sb.Append("'}");
                            break;
                        case CMDbType.フラグ:
                            sb.Append("edittype: 'checkbox', editoptions: { value:'true:false' } ");
                            break;
                        case CMDbType.日付:
                            sb.AppendLine();
                            sb.AppendLine("editoptions: { size: 12, maxlength: 10, " +
                                "dataInit: function (el) { $(el).datepicker({ dateFormat: 'yy/mm/dd' }); $(el).addClass('DateInput'); }}");
                            break;
                        default:
                            sb.AppendFormat("editoptions: {{ size: {0}, maxlength: {1}, " +
                                "dataInit: function (el) {{ $(el).addClass('{2}'); ", Math.Min(40, maxLen), maxLen, cssClass);
                            // 共通検索
                            if (!string.IsNullOrEmpty(row.共通検索ID))
                            {
                                sb.AppendFormat("$(el).change({{ selectId: '{0}', selectParam: \"{1}\", selectOut: '{2}' }}, GetCodeValue); ",
                                    row.共通検索ID, row.共通検索パラメータ, row.共通検索結果出力項目);
                            }
                            // 選択ボタン
                            if (row.選択ボタン)
                            {
                                sb.AppendFormat("addSelectButton($(el), {{ nameId: '{0}', selectId: '{1}', dbCodeCol: '{2}', dbNameCol: '{3}'}});",
                                    row.共通検索結果出力項目, row.共通検索ID2, row.コード値列名, row.名称列名);
                            }
                            sb.Append("} }");
                            break;
                    }
                }

                sb.AppendLine("},");
            }

            string[] updateCols = 
                {
                    "作成日時", "作成者ID", "作成者名", "作成者IP", "作成PG",
                    "更新日時", "更新者ID", "更新者名", "更新者IP", "更新PG"
                };

            foreach (var col in updateCols)
            {
                sb.Append("{ ");
                sb.AppendFormat("name:'{0}'", col);
                if (col.EndsWith("日時")) sb.Append(", align: 'center', formatter : 'date', formatoptions:{ newformat: 'Y/m/d H:i:s' }, width: 110");
                else sb.Append(", width: 70");
                sb.AppendLine(" },");
            }
            sb.Length -= 3;

            return sb.ToString();
        }
        #endregion

        #region フォーム
        //************************************************************************
        /// <summary>
        /// 入力欄の要素を作成する。
        /// </summary>
        /// <param name="col">要素に設定するid</param>
        /// <param name="cssClass">要素のclass</param>
        /// <param name="maxLen">最大長</param>
        /// <param name="row">項目Row</param>
        /// <param name="selectForm">検索条件フラグ</param>
        /// <returns>入力欄の要素</returns>
        //************************************************************************
        protected string CreateInput(string col, string cssClass, int maxLen, CM項目DataSet.項目Row row, bool selectForm)
        {
            StringBuilder sb = new StringBuilder();

            // class=\"ui-widget-content ui-corner-all\" 

            // 項目型
            switch ((CMDbType)Enum.Parse(typeof(CMDbType), row.項目型))
            {
                case CMDbType.区分:
                    sb.AppendFormat("<select id=\"Ddl{0}\" name=\"{0}\"", col);
                    if (row.主キー) sb.Append(" key=\"true\"");
                    sb.Append(">");
                    // option
                    if (selectForm) sb.Append("<option value=\"\"></option>");
                    foreach (DataRow kbnRow in CommonBL.SelectKbn(row.基準値分類CD).Rows)
                        sb.AppendFormat("<option value=\"{0}\">{1}</option>", kbnRow["基準値CD"], kbnRow["表示名"]);
                    sb.Append("</select>");
                    break;

                case CMDbType.フラグ:
                    sb.AppendFormat("<input id=\"Chk{0}\" name=\"{0}\" type=\"checkbox\" value=\"true\" />", col);
                    break;

                default:
                    string format = maxLen < 50 ?
                        "<input id=\"Txt{0}\" name=\"{0}\" class=\"{1}\" type=\"text\"" :
                        "<textarea id=\"Txa{0}\" name=\"{0}\" class=\"{1}\"";
                    sb.AppendFormat(format, col, cssClass);
                    if (row.主キー) sb.Append(" key=\"true\"");
                    if (row.入力制限 == "不可")
                    {
                        sb.Append(" readonly=\"readonly\"/>");
                        break;
                    }
                    else
                    {
                        // 共通検索
                        if (!string.IsNullOrEmpty(row.共通検索ID))
                        {
                            sb.AppendFormat(" changeParam =\"{{ selectId: '{0}', selectParam: &quot;{1}&quot;, selectOut: '{2}_{3}' }}\"",
                                row.共通検索ID, row.共通検索パラメータ, col, row.共通検索結果出力項目);
                        }
                    }

                    if (maxLen < 50)
                    {
                        sb.AppendFormat(" maxlength=\"{0}\" size=\"{0}\"", maxLen);
                        sb.Append("/>");
                    }
                    else sb.AppendFormat(" maxlength=\"{0}\" cols=\"50\" rows=\"{1}\"></textarea>", maxLen, Math.Min(maxLen / 50, 3));
                    break;
            }

            // 選択ボタン
            if (row.選択ボタン)
            {
                sb.AppendLine();
                sb.AppendFormat("<input id=\"Btn{0}\" class=\"SelectButton\" type=\"button\" value=\"...\"", col);
                sb.AppendFormat(" clickParam = \"{{ codeId: 'Txt{0}', nameId: '{0}_{1}', selectId: '{2}', dbCodeCol: '{3}', dbNameCol: '{4}' }}\" />",
                    col, row.共通検索結果出力項目, row.共通検索ID2, row.コード値列名, row.名称列名
                );
            }

            // 共通検索結果出力項目
            if (!string.IsNullOrEmpty(row.共通検索結果出力項目))
            {
                sb.AppendFormat("<input id=\"{0}_{1}\" name=\"{1}\" class=\"TextInput\" type=\"text\" readonly=\"readonly\" size=\"{2}\"/>",
                    col, row.共通検索結果出力項目, 30);
            }

            return sb.ToString();
        }

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからフォームを作成する。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <param name="selectForm">検索条件フラグ</param>
        /// <returns>フォーム</returns>
        //************************************************************************
        protected string CreateForm(string argName, bool selectForm = false)
        {
            // データセットを取得
            CM項目DataSet ds = GetFormDataSet(argName);

            // StringBuilder作成
            StringBuilder sb = new StringBuilder();

            int colCnt = 0;

            // 入力欄作成ループ
            foreach (var row in ds.項目)
            {
                // 非表示列は無視
                if (!string.IsNullOrEmpty(row.非表示) && row.非表示.Contains('F')) continue;

                if (colCnt == 0) sb.Append("<tr>");

                string cssClass;
                int maxLen;
                int width;

                string col = GetColParams(row, out cssClass, out maxLen, out width);

                // 項目名
                sb.AppendFormat("<td class=\"ItemName\">{0}</td><td class=\"ItemPanel\">", col);

                // 入力欄
                if (row.FromTo)
                {
                    sb.Append(CreateInput(row.項目名 + "From", cssClass, maxLen, row, selectForm));
                    sb.Append(" ～ ");
                    sb.Append(CreateInput(row.項目名 + "To", cssClass, maxLen, row, selectForm));
                }
                else sb.Append(CreateInput(row.項目名, cssClass, maxLen, row, selectForm));

                sb.Append("</td>");

                // 改行判定
                if (colCnt == 1 || row.改行)
                {
                    sb.AppendLine("</tr>");
                    colCnt = 0;
                }
                else
                {
                    sb.AppendLine();
                    colCnt++;
                }
            }

            return sb.ToString();
        }

#if ASP_Form
    //************************************************************************
    /// <summary>
    /// 入力欄の要素を作成する。
    /// </summary>
    /// <param name="cell">要素を追加するテーブルのセル</param>
    /// <param name="col">要素に設定するid</param>
    /// <param name="cssClass">要素のclass</param>
    /// <param name="maxLen">最大長</param>
    /// <param name="row">項目Row</param>
    /// <param name="selectForm">検索条件フラグ</param>
    /// <returns>入力欄の要素</returns>
    //************************************************************************
    protected void CreateInput(TableCell cell, string col, string cssClass, int maxLen, CM項目DataSet.項目Row row, bool selectForm)
    {
        // 項目型
        switch ((CMDbType)Enum.Parse(typeof(CMDbType), row.項目型))
        {
            case CMDbType.区分:
                DropDownList ddl = new DropDownList();
                ddl.ID = "Ddl" + col;
                if (row.Key) ddl.Attributes["key"] = "true";
                // option
                DataTable kbnTable = CommonBL.SelectKbn(row.基準値分類CD);
                if (selectForm) kbnTable.Rows.InsertAt(kbnTable.NewRow(), 0);
                ddl.DataSource = kbnTable;
                ddl.DataTextField = "表示名";
                ddl.DataValueField = "基準値CD";
                ddl.DataBind();
                cell.Controls.Add(ddl);
                break;

            case CMDbType.フラグ:
                CheckBox chk = new CheckBox();
                chk.ID = "Chk" + col;
                cell.Controls.Add(chk);
                break;

            default:
                TextBox txt = new TextBox();
                txt.ID = "Txt" + col;
                txt.CssClass = cssClass;
                if (row.Key) txt.Attributes["key"] = "true";
                if (row.入力制限 == "不可")
                {
                    txt.ReadOnly = true;;
                    cell.Controls.Add(txt);
                    break;
                }
                else
                {
                    // 共通検索
                    if (!string.IsNullOrEmpty(row.共通検索ID))
                    {
                        txt.Attributes["changeParam"] =
                            string.Format("{{ selectId: '{0}', selectParam: \"{1}\", selectOut: '{2}_{3}' }}",
                            row.共通検索ID, row.共通検索パラメータ, col, row.共通検索結果出力項目);
                    }
                }

                txt.MaxLength = maxLen;
                txt.Attributes["size"] = maxLen.ToString();
                cell.Controls.Add(txt);
                break;
        }

        // 選択ボタン
        if (row.選択ボタン)
        {
            var btn = new System.Web.UI.HtmlControls.HtmlButton();
            btn.Attributes["type"] = "button";
            btn.ID = "Btn" + col;
            btn.Attributes["class"] = "SelectButton";
            btn.InnerText = "...";
            btn.Attributes["clickParam"] =
            string.Format("{{ codeId: 'Txt{0}', nameId: '{0}_{1}', selectId: '{2}', dbCodeCol: '{3}', dbNameCol: '{4}' }}",
                col, row.共通検索結果出力項目, row.共通検索ID2, row.コード値列名, row.名称列名
            );
            cell.Controls.Add(btn);
        }

        // 共通検索結果出力項目
        if (!string.IsNullOrEmpty(row.共通検索結果出力項目))
        {
            TextBox txt = new TextBox();
            txt.ID = col + "_" + row.共通検索結果出力項目;
            txt.CssClass = "TextInput";
            txt.ReadOnly = true;
            txt.Attributes["size"] = "30";
            cell.Controls.Add(txt);
        }
    }

    //************************************************************************
    /// <summary>
    /// 指定のXmlファイルからフォームを作成する。
    /// </summary>
    /// <param name="table">HTML table</param>
    /// <param name="argName">Xmlファイル名</param>
    /// <param name="selectForm">検索条件フラグ</param>
    /// <returns>フォーム</returns>
    //************************************************************************
    protected void CreateForm(Table table, string argName, bool selectForm = false)
    {
        // データセットを取得
        CM項目DataSet ds = GetFormDataSet(argName);

        // StringBuilder作成
        StringBuilder sb = new StringBuilder();

        int colCnt = 0;
        TableRow tableRow = null;

        // 入力欄作成ループ
        foreach (var row in ds.項目)
        {
            if (row.非表示 == "F") continue;

            if (colCnt == 0)
            {
                tableRow = new TableRow();
                table.Rows.Add(tableRow);
            }

            string cssClass;
            int maxLen;
            int width;

            string col = GetColParams(row, out cssClass, out maxLen, out width);

            // 項目名
            TableCell labelCell = new TableCell();
            labelCell.CssClass = "ItemName";
            labelCell.Text = col;
            tableRow.Cells.Add(labelCell);

            TableCell inputCell = new TableCell();
            inputCell.CssClass = "ItemPanel";
            tableRow.Cells.Add(inputCell);

            // 入力欄
            if (row.FromTo)
            {
                CreateInput(inputCell, row.項目名 + "From", cssClass, maxLen, row, selectForm);
                Label t = new Label();
                t.Text= " ～ ";
                inputCell.Controls.Add(t);
                CreateInput(inputCell, row.項目名 + "To", cssClass, maxLen, row, selectForm);
            }
            else CreateInput(inputCell, row.項目名, cssClass, maxLen, row, selectForm);

            // 改行判定
            if (colCnt == 1) colCnt = 0;
            else colCnt++;
        }
    }
#endif

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルから詳細フォームのValidationRuleを作成する。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>詳細フォームのValidationRule</returns>
        //************************************************************************
        protected string GetValidationRules(string argName)
        {
            // データセットを取得
            CM項目DataSet ds = GetFormDataSet(argName);

            // StringBuilder作成
            StringBuilder sb = new StringBuilder();

            // DataColumn追加
            foreach (var row in ds.項目)
            {
                // 非表示列は無視
                if (!string.IsNullOrEmpty(row.非表示) && row.非表示.Contains('F')) continue;

                StringBuilder rule = new StringBuilder();

                // 必須入力
                if (row.入力制限 == "必須") rule.Append("required: true, ");

                if (rule.Length > 0)
                    sb.Append(row.項目名).Append(": { ").Append(rule).AppendLine("},");
            }

            return sb.ToString();
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// 項目パラメータを返す。
        /// </summary>
        /// <param name="argRow">項目Row</param>
        /// <param name="argCssClass">CSSクラス</param>
        /// <param name="argMaxLen">最大長</param>
        /// <returns>項目名</returns>
        //************************************************************************
        private string GetColParams(CM項目DataSet.項目Row argRow, out string argCssClass, out int argMaxLen, out int argWidth)
        {
            string name = string.IsNullOrEmpty(argRow.ラベル) ? argRow.項目名 : argRow.ラベル;

            // 型
            CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), argRow.項目型);

            argCssClass = null;
            argMaxLen = argRow.長さ;
            argWidth = 0;

            switch (dbType)
            {
                case CMDbType.区分:
                    break;
                case CMDbType.フラグ:
                    break;
                case CMDbType.コード:
                case CMDbType.コード_可変:
                    argCssClass = "CodeInput";
                    argWidth = argMaxLen * 8 + 4;
                    break;
                case CMDbType.文字列:
                    argCssClass = "TextInput";
                    argWidth = Math.Min(argMaxLen * 16 + 4, 120);
                    break;
                case CMDbType.金額:
                case CMDbType.数値:
                    argCssClass = "NumberInput";
                    argMaxLen += (argMaxLen - 1) / 3 + argRow.小数桁 > 0 ? argRow.小数桁 + 1 : 0;
                    argWidth = argMaxLen * 8 + 4;
                    break;
                case CMDbType.日付:
                    argCssClass = "DateInput";
                    argMaxLen = 10;
                    argWidth = 70;
                    break;
                case CMDbType.日時:
                    argCssClass = "DateInput";
                    argMaxLen = 19;
                    argWidth = 110;
                    break;
            }

            argWidth = Math.Max(argWidth, System.Windows.Forms.TextRenderer.MeasureText(name, new System.Drawing.Font("MS UI Gothic", 9)).Width);

            return name;
        }

        //************************************************************************
        /// <summary>
        /// フォームデータセットを取得する。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>フォームデータセット</returns>
        //************************************************************************
        protected CM項目DataSet GetFormDataSet(string argName)
        {
            if (!m_formDsDic.ContainsKey(argName))
            {
                // データセットにファイルを読み込み
                CM項目DataSet ds = new CM項目DataSet();
                ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "View", argName + ".xml"));

                string entName = ds.項目一覧.First().エンティティID;
                if (!string.IsNullOrEmpty(entName))
                {
                    CM項目DataSet entDs = new CM項目DataSet();
                    entDs.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", entName + ".xml"));

                    // マージする項目名
                    string[] colNames =
                    {
                        "主キー", "項目型", "長さ", "小数桁", "基準値分類CD", "デフォルト", "共通検索ID", "共通検索パラメータ"
                    };

                    // フォームにエンティティの情報をマージ
                    foreach (var row in ds.項目)
                    {
                        DataRow[] entRows = entDs.項目.Select("項目名='" + row.項目名 + "'");
                        if (entRows.Length == 0) continue;

                        CM項目DataSet.項目Row entRow = (CM項目DataSet.項目Row)entRows[0];

                        foreach (string col in colNames)
                            if (row[col] == DBNull.Value) row[col] = entRow[col];

                        if (row["入力制限"] == DBNull.Value)
                        {
                            if (entRow.必須 == true) row.入力制限 = "必須";
                            else if (!string.IsNullOrEmpty(entRow.入力制限)) row.入力制限 = entRow.入力制限;
                        }
                    }
                }

                // 編集したデータセットを記憶
                m_formDsDic[argName] = ds;
            }

            return m_formDsDic[argName];
        }

        #region リクエスト実行
        //************************************************************************
        /// <summary>
        /// ブラウザからのリクエストを実行する。
        /// </summary>
        /// <param name="argFacade"></param>
        /// <param name="argForm"></param>
        //************************************************************************
        protected void DoRequest(ICMBaseBL argFacade, NameValueCollection argForm = null)
        {
            if (argForm == null) argForm = Request.Form;

            dynamic result = null;

            // ファサードの呼び出し用変数
            DateTime operationTime;
            CMMessage message;

            try
            {
                // 検索の場合
                if (Request.QueryString["_search"] != null)
                {
                    // 検索パラメータ取得
                    List<CMSelectParam> param = CreateSelectParam();

                    CMSelectType selType = Request.QueryString["_search"] == "edit" ? CMSelectType.Edit : CMSelectType.List;

                    // ファサードの呼び出し
                    DataSet ds = argFacade.Select(param, selType, out operationTime, out message);

                    // 返却メッセージの表示
                    if (message != null) ShowMessage(message);

                    DataTable table = ds.Tables[0];
                    
                    // 返却データクラス作成
                    if (selType == CMSelectType.Edit)
                    {
                        ResultDataSet resultDs = new ResultDataSet();

                        // 最初の行のデータを設定
                        if (table.Rows.Count > 0)
                        {
                            DataRow row = table.Rows[0];

                            foreach (DataColumn dcol in table.Columns)
                                resultDs.firstRow.Add(dcol.ColumnName, row[dcol.ColumnName]);
                        }

                        // DataTableを設定
                        foreach (DataTable dt in ds.Tables)
                        {
                            ResultData rd = new ResultData();
                            rd.records = dt.Rows.Count;
                            resultDs.tables.Add(dt.TableName, rd);

                            foreach (DataRow row in dt.Rows)
                                rd.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });

#if ResultTable
                            ResultTable rt = new ResultTable();
                            rt.records = dt.Rows.Count;
                            resultDs.tables.Add(dt.TableName, rt);

                            foreach (DataRow row in dt.Rows)
                            {
                                Dictionary<string, object> record = new Dictionary<string, object>();
                                rt.rows.Add(record);

                                foreach (DataColumn dcol in dt.Columns)
                                {
                                    string name = dcol.ColumnName;
                                    if (name == "ROWNUMBER") record.Add("id", Convert.ToInt32(row[name]));
                                    else if (name == "削除") record.Add("状態", row[name]);
                                    else record.Add(name, row[name]);
                                }
                            }
#endif
                        }

                        // 新規の場合
                        string mode = Request.QueryString["_mode"];
                        if (mode == "new")
                        {
                            // 全て新規行にする
                            foreach (DataTable dt in ds.Tables)
                                foreach (DataRow row in dt.Rows) row.SetAdded();
                        }

                        // 親はクリア
                        if (mode == "new") table.Rows.Clear();

                        result = resultDs;
                    }
                    // 一覧検索
                    else
                    {
                        result = new ResultData();
                        foreach (DataRow row in table.Rows)
                            result.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });
                    }

                    // 検索結果を保存
                    Session[Request.Path + "_DataSet"] = ds;
                }
                // 編集操作の場合
                else if (argForm["oper"] != null)
                {
                    // 検索結果を取得
                    DataSet ds = (DataSet)Session[Request.Path + "_DataSet"];

                    // 編集対象のDataTable取得
                    DataTable table = Request.QueryString["TableName"] != null ?
                        (DataTable)ds.Tables[Request.QueryString["TableName"]] : (DataTable)ds.Tables[0];

                    // 編集対象のDataRow取得
                    string id = argForm["id"];
                    DataRow row = string.IsNullOrEmpty(id) || id == "_empty" ?
                        null : table.Select("ROWNUMBER=" + id).First();

                    string oper = argForm["oper"];

                    switch (oper)
                    {
                        case "add":
                        case "new":
                            // todo:未検索時はtableをクリアする

                            row = table.NewRow();

                            // 新規のidを取得
                            int retId = table.Rows.Count > 0 ? Convert.ToInt32(table.AsEnumerable().Max(tr => tr["ROWNUMBER"])) + 1 : 0;
                            row["ROWNUMBER"] = retId;

                            // パラメータを設定
                            foreach (string key in argForm.Keys)
                            {
                                if (!table.Columns.Contains(key)) continue;

                                row[key] = GetDataColumnVal(table.Columns[key], argForm[key]);
                            }

                            table.Rows.Add(row);

                            // idを返却
                            if (oper == "add") Response.Write(retId.ToString());
                            else result = new ResultStatus { id = retId };
                            break;

                        case "edit":
                            foreach (string key in argForm.Keys)
                            {
                                if (!table.Columns.Contains(key)) continue;

                                string txtVal = argForm[key];
                                object value = GetDataColumnVal(table.Columns[key], txtVal);

                                if (value == DBNull.Value)
                                {
                                    if (row[key] != DBNull.Value) row[key] = value;
                                    continue;
                                }

                                // 型に応じて、値を比較し、DataTableに値を設定する
                                switch (table.Columns[key].DataType.Name)
                                {
                                    case "bool":
                                    case "Boolean":
                                        if (row[key] == DBNull.Value) row[key] = value;
                                        else if (row[key].ToString() != value.ToString())
                                            row[key] = value;
                                        // できてない
                                        break;

                                    case "DateTime":
                                        if (row[key] == DBNull.Value) row[key] = value;
                                        else if (((DateTime)row[key]) != ((DateTime)value))
                                            row[key] = value;
                                        break;

                                    default:
                                        if (row[key].ToString() != txtVal)
                                            row[key] = value;
                                        break;
                                }
                            }

                            // 変更ありの場合、idを返却
                            Response.Write(row.RowState == DataRowState.Modified ? id : "");
                            break;

                        case "del":
                            result = new ResultStatus();
                            foreach (string ids in id.Split(','))
                            {
                                DataRow delRow = table.Select("ROWNUMBER=" + ids).First();
                                delRow["削除"] = "1";
                            }
                            break;

                        case "cancel":
                            row.RejectChanges();

                            // 返却データクラス作成
                            result = new Dictionary<string, object>();

                            if (row.RowState == DataRowState.Detached) row.Delete();
                            else
                            {
                                foreach (DataColumn dcol in table.Columns)
                                {
                                    if (dcol.ColumnName == "作成日時") break;
                                    result.Add(dcol.ColumnName, row[dcol.ColumnName, DataRowVersion.Original]);
                                }
                            }
                            break;

                        case "commit":
                            result = new ResultStatus();
                            DataSet updateDs = ds.GetChanges();

                            if (updateDs != null)
                            {
                                foreach (DataTable dt in updateDs.Tables)
                                {
                                    // 削除行を確定
                                    foreach (var delRow in dt.Select("削除 = '1'")) delRow.Delete();
                                }

                                // ファサードの呼び出し
                                argFacade.Update(updateDs, out operationTime);
                            }
                            else
                            {
                                result.error = true;
                                // エラーメッセージを設定
                                result.messages.Add(new ResultMessage
                                {
                                    messageCd = "WV106",
                                    message = CMMessageManager.GetMessage("WV106"),
                                });
                            }
                            break;

                        case "csvexp":
                            // 検索パラメータ取得
                            List<CMSelectParam> param = CreateSelectParam();

                            // ファサードの呼び出し
                            DataSet expDs = argFacade.Select(param, CMSelectType.Csv, out operationTime, out message);

                            // ヘッダ設定
                            Response.AppendHeader("Content-type", "application/octet-stream; charset=UTF-8");
                            Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
                                ((dynamic)Master).Title + ".xlsx");

                            // Excelファイル作成
                            var xslDoc = CreateExcel(expDs);
                            xslDoc.SaveAs(Response.OutputStream);
                            break;
                    }
               }
            }
            catch (CMException ex)
            {
                Response.StatusCode = 200;

                result = new ResultStatus { error = true };
                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = ex.CMMessage.MessageCd,
                    message = ex.CMMessage.ToString(),
                    rowField = ex.CMMessage.RowField
                });
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                // データベースエラー
                Response.StatusCode = 500;

                result = new ResultStatus { error = true };
                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = "EV002",
                    message = CMMessageManager.GetMessage("EV002", ex.Message)
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(ex.ToString());
            }

            // 結果をJSONで返却
            if (result != null)
            {
                var serializer = new JavaScriptSerializer();
                Response.ContentType = "text/javascript";
                Response.Write(serializer.Serialize(result));
            }
            Response.End();
        }

        //************************************************************************
        /// <summary>
        /// DataColumnに対応した型の値を取得する。
        /// </summary>
        /// <param name="dcol">DataColumn</param>
        /// <param name="value">値の文字列</param>
        /// <returns>DataColumnに対応した型の値</returns>
        //************************************************************************
        protected object GetDataColumnVal(DataColumn dcol, string value)
        {
            if (value.Length == 0) return DBNull.Value;

            object result;

            // 型に応じて、値を比較し、DataTableに値を設定する
            switch (dcol.DataType.Name)
            {
                case "bool":
                case "Boolean":
                    // できてない
                    result = value == "true";  //Convert.ToBoolean(value);
                    break;

                case "decimal":
                    result = Convert.ToDecimal(value);
                    break;

                case "int32":
                case "Byte":
                    result = Convert.ToInt32(value);
                    break;

                case "DateTime":
                    result = Convert.ToDateTime(value);
                    break;

                default:
                    result = value;
                    break;
            }

            return result;
        }

        //************************************************************************
        /// <summary>
        /// 検索パラメータを作成する。
        /// </summary>
        /// <returns>検索パラメータ</returns>
        //************************************************************************
        protected List<CMSelectParam> CreateSelectParam()
        {
            List<CMSelectParam> param = new List<CMSelectParam>();

            foreach (string key in Request.QueryString)
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

                    bool isSetFrom = !string.IsNullOrEmpty(Request.QueryString[key]);
                    bool isSetTo = !string.IsNullOrEmpty(Request.QueryString[toName]);

                    // FromTo
                    if (isSetFrom && isSetTo)
                    {
                        param.Add(new CMSelectParam(colName,
                            string.Format("BETWEEN @{0} AND @{1}", key, toName),
                            Request.QueryString[key], Request.QueryString[toName]));
                    }
                    // From or To
                    else if (isSetFrom || isSetTo)
                    {
                        string op = isSetFrom ? ">= @" + key : "<= @" + toName;

                        param.Add(new CMSelectParam(colName, op, isSetFrom ? Request.QueryString[key] : Request.QueryString[toName]));
                    }
                }
                // 単一項目の場合
                else
                {
                    // 設定ありの場合
                    if (!string.IsNullOrEmpty(Request.QueryString[key]))
                    {
                        string op = "= @";
                        string value = Request.QueryString[key];

                        // LIKE検索の場合
                        if (key.EndsWith("名"))
                        {
                            op = "LIKE @";
                            value = "%" + value + "%";
                        }

                        param.Add(new CMSelectParam(key, op + key, value));
                    }
                }
            }

            return param;
        }
        #endregion
        #endregion
    }
}
