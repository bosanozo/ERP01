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
public partial class CM_CMSM010F02 : CMBaseEntryForm
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

        // 共通設定
        Master.Body.Attributes.Remove("onload");

        // 操作モードを設定
        string subName = SetOpeMode(PanelKeyItems, PanelSubItems,
            PanelUpdateInfo, PanelFunction, null, BtnCommit, BtnCancel);

        // 画面ヘッダ初期化
        Master.Title = "組織マスタメンテ　" + subName;

        // 以下は初期表示以外は処理しない
        if (IsPostBack) return;

        try
        {
            // 区分値の検索
            DataTable kbnTable = CommonBL.SelectKbn("M001");

            // 区分値の設定
            DataView dv = new DataView(kbnTable, "分類CD = 'M001'", null, DataViewRowState.CurrentRows);

            // 組織階層区分のアイテム設定
            DataTable table = dv.ToTable();
            組織階層区分.DataSource = table;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        // 標準の画面表示処理実行
        OnPageOnLoad(Master.Body, m_facade);
    }

    //************************************************************************
    /// <summary>
    /// 登録ボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnCommit_Click(object sender, EventArgs e)
    {
        // 標準の登録ボタン押下時処理実行
        if (Page.IsValid) OnCommitClick(Master.Body, m_facade);
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
        param.Add(new CMSelectParam("組織CD", "= @組織CD", argKey));
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
        argRow["組織階層区分"] = CMInformationManager.UserInfo.SoshikiKaisoKbn;
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
}