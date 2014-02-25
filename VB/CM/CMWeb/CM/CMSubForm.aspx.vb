Imports System.Data
Imports System.Reflection
Imports System.Web.UI.WebControls

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.BL

''' <summary>
''' �I���q���
''' </summary>
Public Partial Class CMSubForm
    Inherits CMBaseListForm
    #Region "private�ϐ�"
    Private m_codeName As String
    #End Region

    #Region "�C�x���g�n���h��"
    ''' <summary>
    ''' �y�[�W���[�h
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' �@�\�{�^�� �X�N���v�g�o�^
        BtnSelect.Attributes.Add("onclick", "return CheckInputList()")

        ' �ȉ��͏����\���ȊO�͏������Ȃ�
        If IsPostBack Then
            Return
        End If

        ' �R�[�h�l������
        Code.MaxLength = Convert.ToInt32(Request.Params("CodeLen"))
        Code.Width = Code.MaxLength * 8 + 2

        ' �����R�[�h��
        m_codeName = Regex.Replace(Request.Params("Code"), "(From|To)", "")

        ' �O���b�h�̗񖼐ݒ�
        GridView1.Columns(1).HeaderText = GetCodeLabel()
        GridView1.Columns(2).HeaderText = GetNameLabel()

        ' �f�[�^�o�C���h���s
        DataBind()
    End Sub

    ''' <summary>
    ''' �����{�^������
    ''' </summary>
    Protected Sub Select_Command(sender As Object, e As CommandEventArgs)
        ' ��ʂ̏������擾
        Dim formParam As List(Of CMSelectParam) = CreateSelectParam(PanelCondition)

        ' ���ږ��̒u������
        For Each p As CMSelectParam In formParam
            If p.name = "Code" Then
                p.name = If(String.IsNullOrEmpty(Request.Params("DbCodeCol")), GridView1.Columns(1).HeaderText.Replace("�R�[�h", "CD"), Request.Params("DbCodeCol"))
            ElseIf p.name = "Name" Then
                p.name = If(String.IsNullOrEmpty(Request.Params("DbNameCol")), GridView1.Columns(2).HeaderText, Request.Params("DbNameCol"))
                p.condtion = "LIKE @" & Convert.ToString(p.name)
                p.paramFrom = "%" & Convert.ToString(p.paramFrom) & "%"
            End If
        Next

        ' �����p�����[�^�쐬
        Dim param As New List(Of CMSelectParam)()

        ' �ǉ��p�����[�^������ꍇ�A�ǉ�����
        If Not String.IsNullOrEmpty(Request.Params("Params")) Then
            For Each p As String In Request.Params("Params").Split()
                Dim value As Object

                ' "#"����n�܂�ꍇ��UserInfo����ݒ�
                If p(0) = "#"C Then
                    Dim pi As PropertyInfo = CMInformationManager.UserInfo.[GetType]().GetProperty(p.Substring(1))
                    value = pi.GetValue(CMInformationManager.UserInfo, Nothing)
                Else
                    ' �Z���̒l���擾
                    value = p
                End If

                ' �p�����[�^�ǉ�
                param.Add(New CMSelectParam(Nothing, Nothing, value))
            Next
        End If

        ' ��ʂ̏�����ǉ�
        param.AddRange(formParam)

        Dim hasError As Boolean = DoSelect(param, GridView1)

        ' ����I���̏ꍇ
        If Not hasError Then
            ' �����������L��
            Session("SelectCondition") = param
        End If
    End Sub

    ''' <summary>
    ''' �y�[�W�؂�ւ�
    ''' </summary>
    Protected Sub GridView1_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)
        ' ���������擾
        Dim param As List(Of CMSelectParam) = DirectCast(Session("SelectCondition"), List(Of CMSelectParam))
        ' �������s
        DoSelect(param, GridView1, e.NewPageIndex)
    End Sub
    #End Region

    #Region "protected���\�b�h"
    ''' <summary>
    ''' �R�[�h�̃��x���������Ԃ��B
    ''' </summary>
    ''' <returns>�R�[�h�̃��x��������</returns>
    Protected Function GetCodeLabel() As String
        Return m_codeName.Replace("CD", "�R�[�h")
    End Function

    ''' <summary>
    ''' ���̂̃��x���������Ԃ��B
    ''' </summary>
    ''' <returns>���̂̃��x��������</returns>
    Protected Function GetNameLabel() As String
        Return Regex.Replace(m_codeName, "(CD|ID)", "��")
    End Function
    #End Region

    #Region "private���\�b�h"
    ''' <summary>
    ''' ���������s����B
    ''' </summary>
    ''' <param name="argParam">���������p�����[�^</param>
    ''' <param name="argGrid">�ꗗ�\���p�O���b�h</param>
    ''' <param name="argPage">�y�[�W</param>
    ''' <returns>True:�G���[����, False:�G���[�Ȃ�</returns>
    Private Overloads Function DoSelect(argParam As List(Of CMSelectParam), argGrid As GridView, Optional argPage As Integer = 0) As Boolean
        Try
            ' �t�@�T�[�h�̌Ăяo��
            Dim message As CMMessage = Nothing
            Dim result As DataTable = CommonBL.SelectSub(Request.Params("SelectId"), argParam, message)

            ' �ԋp���b�Z�[�W�̕\��
            If message IsNot Nothing Then
                ShowMessage(message)
            End If

            Dim idx As Integer = 0
            For Each col As DataControlField In argGrid.Columns
                ' ��w�b�_�ݒ�
                If TypeOf col Is BoundField Then
                    Dim bf As BoundField = TryCast(col, BoundField)
                    If idx > 1 Then
                        bf.HeaderText = result.Columns(idx).ColumnName
                    End If
                    bf.DataField = result.Columns(idx).ColumnName
                    idx += 1
                End If
            Next

            ' DataSource�ݒ�
            argGrid.DataSource = result
            ' �y�[�W�Z�b�g
            argGrid.PageIndex = argPage
            ' �o�C���h
            argGrid.DataBind()
        Catch ex As Exception
            ' DataSource�N���A
            argGrid.DataSource = Nothing
            argGrid.DataBind()

            ShowError(ex)
            Return True
        End Try

        Return False
    End Function
    #End Region
End Class
