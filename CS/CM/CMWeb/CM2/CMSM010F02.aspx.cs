using System;
using System.Collections.Generic;
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
public partial class CM2_CMSM010F02 : CMBaseJqForm
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
        string oper = Request.Params["oper"];

        // 検索の場合
        if (Request.QueryString["_search"] != null)
        {
            // 検索を実行
            DoSearch(m_facade);
        }
        // 編集操作の場合
        else if (oper != null)
        {
            // 検索結果を取得
            DataSet ds = (DataSet)Session[Request.Path + "_DataSet"];

            // 操作を実行
            DoOperation(m_facade, ds);
        }
        // ASP.Net
        else
        {
            // 画面ヘッダ初期化
            Master.Title = "組織マスタメンテ(jquery版)";

            // 初期表示以外は処理しない
            if (IsPostBack) return;

            // 操作履歴を出力
            WriteOperationLog();
        }
    }
    #endregion
}