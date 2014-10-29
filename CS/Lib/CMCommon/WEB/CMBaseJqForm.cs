using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// Jquery画面の基底クラス
    /// </summary>
    //************************************************************************
    public class CMBaseJqForm : CMBaseForm
    {
        private Dictionary<string, CM項目DataSet> m_formDsDic = new Dictionary<string, CM項目DataSet>();

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMBaseJqForm()
        {
        }
        #endregion

        #region protectedメソッド
        //************************************************************************
        /// <summary>
        /// フォームデータセットを取得する。
        /// </summary>
        /// <param name="argName">Xmlファイル名</param>
        /// <returns>フォームデータセット</returns>
        //************************************************************************
        protected CM項目DataSet GetFormDataSet(string argName)
        {
            // 編集したデータセットを記憶
            if (!m_formDsDic.ContainsKey(argName))
                m_formDsDic[argName] = CM項目DataSet.ReadFormXml(argName);

            return m_formDsDic[argName];
        }

        #region リクエスト実行
        //************************************************************************
        /// <summary>
        /// 検索を実行する。
        /// </summary>
        /// <param name="argFacade">検索を実行するファサード</param>
        /// <param name="argParam">検索パラメータ</argParam>
        /// <returns>検索結果DataSet</returns>
        //************************************************************************
        protected void DoSearch(ICMBaseBL argFacade, List<CMSelectParam> argParam = null)
        {
            // 検索パラメータ取得
            if (argParam == null) argParam = CMSelectParam.CreateSelectParam(Request.QueryString);

            dynamic result = null;
            DataSet ds = null;

            try
            {
                string search = Request.QueryString["_search"];
                CMSelectType selType = search == "edit" ? CMSelectType.Edit : 
                    search == "csvexp" ? CMSelectType.Csv : CMSelectType.List;

                // ファサードの呼び出し用変数
                DateTime operationTime;
                CMMessage message;

                // ファサードの呼び出し
                ds = argFacade.Select(argParam, selType, out operationTime, out message);

                // 返却メッセージの表示
                if (message != null) ShowMessage(message);

                DataTable table = ds.Tables[0];
                    
                // 返却データクラス作成
                switch (selType)
                {
                    // 一覧検索
                    case CMSelectType.List:
                        result = ResultData.CreateResultData(table);
                        break;

                    case CMSelectType.Edit:
                        result = ResultDataSet.CreateResultDataSet(ds);
                        break;

                    case CMSelectType.Csv:
                        // ヘッダ設定
                        Response.AppendHeader("Content-type", "application/octet-stream; charset=UTF-8");
                        Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
                            ((dynamic)Master).Title + ".xlsx");

                        // Excelファイル作成
                        var xslDoc = ExcelUtil.CreateExcel(ds);
                        xslDoc.SaveAs(Response.OutputStream);
                        break;
                }

                // 検索結果を保存
                if (selType != CMSelectType.Csv)
                    Session[Request.Path + "_DataSet"] = ds;
            }
            catch (CMException ex)
            {
                Response.StatusCode = 200;

                result = new ResultStatus { error = true };
                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = ex.CMMessage.MessageCd,
                    message = ex.CMMessage.ToString(),
                    rowField = new RowField(ex.CMMessage.RowField)
                });
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                // データベースエラー
                Response.StatusCode = 500;

                result = new ResultStatus { error = true };
                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = "EV002",
                    message = CMMessageManager.GetMessage("EV002", ex.Message)
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(ex.ToString());
            }

            // 結果をJSONで返却
            if (result != null)
            {
                var serializer = new JavaScriptSerializer();
                Response.ContentType = "text/javascript";
                Response.Write(serializer.Serialize(result));
            }

            Response.End();
        }

        //************************************************************************
        /// <summary>
        /// 操作を実行する。
        /// </summary>
        /// <param name="argFacade">操作を実行するファサード</param>
        /// <param name="argDataSet">操作対象のDataSet</param>
        /// <param name="argForm">Form</param>
        //************************************************************************
        protected void DoOperation(ICMBaseBL argFacade, DataSet argDataSet, NameValueCollection argForm = null)
        {
            if (argForm == null) argForm = Request.Form;

            dynamic result = null;

            try
            {
                // 編集対象のDataTable取得
                DataTable table = Request.QueryString["TableName"] != null ?
                    (DataTable)argDataSet.Tables[Request.QueryString["TableName"]] : (DataTable)argDataSet.Tables[0];

                // 編集対象のDataRow取得
                string id = argForm["id"];
                DataRow row = string.IsNullOrEmpty(id) || id == "_empty" ?
                    null : table.Select("ROWNUMBER=" + id).First();

                string oper = argForm["oper"];

                switch (oper)
                {
                    case "add":
                    case "new":
                        // todo:未検索時はtableをクリアする

                        row = table.NewRow();

                        // 新規のidを取得
                        int retId = table.Rows.Count > 0 ? Convert.ToInt32(table.AsEnumerable().Max(tr => tr["ROWNUMBER"])) + 1 : 0;
                        row["ROWNUMBER"] = retId;

                        // パラメータを設定
                        foreach (string key in argForm.Keys)
                        {
                            if (!table.Columns.Contains(key)) continue;

                            row[key] = CMUtil.GetDataColumnVal(table.Columns[key], argForm[key]);
                        }

                        table.Rows.Add(row);

                        // idを返却
                        if (oper == "add") Response.Write(retId.ToString());
                        else result = new ResultStatus { id = retId };
                        break;

                    case "edit":
                        foreach (string key in argForm.Keys)
                        {
                            if (!table.Columns.Contains(key)) continue;

                            string txtVal = argForm[key];
                            object value = CMUtil.GetDataColumnVal(table.Columns[key], txtVal);

                            if (value == DBNull.Value)
                            {
                                if (row[key] != DBNull.Value) row[key] = value;
                                continue;
                            }

                            // 型に応じて、値を比較し、DataTableに値を設定する
                            switch (table.Columns[key].DataType.Name)
                            {
                                case "bool":
                                case "Boolean":
                                    if (row[key] == DBNull.Value) row[key] = value;
                                    else if (row[key].ToString() != value.ToString())
                                        row[key] = value;
                                    // できてない
                                    break;

                                case "DateTime":
                                    if (row[key] == DBNull.Value) row[key] = value;
                                    else if (((DateTime)row[key]) != ((DateTime)value))
                                        row[key] = value;
                                    break;

                                default:
                                    if (row[key].ToString() != txtVal)
                                        row[key] = value;
                                    break;
                            }
                        }

                        // 変更ありの場合、idを返却
                        Response.Write(row.RowState == DataRowState.Modified ? id : "");
                        break;

                    case "del":
                        result = new ResultStatus();
                        foreach (string ids in id.Split(','))
                        {
                            DataRow delRow = table.Select("ROWNUMBER=" + ids).First();
                            delRow["削除"] = "1";
                        }
                        break;

                    case "cancel":
                        row.RejectChanges();

                        // 返却データクラス作成
                        result = new Dictionary<string, object>();

                        if (row.RowState == DataRowState.Detached) row.Delete();
                        else
                        {
                            foreach (DataColumn dcol in table.Columns)
                            {
                                if (dcol.ColumnName == "作成日時") break;
                                result.Add(dcol.ColumnName, row[dcol.ColumnName, DataRowVersion.Original]);
                            }
                        }
                        break;

                    case "commit":
                        result = new ResultStatus();
                        DataSet updateDs = argDataSet.GetChanges();

                        if (updateDs != null)
                        {
                            foreach (DataTable dt in updateDs.Tables)
                            {
                                // 削除行を確定
                                foreach (var delRow in dt.Select("削除 = '1'")) delRow.Delete();
                            }

                            // ファサードの呼び出し用変数
                            DateTime operationTime;

                            // ファサードの呼び出し
                            argFacade.Update(updateDs, out operationTime);
                        }
                        else
                        {
                            result.error = true;
                            // エラーメッセージを設定
                            result.messages.Add(new ResultMessage
                            {
                                messageCd = "WV106",
                                message = CMMessageManager.GetMessage("WV106"),
                            });
                        }
                        break;
                }
            }
            catch (CMException ex)
            {
                Response.StatusCode = 200;

                result = new ResultStatus { error = true };
                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = ex.CMMessage.MessageCd,
                    message = ex.CMMessage.ToString(),
                    rowField = new RowField(ex.CMMessage.RowField)
                });
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                // データベースエラー
                Response.StatusCode = 500;

                result = new ResultStatus { error = true };
                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = "EV002",
                    message = CMMessageManager.GetMessage("EV002", ex.Message)
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(ex.ToString());
            }

            // 結果をJSONで返却
            if (result != null)
            {
                var serializer = new JavaScriptSerializer();
                Response.ContentType = "text/javascript";
                Response.Write(serializer.Serialize(result));
            }
            Response.End();
        }
        #endregion
        #endregion
    }
}
