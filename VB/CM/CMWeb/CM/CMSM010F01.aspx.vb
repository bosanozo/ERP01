Imports System.Data

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.BL

''' <summary>
''' �g�D�}�X�^�����e
''' </summary>
Public Partial Class CM_CMSM010F01
    Inherits CMBaseListForm
    #Region "BL�C���W�F�N�V�����p�t�B�[���h"
    Protected m_facade As CMSM010BL
    #End Region

    #Region "�C�x���g�n���h��"
    ''' <summary>
    ''' �y�[�W���[�h
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' �g�D�b�c�A�g�D�K�w�敪��ݒ�
        KaishaCd.Value = CMInformationManager.UserInfo.SoshikiCd
        SoshikiLayer.Value = CMInformationManager.UserInfo.SoshikiKaisoKbn

        ' �@�\�{�^�� �X�N���v�g�o�^
        AddFuncOnclick(BtnSelect, BtnCsvOut, BtnInsert, BtnUpdate, BtnDelete)
        ' ��ʃw�b�_������
        Master.Title = "�g�D�}�X�^�����e"

        ' �����\���ȊO�͏������Ȃ�
        If IsPostBack Then
            Return
        End If

        ' �X�V�����擾
        Dim canUpdate As Boolean = m_commonBL.GetRangeCanUpdate(System.IO.Path.GetFileNameWithoutExtension(Me.AppRelativeVirtualPath), False)

        Try
            ' �敪�l�̌���
            Dim kbnTable As DataTable = CommonBL.SelectKbn("M001")

            ' �敪�l�̐ݒ�
            Dim dv As New DataView(kbnTable, "����CD = 'M001'", Nothing, DataViewRowState.CurrentRows)

            ' �g�D�K�w�敪�̃A�C�e���ݒ�
            Dim table As DataTable = dv.ToTable()
            table.Rows.InsertAt(table.NewRow(), 0)
            �g�D�K�w�敪.DataSource = table
            �g�D�K�w�敪.DataBind()
        Catch ex As Exception
            ShowError(ex)
        End Try

        ' ���엚�����o��
        WriteOperationLog()
    End Sub

    ''' <summary>
    ''' �����ACSV�o�̓{�^������
    ''' </summary>
    Protected Sub Select_Command(sender As Object, e As CommandEventArgs)
        ' �����p�����[�^�擾
        Dim param As List(Of CMSelectParam) = CreateSelectParam(PanelCondition)

        Dim hasError As Boolean = False

        ' ����
        If e.CommandName = "Select" Then
            hasError = DoSelect(m_facade, param, GridView1)

            ' ����I���̏ꍇ
            If Not hasError Then
                ' �������s���L��
                Selected.Value = "true"
                ' �����������L��
                Session("SelectCondition") = param
            End If
        ' CSV�o��
        ElseIf e.CommandName = "CsvOut" Then
            Dim url As String = Nothing
            hasError = DoCsvOut(m_facade, param, url)
            ' ���ʕ\��
            If Not hasError Then
                OpenExcel(url)
            End If
        End If
    End Sub

    ''' <summary>
    ''' �V�K�A�C���A�폜�{�^������
    ''' </summary>
    Protected Sub OpenEntryForm_Command(sender As Object, e As CommandEventArgs)
        ' ���������s����Ă���ꍇ�A�Č���
        If Selected.Value.Length > 0 Then
            ' ���������擾
            Dim param As List(Of CMSelectParam) = DirectCast(Session("SelectCondition"), List(Of CMSelectParam))
            ' �������s
            DoSelect(m_facade, param, GridView1, GridView1.PageIndex)
        End If

        ' ���b�Z�[�W��ݒ�
        If e.CommandName = "Delete" Then
            Master.ShowMessage("I", CMMessageManager.GetMessage("IV004"))
        Else
            Master.Body.Attributes.Remove("onload")
        End If
    End Sub

    ''' <summary>
    ''' �f�[�^�o�C���h
    ''' </summary>
    Protected Sub GridView1_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowIndex >= 0 Then
            Dim row As DataRowView = DirectCast(e.Row.DataItem, DataRowView)
        End If
    End Sub

    ''' <summary>
    ''' �y�[�W�؂�ւ�
    ''' </summary>
    Protected Sub GridView1_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)
        ' ���������擾
        Dim param As List(Of CMSelectParam) = DirectCast(Session("SelectCondition"), List(Of CMSelectParam))
        ' �������s
        DoSelect(m_facade, param, GridView1, e.NewPageIndex)
    End Sub
    #End Region
End Class
