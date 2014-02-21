/*******************************************************************************
 * 【共通部品】
 * 
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

//************************************************************************
/// <summary>
/// 日付選択画面
/// </summary>
//************************************************************************
public partial class CMCalender : System.Web.UI.Page
{
    //************************************************************************
    /// <summary>
    /// 月初期化
    /// </summary>
    //************************************************************************
    protected void DdlMonth_Init(object sender, EventArgs e)
    {
        // リスト設定
        for (int i = 1; i <= 12; i++) DdlMonth.Items.Add(i.ToString());
    }

    //************************************************************************
    /// <summary>
    /// ページロード
    /// </summary>
    //************************************************************************
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            DateTime selectDate;
            // 日付が入力されていれば選択して表示
            if (DateTime.TryParse(Request.Params["value"], out selectDate))
                Calendar1.VisibleDate = selectDate;
            else selectDate = DateTime.Now.Date;

            // 年初期化
            for (int i = -5; i <= 5; i++) DdlYear.Items.Add((selectDate.Year + i).ToString());
            DdlYear.SelectedValue = selectDate.Year.ToString();
            DdlMonth.SelectedValue = selectDate.Month.ToString();
            // 日付選択
            Calendar1.SelectedDate = selectDate;
        }
    }

    //************************************************************************
    /// <summary>
    /// 日付選択時イベントハンドラ
    /// </summary>
    //************************************************************************
    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        Body1.Attributes.Add("onload", "window.returnValue = '" + Calendar1.SelectedDate.ToString("yyyy/MM/dd")
             + "'; window.close()");
    }

    //************************************************************************
    /// <summary>
    /// 年月変更
    /// </summary>
    //************************************************************************
    protected void DdlSelectedIndexChanged(object sender, EventArgs e)
    {
        Refresh();
    }

    //************************************************************************
    /// <summary>
    /// 前月
    /// </summary>
    //************************************************************************
    protected void LbPrev_Click(object sender, EventArgs e)
    {
        if (DdlMonth.SelectedIndex == 0)
        {
            DdlMonth.SelectedIndex = DdlMonth.Items.Count - 1;
            DdlYear.SelectedIndex--;
        }
        else DdlMonth.SelectedIndex--;

        Refresh();
    }

    //************************************************************************
    /// <summary>
    /// 次月
    /// </summary>
    //************************************************************************
    protected void LbNext_Click(object sender, EventArgs e)
    {
        if (DdlMonth.SelectedIndex == DdlMonth.Items.Count - 1)
        {
            DdlMonth.SelectedIndex = 0;
            DdlYear.SelectedIndex++;
        }
        else DdlMonth.SelectedIndex++;

        Refresh();
    }

    //************************************************************************
    /// <summary>
    /// 表示更新
    /// </summary>
    //************************************************************************
    private void Refresh()
    {
        // 年ドロップダウンリスト更新
        int year = Convert.ToInt32(DdlYear.SelectedValue);
        DdlYear.Items.Clear();
        for (int i = -5; i <= 5; i++) DdlYear.Items.Add((year + i).ToString());
        DdlYear.SelectedValue = year.ToString();

        // カレンダー更新
        DateTime date = new DateTime(year, Convert.ToInt32(DdlMonth.SelectedValue),
            Calendar1.SelectedDate.Day);
        Calendar1.VisibleDate = date;
        Calendar1.SelectedDate = date;            
    }
}