using System;
using System.Data;
using System.Drawing;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using NEXS.ERP.CM.Common;

//************************************************************************
/// <summary>
/// 表示タイプ
/// </summary>
//************************************************************************
public enum DisplayType
{
    /// <summary>
    /// テキストボックス
    /// </summary>
    TextBox,
    /// <summary>
    /// ラベル
    /// </summary>
    Label
}

//************************************************************************
/// <summary>
/// マスタページ
/// </summary>
//************************************************************************
public partial class CMMaster : System.Web.UI.MasterPage
{
    private DateTime m_operationTime;

    #region publicプロパティ
    //************************************************************************
    /// <summary>
    /// ページタイトル表示エリアへ、タイトルを表示します
    /// </summary>
    //************************************************************************
    public string Title
    {
        get { return LabelTitle.Text; }
        set { LabelTitle.Text = value; }
    }

    /// <summary>操作時刻</summary>
    public DateTime OperationTime
    {
        get { return m_operationTime; }
        set
        {
            m_operationTime = value;
            LabelDateTime.Text = value.ToString("yyyy/MM/dd HH:mm");
        }
    }

    /// <summary>画面ＩＤ</summary>
    public string FormId
    {
        get { return LabelFormId.Text; }
        set { LabelFormId.Text = value; }
    }

    public HtmlGenericControl Body
    {
        get { return Body1; }
    }
    #endregion

    #region publicメソッド
    //************************************************************************
    /// <summary>
    /// メッセージ表示エリアへ、メッセージを表示します。
    /// </summary>
    /// <remarks>
    /// メッセージ種別がエラーの場合は赤文字で表示、ガイドの場合は青文字で表示します
    /// 表示文字列はHTMLエンコードします
    /// また、表示文字列中に改行があった場合は、ｂｒタグで置換します。
    /// </remarks>
    /// <param name="argType">メッセージ種別</param>
    /// <param name="argMessage">表示するメッセージ文字列</param>
    //************************************************************************
    public void ShowMessage(string argType, string argMessage)
    {
        string method = null;

        switch (argType[0])
        {
            case 'E':
                method = "MsgError";
                break;
            case 'W':
                method = "alert";
                break;
            case 'I':
                method = "MsgInfo";
                break;
        }

        // ダイアログ表示
        Body.Attributes.Add("onload", string.Format("{0}('{1}')", //method,
            "alert", argMessage.Replace("\r\n", "\\n")));

#if HtmlMessage
        string message = Server.HtmlEncode(argMessage);
        message = message.Replace("\r\n", "<br/>"); 
        message = message.Replace("\r", "<br/>");
        message = message.Replace("\n", "<br/>");

        switch (argType[0])
        {
            case 'E':
                LabelMessage.ForeColor = Color.Red;
                break;
            case 'W':
                LabelMessage.ForeColor = Color.Yellow;
                break;
            case 'I':
                LabelMessage.ForeColor = Color.Black;
                break;
        }

        LabelMessage.Text = message;
#endif
    }
    #endregion

    #region イベントハンドラ
    //************************************************************************
    /// <summary>
    /// ページの ViewStateUserKey を設定します。
    /// </summary>
    //************************************************************************
    protected void Page_Init(object sender, EventArgs e)
    {
        CMUserInfo user = CMInformationManager.UserInfo;
        if (user != null)
        {
            // ユーザIDとセッションIDを、ページの ViewStateUserKey に設定
            // （ユーザとセッションを検証するため）
            Page.ViewStateUserKey = user.Id + ":" + Session.SessionID;
        }
    }

    //************************************************************************
    /// <summary>
    /// ログインユーザのID、名称を表示します
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        // ユーザIDの設定
        CMUserInfo user = CMInformationManager.UserInfo;

        // ユーザ名称の設定
        LabelUserName.Text = user != null ? Server.HtmlEncode(user.Name) : "";

        // 操作時刻の設定
        OperationTime = DateTime.Now;

        // 画面ＩＤの設定
        FormId = "【" + System.IO.Path.GetFileNameWithoutExtension(Page.AppRelativeVirtualPath) + "】";

        // メッセージクリア
        LabelMessage.Text = "";
    }
    #endregion
}