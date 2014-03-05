using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DocumentFormat.OpenXml;
using SpreadsheetLight;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.WEB;
using NEXS.ERP.CM.DA;
using NEXS.ERP.CM.BL;

//************************************************************************
/// <summary>
/// 組織マスタEXCEL入力
/// </summary>
//************************************************************************
public partial class CM_CMSM010F03 : CMBaseForm
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

        // 画面ヘッダ初期化
        Master.Title = "組織マスタEXCEL入力";

        // 初期表示以外は処理しない
        if (IsPostBack) return;

        // 操作履歴を出力
        WriteOperationLog();
    }

    //************************************************************************
    /// <summary>
    /// EXCEL入力ボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnExcelInput_Click(object sender, EventArgs e)
    {
        if (!FileUpload1.HasFile) return;

        try
        {
            // アップロードファイルからデータを取り込み
            DataSet ds = ImportExcel(FileUpload1.PostedFile.InputStream);

            // データセットを記憶
            Session["ImportDataSet"] = ds;

            // DataSource設定
            GridView1.DataSource = ds.Tables[0];
            // ページセット
            GridView1.PageIndex = 0;
            // バインド
            GridView1.DataBind();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }


    //************************************************************************
    /// <summary>
    /// 登録ボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnUpdate_Click(object sender, EventArgs e)
    {
        if (Session["ImportDataSet"] == null) return;

        // データセットを取得
        DataSet ds = (DataSet)Session["ImportDataSet"];
         
        try
        {
            // ファサードの呼び出し
            DateTime operationTime;
            m_facade.Upload(ds, out operationTime);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }
    #endregion
}