''' <summary>
''' 日付選択画面
''' </summary>
Public Partial Class CMCalender
    Inherits System.Web.UI.Page
    ''' <summary>
    ''' 月初期化
    ''' </summary>
    Protected Sub DdlMonth_Init(sender As Object, e As EventArgs)
        ' リスト設定
        For i As Integer = 1 To 12
            DdlMonth.Items.Add(i.ToString())
        Next
    End Sub

    ''' <summary>
    ''' ページロード
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        If Not IsPostBack Then
            Dim selectDate As DateTime
            ' 日付が入力されていれば選択して表示
            If DateTime.TryParse(Request.Params("value"), selectDate) Then
                Calendar1.VisibleDate = selectDate
            Else
                selectDate = DateTime.Now.[Date]
            End If

            ' 年初期化
            For i As Integer = -5 To 5
                DdlYear.Items.Add((selectDate.Year + i).ToString())
            Next
            DdlYear.SelectedValue = selectDate.Year.ToString()
            DdlMonth.SelectedValue = selectDate.Month.ToString()
            ' 日付選択
            Calendar1.SelectedDate = selectDate
        End If
    End Sub

    ''' <summary>
    ''' 日付選択時イベントハンドラ
    ''' </summary>
    Protected Sub Calendar1_SelectionChanged(sender As Object, e As EventArgs)
        Body1.Attributes.Add("onload", "window.returnValue = '" & Calendar1.SelectedDate.ToString("yyyy/MM/dd") & "'; window.close()")
    End Sub

    ''' <summary>
    ''' 年月変更
    ''' </summary>
    Protected Sub DdlSelectedIndexChanged(sender As Object, e As EventArgs)
        Refresh()
    End Sub

    ''' <summary>
    ''' 前月
    ''' </summary>
    Protected Sub LbPrev_Click(sender As Object, e As EventArgs)
        If DdlMonth.SelectedIndex = 0 Then
            DdlMonth.SelectedIndex = DdlMonth.Items.Count - 1
            DdlYear.SelectedIndex -= 1
        Else
            DdlMonth.SelectedIndex -= 1
        End If

        Refresh()
    End Sub

    ''' <summary>
    ''' 次月
    ''' </summary>
    Protected Sub LbNext_Click(sender As Object, e As EventArgs)
        If DdlMonth.SelectedIndex = DdlMonth.Items.Count - 1 Then
            DdlMonth.SelectedIndex = 0
            DdlYear.SelectedIndex += 1
        Else
            DdlMonth.SelectedIndex += 1
        End If

        Refresh()
    End Sub

    ''' <summary>
    ''' 表示更新
    ''' </summary>
    Private Sub Refresh()
        ' 年ドロップダウンリスト更新
        Dim year As Integer = Convert.ToInt32(DdlYear.SelectedValue)
        DdlYear.Items.Clear()
        For i As Integer = -5 To 5
            DdlYear.Items.Add((year + i).ToString())
        Next
        DdlYear.SelectedValue = year.ToString()

        ' カレンダー更新
        Dim [date] As New DateTime(year, Convert.ToInt32(DdlMonth.SelectedValue), Calendar1.SelectedDate.Day)
        Calendar1.VisibleDate = [date]
        Calendar1.SelectedDate = [date]
    End Sub
End Class
