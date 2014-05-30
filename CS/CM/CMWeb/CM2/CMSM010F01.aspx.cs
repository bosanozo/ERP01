﻿using System;
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
public partial class CM2_CMSM010F01 : CMBaseJqForm
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
        // Ajax
        if (Request.Params["_search"] != null || Request.Params["oper"] != null)
        {
            // ブラウザからのリクエストを実行
            DoRequest(m_facade);
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
}