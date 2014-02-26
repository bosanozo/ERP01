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
            // EXCEL文書を作成
            SLDocument xslDoc = new SLDocument(FileUpload1.PostedFile.InputStream);

            // データセットにデータを取り込む
            DataSet ds = new DataSet();

            // シートでループ
            foreach (string sheet in xslDoc.GetSheetNames())
            {
                // シートを選択
                xslDoc.SelectWorksheet(sheet);

                // データテーブル作成
                DataTable table = CreateDataTableFromXml(sheet);

                var sheetStat = xslDoc.GetWorksheetStatistics();

                // １行ずつ読み込み、先頭行はタイトルとして読み飛ばす
                for (int rowIdx = sheetStat.StartRowIndex + 1; rowIdx <= sheetStat.EndRowIndex; rowIdx++)
                {
                    DataRow newRow = table.NewRow();
                    for (int colIdx = 0; colIdx < table.Columns.Count; colIdx++)
                    {
                        int col = colIdx + sheetStat.StartColumnIndex;

                        // 型に応じて値を取得する
                        switch (table.Columns[colIdx].DataType.Name)
                        {
                            case "bool":
                                newRow[colIdx] = xslDoc.GetCellValueAsBoolean(rowIdx, col);
                                break;

                            case "decimal":
                                newRow[colIdx] = xslDoc.GetCellValueAsDecimal(rowIdx, col);
                                break;

                            case "long":
                                newRow[colIdx] = xslDoc.GetCellValueAsInt64(rowIdx, col);
                                break;

                            case "DateTime":
                                newRow[colIdx] = xslDoc.GetCellValueAsDateTime(rowIdx, col);
                                break;

                            default:
                                newRow[colIdx] = xslDoc.GetCellValueAsString(rowIdx, col);
                                break;
                        }
                    }
                    table.Rows.Add(newRow);
                }

                // データテーブルを追加
                ds.Tables.Add(table);
            }

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
    /// 指定のXmlファイルからデータテーブルを作成する。
    /// </summary>
    /// <param name="argName">Xmlファイル名</param>
    /// <returns>データテーブル</returns>
    //************************************************************************
    private DataTable CreateDataTableFromXml(string argName)
    {
        // データセットにファイルを読み込み
        CMEntityDataSet ds = new CMEntityDataSet();
        ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argName + ".xml"));

        // データテーブル作成
        DataTable table = new DataTable(ds.エンティティ[0].テーブル名);

        // DataColumn追加
        foreach (var row in ds.項目)
        {
            string col = string.IsNullOrEmpty(row.SourceColumn) ?
                row.項目名 : row.SourceColumn;

            // DataColumn作成
            DataColumn dcol = new DataColumn(col);
            // 型
            CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), row.項目型);
            switch (dbType)
            {
                case CMDbType.フラグ:
                    dcol.DataType = typeof(bool);
                    break;
                case CMDbType.金額:
                case CMDbType.小数:
                    dcol.DataType = typeof(decimal);
                    break;
                case CMDbType.整数:
                    dcol.DataType = typeof(long);
                    break;
                case CMDbType.日付:
                case CMDbType.日時:
                    dcol.DataType = typeof(DateTime);
                    break;
            }
            // 必須入力
            if (row.必須) dcol.AllowDBNull = false;

            table.Columns.Add(dcol);
        }

        return table;
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