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
            ' EXCEL�������쐬
            Dim xslDoc As New SLDocument(FileUpload1.PostedFile.InputStream)

            ' �f�[�^�Z�b�g�Ƀf�[�^����荞��
            Dim ds As New DataSet()

            ' �V�[�g�Ń��[�v
            For Each sheet As String In xslDoc.GetSheetNames()
                ' �V�[�g��I��
                xslDoc.SelectWorksheet(sheet)

                ' �f�[�^�e�[�u���쐬
                Dim table As DataTable = CreateDataTableFromXml(sheet)

                Dim sheetStat = xslDoc.GetWorksheetStatistics()

                ' �P�s���ǂݍ��݁A�擪�s�̓^�C�g���Ƃ��ēǂݔ�΂�
                For rowIdx As Integer = sheetStat.StartRowIndex + 1 To sheetStat.EndRowIndex
                    Dim newRow As DataRow = table.NewRow()
                    For colIdx As Integer = 0 To table.Columns.Count - 1
                        Dim col As Integer = colIdx + sheetStat.StartColumnIndex

                        ' �^�ɉ����Ēl���擾����
                        Select Case table.Columns(colIdx).DataType.Name
                            Case "bool"
                                newRow(colIdx) = xslDoc.GetCellValueAsBoolean(rowIdx, col)
                                Exit Select

                            Case "decimal"
                                newRow(colIdx) = xslDoc.GetCellValueAsDecimal(rowIdx, col)
                                Exit Select

                            Case "long"
                                newRow(colIdx) = xslDoc.GetCellValueAsInt64(rowIdx, col)
                                Exit Select

                            Case "DateTime"
                                newRow(colIdx) = xslDoc.GetCellValueAsDateTime(rowIdx, col)
                                Exit Select
                            Case Else

                                newRow(colIdx) = xslDoc.GetCellValueAsString(rowIdx, col)
                                Exit Select
                        End Select
                    Next
                    table.Rows.Add(newRow)
                Next

                ' �f�[�^�e�[�u����ǉ�
                ds.Tables.Add(table)
            Next

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
    ''' �w���Xml�t�@�C������f�[�^�e�[�u�����쐬����B
    ''' </summary>
    ''' <param name="argName">Xml�t�@�C����</param>
    ''' <returns>�f�[�^�e�[�u��</returns>
    Private Function CreateDataTableFromXml(argName As String) As DataTable
        ' �f�[�^�Z�b�g�Ƀt�@�C����ǂݍ���
        Dim ds As New CMEntityDataSet()
        ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argName & ".xml"))

        ' �f�[�^�e�[�u���쐬
        Dim table As New DataTable(ds.�G���e�B�e�B(0).�e�[�u����)

        ' DataColumn�ǉ�
        For Each row As CMEntityDataSet.����Row In ds.����
            ' DataColumn�쐬
            Dim dcol As New DataColumn(row.���ږ�)
            ' �^
            Dim dbType As CMDbType = DirectCast([Enum].Parse(GetType(CMDbType), row.���ڌ^), CMDbType)
            Select Case dbType
                Case CMDbType.�t���O
                    dcol.DataType = GetType(Boolean)
                    Exit Select
                Case CMDbType.���z, CMDbType.����
                    dcol.DataType = GetType(Decimal)
                    Exit Select
                Case CMDbType.����
                    dcol.DataType = GetType(Long)
                    Exit Select
                Case CMDbType.���t, CMDbType.����
                    dcol.DataType = GetType(DateTime)
                    Exit Select
            End Select
            ' �K�{����
            If row.�K�{ Then
                dcol.AllowDBNull = False
            End If

            table.Columns.Add(dcol)
        Next

        Return table
    End Function

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
        'DoSelect(m_facade, param, GridView1, e.NewPageIndex)
    End Sub
    #End Region
End Class
