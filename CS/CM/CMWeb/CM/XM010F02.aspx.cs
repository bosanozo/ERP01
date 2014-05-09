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
using NEXS.ERP.CM.DA;

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
    /// 入力項目グループ１にバインドするDataRow
    /// </summary>
    public DataRow FormRow1
    {
        get
        {
            DataTable table = FormDataSet.Tables[0];
            if (table.Rows.Count == 0) return null;
            return table.Rows[0];
        }
    }

    /// <summary>
    /// 入力項目グループ２にバインドするDataRow
    /// </summary>
    public DataRow FormRow2
    {
        get
        {
            DataTable table = FormDataSet.Tables["XMエンティティ"];
            if (table.Rows.Count == 0)
            {
                DataRow newRow = table.NewRow();
                newRow["VER"] = FormRow1["VER"];
                newRow["エンティティ種別"] = "M";
                table.Rows.Add(newRow);
            }
            return table.Rows[0];
        }
    }

    /// <summary>
    /// グリッドバインドするDataTable
    /// </summary>
    public DataTable FormTable
    {
        get
        {
            return FormDataSet.Tables["XMエンティティ項目"];
        }
    }

    /// <summary>
    /// FormにバインドするDataSet
    /// </summary>
    public DataSet FormDataSet
    {
        get
        {
            return (DataSet)Session["FormDataSet"];
        }
        set
        {
            Session["FormDataSet"] = value;
        }
    }
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

        // 操作モードに応じた設定
        switch (OpeMode)
        {
            case "Insert":
                break;

            case "Update":
                VerUP.Visible = true;
                break;

            //case "Delete":
            default:
                ProtectPanel(PanelSubItems2);
                GridView1.ShowFooter = false;
                GridView1.Columns[0].Visible = false;
                break;
        }

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
    }

    #region グリッドイベント
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
    /// 行データバインド完了
    /// </summary>
    //************************************************************************
    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowIndex < 0) return;

        // 削除行の背景色変更
        CheckBox cb = e.Row.FindControl("削除フラグ") as CheckBox;
        if (cb.Checked) e.Row.BackColor = System.Drawing.Color.LightGray;
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
            GridView1.DataSource = FormTable;

            DataRow newRow = FormTable.NewRow();
            newRow["VER"] = FormRow1["VER"];
            FormTable.Rows.Add(newRow);

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
        GridView1.DataSource = FormTable;
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
        GridView1.DataSource = FormTable;
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
        GridView1.DataSource = FormTable;

        SetData(GetDataRow(e.RowIndex), GridView1.Rows[e.RowIndex]);

        GridView1.DataBind();
    }
    #endregion

    //************************************************************************
    /// <summary>
    /// 登録ボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnCommit_Click(object sender, EventArgs e)
    {
        // 標準の登録ボタン押下時処理実行
        OnCommitClick();
    }

    //************************************************************************
    /// <summary>
    /// ダウンロードボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnDownLoad_Click(object sender, EventArgs e)
    {
        // 出力データ作成
        CMEntityDataSet ds = new CMEntityDataSet();
        ds.エンティティ.AddエンティティRow(エンティティ種別.SelectedValue + オブジェクト名.Text, null);
        foreach (DataRow row in FormTable.Select("削除フラグ <> True", "項目NO"))
        {
            var newRow = ds.項目.New項目Row();
            newRow.項目名 = row["項目名"].ToString();
            newRow.項目型 = row["項目型名"].ToString();
            newRow["項目長"] = row["長さ"];
            newRow["小数桁"] = row["小数桁"];
            if ((bool)row["必須"] == true) newRow.必須 = true;
            if ((bool)row["主キー"] == true) newRow.Key = true;
            if (row["デフォルト"].ToString().Length > 0)
                newRow.デフォルト値 = row["デフォルト"].ToString();
            ds.項目.Add項目Row(newRow);
        }

        // ヘッダの設定
        Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
            HttpUtility.UrlEncode(FormRow1["オブジェクト名"].ToString()) + ".xml");
        // 出力
        Response.Write(ds.GetXml());
        Response.End();
    }

    //************************************************************************
    /// <summary>
    /// キャンセルボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnCancel_Click(object sender, EventArgs e)
    {
        // 標準のキャンセルボタン押下時処理実行
        OnCancelClick();
    }
    #endregion

    #region イベントハンドラから呼ばれるメソッド
    //************************************************************************
    /// <summary>
    /// 画面表示処理
    /// </summary>
    //************************************************************************
    protected void OnPageOnLoad()
    {
        // キーを取得
        string paramKey = Request.Params["keys"];
        if (paramKey.Length == 0) paramKey = ",1";

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
                    // 新規の場合、既存行のVERは1にする
                    if (OpeMode == "Insert")
                    {
                        foreach (DataTable table1 in result.Tables)
                        {
                            foreach (DataRow row in table1.Rows) row["VER"] = 1;
                        }

                        result.AcceptChanges();
                    }

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

                    // セッションに検索結果を保持
                    FormDataSet = result;
                    InputRow = FormRow1;

                    // グリッド
                    FormTable.DefaultView.Sort = "項目NO";
                    // 空行の追加
                    if (FormTable.Rows.Count == 0)
                    {
                        DataRow newRow = FormTable.NewRow();
                        newRow["項目名"] = "項目１";
                        newRow["VER"] = FormRow1["VER"];
                        newRow["項目NO"] = 1;
                        newRow["削除フラグ"] = false;
                        newRow["項目型"] = "1";
                        newRow["必須"] = false;
                        newRow["主キー"] = false;
                        FormTable.Rows.Add(newRow);
                        FormTable.AcceptChanges();
                    }
                    GridView1.DataSource = FormTable;

                    // データバインド実行
                    DataBind();

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
    /// 登録ボタン押下時処理
    /// </summary>
    //************************************************************************
    protected void OnCommitClick()
    {
        // 登録DataSet
        DataSet inputDataSet = FormDataSet;

        // 新規、修正の場合
        if (OpeMode == "Insert" || OpeMode == "Update")
        {
            // データが更新されていなければ、アラート表示
            if (!IsModified())
            {
                ShowMessage("WV106");
                return;
            }

            // 入力データを設定
            bool hasError = SetInputRow();

            // エラーがなければ登録実行
            if (hasError) return;

            // 新規確認の場合
            if (OpeMode == "Insert")
            {
                // 全DataTable、全DataRow新規にする
                foreach (DataTable table in inputDataSet.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (row.RowState == DataRowState.Modified) row.AcceptChanges();
                        if (row.RowState == DataRowState.Unchanged) row.SetAdded();
                    }
                }
            }

            // VerUPの場合、変更行のVERを1UPし、新規にする
            if (VerUP.Checked)
            {
                int newVer = int.Parse(VER.Text) + 1;

                foreach (DataTable table in inputDataSet.Tables)
                {
                    foreach (DataRow row in table.Rows)
                        if (row.RowState == DataRowState.Added ||
                            row.RowState == DataRowState.Modified ||
                            table.TableName == "XM更新履歴")
                        {
                            row["VER"] = newVer;

                            // 修正は新規に変更
                            if (row.RowState == DataRowState.Modified)
                            {
                                row.AcceptChanges();
                                row.SetAdded();
                            }
                        }
                }
            }

            // キー項目値を設定
            foreach (DataRow row in inputDataSet.Tables["XMエンティティ"].Rows)
            {
                if (row.RowState == DataRowState.Added)
                    row["エンティティ名"] = FormRow1["オブジェクト名"];
            }

            // キー項目値を設定
            foreach (DataRow row in inputDataSet.Tables["XMエンティティ項目"].Rows)
            {
                if (row.RowState == DataRowState.Added)
                    row["エンティティ名"] = FormRow1["オブジェクト名"];
            }
        }
        // 削除確認の場合
        else
        {
            // 全DataTable、全DataRow削除
            inputDataSet = FormDataSet.Copy();
            foreach (DataTable table in inputDataSet.Tables)
            {
                foreach (DataRow row in table.Rows) row.Delete();
            }
        }

        try
        {
            // ファサードの呼び出し
            DateTime operationTime;
            m_facade.Update(inputDataSet, out operationTime);

            // 新規、修正の場合
            if (OpeMode == "Insert" || OpeMode == "Update")
            {
                // 変更を確定
                FormDataSet.AcceptChanges();
                // セッションに編集結果を保持
                Session["retMessage"] = CMMessageManager.GetMessage("IV003");
                Session["cancelRet"] = true;
                // 新規画面へリダイレクト
                //Response.Redirect(Request.Path + "?mode=" + OpeMode);
            }
            // 削除確認の場合、画面を閉じる
            else Close(true);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    //************************************************************************
    /// <summary>
    /// キャンセルボタン押下時処理
    /// </summary>
    //************************************************************************
    protected void OnCancelClick()
    {
        // セッションからデータを取得
        bool retVal = (bool)Session["cancelRet"];

        // 新規、修正の場合
        if (OpeMode == "Insert" || OpeMode == "Update")
        {
            string msgcd = IsModified() ? "QV005" : "QV006";

            // 確認画面を表示
            Master.Body.Attributes.Add("onload",
                string.Format("if (confirm('{0}')) {{window.returnValue = {1}; window.close()}}",
                    CMMessageManager.GetMessage(msgcd).Replace("\r\n", "\\n"), retVal.ToString().ToLower()));
        }
        else Close(retVal);
    }
    #endregion

    #region 共通部品メソッド
    //************************************************************************
    /// <summary>
    /// パネルのデータが変更されているかチェックする。
    /// </summary>
    /// <param name="argPanel">パネル</param>
    /// <returns>True:変更あり, False:変更なし</returns>
    //************************************************************************
    protected bool IsPanelModified(Panel argPanel, DataRow argRow)
    {
        foreach (Control c in argPanel.Controls)
        {
            WebControl wc = c as WebControl;

            // テキストとドロップダウンが対象
            if (!(wc is DropDownList) && !(wc is TextBox)) continue;

            if (argRow.RowState == DataRowState.Added) return true;

            // 値を比較
            if (argRow[wc.ID, DataRowVersion.Original].ToString() != GetValue(wc).ToString())
                return true;
        }

        return false;
    }

    //************************************************************************
    /// <summary>
    /// パネルに設定された値をInputRowに設定する。
    /// </summary>
    /// <param name="argPanel">パネル</param>
    //************************************************************************
    protected void SetPanelInputRow(Panel argPanel, DataRow argRow)
    {
        foreach (Control c in argPanel.Controls)
        {
            WebControl wc = c as WebControl;

            // テキストとドロップダウンが対象
            if (!(wc is DropDownList) && !(wc is TextBox)) continue;

            // 値を設定
            if (argRow[wc.ID].ToString() != GetValue(wc).ToString())
                argRow[wc.ID] = GetValue(wc);
        }
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
        if (OpeMode == "Insert" && IsPanelModified(PanelKeyItems, FormRow1)) return true;

        // 従属項目チェック
        if (IsPanelModified(PanelSubItems, FormRow1)) return true;
        if (IsPanelModified(PanelSubItems2, FormRow2)) return true;

        // グリッドのチェック
        if (FormTable.GetChanges() != null) return true;

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
        if (OpeMode == "Insert") SetPanelInputRow(PanelKeyItems, FormRow1);

        // 従属項目値を設定
        SetPanelInputRow(PanelSubItems, FormRow1);
        SetPanelInputRow(PanelSubItems2, FormRow2);

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
        int no = string.IsNullOrEmpty(_no) || int.Parse(_no) > table.Rows.Count ?
            table.Rows.Count : int.Parse(_no);
        if (no == 0) no = 1;
        int p_no = row["項目NO"] == DBNull.Value ? GridView1.Rows.Count + 1 : Convert.ToInt32(row["項目NO"]);

        // 項目値をDataRowに設定
        string[] colNames = { "削除フラグ", "項目名", "説明", "項目型", "長さ", "小数桁", "必須", "主キー", "デフォルト" };
        foreach(string colName in colNames)
        {
            Control c = grow.FindControl(colName);
            if (c == null) continue;
            if (c is TextBox)
            {
                string text = ((TextBox)c).Text;
                Type t = table.Columns[colName].DataType;
                if (t == typeof(int) || t == typeof(byte))
                {
                    if (!string.IsNullOrEmpty(text)) row[colName] = int.Parse(text);
                }
                else row[colName] = text;
            }
            else if (c is DropDownList) row[colName] = ((DropDownList)c).SelectedValue;
            else if (c is CheckBox) row[colName] = ((CheckBox)c).Checked;            
        }

        // NOの付け替え
        int add = p_no < no ? 1 : -1;
        for (int i = p_no; i != no; i += add)
        {
            foreach (DataRow drow in table.Select("項目NO = " + (i + add)))
                drow["項目NO"] = i;
        }

        row["項目NO"] = no;
    }
    #endregion
}