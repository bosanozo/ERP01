/*******************************************************************************
 * 【共通部品】
 * 
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// 一覧画面の基底クラス
    /// </summary>
    //************************************************************************
    public class CMBaseListForm : CMBaseForm
    {
        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMBaseListForm()
        {
        }
        #endregion

        #region protectedメソッド
        //************************************************************************
        /// <summary>
        /// 検索を実行する。
        /// </summary>
        /// <param name="argFacade">使用ファサード</param>
        /// <param name="argParam">検索条件パラメータ</param>
        /// <param name="argGrid">一覧表示用グリッド</param>
        /// <param name="argPage">ページ</param>
        /// <returns>True:エラーあり, False:エラーなし</returns>
        //************************************************************************
        protected bool DoSelect(ICMBaseBL argFacade, List<CMSelectParam> argParam, GridView argGrid, int argPage = 0)
        {
            try
            {
                // ファサードの呼び出し
                DateTime operationTime;
                CMMessage message;
                DataSet result = argFacade.Select(argParam, CMSelectType.List, out operationTime, out message);

                // 返却メッセージの表示
                if (message != null) ShowMessage(message);
                
                // DataSource設定
                argGrid.DataSource = result.Tables[0];
                // ページセット
                argGrid.PageIndex = argPage;
                // バインド
                argGrid.DataBind();
            }
            catch (Exception ex)
            {
                // DataSourceクリア
                argGrid.DataSource = null;
                argGrid.DataBind();

                ShowError(ex);

                return true;
            }

            return false;
        }

        //************************************************************************
        /// <summary>
        /// CSV出力を実行する。
        /// </summary>
        /// <param name="argFacade">使用ファサード</param>
        /// <param name="argParam">検索条件パラメータ</param>
        /// <param name="argUrl">参照URL</param>
        /// <returns>True:エラーあり, False:エラーなし</returns>
        //************************************************************************
        protected bool DoCsvOut(ICMBaseBL argFacade, List<CMSelectParam> argParam, out string argUrl)
        {
            argUrl = null;

            try
            {
                // ファサードの呼び出し
                DateTime operationTime;
                CMMessage message;
                DataSet result = argFacade.Select(argParam, CMSelectType.Csv, out operationTime, out message);

                // 返却メッセージの表示
                if (message != null) ShowMessage(message);

                DataTable table = result.Tables[0];

                bool found = table.Rows.Count > 0;
                // 検索結果ありの場合
                if (found)
                {
                    // CSVファイル名作成
                    //string fname = string.Format("{0}_{1}_{2}.csv",
                    string fname = string.Format("{0}_{1}_{2}.xlsx",
                        table.TableName, DateTime.Now.ToString("yyyyMMddHHmmss"), CMInformationManager.UserInfo.Id);
                    // CSVファイル出力
                    //ExportCsv(table, System.IO.Path.Combine(Request.PhysicalApplicationPath, "Csv", fname));
                    ExportExcel(result, System.IO.Path.Combine(Request.PhysicalApplicationPath, "Csv", fname));
                    // 画面表示
                    argUrl = Request.ApplicationPath + "/Csv/" + Uri.EscapeUriString(fname);
                }
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
        /// 機能ボタンにスクリプトを登録する。
        /// </summary>
        /// <param name="argButtonSelect">検索ボタン</param>
        /// <param name="argButtonCsvOut">CSV出力ボタン</param>
        //************************************************************************
        protected void AddFuncOnclick(Button argButtonSelect, Button argButtonCsvOut)
        {
            argButtonSelect.Attributes.Add("onclick","return CheckInputList()");
            argButtonCsvOut.Attributes.Add("onclick","ShowWaitMessage(); return CheckInputList()");
        }

        //************************************************************************
        /// <summary>
        /// 機能ボタンにスクリプトを登録する。
        /// </summary>
        /// <param name="argButtonSelect">検索ボタン</param>
        /// <param name="argButtonCsvOut">CSV出力ボタン</param>
        /// <param name="argButtonInsert">新規ボタン</param>
        /// <param name="argButtonUpdate">修正ボタン</param>
        /// <param name="argButtonDelete">削除ボタン</param>
        //************************************************************************
        protected void AddFuncOnclick(Button argButtonSelect, Button argButtonCsvOut,
            Button argButtonInsert, Button argButtonUpdate, Button argButtonDelete)
        {
            AddFuncOnclick(argButtonSelect, argButtonCsvOut);
            argButtonInsert.Attributes.Add("onclick","return OpenEntryForm('Insert')");
            argButtonUpdate.Attributes.Add("onclick","return OpenEntryForm('Update')");
            argButtonDelete.Attributes.Add("onclick","return OpenEntryForm('Delete')");
        }

        //************************************************************************
        /// <summary>
        /// 検索パラメータを追加する。
        /// </summary>
        /// <param name="param">検索パラメータ</param>
        /// <param name="wc">WebControl</param>
        /// <param name="argPanel">検索条件パネル</param>
        //************************************************************************
        private void AddSelectParam(List<CMSelectParam> param, WebControl wc, Panel argPanel)
        {
            // テキストとドロップダウンが対象
            if (!(wc is DropDownList) && !(wc is TextBox))
            {
                foreach (Control c in wc.Controls)
                {
                    if (c is WebControl) AddSelectParam(param, (WebControl)c, argPanel);
                }

                return;
            }

            // Toは無視
            if (wc.ID.EndsWith("To")) return;

            // Fromの場合
            if (wc.ID.EndsWith("From"))
            {
                // Fromなし名称取得
                string colName = wc.ID.Substring(0, wc.ID.IndexOf("From"));

                WebControl toCnt = (WebControl)argPanel.FindControl(colName + "To");
                bool isSetFrom = IsSetValue(wc);
                bool isSetTo = IsSetValue(toCnt);

                // FromTo
                if (isSetFrom && isSetTo)
                {
                    param.Add(new CMSelectParam(colName.Substring(3),
                        string.Format("BETWEEN @{0} AND @{1}", wc.ID, toCnt.ID),
                        GetValue(wc), GetValue(toCnt)));
                }
                // From or To
                else if (isSetFrom || isSetTo)
                {
                    string op = isSetFrom ? ">= @" : "<= @";
                    WebControl condCnt = isSetFrom ? wc : toCnt;

                    param.Add(new CMSelectParam(colName.Substring(3), op + condCnt.ID, GetValue(condCnt)));
                }
            }
            // 単一項目の場合
            else
            {
                // 設定ありの場合
                if (IsSetValue(wc))
                {
                    string op = "= @";
                    object value = GetValue(wc);

                    // LIKE検索の場合
                    if (wc is TextBox && wc.ID.EndsWith("名"))
                    {
                        op = "LIKE @";
                        value = "%" + value + "%";
                    }

                    param.Add(new CMSelectParam(wc.ID.Substring(3), op + wc.ID, value));
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// 検索パラメータを作成する。
        /// </summary>
        /// <param name="argPanel">検索条件パネル</param>
        /// <returns>検索パラメータ</returns>
        //************************************************************************
        protected List<CMSelectParam> CreateSelectParam(Panel argPanel)
        {
            List<CMSelectParam> param = new List<CMSelectParam>();

            foreach (Control c in argPanel.Controls)
            {
                if (c is WebControl) AddSelectParam(param, (WebControl)c, argPanel);
            }

            return param;
        }
        #endregion
    }
}
