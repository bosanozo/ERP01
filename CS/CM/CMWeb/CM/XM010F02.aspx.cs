/*******************************************************************************
 * 【ERPシステム】
 *
 * 作成者: XXＴ／XX
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.WEB;
using NEXS.ERP.CM.BL;

//************************************************************************
/// <summary>
/// XXマスタメンテ
/// </summary>
//************************************************************************
public partial class CM_XM010F02 : CMBaseEntryForm
{
    #region BLインジェクション用フィールド
    protected XM010BL m_facade;
    #endregion

    #region プロパティ
    /// <summary>
    /// 入力データを保持するDataRow
    /// </summary>
    public DataRow InputRow2 { get; set; }
    #endregion
    
    #region イベントハンドラ
    //************************************************************************
    /// <summary>
    /// ページロード
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        // 共通設定
        Master.Body.Attributes.Remove("onload");

        // 操作モードを設定
        string subName = SetOpeMode(PanelKeyItems, PanelSubItems,
            PanelUpdateInfo, PanelFunction, null, BtnCommit, BtnCancel);

        // 画面ヘッダ初期化
        Master.Title = "エンティティ定義　" + subName;

        // 以下は初期表示以外は処理しない
        if (IsPostBack) return;

        try
        {
            // 区分値の検索
            DataTable kbnTable = CommonBL.SelectKbn("X002");

            // 区分値の設定
            DataView dv = new DataView(kbnTable, "分類CD = 'X002'", null, DataViewRowState.CurrentRows);

            // エンティティ種別のアイテム設定
            DataTable table = dv.ToTable();
            エンティティ種別.DataSource = table;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        // 標準の画面表示処理実行
        OnPageOnLoad();

        // 新規で既存選択はVerUP
        if (OpeMode == "Insert" && オブジェクト名.Text.Length > 0)
            VER.Text = (Convert.ToInt32(VER.Text) + 1).ToString();
    }

    //************************************************************************
    /// <summary>
    /// 画面表示処理
    /// </summary>
    //************************************************************************
    protected void OnPageOnLoad()
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
                DataSet result = m_facade.Select(param, CMSelectType.Edit, out operationTime, out message);

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
                    InputRow2 = result.Tables["XMエンティティ"].Rows[0];
                    GridView1.DataSource = result.Tables["XMエンティティ項目"];

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
                    Master.Body.Attributes.Add("onload",
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
    /// 登録ボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnCommit_Click(object sender, EventArgs e)
    {
        // 標準の登録ボタン押下時処理実行
        OnCommitClick(Master.Body, m_facade);
    }

    //************************************************************************
    /// <summary>
    /// キャンセルボタン押下
    /// </summary>
    //************************************************************************
    protected void BtnCancel_Click(object sender, EventArgs e)
    {
        // 標準のキャンセルボタン押下時処理実行
        OnCancelClick(Master.Body);
    }
    #endregion

    #region overrideメソッド
    //************************************************************************
    /// <summary>
    /// キーデータ文字列から検索パラメータを作成する。
    /// </summary>
    /// <param name="argKey">キーデータ文字列</param>
    /// <returns>検索パラメータ</returns>
    //************************************************************************
    protected override List<CMSelectParam> CreateSelectParam(string argKey)
    {
        List<CMSelectParam> param = new List<CMSelectParam>();

        // 未選択で新規
        if (String.IsNullOrEmpty(argKey))
            param.Add(new CMSelectParam("オブジェクト名", "= @オブジェクト名", ""));
        // 既存レコードを選択
        else
        {
            string[] keys = argKey.Split(',');
            param.Add(new CMSelectParam("オブジェクト名", "= @オブジェクト名", keys[0]));
            param.Add(new CMSelectParam("VER", "= @VER", int.Parse(keys[1])));
        }

        return param;
    }

    //************************************************************************
    /// <summary>
    /// データが変更されているかチェックする。
    /// </summary>
    /// <returns>True:変更あり, False:変更なし</returns>
    //************************************************************************
    protected override bool IsModified()
    {
        // 新規の場合、キー項目チェック
        if (OpeMode == "Insert" && IsPanelModified(PanelKeyItems)) return true;

        // 従属項目チェック
        if (IsPanelModified(PanelSubItems)) return true;

        return false;
    }

    //************************************************************************
    /// <summary>
    /// 新規行にデフォルト値を設定する。
    /// </summary>
    /// <param name="argRow">デフォルト値を設定するDataRow</param>
    //************************************************************************
    protected override void SetDefaultValue(DataRow argRow)
    {
        argRow["オブジェクト型"] = "1";
        argRow["VER"] = 1;
    }

    //************************************************************************
    /// <summary>
    /// DataRowに入力データを設定する。
    /// </summary>
    /// <returns>True:エラーあり, False:エラーなし</returns>
    //************************************************************************
    protected override bool SetInputRow()
    {
        bool hasError = false;

        // 新規の場合
        if (OpeMode == "Insert") SetPanelInputRow(PanelKeyItems);

        // 従属項目値を設定
        SetPanelInputRow(PanelSubItems);

        // エラー有無を返却
        return hasError;
    }
    #endregion
}