using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

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
    /// Postパラメータ
    /// </summary>
    //************************************************************************
    public class PostParam
    {
        public string id { get; set; }
        public string oper { get; set; }
    }

    //************************************************************************
    /// <summary>
    /// APIコントローラの基底クラス
    /// </summary>
    //************************************************************************
    [CustomExceptionFilter]
    public class BaseApiController : ApiController
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

        /// <summary>
        /// 更新用DataSet
        /// </summary>
        protected DataSet UpdateDataSet
        {
            get {
                return (DataSet)HttpContext.Current.Session[
                    ControllerContext.ControllerDescriptor.ControllerName + "_DataSet"];
            }
            set
            {
                HttpContext.Current.Session[
                    ControllerContext.ControllerDescriptor.ControllerName + "_DataSet"] = value;
            }
        }
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public BaseApiController()
        {
            // ロガーを取得
            m_logger = LogManager.GetLogger(this.GetType());

            // インジェクション実行
            QuillInjector injector = QuillInjector.GetInstance();
            injector.Inject(this);
        }
        #endregion

        #region リクエスト実行
        //************************************************************************
        /// <summary>
        /// 検索を実行する。
        /// </summary>
        /// <param name="argFacade">検索を実行するファサード</param>
        /// <param name="argSelelctType">検索種別</param>
        /// <param name="argParam">検索パラメータ</param>
        /// <returns>検索結果DataSet</returns>
        //************************************************************************
        protected object DoSearch(ICMBaseBL argFacade, CMSelectType argSelelctType, List<CMSelectParam> argParam)
        {
            // ファサードの呼び出し用変数
            DateTime operationTime;
            CMMessage message;

            // ファサードの呼び出し
            var ds = argFacade.Select(argParam, argSelelctType, out operationTime, out message);

            // データテーブル取得
            var table = ds.Tables[0];

            object result = null;

            // 返却データクラス作成
            switch (argSelelctType)
            {
                // 一覧検索
                case CMSelectType.List:
                    result = ResultData.CreateResultData(table);
                    break;

                case CMSelectType.Edit:
                    var resultDataSet = ResultDataSet.CreateResultDataSet(ds);
                    result = resultDataSet;
                    break;

                case CMSelectType.Csv:
                    // Excelファイル作成
                    var xslDoc = ExcelUtil.CreateExcel(ds);

                    /*
                    // ファイルに出力
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "組織" + ".xlsx");
                    xslDoc.SaveAs(path);
                    var stream = new FileStream(path, FileMode.Open);
                     */
                    // メモリに出力
                    var stream = new MemoryStream();
                    xslDoc.SaveAs(stream);
                    stream.Position = 0;

                    // 返却メッセージ作成
                    HttpResponseMessage resultMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    resultMessage.Content = new StreamContent(stream);
                    // ヘッダ設定
                    resultMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    resultMessage.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("Attachment") { FileName = "組織" + ".xlsx" };
                    result = resultMessage;
                    break;
            }

            // 検索結果を保存
            if (argSelelctType != CMSelectType.Csv) UpdateDataSet = ds;

            return result;
        }

        //************************************************************************
        /// <summary>
        /// 編集、キャンセル、登録を実行する。
        /// </summary>
        /// <param name="argFacade">登録を実行するファサード</param>
        /// <param name="argParam">Postパラメータ</param>
        /// <param name="argForm">フォームデータ</param>
        /// <returns>結果</returns>
        //************************************************************************
        protected object DoEdit(ICMBaseBL argFacade, PostParam argParam, NameValueCollection argForm)
        {
            // 編集対象のDataSet取得
            DataSet ds = UpdateDataSet;

            // 編集対象のDataTable取得
            DataTable table = ds.Tables[0];

            // 編集対象のDataRow取得
            DataRow row = string.IsNullOrEmpty(argParam.id) || argParam.id == "_empty" ?
                null : table.Select("ROWNUMBER=" + argParam.id).First();

            object result = null;

            // operによる場合分け
            switch (argParam.oper)
            {
                // 行編集
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
                    result = row.RowState == DataRowState.Modified ? argParam.id : "";
                    break;

                // 行編集キャンセル
                case "cancel":
                    row.RejectChanges();

                    // 返却データクラス作成
                    var dic = new Dictionary<string, object>();

                    if (row.RowState == DataRowState.Detached) row.Delete();
                    else
                    {
                        // 編集前のデータを設定
                        foreach (DataColumn dcol in table.Columns)
                        {
                            if (dcol.ColumnName == "作成日時") break;
                            dic.Add(dcol.ColumnName, row[dcol.ColumnName, DataRowVersion.Original]);
                        }
                    }

                    // 編集前のデータを返却
                    result = dic;
                    break;

                // 登録
                case "commit":
                    var resultStatus = new ResultStatus();
                    DataSet updateDs = ds.GetChanges();

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
                        resultStatus.error = true;
                        // エラーメッセージを設定
                        resultStatus.messages.Add(new ResultMessage
                        {
                            messageCd = "WV106",
                            message = CMMessageManager.GetMessage("WV106"),
                        });
                    }

                    result = resultStatus;
                    break;
            }

            return result;
        }

        //************************************************************************
        /// <summary>
        /// データセットに行追加を実行する。
        /// </summary>
        /// <param name="argForm">フォームデータ</param>
        /// <returns>新規に採番したid</returns>
        //************************************************************************
        protected string DoAdd(NameValueCollection argForm)
        {
            // 編集対象のDataSet取得
            DataSet ds = UpdateDataSet;

            // 編集対象のDataTable取得
            DataTable table = ds.Tables[0];

            // todo:未検索時はtableをクリアする

            var row = table.NewRow();

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
            return retId.ToString();
            //if (oper == "add") Response.Write(retId.ToString());
            //else result = new ResultStatus { id = retId };
        }

        //************************************************************************
        /// <summary>
        /// データセットの行削除を実行する。
        /// </summary>
        /// <param name="argParam">フォームデータ</param>
        /// <returns>結果ステータス</returns>
        //************************************************************************
        protected ResultStatus DoDelete(PostParam argParam)
        {
            // 編集対象のDataSet取得
            DataSet ds = UpdateDataSet;

            // 編集対象のDataTable取得
            DataTable table = ds.Tables[0];

            var result = new ResultStatus();
            foreach (string ids in argParam.id.Split(','))
            {
                DataRow delRow = table.Select("ROWNUMBER=" + ids).First();
                delRow["削除"] = "1";
            }

            return result;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// 検索種別を返す。
        /// </summary>
        /// <param name="arg">検索種別文字列</param>
        /// <returns>検索種別</returns>
        //************************************************************************
        protected CMSelectType GetSelectType(string arg)
        {
            return arg == "edit" ? CMSelectType.Edit :
                arg == "csvexp" ? CMSelectType.Csv : CMSelectType.List;
        }
    }
}
