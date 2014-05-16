using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.WEB;
using NEXS.ERP.CM.BL;

using NEXS.ERP.CM.DA;

//************************************************************************
/// <summary>
/// 組織マスタメンテ
/// </summary>
//************************************************************************
public partial class CM2_CMSM010F01 : CMBaseForm
{
    #region BLインジェクション用フィールド
    protected CMSM010BL m_facade;
    #endregion

    private Dictionary<string, CMFormDataSet> m_formDsDic = new Dictionary<string, CMFormDataSet>();

    #region イベントハンドラ
#if ASP_Form
    //************************************************************************
    /// <summary>
    /// 検索条件初期化
    /// </summary>
    //************************************************************************
    protected void TblCondition_Init(object sender, EventArgs e)
    {
        try
        {
            // 入力欄を作成
            //CreateForm((Table)sender, "CMSM組織検索条件", true);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }
#endif
    //************************************************************************
    /// <summary>
    /// ページロード
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        // Ajax
        if (Request.Params["_search"] != null || Request.Params["oper"] != null)
        {
            dynamic result = null;

            // ファサードの呼び出し用変数
            DateTime operationTime;
            CMMessage message;

            try
            {
                // 検索の場合
                if (Request.Params["_search"] != null)
                {
                    // 検索パラメータ取得
                    List<CMSelectParam> param = CreateSelectParam();

                    // ファサードの呼び出し
                    DataSet ds = m_facade.Select(param, CMSelectType.List, out operationTime, out message);

                    // 返却メッセージの表示
                    if (message != null) ShowMessage(message);

                    // 検索結果を保存
                    Session["dataTable"] = ds.Tables[0];

                    // 返却データクラス作成
                    result = new ResultData();
                    foreach (DataRow row in ds.Tables[0].Rows)
                        result.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });
                }
                // 編集操作の場合
                else if (Request.Params["oper"] != null)
                {
                    // 検索結果を取得
                    DataTable table = (DataTable)Session["dataTable"];

                    string oper = Request.Params["oper"];

                    switch (oper)
                    {
                        case "add":
                        case "new":
                            // todo:未検索時はtableをクリアする

                            DataRow row = table.NewRow();

                            // 新規のidを取得
                            int retId = Convert.ToInt32(table.AsEnumerable().Max(tr => tr["ROWNUMBER"])) + 1;
                            row["ROWNUMBER"] = retId;

                            foreach (string key in Request.Form.Keys)
                            {
                                if (!table.Columns.Contains(key)) continue;

                                row[key] = Request.Form[key];
                            }

                            table.Rows.Add(row);

                            // idを返却
                            if (oper == "new") result = new ResultStatus { id = retId };
                            else Response.Write(retId.ToString());
                            break;

                        case "edit":
                            int idx = int.Parse(Request.Params["id"]);
                            row = table.Select("ROWNUMBER=" + idx).First();

                            foreach (string key in Request.Form.Keys)
                            {
                                if (!table.Columns.Contains(key)) continue;

                                switch (table.Columns[key].DataType.Name)
                                {
                                    case "bool":
                                        break;

                                    case "decimal":
                                        decimal dVal = Convert.ToDecimal(Request.Form[key]);
                                        if ((decimal)row[key] != dVal) row[key] = dVal;
                                        break;

                                    case "int32":
                                        int iVal = Convert.ToInt32(Request.Form[key]);
                                        if ((int)row[key] != iVal) row[key] = iVal;
                                        break;

                                    case "DateTime":
                                        break;

                                    default:
                                        if (row[key].ToString() != Request.Form[key])
                                            row[key] = Request.Form[key];
                                        break;
                                }
                            }

                            // 変更ありの場合、idを返却
                            Response.Write(row.RowState == DataRowState.Modified ? idx.ToString() : "");
                            break;

                        case "del":
                            result = new ResultStatus();
                            foreach (string id in Request.Params["id"].Split(','))
                            {
                                DataRow delRow = table.Select("ROWNUMBER=" + id).First();
                                delRow["削除"] = "1";
                            }
                            break;

                        case "cancel":
                            row = table.Select("ROWNUMBER=" + Request.Params["id"]).First();
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
                            DataSet updateDs = table.DataSet.GetChanges();

                            if (updateDs != null)
                            {
                                // 削除行を確定
                                foreach (var delRow in updateDs.Tables[0].Select("削除 = '1'")) delRow.Delete();

                                // ファサードの呼び出し
                                m_facade.Update(updateDs, out operationTime);
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
                            DataSet ds = m_facade.Select(param, CMSelectType.Csv, out operationTime, out message);

                            // ヘッダ設定
                            Response.AppendHeader("Content-type", "application/octet-stream; charset=UTF-8");
                            Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
                                Master.Title + ".xlsx");

                            // Excelファイル作成
                            var xslDoc = CreateExcel(ds);
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
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(ex.ToString());
            }

            // 結果をJSONで返却
            if (result != null)
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Response.ContentType = "text/javascript";
                Response.Write(serializer.Serialize(result));
            }
            Response.End();
        }
        // ASP.Net
        else
        {
            // 組織ＣＤ、組織階層区分を設定
            KaishaCd.Value = CMInformationManager.UserInfo.SoshikiCd;
            SoshikiLayer.Value = CMInformationManager.UserInfo.SoshikiKaisoKbn;

            // 機能ボタン スクリプト登録
            //AddFuncOnclick(BtnSelect, BtnCsvOut, BtnInsert, BtnUpdate, BtnDelete);
            // 画面ヘッダ初期化
            Master.Title = "組織マスタメンテ(jquery版)";

            // 初期表示以外は処理しない
            if (IsPostBack) return;

            // 更新許可を取得
            bool canUpdate = m_commonBL.GetRangeCanUpdate(System.IO.Path.GetFileNameWithoutExtension(this.AppRelativeVirtualPath), false);

            /*
            // 画面初期化
            // 全社以外の場合、会社ＣＤは固定
            if (SoshikiLayer.Value != CMSoshikiKaiso.ALL)
            {
                会社CDFrom.Text = KaishaCd.Value;
                会社CDTo.Text = KaishaCd.Value;
                ProtectTextBox(会社CDFrom);
                ProtectTextBox(会社CDTo);
                B会社CDFrom.Visible = false;
                B会社CDTo.Visible = false;
            }*/
            
            // 操作履歴を出力
            WriteOperationLog();
        }
    }
    #endregion

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
        CMFormDataSet ds = GetFormDataSet(argName);

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
        CMFormDataSet ds = GetFormDataSet(argName);

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
            if (dbType == CMDbType.金額 || dbType == CMDbType.整数 || dbType == CMDbType.小数 )
                sb.Append("align: right, ");
            else if (dbType == CMDbType.フラグ || dbType == CMDbType.日付 || dbType == CMDbType.日時)
                sb.Append("align: center, ");
            
            // キー
            if (row.Key) sb.Append("frozen: true, ");

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
                        sb.Append("edittype: 'checkbox', ");
                        break;
                    case CMDbType.日付:
                        sb.AppendLine();
                        sb.AppendLine("editoptions: { size: 12, maxlength: 10, " +
                            "dataInit: function (el) { $(el).datepicker({ dateFormat: 'yy/mm/dd' }); $(el).addClass('DateInput'); }}");
                        break;
                    default:
                        sb.AppendFormat("editoptions: {{ size: {0}, maxlength: {0}, " +
                            "dataInit: function (el) {{ $(el).addClass('{1}'); ", maxLen, cssClass);
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
        CMFormDataSet ds = GetFormDataSet(argName);

        // StringBuilder作成
        StringBuilder sb = new StringBuilder();

        // DataColumn追加
        foreach (var row in ds.項目)
        {
            StringBuilder rule = new StringBuilder();

            // 必須入力
            if (row.入力制限 == "必須") rule.Append("required: true, ");

            if (rule.Length > 0)
                sb.Append(row.項目名).Append(": { ").Append(rule).AppendLine("},");
        }

        return sb.ToString();
    }

    //************************************************************************
    /// <summary>
    /// 項目パラメータを返す。
    /// </summary>
    /// <param name="argRow">項目Row</param>
    /// <param name="argCssClass">CSSクラス</param>
    /// <param name="argMaxLen">最大長</param>
    /// <returns>項目名</returns>
    //************************************************************************
    private string GetColParams(CMFormDataSet.項目Row argRow, out string argCssClass, out int argMaxLen, out int argWidth)
    {
        string name = string.IsNullOrEmpty(argRow.ラベル) ? argRow.項目名 : argRow.ラベル;
                
        // 型
        CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), argRow.項目型);

        argCssClass = null;
        argMaxLen = argRow.Is項目長Null() ? 0 : argRow.項目長;
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
            case CMDbType.小数: // 数値に統合する
            case CMDbType.整数:
                argCssClass = "NumberInput";
                argMaxLen += (argMaxLen - 1) / 3;
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
    /// 入力欄の要素を作成する。
    /// </summary>
    /// <param name="col">要素に設定するid</param>
    /// <param name="cssClass">要素のclass</param>
    /// <param name="maxLen">最大長</param>
    /// <param name="row">項目Row</param>
    /// <param name="selectForm">検索条件フラグ</param>
    /// <returns>入力欄の要素</returns>
    //************************************************************************
    protected string CreateInput(string col, string cssClass, int maxLen, CMFormDataSet.項目Row row, bool selectForm)
    {
        StringBuilder sb = new StringBuilder();

        // class=\"ui-widget-content ui-corner-all\" 

        // 項目型
        switch ((CMDbType)Enum.Parse(typeof(CMDbType), row.項目型))
        {
            case CMDbType.区分:
                sb.AppendFormat("<select id=\"Ddl{0}\" name=\"{0}\"", col);
                if (row.Key) sb.Append(" key=\"true\"");
                sb.Append(">");
                // option
                if (selectForm) sb.Append("<option value=\"\"></option>");
                foreach (DataRow kbnRow in CommonBL.SelectKbn(row.基準値分類CD).Rows)
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", kbnRow["基準値CD"], kbnRow["表示名"]);
                sb.Append("</select>");
                break;

            case CMDbType.フラグ:
                sb.AppendFormat("<input id=\"Chk{0}\" name=\"{0}\" type=\"checkbox\"/>", col);
                break;

            default:
                sb.AppendFormat("<input id=\"Txt{0}\" name=\"{0}\" class=\"{1}\" type=\"text\"", col, cssClass);
                if (row.Key) sb.Append(" key=\"true\"");
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
                sb.AppendFormat(" maxlength=\"{0}\" size=\"{0}\"", maxLen);
                sb.Append("/>");            
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
    protected void CreateInput(TableCell cell, string col, string cssClass, int maxLen, CMFormDataSet.項目Row row, bool selectForm)
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
        CMFormDataSet ds = GetFormDataSet(argName);

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
    /// フォームデータセットを取得する。
    /// </summary>
    /// <param name="argName">Xmlファイル名</param>
    /// <returns>フォームデータセット</returns>
    //************************************************************************
    protected CMFormDataSet GetFormDataSet(string argName)
    {
        if (!m_formDsDic.ContainsKey(argName))
        {
            // データセットにファイルを読み込み
            CMFormDataSet ds = new CMFormDataSet();
            ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "View", argName + ".xml"));

            string entName = ds.フォーム.First().エンティティ;
            if (!string.IsNullOrEmpty(entName))
            {
                CMEntityDataSet entDs = new CMEntityDataSet();
                entDs.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", entName + ".xml"));

                // マージする項目名
                string[] colNames =
                {
                    "Key", "項目型", "項目長", "小数桁", "基準値分類CD", "デフォルト値", "共通検索ID", "共通検索パラメータ"
                };
                
                // フォームにエンティティの情報をマージ
                foreach(var row in ds.項目)
                {
                    DataRow[] entRows = entDs.項目.Select("項目名='" + row.項目名 + "'");
                    if (entRows.Length == 0) continue;

                    CMEntityDataSet.項目Row entRow = (CMEntityDataSet.項目Row)entRows[0];

                    foreach (string col in colNames)
                        if (row[col] == DBNull.Value) row[col] = entRow[col];

                    if (row["入力制限"] == DBNull.Value)
                    {
                        if (entRow.必須 == true) row.入力制限 = "必須";
                        else if (entRow.更新対象外 == true) row.入力制限 = "不可";
                    }
                }
            }

            // 編集したデータセットを記憶
            m_formDsDic[argName] = ds;
        }

        return m_formDsDic[argName];
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
        CMFormDataSet ds = GetFormDataSet(argName);

        // StringBuilder作成
        StringBuilder sb = new StringBuilder();

        int colCnt = 0;

        // 入力欄作成ループ
        foreach (var row in ds.項目)
        {
            if (row.非表示 == "F") continue;

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
            if (colCnt == 1)
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

                bool isSetFrom = Request.QueryString[key].Length > 0;
                bool isSetTo = Request.QueryString[toName].Length > 0;

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
                if (Request.QueryString[key].Length > 0)
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
}