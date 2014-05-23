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
                // 検索パラメータ取得
                List<CMSelectParam> param = CreateSelectParam();

                // ファサードの呼び出し
                CMMessage message;
                DataTable table = CommonBL.SelectSub(Request.Params["SelectId"], param, out message);

                // 返却メッセージの表示
                if (message != null) ShowMessage(message);

                // 返却データクラス作成
                result = new ResultData();
                foreach (DataRow row in table.Rows)
                    result.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });
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
                    rowField = ex.CMMessage.RowField
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
            Code.Width = Code.MaxLength * 8 + 2;

            // 検索コード名
            m_codeName = Regex.Replace(Request.Params["CodeId"], "(From|To)", "");

            DataBind();
        }
    }

    //************************************************************************
    /// <summary>
    /// 検索パラメータ作成
    /// </summary>
    /// <returns>検索パラメータ</returns>
    //************************************************************************
    protected List<CMSelectParam> CreateSelectParam()
    {
        // 画面の条件を取得
        List<CMSelectParam> formParam = new List<CMSelectParam>();

        if (!string.IsNullOrEmpty(Request.Params["Name"]))
            formParam.Add(new CMSelectParam("Name", "LIKE @Name", "%" + Request.Params["Name"] + "%"));

        if (!string.IsNullOrEmpty(Request.Params["Code"]))
            formParam.Add(new CMSelectParam("Code", "= @Code", Request.Params["Code"]));

        // 項目名の置き換え
        foreach (var p in formParam)
        {
            if (p.name == "Code") p.name = string.IsNullOrEmpty(Request.Params["DbCodeCol"]) ?
                m_codeName : Request.Params["DbCodeCol"];
            else if (p.name == "Name")
            {
                p.name = string.IsNullOrEmpty(Request.Params["DbNameCol"]) ?
                   GetNameLabel() : Request.Params["DbNameCol"];
                p.condtion = "LIKE @" + p.name;
                p.paramFrom = "%" + p.paramFrom + "%";
            }
        }

        // 検索パラメータ作成
        List<CMSelectParam> param = new List<CMSelectParam>();

        // 追加パラメータがある場合、追加する
        if (!string.IsNullOrEmpty(Request.Params["Params"]))
        {
            foreach (string p in Request.Params["Params"].Split())
            {
                object value;

                // "#"から始まる場合はUserInfoから設定
                if (p[0] == '#')
                {
                    PropertyInfo pi = CMInformationManager.UserInfo.GetType().GetProperty(p.Substring(1));
                    value = pi.GetValue(CMInformationManager.UserInfo, null);
                }
                // セルの値を取得
                else value = p;

                // パラメータ追加
                param.Add(new CMSelectParam(null, null, value));
            }
        }

        // 画面の条件を追加
        param.AddRange(formParam);

        return param;
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