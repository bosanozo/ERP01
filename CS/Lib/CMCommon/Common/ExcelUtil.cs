using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DocumentFormat.OpenXml;
using SpreadsheetLight;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// Excel ユーティリティクラス
    /// </summary>
    //************************************************************************
    public static class ExcelUtil
    {
        #region EXCEL出力
        //******************************************************************************
        /// <summary>
        /// 指定されたデータセットの内容をEXCELファイルに出力する。
        /// </summary>
        /// <param name="argDataSet">データセット</param>
        /// <returns>SLDocument</returns>
        /// <remarks>テンプレートファイルがある場合は、テンプレートファイルの書式に従って
        /// データを出力する。出力開始位置はセルに"開始"と記述することで指定可能(無くても可)。
        /// データを出力するシートはデータテーブル名とシート名が一致するものを使用する。
        /// テンプレートファイルが無い場合は、デフォルトの形式でデータを出力する。</remarks>
        //******************************************************************************
        public static SLDocument CreateExcel(DataSet argDataSet)
        {
            SLDocument xslDoc;

            // テンプレートファイル名作成
            string template = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Template", argDataSet.Tables[0].TableName + ".xlsx");

            if (File.Exists(template))
            {
                // テンプレートを読み込み
                xslDoc = new SLDocument(template);

                foreach (string sheet in xslDoc.GetSheetNames())
                {
                    DataTable table = argDataSet.Tables[sheet];
                    if (table == null) continue;

                    // シートを選択
                    xslDoc.SelectWorksheet(sheet);

                    var sheetStat = xslDoc.GetWorksheetStatistics();
                    int startRow = sheetStat.StartRowIndex + 1;
                    int startCol = sheetStat.StartColumnIndex;

                    // 開始位置を検索
                    var cells = xslDoc.GetCells().Where(c => c.Value.DataType ==
                        DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString);
                    foreach (var cell in cells)
                    {
                        if (xslDoc.GetCellValueAsRstType(cell.Key.RowIndex, cell.Key.ColumnIndex).GetText() == "開始")
                        {
                            startRow = cell.Key.RowIndex;
                            startCol = cell.Key.ColumnIndex;
                            break;
                        }
                    }

                    // スタイルを設定
                    for (int i = 0; i < table.Columns.Count; i++)
                        xslDoc.SetCellStyle(startRow + 1, i + startCol,
                            table.Rows.Count + startRow - 1, i + startCol,
                            xslDoc.GetCellStyle(startRow, i + startCol));

                    // データの出力
                    xslDoc.ImportDataTable(startRow, startCol, table, false);
                }
            }
            else
            {
                // Bookを作成
                xslDoc = new SLDocument();

                foreach (DataTable table in argDataSet.Tables)
                {
                    // シートを追加
                    if (xslDoc.GetCurrentWorksheetName() == SLDocument.DefaultFirstSheetName)
                        xslDoc.RenameWorksheet(SLDocument.DefaultFirstSheetName, table.TableName);
                    else
                        xslDoc.AddWorksheet(table.TableName);

                    // スタイルを設定
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        // DateTimeの場合
                        if (table.Columns[i].DataType == typeof(DateTime))
                        {
                            xslDoc.SetColumnWidth(i + 1, 11);
                            var style = xslDoc.CreateStyle();
                            style.FormatCode = "yyyy/m/d";

                            xslDoc.SetCellStyle(2, i + 1,
                                table.Rows.Count + 2, i + 1, style);
                        }
                    }

                    // データの出力
                    xslDoc.ImportDataTable(1, 1, table, true);
                }
            }

            // ブックを返却
            return xslDoc;
        }
        #endregion
    }
}
