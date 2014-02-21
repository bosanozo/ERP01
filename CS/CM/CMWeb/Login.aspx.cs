using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Seasar.Quill;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;

//************************************************************************
/// <summary>
/// ログイン画面
/// </summary>
/// <remarks>
/// ログイン、ログアウト処理を実施
/// </remarks>
/// <author></author>
/// <date>2006/03/31</date>
/// <version>新規作成</version>
//************************************************************************
public partial class Login : System.Web.UI.Page
{
    #region インジェクション用フィールド
    protected ICMAuthenticationBL m_authenticationBL;
    #endregion

    #region コンストラクタ
    //************************************************************************
    /// <summary>
    /// コンストラクタ
    /// </summary>
    //************************************************************************
    public Login()
    {
        // インジェクション実行
        QuillInjector injector = QuillInjector.GetInstance();
        injector.Inject(this);
    }
    #endregion

    #region イベントハンドラ
    //************************************************************************
    /// <summary>
    /// Page Init(画面初期化)
    /// </summary>
    //************************************************************************
    protected void Page_Init(object sender, EventArgs e)
    {
        // このページはユーザのログイン状態が確定していないため、ユーザの操作によっては
        // 同一ページの表示中にユーザのログイン状態が変化する可能性がある。
        // ユーザのログイン状態が変化すると、ViewStateUserKeyの状態が不正となるため、
        // このページでは明示的に Nothing を設定し、ユーザ状態による検証を無効にしている。
        ViewStateUserKey = null;
    }

    //************************************************************************
    /// <summary>
    /// Page Load(画面表示）
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        // 画面タイトル設定
        Master.Title = "ログイン";
    }

    //************************************************************************
    /// <summary>
    /// 「ログイン」ボタン　Click
    /// </summary>
    //************************************************************************
    protected void ButtonLogin_Click(object sender, EventArgs e)
    {
        // ユーザＩＤ、パスワードによる認証確認
        string userId = TextBoxUserId.Text.ToUpper().Trim();
        string password = TextBoxPassword.Text.Trim();
        bool authenticated = m_authenticationBL.Authenticate(userId, password);
        if (authenticated)
        {
            // フォーム認証チケットの発行
            FormsAuthentication.SetAuthCookie(userId, false);

            // メニュー画面へ遷移
            //Response.Redirect("~/CM/CMSM010F01.aspx");
            Response.Redirect("Menu.aspx");
        }
        else
        {
            // 認証失敗時エラーメッセージの表示
            Master.ShowMessage("E", "IDまたはパスワードが不正です。");
        }
    }
    #endregion
}
