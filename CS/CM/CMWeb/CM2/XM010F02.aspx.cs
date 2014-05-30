using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.WEB;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.DA;

//************************************************************************
/// <summary>
/// 組織マスタメンテ
/// </summary>
//************************************************************************
public partial class CM2_XM010F02 : CMBaseJqForm
{
    protected const string FORM_XML = "XMFN項目一覧";
    protected const string GRID1_XML = "XMFN結合テーブル";
    protected const string GRID2_XML = "XMFN項目";

    #region BLインジェクション用フィールド
    protected XM010BL m_facade;
    #endregion

    #region イベントハンドラ
    //************************************************************************
    /// <summary>
    /// ページロード
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        string oper = Request.Params["oper"];

        NameValueCollection customForm = new NameValueCollection(Request.Form);

        // Ajax
        if (Request.QueryString["_search"] != null || oper != null)
        {
            // 検索結果を取得
            DataSet ds = (DataSet)Session[Request.Path + "_DataSet"];

            // グリッドの編集操作の場合
            if (Request.QueryString["TableName"] != null)
            {
                // 編集対象のDataTable取得
                DataTable table = ds.Tables[Request.QueryString["TableName"]];

                // 編集対象のDataRow取得
                string id = customForm["id"];
                DataRow row = string.IsNullOrEmpty(id) || id == "_empty" ?
                    null : table.Select("ROWNUMBER=" + id).First();

                string nocol = table.TableName == "XMEM結合テーブル" ? "テーブルNO" : "項目NO";
                string no = customForm[nocol];

                switch (oper)
                {
                    case "add":
                    case "new":
                        customForm[nocol] = SetNo(table, row, no, nocol);
                        break;

                    case "edit":
                        customForm[nocol] = SetNo(table, row, no, nocol);
                        break;

                    case "del":
                        customForm[nocol] = SetNo(table, row, table.Select("削除 <> '1'").Length.ToString(), nocol);
                        break;

                    case "cancel":
                        customForm[nocol] = SetNo(table, row, row.RowState == DataRowState.Added ?
                            table.Select("削除 <> '1'").Length.ToString() :
                            row[nocol, DataRowVersion.Original].ToString(), nocol);
                        break;
                }
            }
            else
            {
                switch (oper)
                {
                    // 新版保存
                    case "edit":
                        Session[Request.Path + "_VerUp"] = customForm["VerUp"] == "on";
                        break;

                    // 登録の場合
                    case "commit":
                        string mode = Request.QueryString["_mode"];

                        // 親の行
                        DataRow prow = ds.Tables[0].Rows[0];

                        // 新版の場合は新規に変更
                        if (mode == "edit" && (bool)Session[Request.Path + "_VerUp"])
                        {
                            prow.AcceptChanges();
                            prow.SetAdded();
                        }

                        DataViewRowState state = DataViewRowState.Added;

                        // 修正のときは修正行も対象
                        if (mode == "edit") state |= DataViewRowState.ModifiedCurrent;

                        // 新規行に親のキー値を設定
                        for (int i = 1; i < ds.Tables.Count; i++)
                            foreach (DataRow newRow in ds.Tables[i].Select(null, null, state))
                            {
                                newRow["項目一覧ID"] = prow["項目一覧ID"];

                                if (newRow["VER"].ToString() != prow["VER"].ToString())
                                {
                                    newRow["VER"] = prow["VER"];

                                    // 修正のとき修正行を新規で登録
                                    if (newRow.RowState == DataRowState.Modified)
                                    {
                                        newRow.AcceptChanges();
                                        newRow.SetAdded();
                                    }
                                }
                            }

                        // 削除行の全VERの削除データを追加する
                        m_facade.AddDelRow(ds);
                        break;

                    // XML出力の場合
                    case "xml":
                        WriteXml(ds);
                        break;
                }
            }

            // ブラウザからのリクエストを実行
            DoRequest(m_facade, customForm);
        }
        // ASP.Net
        else
        {
            // 画面ヘッダ初期化
            Master.Title = "項目定義";

            // 初期表示以外は処理しない
            if (IsPostBack) return;

            // 操作履歴を出力
            WriteOperationLog();
        }
    }

    //************************************************************************
    /// <summary>
    /// 指定されたDataSetの内容をXMLファイルを出力する。
    /// </summary>
    /// <param name="ds">DataSet</param>
    //************************************************************************
    private void WriteXml(DataSet ds)
    {
        // ヘッダ設定
        Response.AppendHeader("Content-type", "application/octet-stream; charset=UTF-8");
        Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
            ds.Tables[0].Rows[0]["項目一覧ID"] + ".xml");

        Response.Output.WriteLine("<?xml version=\"1.0\"?>");

        foreach (DataTable dt in ds.Tables)
        {
            bool child = true;
            int start = dt.Columns.IndexOf("項目一覧ID");
            int verInd = dt.Columns.IndexOf("VER");
            int end = dt.Columns.IndexOf("作成日時");

            string filter = null, sort = null;

            if (dt.TableName == "XMEM結合テーブル")
            {
                sort = "テーブルNO";
            }
            else if (dt.TableName == "XMEM項目")
            {
                filter = "削除フラグ is null or 削除フラグ = False";
                sort = "項目NO";
            }
            else child = false;

            int sortInd = dt.Columns.IndexOf(sort);
            if (child) start++; // 親と同じキーは出力しない

            foreach (DataRow row in dt.Select(filter, sort))
            {
                if (child) Response.Output.Write('\t');

                Response.Output.WriteLine("<" + dt.TableName.Substring(4) + ">");

                for (int i = start; i < end; i++)
                {
                    if (i == verInd || i == sortInd || row[i] == DBNull.Value) continue;

                    object value = null;

                    if (dt.Columns[i].DataType == typeof(Boolean))
                    {
                        if (Convert.ToBoolean(row[i])) value = "true";
                    }
                    else if (row[i].ToString().Length > 0) value = row[i];

                    // 値が設定されているときに出力
                    if (value != null)
                    {
                        string colName = dt.Columns[i].ColumnName;

                        // ドロップダウンの値を文字列に変換
                        if (colName == "項目型" || colName == "入力制限")
                        {
                            int intVal = Convert.ToInt32(value);
                            if (colName == "項目型") value = Enum.GetName(typeof(CMDbType), intVal);
                            else if (colName == "入力制限")
                            {
                                if (intVal == 0) continue;
                                value = Enum.GetName(typeof(CMInputType), intVal);
                            }
                        }

                        if (child) Response.Output.Write('\t');
                        Response.Output.WriteLine(string.Format("\t<{0}>{1}</{0}>", colName, value));
                    }
                }

                if (child) Response.Output.WriteLine("\t</" + dt.TableName.Substring(4) + ">");
            }
        }

        Response.Output.WriteLine("</項目一覧>");
    }
    #endregion

    //************************************************************************
    /// <summary>
    /// DataRowにグリッド上の値を設定する。
    /// </summary>
    /// <param name="row">値を設定するDataRow</param>
    /// <param name="grow">値を保持しているGridViewRow</param>
    //************************************************************************
    private string SetNo(DataTable table, DataRow row, string _no, string col)
    {
        int rowCnt = table.Select("削除 <> '1'").Length;
        
        // 編集後のNO
        int no = row == null ? rowCnt + 1 : rowCnt;
        
        if (!string.IsNullOrEmpty(_no))
        {
            int n = int.Parse(_no);

            if (n <= 0) no = 1;
            else if (n <= rowCnt) no = n;
        }

        // 編集前のNO
        int p_no = row == null ? rowCnt + 1: Convert.ToInt32(row[col]);

        // NOの付け替え
        int add = p_no < no ? 1 : -1;
        for (int i = p_no; i != no; i += add)
        {
            foreach (DataRow drow in table.Select(col + " = " + (i + add)))
                drow[col] = i;
        }

        return no.ToString();
    }
}