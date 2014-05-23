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

        NameValueCollection customParams = new NameValueCollection(Request.QueryString);
        customParams.Add(Request.Form);

        // Ajax
        if (customParams["_search"] != null || oper != null)
        {
            // 検索結果を取得
            DataSet ds = (DataSet)Session[Request.Path + "_DataSet"];

            // グリッドの編集操作の場合
            if (customParams["TableName"] != null)
            {
                // 編集対象のDataTable取得
                DataTable table = ds.Tables[customParams["TableName"]];

                // 編集対象のDataRow取得
                string id = customParams["id"];
                DataRow row = string.IsNullOrEmpty(id) || id == "_empty" ?
                    null : table.Select("ROWNUMBER=" + id).First();

                string nocol = table.TableName == "XMEM結合テーブル" ? "テーブルNO" : "項目NO";
                string no = customParams[nocol];

                switch (oper)
                {
                    case "add":
                    case "new":
                        customParams[nocol] = SetNo(table, row, no, nocol);
                        break;

                    case "edit":
                        customParams[nocol] = SetNo(table, row, no, nocol);
                        break;

                    case "del":
                        break;

                    case "cancel":
                        break;
                }
            }
            // 登録の場合
            else if (oper == "commit")
            {
                // 新規行に親のキー値を設定
                DataRow prow = ds.Tables[0].Rows[0];

                for (int i = 1; i < ds.Tables.Count; i++)
                    foreach (DataRow newRow in ds.Tables[i].Select(null, null, DataViewRowState.Added))
                    {
                        newRow["項目一覧ID"] = prow["項目一覧ID"];
                        newRow["VER"] = prow["VER"];
                    }
            }

            // ブラウザからのリクエストを実行
            DoRequest(m_facade, customParams);
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
        int no;
 
        // 新規
        if (row == null)         
            no = string.IsNullOrEmpty(_no) || int.Parse(_no) >= table.Rows.Count ?
            table.Rows.Count + 1: int.Parse(_no);
        // 修正
        else
            no = string.IsNullOrEmpty(_no) || int.Parse(_no) > table.Rows.Count ?
            table.Rows.Count : int.Parse(_no);
        
        if (no == 0) no = 1;

        // 編集前のNO
        int p_no = row == null ? table.Rows.Count : Convert.ToInt32(row[col]);

        あああああ

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