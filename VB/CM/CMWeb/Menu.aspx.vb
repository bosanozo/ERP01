Imports System.Data

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL
Imports NEXS.ERP.CM.WEB

''' <summary>
''' メニュー
''' </summary>
Public Partial Class Menu
    Inherits CMBaseListForm
    #Region "イベントハンドラ"
    ''' <summary>
    ''' ページロード
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        Try
            ' 以下は初期表示以外は処理しない
            If IsPostBack Then
                Return
            End If

            MultiView1.ActiveViewIndex = 0

            ' システム情報の検索
            Dim table As DataTable = CommonBL.[Select]("CMSTシステム情報")
            GridView1.DataSource = table

            Dim uinfo As CMUserInfo = CMInformationManager.UserInfo

            ' メニューレベル１の検索
            Dim table2 As DataTable = CommonBL.[Select]("CMSMメニューレベル1", uinfo.SoshikiCd, uinfo.Id)

            For Each row As DataRow In table2.Rows
                Dim item As New MenuItem()
                item.Text = row("画面名").ToString()
                item.Value = row("メニューID").ToString()
                item.Enabled = row("許否フラグ").ToString() = "True"
                Menu1.Items.Add(item)
            Next

            DataBind()
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    ''' <summary>
    ''' タブクリック
    ''' </summary>
    Protected Sub Menu1_MenuItemClick(sender As Object, e As MenuEventArgs)
        MultiView1.ActiveViewIndex = If(Menu1.Items.IndexOf(e.Item) > 0, 1, 0)

        Try
            If MultiView1.ActiveViewIndex = 0 Then
            Else
                Menu2.Items.Clear()

                Dim uinfo As CMUserInfo = CMInformationManager.UserInfo

                ' メニューレベル２の検索
                Dim table As DataTable = CommonBL.[Select]("CMSMメニューレベル2", uinfo.SoshikiCd, uinfo.Id, e.Item.Value)

                For Each row As DataRow In table.Rows
                    Dim item As New MenuItem()
                    item.Text = row("画面名").ToString()
                    item.NavigateUrl = row("URL").ToString()
                    item.Target = row("オプション").ToString()
                    item.Enabled = row("許否フラグ").ToString() <> "False"
                    Menu2.Items.Add(item)
                Next
            End If
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub
    #End Region
End Class
