using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.WEB;
using NEXS.ERP.CM.BL;

//************************************************************************
/// <summary>
/// 組織マスタメンテ
/// </summary>
//************************************************************************
public partial class CM_CMSM010F01 : CMBaseListForm
{
    #region BLインジェクション用フィールド
    protected CMSM010BL m_facade;
    #endregion

    #region イベントハンドラ
    //************************************************************************
    /// <summary>
    /// ページロード
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        // 組織ＣＤ、組織階層区分を設定
        KaishaCd.Value = CMInformationManager.UserInfo.SoshikiCd;
        SoshikiLayer.Value = CMInformationManager.UserInfo.SoshikiKaisoKbn;

        // 機能ボタン スクリプト登録
        AddFuncOnclick(BtnSelect, BtnCsvOut, BtnInsert, BtnUpdate, BtnDelete);
        // 画面ヘッダ初期化
        Master.Title = "組織マスタメンテ";

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

        try
        {
            // 区分値の検索
            DataTable kbnTable = CommonBL.SelectKbn("M001");

            // 区分値の設定
            DataView dv = new DataView(kbnTable, "分類CD = 'M001'", null, DataViewRowState.CurrentRows);

            // 組織階層区分のアイテム設定
            DataTable table = dv.ToTable();
            table.Rows.InsertAt(table.NewRow(), 0);
            組織階層区分.DataSource = table;
            組織階層区分.DataBind();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        // 操作履歴を出力
        WriteOperationLog();
    }

    //************************************************************************
    /// <summary>
    /// 検索、CSV出力ボタン押下
    /// </summary>
    //************************************************************************
    protected void Select_Command(object sender, CommandEventArgs e)
    {
        // 検索パラメータ取得
        List<CMSelectParam> param = CreateSelectParam(PanelCondition);

        bool hasError = false;

        // 検索
        if (e.CommandName == "Select")
        {
            hasError = DoSelect(m_facade, param, GridView1);

            // 正常終了の場合
            if (!hasError)
            {
                // 検索実行を記憶
                Selected.Value = "true";
                // 検索条件を記憶
                Session["SelectCondition"] = param;
            }
        }
        // CSV出力
        else if (e.CommandName == "CsvOut")
        {
            string url;
            hasError = DoCsvOut(m_facade, param, out url);
            // 結果表示
            if (!hasError) OpenExcel(url);
        }
    }

    //************************************************************************
    /// <summary>
    /// 新規、修正、削除ボタン押下
    /// </summary>
    //************************************************************************
    protected void OpenEntryForm_Command(object sender, CommandEventArgs e)
    {
        // 検索が実行されている場合、再検索
        if (Selected.Value.Length > 0)
        {
            // 検索条件取得
            List<CMSelectParam> param = (List<CMSelectParam>)Session["SelectCondition"];
            // 検索実行
            DoSelect(m_facade, param, GridView1, GridView1.PageIndex);
        }

        // メッセージを設定
        if (e.CommandName == "Delete")
            Master.ShowMessage("I", CMMessageManager.GetMessage("IV004"));
        else Master.Body.Attributes.Remove("onload");
    }

    //************************************************************************
    /// <summary>
    /// データバインド
    /// </summary>
    //************************************************************************
    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowIndex >= 0)
        {
            DataRowView row = (DataRowView)e.Row.DataItem;
        }
    }

    //************************************************************************
    /// <summary>
    /// ページ切り替え
    /// </summary>
    //************************************************************************
    protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        // 検索条件取得
        List<CMSelectParam> param = (List<CMSelectParam>)Session["SelectCondition"];
        // 検索実行
        DoSelect(m_facade, param, GridView1, e.NewPageIndex);
    }
    #endregion
}