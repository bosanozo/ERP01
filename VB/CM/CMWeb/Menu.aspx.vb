Imports System.Data

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL
Imports NEXS.ERP.CM.WEB

''' <summary>
''' ���j���[
''' </summary>
Public Partial Class Menu
    Inherits CMBaseListForm
    #Region "�C�x���g�n���h��"
    ''' <summary>
    ''' �y�[�W���[�h
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        Try
            ' �ȉ��͏����\���ȊO�͏������Ȃ�
            If IsPostBack Then
                Return
            End If

            MultiView1.ActiveViewIndex = 0

            ' �V�X�e�����̌���
            Dim table As DataTable = CommonBL.[Select]("CMST�V�X�e�����")
            GridView1.DataSource = table

            Dim uinfo As CMUserInfo = CMInformationManager.UserInfo

            ' ���j���[���x���P�̌���
            Dim table2 As DataTable = CommonBL.[Select]("CMSM���j���[���x��1", uinfo.SoshikiCd, uinfo.Id)

            For Each row As DataRow In table2.Rows
                Dim item As New MenuItem()
                item.Text = row("��ʖ�").ToString()
                item.Value = row("���j���[ID").ToString()
                item.Enabled = row("���ۃt���O").ToString() = "True"
                Menu1.Items.Add(item)
            Next

            DataBind()
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    ''' <summary>
    ''' �^�u�N���b�N
    ''' </summary>
    Protected Sub Menu1_MenuItemClick(sender As Object, e As MenuEventArgs)
        MultiView1.ActiveViewIndex = If(Menu1.Items.IndexOf(e.Item) > 0, 1, 0)

        Try
            If MultiView1.ActiveViewIndex = 0 Then
            Else
                Menu2.Items.Clear()

                Dim uinfo As CMUserInfo = CMInformationManager.UserInfo

                ' ���j���[���x���Q�̌���
                Dim table As DataTable = CommonBL.[Select]("CMSM���j���[���x��2", uinfo.SoshikiCd, uinfo.Id, e.Item.Value)

                For Each row As DataRow In table.Rows
                    Dim item As New MenuItem()
                    item.Text = row("��ʖ�").ToString()
                    item.NavigateUrl = row("URL").ToString()
                    item.Target = row("�I�v�V����").ToString()
                    item.Enabled = row("���ۃt���O").ToString() <> "False"
                    Menu2.Items.Add(item)
                Next
            End If
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub
    #End Region
End Class
