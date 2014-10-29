/*******************************************************************************
 * 【共通部品】
 * 
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.WEB;
using NEXS.ERP.CM.BL;

//************************************************************************
/// <summary>
/// 選択子画面
/// </summary>
//************************************************************************
public partial class CM2_CMSubForm : CMBaseJqForm
{
    #region private変数
    private string m_codeName;
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
        if (Request.Params["_search"] != null)
        {
            dynamic result = null;

            try
            {
                var p = Request.QueryString;

                // 検索パラメータ取得
                List<CMSelectParam> param = CMSelectParam.CreateSelectParam(
                    p["Name"], p["Code"], p["Params"], p["DbCodeCol"], p["DbNameCol"], p["CodeId"]);

                // ファサードの呼び出し
                CMMessage message;
                DataTable table = CommonBL.SelectSub(Request.Params["SelectId"], param, out message);

                // 返却メッセージの表示
                if (message != null) ShowMessage(message);

                // 返却データクラス作成
                result = ResultData.CreateResultData(table);
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
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(ex.ToString());
            }

            // 結果をJSONで返却
            if (result != null)
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Response.ContentType = "text/javascript";
                Response.Write(serializer.Serialize(result));
            }
            Response.End();
        }
        // ASP.Net
        else
        {
            // コード値幅調整
            Code.MaxLength = Convert.ToInt32(Request.Params["CodeLen"]);
            Code.Attributes["size"] = Request.Params["CodeLen"];

            // 検索コード名
            m_codeName = Regex.Replace(Request.Params["CodeId"], "(From|To)", "");

            DataBind();
        }
    }
    #endregion

    #region protectedメソッド
    //************************************************************************
    /// <summary>
    /// コードのラベル文字列を返す。
    /// </summary>
    /// <returns>コードのラベル文字列</returns>
    //************************************************************************
    protected string GetCodeLabel()
    {
        return m_codeName.Replace("CD", "コード");
    }

    //************************************************************************
    /// <summary>
    /// 名称のラベル文字列を返す。
    /// </summary>
    /// <returns>名称のラベル文字列</returns>
    //************************************************************************
    protected string GetNameLabel()
    {
        return Regex.Replace(m_codeName, "(CD|ID)", "名");
    }
    #endregion
}