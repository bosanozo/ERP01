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
public partial class CMSubForm : CMBaseListForm
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
        // 機能ボタン スクリプト登録
        BtnSelect.Attributes.Add("onclick", "return CheckInputList()");

        // 以下は初期表示以外は処理しない
        if (IsPostBack) return;

        // コード値幅調整
        Code.MaxLength = Convert.ToInt32(Request.Params["CodeLen"]);
        Code.Width = Code.MaxLength * 8 + 2;

        // 検索コード名
        m_codeName = Regex.Replace(Request.Params["Code"], "(From|To)", "");

        // グリッドの列名設定
        GridView1.Columns[1].HeaderText = GetCodeLabel();
        GridView1.Columns[2].HeaderText = GetNameLabel();
        
        // データバインド実行
        DataBind();
    }

    //************************************************************************
    /// <summary>
    /// 検索ボタン押下
    /// </summary>
    //************************************************************************
    protected void Select_Command(object sender, CommandEventArgs e)
    {
        // 画面の条件を取得
        List<CMSelectParam> formParam = CreateSelectParam(PanelCondition);

        // 項目名の置き換え
        foreach (var p in formParam)
        {
            if (p.name == "Code") p.name = string.IsNullOrEmpty(Request.Params["DbCodeCol"]) ?
                GridView1.Columns[1].HeaderText.Replace("コード", "CD") : Request.Params["DbCodeCol"];
            else if (p.name == "Name")
            {
                p.name = string.IsNullOrEmpty(Request.Params["DbNameCol"]) ?
                   GridView1.Columns[2].HeaderText : Request.Params["DbNameCol"];
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

        bool hasError = DoSelect(param, GridView1);

        // 正常終了の場合
        if (!hasError)
        {
            // 検索条件を記憶
            Session["SelectCondition"] = param;
        }
    }

    //************************************************************************
    /// <summary>
    /// ページ切り替え
    /// </summary>
    //************************************************************************
    protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        // 検索条件取得
        List<CMSelectParam> param = (List<CMSelectParam>)Session["SelectCondition"];
        // 検索実行
        DoSelect(param, GridView1, e.NewPageIndex);
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

    #region privateメソッド
    //************************************************************************
    /// <summary>
    /// 検索を実行する。
    /// </summary>
    /// <param name="argParam">検索条件パラメータ</param>
    /// <param name="argGrid">一覧表示用グリッド</param>
    /// <param name="argPage">ページ</param>
    /// <returns>True:エラーあり, False:エラーなし</returns>
    //************************************************************************
    private bool DoSelect(List<CMSelectParam> argParam, GridView argGrid, int argPage = 0)
    {
        try
        {
            // ファサードの呼び出し
            CMMessage message;
            DataTable result = CommonBL.SelectSub(Request.Params["SelectId"], argParam, out message);

            // 返却メッセージの表示
            if (message != null) ShowMessage(message);

            int idx = 0;
            foreach (var col in argGrid.Columns)
            {
                // 列ヘッダ設定
                if (col is BoundField)
                {
                    BoundField bf = col as BoundField;
                    if (idx > 1) bf.HeaderText = result.Columns[idx].ColumnName;
                    bf.DataField = result.Columns[idx].ColumnName;
                    idx++;
                }
            }

            // DataSource設定
            argGrid.DataSource = result;
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
    #endregion
}