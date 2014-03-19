/*******************************************************************************
 * 【ERPシステム】
 *
 * 作成者: XXＴ／XX
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.WEB;
using NEXS.ERP.CM.BL;

//************************************************************************
/// <summary>
/// XXマスタメンテ
/// </summary>
//************************************************************************
public partial class CM_XM010F02 : CMBaseEntryForm
{
    #region BLインジェクション用フィールド
    protected XM010BL m_facade;
    #endregion

    #region プロパティ
    /// <summary>
    /// 入力データを保持するDataRow
    /// </summary>
    public DataRow InputRow2 { get; set; }
    #endregion
    
    #region イベントハンドラ
    //************************************************************************
    /// <summary>
    /// ページロード
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        // 共通設定
        Master.Body.Attributes.Remove("onload");

        // 操作モードを設定
        string subName = SetOpeMode(PanelKeyItems, PanelSubItems,
            PanelUpdateInfo, PanelFunction, null, BtnCommit, BtnCancel);

        // 画面ヘッダ初期化
        Master.Title = "エンティティ定義　" + subName;

        // 以下は初期表示以外は処理しない
        if (IsPostBack) return;

        try
        {
            // 区分値の検索
            DataTable kbnTable = CommonBL.SelectKbn("X002", "X003");
            Session["kbnTable"] = kbnTable;

            // 区分値の設定
            DataView dv = new DataView(kbnTable, "分類CD = 'X002'", null, DataViewRowState.CurrentRows);

            // エンティティ種別のアイテム設定
            DataTable table = dv.ToTable();
            エンティティ種別.DataSource = table;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        // 標準の画面表示処理実行
        OnPageOnLoad();

        // 新規で既存選択はVerUP
        if (OpeMode == "Insert" && オブジェクト名.Text.Length > 0)
            VER.Text = (Convert.ToInt32(VER.Text) + 1).ToString();
    }

    //************************************************************************
    /// <summary>
    /// 画面表示処理
    /// </summary>
    //************************************************************************
    protected void OnPageOnLoad()
    {
        // キーを取得
        string paramKey = Request.Params["keys"];

        // 初期表示の場合
        if (paramKey != null)
        {
            // キャンセルボタンの戻り値を初期化
            Session["cancelRet"] = false;

            // パラメータ作成
            List<CMSelectParam> param = CreateSelectParam(paramKey);

            try
            {
                // ファサードの呼び出し
                DateTime operationTime;
                CMMessage message;
                DataSet result = m_facade.Select(param, CMSelectType.Edit, out operationTime, out message);

                DataTable table = result.Tables[0];

                bool found = table.Rows.Count > 0;
                // 新規または検索結果ありの場合
                if (OpeMode == "Insert" || found)
                {
                    // 新規で検索結果なしの場合
                    if (!found)
                    {
                        // デフォルトの行を作成
                        DataRow newRow = table.NewRow();
                        // 新規行にデフォルト値を設定する
                        SetDefaultValue(newRow);
                        // 新規行を追加
                        table.Rows.Add(newRow);
                        // 更新を確定
                        table.AcceptChanges();
                    }

                    // 検索結果を取得
                    InputRow = table.Rows[0];
                    InputRow2 = result.Tables["XMエンティティ"].Rows[0];
                    DataTable girdTable = result.Tables["XMエンティティ項目"];
                    girdTable.DefaultView.Sort = "項目NO";
                    GridView1.DataSource = girdTable;

                    // データバインド実行
                    DataBind();

                    // セッションに検索結果を保持
                    Session["inputRow"] = InputRow;
                    Session["table"] = GridView1.DataSource;

                    // 操作履歴を出力
                    WriteOperationLog();
                }
                // 検索結果なしの場合
                else
                {
                    Master.Body.Attributes.Add("onload",
                        "alert('" + CMMessageManager.GetMessage("IV001") +
                        "'); window.returnValue = false; window.close()");
                }            
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return;
            }
        }
        // 確認画面、戻った画面の場合
        else
        {
            // 編集結果を取得
            InputRow = (DataRow)Session["inputRow"];

            // 結果メッセージを表示
            string mes = (string)Session["retMessage"];
            if (mes != null && mes.Length > 0)
            {
                ((dynamic)Master).ShowMessage("I", mes);
                Session.Remove("retMessage");
            }

            // データバインド実行
            DataBind();
        } 
    }

    //************************************************************************
    /// <summary>
    /// データバインド完了
    /// </summary>
    //************************************************************************
    protected void GridView1_DataBound(object sender, EventArgs e)
    {
        // DropDownListのDataSourceを設定
        SetListDataSource(GridView1.FooterRow);
    }

    //************************************************************************
    /// <summary>
    /// コマンドボタンクリック
    /// </summary>
    //************************************************************************
    protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // 新規の場合
        if (e.CommandName == "New")
        {
            DataTable table = (DataTable)Session["table"];
            GridView1.DataSource = table;

            DataRow newRow = table.NewRow();
            table.Rows.Add(newRow);

            SetData(newRow, GridView1.FooterRow);

            GridView1.DataBind();
        }
    }

    //************************************************************************
    /// <summary>
    /// 行更新開始
    /// </summary>
    //************************************************************************
    protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
    {
        GridView1.EditIndex = e.NewEditIndex;
        GridView1.DataSource = Session["table"];
        GridView1.DataBind();

        // DropDownListのDataSourceを設定
        SetListDataSource(GridView1.Rows[e.NewEditIndex], GetDataRow(e.NewEditIndex)["項目型"].ToString());
    }

    //************************************************************************
    /// <summary>
    /// 行更新キャンセル
    /// </summary>
    //************************************************************************
    protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GridView1.EditIndex = -1;
        GridView1.DataSource = Session["table"];
        GridView1.DataBind();
    }

    //************************************************************************
    /// <summary>
    /// 行更新確定
    /// </summary>
    //************************************************************************
    protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        GridView1.EditIndex = -1;
        GridView1.DataSource = Session["table"];

        SetData(GetDataRow(e.RowIndex), GridView1.Rows[e.RowIndex]);

        GridView1.DataBind();
    }

    //************************************************************************
    /// <summary>
    /// 登録ボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnCommit_Click(object sender, EventArgs e)
    {
        // 標準の登録ボタン押下時処理実行
        OnCommitClick(Master.Body, m_facade);
    }

    //************************************************************************
    /// <summary>
    /// キャンセルボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnCancel_Click(object sender, EventArgs e)
    {
        // 標準のキャンセルボタン押下時処理実行
        OnCancelClick(Master.Body);
    }
    #endregion

    #region overrideメソッド
    //************************************************************************
    /// <summary>
    /// キーデータ文字列から検索パラメータを作成する。
    /// </summary>
    /// <param name="argKey">キーデータ文字列</param>
    /// <returns>検索パラメータ</returns>
    //************************************************************************
    protected override List<CMSelectParam> CreateSelectParam(string argKey)
    {
        List<CMSelectParam> param = new List<CMSelectParam>();

        // 未選択で新規
        if (String.IsNullOrEmpty(argKey))
            param.Add(new CMSelectParam("オブジェクト名", "= @オブジェクト名", ""));
        // 既存レコードを選択
        else
        {
            string[] keys = argKey.Split(',');
            param.Add(new CMSelectParam("オブジェクト名", "= @オブジェクト名", keys[0]));
            param.Add(new CMSelectParam("VER", "= @VER", int.Parse(keys[1])));
        }

        return param;
    }

    //************************************************************************
    /// <summary>
    /// データが変更されているかチェックする。
    /// </summary>
    /// <returns>True:変更あり, False:変更なし</returns>
    //************************************************************************
    protected override bool IsModified()
    {
        // 新規の場合、キー項目チェック
        if (OpeMode == "Insert" && IsPanelModified(PanelKeyItems)) return true;

        // 従属項目チェック
        if (IsPanelModified(PanelSubItems)) return true;

        return false;
    }

    //************************************************************************
    /// <summary>
    /// 新規行にデフォルト値を設定する。
    /// </summary>
    /// <param name="argRow">デフォルト値を設定するDataRow</param>
    //************************************************************************
    protected override void SetDefaultValue(DataRow argRow)
    {
        argRow["オブジェクト型"] = "1";
        argRow["VER"] = 1;
    }

    //************************************************************************
    /// <summary>
    /// DataRowに入力データを設定する。
    /// </summary>
    /// <returns>True:エラーあり, False:エラーなし</returns>
    //************************************************************************
    protected override bool SetInputRow()
    {
        bool hasError = false;

        // 新規の場合
        if (OpeMode == "Insert") SetPanelInputRow(PanelKeyItems);

        // 従属項目値を設定
        SetPanelInputRow(PanelSubItems);

        // エラー有無を返却
        return hasError;
    }
    #endregion

    #region privateメソッド
    //************************************************************************
    /// <summary>
    /// グリッドにバインドされたDataRowの行番号を取得する。
    /// </summary>
    /// <param name="argDataItem">Container.DataItem</param>
    /// <returns>バインドされたDataRowの行番号</returns>
    //************************************************************************
    protected int GetRowIdx(object argDataItem)
    {
        DataRow row = ((DataRowView)argDataItem).Row;
        return row.Table.Rows.IndexOf(row);
    }

    //************************************************************************
    /// <summary>
    /// グリッド上の指定行に対応するDataRowを取得する。
    /// </summary>
    /// <param name="idx">グリッド上の行番号</param>
    /// <returns>対応するDataRow</returns>
    //************************************************************************
    private DataRow GetDataRow(int idx)
    {
        string no = ((HiddenField)GridView1.Rows[idx].FindControl("RowIdx")).Value;
        return ((DataTable)GridView1.DataSource).Rows[int.Parse(no)];
    }

    //************************************************************************
    /// <summary>
    /// DropDownListのDataSourceを設定する。
    /// </summary>
    /// <param name="argGrow">DropDownListがあるGridViewRow</param>
    /// <param name="argValue">SelectedValueに設定する値</param>
    //************************************************************************
    private void SetListDataSource(GridViewRow argGrow, string argValue = null)
    {
        DropDownList ddl = argGrow.FindControl("項目型") as DropDownList;
        DataView dv = new DataView((DataTable)Session["kbnTable"], "分類CD = 'X003'", null, DataViewRowState.CurrentRows);
        ddl.DataSource = dv.ToTable();
        if (!string.IsNullOrEmpty(argValue)) ddl.SelectedValue = argValue;
        ddl.DataBind();
    }

    //************************************************************************
    /// <summary>
    /// DataRowにグリッド上の値を設定する。
    /// </summary>
    /// <param name="row">値を設定するDataRow</param>
    /// <param name="grow">値を保持しているGridViewRow</param>
    //************************************************************************
    private void SetData(DataRow row, GridViewRow grow)
    {
        DataTable table = (DataTable)GridView1.DataSource;

        string _no = ((TextBox)grow.FindControl("項目NO")).Text;
        int no = String.IsNullOrEmpty(_no) || int.Parse(_no) > table.Rows.Count ?
            table.Rows.Count : int.Parse(_no);
        if (no == 0) no = 1;
        int p_no = row["項目NO"] == DBNull.Value ? GridView1.Rows.Count + 1 : Convert.ToInt32(row["項目NO"]);

        row["項目NO"] = no;
        var delFlg = (CheckBox)grow.FindControl("削除フラグ");
        row["削除フラグ"] = delFlg != null ? delFlg.Checked : false;

        foreach (Control ctl in grow.Controls)
        {
            DataControlFieldCell cell = ctl as DataControlFieldCell;
            if (cell == null) continue;

            TemplateField tf = cell.ContainingField as TemplateField;
            if (tf == null) continue;        
        }

        string[] colNames = { "項目型", "説明", "長さ", "必須", "主キー" };
        foreach(string colName in colNames)
        {
            Control c = grow.FindControl(colName);
            if (c == null) continue;
            if (c is TextBox)
            {
                string text = ((TextBox)c).Text;
                Type t = table.Columns[colName].DataType;
                if (t == typeof(int))
                {
                    if (!string.IsNullOrEmpty(text)) row[colName] = int.Parse(text);
                }
                else row[colName] = text;
            }
            else if (c is DropDownList) row[colName] = ((DropDownList)c).SelectedValue;
            else if (c is CheckBox) row[colName] = ((CheckBox)c).Checked;            
        }

        // NOの付け替え
        bool od = p_no < no;
        int from = od ? p_no : no;
        int to = od ? no : p_no;
        int add = od ? -1 : 1;
        int i = from;
        foreach (DataRow drow in table.Select(string.Format("項目NO >= {0} AND 項目NO <= {1}", from, to), "項目NO"))
        {
            int row_no = Convert.ToInt32(drow["項目NO"]);

            if (row_no == no)
            {
                if (!drow.Equals(row)) drow["項目NO"] = no == i ? no + add : i;
            }
            else drow["項目NO"] = i;
            i++;
        }
    }
    #endregion
}