/*******************************************************************************
 * 【メニュー】
 * 
 * 作成者: 日進テクノロジー／田中 望
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
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.WEB;

//************************************************************************
/// <summary>
/// メニュー
/// </summary>
//************************************************************************
public partial class Menu : CMBaseListForm
{
    #region イベントハンドラ
    //************************************************************************
    /// <summary>
    /// ページロード
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            // 以下は初期表示以外は処理しない
            if (IsPostBack) return;

            MultiView1.ActiveViewIndex = 0;

            // システム情報の検索
            DataTable table = CommonBL.Select("CMSTシステム情報");
            GridView1.DataSource = table;

            CMUserInfo uinfo = CMInformationManager.UserInfo;

            // メニューレベル１の検索
            DataTable table2 = CommonBL.Select("CMSMメニューレベル1", uinfo.SoshikiCd, uinfo.Id);

            foreach (DataRow row in table2.Rows)
            {
                MenuItem item = new MenuItem();
                item.Text = row["画面名"].ToString();
                item.Value = row["メニューID"].ToString();
                item.Enabled = row["許否フラグ"].ToString() == "True";
                Menu1.Items.Add(item);
            }

            DataBind();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    //************************************************************************
    /// <summary>
    /// タブクリック
    /// </summary>
    //************************************************************************
    protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
    {
        MultiView1.ActiveViewIndex = Menu1.Items.IndexOf(e.Item) > 0 ? 1 : 0;

        try
        {
            if (MultiView1.ActiveViewIndex == 0)
            {
            }
            else
            {
                Menu2.Items.Clear();

                CMUserInfo uinfo = CMInformationManager.UserInfo;

                // メニューレベル２の検索
                DataTable table = CommonBL.Select("CMSMメニューレベル2", uinfo.SoshikiCd, uinfo.Id, e.Item.Value);

                foreach (DataRow row in table.Rows)
                {
                    MenuItem item = new MenuItem();
                    item.Text = row["画面名"].ToString();
                    item.NavigateUrl = row["URL"].ToString();
                    item.Target = row["オプション"].ToString();
                    item.Enabled = row["許否フラグ"].ToString() != "False";
                    Menu2.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }
    #endregion
}