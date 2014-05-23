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
public partial class CM2_XM010F01 : CMBaseJqForm
{
    protected const string FORM_XML = "XMFN項目一覧";

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
        // Ajax
        if (Request.Params["_search"] != null || Request.Params["oper"] != null)
        {
            // ブラウザからのリクエストを実行
            DoRequest(m_facade);
        }
        // ASP.Net
        else
        {
            // 機能ボタン スクリプト登録
            //AddFuncOnclick(BtnSelect, BtnCsvOut, BtnInsert, BtnUpdate, BtnDelete);
            // 画面ヘッダ初期化
            Master.Title = "項目定義";

            // 初期表示以外は処理しない
            if (IsPostBack) return;

            // 操作履歴を出力
            WriteOperationLog();
        }
    }
    #endregion
}