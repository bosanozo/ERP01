Imports System.Data

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.BL

''' <summary>
''' XX�}�X�^�����e
''' </summary>
Public Partial Class CM_CMSM010F02
    Inherits CMBaseEntryForm
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

        ' ���ʐݒ�
        Master.Body.Attributes.Remove("onload")

        ' ���샂�[�h��ݒ�
        Dim subName As String = SetOpeMode(PanelKeyItems, PanelSubItems, PanelUpdateInfo, PanelFunction, Nothing, BtnCommit, _
            BtnCancel)

        ' ��ʃw�b�_������
        Master.Title = "�g�D�}�X�^�����e�@" & subName

        ' �ȉ��͏����\���ȊO�͏������Ȃ�
        If IsPostBack Then
            Return
        End If

        Try
            ' �敪�l�̌���
            Dim kbnTable As DataTable = CommonBL.SelectKbn("M001")

            ' �敪�l�̐ݒ�
            Dim dv As New DataView(kbnTable, "����CD = 'M001'", Nothing, DataViewRowState.CurrentRows)

            ' �g�D�K�w�敪�̃A�C�e���ݒ�
            Dim table As DataTable = dv.ToTable()
            �g�D�K�w�敪.DataSource = table
        Catch ex As Exception
            ShowError(ex)
        End Try

        ' �W���̉�ʕ\���������s
        OnPageOnLoad(Master.Body, m_facade)
    End Sub

    ''' <summary>
    ''' �o�^�{�^������
    ''' </summary>
    Protected Sub BtnCommit_Click(sender As Object, e As EventArgs)
        ' �W���̓o�^�{�^���������������s
        OnCommitClick(Master.Body, m_facade)
    End Sub

    ''' <summary>
    ''' �L�����Z���{�^������
    ''' </summary>
    Protected Sub BtnCancel_Click(sender As Object, e As EventArgs)
        ' �W���̃L�����Z���{�^���������������s
        OnCancelClick(Master.Body)
    End Sub
    #End Region

    #Region "override���\�b�h"
    ''' <summary>
    ''' �L�[�f�[�^�����񂩂猟���p�����[�^���쐬����B
    ''' </summary>
    ''' <param name="argKey">�L�[�f�[�^������</param>
    ''' <returns>�����p�����[�^</returns>
    Protected Overrides Function CreateSelectParam(argKey As String) As List(Of CMSelectParam)
        Dim param As New List(Of CMSelectParam)()
        param.Add(New CMSelectParam("�g�DCD", "= @�g�DCD", argKey))
        Return param
    End Function

    ''' <summary>
    ''' �f�[�^���ύX����Ă��邩�`�F�b�N����B
    ''' </summary>
    ''' <returns>True:�ύX����, False:�ύX�Ȃ�</returns>
    Protected Overrides Function IsModified() As Boolean
        ' �V�K�̏ꍇ�A�L�[���ڃ`�F�b�N
        If OpeMode = "Insert" AndAlso IsPanelModified(PanelKeyItems) Then
            Return True
        End If

        ' �]�����ڃ`�F�b�N
        If IsPanelModified(PanelSubItems) Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' �V�K�s�Ƀf�t�H���g�l��ݒ肷��B
    ''' </summary>
    ''' <param name="argRow">�f�t�H���g�l��ݒ肷��DataRow</param>
    Protected Overrides Sub SetDefaultValue(argRow As DataRow)
        argRow("�g�D�K�w�敪") = CMInformationManager.UserInfo.SoshikiKaisoKbn
    End Sub

    ''' <summary>
    ''' DataRow�ɓ��̓f�[�^��ݒ肷��B
    ''' </summary>
    ''' <returns>True:�G���[����, False:�G���[�Ȃ�</returns>
    Protected Overrides Function SetInputRow() As Boolean
        Dim hasError As Boolean = False

        ' �V�K�̏ꍇ
        If OpeMode = "Insert" Then
            SetPanelInputRow(PanelKeyItems)
        End If

        ' �]�����ڒl��ݒ�
        SetPanelInputRow(PanelSubItems)

        ' �G���[�L����ԋp
        Return hasError
    End Function
    #End Region
End Class
