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
    /// 登録画面の基底クラス
    /// </summary>
    //************************************************************************
    public class CMBaseEntryForm : CMBaseForm
    {
        #region プロパティ
        /// <summary>
        /// 操作モード
        /// </summary>
        public string OpeMode { get; set; }

        /// <summary>
        /// 入力データを保持するDataRow
        /// </summary>
        public DataRow InputRow { get; set; }
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMBaseEntryForm()
        {
        }
        #endregion

        #region protectedメソッド
        //************************************************************************
        /// <summary>
        /// 操作モードを設定し、操作モードに応じ画面の状態を変更する。
        /// </summary>
        /// <param name="argPanelKeyItems">キー項目パネル</param>
        /// <param name="argPanelSubItems">従属項目パネル</param>
        /// <param name="argPanelUpdateInfo">更新情報パネル</param>
        /// <param name="argPanelFunction">機能ボタンパネル</param>
        /// <param name="argButtonClose">閉じるボタン</param>
        /// <param name="argButtonConfirm">確認ボタン</param>
        /// <param name="argButtonCancel">キャンセルボタン</param>
        /// <returns>サブ画面名</returns>
        //************************************************************************
        protected string SetOpeMode(Panel argPanelKeyItems, Panel argPanelSubItems,
            Panel argPanelUpdateInfo, Panel argPanelFunction,
            HtmlInputButton argButtonClose, Button argButtonConfirm, Button argButtonCancel)
        {
            // 操作モードを取得
            OpeMode = Request.Params["mode"];

            string subName;
            //argButtonClose.Visible = false;

            // 操作モードに応じた設定
            switch (OpeMode)
            {
                case "Insert":
                    subName = "新規";
                    argPanelUpdateInfo.Visible = false;
                    argButtonConfirm.Attributes.Add("onclick",
                        string.Format("return confirm('{0}') && CheckInputEntry('{1}')",
                            CMMessageManager.GetMessage("QV001"), OpeMode));
                    break;
                case "Update":
                    subName = "修正";
                    ProtectPanel(argPanelKeyItems);
                    argButtonConfirm.Attributes.Add("onclick",
                        string.Format("return confirm('{0}') && CheckInputEntry('{1}')",
                            CMMessageManager.GetMessage("QV001"), OpeMode));
                    break;
                case "Delete":
                    subName = "削除確認";
                    ProtectPanel(argPanelKeyItems);
                    ProtectPanel(argPanelSubItems);
                    argButtonConfirm.Text = "削除実行";
                    argButtonCancel.Text = "キャンセル";
                    argButtonConfirm.Attributes.Add("onclick",
                        string.Format("return confirm('{0}')", CMMessageManager.GetMessage("QV002")));
                    break;
                default:
                    subName = "参照";
                    //argButtonClose.Visible = true;
                    ProtectPanel(argPanelKeyItems);
                    ProtectPanel(argPanelSubItems);
                    argButtonConfirm.Enabled = false;
                    break;
            }

            return subName;
        }

        //************************************************************************
        /// <summary>
        /// 画面表示処理
        /// </summary>
        /// <param name="argBody">bodyタグ</param>
        /// <param name="argFacade">使用ファサード</param>
        //************************************************************************
        protected void OnPageOnLoad(HtmlGenericControl argBody,
            ICMBaseBL argFacade)
        {
            // キーを取得
            string paramKey = Request.Params["keys"];

            // 初期表示の場合
            if (paramKey != null)
            {
                // キャンセルボタンの戻り値を初期化
                Session["cancelRet"] = false;

                // パラメータ作成
                List<CMSelectParam> param = CreateSelectParam(paramKey);

                try
                {
                    // ファサードの呼び出し
                    DateTime operationTime;
                    CMMessage message;
                    DataSet result = argFacade.Select(param, CMSelectType.Edit, out operationTime, out message);

                    DataTable table = result.Tables[0];

                    bool found = table.Rows.Count > 0;
                    // 新規または検索結果ありの場合
                    if (OpeMode == "Insert" || found)
                    {
                        // 新規で検索結果なしの場合
                        if (!found)
                        {
                            // デフォルトの行を作成
                            DataRow newRow = table.NewRow();
                            // 新規行にデフォルト値を設定する
                            SetDefaultValue(newRow);
                            // 新規行を追加
                            table.Rows.Add(newRow);
                            // 更新を確定
                            table.AcceptChanges();
                        }

                        // 検索結果を取得
                        InputRow = table.Rows[0];

                        // データバインド実行
                        DataBind();

                        // セッションに検索結果を保持
                        Session["inputRow"] = InputRow;

                        // 操作履歴を出力
                        WriteOperationLog();
                    }
                    // 検索結果なしの場合
                    else
                    {
                        argBody.Attributes.Add("onload",
                            "alert('" + CMMessageManager.GetMessage("IV001") +
                            "'); window.returnValue = false; window.close()");
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                    return;
                }
            }
            // 確認画面、戻った画面の場合
            else
            {
                // 編集結果を取得
                InputRow = (DataRow)Session["inputRow"];

                // 結果メッセージを表示
                string mes = (string)Session["retMessage"];
                if (mes != null && mes.Length > 0)
                {
                    ((dynamic)Master).ShowMessage("I", mes);
                    Session.Remove("retMessage");
                }

                // データバインド実行
                DataBind();
            }
        }
        
        //************************************************************************
        /// <summary>
        /// 登録ボタン押下時処理
        /// </summary>
        /// <param name="argBody">bodyタグ</param>
        /// <param name="argFacade">使用ファサード</param>
        //************************************************************************
        protected void OnCommitClick(HtmlGenericControl argBody, ICMBaseBL argFacade)
        {
            // セッションからデータを取得
            InputRow = (DataRow) Session["inputRow"];
            // 登録DataTable
            DataTable inputTable = InputRow.Table;

            // 新規、修正の場合
            if (OpeMode == "Insert" || OpeMode == "Update")
            {
                // データが更新されていなければ、アラート表示
                if (!IsModified())
                {
                    ShowMessage("WV106");
                    return;
                }

                // 入力データを設定
                bool hasError = SetInputRow();

                // セッションに編集結果を保持
                Session["inputRow"] = InputRow;

                // エラーがなければ登録実行
                if (hasError) return;

                // 新規確認の場合
                if (OpeMode == "Insert")
                {
                    DataSet ds = InputRow.Table.DataSet.Clone();
                    inputTable = ds.Tables[0];
                    DataRow row = inputTable.NewRow();
                    // データコピー
                    for (int i = 0; i < inputTable.Columns.Count; i++) row[i] = InputRow[i];
                    // 新規行追加
                    inputTable.Rows.Add(row);
                }
            }
            // 削除確認の場合
            else
            {
                DataSet ds = InputRow.Table.DataSet.Copy();
                inputTable = ds.Tables[0];
                inputTable.Rows[0].Delete();
            }

            try
            {
                // ファサードの呼び出し
                DateTime operationTime;
                argFacade.Update(inputTable.DataSet, out operationTime);

                // 新規、修正の場合
                if (OpeMode == "Insert" || OpeMode == "Update")
                {
                    // 変更を確定
                    InputRow.AcceptChanges();
                    // セッションに編集結果を保持
                    Session["inputRow"] = InputRow;
                    Session["retMessage"] = CMMessageManager.GetMessage("IV003");
                    Session["cancelRet"] = true;
                    // 新規画面へリダイレクト
                    //Response.Redirect(Request.Path + "?mode=" + OpeMode);
                }
                // 削除確認の場合、画面を閉じる
                else Close(true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        //************************************************************************
        /// <summary>
        /// キャンセルボタン押下時処理
        /// </summary>
        /// <param name="argBody">bodyタグ</param>
        //************************************************************************
        protected void OnCancelClick(HtmlGenericControl argBody)
        {
            // セッションからデータを取得
            InputRow = (DataRow)Session["inputRow"];
            bool retVal = (bool)Session["cancelRet"];

            // 新規、修正の場合
            if (OpeMode == "Insert" || OpeMode == "Update")
            {
                string msgcd = IsModified() ? "QV005" : "QV006";

                // 確認画面を表示
                argBody.Attributes.Add("onload",
                    string.Format("if (confirm('{0}')) {{window.returnValue = {1}; window.close()}}",
                        CMMessageManager.GetMessage(msgcd).Replace("\r\n", "\\n"), retVal.ToString().ToLower()));
            }
            else Close(retVal);
        }

        //************************************************************************
        /// <summary>
        /// パネルのデータが変更されているかチェックする。
        /// </summary>
        /// <param name="argPanel">パネル</param>
        /// <returns>True:変更あり, False:変更なし</returns>
        //************************************************************************
        protected bool IsPanelModified(Panel argPanel)
        {
            foreach (Control c in argPanel.Controls)
            {
                WebControl wc = c as WebControl;

                // テキストとドロップダウンが対象
                if (!(wc is DropDownList) && !(wc is TextBox)) continue;

                // 値を比較
                if (InputRow[wc.ID, DataRowVersion.Original].ToString() != GetValue(wc).ToString())
                    return true;
            }

            return false;
        }

        //************************************************************************
        /// <summary>
        /// パネルに設定された値をInputRowに設定する。
        /// </summary>
        /// <param name="argPanel">パネル</param>
        //************************************************************************
        protected void SetPanelInputRow(Panel argPanel)
        {
            foreach (Control c in argPanel.Controls)
            {
                WebControl wc = c as WebControl;

                // テキストとドロップダウンが対象
                if (!(wc is DropDownList) && !(wc is TextBox)) continue;

                // 値を設定
                InputRow[wc.ID] = GetValue(wc);
            }
        }

        //************************************************************************
        /// <summary>
        /// InputRow中で指定列の値が変更されているかチェックし、変更されている場合、
        /// 項目名ラベルの文字色をオレンジに変更する。
        /// </summary>
        /// <param name="argColname">列名</param>
        /// <param name="argLabel">項目名ラベル</param>
        //************************************************************************
        protected void CheckSetModColor(string argColname, Label argLabel)
        {
            if (InputRow[argColname].ToString() != InputRow[argColname, DataRowVersion.Original].ToString())
                argLabel.Attributes.Add("class", "transp head2");
        }

        //************************************************************************
        /// <summary>
        /// 登録日時の文字列を取得する。
        /// </summary>
        /// <returns>登録日時の文字列</returns>
        //************************************************************************
        protected string GetAddInfo()
        {
            if (InputRow == null) return "";
            
            return string.Format("{0}：{1:yyyy/MM/dd HH:mm:ss}&nbsp;</TD><TD>{2}",
                "作成日時", InputRow["作成日時"], InputRow["作成者名"]);
        }

        //************************************************************************
        /// <summary>
        /// 更新日時の文字列を取得する。
        /// </summary>
        /// <returns>更新日時の文字列</returns>
        //************************************************************************
        protected string GetUpdateInfo()
        {
            if (InputRow == null) return "";
            
            return string.Format("{0}：{1:yyyy/MM/dd HH:mm:ss}&nbsp;</TD><TD>{2}",
                "更新日時", InputRow["更新日時"], InputRow["更新者名"]);
        }

        //************************************************************************
        /// <summary>
        /// 指定列の時刻部分を取得する。
        /// </summary>
        /// <param name="argCol">列名</param>
        /// <returns>時刻部分文字列</returns>
        //************************************************************************
        protected string GetHour(string argCol)
        {
            string s = InputRow[argCol].ToString();
            return s.Length < 2 ? "" : s.Substring(0, 2);  
        }

        //************************************************************************
        /// <summary>
        /// 指定列の分部分を取得する。
        /// </summary>
        /// <param name="argCol">列名</param>
        /// <returns>分部分文字列</returns>
        //************************************************************************
        protected string GetMinute(string argCol)
        {
            string s = InputRow[argCol].ToString();
            return s.Length < 4 ? "" : s.Substring(2, 2);
        }

        //************************************************************************
        /// <summary>
        /// 時刻文字列を取得する。
        /// </summary>
        /// <param name="argCol">列名</param>
        /// <returns>分部分文字列</returns>
        //************************************************************************
        protected string GetTimeStr(string argCol)
        {
            // 新規、修正の場合、時刻、分はドロップダウンリストに表示する
            if (OpeMode == "Insert" || OpeMode == "Update") return "：";

            string s = InputRow[argCol].ToString();
            return s.Length < 4 ? "" : s.Substring(0, 2) + "：" + s.Substring(2, 2);
        }
        #endregion

        #region サブクラスで上書きするメソッド
        //************************************************************************
        /// <summary>
        /// キーデータ文字列から検索パラメータを作成する。
        /// </summary>
        /// <param name="argKey">キーデータ文字列</param>
        /// <returns>検索パラメータ</returns>
        //************************************************************************
        protected virtual List<CMSelectParam> CreateSelectParam(string argKey)
        {
            return new List<CMSelectParam>();
        }
        
        //************************************************************************
        /// <summary>
        /// 新規行にデフォルト値を設定する。
        /// </summary>
        /// <param name="argRow">デフォルト値を設定するDataRow</param>
        //************************************************************************
        protected virtual void SetDefaultValue(DataRow argRow)
        {
        }

        //************************************************************************
        /// <summary>
        /// データが変更されているかチェックする。
        /// </summary>
        /// <returns>True:変更あり, False:変更なし</returns>
        //************************************************************************
        protected virtual bool IsModified()
        {
            return false;
        }

        //************************************************************************
        /// <summary>
        /// InputRowに入力データを設定する。
        /// </summary>
        /// <returns>True:エラーあり, False:エラーなし</returns>
        //************************************************************************
        protected virtual bool SetInputRow()
        {
            return false;
        }
        #endregion
    }
}
