/*******************************************************************************
 * 【共通部品】
 * 
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using log4net;
using Seasar.Quill;

using DocumentFormat.OpenXml;
using SpreadsheetLight;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// 画面の基底クラス
    /// </summary>
    //************************************************************************
    public class CMBaseForm : Page
    {
        #region ロガーフィールド
        private ILog m_logger;
        #endregion

        #region インジェクション用フィールド
        protected ICMCommonBL m_commonBL;
        #endregion

        #region プロパティ
        /// <summary>
        /// ロガー
        /// </summary>
        protected ILog Log
        {
            get { return m_logger; }
        }

        /// <summary>
        /// 共通処理ファサード
        /// </summary>
        protected ICMCommonBL CommonBL
        {
            get { return m_commonBL; }
        }        
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMBaseForm()
        {
            // ロガーを取得
            m_logger = LogManager.GetLogger(this.GetType());

            // インジェクション実行
            QuillInjector injector = QuillInjector.GetInstance();
            injector.Inject(this);
        }
        #endregion

        #region protectedメソッド
        #region メッセージ関連
        //************************************************************************
        /// <summary>
        /// メッセージをクリアする。
        /// </summary>
        //************************************************************************
        /*
        protected void ClearMessage()
        {
            ShowMessage("I", "");
        }*/

        //************************************************************************
        /// <summary>
        /// 発生した例外をメッセージ表示する。
        /// </summary>
        /// <param name="argException">発生した例外</param>
        //************************************************************************
        protected void ShowError(Exception argException)
        {
            // CMExceptionの場合
            if (argException is CMException)
            {
                CMException ex = (CMException)argException;

                // ログの出力
                if (ex.CMMessage != null && ex.CMMessage.MessageCd != null &&
                    ex.CMMessage.MessageCd.Length > 0)
                {
                    if (ex.CMMessage.MessageCd[0] == 'E')
                        Log.Error(ex.CMMessage.ToString(), argException);
                }

                // メッセージ表示
                ShowMessage(ex.CMMessage);
            }
            // その他の場合
            else
            {
                string msgCd = "EV001";

                if (argException is FileNotFoundException)
                    msgCd = "W";
                else if (argException is IOException)
                    msgCd = "EV003";

                // ログの出力
                if (msgCd[0] == 'E')
                    Log.Error(argException.Message, argException);

                // メッセージ表示
                ShowMessage(msgCd, argException.Message);
            }
        }

        //************************************************************************
        /// <summary>
        /// 指定されたメッセージコードのメッセージを表示する。
        /// </summary>
        /// <param name="argMessage">メッセージ</param>
        /// <returns>ダイアログリザルト</returns>
        //************************************************************************
        protected void ShowMessage(CMMessage argMessage)
        {
            ShowMessage(argMessage.MessageCd, argMessage.Params);
        }

        //************************************************************************
        /// <summary>
        /// 指定されたメッセージコードのメッセージを表示する。
        /// </summary>
        /// <param name="argCode">メッセージコード</param>
        /// <param name="argParams">パラメータ</param>
        //************************************************************************
        protected void ShowMessage(string argCode, params object[] argParams)
        {
            if ((dynamic)Master != null)
                ((dynamic)Master).ShowMessage(argCode, CMMessageManager.GetMessage(argCode, argParams));
        }
        #endregion

        #region UI操作関連
        //************************************************************************
        /// <summary>
        /// 画面を閉じる
        /// </summary>
        /// <param name="argStatus">成功:True, 失敗:False</param>
        //************************************************************************
        protected void Close(bool argStatus)
        {
            string script =
                "<script language=JavaScript>" +
                "window.onLoad = window.returnValue = {0};" +
                "window.close()<" +
                "/" + "script>";

            // スクリプト登録
            ClientScript.RegisterClientScriptBlock(GetType(),
                "Close", string.Format(script, argStatus.ToString().ToLower()));
        }

        //************************************************************************
        /// <summary>
        /// パネルを読み取り専用にする。
        /// </summary>
        /// <param name="argPanel">読み取り専用にするパネル</param>
        //************************************************************************
        protected void ProtectPanel(Panel argPanel)
        {
            foreach (Control c in argPanel.Controls)
            {
                if (c is TextBox)
                {
                    TextBox t = (TextBox)c;
                    ProtectTextBox(t);
                }
                else if (c is DropDownList)
                {
                    DropDownList d = (DropDownList)c;
                    d.Enabled = false;
                    d.BackColor = Color.FromName("#CCCCFF");
                    //if (d.Visible) d.Visible = false;
                }
                else if (c is HtmlInputButton)
                {
                    HtmlInputButton b = (HtmlInputButton)c;
                    b.Disabled = true;
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// テキストボックスを読み取り専用にする。
        /// </summary>
        /// <param name="argTextBox">読み取り専用にするテキストボックス</param>
        //************************************************************************
        protected void ProtectTextBox(TextBox argTextBox)
        {
            if (argTextBox.ReadOnly) return;

            //argTextBox.BorderStyle = BorderStyle.None;
            argTextBox.BackColor = Color.FromName("#CCCCFF"); //Color.Transparent;
            argTextBox.ReadOnly = true;
            argTextBox.TabIndex = -1;
        }

        //************************************************************************
        /// <summary>
        /// コントロールに値が設定されているか返す。
        /// </summary>
        /// <param name="arg">コントロール</param>
        /// <returns>True:設定あり, False:設定なし</returns>
        //************************************************************************
        protected bool IsSetValue(WebControl arg)
        {
            return arg is TextBox && ((TextBox)arg).Text.Trim().Length > 0 ||
                arg is DropDownList && ((DropDownList)arg).SelectedValue.Trim().Length > 0;
        }

        //************************************************************************
        /// <summary>
        /// コントロールに入力された文字列を返す。
        /// </summary>
        /// <param name="argControl">コントロール</param>
        /// <returns>入力された文字列</returns>
        //************************************************************************
        protected virtual object GetValue(WebControl argControl)
        {
            if (argControl is TextBox)
            {
                if (argControl.CssClass == "DateInput")
                    return Convert.ToDateTime(((TextBox)argControl).Text);
                else
                    return ((TextBox)argControl).Text;
            }

            if (argControl is DropDownList)
                return ((DropDownList)argControl).SelectedValue;

            return "";
        }
        #endregion

        #region 共通ファサード呼び出し
        //************************************************************************
        /// <summary>
        /// 操作履歴を出力する。
        /// </summary>
        /// <returns>True:エラーあり、False:エラーなし</returns>
        //************************************************************************
        protected bool WriteOperationLog()
        {
            try
            {
                // 操作ログ記録
                CommonBL.WriteOperationLog(((dynamic)Master).Title);
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return true;
            }

            return false;
        }

        //************************************************************************
        /// <summary>
        /// コード値から名称を取得する。
        /// </summary>
        /// <param name="argSelectId">検索種別</param>
        /// <param name="argNotFound">True:名称取得失敗, False:名称取得成功</param>
        /// <param name="argTextBox">名称表示テキストボックス</param>
        /// <param name="argParams">パラメータ</param>
        /// <returns>名称</returns>
        //************************************************************************
        protected string GetCodeName(string argSelectId, out bool argNotFound,
            TextBox argTextBox, params object[] argParams)
        {
            string name = null;
            argNotFound = true;

            try
            {
                // 検索
                DataTable result = CommonBL.Select(argSelectId, argParams);

                argNotFound = result == null || result.Rows.Count == 0;

                name = argNotFound ? "コードエラー" : result.Rows[0][0].ToString();
                string cssClass = argNotFound ? "transp warning" : "1 transp";

                // ラベルの設定
                argTextBox.Text = name;
                argTextBox.CssClass = cssClass;
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }

            return name;
        }

        //************************************************************************
        /// <summary>
        /// ドロップダウンリストにアイテムを設定する。
        /// </summary>
        /// <param name="argSelectId">検索種別</param>
        /// <param name="argDDList">ドロップダウンリスト</param>
        /// <param name="argParams">パラメータ</param>
        //************************************************************************
        protected void SetDropDownItems(string argSelectId, DropDownList argDDList,
            params object[] argParams)
        {
            try
            {
                // 検索
                DataTable result = CommonBL.Select(argSelectId, argParams);

                // 検索結果を設定
                argDDList.DataSource = result;
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
        #endregion

        #region ドロップダウンリスト関連
        //************************************************************************
        /// <summary>
        /// ドロップダウンリストにアイテムを設定する。
        /// ドロップダウンリストの最初に指定なしを挿入する。
        /// </summary>
        /// <param name="argSelectId">検索種別</param>
        /// <param name="argDDList">ドロップダウンリスト</param>
        /// <param name="argParams">パラメータ</param>
        //************************************************************************
        protected void SetDropDownItemsList(string argSelectId, DropDownList argDDList,
            params object[] argParams)
        {
            SetDropDownItems(argSelectId, argDDList, argParams);
            InsertTopItem(argDDList, "指定なし");
        }

        //************************************************************************
        /// <summary>
        /// ドロップダウンリストにアイテムを設定する。
        /// ドロップダウンリストの最初に指定なしを挿入する。
        /// </summary>
        /// <param name="argSelectId">検索種別</param>
        /// <param name="argDDList">ドロップダウンリスト</param>
        /// <param name="argParams">パラメータ</param>
        //************************************************************************
        protected void SetDropDownItemsEntry(string argSelectId, DropDownList argDDList,
            params object[] argParams)
        {
            SetDropDownItems(argSelectId, argDDList, argParams);
            InsertTopItem(argDDList, "");
        }

        //************************************************************************
        /// <summary>
        /// ドロップダウンリストの最初にアイテムを挿入する。
        /// </summary>
        /// <param name="argDDList">ドロップダウンリスト</param>
        /// <param name="argTopText">アイテム表示名</param>
        //************************************************************************
        protected void InsertTopItem(DropDownList argDDList, string argTopText)
        {
            DataTable table = (DataTable)argDDList.DataSource;
            DataRow row = table.NewRow();
            row[argDDList.DataTextField] = argTopText;
            table.Rows.InsertAt(row, 0);
        }

        //************************************************************************
        /// <summary>
        /// ドロップダウンリストに時間アイテムを設定する。
        /// </summary>
        /// <param name="argDDList">ドロップダウンリスト</param>
        //************************************************************************
        protected void SetHourItems(DropDownList argDDList)
        {
            argDDList.Items.Add("");
            for (int i = 0; i < 24; i++) argDDList.Items.Add(i.ToString("00"));
        }

        //************************************************************************
        /// <summary>
        /// ドロップダウンリストに分アイテムを設定する。
        /// </summary>
        /// <param name="argDDList">ドロップダウンリスト</param>
        //************************************************************************
        protected void SetMinuteItems(DropDownList argDDList)
        {
            argDDList.Items.Add("");
            for (int i = 0; i < 12; i++) argDDList.Items.Add((i * 5).ToString("00"));
        }

        //************************************************************************
        /// <summary>
        /// ドロップダウンリストのアイテムの名称を取得する。
        /// </summary>
        /// <param name="argDDList">ドロップダウンリスト</param>
        /// <returns>名称</returns>
        //************************************************************************
        protected string GetItemName(DropDownList argDDList)
        {
            string text = argDDList.SelectedItem.Text;
            int idx = text.IndexOf(' ');
            return idx >= 0 ? text.Substring(idx) : "";
        }
        #endregion

        #region EXCEL入力
        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからデータテーブルを作成する。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>データテーブル</returns>
        //************************************************************************
        protected DataTable CreateDataTableFromXml(string argName)
        {
            // データセットにファイルを読み込み
            CM項目DataSet ds = new CM項目DataSet();
            ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argName + ".xml"));

            // データテーブル作成
            DataTable table = new DataTable(ds.項目一覧[0].項目一覧ID);

            // DataColumn追加
            foreach (var row in ds.項目)
            {
                // DataColumn作成
                DataColumn dcol = new DataColumn(row.項目名);
                // 型
                CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), row.項目型);
                switch (dbType)
                {
                    case CMDbType.フラグ:
                        dcol.DataType = typeof(bool);
                        break;
                    case CMDbType.金額:
                    case CMDbType.数値:
                        dcol.DataType = row.小数桁 > 0 ? typeof(decimal) : typeof(long);
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

        //******************************************************************************
        /// <summary>
        /// 指定StreamからDataSetにデータを取り込む。
        /// </summary>
        /// <param name="argInputStream">入力Stream</param>
        /// <returns>データを取り込んだDataSet</returns>
        /// <remarks>データを取り込むDataTableのスキーマはエンティティ定義XMLファイルより生成する。
        /// シート名がXMLファイル名になる。</remarks>
        //******************************************************************************
        protected DataSet ImportExcel(Stream argInputStream)
        {
            // EXCEL文書を作成
            SLDocument xslDoc = new SLDocument(argInputStream);

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

            return ds;
        }
        #endregion

        #region EXCEL出力
        //******************************************************************************
        /// <summary>
        /// 指定されたデータセットの内容をEXCELファイルに出力する。
        /// </summary>
        /// <param name="argDataSet">データセット</param>
        /// <param name="argPath">ファイル出力フルパス</param>
        /// <returns>true:出力した, false:キャンセルした</returns>
        /// <remarks>テンプレートファイルがある場合は、テンプレートファイルの書式に従って
        /// データを出力する。出力開始位置はセルに"開始"と記述することで指定可能(無くても可)。
        /// データを出力するシートはデータテーブル名とシート名が一致するものを使用する。
        /// テンプレートファイルが無い場合は、デフォルトの形式でデータを出力する。</remarks>
        //******************************************************************************
        protected bool ExportExcel(DataSet argDataSet, string argPath)
        {
            SLDocument xslDoc = CreateExcel(argDataSet);

            // ブックを保存
            xslDoc.SaveAs(argPath);

            return true;
        }

        protected SLDocument CreateExcel(DataSet argDataSet)
        {
            SLDocument xslDoc;

            // テンプレートファイル名作成
            string template = Path.Combine(Request.PhysicalApplicationPath,
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

        #region CSV出力
        //************************************************************************
        /// <summary>
        /// DataTableに保持されているデータをCSVファイルに出力する。
        /// </summary>
        /// <param name="argTable">出力するデータが保持されているDataTable</param>
        /// <param name="argPath">ファイル出力フルパス</param>
        /// <param name="argAppend">true:追加書き込み, false:新規作成</param>
        /// <param name="argOutputHeader">ヘッダ出力フラグ</param>
        /// <param name="argDuplicateNull">重複データNULLフラグ：trueの場合はNULL値を前の行のデータで復元する。</param>
        //************************************************************************
        protected void ExportCsv(DataTable argTable, string argPath, bool argAppend = false,
            bool argOutputHeader = true, bool argDuplicateNull = false)
        {
            // データなしは処理しない
            if (argTable.Rows.Count == 0) return;

            StringBuilder builder = new StringBuilder();
            DataColumnCollection colmuns = argTable.Columns;

            // ファイル出力
            using (StreamWriter writer = new StreamWriter(argPath, argAppend, Encoding.Default))
            {
                // 新規作成かつヘッダ出力ありの場合、ヘッダを出力する
                if (!argAppend && argOutputHeader)
                {
                    // 1列目
                    builder.Append(colmuns[0].Caption);
                    // 列毎のループ
                    for (int i = 1; i < colmuns.Count; i++)
                        builder.Append(',').Append(colmuns[i].Caption);

                    // 一行分出力
                    writer.WriteLine(builder);
                    // クリア
                    builder.Length = 0;
                }

                // 重複データ復元の指定ありの場合
                if (argDuplicateNull)
                {
                    // 重複データ記憶用
                    DataRow preRow = argTable.NewRow();

                    // 行毎のループ
                    foreach (DataRow row in argTable.Rows)
                    {
                        // 1列目
                        if (row[0] != DBNull.Value) preRow[0] = row[0];
                        builder.Append(preRow[0].ToString());

                        // 列毎のループ
                        for (int i = 1; i < colmuns.Count; i++)
                        {
                            // 重複していなければ値を記憶
                            if (row[i] != DBNull.Value) preRow[i] = row[i];
                            builder.Append(',').Append(preRow[i].ToString());
                        }

                        // 一行分出力
                        writer.WriteLine(builder);
                        // クリア
                        builder.Length = 0;
                    }
                }
                // 重複データ復元の指定なしの場合
                else
                {
                    // 行毎のループ
                    foreach (DataRow row in argTable.Rows)
                    {
                        // 1列目
                        builder.Append(row[0].ToString());
                        // 列毎のループ
                        for (int i = 1; i < colmuns.Count; i++)
                            builder.Append(',').Append(row[i].ToString());

                        // 一行分出力
                        writer.WriteLine(builder);
                        // クリア
                        builder.Length = 0;
                    }
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// Excelファイルを別のWindowsに開くメソッド
        /// </summary>
        /// <param name="argUrl">ExcelファイルのURL</param>
        //************************************************************************
        protected void OpenExcel(string argUrl)
        {
            // 返すjavascript文作成
            StringBuilder sb = new StringBuilder();
            sb.Append("<script language='javascript'>");
            sb.Append("function openExcel(){");
            // 窓口のサイズを取得
            sb.Append("var xMax = screen.Width, yMax = screen.Height;");
            sb.Append("var xOffset = (xMax - 800)/2, yOffset = (yMax - 600)/4;");
            sb.Append("window.open('").Append(argUrl);
            // 出力窓口の状態を設定
            sb.Append("',null,'menubar=yes,toolbar=no,location=no,width=800,height=600,screenX='+xOffset+',screenY='+yOffset+',top='+yOffset+',left='+xOffset+',resizable=yes,status=yes,scrollbars=yes,center=yes');}");
            sb.Append("window.onLoad=openExcel();");
            sb.Append("</script>");

            if (!ClientScript.IsClientScriptBlockRegistered("openExcelScript"))
                ClientScript.RegisterClientScriptBlock(GetType(), "openExcelScript", sb.ToString());
        }
        #endregion
        #endregion
    }
}
