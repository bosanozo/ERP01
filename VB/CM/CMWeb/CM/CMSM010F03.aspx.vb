Imports System.Data

Imports DocumentFormat.OpenXml
Imports SpreadsheetLight

Imports NEXS.ERP.CM
Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.DA
Imports NEXS.ERP.CM.BL

''' <summary>
''' �g�D�}�X�^EXCEL����
''' </summary>
Public Partial Class CM_CMSM010F03
    Inherits CMBaseForm
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

        ' ��ʃw�b�_������
        Master.Title = "�g�D�}�X�^EXCEL����"

        ' �����\���ȊO�͏������Ȃ�
        If IsPostBack Then
            Return
        End If

        ' ���엚�����o��
        WriteOperationLog()
    End Sub

    ''' <summary>
    ''' EXCEL���̓{�^������
    ''' </summary>
    Protected Sub BtnExcelInput_Click(sender As Object, e As EventArgs)
        If Not FileUpload1.HasFile Then
            Return
        End If

        Try
            ' �A�b�v���[�h�t�@�C������f�[�^����荞��
            Dim ds As DataSet = ImportExcel(FileUpload1.PostedFile.InputStream)

            ' �f�[�^�Z�b�g���L��
            Session("ImportDataSet") = ds

            ' DataSource�ݒ�
            GridView1.DataSource = ds.Tables(0)
            ' �y�[�W�Z�b�g
            GridView1.PageIndex = 0
            ' �o�C���h
            GridView1.DataBind()
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub


    ''' <summary>
    ''' �o�^�{�^������
    ''' </summary>
    Protected Sub BtnUpdate_Click(sender As Object, e As EventArgs)
        If Session("ImportDataSet") Is Nothing Then
            Return
        End If

        ' �f�[�^�Z�b�g���擾
        Dim ds As DataSet = DirectCast(Session("ImportDataSet"), DataSet)

        Try
            ' �t�@�T�[�h�̌Ăяo��
            Dim operationTime As DateTime
            m_facade.Upload(ds, operationTime)
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub
    #End Region
End Class
